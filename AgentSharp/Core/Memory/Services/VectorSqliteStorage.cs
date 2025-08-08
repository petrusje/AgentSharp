using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Utils;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Storage SQLite com suporte a busca vetorial usando embeddings
    /// </summary>
    public class VectorSqliteStorage : IStorage
    {
        private readonly string _connectionString;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger _logger;
        
        public ISessionStorage Sessions { get; }
        public IMemoryStorage Memories { get; }
        public IEmbeddingStorage Embeddings { get; }

        public VectorSqliteStorage(string connectionString, IEmbeddingService embeddingService = null, ILogger logger = null)
        {
            _connectionString = connectionString;
            _embeddingService = embeddingService;
            _logger = logger ?? new ConsoleLogger();
            
            Sessions = new SqliteSessionStorage(connectionString);
            Memories = new VectorSqliteMemoryStorage(connectionString, embeddingService, logger);
            Embeddings = new SqliteEmbeddingStorage(connectionString);
        }

        public async Task InitializeAsync()
        {
            await ((SqliteSessionStorage)Sessions).InitializeAsync();
            await ((VectorSqliteMemoryStorage)Memories).InitializeAsync();
        }

        public async Task ClearAllAsync()
        {
            ((SqliteSessionStorage)Sessions).Clear();
            ((VectorSqliteMemoryStorage)Memories).Clear();
            ((SqliteEmbeddingStorage)Embeddings).Clear();
            await Task.CompletedTask;
        }

        // Métodos de compatibilidade
        public bool IsConnected => true;
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;
        public Task SaveMessageAsync(Message message) => Task.CompletedTask;
        public Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null) => Task.FromResult(new List<Message>());
        public Task SaveMemoryAsync(UserMemory memory) => Memories.AddMemoryAsync(memory);
        public Task<List<UserMemory>> GetMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null, int? limit = null)
        {
            if (context == null) 
            {
                return ((VectorSqliteMemoryStorage)Memories).GetMemoriesAsync("", null);
            }
            return ((VectorSqliteMemoryStorage)Memories).GetMemoriesAsync(context.UserId, context.SessionId);
        }
        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context, int limit = 10)
        {
            return await ((VectorSqliteMemoryStorage)Memories).SearchMemoriesAsync(query, context?.UserId ?? "", limit);
        }
        public Task UpdateMemoryAsync(UserMemory memory) => Memories.UpdateMemoryAsync(memory);
        public Task DeleteMemoryAsync(string id) => Memories.DeleteMemoryAsync(id);
        public Task ClearMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null) => Task.CompletedTask;
        public async Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId)
        {
            var session = await Sessions.GetSessionAsync(sessionId);
            if (session != null) return session;

            var newSession = new AgentSession
            {
                Id = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await Sessions.CreateSessionAsync(newSession);
            return newSession;
        }
        public async Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null)
        {
            var sessions = await Sessions.GetUserSessionsAsync(userId);
            var result = sessions.Cast<ISession>().ToList();
            
            if (limit.HasValue && result.Count > limit.Value)
            {
                result = result.Take(limit.Value).ToList();
            }
            
            return result;
        }
        public Task DeleteSessionAsync(string sessionId) => Sessions.DeleteSessionAsync(sessionId);
    }

    /// <summary>
    /// Storage de memórias com suporte a embeddings vetoriais
    /// </summary>
    public class VectorSqliteMemoryStorage : IMemoryStorage
    {
        private readonly string _connectionString;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger _logger;

        public VectorSqliteMemoryStorage(string connectionString, IEmbeddingService embeddingService = null, ILogger logger = null)
        {
            _connectionString = connectionString;
            _embeddingService = embeddingService;
            _logger = logger ?? new ConsoleLogger();
        }

        public async Task InitializeAsync()
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UserMemory (
                    Id TEXT PRIMARY KEY,
                    UserId TEXT,
                    SessionId TEXT,
                    Content TEXT,
                    Type INTEGER,
                    CreatedAt TEXT,
                    UpdatedAt TEXT,
                    AccessCount INTEGER,
                    LastAccessedAt TEXT,
                    RelevanceScore REAL,
                    Tags TEXT,
                    Metadata TEXT,
                    IsActive INTEGER,
                    Priority INTEGER,
                    Context TEXT,
                    Embedding BLOB
                );";
                await cmd.ExecuteNonQueryAsync();

                // Criar índice para melhorar performance de busca
                cmd.CommandText = @"CREATE INDEX IF NOT EXISTS idx_user_memory_search 
                                    ON UserMemory(UserId, IsActive, UpdatedAt);";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                // Gerar embedding se o serviço estiver disponível
                byte[] embeddingBytes = null;
                if (_embeddingService != null && !string.IsNullOrEmpty(memory.Content))
                {
                    try
                    {
                        var embedding = await _embeddingService.GenerateEmbeddingAsync(memory.Content);
                        embeddingBytes = SerializeEmbedding(embedding);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Warning, $"Erro ao gerar embedding: {ex.Message}");
                    }
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO UserMemory 
                    (Id, UserId, SessionId, Content, Type, CreatedAt, UpdatedAt, AccessCount, LastAccessedAt, 
                     RelevanceScore, Tags, Metadata, IsActive, Priority, Context, Embedding)
                    VALUES ($id, $userId, $sessionId, $content, $type, $createdAt, $updatedAt, $accessCount, 
                            $lastAccessedAt, $relevanceScore, $tags, $metadata, $isActive, $priority, $context, $embedding);";
                
                cmd.Parameters.AddWithValue("$id", memory.Id);
                cmd.Parameters.AddWithValue("$userId", memory.UserId ?? "");
                cmd.Parameters.AddWithValue("$sessionId", memory.SessionId ?? "");
                cmd.Parameters.AddWithValue("$content", memory.Content ?? "");
                cmd.Parameters.AddWithValue("$type", (int)memory.Type);
                cmd.Parameters.AddWithValue("$createdAt", memory.CreatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$updatedAt", memory.UpdatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$accessCount", memory.AccessCount);
                cmd.Parameters.AddWithValue("$lastAccessedAt", memory.LastAccessedAt?.ToString("o") ?? "");
                cmd.Parameters.AddWithValue("$relevanceScore", memory.RelevanceScore);
                cmd.Parameters.AddWithValue("$tags", string.Join(",", memory.Tags ?? new List<string>()));
                cmd.Parameters.AddWithValue("$metadata", System.Text.Json.JsonSerializer.Serialize(memory.Metadata ?? new Dictionary<string, object>()));
                cmd.Parameters.AddWithValue("$isActive", memory.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("$priority", memory.Priority);
                cmd.Parameters.AddWithValue("$context", memory.Context ?? "");
                cmd.Parameters.AddWithValue("$embedding", embeddingBytes);
                
                await cmd.ExecuteNonQueryAsync();
                return memory.Id;
            }
        }

        public async Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM UserMemory WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", memoryId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadUserMemory(reader);
                    }
                    return null;
                }
            }
        }

        public async Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                string whereClause;
                if (string.IsNullOrEmpty(userId))
                {
                    whereClause = "WHERE IsActive = 1";
                    if (sessionId != null)
                    {
                        whereClause += " AND SessionId = $sessionId";
                        cmd.Parameters.AddWithValue("$sessionId", sessionId);
                    }
                }
                else
                {
                    whereClause = "WHERE UserId = $userId AND IsActive = 1";
                    cmd.Parameters.AddWithValue("$userId", userId);
                    if (sessionId != null)
                    {
                        whereClause += " AND SessionId = $sessionId";
                        cmd.Parameters.AddWithValue("$sessionId", sessionId);
                    }
                }
                
                cmd.CommandText = $"SELECT * FROM UserMemory {whereClause} ORDER BY UpdatedAt DESC";
                
                var result = new List<UserMemory>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(ReadUserMemory(reader));
                    }
                    return result;
                }
            }
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            // Se temos embeddings disponíveis, usar busca semântica
            if (_embeddingService != null && !string.IsNullOrWhiteSpace(query))
            {
                return await SemanticSearchAsync(query, userId, limit);
            }
            
            // Fallback para busca textual tradicional
            return await TextualSearchAsync(query, userId, limit);
        }

        private async Task<List<UserMemory>> SemanticSearchAsync(string query, string userId, int limit)
        {
            try
            {
                var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
                return await SearchBySimilarityAsync(queryEmbedding, userId, limit);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro na busca semântica, usando busca textual: {ex.Message}");
                return await TextualSearchAsync(query, userId, limit);
            }
        }

        private async Task<List<UserMemory>> SearchBySimilarityAsync(List<float> queryEmbedding, string userId, int limit)
        {
            var results = new List<(UserMemory memory, double similarity)>();
            
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                cmd.CommandText = @"SELECT * FROM UserMemory 
                                   WHERE UserId = $userId AND IsActive = 1 AND Embedding IS NOT NULL
                                   ORDER BY UpdatedAt DESC";
                cmd.Parameters.AddWithValue("$userId", userId ?? "");
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var memory = ReadUserMemory(reader);
                        var embeddingBytes = reader["Embedding"] as byte[];
                        
                        if (embeddingBytes != null)
                        {
                            var embedding = DeserializeEmbedding(embeddingBytes);
                            var similarity = CalculateCosineSimilarity(queryEmbedding, embedding);
                            results.Add((memory, similarity));
                        }
                    }
                }
            }

            // Ordenar por similaridade e retornar top results
            return results
                .OrderByDescending(r => r.similarity)
                .Where(r => r.similarity > 0.5) // Threshold mínimo
                .Take(limit)
                .Select(r => r.memory)
                .ToList();
        }

        private async Task<List<UserMemory>> TextualSearchAsync(string query, string userId, int limit)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                var queryWords = query.ToLowerInvariant()
                    .Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2).Take(5).ToArray();
                
                if (queryWords.Length == 0)
                {
                    cmd.CommandText = "SELECT * FROM UserMemory WHERE UserId = $userId AND IsActive = 1 ORDER BY UpdatedAt DESC LIMIT $limit";
                    cmd.Parameters.AddWithValue("$userId", userId ?? "");
                    cmd.Parameters.AddWithValue("$limit", limit);
                }
                else
                {
                    var whereClauses = new List<string>();
                    for (int i = 0; i < queryWords.Length; i++)
                    {
                        whereClauses.Add($"LOWER(Content) LIKE $word{i}");
                        cmd.Parameters.AddWithValue($"$word{i}", "%" + queryWords[i] + "%");
                    }
                    
                    var whereClause = string.Join(" OR ", whereClauses);
                    cmd.CommandText = $@"
                        SELECT *, 
                               (RelevanceScore * 0.7 + 
                                CASE WHEN datetime(UpdatedAt) > datetime('now', '-1 day') THEN 0.3 ELSE 0.1 END) as SearchScore
                        FROM UserMemory 
                        WHERE UserId = $userId AND IsActive = 1 AND ({whereClause})
                        ORDER BY SearchScore DESC, RelevanceScore DESC, UpdatedAt DESC 
                        LIMIT $limit";
                    
                    cmd.Parameters.AddWithValue("$userId", userId ?? "");
                    cmd.Parameters.AddWithValue("$limit", limit);
                }
                
                var result = new List<UserMemory>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(ReadUserMemory(reader));
                    }
                    return result;
                }
            }
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                
                // Atualizar embedding se o conteúdo foi modificado
                byte[] embeddingBytes = null;
                if (_embeddingService != null && !string.IsNullOrEmpty(memory.Content))
                {
                    try
                    {
                        var embedding = await _embeddingService.GenerateEmbeddingAsync(memory.Content);
                        embeddingBytes = SerializeEmbedding(embedding);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Warning, $"Erro ao atualizar embedding: {ex.Message}");
                    }
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE UserMemory SET 
                    Content = $content, Type = $type, UpdatedAt = $updatedAt, AccessCount = $accessCount, 
                    LastAccessedAt = $lastAccessedAt, RelevanceScore = $relevanceScore, Tags = $tags, 
                    Metadata = $metadata, IsActive = $isActive, Priority = $priority, Context = $context, 
                    Embedding = $embedding
                    WHERE Id = $id";
                
                cmd.Parameters.AddWithValue("$id", memory.Id);
                cmd.Parameters.AddWithValue("$content", memory.Content ?? "");
                cmd.Parameters.AddWithValue("$type", (int)memory.Type);
                cmd.Parameters.AddWithValue("$updatedAt", memory.UpdatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$accessCount", memory.AccessCount);
                cmd.Parameters.AddWithValue("$lastAccessedAt", memory.LastAccessedAt?.ToString("o") ?? "");
                cmd.Parameters.AddWithValue("$relevanceScore", memory.RelevanceScore);
                cmd.Parameters.AddWithValue("$tags", string.Join(",", memory.Tags ?? new List<string>()));
                cmd.Parameters.AddWithValue("$metadata", System.Text.Json.JsonSerializer.Serialize(memory.Metadata ?? new Dictionary<string, object>()));
                cmd.Parameters.AddWithValue("$isActive", memory.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("$priority", memory.Priority);
                cmd.Parameters.AddWithValue("$context", memory.Context ?? "");
                cmd.Parameters.AddWithValue("$embedding", embeddingBytes);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE UserMemory SET IsActive = 0 WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", memoryId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void Clear()
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM UserMemory";
                cmd.ExecuteNonQuery();
            }
        }

        #region Helper Methods

        private UserMemory ReadUserMemory(SqliteDataReader reader)
        {
            var memory = new UserMemory
            {
                Tags = new List<string>(),
                Metadata = new Dictionary<string, object>()
            };
            
            memory.Id = reader["Id"].ToString();
            memory.UserId = reader["UserId"].ToString();
            memory.SessionId = reader["SessionId"].ToString();
            memory.Content = reader["Content"].ToString();
            memory.Type = (AgentMemoryType)Convert.ToInt32(reader["Type"]);
            memory.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());
            memory.UpdatedAt = DateTime.Parse(reader["UpdatedAt"].ToString());
            memory.AccessCount = Convert.ToInt32(reader["AccessCount"]);
            
            var lastAccessed = reader["LastAccessedAt"].ToString();
            if (!string.IsNullOrEmpty(lastAccessed))
            {
                memory.LastAccessedAt = DateTime.Parse(lastAccessed);
            }
            
            memory.RelevanceScore = Convert.ToDouble(reader["RelevanceScore"]);
            
            var tagsString = reader["Tags"].ToString() ?? "";
            memory.Tags = string.IsNullOrEmpty(tagsString) ?
                new List<string>() :
                new List<string>(tagsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                
            var metadataJson = reader["Metadata"].ToString();
            memory.Metadata = string.IsNullOrEmpty(metadataJson) ? 
                new Dictionary<string, object>() : 
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
                
            memory.IsActive = Convert.ToInt32(reader["IsActive"]) == 1;
            memory.Priority = Convert.ToInt32(reader["Priority"]);
            memory.Context = reader["Context"].ToString();
            
            return memory;
        }

        private byte[] SerializeEmbedding(List<float> embedding)
        {
            if (embedding == null || embedding.Count == 0) return null;
            
            var bytes = new byte[embedding.Count * 4];
            for (int i = 0; i < embedding.Count; i++)
            {
                var floatBytes = BitConverter.GetBytes(embedding[i]);
                Array.Copy(floatBytes, 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        private List<float> DeserializeEmbedding(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return new List<float>();
            
            var embedding = new List<float>();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var value = BitConverter.ToSingle(bytes, i);
                embedding.Add(value);
            }
            return embedding;
        }

        private double CalculateCosineSimilarity(List<float> vector1, List<float> vector2)
        {
            if (vector1.Count != vector2.Count) return 0;
            
            double dotProduct = 0;
            double norm1 = 0;
            double norm2 = 0;
            
            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                norm1 += vector1[i] * vector1[i];
                norm2 += vector2[i] * vector2[i];
            }
            
            if (norm1 == 0 || norm2 == 0) return 0;
            
            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        #endregion
    }
}