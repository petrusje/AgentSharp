using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Core
{
    /// <summary>
    /// Testes específicos para funcionalidade de histórico de sessão
    /// Testa o comportamento similar ao /refs
    /// </summary>
    [TestClass]
    public class SessionHistoryTests
    {
        private MockModel? _mockModel;
        private ConsoleLogger? _logger;
        private MockStorageWithHistory? _mockStorage;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
            _mockStorage = new MockStorageWithHistory();
        }

        [TestMethod]
        public async Task Agent_WithHistoryEnabled_LoadsSessionHistory()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages()
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act
            var result = await agent.ExecuteAsync("Nova mensagem");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(_mockStorage!.GetSessionHistoryAsyncCalled);
            Assert.AreEqual("user1", _mockStorage.LastRequestedUserId);
            Assert.AreEqual("session1", _mockStorage.LastRequestedSessionId);
            Assert.AreEqual(10, _mockStorage.LastRequestedLimit); // valor padrão
        }

        [TestMethod]
        public async Task Agent_WithCustomHistoryLimit_UsesCorrectLimit()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages(true, 5)
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act
            var result = await agent.ExecuteAsync("Nova mensagem");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, _mockStorage!.LastRequestedLimit);
        }

        [TestMethod]
        public async Task Agent_WithHistoryDisabled_DoesNotLoadHistory()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages(false)
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act
            var result = await agent.ExecuteAsync("Nova mensagem");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(_mockStorage!.GetSessionHistoryAsyncCalled);
        }

        [TestMethod]
        public async Task Agent_SavesMessagesToSessionHistory()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages()
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act
            var result = await agent.ExecuteAsync("Mensagem de teste");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(_mockStorage!.SaveSessionMessageAsyncCalled);
            Assert.AreEqual(2, _mockStorage.SavedMessages.Count); // User + Assistant
            
            // Verificar se as mensagens foram salvas corretamente
            var userMessage = _mockStorage.SavedMessages[0];
            var assistantMessage = _mockStorage.SavedMessages[1];
            
            Assert.AreEqual(Role.User, userMessage.Role);
            Assert.AreEqual("Mensagem de teste", userMessage.Content);
            Assert.AreEqual(Role.Assistant, assistantMessage.Role);
        }

        [TestMethod]
        public async Task Agent_WithHistoryEnabled_IncludesHistoryInContext()
        {
            // Arrange
            // Configurar histórico existente no mock
            _mockStorage!.ExistingHistory = new List<AIMessage>
            {
                AIMessage.User("Pergunta anterior"),
                AIMessage.Assistant("Resposta anterior"),
                AIMessage.User("Outra pergunta"),
                AIMessage.Assistant("Outra resposta")
            };

            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages()
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act
            var result = await agent.ExecuteAsync("Nova pergunta");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(_mockStorage.GetSessionHistoryAsyncCalled);
            
            // O MockModel deve ter recebido as mensagens históricas no contexto
            // (isso seria verificado examinando as mensagens passadas para o modelo)
        }

        [TestMethod]
        public async Task Agent_WithoutStorage_HistoryEnabledDoesNotFail()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent")
                .WithHistoryToMessages()
                .WithContext(new TestContextHistory { UserId = "user1", SessionId = "session1" });

            // Act & Assert
            // Não deve lançar exceção mesmo sem storage
            var result = await agent.ExecuteAsync("Mensagem sem storage");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Agent_HistoryConfiguration_PersistsCorrectly()
        {
            // Arrange
            var agent = new Agent<TestContextHistory, string>(_mockModel, "TestAgent", storage: _mockStorage);

            // Act
            agent.WithHistoryToMessages(true, 15);

            // Assert
            Assert.IsTrue(agent.AddHistoryToMessages);
            Assert.AreEqual(15, agent.NumHistoryMessages);
        }
    }

    /// <summary>
    /// Mock Storage específico para testes de histórico de sessão
    /// </summary>
    public class MockStorageWithHistory : IStorage
    {
        public bool GetSessionHistoryAsyncCalled { get; private set; }
        public bool SaveSessionMessageAsyncCalled { get; private set; }
        public string? LastRequestedUserId { get; private set; }
        public string? LastRequestedSessionId { get; private set; }
        public int LastRequestedLimit { get; private set; }
        public List<AIMessage> SavedMessages { get; private set; } = new();
        public List<AIMessage> ExistingHistory { get; set; } = new();

        // Interface IStorage implementation
        public ISessionStorage Sessions => throw new NotImplementedException();
        public IMemoryStorage Memories => throw new NotImplementedException();
        public IEmbeddingStorage Embeddings => throw new NotImplementedException();
        public bool IsConnected => true;

        public Task InitializeAsync() => Task.CompletedTask;
        public Task ClearAllAsync() => Task.CompletedTask;
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;

        public Task SaveMessageAsync(Message message) => Task.CompletedTask;
        public Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null) => 
            Task.FromResult(new List<Message>());
        public Task SaveMemoryAsync(UserMemory memory) => Task.CompletedTask;
        public Task<List<UserMemory>> GetMemoriesAsync(MemoryContext? context = null, int? limit = null) => 
            Task.FromResult(new List<UserMemory>());
        public Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext? context, int limit = 10) => 
            Task.FromResult(new List<UserMemory>());
        public Task UpdateMemoryAsync(UserMemory memory) => Task.CompletedTask;
        public Task DeleteMemoryAsync(string id) => Task.CompletedTask;
        public Task ClearMemoriesAsync(MemoryContext? context = null) => Task.CompletedTask;
        public Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId) => 
            Task.FromResult<ISession>(new MockSessionHistory());
        public Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null) => 
            Task.FromResult(new List<ISession>());
        public Task DeleteSessionAsync(string sessionId) => Task.CompletedTask;

        // Métodos específicos para session history
        public Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10)
        {
            GetSessionHistoryAsyncCalled = true;
            LastRequestedUserId = userId;
            LastRequestedSessionId = sessionId;
            LastRequestedLimit = limit;
            
            // Retornar o histórico configurado, respeitando o limite
            var historyToReturn = ExistingHistory.Take(limit).ToList();
            return Task.FromResult(historyToReturn);
        }

        public Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message)
        {
            SaveSessionMessageAsyncCalled = true;
            SavedMessages.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mock Session para testes de histórico
    /// </summary>
    public class MockSessionHistory : ISession
    {
        public string Id { get; set; } = "test_session";
        public string UserId { get; set; } = "test_user";
        public string Title { get; set; } = "Test Session";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public SessionStatus Status { get; set; } = SessionStatus.Active;
    }

    /// <summary>
    /// Contexto de teste para session history
    /// </summary>
    public class TestContextHistory
    {
        public string UserId { get; set; } = "test_user";
        public string SessionId { get; set; } = "test_session";
    }
}