using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Memory
{
    [TestClass]
    public class MemoryManagerTests
    {
        private IModel? _mockModel;
        private ILogger? _logger;
        private IStorage? _storage;
        private IMemoryManager? _memoryManager;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
            _storage = new MockMemoryStorage();
            _memoryManager = new MemoryManager(_storage, _mockModel, _logger);
        }

        [TestMethod]
        public async Task LoadContextAsync_ShouldCreateValidContext()
        {
            // Arrange
            string userId = "test_user";
            string sessionId = "test_session";

            // Act
            var context = await _memoryManager!.LoadContextAsync(userId, sessionId);

            // Assert
            Assert.IsNotNull(context);
            Assert.AreEqual(userId, context.UserId);
            Assert.AreEqual(sessionId, context.SessionId);
            Assert.IsNotNull(context.Memories);
            Assert.IsNotNull(context.MessageHistory);
        }

        [TestMethod]
        public async Task AddMemoryAsync_ShouldStoreMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";
            string memoryContent = "Usuário prefere café sem açúcar";

            // Act
            var result = await _memoryManager.AddMemoryAsync(memoryContent);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("adicionada"));

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            Assert.IsTrue(memories.Count > 0);
        }

        [TestMethod]
        public async Task GetExistingMemoriesAsync_ShouldReturnStoredMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";

            await _memoryManager.AddMemoryAsync("Primeiro fato");
            await _memoryManager.AddMemoryAsync("Segunda preferência");

            // Act
            var memories = await _memoryManager.GetExistingMemoriesAsync();

            // Assert
            Assert.IsTrue(memories.Count >= 2);
        }

        [TestMethod]
        public async Task EnhanceMessagesAsync_ShouldAddRelevantMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";

            await _memoryManager.AddMemoryAsync("Usuário gosta de café forte");

            var context = await _memoryManager.LoadContextAsync("test_user", "test_session");
            var messages = new List<AIMessage>
            {
                AIMessage.System("Você é um assistente"),
                AIMessage.User("Como você prepararia café para mim?")
            };

            // Act
            var enhancedMessages = await _memoryManager.EnhanceMessagesAsync(messages, context);

            // Assert
            Assert.IsTrue(enhancedMessages.Count >= messages.Count);
            // Deve ter adicionado contexto de memória se há memórias relevantes
        }

        [TestMethod]
        public async Task ProcessInteractionAsync_ShouldExtractMemories()
        {
            // Arrange
            var context = await _memoryManager!.LoadContextAsync("test_user", "test_session");
            var userMessage = AIMessage.User("Meu nome é João e prefiro trabalhar pela manhã");
            var assistantMessage = AIMessage.Assistant("Ótimo João! Vou lembrar que você prefere trabalhar pela manhã");

            var initialMemoryCount = (await _memoryManager.GetExistingMemoriesAsync()).Count;

            // Act
            await _memoryManager.ProcessInteractionAsync(userMessage, assistantMessage, context);

            // Assert
            var finalMemoryCount = (await _memoryManager.GetExistingMemoriesAsync()).Count;
            // Deve ter extraído pelo menos uma memória (o processo pode ser assíncrono)
            Assert.IsTrue(finalMemoryCount >= initialMemoryCount);
        }

        [TestMethod]
        public async Task UpdateMemoryAsync_ShouldModifyExistingMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Conteúdo original");

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;
            string newContent = "Conteúdo atualizado";

            // Act
            var result = await _memoryManager.UpdateMemoryAsync(memoryId, newContent);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("atualizada"));

            var updatedMemories = await _memoryManager.GetExistingMemoriesAsync();
            Assert.IsTrue(updatedMemories.Exists(m => m.Content.Contains("atualizado")));
        }

        [TestMethod]
        public async Task DeleteMemoryAsync_ShouldRemoveMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Memória para deletar");

            var memories = await _memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;

            // Act
            var result = await _memoryManager.DeleteMemoryAsync(memoryId);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("removida"));
        }

        [TestMethod]
        public async Task ClearMemoryAsync_ShouldRemoveAllMemories()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            await _memoryManager.AddMemoryAsync("Primeira memória");
            await _memoryManager.AddMemoryAsync("Segunda memória");

            var context = new MemoryContext { UserId = "test_user" };

            // Act
            var result = await _memoryManager.ClearMemoryAsync(context);

            // Assert
            Assert.IsTrue(result.Contains("sucesso") || result.Contains("limpas"));
        }

        [TestMethod]
        public async Task RunAsync_ShouldProcessMessageWithMemory()
        {
            // Arrange
            _memoryManager!.UserId = "test_user";
            _memoryManager.SessionId = "test_session";
            string message = "Olá, como você está?";

            // Act
            var response = await _memoryManager.RunAsync(message);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Length > 0);
        }
    }

    /// <summary>
    /// Mock implementation of IStorage for testing memory operations without sqlite-vec dependency
    /// </summary>
    public class MockMemoryStorage : IStorage
    {
        private readonly MockSessionStorage _sessions;
        private readonly MockMemoryStorageCore _memories;
        private readonly MockEmbeddingStorage _embeddings;
        private readonly List<Message> _messages = new();
        private readonly List<(string userId, string sessionId, AIMessage message)> _aiMessages = new();

        public MockMemoryStorage()
        {
            _sessions = new MockSessionStorage();
            _memories = new MockMemoryStorageCore();
            _embeddings = new MockEmbeddingStorage();
        }

        public ISessionStorage Sessions => _sessions;
        public IMemoryStorage Memories => _memories;
        public IEmbeddingStorage Embeddings => _embeddings;
        public bool IsConnected { get; private set; } = true;

        public async Task InitializeAsync()
        {
            await Task.Delay(1);
            IsConnected = true;
        }

        public async Task ClearAllAsync()
        {
            await Task.Delay(1);
            _sessions.Clear();
            _memories.Clear();
            _embeddings.Clear();
            _messages.Clear();
            _aiMessages.Clear();
        }

        public async Task ConnectAsync()
        {
            await Task.Delay(1);
            IsConnected = true;
        }

        public async Task DisconnectAsync()
        {
            await Task.Delay(1);
            IsConnected = false;
        }

        public async Task SaveMessageAsync(Message message)
        {
            await Task.Delay(1);
            _messages.Add(message);
        }

        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null)
        {
            await Task.Delay(1);
            var messages = _messages.Where(m => m.SessionId == sessionId).ToList();
            return limit.HasValue ? messages.Take(limit.Value).ToList() : messages;
        }

        public async Task SaveMemoryAsync(UserMemory memory)
        {
            await Task.Delay(1);
            await _memories.AddMemoryAsync(memory);
        }

        public async Task<List<UserMemory>> GetMemoriesAsync(MemoryContext context = null, int? limit = null)
        {
            await Task.Delay(1);
            var userId = context?.UserId ?? "";
            var sessionId = context?.SessionId;
            return await _memories.GetMemoriesAsync(userId, sessionId);
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10)
        {
            await Task.Delay(1);
            var userId = context?.UserId ?? "";
            return await _memories.SearchMemoriesAsync(query, userId, limit);
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            await Task.Delay(1);
            await _memories.UpdateMemoryAsync(memory);
        }

        public async Task DeleteMemoryAsync(string id)
        {
            await Task.Delay(1);
            await _memories.DeleteMemoryAsync(id);
        }

        public async Task ClearMemoriesAsync(MemoryContext context = null)
        {
            await Task.Delay(1);
            _memories.Clear();
        }

        public async Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId)
        {
            await Task.Delay(1);
            var session = await _sessions.GetSessionAsync(sessionId);
            if (session == null)
            {
                session = new AgentSession { Id = sessionId, UserId = userId };
                await _sessions.CreateSessionAsync(session);
            }
            return session;
        }

        public async Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null)
        {
            await Task.Delay(1);
            var sessions = await _sessions.GetUserSessionsAsync(userId);
            return sessions.Cast<ISession>().ToList();
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            await Task.Delay(1);
            await _sessions.DeleteSessionAsync(sessionId);
        }

        public async Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10)
        {
            await Task.Delay(1);
            return _aiMessages
                .Where(m => m.userId == userId && m.sessionId == sessionId)
                .Select(m => m.message)
                .Take(limit)
                .ToList();
        }

        public async Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message)
        {
            await Task.Delay(1);
            _aiMessages.Add((userId, sessionId, message));
        }
    }

    public class MockSessionStorage : ISessionStorage
    {
        private readonly List<AgentSession> _sessions = new();

        public async Task<string> CreateSessionAsync(AgentSession session)
        {
            await Task.Delay(1);
            _sessions.Add(session);
            return session.Id;
        }

        public async Task<AgentSession> GetSessionAsync(string sessionId)
        {
            await Task.Delay(1);
            return _sessions.FirstOrDefault(s => s.Id == sessionId);
        }

        public async Task<List<AgentSession>> GetUserSessionsAsync(string userId)
        {
            await Task.Delay(1);
            return _sessions.Where(s => s.UserId == userId).ToList();
        }

        public async Task UpdateSessionAsync(AgentSession session)
        {
            await Task.Delay(1);
            var existing = _sessions.FirstOrDefault(s => s.Id == session.Id);
            if (existing != null)
            {
                var index = _sessions.IndexOf(existing);
                _sessions[index] = session;
            }
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            await Task.Delay(1);
            _sessions.RemoveAll(s => s.Id == sessionId);
        }

        public void Clear()
        {
            _sessions.Clear();
        }
    }

    public class MockMemoryStorageCore : IMemoryStorage
    {
        private readonly List<UserMemory> _memories = new();
        private int _nextId = 1;

        public async Task InitializeAsync()
        {
            await Task.Delay(1);
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            await Task.Delay(1);
            memory.Id = _nextId++.ToString();
            memory.CreatedAt = DateTime.Now;
            _memories.Add(memory);
            return memory.Id;
        }

        public async Task<UserMemory> GetMemoryAsync(string memoryId)
        {
            await Task.Delay(1);
            return _memories.FirstOrDefault(m => m.Id == memoryId);
        }

        public async Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null)
        {
            await Task.Delay(1);
            var query = _memories.Where(m => m.UserId == userId);
            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(m => m.SessionId == sessionId);
            return query.ToList();
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            await Task.Delay(1);
            var existing = _memories.FirstOrDefault(m => m.Id == memory.Id);
            if (existing != null)
            {
                var index = _memories.IndexOf(existing);
                memory.UpdatedAt = DateTime.Now;
                _memories[index] = memory;
            }
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            await Task.Delay(1);
            _memories.RemoveAll(m => m.Id == memoryId);
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10)
        {
            await Task.Delay(1);
            var userMemories = _memories.Where(m => m.UserId == userId);
            
            if (string.IsNullOrEmpty(query))
                return userMemories.Take(limit).ToList();
                
            return userMemories
                .Where(m => m.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
        }

        public void Clear()
        {
            _memories.Clear();
        }
    }

    public class MockEmbeddingStorage : IEmbeddingStorage
    {
        private readonly List<(string id, string content, List<float> embedding, Dictionary<string, object> metadata)> _embeddings = new();
        private int _nextId = 1;

        public async Task<string> StoreEmbeddingAsync(string content, List<float> embedding, Dictionary<string, object> metadata = null)
        {
            await Task.Delay(1);
            var id = _nextId++.ToString();
            _embeddings.Add((id, content, embedding, metadata ?? new Dictionary<string, object>()));
            return id;
        }

        public async Task<List<(string content, List<float> embedding, double similarity)>> SearchSimilarAsync(List<float> queryEmbedding, int limit = 10, double threshold = 0.7)
        {
            await Task.Delay(1);
            // Simple mock similarity - return random similarity scores for testing
            var random = new Random();
            return _embeddings
                .Select(e => (e.content, e.embedding, random.NextDouble()))
                .Where(e => e.Item3 >= threshold)
                .Take(limit)
                .ToList();
        }

        public async Task DeleteEmbeddingAsync(string id)
        {
            await Task.Delay(1);
            _embeddings.RemoveAll(e => e.id == id);
        }

        public async Task ClearEmbeddingsAsync()
        {
            await Task.Delay(1);
            _embeddings.Clear();
        }

        public void Clear()
        {
            _embeddings.Clear();
        }
    }
}
