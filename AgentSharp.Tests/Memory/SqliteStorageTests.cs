using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Memory
{
    [TestClass]
    public class SqliteStorageTests
    {
        private SqliteStorage _storage;
        private string _testDbPath;

        [TestInitialize]
        public async Task Setup()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _storage = new SqliteStorage($"Data Source={_testDbPath}");
            await _storage.InitializeAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _storage = null;
            
            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [TestMethod]
        public async Task InitializeAsync_ShouldCreateDatabase()
        {
            // Assert
            Assert.IsTrue(File.Exists(_testDbPath));
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Act
            var sessionId = await _storage.Sessions.CreateSessionAsync(session);
            var retrievedSession = await _storage.Sessions.GetSessionAsync(sessionId);

            // Assert
            Assert.AreEqual(session.Id, sessionId);
            Assert.IsNotNull(retrievedSession);
            Assert.AreEqual(session.Id, retrievedSession.Id);
            Assert.AreEqual(session.UserId, retrievedSession.UserId);
            Assert.AreEqual(session.Title, retrievedSession.Title);
            Assert.AreEqual(session.Description, retrievedSession.Description);
            Assert.AreEqual(session.IsActive, retrievedSession.IsActive);
        }

        [TestMethod]
        public async Task Sessions_GetUserSessions_ShouldReturnCorrectSessions()
        {
            // Arrange
            var userId = "test_user";
            var session1 = new AgentSession { Id = "session1", UserId = userId, IsActive = true };
            var session2 = new AgentSession { Id = "session2", UserId = userId, IsActive = true };
            var session3 = new AgentSession { Id = "session3", UserId = "other_user", IsActive = true };

            await _storage.Sessions.CreateSessionAsync(session1);
            await _storage.Sessions.CreateSessionAsync(session2);
            await _storage.Sessions.CreateSessionAsync(session3);

            // Act
            var userSessions = await _storage.Sessions.GetUserSessionsAsync(userId);

            // Assert
            Assert.AreEqual(2, userSessions.Count);
            Assert.IsTrue(userSessions.Exists(s => s.Id == "session1"));
            Assert.IsTrue(userSessions.Exists(s => s.Id == "session2"));
            Assert.IsFalse(userSessions.Exists(s => s.Id == "session3"));
        }

        [TestMethod]
        public async Task Sessions_UpdateSession_ShouldModifyData()
        {
            // Arrange
            var session = new AgentSession
            {
                Id = "test_session",
                UserId = "test_user",
                Title = "Original Title",
                IsActive = true
            };

            await _storage.Sessions.CreateSessionAsync(session);

            // Act
            session.Title = "Updated Title";
            session.IsActive = false;
            await _storage.Sessions.UpdateSessionAsync(session);

            var updatedSession = await _storage.Sessions.GetSessionAsync(session.Id);

            // Assert
            Assert.AreEqual("Updated Title", updatedSession.Title);
            Assert.IsFalse(updatedSession.IsActive);
        }

        [TestMethod]
        public async Task Sessions_DeleteSession_ShouldSoftDelete()
        {
            // Arrange
            var session = new AgentSession
            {
                Id = "test_session",
                UserId = "test_user",
                IsActive = true
            };

            await _storage.Sessions.CreateSessionAsync(session);

            // Act
            await _storage.Sessions.DeleteSessionAsync(session.Id);
            var deletedSession = await _storage.Sessions.GetSessionAsync(session.Id);

            // Assert
            Assert.IsNotNull(deletedSession);
            Assert.IsFalse(deletedSession.IsActive); // Soft delete
        }

        [TestMethod]
        public async Task Memories_CreateAndRetrieve_ShouldWork()
        {
            // Arrange
            var memory = new UserMemory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test_user",
                SessionId = "test_session",
                Content = "Test memory content",
                Type = AgentMemoryType.Fact,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                RelevanceScore = 0.8
            };

            // Act
            var memoryId = await _storage.Memories.AddMemoryAsync(memory);
            var retrievedMemory = await _storage.Memories.GetMemoryAsync(memoryId);

            // Assert
            Assert.AreEqual(memory.Id, memoryId);
            Assert.IsNotNull(retrievedMemory);
            Assert.AreEqual(memory.Content, retrievedMemory.Content);
            Assert.AreEqual(memory.Type, retrievedMemory.Type);
            Assert.AreEqual(memory.RelevanceScore, retrievedMemory.RelevanceScore, 0.01);
        }

        [TestMethod]
        public async Task Memories_GetMemoriesByUser_ShouldReturnCorrectMemories()
        {
            // Arrange
            var userId = "test_user";
            var memory1 = new UserMemory { UserId = userId, Content = "Memory 1", IsActive = true };
            var memory2 = new UserMemory { UserId = userId, Content = "Memory 2", IsActive = true };
            var memory3 = new UserMemory { UserId = "other_user", Content = "Memory 3", IsActive = true };

            await _storage.Memories.AddMemoryAsync(memory1);
            await _storage.Memories.AddMemoryAsync(memory2);
            await _storage.Memories.AddMemoryAsync(memory3);

            // Act
            var userMemories = await _storage.Memories.GetMemoriesAsync(userId);

            // Assert
            Assert.AreEqual(2, userMemories.Count);
            Assert.IsTrue(userMemories.Exists(m => m.Content == "Memory 1"));
            Assert.IsTrue(userMemories.Exists(m => m.Content == "Memory 2"));
        }

        [TestMethod]
        public async Task Memories_SearchMemories_ShouldReturnRelevantResults()
        {
            // Arrange
            var userId = "test_user";
            var memory1 = new UserMemory { UserId = userId, Content = "Usuário gosta de café forte", IsActive = true };
            var memory2 = new UserMemory { UserId = userId, Content = "Prefere trabalhar pela manhã", IsActive = true };
            var memory3 = new UserMemory { UserId = userId, Content = "Tem dois filhos", IsActive = true };

            await _storage.Memories.AddMemoryAsync(memory1);
            await _storage.Memories.AddMemoryAsync(memory2);
            await _storage.Memories.AddMemoryAsync(memory3);

            // Act
            var searchResults = await _storage.Memories.SearchMemoriesAsync("café", userId);

            // Assert
            Assert.AreEqual(1, searchResults.Count);
            Assert.IsTrue(searchResults[0].Content.Contains("café"));
        }

        [TestMethod]
        public async Task Memories_UpdateMemory_ShouldModifyContent()
        {
            // Arrange
            var memory = new UserMemory
            {
                UserId = "test_user",
                Content = "Original content",
                IsActive = true
            };

            await _storage.Memories.AddMemoryAsync(memory);

            // Act
            memory.Content = "Updated content";
            await _storage.Memories.UpdateMemoryAsync(memory);

            var updatedMemory = await _storage.Memories.GetMemoryAsync(memory.Id);

            // Assert
            Assert.AreEqual("Updated content", updatedMemory.Content);
        }

        [TestMethod]
        public async Task Memories_DeleteMemory_ShouldSoftDelete()
        {
            // Arrange
            var memory = new UserMemory
            {
                UserId = "test_user",
                Content = "Memory to delete",
                IsActive = true
            };

            await _storage.Memories.AddMemoryAsync(memory);

            // Act
            await _storage.Memories.DeleteMemoryAsync(memory.Id);
            var deletedMemory = await _storage.Memories.GetMemoryAsync(memory.Id);

            // Assert
            Assert.IsNotNull(deletedMemory);
            Assert.IsFalse(deletedMemory.IsActive); // Soft delete
        }

        [TestMethod]
        public async Task GetOrCreateSession_ShouldCreateIfNotExists()
        {
            // Arrange
            string sessionId = "new_session";
            string userId = "test_user";

            // Act
            var session1 = await _storage.GetOrCreateSessionAsync(sessionId, userId);
            var session2 = await _storage.GetOrCreateSessionAsync(sessionId, userId);

            // Assert
            Assert.IsNotNull(session1);
            Assert.IsNotNull(session2);
            Assert.AreEqual(sessionId, session1.Id);
            Assert.AreEqual(sessionId, session2.Id);
            Assert.AreEqual(userId, session1.UserId);
        }

        [TestMethod]
        public async Task ClearAllAsync_ShouldRemoveAllData()
        {
            // Arrange
            var session = new AgentSession { Id = "test_session", UserId = "test_user" };
            var memory = new UserMemory { UserId = "test_user", Content = "Test memory" };

            await _storage.Sessions.CreateSessionAsync(session);
            await _storage.Memories.AddMemoryAsync(memory);

            // Act
            await _storage.ClearAllAsync();

            // Assert
            var sessions = await _storage.Sessions.GetUserSessionsAsync("test_user");
            var memories = await _storage.Memories.GetMemoriesAsync("test_user");

            Assert.AreEqual(0, sessions.Count);
            Assert.AreEqual(0, memories.Count);
        }
    }
}