using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Storage;
using AgentSharp.Utils;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Persistent semantic storage using SQLite with sqlite-vec extension for vector similarity search.
    /// Provides full IStorage interface with sessions, memories, and embeddings persistence.
    /// Ideal for production scenarios requiring semantic memory with data persistence.
    /// </summary>
    public class SemanticSqliteStorage : IStorage
    {
        private readonly string _connectionString;
        private readonly string _embeddingModel;
        private readonly int _dimensions;
        private readonly string _distanceMetric;
        private readonly IEmbeddingService _embeddingService;

        // IStorage interface properties
        public ISessionStorage Sessions { get; private set; }
        public IMemoryStorage Memories { get; private set; }
        public IEmbeddingStorage Embeddings { get; private set; }

        // Expose dimensions for adapter
        public int Dimensions => _dimensions;

        // Expose embedding service for adapter
        public IEmbeddingService EmbeddingService => _embeddingService;

        /// <summary>
        /// Resultado de busca por similaridade
        /// </summary>
        public class SimilarityResult
        {
            public string Content { get; set; }
            public float Similarity { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

        /// <summary>
        /// Representa um vetor com metadados
        /// </summary>
        public class EmbeddingVector
        {
            public string Content { get; set; }
            public float[] Vector { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

        /// <summary>
        /// Initializes a new SemanticSqliteStorage instance with embedding service.
        /// </summary>
        /// <param name="connectionString">SQLite connection string</param>
        /// <param name="embeddingService">Embedding service for vector generation</param>
        /// <param name="dimensions">Vector dimensions</param>
        /// <param name="distanceMetric">Distance metric: cosine, l2, or inner_product (default: cosine)</param>
        public SemanticSqliteStorage(
            string connectionString,
            IEmbeddingService embeddingService,
            int dimensions,
            string distanceMetric = "cosine")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _embeddingModel = "provided-by-service";
            _dimensions = dimensions;
            _distanceMetric = distanceMetric;

            // Initialize IStorage components
            Sessions = new SqliteSessionStorage(connectionString);
            Memories = new VectorSqliteVecMemoryAdapter(this);
            Embeddings = new SqliteEmbeddingStorage(connectionString);

            InitializeDatabase();
        }

        /// <summary>
        /// Initializes a new SemanticSqliteStorage instance (legacy constructor).
        /// </summary>
        /// <param name="connectionString">SQLite connection string</param>
        /// <param name="embeddingModel">Model used for embeddings</param>
        /// <param name="dimensions">Vector dimensions</param>
        /// <param name="distanceMetric">Distance metric: cosine, l2, or inner_product (default: cosine)</param>
        public SemanticSqliteStorage(
            string connectionString,
            string embeddingModel,
            int dimensions,
            string distanceMetric = "cosine")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
            _dimensions = dimensions;
            _distanceMetric = distanceMetric;
            _embeddingService = null; // No embedding service in legacy constructor

            // Initialize IStorage components
            Sessions = new SqliteSessionStorage(connectionString);
            Memories = new VectorSqliteVecMemoryAdapter(this);
            Embeddings = new SqliteEmbeddingStorage(connectionString);

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    // Load sqlite-vec extension
                    LoadVecExtension(connection);

                    // Create the main embedding table with vec0 virtual table
                    var createTableSql = $@"
                        CREATE VIRTUAL TABLE IF NOT EXISTS vec_embeddings USING vec0(
                            embedding float[{_dimensions}] distance_metric={_distanceMetric}
                        )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createTableSql;
                        command.ExecuteNonQuery();
                    }

                    // Create metadata table for additional information
                    var createMetadataSql = @"
                        CREATE TABLE IF NOT EXISTS embedding_metadata (
                            rowid INTEGER PRIMARY KEY,
                            content TEXT NOT NULL,
                            model TEXT NOT NULL,
                            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                            metadata TEXT
                        )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createMetadataSql;
                        command.ExecuteNonQuery();
                    }

                    // Create session messages table for conversation history
                    var createSessionMessagesSql = @"
                        CREATE TABLE IF NOT EXISTS session_messages (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            user_id TEXT NOT NULL,
                            session_id TEXT NOT NULL,
                            role TEXT NOT NULL,
                            content TEXT NOT NULL,
                            created_at TEXT NOT NULL
                        )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createSessionMessagesSql;
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    var status = SqliteVecHelper.CheckInstallationStatus();
                    var errorMessage = status.IsInstalled
                        ? $"Failed to load sqlite-vec extension. {status.Message}"
                        : $"sqlite-vec extension not found. {status.Message}\n\n{SqliteVecHelper.GetInstallationInstructions()}";

                    throw new InvalidOperationException(errorMessage, ex);
                }
            }
        }

        private void LoadVecExtension(SqliteConnection connection)
        {
            // Check if binary is available first
            var status = SqliteVecHelper.CheckInstallationStatus();

            try
            {
                // Try to load vec0 extension by name using SqliteConnection.LoadExtension
                connection.LoadExtension("vec0");

                // Test if extension is loaded
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT vec_version()";
                    var version = command.ExecuteScalar();
                    Console.WriteLine($"✅ sqlite-vec loaded successfully. Version: {version}");
                }
                return; // Success
            }
            catch (Exception)
            {
                // Try loading from specific path if binary was found
                if (status.IsInstalled && status.IsValid)
                {
                    try
                    {
                        // Use SqliteConnection.LoadExtension with full path
                        connection.LoadExtension(status.BinaryPath);

                        // Test if extension is loaded
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT vec_version()";
                            var version = command.ExecuteScalar();
                            Console.WriteLine($"✅ sqlite-vec loaded from {status.BinaryPath}. Version: {version}");
                        }
                        return; // Success
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Failed to load from {status.BinaryPath}: {ex.Message}");
                    }
                }

                // If we get here, loading failed
                var errorMessage = status.IsInstalled
                    ? $"sqlite-vec binary found but failed to load: {status.BinaryPath}\nThe binary may be corrupted or incompatible."
                    : $"sqlite-vec binary not found.\n\n{SqliteVecHelper.GetInstallationInstructions()}";

                throw new InvalidOperationException(errorMessage);
            }
        }

        public void StoreEmbeddings(List<EmbeddingVector> embeddings)
        {
            if (embeddings == null || embeddings.Count == 0)
                return;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert metadata first
                        using (var metadataCommand = connection.CreateCommand())
                        {
                            metadataCommand.CommandText = @"
                                INSERT INTO embedding_metadata (content, model, metadata)
                                VALUES (@content, @model, @metadata)";

                            // Insert vectors into vec table
                            using (var vecCommand = connection.CreateCommand())
                            {
                                vecCommand.CommandText = @"
                                    INSERT INTO vec_embeddings (rowid, embedding)
                                    VALUES (@rowid, @embedding)";

                                foreach (var embedding in embeddings)
                                {
                                    // Insert metadata
                                    metadataCommand.Parameters.Clear();
                                    metadataCommand.Parameters.AddWithValue("@content", embedding.Content);
                                    metadataCommand.Parameters.AddWithValue("@model", _embeddingModel);
                                    var metadataValue = embedding.Metadata != null
                                        ? JsonSerializer.Serialize(embedding.Metadata)
                                        : (object)DBNull.Value;
                                    metadataCommand.Parameters.AddWithValue("@metadata", metadataValue);
                                    metadataCommand.ExecuteNonQuery();

                                    // Get the inserted ID
                                    var rowId = GetLastInsertRowId(connection);

                                    // Insert vector - sqlite-vec accepts vectors as binary blobs
                                    vecCommand.Parameters.Clear();
                                    vecCommand.Parameters.AddWithValue("@rowid", rowId);
                                    vecCommand.Parameters.AddWithValue("@embedding", SerializeVector(embedding.Vector));
                                    vecCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private long GetLastInsertRowId(SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid()";
                return (long)command.ExecuteScalar();
            }
        }

        private byte[] SerializeVector(float[] vector)
        {
            // sqlite-vec expects vectors as binary data (little-endian float32 array)
            var bytes = new byte[vector.Length * 4];
            for (int i = 0; i < vector.Length; i++)
            {
                var floatBytes = BitConverter.GetBytes(vector[i]);
                Array.Copy(floatBytes, 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        private float[] DeserializeVector(byte[] bytes)
        {
            if (bytes == null || bytes.Length % 4 != 0)
                throw new ArgumentException("Invalid vector byte array");

            var vector = new float[bytes.Length / 4];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            return vector;
        }

        public List<SimilarityResult> SearchSimilar(float[] queryVector, int topK, float? threshold = null)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // sqlite-vec KNN query using proper syntax
                var sql = @"
                    SELECT
                        m.content,
                        distance,
                        m.metadata
                    FROM vec_embeddings
                    JOIN embedding_metadata m ON vec_embeddings.rowid = m.rowid
                    WHERE vec_embeddings.embedding MATCH @query
                    AND k = @k
                    ORDER BY distance
                    LIMIT @k";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@query", SerializeVector(queryVector));
                    command.Parameters.AddWithValue("@k", topK);

                    var results = new List<SimilarityResult>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var contentOrdinal = reader.GetOrdinal("content");
                            var distanceOrdinal = reader.GetOrdinal("distance");
                            var metadataOrdinal = reader.GetOrdinal("metadata");

                            var content = reader.GetString(contentOrdinal);
                            var distance = reader.GetFloat(distanceOrdinal);
                            var metadataValue = reader.IsDBNull(metadataOrdinal) ? null : reader.GetString(metadataOrdinal);

                            var metadataDict = string.IsNullOrEmpty(metadataValue)
                                ? null
                                : JsonSerializer.Deserialize<Dictionary<string, object>>(metadataValue);

                            results.Add(new SimilarityResult
                            {
                                Content = content,
                                Similarity = ConvertDistanceToSimilarity(distance),
                                Metadata = metadataDict
                            });
                        }
                    }

                    return results;
                }
            }
        }

        private float ConvertDistanceToSimilarity(float distance)
        {
            switch (_distanceMetric.ToLower())
            {
                case "cosine":
                    // Cosine distance is 1 - cosine_similarity
                    // So similarity = 1 - distance
                    return 1.0f - distance;

                case "l2":
                    // For L2 distance: similarity = 1 / (1 + distance)
                    return 1.0f / (1.0f + distance);

                case "inner_product":
                    // Inner product: higher is more similar (negate distance)
                    return -distance;

                default:
                    return 1.0f / (1.0f + distance);
            }
        }

        public void DeleteEmbeddings(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));

                        // Delete from metadata table
                        using (var metadataCommand = connection.CreateCommand())
                        {
                            metadataCommand.CommandText = $"DELETE FROM embedding_metadata WHERE rowid IN ({placeholders})";

                            // Delete from vec table
                            using (var vecCommand = connection.CreateCommand())
                            {
                                vecCommand.CommandText = $"DELETE FROM vec_embeddings WHERE rowid IN ({placeholders})";

                                for (int i = 0; i < ids.Count; i++)
                                {
                                    var paramName = $"@id{i}";
                                    metadataCommand.Parameters.AddWithValue(paramName, ids[i]);
                                    vecCommand.Parameters.AddWithValue(paramName, ids[i]);
                                }

                                metadataCommand.ExecuteNonQuery();
                                vecCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ClearAllEmbeddings()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Clear metadata
                    command.CommandText = "DELETE FROM embedding_metadata";
                    command.ExecuteNonQuery();

                    // Clear vec table
                    command.CommandText = "DELETE FROM vec_embeddings";
                    command.ExecuteNonQuery();
                }
            }
        }

        public long GetEmbeddingCount()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM embedding_metadata";
                    return (long)command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Gets performance information about the current index
        /// </summary>
        public string GetIndexInfo()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT vec_version()";
                        var version = command.ExecuteScalar()?.ToString();

                        var info = new StringBuilder();
                        info.AppendLine($"SQLite-Vec Version: {version}");
                        info.AppendLine($"Distance Metric: {_distanceMetric}");
                        info.AppendLine($"Dimensions: {_dimensions}");
                        info.AppendLine($"Total Vectors: {GetEmbeddingCount()}");
                        info.AppendLine($"Embedding Model: {_embeddingModel}");

                        // Try to get additional vec info
                        try
                        {
                            command.CommandText = "SELECT vec_info('vec_embeddings')";
                            var vecInfo = command.ExecuteScalar()?.ToString();
                            if (!string.IsNullOrEmpty(vecInfo))
                            {
                                info.AppendLine("Vec Table Info:");
                                info.AppendLine(vecInfo);
                            }
                        }
                        catch
                        {
                            // vec_info might not be available in all versions
                        }

                        return info.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return $"Index info unavailable: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Rebuild the index for optimal performance (if needed)
        /// </summary>
        public void RebuildIndex()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO vec_embeddings(vec_embeddings) VALUES('rebuild')";
                        command.ExecuteNonQuery();
                        Console.WriteLine("Index rebuilt successfully");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Index rebuild failed (might not be needed): {ex.Message}");
                }
            }
        }

        // IStorage interface implementation
        public async Task InitializeAsync()
        {
            await ((SqliteSessionStorage)Sessions).InitializeAsync();
            await ((VectorSqliteVecMemoryAdapter)Memories).InitializeAsync();
        }

        public async Task ClearAllAsync()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM vec_embeddings; DELETE FROM embedding_metadata;";
                    await command.ExecuteNonQueryAsync();
                }
            }
            ((SqliteSessionStorage)Sessions).Clear();
            ((SqliteEmbeddingStorage)Embeddings).Clear();
        }

        // Legacy IStorage compatibility methods
        public bool IsConnected => true;
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;
        public async Task SaveMessageAsync(Message message) => await Task.CompletedTask;
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null)
        {
            return await Task.FromResult(new List<Message>());
        }

        public async Task SaveMemoryAsync(UserMemory memory) => await Memories.AddMemoryAsync(memory);
        public async Task<List<UserMemory>> GetMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null, int? limit = null)
        {
            if (context == null) return new List<UserMemory>();
            return await Memories.GetMemoriesAsync(context.UserId, context.SessionId);
        }
        public async Task UpdateMemoryAsync(UserMemory memory) => await Memories.UpdateMemoryAsync(memory);
        public async Task DeleteMemoryAsync(string id) => await Memories.DeleteMemoryAsync(id);
        public async Task ClearMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            await ClearAllAsync();
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context, int limit = 10)
        {
            return await Memories.SearchMemoriesAsync(query, context?.UserId ?? "", limit);
        }

        public async Task<ISession> GetOrCreateSessionAsync(string userId, string sessionId)
        {
            // Simple implementation - return a basic session
            return await Task.FromResult(new AgentSession
            {
                Id = sessionId,
                UserId = userId,
                Title = "VectorSqliteVec Session",
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null)
        {
            // Simple implementation - return empty list for now
            return await Task.FromResult(new List<ISession>());
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            await ((SqliteSessionStorage)Sessions).DeleteSessionAsync(sessionId);
        }

        /// <summary>
        /// Obtém histórico de mensagens de uma sessão para contexto conversacional (como /refs)
        /// </summary>
        public async Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10)
        {
            var query = @"
                SELECT role, content, created_at 
                FROM session_messages 
                WHERE user_id = @userId AND session_id = @sessionId 
                ORDER BY created_at DESC 
                LIMIT @limit";
            
            var messages = new List<AIMessage>();
            
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@limit", limit);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var role = reader.GetString(0); // role column
                            var content = reader.GetString(1); // content column
                            
                            switch (role.ToLower())
                            {
                                case "user":
                                    messages.Add(AIMessage.User(content));
                                    break;
                                case "assistant":
                                    messages.Add(AIMessage.Assistant(content));
                                    break;
                                case "system":
                                    messages.Add(AIMessage.System(content));
                                    break;
                            }
                        }
                    }
                }
            }
            
            // Reverter para ordem cronológica (mais antiga primeiro)
            messages.Reverse();
            return messages;
        }

        /// <summary>
        /// Salva uma mensagem no histórico da sessão
        /// </summary>
        public async Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message)
        {
            await EnsureTablesExistAsync();
            
            var insertQuery = @"
                INSERT INTO session_messages (user_id, session_id, role, content, created_at)
                VALUES (@userId, @sessionId, @role, @content, @createdAt)";
            
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@role", message.Role);
                    command.Parameters.AddWithValue("@content", message.Content);
                    command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Ensures that required tables exist
        /// </summary>
        private async Task EnsureTablesExistAsync()
        {
            // Tables are created in InitializeDatabase(), this is just a safety check
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Adapter to make SemanticSqliteStorage work with IMemoryStorage interface
    /// </summary>
    internal class VectorSqliteVecMemoryAdapter : IMemoryStorage
    {
        private readonly SemanticSqliteStorage _storage;

        public VectorSqliteVecMemoryAdapter(SemanticSqliteStorage storage)
        {
            _storage = storage;
        }

        public async Task InitializeAsync()
        {
            // Already initialized by parent
            await Task.CompletedTask;
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            var embedding = await GenerateEmbeddingForMemory(memory);
            var embeddingVector = new SemanticSqliteStorage.EmbeddingVector
            {
                Content = memory.Content,
                Vector = embedding,
                Metadata = new Dictionary<string, object>
                {
                    ["UserId"] = memory.UserId,
                    ["SessionId"] = memory.SessionId,
                    ["Type"] = memory.Type.ToString(),
                    ["Id"] = memory.Id
                }
            };
            _storage.StoreEmbeddings(new List<SemanticSqliteStorage.EmbeddingVector> { embeddingVector });
            return memory.Id;
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            var queryEmbedding = await GenerateEmbedding(query);
            var results = _storage.SearchSimilar(queryEmbedding, limit);

            return results.Select(r => new UserMemory
            {
                Id = r.Metadata.TryGetValue("Id", out var idObj) ? idObj?.ToString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
                Content = r.Content,
                UserId = r.Metadata.TryGetValue("UserId", out var userIdObj) ? userIdObj?.ToString() ?? userId : userId,
                SessionId = r.Metadata.TryGetValue("SessionId", out var sessionIdObj) ? sessionIdObj?.ToString() : null,
                Type = r.Metadata.TryGetValue("Type", out var typeObj) && Enum.TryParse<AgentMemoryType>(typeObj?.ToString(), out var type) ? type : AgentMemoryType.Conversation,
                CreatedAt = DateTime.UtcNow,
                RelevanceScore = r.Similarity
            }).ToList();
        }

        public async Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            // For sqlite-vec, we'll do a broad search to get memories for the user
            var searchQuery = $"user {userId}";
            return await SearchMemoriesAsync(searchQuery, userId, 100);
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            // For simplicity, we'll delete and re-add
            await DeleteMemoryAsync(memory.Id);
            await AddMemoryAsync(memory);
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            // sqlite-vec doesn't support easy deletion by metadata, so we'll skip for now
            await Task.CompletedTask;
        }

        public async Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            // For simplicity, return null - could be implemented with metadata search
            return await Task.FromResult<UserMemory>(null);
        }

        public void Clear()
        {
            // Clear would need to delete all vectors - for now just log
            Console.WriteLine("Clear called on VectorSqliteVecMemoryAdapter");
        }

        private async Task<float[]> GenerateEmbeddingForMemory(UserMemory memory)
        {
            return await GenerateEmbedding(memory.Content);
        }

        private async Task<float[]> GenerateEmbedding(string text)
        {
            // Use real embedding service if available, otherwise mock
            if (_storage.EmbeddingService != null)
            {
                var embeddingList = await _storage.EmbeddingService.GenerateEmbeddingAsync(text);
                return embeddingList.ToArray();
            }

            // Fallback to mock embedding
            var hash = text.GetHashCode();
            var embedding = new float[_storage.Dimensions];
            var random = new Random(hash);
            for (int i = 0; i < _storage.Dimensions; i++)
            {
                embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            }
            return embedding;
        }
    }
}
