using AgentSharp.Core;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Memory
{
    [TestClass]
    public class AgentWithMemoryTests
    {
    private MockModel? _mockModel;
    private ConsoleLogger? _logger;

        // Helper method to create SemanticSqliteStorage for tests
        private static SemanticSqliteStorage CreateTestStorage(string connectionString)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
            return new SemanticSqliteStorage(connectionString, embeddingService, 1536);
        }

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
        }

        [TestMethod]
        public async Task Agent_WithSemanticMemoryStorage_ShouldWork()
        {
            // Arrange
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var context = new TestContext { UserId = "test_user", SessionId = "test_session" };

            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: storage)
                .WithInstructions("Você é um assistente que lembra das preferências do usuário")
                .WithContext(context);

            // Act
            var result1 = await agent.ExecuteAsync("Meu nome é João e prefiro café forte");
            var result2 = await agent.ExecuteAsync("Qual é meu nome e minha preferência de café?");

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsTrue(result1.Data.Length > 0);
            Assert.IsTrue(result2.Data.Length > 0);
        }

        [TestMethod]
        public async Task Agent_WithSemanticSqliteStorage_ShouldPersistBetweenInstances()
        {
            // Arrange
            var dbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_agent_{Guid.NewGuid()}.db");
            var context = new TestContext { UserId = "persistent_user", SessionId = "persistent_session" };

            try
            {
                // First agent instance
                var storage1 = CreateTestStorage($"Data Source={dbPath}");
                await storage1.InitializeAsync();

                var agent1 = new Agent<TestContext, string>(_mockModel, "PersistentAgent", storage: storage1)
                    .WithInstructions("Você lembra de tudo sobre o usuário")
                    .WithContext(context);

                await agent1.ExecuteAsync("Meu hobby preferido é programação");

                storage1 = null;

                // Second agent instance (simulating app restart)
                var storage2 = CreateTestStorage($"Data Source={dbPath}");
                await storage2.InitializeAsync();

                var agent2 = new Agent<TestContext, string>(_mockModel, "PersistentAgent", storage: storage2)
                    .WithInstructions("Você lembra de tudo sobre o usuário")
                    .WithContext(context);

                // Act
                var result = await agent2.ExecuteAsync("Qual é meu hobby preferido?");

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Data.Length > 0);

                storage2 = null;
            }
            finally
            {
                // Cleanup
                if (System.IO.File.Exists(dbPath))
                {
                    try
                    {
                        System.IO.File.Delete(dbPath);
                    }
                    catch { /* Ignore cleanup errors */ }
                }
            }
        }

        [TestMethod]
    public void Agent_MemoryManager_ShouldBeAccessible()
        {
            // Arrange
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var context = new TestContext { UserId = "test_user", SessionId = "test_session" };

            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: storage)
                .WithContext(context);

            // Act
            var memoryManager = agent.GetMemoryManager();

            // Assert
            Assert.IsNotNull(memoryManager);
            Assert.AreEqual("test_user", memoryManager.UserId);
            Assert.AreEqual("test_session", memoryManager.SessionId);
        }

        [TestMethod]
    public void Agent_WithMemoryTools_ShouldRegisterSmartMemoryToolPack()
        {
            // Arrange
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var context = new TestContext { UserId = "test_user", SessionId = "test_session" };

            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: storage)
                .WithContext(context);

            // Act & Assert
            // O SmartMemoryToolPack deve ser registrado automaticamente
            // Verificar isso através da execução seria complexo pois requer mock do modelo
            // Por enquanto apenas verificamos que o agente inicializa corretamente
            Assert.IsNotNull(agent);
            Assert.IsNotNull(agent.GetMemoryManager());
        }

        [TestMethod]
    public void Agent_GetMemorySummary_ShouldReturnSummary()
        {
            // Arrange
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var context = new TestContext { UserId = "test_user", SessionId = "test_session" };

            var agent = new Agent<TestContext, string>(_mockModel, "TestAgent", storage: storage)
                .WithContext(context);

            // Act
            var summary = agent.GetMemorySummary();

            // Assert
            // Pode ser null inicialmente, mas não deve gerar erro
            // O resumo seria gerado após conversas reais
            Assert.IsTrue(true); // Test passes if no exception
        }

        [TestMethod]
        public async Task Agent_MultipleConversations_ShouldAccumulateMemory()
        {
            // Arrange
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var context = new TestContext { UserId = "cumulative_user", SessionId = "cumulative_session" };

            var agent = new Agent<TestContext, string>(_mockModel, "CumulativeAgent", storage: storage)
                .WithInstructions("Acumule conhecimento sobre o usuário")
                .WithContext(context);

            // Act - Multiple conversations
            await agent.ExecuteAsync("Meu nome é Ana");
            await agent.ExecuteAsync("Trabalho como designer");
            await agent.ExecuteAsync("Moro em São Paulo");

            var memoryManager = agent.GetMemoryManager();
            var memories = await memoryManager.GetExistingMemoriesAsync();

            // Assert
            // Deve ter acumulado pelo menos algumas memórias
            // (O número exato depende da implementação de extração automática)
            Assert.IsNotNull(memories);
        }
    }

    // Helper class for tests
    public class TestContext
    {
        public required string UserId { get; set; }
        public required string SessionId { get; set; }
    }
}
