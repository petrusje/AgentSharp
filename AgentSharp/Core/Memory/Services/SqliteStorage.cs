using AgentSharp.Core.Memory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Implementação completa de Storage usando SQLite.
    /// Fornece persistência para memórias e sessões de agentes.
    /// </summary>
    public class SqliteStorage : IStorage
    {
        public ISessionStorage Sessions { get; }
        public IMemoryStorage Memories { get; }
        public IEmbeddingStorage Embeddings { get; }

        public SqliteStorage(string connectionString)
        {
            // TODO: Inicializar providers reais usando SQLite
            Sessions = new SqliteSessionStorage(connectionString);
            Memories = new SqliteMemoryStorage(connectionString);
            Embeddings = new SqliteEmbeddingStorage(connectionString);
        }

        public async Task InitializeAsync()
        {
            // Inicializar todas as storages
            await ((SqliteSessionStorage)Sessions).InitializeAsync();
            // MemoryStorage já é inicializado no construtor
            // EmbeddingStorage postergado
        }

        public async Task ClearAllAsync()
        {
            // Limpar todas as tabelas
            ((SqliteSessionStorage)Sessions).Clear();
            ((SqliteMemoryStorage)Memories).Clear();
            ((SqliteEmbeddingStorage)Embeddings).Clear();
            await Task.CompletedTask;
        }

        // Métodos legados para compatibilidade
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
                // Se context é null, buscar todas as memórias ativas (até o limite especificado)
                return Memories.GetMemoriesAsync("", null);
            }
            return Memories.GetMemoriesAsync(context.UserId, context.SessionId);
        }
        public Task<List<UserMemory>> SearchMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context, int limit = 10)
        {
            return Memories.SearchMemoriesAsync(query, context?.UserId ?? "", limit);
        }
        public Task UpdateMemoryAsync(UserMemory memory) => Memories.UpdateMemoryAsync(memory);
        public Task DeleteMemoryAsync(string id) => Memories.DeleteMemoryAsync(id);
        public Task ClearMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            // TODO: Limpar memórias do contexto
            return Task.CompletedTask;
        }
        public async Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId)
        {
            // Buscar sessão existente
            var session = await Sessions.GetSessionAsync(sessionId);
            if (session != null)
            {
                return session;
            }

            // Criar nova sessão se não existir
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
    /// Implementação SQLite para armazenamento de sessões
    /// </summary>
    public class SqliteSessionStorage : ISessionStorage
    {
        private readonly string _connectionString;

        public SqliteSessionStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AgentSession (
                        Id TEXT PRIMARY KEY,
                        UserId TEXT NOT NULL,
                        Title TEXT,
                        Description TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL,
                        LastActivity TEXT NOT NULL,
                        LastAccessedAt TEXT,
                        IsActive INTEGER NOT NULL,
                        Metadata TEXT,
                        Settings TEXT,
                        Tags TEXT,
                        Status INTEGER NOT NULL
                    );";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<string> CreateSessionAsync(AgentSession session)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO AgentSession (Id, UserId, Title, Description, CreatedAt, UpdatedAt, LastActivity, LastAccessedAt, IsActive, Metadata, Settings, Tags, Status)
                    VALUES ($id, $userId, $title, $description, $createdAt, $updatedAt, $lastActivity, $lastAccessedAt, $isActive, $metadata, $settings, $tags, $status);";
                
                cmd.Parameters.AddWithValue("$id", session.Id);
                cmd.Parameters.AddWithValue("$userId", session.UserId ?? "");
                cmd.Parameters.AddWithValue("$title", session.Title ?? "");
                cmd.Parameters.AddWithValue("$description", session.Description ?? "");
                cmd.Parameters.AddWithValue("$createdAt", session.CreatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$updatedAt", session.UpdatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$lastActivity", session.LastActivity.ToString("o"));
                cmd.Parameters.AddWithValue("$lastAccessedAt", session.LastAccessedAt?.ToString("o") ?? "");
                cmd.Parameters.AddWithValue("$isActive", session.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("$metadata", System.Text.Json.JsonSerializer.Serialize(session.Metadata ?? new Dictionary<string, object>()));
                cmd.Parameters.AddWithValue("$settings", System.Text.Json.JsonSerializer.Serialize(session.Settings ?? new Dictionary<string, string>()));
                cmd.Parameters.AddWithValue("$tags", string.Join(",", session.Tags ?? new List<string>()));
                cmd.Parameters.AddWithValue("$status", (int)session.Status);
                
                await cmd.ExecuteNonQueryAsync();
                return session.Id;
            }
        }

        public async Task<AgentSession> GetSessionAsync(string sessionId)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM AgentSession WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", sessionId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadAgentSession(reader);
                    }
                    return null;
                }
            }
        }

        public async Task<List<AgentSession>> GetUserSessionsAsync(string userId)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM AgentSession WHERE UserId = $userId ORDER BY UpdatedAt DESC";
                cmd.Parameters.AddWithValue("$userId", userId);
                
                var result = new List<AgentSession>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(ReadAgentSession(reader));
                    }
                }
                return result;
            }
        }

        public async Task UpdateSessionAsync(AgentSession session)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE AgentSession SET 
                        Title = $title,
                        Description = $description,
                        UpdatedAt = $updatedAt,
                        LastActivity = $lastActivity,
                        LastAccessedAt = $lastAccessedAt,
                        IsActive = $isActive,
                        Metadata = $metadata,
                        Settings = $settings,
                        Tags = $tags,
                        Status = $status
                    WHERE Id = $id";
                
                cmd.Parameters.AddWithValue("$id", session.Id);
                cmd.Parameters.AddWithValue("$title", session.Title ?? "");
                cmd.Parameters.AddWithValue("$description", session.Description ?? "");
                cmd.Parameters.AddWithValue("$updatedAt", session.UpdatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$lastActivity", session.LastActivity.ToString("o"));
                cmd.Parameters.AddWithValue("$lastAccessedAt", session.LastAccessedAt?.ToString("o") ?? "");
                cmd.Parameters.AddWithValue("$isActive", session.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("$metadata", System.Text.Json.JsonSerializer.Serialize(session.Metadata ?? new Dictionary<string, object>()));
                cmd.Parameters.AddWithValue("$settings", System.Text.Json.JsonSerializer.Serialize(session.Settings ?? new Dictionary<string, string>()));
                cmd.Parameters.AddWithValue("$tags", string.Join(",", session.Tags ?? new List<string>()));
                cmd.Parameters.AddWithValue("$status", (int)session.Status);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE AgentSession SET IsActive = 0 WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", sessionId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void Clear()
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM AgentSession";
                cmd.ExecuteNonQuery();
            }
        }

        private static AgentSession ReadAgentSession(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            var session = new AgentSession
            {
                Id = reader["Id"].ToString(),
                UserId = reader["UserId"].ToString(),
                Title = reader["Title"].ToString(),
                Description = reader["Description"].ToString(),
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = DateTime.Parse(reader["UpdatedAt"].ToString()),
                LastActivity = DateTime.Parse(reader["LastActivity"].ToString()),
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                Status = (AgentSharp.Core.Memory.Interfaces.SessionStatus)Convert.ToInt32(reader["Status"])
            };

            var lastAccessed = reader["LastAccessedAt"].ToString();
            if (!string.IsNullOrEmpty(lastAccessed))
            {
                session.LastAccessedAt = DateTime.Parse(lastAccessed);
            }

            var metadataJson = reader["Metadata"].ToString();
            session.Metadata = string.IsNullOrEmpty(metadataJson) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);

            var settingsJson = reader["Settings"].ToString();
            session.Settings = string.IsNullOrEmpty(settingsJson) 
                ? new Dictionary<string, string>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(settingsJson);

            var tagsString = reader["Tags"].ToString() ?? "";
            session.Tags = string.IsNullOrEmpty(tagsString) 
                ? new List<string>() 
                : new List<string>(tagsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            return session;
        }
    }
    public class SqliteMemoryStorage : IMemoryStorage
    {
        private readonly string _connectionString;

        public SqliteMemoryStorage(string connectionString)
        {
            _connectionString = connectionString;
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                conn.Open();
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
                    Context TEXT
                );";
                cmd.ExecuteNonQuery();
            }
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO UserMemory (Id, UserId, SessionId, Content, Type, CreatedAt, UpdatedAt, AccessCount, LastAccessedAt, RelevanceScore, Tags, Metadata, IsActive, Priority, Context)
                    VALUES ($id, $userId, $sessionId, $content, $type, $createdAt, $updatedAt, $accessCount, $lastAccessedAt, $relevanceScore, $tags, $metadata, $isActive, $priority, $context);";
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
                await cmd.ExecuteNonQueryAsync();
                return memory.Id;
            }
        }

        public async Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
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
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                // Se userId for vazio ou null, buscar todas as memórias ativas
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

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE UserMemory SET Content = $content, Type = $type, UpdatedAt = $updatedAt, AccessCount = $accessCount, LastAccessedAt = $lastAccessedAt, RelevanceScore = $relevanceScore, Tags = $tags, Metadata = $metadata, IsActive = $isActive, Priority = $priority, Context = $context WHERE Id = $id";
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
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE UserMemory SET IsActive = 0 WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", memoryId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                
                // Melhor query com busca mais inteligente
                // Buscar por múltiplas palavras da query e dar peso por relevância e recência
                var queryWords = query.ToLowerInvariant().Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2).Take(5).ToArray();
                
                if (queryWords.Length == 0)
                {
                    // Se não há palavras válidas, retornar memórias mais recentes
                    cmd.CommandText = "SELECT * FROM UserMemory WHERE UserId = $userId AND IsActive = 1 ORDER BY UpdatedAt DESC LIMIT $limit";
                    cmd.Parameters.AddWithValue("$userId", userId ?? "");
                    cmd.Parameters.AddWithValue("$limit", limit);
                }
                else
                {
                    // Construir query dinâmica com OR para múltiplas palavras
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

        public void Clear()
        {
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM UserMemory";
                cmd.ExecuteNonQuery();
            }
        }

        private UserMemory ReadUserMemory(Microsoft.Data.Sqlite.SqliteDataReader reader)
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
            memory.Metadata = string.IsNullOrEmpty(metadataJson) ? new Dictionary<string, object>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
            memory.IsActive = Convert.ToInt32(reader["IsActive"]) == 1;
            memory.Priority = Convert.ToInt32(reader["Priority"]);
            memory.Context = reader["Context"].ToString();
            return memory;
        }
    }
    public class SqliteEmbeddingStorage : IEmbeddingStorage
    {
        public SqliteEmbeddingStorage(string connectionString) 
        {
            // Connection string armazenado para uso futuro quando embeddings forem implementados
            _ = connectionString;
        }
        // Embeddings: implementação postergada. Métodos lançam NotImplementedException.
        public Task<string> StoreEmbeddingAsync(string content, List<float> embedding, Dictionary<string, object> metadata = null)
            => throw new NotImplementedException("Embeddings: implementação postergada. Use provider especializado.");
        public Task<List<(string content, List<float> embedding, double similarity)>> SearchSimilarAsync(List<float> queryEmbedding, int limit = 10, double threshold = 0.7)
            => throw new NotImplementedException("Busca semântica: implementação postergada. Use provider especializado.");
        public Task DeleteEmbeddingAsync(string id)
            => throw new NotImplementedException("Embeddings: implementação postergada.");
        public Task ClearEmbeddingsAsync()
            => throw new NotImplementedException("Embeddings: implementação postergada.");
        public void Clear() { }
    }
}
