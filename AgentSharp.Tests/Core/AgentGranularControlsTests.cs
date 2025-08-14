using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Core
{
    /// <summary>
    /// Testes para os controles granulares implementados no Agent
    /// Controles similares aos do framework /refs
    /// </summary>
    [TestClass]
    public class AgentGranularControlsTests
    {
        private MockModel? _mockModel;
        private ConsoleLogger? _logger;
        private MockStorage? _mockStorage;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
            _mockStorage = new MockStorage();
        }

        [TestMethod]
        public void WithUserMemories_SetsEnableUserMemoriesProperty()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithUserMemories();

            // Assert
            Assert.IsTrue(agent.EnableUserMemories);
        }

        [TestMethod]
        public void WithUserMemories_WithFalse_DisablesUserMemories()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithUserMemories(false);

            // Assert
            Assert.IsFalse(agent.EnableUserMemories);
        }

        [TestMethod]
        public void WithMemorySearch_SetsEnableMemorySearchProperty()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithMemorySearch();

            // Assert
            Assert.IsTrue(agent.EnableMemorySearch);
        }

        [TestMethod]
        public void WithMemorySearch_WithFalse_DisablesMemorySearch()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithMemorySearch(false);

            // Assert
            Assert.IsFalse(agent.EnableMemorySearch);
        }

        [TestMethod]
        public void WithHistoryToMessages_SetsAddHistoryToMessagesProperty()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: _mockStorage);

            // Act
            agent.WithHistoryToMessages();

            // Assert
            Assert.IsTrue(agent.AddHistoryToMessages);
            Assert.AreEqual(10, agent.NumHistoryMessages); // valor padrão
        }

        [TestMethod]
        public void WithHistoryToMessages_WithCustomCount_SetsProperties()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: _mockStorage);

            // Act
            agent.WithHistoryToMessages(true, 20);

            // Assert
            Assert.IsTrue(agent.AddHistoryToMessages);
            Assert.AreEqual(20, agent.NumHistoryMessages);
        }

        [TestMethod]
        public void WithKnowledgeSearch_SetsEnableKnowledgeSearchProperty()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithKnowledgeSearch();

            // Assert
            Assert.IsTrue(agent.EnableKnowledgeSearch);
        }

        [TestMethod]
        public void FluentInterface_AllowsChainingCalls()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: _mockStorage);

            // Act
            agent
                .WithUserMemories()
                .WithMemorySearch()
                .WithHistoryToMessages(true, 15)
                .WithKnowledgeSearch();

            // Assert
            Assert.IsTrue(agent.EnableUserMemories);
            Assert.IsTrue(agent.EnableMemorySearch);
            Assert.IsTrue(agent.AddHistoryToMessages);
            Assert.AreEqual(15, agent.NumHistoryMessages);
            Assert.IsTrue(agent.EnableKnowledgeSearch);
        }

        [TestMethod]
        public void DefaultConfiguration_HasExpectedValues()
        {
            // Arrange & Act
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent");

            // Assert
            Assert.IsFalse(agent.EnableUserMemories);
            Assert.IsFalse(agent.EnableMemorySearch);
            Assert.IsFalse(agent.AddHistoryToMessages);
            Assert.AreEqual(10, agent.NumHistoryMessages);
            Assert.IsFalse(agent.EnableKnowledgeSearch);
        }

        [TestMethod]
        public async Task Agent_WithHistoryEnabled_ShouldNotThrowException()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: _mockStorage)
                .WithHistoryToMessages()
                .WithContext(new TestContext { UserId = "test_user", SessionId = "test_session" });

            // Act & Assert
            // O teste principal é que não deve lançar exceção
            var result = await agent.ExecuteAsync("Test message");
            Assert.IsNotNull(result);
            
            // Verificar que as configurações estão corretas
            Assert.IsTrue(agent.AddHistoryToMessages, "AddHistoryToMessages should be enabled");
            Assert.AreEqual(10, agent.NumHistoryMessages, "Should use default history message count");
        }

        [TestMethod]
        public async Task Agent_WithoutStorageAndHistoryEnabled_ShouldStillWork()
        {
            // Arrange
            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent")
                .WithHistoryToMessages()
                .WithContext(new TestContext { UserId = "test_user", SessionId = "test_session" });

            // Act
            var result = await agent.ExecuteAsync("Test message");

            // Assert
            Assert.IsNotNull(result);
            // Não deve gerar erro mesmo sem storage
        }
    }

    /// <summary>
    /// Mock Storage para testes unitários
    /// </summary>
    public class MockStorage : IStorage
    {
        public bool GetSessionHistoryAsyncCalled { get; private set; }
        public bool SaveSessionMessageAsyncCalled { get; private set; }

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
            Task.FromResult<ISession>(new MockSession());
        public Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null) => 
            Task.FromResult(new List<ISession>());
        public Task DeleteSessionAsync(string sessionId) => Task.CompletedTask;

        public Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10)
        {
            GetSessionHistoryAsyncCalled = true;
            return Task.FromResult(new List<AIMessage>
            {
                AIMessage.User("Previous user message"),
                AIMessage.Assistant("Previous assistant response")
            });
        }

        public Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message)
        {
            SaveSessionMessageAsyncCalled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mock Session para testes
    /// </summary>
    public class MockSession : ISession
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
    /// Contexto de teste
    /// </summary>
    public class TestContext
    {
        public string UserId { get; set; } = "test_user";
        public string SessionId { get; set; } = "test_session";
    }
}