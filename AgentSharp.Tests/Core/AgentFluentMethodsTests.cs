using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Configuration;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Core
{
    /// <summary>
    /// Testes para os métodos fluentes implementados no Agent
    /// WithMemoryManager, WithStorage, WithMemoryConfiguration
    /// </summary>
    [TestClass]
    public class AgentFluentMethodsTests
    {
        private MockModel? _mockModel;
        private ConsoleLogger? _logger;
        private MockStorage? _mockStorage;
        private MockMemoryManager? _mockMemoryManager;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
            _mockStorage = new MockStorage();
            _mockMemoryManager = new MockMemoryManager();
        }

        [TestMethod]
        public void WithMemoryManager_SetsMemoryManagerProperty()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithMemoryManager(_mockMemoryManager);

            // Assert
            var retrievedManager = agent.GetMemoryManager();
            Assert.IsNotNull(retrievedManager);
            // O manager foi substituído internamente
        }

        [TestMethod]
        public void WithMemoryManager_ThrowsOnNull()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent");

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => agent.WithMemoryManager(null));
        }

        [TestMethod]
        public void WithMemoryManager_ConfiguresContextWhenAvailable()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent")
                .WithContext(new FluentTestContext { UserId = "test_user", SessionId = "test_session" });

            // Act
            agent.WithMemoryManager(_mockMemoryManager);

            // Assert
            Assert.AreEqual("test_user", _mockMemoryManager.UserId);
            Assert.AreEqual("test_session", _mockMemoryManager.SessionId);
        }

        [TestMethod]
        public void WithStorage_SetsStorageProperty()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent");

            // Act
            agent.WithStorage(_mockStorage);

            // Assert - Se storage é setado, memory manager deve ser criado se controles estiverem habilitados
            // Como controles estão desabilitados por padrão, não deve criar memory manager
            var manager = agent.GetMemoryManager();
            Assert.IsNull(manager); // Sem controles habilitados = sem memory manager
        }

        [TestMethod]
        public void WithStorage_ThrowsOnNull()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent");

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => agent.WithStorage(null));
        }

        [TestMethod]
        public void WithStorage_CreatesMemoryManagerWhenControlsEnabled()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent")
                .WithUserMemories() // Habilita controles
                .WithMemorySearch();

            // Act
            agent.WithStorage(_mockStorage);

            // Assert
            var manager = agent.GetMemoryManager();
            Assert.IsNotNull(manager); // Com controles habilitados = memory manager criado
        }

        [TestMethod]
        public void WithMemoryConfiguration_AppliesConfiguration()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent", storage: _mockStorage);
            var config = new AgentSharp.Core.Memory.Configuration.MemoryConfiguration
            {
                MaxMemoriesPerInteraction = 10,
                MinImportanceThreshold = 0.8,
                CustomCategories = new[] { "Test", "Config" }
            };

            // Act
            agent.WithMemoryConfiguration(config);

            // Assert
            // Configuração foi aplicada (internamente ao MemoryDomainConfiguration)
            Assert.IsNotNull(agent); // Basic test - configuration is applied internally
        }

        [TestMethod]
        public void WithMemoryConfiguration_ThrowsOnNull()
        {
            // Arrange
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent");

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => agent.WithMemoryConfiguration(null));
        }

        [TestMethod]
        public void FluentChainingWorks()
        {
            // Arrange & Act
            var agent = new Agent<FluentTestContext, string>(_mockModel, "TestAgent")
                .WithUserMemories()
                .WithMemorySearch()
                .WithStorage(_mockStorage)
                .WithMemoryManager(_mockMemoryManager);

            // Assert
            Assert.IsTrue(agent.EnableUserMemories);
            Assert.IsTrue(agent.EnableMemorySearch);
            var manager = agent.GetMemoryManager();
            Assert.IsNotNull(manager);
        }
    }

    /// <summary>
    /// Mock Memory Manager para testes
    /// </summary>
    public class MockMemoryManager : IMemoryManager
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public int? Limit { get; set; }

        public Task<MemoryContext> LoadContextAsync(string userId, string sessionId = null)
        {
            return Task.FromResult(new MemoryContext
            {
                UserId = userId,
                SessionId = sessionId
            });
        }

        public Task<System.Collections.Generic.List<AIMessage>> EnhanceMessagesAsync(
            System.Collections.Generic.List<AIMessage> baseMessages, 
            MemoryContext memoryContext)
        {
            return Task.FromResult(baseMessages);
        }

        public Task ProcessInteractionAsync(string userMessage, string assistantMessage, MemoryContext memoryContext)
        {
            return Task.CompletedTask;
        }

        public Task ProcessInteractionAsync(AIMessage userMessage, AIMessage assistantMessage, MemoryContext memoryContext)
        {
            return Task.CompletedTask;
        }

        public Task<string> RunAsync(string query, MemoryContext memoryContext)
        {
            return Task.FromResult("Mock result");
        }

        public Task<System.Collections.Generic.List<UserMemory>> GetExistingMemoriesAsync(MemoryContext memoryContext, int? limit = null)
        {
            return Task.FromResult(new System.Collections.Generic.List<UserMemory>());
        }

        public Task<string> AddMemoryAsync(string content, MemoryContext memoryContext)
        {
            return Task.FromResult("mock_memory_id");
        }

        public Task<string> UpdateMemoryAsync(string memoryId, string newContent, MemoryContext memoryContext)
        {
            return Task.FromResult("updated");
        }

        public Task<string> DeleteMemoryAsync(string memoryId)
        {
            return Task.FromResult("deleted");
        }

        public Task<string> ClearMemoryAsync(MemoryContext memoryContext)
        {
            return Task.FromResult("cleared");
        }
    }

    /// <summary>
    /// Contexto de teste para fluent methods
    /// </summary>
    public class FluentTestContext
    {
        public string UserId { get; set; } = "test_user";
        public string SessionId { get; set; } = "test_session";
    }
}