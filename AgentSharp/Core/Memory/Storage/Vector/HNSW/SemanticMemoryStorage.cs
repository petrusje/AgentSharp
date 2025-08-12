using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Utils;
using HNSW.Net;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// In-memory semantic storage using HNSW (Hierarchical Navigable Small World) for fast vector similarity search.
    /// Optimized for development, testing, and high-performance scenarios requiring semantic memory without persistence.
    /// </summary>
    public class SemanticMemoryStorage : IMemoryStorage, IDisposable, IAsyncDisposable
    {
        private readonly CompactHNSWConfiguration _config;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger _logger;
        
        // Core HNSW components
        private volatile SmallWorld<float[], float> _hnswGraph;
        private readonly object _graphInitLock = new object();
        private readonly Func<float[], float[], float> _distanceFunction;
        
        // Bidirectional mapping between memory ID and HNSW graph indices
        private readonly ConcurrentDictionary<string, int> _idToGraphIndex = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<int, string> _graphIndexToId = new ConcurrentDictionary<int, string>();
        
        // Memory storage for metadata
        private readonly ConcurrentDictionary<string, UserMemory> _memories = new ConcurrentDictionary<string, UserMemory>();
        
        // Threading and synchronization
        private readonly SemaphoreSlim _graphLock = new SemaphoreSlim(1, 1);
        
        // Cleanup management
        private readonly ConcurrentQueue<string> _pendingDeletes = new ConcurrentQueue<string>();
        private readonly Timer _cleanupTimer;
        
        // Metrics tracking
        private long _totalInserts = 0;
        private long _totalSearches = 0;
        private volatile bool _isInitialized = false;
        private volatile bool _disposed = false;

        public SemanticMemoryStorage(
            IEmbeddingService embeddingService = null,
            CompactHNSWConfiguration config = null,
            ILogger logger = null)
        {
            _config = config ?? CompactHNSWConfiguration.ForDevelopment();
            _logger = logger ?? new ConsoleLogger();
            
            // Create compact embedding service wrapper
            _embeddingService = CompactEmbeddingServiceFactory.Create(embeddingService, _config);
            
            // Validate configuration
            _config.Validate();
            
            // Set up distance function
            _distanceFunction = HNSWDistanceFunctions.GetDistanceFunction(_config.DistanceMetric);
            
            // Initialize cleanup timer (runs every 5 minutes)
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            
            _logger.Log(LogLevel.Info, 
                $"SemanticMemoryStorage initialized with {_config.GetDimensions()} dimensions " +
                $"({_config.GetEstimatedBytesPerVector()} bytes per vector)");
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _graphLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_isInitialized)
                    return;

                _logger.Log(LogLevel.Info, "Initializing compact HNSW graph...");
                _isInitialized = true;
                _logger.Log(LogLevel.Info, "Compact HNSW initialization complete");
            }
            finally
            {
                _graphLock.Release();
            }
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            
            if (string.IsNullOrEmpty(memory.Content))
                throw new ArgumentException("Memory content cannot be null or empty");

            if (!_isInitialized)
                await InitializeAsync();

            try
            {
                // Store memory metadata
                _memories[memory.Id] = memory;
                
                // Generate compact embedding
                var embedding = await _embeddingService.GenerateEmbeddingAsync(memory.Content).ConfigureAwait(false);
                var embeddingArray = embedding.ToArray();

                await _graphLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    // Initialize graph with thread-safe double-checked locking
                    if (_hnswGraph == null)
                    {
                        lock (_graphInitLock)
                        {
                            if (_hnswGraph == null)
                            {
                                _hnswGraph = new SmallWorld<float[], float>(_distanceFunction);
                            }
                        }
                    }

                    // Add to HNSW graph
                    var graphIndex = _hnswGraph.AddItem(embeddingArray);
                    
                    // Update mappings
                    _idToGraphIndex[memory.Id] = graphIndex;
                    _graphIndexToId[graphIndex] = memory.Id;
                    
                    Interlocked.Increment(ref _totalInserts);
                    
                    _logger.Log(LogLevel.Debug, 
                        $"Added memory {memory.Id} to compact HNSW at index {graphIndex} " +
                        $"(total vectors: {_hnswGraph.Count})");
                }
                finally
                {
                    _graphLock.Release();
                }

                return memory.Id;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Failed to add memory to compact HNSW: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            if (string.IsNullOrEmpty(query))
                return new List<UserMemory>();

            if (!_isInitialized)
                await InitializeAsync();

            Interlocked.Increment(ref _totalSearches);

            try
            {
                // Generate query embedding
                var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query).ConfigureAwait(false);
                var queryArray = queryEmbedding.ToArray();

                await _graphLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_hnswGraph == null || _hnswGraph.Count == 0)
                        return new List<UserMemory>();

                    // Perform HNSW search
                    var searchResults = _hnswGraph.KNNSearch(queryArray, Math.Min(limit * 2, _config.MaxCandidates));
                    
                    var results = new List<UserMemory>();
                    foreach (var result in searchResults.Take(limit))
                    {
                        if (_graphIndexToId.TryGetValue(result.Id, out var memoryId) &&
                            _memories.TryGetValue(memoryId, out var memory) &&
                            (string.IsNullOrEmpty(userId) || memory.UserId == userId) &&
                            memory.IsActive)
                        {
                            memory.RelevanceScore = 1.0f - result.Distance; // Convert distance to similarity
                            results.Add(memory);
                        }
                    }

                    return results.OrderByDescending(m => m.RelevanceScore).ToList();
                }
                finally
                {
                    _graphLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Failed to search memories in compact HNSW: {ex.Message}");
                
                // Fallback to simple string matching
                return FallbackStringSearch(query, userId, limit);
            }
        }

        public async Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            _memories.TryGetValue(memoryId, out var memory);
            return await Task.FromResult(memory);
        }

        public async Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            var memories = _memories.Values
                .Where(m => m.UserId == userId && m.IsActive)
                .Where(m => string.IsNullOrEmpty(sessionId) || m.SessionId == sessionId)
                .OrderByDescending(m => m.UpdatedAt)
                .ToList();

            return await Task.FromResult(memories);
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));

            if (_memories.ContainsKey(memory.Id))
            {
                memory.UpdatedAt = DateTime.UtcNow;
                _memories[memory.Id] = memory;
                
                _logger.Log(LogLevel.Debug, $"Updated memory {memory.Id}");
            }

            await Task.CompletedTask;
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            if (_memories.TryGetValue(memoryId, out var memory))
            {
                memory.IsActive = false;
                memory.UpdatedAt = DateTime.UtcNow;
                
                // Add to cleanup queue for actual removal
                _pendingDeletes.Enqueue(memoryId);
                
                _logger.Log(LogLevel.Debug, $"Deleted memory {memoryId} (soft delete, queued for cleanup)");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Fallback string search when HNSW fails
        /// </summary>
        private List<UserMemory> FallbackStringSearch(string query, string userId, int limit)
        {
            _logger.Log(LogLevel.Warning, "Using fallback string search");
            
            return _memories.Values
                .Where(m => (string.IsNullOrEmpty(userId) || m.UserId == userId) && m.IsActive)
                .Where(m => m.Content != null && m.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(m => m.UpdatedAt)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        public CompactHNSWMetrics GetMetrics()
        {
            return new CompactHNSWMetrics
            {
                TotalVectors = _hnswGraph?.Count ?? 0,
                TotalMemories = _memories.Count,
                TotalInserts = _totalInserts,
                TotalSearches = _totalSearches,
                ConfiguredDimensions = _config.GetDimensions(),
                EstimatedBytesPerVector = _config.GetEstimatedBytesPerVector(),
                EstimatedTotalMemoryUsage = (_hnswGraph?.Count ?? 0) * _config.GetEstimatedBytesPerVector(),
                GraphConnectivity = _config.M,
                IsInitialized = _isInitialized,
                EmbeddingSize = _config.EmbeddingSize,
                UseQuantization = _config.UseQuantization
            };
        }

        /// <summary>
        /// Clear all data (synchronous interface requirement)
        /// </summary>
        public void Clear()
        {
            ClearAsync().Wait();
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public async Task ClearAsync()
        {
            await _graphLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _hnswGraph = null;
                _idToGraphIndex.Clear();
                _graphIndexToId.Clear();
                _memories.Clear();
                _totalInserts = 0;
                _totalSearches = 0;
                
                _logger.Log(LogLevel.Info, "Cleared all compact HNSW data");
            }
            finally
            {
                _graphLock.Release();
            }
        }

        /// <summary>
        /// Periodic cleanup method to actually remove deleted memories
        /// </summary>
        private void PerformCleanup(object state)
        {
            if (_disposed) return;
            
            try
            {
                var deletedCount = 0;
                var processedCount = 0;
                var maxProcessPerCycle = 100; // Limit processing to avoid blocking
                
                while (_pendingDeletes.TryDequeue(out var memoryId) && processedCount < maxProcessPerCycle)
                {
                    processedCount++;
                    
                    if (_memories.TryGetValue(memoryId, out var memory) && 
                        !memory.IsActive && 
                        DateTime.UtcNow - memory.UpdatedAt > TimeSpan.FromMinutes(10)) // Grace period
                    {
                        _memories.TryRemove(memoryId, out _);
                        
                        // Remove from HNSW mappings (Note: HNSW.Net doesn't support removal, so we keep mapping)
                        // In production, consider rebuilding index periodically or use a different approach
                        
                        deletedCount++;
                    }
                }
                
                if (deletedCount > 0)
                {
                    _logger.Log(LogLevel.Debug, $"Cleanup removed {deletedCount} deleted memories");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error during memory cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        /// <summary>
        /// Async dispose resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            // Stop the cleanup timer first
            _cleanupTimer?.Dispose();
            
            // Wait for any ongoing operations with timeout
            try
            {
                await _graphLock.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                try
                {
                    // Perform final cleanup
                    while (_pendingDeletes.TryDequeue(out var memoryId))
                    {
                        _memories.TryRemove(memoryId, out _);
                    }
                    
                    _logger.Log(LogLevel.Info, "SemanticMemoryStorage disposed");
                }
                finally
                {
                    _graphLock.Release();
                }
            }
            catch (TimeoutException)
            {
                _logger.Log(LogLevel.Warning, "Timeout during disposal, forcing cleanup");
            }
            finally
            {
                _graphLock?.Dispose();
            }
        }
    }

    /// <summary>
    /// Performance metrics for compact HNSW storage
    /// </summary>
    public class CompactHNSWMetrics
    {
        public long TotalVectors { get; set; }
        public int TotalMemories { get; set; }
        public long TotalInserts { get; set; }
        public long TotalSearches { get; set; }
        public int ConfiguredDimensions { get; set; }
        public long EstimatedBytesPerVector { get; set; }
        public long EstimatedTotalMemoryUsage { get; set; }
        public int GraphConnectivity { get; set; }
        public bool IsInitialized { get; set; }
        public EmbeddingSize EmbeddingSize { get; set; }
        public bool UseQuantization { get; set; }

        public string GenerateReport()
        {
            var memoryMB = EstimatedTotalMemoryUsage / (1024.0 * 1024.0);
            return $@"
Compact HNSW Metrics:
  Vectors: {TotalVectors:N0}
  Memories: {TotalMemories:N0}
  Dimensions: {ConfiguredDimensions}
  Size: {EmbeddingSize}
  Memory Usage: {memoryMB:F1} MB
  Bytes/Vector: {EstimatedBytesPerVector:N0}
  Connectivity: M={GraphConnectivity}
  Operations: {TotalInserts:N0} inserts, {TotalSearches:N0} searches
  Quantization: {(UseQuantization ? "Enabled" : "Disabled")}";
        }
    }
}