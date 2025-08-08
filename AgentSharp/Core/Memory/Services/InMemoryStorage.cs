using AgentSharp.Core.Memory.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Implementação em memória do Storage
    /// </summary>
    public class InMemoryStorage : IStorage
    {
        public ISessionStorage Sessions { get; }
        public IMemoryStorage Memories { get; }
        public IEmbeddingStorage Embeddings { get; }

        public InMemoryStorage()
        {
            Sessions = new InMemorySessionStorage();
            Memories = new InMemoryMemoryStorage();
            Embeddings = new InMemoryEmbeddingStorage();
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task ClearAllAsync()
        {
            ((InMemorySessionStorage)Sessions).Clear();
            ((InMemoryMemoryStorage)Memories).Clear();
            ((InMemoryEmbeddingStorage)Embeddings).Clear();
            return Task.CompletedTask;
        }

        #region Legacy IStorage Implementation (para compatibilidade)
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
            if (context?.UserId != null)
            {
                var memories = await Memories.GetMemoriesAsync(context.UserId, context.SessionId);
                foreach (var memory in memories)
                {
                    await Memories.DeleteMemoryAsync(memory.Id);
                }
            }
        }
        public async Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId)
        {
            var session = await Sessions.GetSessionAsync(sessionId);
            if (session == null)
            {
                session = new AgentSession
                {
                    Id = sessionId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await Sessions.CreateSessionAsync(session);
            }
            return session;
        }
        public async Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null)
        {
            var sessions = await Sessions.GetUserSessionsAsync(userId);
            return sessions.Cast<ISession>().ToList();
        }
        public async Task DeleteSessionAsync(string sessionId) => await Sessions.DeleteSessionAsync(sessionId);

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context, int limit = 10)
        {
            if (context?.UserId == null) return new List<UserMemory>();

            var memories = await Memories.GetMemoriesAsync(context.UserId, context.SessionId);
            var results = memories
                .Where(m => m.Content != null && m.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(limit)
                .ToList();

            return results;
        }

        #endregion
    }

    /// <summary>
    /// Implementação em memória do SessionStorage
    /// </summary>
    public class InMemorySessionStorage : ISessionStorage
    {
        private readonly ConcurrentDictionary<string, AgentSession> _sessions = new ConcurrentDictionary<string, AgentSession>();
        public Task<string> CreateSessionAsync(AgentSession session)
        {
            _sessions[session.Id] = session;
            return Task.FromResult(session.Id);
        }
        public Task<AgentSession> GetSessionAsync(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return Task.FromResult(session);
        }
        public Task<List<AgentSession>> GetUserSessionsAsync(string userId)
        {
            var sessions = _sessions.Values
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .ToList();
            return Task.FromResult(sessions);
        }
        public Task UpdateSessionAsync(AgentSession session)
        {
            if (_sessions.ContainsKey(session.Id))
            {
                session.UpdatedAt = DateTime.UtcNow;
                _sessions[session.Id] = session;
            }
            return Task.CompletedTask;
        }
        public Task DeleteSessionAsync(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
            return Task.CompletedTask;
        }
        public void Clear() => _sessions.Clear();
    }

    /// <summary>
    /// Implementação em memória do MemoryStorage
    /// </summary>
    public class InMemoryMemoryStorage : IMemoryStorage
    {
        private readonly ConcurrentDictionary<string, UserMemory> _memories = new ConcurrentDictionary<string, UserMemory>();
        public Task<string> AddMemoryAsync(UserMemory memory)
        {
            _memories[memory.Id] = memory;
            return Task.FromResult(memory.Id);
        }
        public Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            _memories.TryGetValue(memoryId, out var memory);
            return Task.FromResult(memory);
        }
        public Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            var query = _memories.Values.Where(m => m.UserId == userId && m.IsActive);
            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(m => m.SessionId == sessionId);
            var memories = query
                .OrderByDescending(m => m.UpdatedAt)
                .ToList();
            return Task.FromResult(memories);
        }
        public Task UpdateMemoryAsync(UserMemory memory)
        {
            if (_memories.ContainsKey(memory.Id))
            {
                memory.UpdatedAt = DateTime.UtcNow;
                _memories[memory.Id] = memory;
            }
            return Task.CompletedTask;
        }
        public Task DeleteMemoryAsync(string memoryId)
        {
            if (_memories.TryGetValue(memoryId, out var memory))
            {
                memory.IsActive = false;
                memory.UpdatedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
        public Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(userId))
                return Task.FromResult(new List<UserMemory>());
            var memories = _memories.Values
                .Where(m => m.UserId == userId && m.IsActive)
                .Where(m => m.Content != null && m.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(m => m.RelevanceScore)
                .ThenByDescending(m => m.UpdatedAt)
                .Take(limit)
                .ToList();
            return Task.FromResult(memories);
        }
        public void Clear() => _memories.Clear();
    }

    /// <summary>
    /// Implementação em memória do EmbeddingStorage
    /// </summary>
    public class InMemoryEmbeddingStorage : IEmbeddingStorage
    {
        private readonly ConcurrentDictionary<string, EmbeddingItem> _embeddings = new ConcurrentDictionary<string, EmbeddingItem>();
        public Task<string> StoreEmbeddingAsync(string content, List<float> embedding, Dictionary<string, object> metadata = null)
        {
            var id = Guid.NewGuid().ToString();
            var item = new EmbeddingItem
            {
                Id = id,
                Content = content,
                Embedding = embedding,
                Metadata = metadata ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };
            _embeddings[id] = item;
            return Task.FromResult(id);
        }
        public Task<List<(string content, List<float> embedding, double similarity)>> SearchSimilarAsync(List<float> queryEmbedding, int limit = 10, double threshold = 0.7)
        {
            var results = new List<(string content, List<float> embedding, double similarity)>();
            foreach (var item in _embeddings.Values)
            {
                var similarity = CalculateCosineSimilarity(queryEmbedding, item.Embedding);
                if (similarity >= threshold)
                {
                    results.Add((item.Content, item.Embedding, similarity));
                }
            }
            results = results
                .OrderByDescending(r => r.similarity)
                .Take(limit)
                .ToList();
            return Task.FromResult(results);
        }
        public Task DeleteEmbeddingAsync(string id)
        {
            _embeddings.TryRemove(id, out _);
            return Task.CompletedTask;
        }
        public Task ClearEmbeddingsAsync()
        {
            _embeddings.Clear();
            return Task.CompletedTask;
        }
        public void Clear() => _embeddings.Clear();
        private double CalculateCosineSimilarity(List<float> vec1, List<float> vec2)
        {
            if (vec1?.Count != vec2?.Count)
                return 0.0;
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;
            for (int i = 0; i < vec1.Count; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }
            norm1 = Math.Sqrt(norm1);
            norm2 = Math.Sqrt(norm2);
            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;
            return dotProduct / (norm1 * norm2);
        }
        private class EmbeddingItem
        {
            public string Id { get; set; }
            public string Content { get; set; }
            public List<float> Embedding { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
