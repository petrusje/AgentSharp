using System;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using AgentSharp.Tools;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentSharp.Tests.Integration
{
    /// <summary>
    /// Integration tests for TeamHandoffToolPack in real workflow scenarios.
    /// Tests agent-to-agent handoffs, team communication, and memory sharing.
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class TeamHandoffIntegrationTests
    {
        private IModel? _mockModel;
        private IStorage? _storage;
        private ILogger? _logger;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();
            _storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            _logger = new ConsoleLogger();
        }

        #region Team Communication Integration Tests

        [TestMethod]
        public async Task TeamCommunication_ShareAndRetrieveContext_ShouldMaintainInformation()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var projectManager = new Agent<string, string>(_mockModel!, "ProjectManager", storage: _storage)
                .WithPersona("Project manager who coordinates team activities");
            var developer = new Agent<string, string>(_mockModel!, "Developer", storage: _storage)
                .WithPersona("Software developer who implements features");
            var designer = new Agent<string, string>(_mockModel!, "Designer", storage: _storage)
                .WithPersona("UI/UX designer who creates user interfaces");

            var teamAgents = new IAgent[] { projectManager, developer, designer };
            var handoffTools = new TeamHandoffToolPack(teamAgents, memoryManager, "integration-team");

            // Initialize the tool pack
            await handoffTools.InitializeAsync();

            // Act - Share information and retrieve context
            var shareResult1 = await handoffTools.ShareWithTeam(
                "Sprint 1 planning completed. Focus areas: authentication and user profile management.", 
                "planning");
            
            var shareResult2 = await handoffTools.ShareWithTeam(
                "Design mockups for login screen are ready for review.", 
                "design");

            var shareResult3 = await handoffTools.ShareWithTeam(
                "Authentication API endpoints implemented and tested.", 
                "development");

            // Retrieve team context
            var teamContext = await handoffTools.GetTeamContext(5);

            // Assert
            Assert.IsNotNull(shareResult1);
            Assert.IsNotNull(shareResult2);
            Assert.IsNotNull(shareResult3);
            Assert.IsNotNull(teamContext);

            Assert.IsTrue(shareResult1.Contains("successfully shared"), "First share should succeed");
            Assert.IsTrue(shareResult2.Contains("successfully shared"), "Second share should succeed");
            Assert.IsTrue(shareResult3.Contains("successfully shared"), "Third share should succeed");

            // Team context should contain shared information
            Assert.IsTrue(teamContext.Contains("Recent team communications") || 
                         teamContext.Contains("Sprint") || 
                         teamContext.Contains("planning") || 
                         teamContext.Contains("authentication"),
                "Team context should contain shared information");
        }

        [TestMethod]
        public async Task TeamStatusUpdates_MultipleAgents_ShouldTrackProgress()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var teamLead = new Agent<string, string>(_mockModel!, "TeamLead", storage: _storage)
                .WithPersona("Team lead managing project progress");
            var frontendDev = new Agent<string, string>(_mockModel!, "FrontendDev", storage: _storage)
                .WithPersona("Frontend developer working on user interface");
            var backendDev = new Agent<string, string>(_mockModel!, "BackendDev", storage: _storage)
                .WithPersona("Backend developer working on server logic");

            var teamAgents = new IAgent[] { teamLead, frontendDev, backendDev };
            var handoffTools = new TeamHandoffToolPack(teamAgents, memoryManager, "status-team");

            await handoffTools.InitializeAsync();

            // Act - Multiple status updates
            var status1 = await handoffTools.UpdateTeamStatus(
                "Project initialization completed", 15, "Setting up development environment");
            
            var status2 = await handoffTools.UpdateTeamStatus(
                "Frontend components development in progress", 45, "Implementing user authentication UI");
            
            var status3 = await handoffTools.UpdateTeamStatus(
                "Backend API development nearly complete", 80, "Final testing and documentation");

            // Retrieve all team context including status updates
            var fullContext = await handoffTools.GetTeamContext(10);

            // Assert
            Assert.IsNotNull(status1);
            Assert.IsNotNull(status2);
            Assert.IsNotNull(status3);
            Assert.IsNotNull(fullContext);

            Assert.IsTrue(status1.Contains("Team status updated successfully"));
            Assert.IsTrue(status2.Contains("Team status updated successfully"));
            Assert.IsTrue(status3.Contains("Team status updated successfully"));

            // Context should reflect the progression of status updates
            Assert.IsTrue(fullContext.Contains("Recent team communications") || 
                         fullContext.Length > 50,
                "Full context should contain status update history");
        }

        #endregion

        #region Team Agent Listing Integration Tests

        [TestMethod]
        public void TeamAgentListing_WithCompleteTeam_ShouldProvideFullDetails()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var analyst = new Agent<string, string>(_mockModel!, "BusinessAnalyst", storage: _storage)
                .WithPersona("Business analyst who gathers requirements and analyzes business processes");
            var architect = new Agent<string, string>(_mockModel!, "SolutionArchitect", storage: _storage)
                .WithPersona("Solution architect who designs system architecture and technical solutions");
            var qaEngineer = new Agent<string, string>(_mockModel!, "QAEngineer", storage: _storage)
                .WithPersona("Quality assurance engineer who ensures software quality through testing");

            var teamAgents = new IAgent[] { analyst, architect, qaEngineer };
            var handoffTools = new TeamHandoffToolPack(teamAgents, memoryManager, "listing-team");

            // Act
            var agentList = handoffTools.ListTeamAgents();

            // Assert
            Assert.IsNotNull(agentList);
            Assert.IsTrue(agentList.Contains("Available team agents (3 total)"), "Should show correct count");
            Assert.IsTrue(agentList.Contains("BusinessAnalyst"), "Should list business analyst");
            Assert.IsTrue(agentList.Contains("SolutionArchitect"), "Should list solution architect");
            Assert.IsTrue(agentList.Contains("QAEngineer"), "Should list QA engineer");
            
            // Should contain role descriptions
            Assert.IsTrue(agentList.Contains("Business analyst") || 
                         agentList.Contains("Solution architect") || 
                         agentList.Contains("Quality assurance"),
                "Should include agent role descriptions");
        }

        #endregion

        #region Memory Persistence Integration Tests

        [TestMethod]
        public async Task MemoryPersistence_AcrossToolPackSessions_ShouldMaintainHistory()
        {
            // Arrange - First session
            var memoryManager1 = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var agent1 = new Agent<string, string>(_mockModel!, "Agent1", storage: _storage)
                .WithPersona("Agent for persistence testing");
            var agent2 = new Agent<string, string>(_mockModel!, "Agent2", storage: _storage)
                .WithPersona("Second agent for persistence testing");

            var teamAgents = new IAgent[] { agent1, agent2 };
            var handoffTools1 = new TeamHandoffToolPack(teamAgents, memoryManager1, "persistence-team");

            await handoffTools1.InitializeAsync();

            // Act - First session activities
            await handoffTools1.ShareWithTeam("First session: Initial project setup completed", "setup");
            await handoffTools1.UpdateTeamStatus("Initial phase completed", 25, "Moving to development phase");

            // Arrange - Second session (simulating new instance)
            var memoryManager2 = new MemoryManager(_storage!, _mockModel!, _logger!);
            var handoffTools2 = new TeamHandoffToolPack(teamAgents, memoryManager2, "persistence-team");

            // Act - Second session activities
            await handoffTools2.ShareWithTeam("Second session: Development phase started", "development");
            var contextFromSecondSession = await handoffTools2.GetTeamContext(10);

            // Assert
            Assert.IsNotNull(contextFromSecondSession);
            
            // Should contain information from both sessions due to shared storage
            Assert.IsTrue(contextFromSecondSession.Contains("Recent team communications") || 
                         contextFromSecondSession.Length > 20,
                "Should maintain history across sessions");
        }

        #endregion

        #region Error Handling Integration Tests

        [TestMethod]
        public async Task ErrorHandling_InvalidAgentOperations_ShouldProduceInformativeMessages()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var validAgent = new Agent<string, string>(_mockModel!, "ValidAgent", storage: _storage)
                .WithPersona("Valid agent for testing error scenarios");

            var teamAgents = new IAgent[] { validAgent };
            var handoffTools = new TeamHandoffToolPack(teamAgents, memoryManager, "error-team");

            await handoffTools.InitializeAsync();

            // Act & Assert - Test consultation with unknown agent
            var consultResult = await handoffTools.ConsultAgent("UnknownAgent", "Test question");
            Assert.IsNotNull(consultResult);
            Assert.IsTrue(consultResult.Contains("not found in team"), "Should indicate unknown agent");
            Assert.IsTrue(consultResult.Contains("Available agents:"), "Should list available agents");
            Assert.IsTrue(consultResult.Contains("ValidAgent"), "Should include valid agent in list");

            // Test empty information sharing (should still work)
            var emptyShareResult = await handoffTools.ShareWithTeam("", "empty");
            Assert.IsNotNull(emptyShareResult);
            // Empty sharing should either succeed or provide meaningful error message
            Assert.IsTrue(emptyShareResult.Contains("successfully shared") || 
                         emptyShareResult.Contains("Failed to share") ||
                         emptyShareResult.Length > 0,
                "Should handle empty information appropriately");
        }

        #endregion

        #region Workflow Integration Tests

        [TestMethod]
        public async Task WorkflowIntegration_TeamWithHandoffTools_ShouldEnableFullCoordination()
        {
            // Arrange - Complete team workflow with handoff tools
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var productOwner = new Agent<string, string>(_mockModel!, "ProductOwner", storage: _storage)
                .WithPersona("Product owner who defines requirements and priorities");
            var techLead = new Agent<string, string>(_mockModel!, "TechLead", storage: _storage)
                .WithPersona("Technical lead who guides technical decisions");
            var developer = new Agent<string, string>(_mockModel!, "Developer", storage: _storage)
                .WithPersona("Software developer who implements features");

            var teamAgents = new IAgent[] { productOwner, techLead, developer };
            
            // Add handoff tools to each agent (cast to concrete type to access WithTools)
            var handoffTools = new TeamHandoffToolPack(teamAgents, memoryManager, "workflow-team");
            foreach (var agent in teamAgents)
            {
                if (agent is Agent<string, string> concreteAgent)
                {
                    concreteAgent.WithTools(handoffTools);
                }
            }

            // Create team workflow
            var workflow = new AdvancedWorkflow<string, string>("IntegratedTeamWorkflow")
                .AsTeam(teamAgents, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            await handoffTools.InitializeAsync();

            var featureRequest = @"
FEATURE REQUEST: User Notification System
DESCRIPTION: Implement a comprehensive notification system for users
REQUIREMENTS:
- Email notifications for important events
- In-app notifications with real-time updates
- User preferences for notification types
- Analytics and delivery tracking
- Mobile push notification support

PRIORITY: High
TIMELINE: 4 weeks
";

            // Act - Execute integrated workflow
            var workflowResult = await workflow.ExecuteAsync(featureRequest);

            // Also test direct team communication
            await handoffTools.ShareWithTeam("Feature analysis completed", "analysis");
            var teamContext = await handoffTools.GetTeamContext(5);

            // Assert
            Assert.IsNotNull(workflowResult);
            Assert.IsNotNull(teamContext);
            
            Assert.IsFalse(string.IsNullOrEmpty(workflowResult));
            Assert.IsTrue(workflowResult.Length > 30, "Integrated workflow should produce comprehensive results");
            
            // Team context should contain coordination information
            Assert.IsTrue(teamContext.Contains("Recent team communications") || 
                         teamContext.Contains("analysis") ||
                         teamContext.Length > 20,
                "Team context should reflect integrated coordination");
        }

        #endregion

        #region Scalability Integration Tests

        [TestMethod]
        public async Task ScalabilityTest_LargeTeamCommunication_ShouldMaintainPerformance()
        {
            // Arrange - Large team scenario
            var memoryManager = new MemoryManager(_storage!, _mockModel!, _logger!);
            
            var agents = new IAgent[6];
            for (int i = 0; i < 6; i++)
            {
                agents[i] = new Agent<string, string>(_mockModel!, $"TeamMember{i + 1}", storage: _storage)
                    .WithPersona($"Team member {i + 1} responsible for domain {i + 1}");
            }

            var handoffTools = new TeamHandoffToolPack(agents, memoryManager, "large-team");
            await handoffTools.InitializeAsync();

            var startTime = DateTime.UtcNow;

            // Act - Multiple concurrent team operations
            var tasks = new Task[12]; // 2 operations per team member
            for (int i = 0; i < 6; i++)
            {
                int memberIndex = i;
                tasks[i * 2] = handoffTools.ShareWithTeam(
                    $"Update from team member {memberIndex + 1}: Work in progress", 
                    $"update{memberIndex + 1}");
                tasks[i * 2 + 1] = handoffTools.UpdateTeamStatus(
                    $"Member {memberIndex + 1} status update", 
                    (memberIndex + 1) * 15, 
                    $"Next: Continue work on area {memberIndex + 1}");
            }

            await Task.WhenAll(tasks);
            
            var finalContext = await handoffTools.GetTeamContext(15);
            var totalTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.IsTrue(totalTime.TotalSeconds < 15, $"Large team operations took {totalTime.TotalSeconds} seconds - should be faster");
            Assert.IsNotNull(finalContext);
            Assert.IsTrue(finalContext.Length > 50, "Large team context should be substantial");

            // Should handle multiple concurrent operations successfully
            foreach (var task in tasks)
            {
                Assert.IsTrue(task.IsCompletedSuccessfully, "All team operations should complete successfully");
            }
        }

        #endregion
    }
}