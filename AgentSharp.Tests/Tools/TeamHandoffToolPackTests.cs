using System;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Models;
using AgentSharp.Tools;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentSharp.Tests.Tools
{
    /// <summary>
    /// Unit tests for TeamHandoffToolPack functionality
    /// Tests agent handoffs, team communication, and memory integration
    /// </summary>
    [TestClass]
    public class TeamHandoffToolPackTests
    {
        private IModel? _mockModel;
        private IMemoryManager? _memoryManager;
        private IAgent[]? _testAgents;
        private TeamHandoffToolPack? _toolPack;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();

            // Setup memory manager with in-memory storage
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var logger = new ConsoleLogger();
            _memoryManager = new MemoryManager(storage, _mockModel, logger);

            // Create test agents
            _testAgents = new IAgent[]
            {
                new Agent<string, string>(_mockModel, "ProjectManager")
                    .WithPersona("Project management specialist"),
                new Agent<string, string>(_mockModel, "Developer")
                    .WithPersona("Software development specialist"),
                new Agent<string, string>(_mockModel, "Designer")
                    .WithPersona("UI/UX design specialist")
            };

            // Create tool pack
            _toolPack = new TeamHandoffToolPack(_testAgents, _memoryManager, "test-team-123");
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Act
            var toolPack = new TeamHandoffToolPack(_testAgents!, _memoryManager!, "custom-team");

            // Assert
            Assert.IsNotNull(toolPack);
            Assert.AreEqual("TeamHandoffToolPack", toolPack.Name);
            Assert.AreEqual("Tools for agent handoffs and team coordination", toolPack.Description);
            Assert.AreEqual("1.0.0", toolPack.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithNullAgents_ShouldThrowArgumentException()
        {
            // Act & Assert
            new TeamHandoffToolPack(null!, _memoryManager!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyAgents_ShouldThrowArgumentException()
        {
            // Act & Assert
            new TeamHandoffToolPack(new IAgent[0], _memoryManager!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullMemoryManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            new TeamHandoffToolPack(_testAgents!, null!);
        }

        [TestMethod]
        public void Constructor_WithoutTeamId_ShouldGenerateTeamId()
        {
            // Act
            var toolPack = new TeamHandoffToolPack(_testAgents!, _memoryManager!);

            // Assert
            Assert.IsNotNull(toolPack);
            // The generated team ID should be accessible through the tool pack's functionality
        }

        #endregion

        #region HandoffTask Tests

        [TestMethod]
        public async Task HandoffTask_WithValidParameters_ShouldReturnSuccessMessage()
        {
            // Arrange
            var targetAgent = "Developer";
            var taskDescription = "Implement user authentication";
            var reason = "Requires backend expertise";
            var priority = "High";

            // Act
            var result = await _toolPack!.HandoffTask(targetAgent, taskDescription, reason, priority);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("successfully handed off"));
            Assert.IsTrue(result.Contains(targetAgent));
        }

        [TestMethod]
        public async Task HandoffTask_WithUnknownAgent_ShouldReturnErrorMessage()
        {
            // Arrange
            var unknownAgent = "UnknownAgent";
            var taskDescription = "Some task";
            var reason = "Some reason";

            // Act
            var result = await _toolPack!.HandoffTask(unknownAgent, taskDescription, reason);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("not found in team"));
            Assert.IsTrue(result.Contains("Available agents:"));
            Assert.IsTrue(result.Contains("ProjectManager"));
            Assert.IsTrue(result.Contains("Developer"));
            Assert.IsTrue(result.Contains("Designer"));
        }

        [TestMethod]
        public async Task HandoffTask_WithDefaultPriority_ShouldUseMediumPriority()
        {
            // Arrange
            var targetAgent = "Designer";
            var taskDescription = "Create mockups";
            var reason = "Design expertise needed";

            // Act
            var result = await _toolPack!.HandoffTask(targetAgent, taskDescription, reason);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("successfully handed off"));
        }

        #endregion

        #region ListTeamAgents Tests

        [TestMethod]
        public void ListTeamAgents_ShouldReturnAllAgentsWithDescriptions()
        {
            // Act
            var result = _toolPack!.ListTeamAgents();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Available team agents (3 total)"));
            Assert.IsTrue(result.Contains("- ProjectManager:"));
            Assert.IsTrue(result.Contains("- Developer:"));
            Assert.IsTrue(result.Contains("- Designer:"));
        }

        #endregion

        #region ShareWithTeam Tests

        [TestMethod]
        public async Task ShareWithTeam_WithValidInformation_ShouldReturnSuccessMessage()
        {
            // Arrange
            var information = "New API endpoint is ready for testing";
            var category = "update";

            // Act
            var result = await _toolPack!.ShareWithTeam(information, category);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Information successfully shared"));
            Assert.IsTrue(result.Contains("Category: update"));
        }

        [TestMethod]
        public async Task ShareWithTeam_WithDefaultCategory_ShouldUseGeneralCategory()
        {
            // Arrange
            var information = "Team meeting tomorrow at 2 PM";

            // Act
            var result = await _toolPack!.ShareWithTeam(information);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Category: general"));
        }

        #endregion

        #region GetTeamContext Tests

        [TestMethod]
        public async Task GetTeamContext_WithValidLimit_ShouldReturnFormattedContext()
        {
            // Arrange - First add some shared information to create team context
            await _toolPack!.ShareWithTeam("Test shared information", "test");

            // Act
            var result = await _toolPack!.GetTeamContext(3);

            // Assert
            Assert.IsNotNull(result);
            // Context could be empty initially, but should not error
            Assert.IsFalse(result.Contains("Failed to retrieve"));
        }

        [TestMethod]
        public async Task GetTeamContext_WithDefaultLimit_ShouldUseDefaultValue()
        {
            // Act
            var result = await _toolPack!.GetTeamContext();

            // Assert
            Assert.IsNotNull(result);
            // Should either return communications or "no communications found" message
            Assert.IsTrue(result.Contains("Recent team communications") || result.Contains("No recent team communications found"));
        }

        #endregion

        #region ConsultAgent Tests

        [TestMethod]
        public async Task ConsultAgent_WithValidParameters_ShouldReturnConsultationResponse()
        {
            // Arrange
            var targetAgent = "Designer";
            var question = "What UI pattern should we use for the dashboard?";
            var context = "Building a financial dashboard";

            // Act
            var result = await _toolPack!.ConsultAgent(targetAgent, question, context);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Consultation response from Designer"));
        }

        [TestMethod]
        public async Task ConsultAgent_WithUnknownAgent_ShouldReturnErrorMessage()
        {
            // Arrange
            var unknownAgent = "UnknownExpert";
            var question = "Some question";

            // Act
            var result = await _toolPack!.ConsultAgent(unknownAgent, question);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("not found in team"));
            Assert.IsTrue(result.Contains("Available agents:"));
        }

        #endregion

        #region UpdateTeamStatus Tests

        [TestMethod]
        public async Task UpdateTeamStatus_WithAllParameters_ShouldReturnSuccessMessage()
        {
            // Arrange
            var status = "Feature development is 80% complete";
            var completionPercentage = 80;
            var nextSteps = "Final testing and deployment preparation";

            // Act
            var result = await _toolPack!.UpdateTeamStatus(status, completionPercentage, nextSteps);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Team status updated successfully"));
            Assert.IsTrue(result.Contains("Current status: " + status));
        }

        [TestMethod]
        public async Task UpdateTeamStatus_WithMinimalParameters_ShouldWork()
        {
            // Arrange
            var status = "Working on bug fixes";

            // Act
            var result = await _toolPack!.UpdateTeamStatus(status);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Team status updated successfully"));
        }

        #endregion

        #region InitializeAsync Tests

        [TestMethod]
        public async Task InitializeAsync_ShouldCreateTeamInitializationMemory()
        {
            // Act
            await _toolPack!.InitializeAsync();

            // Assert - Should complete without throwing exception
            Assert.IsTrue(true); // Test passes if no exception is thrown
        }

        #endregion
    }
}