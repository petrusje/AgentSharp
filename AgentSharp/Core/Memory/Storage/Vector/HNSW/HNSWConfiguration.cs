using System;
using AgentSharp.Utils;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Configuration class for HNSW Vector Storage
    /// </summary>
    public class HNSWConfiguration
    {
        // HNSW Parameters
        /// <summary>
        /// Number of bi-directional links created for every new element during construction. Reasonable range: 2-100
        /// </summary>
        public int M { get; set; } = 16;

        /// <summary>
        /// Level generation probability parameter. Default: 1.0 / ln(2.0 * M)
        /// </summary>
        public double LevelLambda { get; set; } = 1.0 / Math.Log(16);

        /// <summary>
        /// Size of the dynamic candidate list. Reasonable range: 100-800
        /// </summary>
        public int EfConstruction { get; set; } = 200;

        /// <summary>
        /// Size of the dynamic candidate list during search. Reasonable range: 50-400
        /// </summary>
        public int EfSearch { get; set; } = 50;

        // Performance Settings
        /// <summary>
        /// Batch size for bulk insertions
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Interval for automatic index rebuilding
        /// </summary>
        public TimeSpan RebuildInterval { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Enable automatic index rebuilding
        /// </summary>
        public bool AutoRebuild { get; set; } = true;

        // Storage Settings
        /// <summary>
        /// File path to save the index
        /// </summary>
        public string IndexFilePath { get; set; }

        /// <summary>
        /// Persist index to disk automatically
        /// </summary>
        public bool PersistToDisk { get; set; } = true;

        /// <summary>
        /// Compress index when saving to disk
        /// </summary>
        public bool CompressIndex { get; set; } = false;

        // Search Settings
        /// <summary>
        /// Distance metric for vector comparison
        /// </summary>
        public DistanceMetric DistanceMetric { get; set; } = DistanceMetric.Cosine;

        /// <summary>
        /// Minimum similarity threshold for search results
        /// </summary>
        public float SimilarityThreshold { get; set; } = 0.5f;

        /// <summary>
        /// Maximum number of candidates to consider for re-ranking
        /// </summary>
        public int MaxCandidates { get; set; } = 1000;

        // Fallback Settings
        /// <summary>
        /// Enable fallback to SQLite storage for small datasets
        /// </summary>
        public bool EnableFallback { get; set; } = true;

        /// <summary>
        /// Minimum number of embeddings to use HNSW instead of SQLite fallback
        /// </summary>
        public int FallbackThreshold { get; set; } = 100;

        // Memory Management Settings
        /// <summary>
        /// Threshold for orphaned node cleanup (default: 1000)
        /// </summary>
        public int OrphanCleanupThreshold { get; set; } = 1000;

        /// <summary>
        /// Maximum number of orphaned nodes to process in one cleanup cycle
        /// </summary>
        public int MaxOrphansPerCleanup { get; set; } = 100;

        /// <summary>
        /// Validate configuration parameters with performance warnings
        /// </summary>
        /// <param name="logger">Optional logger for warnings</param>
        public void Validate(ILogger logger = null)
        {
            // Critical validation - throw exceptions
            if (M <= 0 || M > 100)
                throw new HNSWConfigurationException(nameof(M), M, "M must be between 1 and 100");

            if (EfConstruction <= 0)
                throw new HNSWConfigurationException(nameof(EfConstruction), EfConstruction, "EfConstruction must be positive");

            if (EfSearch <= 0)
                throw new HNSWConfigurationException(nameof(EfSearch), EfSearch, "EfSearch must be positive");

            if (BatchSize <= 0)
                throw new HNSWConfigurationException(nameof(BatchSize), BatchSize, "BatchSize must be positive");

            if (MaxCandidates <= 0)
                throw new HNSWConfigurationException(nameof(MaxCandidates), MaxCandidates, "MaxCandidates must be positive");

            if (FallbackThreshold < 0)
                throw new HNSWConfigurationException(nameof(FallbackThreshold), FallbackThreshold, "FallbackThreshold cannot be negative");

            if (SimilarityThreshold < 0 || SimilarityThreshold > 1)
                throw new HNSWConfigurationException(nameof(SimilarityThreshold), SimilarityThreshold, "SimilarityThreshold must be between 0 and 1");

            if (OrphanCleanupThreshold <= 0)
                throw new HNSWConfigurationException(nameof(OrphanCleanupThreshold), OrphanCleanupThreshold, "OrphanCleanupThreshold must be positive");

            if (MaxOrphansPerCleanup <= 0)
                throw new HNSWConfigurationException(nameof(MaxOrphansPerCleanup), MaxOrphansPerCleanup, "MaxOrphansPerCleanup must be positive");

            // Performance warnings - log but don't throw
            if (logger != null)
            {
                if (EfSearch > EfConstruction)
                    logger.Log(LogLevel.Warning, $"EfSearch ({EfSearch}) > EfConstruction ({EfConstruction}) may cause suboptimal performance");
                
                if (M > 16 && EfConstruction < M * 10)
                    logger.Log(LogLevel.Warning, $"High M ({M}) with low EfConstruction ({EfConstruction}) may cause poor connectivity");
                
                // Memory estimation for common vector size (1536 dimensions like OpenAI)
                var estimatedMemoryMB = EstimateMemoryUsage(10000, 1536);
                if (estimatedMemoryMB > 1000) // > 1GB
                    logger.Log(LogLevel.Warning, $"Configuration may use significant memory: ~{estimatedMemoryMB}MB for 10k vectors (1536 dims)");

                if (BatchSize > 10000)
                    logger.Log(LogLevel.Warning, $"Large BatchSize ({BatchSize}) may cause memory spikes during bulk operations");
            }
        }

        /// <summary>
        /// Estimate memory usage for given vector count and dimensions
        /// </summary>
        /// <param name="vectorCount">Number of vectors</param>
        /// <param name="dimensions">Vector dimensions</param>
        /// <returns>Estimated memory usage in MB</returns>
        public long EstimateMemoryUsage(long vectorCount, int dimensions = 1536)
        {
            // Rough estimation: (dimensions * 4 bytes + overhead) * M connections
            var bytesPerVector = dimensions * 4 + (M * 8) + 100; // overhead for metadata
            return (vectorCount * bytesPerVector) / (1024 * 1024); // Convert to MB
        }

        /// <summary>
        /// Create high performance configuration preset
        /// </summary>
        public static HNSWConfiguration HighPerformance()
        {
            return new HNSWConfiguration
            {
                M = 32,
                EfConstruction = 400,
                EfSearch = 100,
                BatchSize = 2000,
                MaxCandidates = 2000
            };
        }

        /// <summary>
        /// Create low memory configuration preset
        /// </summary>
        public static HNSWConfiguration LowMemory()
        {
            return new HNSWConfiguration
            {
                M = 8,
                EfConstruction = 100,
                EfSearch = 32,
                CompressIndex = true,
                BatchSize = 500,
                MaxCandidates = 500
            };
        }

        /// <summary>
        /// Create balanced configuration preset
        /// </summary>
        public static HNSWConfiguration Balanced()
        {
            return new HNSWConfiguration(); // Uses defaults
        }
    }

    /// <summary>
    /// Distance metrics supported by HNSW
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>
        /// Cosine similarity (1 - cosine distance)
        /// </summary>
        Cosine,

        /// <summary>
        /// Euclidean distance (L2 norm)
        /// </summary>
        Euclidean,

        /// <summary>
        /// Dot product distance
        /// </summary>
        DotProduct
    }
}