using AgentSharp.Core;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Core
{
    [TestClass]
    public class AnonymousModeTests
    {
    private MockModel? _mockModel;
    private ConsoleLogger? _logger;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _logger = new ConsoleLogger();
        }

        [TestMethod]
        public async Task Agent_WithAnonymousMode_ShouldGenerateUserIdAndSessionId()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var agent = new Agent<object, string>(_mockModel, "AnonymousAgent", storage: storage)
                .WithAnonymousMode(true);

            // Act
            var result = await agent.ExecuteAsync("Hello");

            // Assert
            Assert.IsNotNull(result.SessionInfo);
            Assert.IsNotNull(result.SessionInfo.UserId);
            Assert.IsNotNull(result.SessionInfo.SessionId);
            Assert.IsTrue(result.SessionInfo.WasGenerated);
            Assert.IsTrue(result.SessionInfo.IsAnonymous);
            Assert.IsTrue(result.SessionInfo.UserId.StartsWith("anonymous_"));
            Assert.AreEqual(36, result.SessionInfo.SessionId.Length); // GUID length
        }

        [TestMethod]
        public async Task Agent_WithAnonymousModeAndContext_ShouldUseProvidedIds()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var context = new TestContextWithIds { UserId = "user123", SessionId = "session456" };

            var agent = new Agent<TestContextWithIds, string>(_mockModel, "AnonymousAgent", storage: storage)
                .WithAnonymousMode(true)
                .WithContext(context);

            // Act
            var result = await agent.ExecuteAsync("Hello");

            // Assert
            Assert.IsNotNull(result.SessionInfo);
            Assert.AreEqual("user123", result.SessionInfo.UserId);
            Assert.AreEqual("session456", result.SessionInfo.SessionId);
            Assert.IsFalse(result.SessionInfo.WasGenerated);
            Assert.IsFalse(result.SessionInfo.IsAnonymous);
        }

        [TestMethod]
        public async Task Agent_WithoutAnonymousMode_ShouldUseDefaultIds()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var agent = new Agent<object, string>(_mockModel, "RegularAgent", storage: storage);

            // Act
            var result = await agent.ExecuteAsync("Hello");

            // Assert
            Assert.IsNotNull(result.SessionInfo);
            Assert.AreEqual("default_user", result.SessionInfo.UserId);
            Assert.IsTrue(result.SessionInfo.SessionId.StartsWith("RegularAgent_session_"));
            Assert.IsFalse(result.SessionInfo.WasGenerated);
            Assert.IsFalse(result.SessionInfo.IsAnonymous);
        }

        [TestMethod]
        public async Task Agent_AnonymousMode_ConsistentIdsAcrossRequests()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var agent = new Agent<object, string>(_mockModel, "ConsistentAgent", storage: storage)
                .WithAnonymousMode(true);

            // Act
            var result1 = await agent.ExecuteAsync("First message");
            var result2 = await agent.ExecuteAsync("Second message");

            // Assert
            Assert.AreEqual(result1.SessionInfo.UserId, result2.SessionInfo.UserId);
            Assert.AreEqual(result1.SessionInfo.SessionId, result2.SessionInfo.SessionId);
            Assert.IsTrue(result1.SessionInfo.WasGenerated);
            Assert.IsTrue(result2.SessionInfo.WasGenerated);
        }

        [TestMethod]
        public async Task Agent_AnonymousMode_MemoryManagerShouldHaveCorrectIds()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var agent = new Agent<object, string>(_mockModel, "MemoryAgent", storage: storage)
                .WithAnonymousMode(true);

            // Act - Execute first to generate IDs
            await agent.ExecuteAsync("Hello");
            var memoryManager = agent.GetMemoryManager();

            // Assert
            Assert.IsNotNull(memoryManager);
            Assert.IsNotNull(memoryManager.UserId);
            Assert.IsNotNull(memoryManager.SessionId);
            Assert.IsTrue(memoryManager.UserId.StartsWith("anonymous_"));
        }

        [TestMethod]
        public async Task Agent_AnonymousMode_WithPartialContext_ShouldGenerateSession()
        {
            // Arrange
            var storage = new InMemoryStorage();
            var context = new TestContextWithIds { UserId = "provided_user" }; // SessionId missing

            var agent = new Agent<TestContextWithIds, string>(_mockModel, "PartialAgent", storage: storage)
                .WithAnonymousMode(true)
                .WithContext(context);

            // Act
            var result = await agent.ExecuteAsync("Hello");

            // Assert
            Assert.IsNotNull(result.SessionInfo);
            Assert.AreEqual("provided_user", result.SessionInfo.UserId);
            Assert.IsNotNull(result.SessionInfo.SessionId);
            Assert.AreEqual(36, result.SessionInfo.SessionId.Length); // Generated GUID
            Assert.IsTrue(result.SessionInfo.WasGenerated); // Session was generated
            Assert.IsFalse(result.SessionInfo.IsAnonymous); // User ID is not anonymous
        }
    }

    // Helper class for tests
    public class TestContextWithIds
    {
    public required string UserId { get; set; }
    public string? SessionId { get; set; }
    }
}
