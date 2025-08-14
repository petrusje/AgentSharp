using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;

namespace AgentSharp.Core.Memory.Services
{
    // Classe auxiliar para armazenar mensagens com metadados de sessão
    internal class SessionMessage
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public AIMessage Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Simple in-memory implementation of IStorage for development and testing
    /// This is a minimal implementation that satisfies the IStorage interface
    /// </summary>
    public class InMemoryStorage : IStorage
    {
        private readonly List<Message> _messages = new List<Message>();
        private readonly List<UserMemory> _memories = new List<UserMemory>();
        private readonly List<AgentSession> _sessions = new List<AgentSession>();
        private readonly List<SessionMessage> _aiMessages = new List<SessionMessage>();
        private readonly Dictionary<string, List<float>> _embeddings = new Dictionary<string, List<float>>();

        public bool IsConnected => true;

        // Implementações simplificadas para as propriedades especializadas
        public ISessionStorage Sessions => new SimpleInMemorySessionStorage(_sessions);
        public IMemoryStorage Memories => new SimpleInMemoryMemoryStorage(_memories);
        public IEmbeddingStorage Embeddings => new SimpleInMemoryEmbeddingStorage(_embeddings);

        public Task InitializeAsync() => Task.CompletedTask;
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;
        
        public Task ClearAllAsync()
        {
            _messages.Clear();
            _memories.Clear();
            _sessions.Clear();
            _aiMessages.Clear();
            _embeddings.Clear();
            return Task.CompletedTask;
        }

        // Métodos legados da interface IStorage
        public Task SaveMessageAsync(Message message)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null)
        {
            var result = _messages.Where(m => m.SessionId == sessionId).ToList();
            if (limit.HasValue)
                result = result.Take(limit.Value).ToList();
            return Task.FromResult(result);
        }

        public Task SaveMemoryAsync(UserMemory memory)
        {
            var existing = _memories.FirstOrDefault(m => m.Id == memory.Id);
            if (existing != null)
                _memories.Remove(existing);
            _memories.Add(memory);
            return Task.CompletedTask;
        }

        public Task<List<UserMemory>> GetMemoriesAsync(MemoryContext context = null, int? limit = null)
        {
            var query = _memories.AsQueryable();
            if (context != null)
            {
                query = query.Where(m => 
                    (context.UserId == null || m.UserId == context.UserId) &&
                    (context.SessionId == null || m.SessionId == context.SessionId));
            }
            var result = query.ToList();
            if (limit.HasValue)
                result = result.Take(limit.Value).ToList();
            return Task.FromResult(result);
        }

        public Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10)
        {
            var result = _memories.Where(m => 
                m.Content.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                .Take(limit)
                .ToList();
            return Task.FromResult(result);
        }

        public Task UpdateMemoryAsync(UserMemory memory) => SaveMemoryAsync(memory);

        public Task DeleteMemoryAsync(string id)
        {
            var memory = _memories.FirstOrDefault(m => m.Id == id);
            if (memory != null)
                _memories.Remove(memory);
            return Task.CompletedTask;
        }

        public Task ClearMemoriesAsync(MemoryContext context = null)
        {
            if (context == null)
                _memories.Clear();
            else
            {
                var toRemove = _memories.Where(m => 
                    (context.UserId == null || m.UserId == context.UserId) &&
                    (context.SessionId == null || m.SessionId == context.SessionId))
                    .ToList();
                foreach (var memory in toRemove)
                    _memories.Remove(memory);
            }
            return Task.CompletedTask;
        }

        public Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
            {
                session = new AgentSession { Id = sessionId, UserId = userId, CreatedAt = DateTime.UtcNow };
                _sessions.Add(session);
            }
            return Task.FromResult(session as ISession);
        }

        public Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null)
        {
            var result = _sessions.Where(s => s.UserId == userId).Cast<ISession>().ToList();
            if (limit.HasValue)
                result = result.Take(limit.Value).ToList();
            return Task.FromResult(result);
        }

        public Task DeleteSessionAsync(string sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                _sessions.Remove(session);
                _messages.RemoveAll(m => m.SessionId == sessionId);
                _aiMessages.RemoveAll(m => m.SessionId == sessionId);
            }
            return Task.CompletedTask;
        }

        public Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10)
        {
            var result = _aiMessages
                .Where(m => m.UserId == userId && m.SessionId == sessionId)
                .OrderByDescending(m => m.Timestamp)
                .Select(m => m.Message)
                .Take(limit)
                .ToList();
            return Task.FromResult(result);
        }

        public Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message)
        {
            var sessionMessage = new SessionMessage
            {
                UserId = userId,
                SessionId = sessionId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            _aiMessages.Add(sessionMessage);
            return Task.CompletedTask;
        }
    }

    // Implementações simplificadas para os storages especializados
    internal class SimpleInMemorySessionStorage : ISessionStorage
    {
        private readonly List<AgentSession> _sessions;

        public SimpleInMemorySessionStorage(List<AgentSession> sessions)
        {
            _sessions = sessions;
        }

        public Task<string> CreateSessionAsync(AgentSession session)
        {
            _sessions.Add(session);
            return Task.FromResult(session.Id);
        }

        public Task<AgentSession> GetSessionAsync(string sessionId)
        {
            return Task.FromResult(_sessions.FirstOrDefault(s => s.Id == sessionId));
        }

        public Task<List<AgentSession>> GetUserSessionsAsync(string userId)
        {
            var result = _sessions.Where(s => s.UserId == userId).ToList();
            return Task.FromResult(result);
        }

        public Task UpdateSessionAsync(AgentSession session)
        {
            var existing = _sessions.FirstOrDefault(s => s.Id == session.Id);
            if (existing != null)
            {
                var index = _sessions.IndexOf(existing);
                _sessions[index] = session;
            }
            return Task.CompletedTask;
        }

        public Task DeleteSessionAsync(string sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
                _sessions.Remove(session);
            return Task.CompletedTask;
        }

        public void Clear() => _sessions.Clear();
    }

    internal class SimpleInMemoryMemoryStorage : IMemoryStorage
    {
        private readonly List<UserMemory> _memories;

        public SimpleInMemoryMemoryStorage(List<UserMemory> memories)
        {
            _memories = memories;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task<string> AddMemoryAsync(UserMemory memory)
        {
            _memories.Add(memory);
            return Task.FromResult(memory.Id);
        }

        public Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            return Task.FromResult(_memories.FirstOrDefault(m => m.Id == memoryId));
        }

        public Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            var query = _memories.Where(m => m.UserId == userId);
            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(m => m.SessionId == sessionId);
            return Task.FromResult(query.ToList());
        }

        public Task UpdateMemoryAsync(UserMemory memory)
        {
            var existing = _memories.FirstOrDefault(m => m.Id == memory.Id);
            if (existing != null)
            {
                var index = _memories.IndexOf(existing);
                _memories[index] = memory;
            }
            return Task.CompletedTask;
        }

        public Task DeleteMemoryAsync(string memoryId)
        {
            var memory = _memories.FirstOrDefault(m => m.Id == memoryId);
            if (memory != null)
                _memories.Remove(memory);
            return Task.CompletedTask;
        }

        public Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            var result = _memories
                .Where(m => m.UserId == userId &&
                           m.Content.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                .Take(limit)
                .ToList();
            return Task.FromResult(result);
        }

        public void Clear() => _memories.Clear();
    }

    internal class SimpleInMemoryEmbeddingStorage : IEmbeddingStorage
    {
        private readonly Dictionary<string, List<float>> _embeddings;
        private readonly Dictionary<string, Dictionary<string, object>> _metadata = new Dictionary<string, Dictionary<string, object>>();

        public SimpleInMemoryEmbeddingStorage(Dictionary<string, List<float>> embeddings)
        {
            _embeddings = embeddings;
        }

        public Task<string> StoreEmbeddingAsync(string content, List<float> embedding, Dictionary<string, object> metadata = null)
        {
            var id = Guid.NewGuid().ToString();
            _embeddings[id] = embedding;
            if (metadata != null)
                _metadata[id] = metadata;
            return Task.FromResult(id);
        }

        public Task<List<(string content, List<float> embedding, double similarity)>> SearchSimilarAsync(
            List<float> queryEmbedding, int limit = 10, double threshold = 0.7)
        {
            // Implementação básica sem cálculo de similaridade real
            var result = new List<(string, List<float>, double)>();
            return Task.FromResult(result);
        }

        public Task DeleteEmbeddingAsync(string id)
        {
            _embeddings.Remove(id);
            _metadata.Remove(id);
            return Task.CompletedTask;
        }

        public Task ClearEmbeddingsAsync()
        {
            _embeddings.Clear();
            _metadata.Clear();
            return Task.CompletedTask;
        }

        public void Clear()
        {
            _embeddings.Clear();
            _metadata.Clear();
        }
    }
}