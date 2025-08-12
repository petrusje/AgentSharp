using System;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Storage;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Models;
using AgentSharp.Utils;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Simple runner program to demonstrate team workflow functionality.
    /// Run this to see all team orchestration features in action.
    /// </summary>
    public class TeamWorkflowRunner
    {
        public static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();
            logger.Log(LogLevel.Info, "Starting Team Workflow Examples");

            try
            {
                // Initialize components (replace with actual model and memory manager)
                var model = new MockModel(); // Replace with OpenAIModel or your preferred model
                var storage = new SemanticSqliteStorage("Data Source=:memory:", new MockEmbeddingService(), 1536);
                var memoryManager = new MemoryManager(storage, model, logger, new MockEmbeddingService(), null);

                var examples = new TeamWorkflowExample(model, memoryManager, logger);

                // Run all examples
                await RunAllExamples(examples, logger);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error running team workflow examples: {ex.Message}", ex);
            }

            logger.Log(LogLevel.Info, "Team Workflow Examples completed. Press any key to exit.");
            Console.ReadKey();
        }

        private static async Task RunAllExamples(TeamWorkflowExample examples, ILogger logger)
        {
            logger.Log(LogLevel.Info, "=== TEAM WORKFLOW EXAMPLES ===\n");

            // Example 1: Development Team Coordination
            logger.Log(LogLevel.Info, "1. Running Development Team Coordination Example...");
            try
            {
                var result1 = await examples.RunDevelopmentTeamCoordinationExample();
                logger.Log(LogLevel.Info, "Development Team Result:");
                logger.Log(LogLevel.Info, result1);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Development Team Example failed: {ex.Message}");
            }

            logger.Log(LogLevel.Info, "\n" + new string('=', 80) + "\n");

            // Example 2: Customer Service Routing
            logger.Log(LogLevel.Info, "2. Running Customer Service Routing Example...");
            try
            {
                var result2 = await examples.RunCustomerServiceRoutingExample();
                logger.Log(LogLevel.Info, "Customer Service Results:");
                logger.Log(LogLevel.Info, result2);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Customer Service Example failed: {ex.Message}");
            }

            logger.Log(LogLevel.Info, "\n" + new string('=', 80) + "\n");

            // Example 3: Research Team Collaboration
            logger.Log(LogLevel.Info, "3. Running Research Team Collaboration Example...");
            try
            {
                var result3 = await examples.RunResearchTeamCollaborationExample();
                logger.Log(LogLevel.Info, "Research Team Result:");
                logger.Log(LogLevel.Info, result3);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Research Team Example failed: {ex.Message}");
            }

            logger.Log(LogLevel.Info, "\n" + new string('=', 80) + "\n");

            // Example 4: Advanced Routing
            logger.Log(LogLevel.Info, "4. Running Advanced Routing Example...");
            try
            {
                var result4 = await examples.RunAdvancedRoutingExample();
                logger.Log(LogLevel.Info, "Advanced Routing Results:");
                logger.Log(LogLevel.Info, result4);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Advanced Routing Example failed: {ex.Message}");
            }

            logger.Log(LogLevel.Info, "\n" + new string('=', 80) + "\n");

            // Example 5: Complex Handoff Chain
            logger.Log(LogLevel.Info, "5. Running Complex Handoff Chain Example...");
            try
            {
                var result5 = await examples.RunComplexHandoffChainExample();
                logger.Log(LogLevel.Info, "Complex Handoff Chain Result:");
                logger.Log(LogLevel.Info, result5);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Complex Handoff Chain Example failed: {ex.Message}");
            }

            logger.Log(LogLevel.Info, "\n=== ALL EXAMPLES COMPLETED ===");
        }

        /// <summary>
        /// Run a single example by number for testing purposes
        /// </summary>
        public static async Task RunSingleExample(int exampleNumber)
        {
            var logger = new ConsoleLogger();
            var model = new MockModel();
            var storage = new SemanticSqliteStorage("Data Source=:memory:", new MockEmbeddingService(), 1536);
            var memoryManager = new MemoryManager(storage, model, logger, new MockEmbeddingService(), null);
            var examples = new TeamWorkflowExample(model, memoryManager, logger);

            switch (exampleNumber)
            {
                case 1:
                    logger.Log(LogLevel.Info, "Running Development Team Coordination Example...");
                    var result1 = await examples.RunDevelopmentTeamCoordinationExample();
                    logger.Log(LogLevel.Info, $"Result: {result1}");
                    break;

                case 2:
                    logger.Log(LogLevel.Info, "Running Customer Service Routing Example...");
                    var result2 = await examples.RunCustomerServiceRoutingExample();
                    logger.Log(LogLevel.Info, $"Result: {result2}");
                    break;

                case 3:
                    logger.Log(LogLevel.Info, "Running Research Team Collaboration Example...");
                    var result3 = await examples.RunResearchTeamCollaborationExample();
                    logger.Log(LogLevel.Info, $"Result: {result3}");
                    break;

                case 4:
                    logger.Log(LogLevel.Info, "Running Advanced Routing Example...");
                    var result4 = await examples.RunAdvancedRoutingExample();
                    logger.Log(LogLevel.Info, $"Result: {result4}");
                    break;

                case 5:
                    logger.Log(LogLevel.Info, "Running Complex Handoff Chain Example...");
                    var result5 = await examples.RunComplexHandoffChainExample();
                    logger.Log(LogLevel.Info, $"Result: {result5}");
                    break;

                default:
                    logger.Log(LogLevel.Error, $"Invalid example number: {exampleNumber}. Choose 1-5.");
                    break;
            }
        }
    }
}
