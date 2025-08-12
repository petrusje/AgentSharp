using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Memory
{
    [TestClass]
    public class SemanticMemoryStorageTests
    {
    private SemanticSqliteStorage? _storage;

        [TestInitialize]
        public void Setup()
        {
            // Create an embedding service for testing
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
            
            // Use in-memory SQLite database for testing
            _storage = new SemanticSqliteStorage("Data Source=:memory:", embeddingService, 1536);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _storage = null;
        }

        [TestMethod]
        public void IsConnected_ShouldReturnTrue()
        {
            // Assert
            Assert.IsTrue(_storage!.IsConnected);
            Assert.IsTrue(_storage.IsConnected);
        }

        [TestMethod]
        public async Task Sessions_CreateAndRetrieve_ShouldWork()
        {
            // Arrange
            var session = new AgentSession
            {
                Id = "test_session",
                UserId = "test_user",
                Title = "Test Session",
                Description = "A test session",
                IsActive = true
            };
            // Act
            var sessionId = await _storage!.Sessions.CreateSessionAsync(session);
            var retrievedSession = await _storage!.Sessions.GetSessionAsync(sessionId);

            // Assert
            Assert.AreEqual(session.Id, sessionId);
            Assert.IsNotNull(retrievedSession);
            Assert.AreEqual(session.Id, retrievedSession.Id);
            Assert.AreEqual(session.UserId, retrievedSession.UserId);
            Assert.AreEqual(session.Title, retrievedSession.Title);
        }

        [TestMethod]
        public async Task Sessions_GetUserSessions_ShouldReturnCorrectSessions()
        {
            // Arrange
            var userId = "test_user";
            var session1 = new AgentSession { Id = "session1", UserId = userId };
            var session2 = new AgentSession { Id = "session2", UserId = userId };
            var session3 = new AgentSession { Id = "session3", UserId = "other_user" };

            await _storage!.Sessions.CreateSessionAsync(session1);
            await _storage!.Sessions.CreateSessionAsync(session2);
            await _storage!.Sessions.CreateSessionAsync(session3);

            // Act
            var userSessions = await _storage!.Sessions.GetUserSessionsAsync(userId);

            // Assert
            Assert.AreEqual(2, userSessions.Count);
        }

        [TestMethod]
        public async Task Memories_CreateAndRetrieve_ShouldWork()
        {
            // Arrange
            var memory = new UserMemory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test_user",
                Content = "Test memory content",
                Type = AgentMemoryType.Fact,
                IsActive = true,
                RelevanceScore = 0.8
            };
            // Act
            var memoryId = await _storage!.Memories.AddMemoryAsync(memory);
            var retrievedMemory = await _storage!.Memories.GetMemoryAsync(memoryId);

            // Assert
            Assert.AreEqual(memory.Id, memoryId);
            Assert.IsNotNull(retrievedMemory);
            Assert.AreEqual(memory.Content, retrievedMemory.Content);
            Assert.AreEqual(memory.Type, retrievedMemory.Type);
        }

        [TestMethod]
        public async Task Memories_SearchMemories_ShouldReturnRelevantResults()
        {
            // Arrange
            var userId = "test_user";
            var memory1 = new UserMemory { UserId = userId, Content = "Usuário gosta de café forte", IsActive = true };
            var memory2 = new UserMemory { UserId = userId, Content = "Prefere trabalhar pela manhã", IsActive = true };
            // (Removido duplicidade: já está acima)

            await _storage!.Memories.AddMemoryAsync(memory1);
            await _storage!.Memories.AddMemoryAsync(memory2);

            // Act
            var context = new MemoryContext { UserId = userId };
            var searchResults = await _storage!.SearchMemoriesAsync("café", context);

            // Assert
            Assert.AreEqual(1, searchResults.Count);
            Assert.IsTrue(searchResults[0].Content.Contains("café"));
    }
        [TestMethod]
        public async Task GetOrCreateSession_ShouldCreateNewSession()
        {
            // Arrange
            string sessionId = "new_session";
            string userId = "test_user";

            // Act
            var session = await _storage!.GetOrCreateSessionAsync(sessionId, userId);

            // Assert
            Assert.IsNotNull(session);
            Assert.AreEqual(sessionId, session.Id);
            Assert.AreEqual(userId, session.UserId);
        }

        [TestMethod]
        public async Task GetOrCreateSession_ShouldReturnExistingSession()
        {
            // Arrange
            string sessionId = "existing_session";
            string userId = "test_user";

            var originalSession = new AgentSession { Id = sessionId, UserId = userId, Title = "Original" };
            await _storage!.Sessions.CreateSessionAsync(originalSession);

            // Act
            var session = await _storage!.GetOrCreateSessionAsync(sessionId, userId);

            // Assert
            Assert.IsNotNull(session);
            Assert.AreEqual(sessionId, session.Id);
            Assert.AreEqual("Original", ((AgentSession)session).Title);
    }

        [TestMethod]
        public async Task ClearAllAsync_ShouldRemoveAllData()
        {
            // Arrange
            var session = new AgentSession { Id = "test_session", UserId = "test_user" };
            var memory = new UserMemory { UserId = "test_user", Content = "Test memory", IsActive = true };

            await _storage!.Sessions.CreateSessionAsync(session);
            await _storage!.Memories.AddMemoryAsync(memory);

            // Act
            await _storage!.ClearAllAsync();

            // Assert
            var retrievedSession = await _storage!.Sessions.GetSessionAsync("test_session");
            var retrievedMemory = await _storage!.Memories.GetMemoryAsync(memory.Id);

            Assert.IsNull(retrievedSession);
            Assert.IsNull(retrievedMemory);
        }
        [TestMethod]
        public async Task Embeddings_ShouldWorkBasically()
        {
            // Arrange
            var content = "Test content";
            var embedding = new System.Collections.Generic.List<float> { 0.1f, 0.2f, 0.3f };

            // Act
            var id = await _storage!.Embeddings.StoreEmbeddingAsync(content, embedding);
            var searchResults = await _storage!.Embeddings.SearchSimilarAsync(embedding, 10, 0.5);

            // Assert
            Assert.IsNotNull(id);
            Assert.IsTrue(searchResults.Count <= 10);
        }

        [TestMethod]
        public async Task Embeddings_SearchSimilar_ShouldReturnRelevantResults()
        {
            // Arrange
            var content1 = "Coffee preferences";
            var content2 = "Work schedule";
            var embedding1 = new System.Collections.Generic.List<float> { 0.8f, 0.1f, 0.1f };
            var embedding2 = new System.Collections.Generic.List<float> { 0.1f, 0.8f, 0.1f };
            var queryEmbedding = new System.Collections.Generic.List<float> { 0.9f, 0.05f, 0.05f };

            await _storage!.Embeddings.StoreEmbeddingAsync(content1, embedding1);
            await _storage!.Embeddings.StoreEmbeddingAsync(content2, embedding2);

            // Act
            var results = await _storage!.Embeddings.SearchSimilarAsync(queryEmbedding, 5, 0.5);

            // Assert
            Assert.IsTrue(results.Count > 0);
            // O primeiro resultado deve ser o mais similar (content1)
            if (results.Count > 0)
            {
                Assert.IsTrue(results[0].similarity > 0.5);
            }
        }
    }
}
