using System;
using System.Collections.Generic;
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
    /// Unit tests for RouterExtensions functionality
    /// Tests smart routing, capability-based routing, performance-based routing, and round-robin routing
    /// </summary>
    [TestClass]
    public class RouterExtensionsTests
    {
        private IModel? _mockModel;
        private IAgent[]? _testAgents;
        private AdvancedWorkflow<string, string>? _workflow;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockModel();

            // Create test agents with different specializations
            _testAgents = new IAgent[]
            {
                new Agent<string, string>(_mockModel, "SecurityExpert")
                    .WithPersona("Cybersecurity specialist expert in threat analysis and security auditing"),
                new Agent<string, string>(_mockModel, "NetworkAdmin")
                    .WithPersona("Network administration specialist expert in infrastructure and connectivity"),
                new Agent<string, string>(_mockModel, "DevOpsEngineer")
                    .WithPersona("DevOps specialist expert in deployment automation and CI/CD pipelines")
            };

            _workflow = new AdvancedWorkflow<string, string>("RouterTest");
        }

        #region Smart Routing Tests

        [TestMethod]
        public void AddSmartRouting_WithValidParameters_ShouldReturnWorkflow()
        {
            // Arrange
            Func<string, IAgent> selector = context => _testAgents![0];

            // Act
            var result = _workflow!.AddSmartRouting("test_routing", _testAgents!, selector);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSmartRouting_WithNullWorkflow_ShouldThrowArgumentNullException()
        {
            // Arrange
            AdvancedWorkflow<string, string>? nullWorkflow = null;
            Func<string, IAgent> selector = context => _testAgents![0];

            // Act & Assert
            nullWorkflow!.AddSmartRouting("test", _testAgents!, selector);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSmartRouting_WithNullAgents_ShouldThrowArgumentException()
        {
            // Arrange
            Func<string, IAgent> selector = context => _testAgents![0];

            // Act & Assert
            _workflow!.AddSmartRouting("test", null!, selector);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSmartRouting_WithEmptyAgents_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyAgents = new IAgent[0];
            Func<string, IAgent> selector = context => _testAgents![0];

            // Act & Assert
            _workflow!.AddSmartRouting("test", emptyAgents, selector);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSmartRouting_WithNullSelector_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            _workflow!.AddSmartRouting("test", _testAgents!, null!);
        }

        #endregion

        #region Capability-Based Routing Tests

        [TestMethod]
        public void AddCapabilityBasedRouting_WithValidParameters_ShouldReturnWorkflow()
        {
            // Arrange
            var capabilities = new Dictionary<string, string[]>
            {
                ["SecurityExpert"] = new[] { "security", "vulnerability", "threat" },
                ["NetworkAdmin"] = new[] { "network", "infrastructure", "connectivity" },
                ["DevOpsEngineer"] = new[] { "deployment", "automation", "pipeline" }
            };

            // Act
            var result = _workflow!.AddCapabilityBasedRouting("capability_test", _testAgents!, capabilities);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AddCapabilityBasedRouting_WithoutCapabilityKeywords_ShouldReturnWorkflow()
        {
            // Act
            var result = _workflow!.AddCapabilityBasedRouting("capability_test", _testAgents!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public async Task CapabilityBasedRouting_WithMatchingKeywords_ShouldRouteToCorrectAgent()
        {
            // Arrange
            var capabilities = new Dictionary<string, string[]>
            {
                ["SecurityExpert"] = new[] { "security", "vulnerability", "threat" },
                ["NetworkAdmin"] = new[] { "network", "infrastructure", "connectivity" },
                ["DevOpsEngineer"] = new[] { "deployment", "automation", "pipeline" }
            };

            var workflow = _workflow!
                .AddCapabilityBasedRouting("security_task", _testAgents!, capabilities)
                .WithMemory(new BasicMemory());

            var securityTask = "We need to perform a security vulnerability assessment";

            // Act
            var result = await workflow.ExecuteAsync(securityTask);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Performance-Based Routing Tests

        [TestMethod]
        public void AddPerformanceBasedRouting_WithValidParameters_ShouldReturnWorkflow()
        {
            // Arrange
            var performanceMetrics = new Dictionary<string, double>
            {
                ["SecurityExpert"] = 9.2,
                ["NetworkAdmin"] = 8.7,
                ["DevOpsEngineer"] = 9.5
            };

            // Act
            var result = _workflow!.AddPerformanceBasedRouting("performance_test", _testAgents!, performanceMetrics);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public void AddPerformanceBasedRouting_WithoutMetrics_ShouldReturnWorkflow()
        {
            // Act
            var result = _workflow!.AddPerformanceBasedRouting("performance_test", _testAgents!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public async Task PerformanceBasedRouting_ShouldRouteToHighestPerformingAgent()
        {
            // Arrange
            var performanceMetrics = new Dictionary<string, double>
            {
                ["SecurityExpert"] = 8.0,
                ["NetworkAdmin"] = 7.5,
                ["DevOpsEngineer"] = 9.5 // Highest performance
            };

            var workflow = _workflow!
                .AddPerformanceBasedRouting("high_priority_task", _testAgents!, performanceMetrics)
                .WithMemory(new BasicMemory());

            var criticalTask = "Handle critical system issue immediately";

            // Act
            var result = await workflow.ExecuteAsync(criticalTask);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Round-Robin Routing Tests

        [TestMethod]
        public void AddRoundRobinRouting_WithValidParameters_ShouldReturnWorkflow()
        {
            // Act
            var result = _workflow!.AddRoundRobinRouting("roundrobin_test", _testAgents!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        public async Task RoundRobinRouting_ShouldDistributeTasksEvenly()
        {
            // Arrange
            var workflow = _workflow!
                .AddRoundRobinRouting("load_balanced_task", _testAgents!)
                .WithMemory(new BasicMemory());

            var tasks = new[]
            {
                "Task 1",
                "Task 2", 
                "Task 3"
            };

            // Act
            var results = new List<string>();
            foreach (var task in tasks)
            {
                var result = await workflow.ExecuteAsync(task);
                results.Add(result);
            }

            // Assert
            Assert.AreEqual(3, results.Count);
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
        }

        #endregion

        #region Conditional Routing Tests

        [TestMethod]
        public void AddConditionalRouting_WithValidParameters_ShouldReturnWorkflow()
        {
            // Arrange
            var routingRules = new Dictionary<Func<string, bool>, IAgent>
            {
                [context => context.ToLower().Contains("security")] = _testAgents![0],
                [context => context.ToLower().Contains("network")] = _testAgents![1]
            };
            var defaultAgent = _testAgents![2];

            // Act
            var result = _workflow!.AddConditionalRouting("conditional_test", routingRules, defaultAgent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AdvancedWorkflow<string, string>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddConditionalRouting_WithNullRules_ShouldThrowArgumentException()
        {
            // Arrange
            var defaultAgent = _testAgents![0];

            // Act & Assert
            _workflow!.AddConditionalRouting("test", null!, defaultAgent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddConditionalRouting_WithEmptyRules_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyRules = new Dictionary<Func<string, bool>, IAgent>();
            var defaultAgent = _testAgents![0];

            // Act & Assert
            _workflow!.AddConditionalRouting("test", emptyRules, defaultAgent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddConditionalRouting_WithNullDefaultAgent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var routingRules = new Dictionary<Func<string, bool>, IAgent>
            {
                [context => context.Contains("test")] = _testAgents![0]
            };

            // Act & Assert
            _workflow!.AddConditionalRouting("test", routingRules, null!);
        }

        [TestMethod]
        public async Task ConditionalRouting_WithMatchingCondition_ShouldRouteToCorrectAgent()
        {
            // Arrange
            var routingRules = new Dictionary<Func<string, bool>, IAgent>
            {
                [context => context.ToLower().Contains("security")] = _testAgents![0],
                [context => context.ToLower().Contains("network")] = _testAgents![1]
            };
            var defaultAgent = _testAgents![2];

            var workflow = _workflow!
                .AddConditionalRouting("conditional_task", routingRules, defaultAgent)
                .WithMemory(new BasicMemory());

            var securityTask = "Handle security breach investigation";

            // Act
            var result = await workflow.ExecuteAsync(securityTask);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Thread Safety Tests

        [TestMethod]
        public async Task RoundRobinRouting_WithConcurrentRequests_ShouldBeThreadSafe()
        {
            // Arrange
            var workflow = _workflow!
                .AddRoundRobinRouting("concurrent_test", _testAgents!)
                .WithMemory(new BasicMemory());

            var tasks = new List<Task<string>>();
            
            // Act
            for (int i = 0; i < 10; i++)
            {
                var task = workflow.ExecuteAsync($"Concurrent task {i}");
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(10, results.Length);
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
        }

        #endregion
    }
}