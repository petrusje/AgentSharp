using System;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Unit tests for TeamExtensions functionality
    /// Tests the core team orchestration methods and team modes
    /// </summary>
    [TestClass]
    public class TeamExtensionsTests
    {
        private IModel? _mockModel;
        private IAgent[]? _testAgents;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();

            // Create test agents
            _testAgents = new IAgent[]
            {
                new Agent<string, string>(_mockModel, "Agent1")
                    .WithPersona("Test agent 1 for coordination"),
                new Agent<string, string>(_mockModel, "Agent2")
                    .WithPersona("Test agent 2 for backend tasks"),
                new Agent<string, string>(_mockModel, "Agent3")
                    .WithPersona("Test agent 3 for frontend tasks")
            };
        }

        #region AsTeam Method Tests

        [TestMethod]
        public void AsTeam_WithValidAgentsAndCoordinateMode_ShouldReturnWorkflow()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("TestWorkflow");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Coordinate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AsTeam_WithValidAgentsAndRouteMode_ShouldReturnWorkflow()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("TestWorkflow");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Route);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AsTeam_WithValidAgentsAndCollaborateMode_ShouldReturnWorkflow()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("TestWorkflow");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Collaborate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AsTeam_WithNullWorkflow_ShouldThrowArgumentNullException()
        {
            // Arrange
            AdvancedWorkflow<string, string>? workflow = null;

            // Act & Assert
            workflow!.AsTeam(_testAgents!, TeamMode.Coordinate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AsTeam_WithNullAgents_ShouldThrowArgumentException()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("TestWorkflow");
            IAgent[]? nullAgents = null;

            // Act & Assert
            workflow.AsTeam(nullAgents!, TeamMode.Coordinate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AsTeam_WithEmptyAgents_ShouldThrowArgumentException()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("TestWorkflow");
            var emptyAgents = new IAgent[0];

            // Act & Assert
            workflow.AsTeam(emptyAgents, TeamMode.Coordinate);
        }

        #endregion

        #region Team Mode Integration Tests

        [TestMethod]
        public async Task TeamWorkflow_CoordinateMode_ShouldExecuteSuccessfully()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("CoordinateTest")
                .AsTeam(_testAgents!, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            var testInput = "Test coordinate task";

            // Act
            var result = await workflow.ExecuteAsync(testInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public async Task TeamWorkflow_RouteMode_ShouldExecuteSuccessfully()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("RouteTest")
                .AsTeam(_testAgents!, TeamMode.Route)
                .WithMemory(new BasicMemory());

            var testInput = "Test route task";

            // Act
            var result = await workflow.ExecuteAsync(testInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public async Task TeamWorkflow_CollaborateMode_ShouldExecuteSuccessfully()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("CollaborateTest")
                .AsTeam(_testAgents!, TeamMode.Collaborate)
                .WithMemory(new BasicMemory());

            var testInput = "Test collaborate task";

            // Act
            var result = await workflow.ExecuteAsync(testInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Team Configuration Tests

        [TestMethod]
        public void AsTeam_WithCoordinateMode_ShouldConfigureCorrectly()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("ModeTest");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Coordinate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AsTeam_WithRouteMode_ShouldConfigureCorrectly()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("ModeTest");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Route);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AsTeam_WithCollaborateMode_ShouldConfigureCorrectly()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("ModeTest");

            // Act
            var result = workflow.AsTeam(_testAgents!, TeamMode.Collaborate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AsTeam_WithSingleAgent_ShouldWork()
        {
            // Arrange
            var workflow = new AdvancedWorkflow<string, string>("SingleAgentTest");
            var singleAgent = new IAgent[] { _testAgents![0] };

            // Act
            var result = workflow.AsTeam(singleAgent, TeamMode.Coordinate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        #endregion
    }
}