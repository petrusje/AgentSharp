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
    [TestClass]
    public class TeamHandoffDebugTest
    {
        [TestMethod]
        public async Task DebugHandoffTask()
        {
            // Setup
            var mockModel = new MockModel();
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new OpenAIEmbeddingService("test", "https://test"), 1536);
            var logger = new ConsoleLogger();
            var memoryManager = new MemoryManager(storage, mockModel, logger);

            var testAgents = new IAgent[]
            {
                new Agent<string, string>(mockModel, "ProjectManager")
                    .WithPersona("Project management specialist"),
                new Agent<string, string>(mockModel, "Developer")
                    .WithPersona("Software development specialist")
            };

            var toolPack = new TeamHandoffToolPack(testAgents, memoryManager, "test-team");

            // Test
            var result = await toolPack.HandoffTask("Developer", "Test task", "Test reason", "High");
            
            // Debug output
            Console.WriteLine($"Actual result: '{result}'");
            Console.WriteLine($"Contains 'successfully handed off': {result.Contains("successfully handed off")}");
            Console.WriteLine($"Contains 'Developer': {result.Contains("Developer")}");
            
            // Let's see what went wrong
            Assert.IsNotNull(result);
        }
    }
}