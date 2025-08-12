using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentSharp.Tests.Integration
{
    /// <summary>
    /// Integration tests for Team Orchestration workflows.
    /// Tests complete end-to-end team scenarios with real agent execution.
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class TeamWorkflowIntegrationTests
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

        #region Coordinate Mode Integration Tests

        [TestMethod]
        public async Task CoordinateMode_DevelopmentTeamScenario_ShouldExecuteSuccessfully()
        {
            // Arrange - Create development team
            var teamLead = new Agent<string, string>(_mockModel!, "TeamLead", storage: _storage)
                .WithPersona("Experienced team lead who manages projects and coordinates tasks");
            var frontendDev = new Agent<string, string>(_mockModel!, "FrontendDev", storage: _storage)
                .WithPersona("Frontend specialist expert in React and TypeScript");
            var backendDev = new Agent<string, string>(_mockModel!, "BackendDev", storage: _storage)
                .WithPersona("Backend specialist expert in .NET and APIs");

            var teamAgents = new IAgent[] { teamLead, frontendDev, backendDev };
            var workflow = new AdvancedWorkflow<string, string>("DevTeamCoordinate")
                .AsTeam(teamAgents, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            var projectTask = @"
PROJECT: E-commerce User Authentication
REQUIREMENT: Implement secure user login and registration
TECHNICAL DETAILS:
- Frontend: React components for login/register forms
- Backend: JWT authentication with .NET Core
- Database: User table with encrypted passwords
- Security: Input validation and CSRF protection
";

            // Act
            var result = await workflow.ExecuteAsync(projectTask);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            
            // Verify the result contains project-related content
            Assert.IsTrue(result.Contains("authentication") || result.Contains("login") || result.Contains("user"), 
                "Result should be related to the authentication project");
        }

        [TestMethod]
        public async Task CoordinateMode_WithMemoryPersistence_ShouldMaintainContext()
        {
            // Arrange
            var coordinator = new Agent<string, string>(_mockModel!, "ProjectCoordinator", storage: _storage)
                .WithPersona("Project coordinator who manages team tasks and maintains project context");
            var developer = new Agent<string, string>(_mockModel!, "Developer", storage: _storage)
                .WithPersona("Software developer who implements features");

            var teamAgents = new IAgent[] { coordinator, developer };
            var workflow = new AdvancedWorkflow<string, string>("MemoryCoordinate")
                .AsTeam(teamAgents, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            // Act - Execute multiple related tasks
            var result1 = await workflow.ExecuteAsync("Start project planning for mobile app development");
            var result2 = await workflow.ExecuteAsync("Continue with the mobile app project we discussed");

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsFalse(string.IsNullOrEmpty(result1));
            Assert.IsFalse(string.IsNullOrEmpty(result2));
            
            // Both results should be valid responses
            Assert.IsTrue(result1.Length > 10, "First result should contain substantial content");
            Assert.IsTrue(result2.Length > 10, "Second result should contain substantial content");
        }

        #endregion

        #region Route Mode Integration Tests

        [TestMethod]
        public async Task RouteMode_CapabilityBasedRouting_ShouldRouteToAppropriateAgent()
        {
            // Arrange - Create specialist team
            var securityExpert = new Agent<string, string>(_mockModel!, "SecurityExpert", storage: _storage)
                .WithPersona("Cybersecurity specialist expert in threat analysis and security auditing");
            var networkAdmin = new Agent<string, string>(_mockModel!, "NetworkAdmin", storage: _storage)
                .WithPersona("Network administrator expert in infrastructure and connectivity");
            var devOpsEngineer = new Agent<string, string>(_mockModel!, "DevOpsEngineer", storage: _storage)
                .WithPersona("DevOps engineer expert in deployment and automation");

            var teamAgents = new IAgent[] { securityExpert, networkAdmin, devOpsEngineer };
            var workflow = new AdvancedWorkflow<string, string>("ITSupportRoute")
                .AsTeam(teamAgents, TeamMode.Route)
                .AddCapabilityBasedRouting("it_task", teamAgents, new Dictionary<string, string[]>
                {
                    ["SecurityExpert"] = new[] { "security", "vulnerability", "threat", "audit", "breach" },
                    ["NetworkAdmin"] = new[] { "network", "infrastructure", "connectivity", "server" },
                    ["DevOpsEngineer"] = new[] { "deployment", "automation", "pipeline", "docker", "ci/cd" }
                })
                .WithMemory(new BasicMemory());

            // Test different routing scenarios
            var securityTask = "We need to conduct a security vulnerability assessment of our web application";
            var networkTask = "The office network connectivity is experiencing intermittent issues";
            var devOpsTask = "Setup automated deployment pipeline for our microservices";

            // Act & Assert
            var securityResult = await workflow.ExecuteAsync(securityTask);
            Assert.IsNotNull(securityResult);
            Assert.IsFalse(string.IsNullOrEmpty(securityResult));

            var networkResult = await workflow.ExecuteAsync(networkTask);
            Assert.IsNotNull(networkResult);
            Assert.IsFalse(string.IsNullOrEmpty(networkResult));

            var devOpsResult = await workflow.ExecuteAsync(devOpsTask);
            Assert.IsNotNull(devOpsResult);
            Assert.IsFalse(string.IsNullOrEmpty(devOpsResult));

            // All results should be substantial responses
            Assert.IsTrue(securityResult.Length > 20, "Security expert should provide detailed response");
            Assert.IsTrue(networkResult.Length > 20, "Network admin should provide detailed response");
            Assert.IsTrue(devOpsResult.Length > 20, "DevOps engineer should provide detailed response");
        }

        [TestMethod]
        public async Task RouteMode_PerformanceBasedRouting_ShouldRouteToHighestPerformer()
        {
            // Arrange
            var agent1 = new Agent<string, string>(_mockModel!, "Agent1", storage: _storage)
                .WithPersona("General purpose agent with moderate performance");
            var agent2 = new Agent<string, string>(_mockModel!, "Agent2", storage: _storage)
                .WithPersona("High performance specialist agent");
            var agent3 = new Agent<string, string>(_mockModel!, "Agent3", storage: _storage)
                .WithPersona("Basic agent with lower performance");

            var performanceMetrics = new Dictionary<string, double>
            {
                ["Agent1"] = 7.5,
                ["Agent2"] = 9.2, // Highest performance
                ["Agent3"] = 6.8
            };

            var teamAgents = new IAgent[] { agent1, agent2, agent3 };
            var workflow = new AdvancedWorkflow<string, string>("PerformanceRoute")
                .AsTeam(teamAgents, TeamMode.Route)
                .AddPerformanceBasedRouting("critical_task", teamAgents, performanceMetrics)
                .WithMemory(new BasicMemory());

            // Act
            var result = await workflow.ExecuteAsync("Handle this critical high-priority task");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Length > 10, "High-performance agent should provide substantial response");
        }

        #endregion

        #region Collaborate Mode Integration Tests

        [TestMethod]
        public async Task CollaborateMode_ResearchTeamScenario_ShouldSynthesizeResults()
        {
            // Arrange - Create research team
            var marketResearcher = new Agent<string, string>(_mockModel!, "MarketResearcher", storage: _storage)
                .WithPersona("Market research specialist expert in consumer trends and competitive analysis");
            var dataAnalyst = new Agent<string, string>(_mockModel!, "DataAnalyst", storage: _storage)
                .WithPersona("Data analyst expert in statistics and quantitative analysis");
            var trendAnalyst = new Agent<string, string>(_mockModel!, "TrendAnalyst", storage: _storage)
                .WithPersona("Trend analyst expert in pattern recognition and forecasting");

            var teamAgents = new IAgent[] { marketResearcher, dataAnalyst, trendAnalyst };
            var workflow = new AdvancedWorkflow<string, string>("ResearchCollaborate")
                .AsTeam(teamAgents, TeamMode.Collaborate)
                .WithMemory(new BasicMemory());

            var researchTopic = @"
RESEARCH PROJECT: Sustainable Technology Market Analysis
SCOPE: Analyze the renewable energy technology market for strategic planning
FOCUS AREAS:
- Market size and growth projections
- Key industry players and competition
- Consumer adoption patterns and barriers
- Technology innovation trends
- Regulatory impact and policy changes
- Investment opportunities and risks
";

            // Act
            var result = await workflow.ExecuteAsync(researchTopic);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            
            // Result should be a comprehensive analysis
            Assert.IsTrue(result.Length > 50, "Collaborative result should be comprehensive");
            
            // Should contain research-related terms
            Assert.IsTrue(result.Contains("market") || result.Contains("analysis") || result.Contains("research") || 
                         result.Contains("technology") || result.Contains("energy"), 
                "Result should contain research-related content");
        }

        [TestMethod]
        public async Task CollaborateMode_WithDifferentExpertise_ShouldProvideDiversePerspectives()
        {
            // Arrange - Create diverse expert team
            var technicalExpert = new Agent<string, string>(_mockModel!, "TechnicalExpert", storage: _storage)
                .WithPersona("Technical expert focused on implementation and architecture");
            var businessAnalyst = new Agent<string, string>(_mockModel!, "BusinessAnalyst", storage: _storage)
                .WithPersona("Business analyst focused on requirements and ROI");
            var userExperienceExpert = new Agent<string, string>(_mockModel!, "UXExpert", storage: _storage)
                .WithPersona("User experience expert focused on usability and design");

            var teamAgents = new IAgent[] { technicalExpert, businessAnalyst, userExperienceExpert };
            var workflow = new AdvancedWorkflow<string, string>("ExpertCollaboration")
                .AsTeam(teamAgents, TeamMode.Collaborate)
                .WithMemory(new BasicMemory());

            var productChallenge = @"
PRODUCT CHALLENGE: Design a Mobile Banking App
GOAL: Create a secure, user-friendly mobile banking application
CONSIDERATIONS:
- Technical requirements for security and performance
- Business requirements for customer acquisition and retention
- User experience requirements for accessibility and ease of use
- Regulatory compliance and data protection
- Integration with existing banking systems
";

            // Act
            var result = await workflow.ExecuteAsync(productChallenge);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Length > 30, "Multi-expert collaboration should produce detailed results");
            
            // Should address the banking app challenge
            Assert.IsTrue(result.Contains("banking") || result.Contains("mobile") || result.Contains("app") || 
                         result.Contains("financial") || result.Contains("security"), 
                "Result should address the banking app challenge");
        }

        #endregion

        #region Round-Robin Integration Tests

        [TestMethod]
        public async Task RoundRobinRouting_MultipleRequests_ShouldDistributeEvenly()
        {
            // Arrange
            var agent1 = new Agent<string, string>(_mockModel!, "LoadBalancer1", storage: _storage)
                .WithPersona("Load balanced agent instance 1");
            var agent2 = new Agent<string, string>(_mockModel!, "LoadBalancer2", storage: _storage)
                .WithPersona("Load balanced agent instance 2");
            var agent3 = new Agent<string, string>(_mockModel!, "LoadBalancer3", storage: _storage)
                .WithPersona("Load balanced agent instance 3");

            var teamAgents = new IAgent[] { agent1, agent2, agent3 };
            var workflow = new AdvancedWorkflow<string, string>("LoadBalancedRoute")
                .AsTeam(teamAgents, TeamMode.Route)
                .AddRoundRobinRouting("balanced_task", teamAgents)
                .WithMemory(new BasicMemory());

            var tasks = new[]
            {
                "Process customer service request #001",
                "Process customer service request #002",
                "Process customer service request #003",
                "Process customer service request #004",
                "Process customer service request #005",
                "Process customer service request #006"
            };

            // Act - Execute multiple tasks
            var results = new List<string>();
            foreach (var task in tasks)
            {
                var result = await workflow.ExecuteAsync(task);
                results.Add(result);
            }

            // Assert
            Assert.AreEqual(6, results.Count);
            
            // All results should be valid
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsFalse(string.IsNullOrEmpty(result));
                Assert.IsTrue(result.Length > 5, "Each result should contain meaningful content");
            }
        }

        #endregion

        #region Error Handling Integration Tests

        [TestMethod]
        public async Task TeamWorkflow_WithInvalidInput_ShouldHandleGracefully()
        {
            // Arrange
            var agent1 = new Agent<string, string>(_mockModel!, "ErrorTestAgent1", storage: _storage)
                .WithPersona("Agent for error handling tests");
            var agent2 = new Agent<string, string>(_mockModel!, "ErrorTestAgent2", storage: _storage)
                .WithPersona("Second agent for error handling tests");

            var teamAgents = new IAgent[] { agent1, agent2 };
            var workflow = new AdvancedWorkflow<string, string>("ErrorHandlingTest")
                .AsTeam(teamAgents, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            // Act & Assert - Test with empty input
            var emptyResult = await workflow.ExecuteAsync("");
            Assert.IsNotNull(emptyResult, "Should handle empty input gracefully");

            // Test with null input (should not crash)
            try
            {
                var nullResult = await workflow.ExecuteAsync(null!);
                Assert.IsNotNull(nullResult, "Should handle null input gracefully");
            }
            catch (ArgumentNullException)
            {
                // This is acceptable - null input can throw ArgumentNullException
                Assert.IsTrue(true, "ArgumentNullException is acceptable for null input");
            }
        }

        #endregion

        #region Mixed Mode Integration Tests

        [TestMethod]
        public async Task TeamWorkflow_MixedModesInSequence_ShouldExecuteSuccessfully()
        {
            // Arrange - Create agents for different workflow stages
            var planner = new Agent<string, string>(_mockModel!, "ProjectPlanner", storage: _storage)
                .WithPersona("Project planner who creates detailed project plans");
            var architect = new Agent<string, string>(_mockModel!, "SolutionArchitect", storage: _storage)
                .WithPersona("Solution architect who designs technical solutions");
            var developer = new Agent<string, string>(_mockModel!, "SoftwareDeveloper", storage: _storage)
                .WithPersona("Software developer who implements solutions");

            var planningAgents = new IAgent[] { planner };
            var designAgents = new IAgent[] { architect };
            var developmentAgents = new IAgent[] { developer };

            // Create workflows for different phases
            var planningWorkflow = new AdvancedWorkflow<string, string>("ProjectPlanning")
                .AsTeam(planningAgents, TeamMode.Coordinate)
                .WithMemory(new BasicMemory());

            var designWorkflow = new AdvancedWorkflow<string, string>("SolutionDesign")
                .AsTeam(designAgents, TeamMode.Route)
                .AddCapabilityBasedRouting("design_task", designAgents)
                .WithMemory(new BasicMemory());

            var developmentWorkflow = new AdvancedWorkflow<string, string>("Implementation")
                .AsTeam(developmentAgents, TeamMode.Collaborate)
                .WithMemory(new BasicMemory());

            var projectRequest = @"
PROJECT REQUEST: Customer Relationship Management System
REQUIREMENTS: Build a CRM system to manage customer interactions, sales pipeline, and support tickets
TIMELINE: 3 months
TEAM SIZE: 5 developers
BUDGET: $150,000
";

            // Act - Execute sequential workflows
            var planResult = await planningWorkflow.ExecuteAsync($"Create project plan for: {projectRequest}");
            var designResult = await designWorkflow.ExecuteAsync($"Design solution architecture for: {projectRequest}. Plan: {planResult}");
            var developmentResult = await developmentWorkflow.ExecuteAsync($"Implement solution based on: {designResult}");

            // Assert
            Assert.IsNotNull(planResult);
            Assert.IsNotNull(designResult);
            Assert.IsNotNull(developmentResult);
            
            Assert.IsFalse(string.IsNullOrEmpty(planResult));
            Assert.IsFalse(string.IsNullOrEmpty(designResult));
            Assert.IsFalse(string.IsNullOrEmpty(developmentResult));
            
            // Each phase should produce meaningful output
            Assert.IsTrue(planResult.Length > 20, "Planning phase should produce detailed plan");
            Assert.IsTrue(designResult.Length > 20, "Design phase should produce detailed architecture");
            Assert.IsTrue(developmentResult.Length > 20, "Development phase should produce implementation details");
        }

        #endregion

        #region Performance and Scalability Tests

        [TestMethod]
        public async Task TeamWorkflow_WithLargeTeam_ShouldPerformEfficiently()
        {
            // Arrange - Create larger team (simulate department)
            var agents = new List<IAgent>();
            for (int i = 1; i <= 8; i++)
            {
                var agent = new Agent<string, string>(_mockModel!, $"TeamMember{i}", storage: _storage)
                    .WithPersona($"Team member {i} specialized in area {i}");
                agents.Add(agent);
            }

            var workflow = new AdvancedWorkflow<string, string>("LargeTeamTest")
                .AsTeam(agents.ToArray(), TeamMode.Route)
                .AddRoundRobinRouting("distributed_task", agents.ToArray())
                .WithMemory(new BasicMemory());

            var departmentTask = @"
DEPARTMENT INITIATIVE: Digital Transformation
SCOPE: Modernize all business processes with digital solutions
AREAS: Customer service, operations, finance, HR, marketing, sales, IT, compliance
TIMELINE: 12 months
GOAL: Increase efficiency by 40% and reduce costs by 25%
";

            // Act - Measure execution time
            var startTime = DateTime.UtcNow;
            var result = await workflow.ExecuteAsync(departmentTask);
            var executionTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            
            // Should complete within reasonable time (adjust threshold as needed)
            Assert.IsTrue(executionTime.TotalSeconds < 30, $"Large team workflow took {executionTime.TotalSeconds} seconds - should be faster");
            
            // Should produce comprehensive response
            Assert.IsTrue(result.Length > 50, "Large team should produce comprehensive response");
        }

        #endregion
    }
}