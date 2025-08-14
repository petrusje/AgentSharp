using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using AgentSharp.Utils;
using AgentSharp.console.Utils;

namespace AgentSharp.console
{
    /// <summary>
    /// Console examples demonstrating Team Orchestration capabilities.
    /// Shows how to use multiple agents working together in coordinated teams.
    /// </summary>
    public static class TeamOrchestrationExamples
    {
        private static readonly ConsoleObj _console = new();

        #region Team Mode Examples

        /// <summary>
        /// Example 23: Coordinate Mode - Development Team with Lead Coordination
        /// Demonstrates how one coordinator manages task delegation to specialists.
        /// </summary>
        public static async Task ExecutarExemploCoordinateMode(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Cyan)
                .WriteLine("🎯 TEAM ORCHESTRATION: Coordinate Mode - Development Team")
                .WriteLine("═══════════════════════════════════════════════════════════")
                .ResetColor();

            try
            {
                // Create specialized agents
                var teamLead = new Agent<string, string>(modelo, "TeamLead")
                    .WithPersona("Experienced team lead who manages projects, coordinates tasks, and makes technical decisions");
                var frontendDev = new Agent<string, string>(modelo, "FrontendDev")
                    .WithPersona("Frontend specialist expert in React, TypeScript, and modern UI/UX development");
                var backendDev = new Agent<string, string>(modelo, "BackendDev")
                    .WithPersona("Backend specialist expert in .NET, APIs, databases, and system architecture");
                var qaDev = new Agent<string, string>(modelo, "QADev")
                    .WithPersona("Quality assurance specialist expert in testing strategies, automation, and bug detection");

                // Create team workflow using Coordinate mode
                var teamAgents = new IAgent[] { teamLead, frontendDev, backendDev, qaDev };
                var workflow = new AdvancedWorkflow<string, string>("DevTeamWorkflow")
                    .AsTeam(teamAgents, TeamMode.Coordinate)
                    .WithMemory(new BasicMemory());

                // Project context
                var projectContext = @"
PROJECT: E-commerce Platform Feature Request
REQUIREMENT: Implement user wishlist functionality
DETAILS: Users need to save products to a wishlist, view saved items, and manage their lists
CONSTRAINTS: Must integrate with existing user system and product catalog
DEADLINE: 2 weeks
TECHNICAL STACK: React frontend, .NET backend, SQL Server database
";

                _console.WriteLine("📋 Executing coordinate mode workflow...");
                _console.WriteLine($"Context: {projectContext}");
                _console.WriteLine("");

                var result = await workflow.ExecuteAsync(projectContext);

                _console.WithColor(ConsoleColor.Green)
                    .WriteLine("✅ COORDINATE MODE RESULT:")
                    .WriteLine($"{result}")
                    .ResetColor();
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Error in Coordinate Mode example: {ex.Message}")
                    .ResetColor();
            }
        }

        /// <summary>
        /// Example 24: Route Mode - Smart Customer Service Routing
        /// Demonstrates intelligent routing to the most appropriate specialist.
        /// </summary>
        public static async Task ExecutarExemploRouteMode(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🎯 TEAM ORCHESTRATION: Route Mode - Customer Service")
                .WriteLine("═════════════════════════════════════════════════════")
                .ResetColor();

            try
            {
                // Create specialized customer service agents
                var technicalSupport = new Agent<string, string>(modelo, "TechnicalSupport")
                    .WithPersona("Technical support specialist expert in troubleshooting, software issues, and system problems");
                var billingSupport = new Agent<string, string>(modelo, "BillingSupport")
                    .WithPersona("Billing support specialist expert in payments, subscriptions, refunds, and account issues");
                var generalSupport = new Agent<string, string>(modelo, "GeneralSupport")
                    .WithPersona("General customer support specialist expert in product information, basic questions, and general assistance");

                // Create team workflow using Route mode with capability-based routing
                var teamAgents = new IAgent[] { technicalSupport, billingSupport, generalSupport };
                var workflow = new AdvancedWorkflow<string, string>("CustomerServiceWorkflow")
                    .AsTeam(teamAgents, TeamMode.Route)
                    .AddCapabilityBasedRouting("customer_inquiry", teamAgents, new Dictionary<string, string[]>
                    {
                        ["TechnicalSupport"] = new[] { "technical", "bug", "error", "not working", "crash", "login", "system", "software" },
                        ["BillingSupport"] = new[] { "billing", "payment", "charge", "subscription", "refund", "invoice", "account", "money" },
                        ["GeneralSupport"] = new[] { "general", "information", "question", "help", "how to", "what is" }
                    })
                    .WithMemory(new BasicMemory());

                // Customer inquiries to route
                var inquiries = new[]
                {
                    "Hi, I'm having trouble logging into my account. The system keeps giving me an error message.",
                    "I was charged twice for my subscription this month. Can you help me get a refund?",
                    "What are the different pricing plans available for your service?"
                };

                foreach (var inquiry in inquiries)
                {
                    _console.WriteLine($"📞 Processing inquiry: {inquiry}");
                    _console.WriteLine("");

                    var result = await workflow.ExecuteAsync(inquiry);

                    _console.WithColor(ConsoleColor.Green)
                        .WriteLine("✅ ROUTE MODE RESULT:")
                        .WriteLine($"{result}")
                        .WriteLine("")
                        .ResetColor();
                }
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Error in Route Mode example: {ex.Message}")
                    .ResetColor();
            }
        }

        /// <summary>
        /// Example 25: Collaborate Mode - Research Team Collaboration
        /// Demonstrates parallel execution with result synthesis.
        /// </summary>
        public static async Task ExecutarExemploCollaborateMode(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Magenta)
                .WriteLine("🎯 TEAM ORCHESTRATION: Collaborate Mode - Research Team")
                .WriteLine("═══════════════════════════════════════════════════════")
                .ResetColor();

            try
            {
                // Create specialized research agents
                var marketResearcher = new Agent<string, string>(modelo, "MarketResearcher")
                    .WithPersona("Market research specialist expert in market analysis, consumer trends, and competitive intelligence");
                var dataAnalyst = new Agent<string, string>(modelo, "DataAnalyst")
                    .WithPersona("Data analysis specialist expert in statistics, data mining, and quantitative analysis");
                var trendAnalyst = new Agent<string, string>(modelo, "TrendAnalyst")
                    .WithPersona("Trend analysis specialist expert in identifying patterns, forecasting, and predictive analysis");

                // Create team workflow using Collaborate mode
                var teamAgents = new IAgent[] { marketResearcher, dataAnalyst, trendAnalyst };
                var workflow = new AdvancedWorkflow<string, string>("ResearchTeamWorkflow")
                    .AsTeam(teamAgents, TeamMode.Collaborate)
                    .WithMemory(new BasicMemory());

                // Research topic
                var researchTopic = @"
RESEARCH TOPIC: Electric Vehicle Market Analysis
SCOPE: Analyze the current state and future prospects of the electric vehicle market
FOCUS AREAS:
- Market size and growth trends
- Key players and competitive landscape
- Consumer adoption patterns
- Technology developments
- Regulatory environment impact
- Investment and funding trends

OBJECTIVE: Provide comprehensive insights for strategic planning
TARGET AUDIENCE: C-level executives and investors
";

                _console.WriteLine("🔍 Executing collaborate mode workflow...");
                _console.WriteLine($"Research Topic: {researchTopic}");
                _console.WriteLine("");

                var result = await workflow.ExecuteAsync(researchTopic);

                _console.WithColor(ConsoleColor.Green)
                    .WriteLine("✅ COLLABORATE MODE RESULT:")
                    .WriteLine($"{result}")
                    .ResetColor();
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Error in Collaborate Mode example: {ex.Message}")
                    .ResetColor();
            }
        }

        #endregion

        #region Routing Strategy Examples

        /// <summary>
        /// Example 26: Advanced Routing Strategies
        /// Demonstrates different routing approaches: capability-based, performance-based, round-robin.
        /// </summary>
        public static async Task ExecutarExemploRoutingStrategies(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Blue)
                .WriteLine("🎯 TEAM ORCHESTRATION: Advanced Routing Strategies")
                .WriteLine("════════════════════════════════════════════════")
                .ResetColor();

            try
            {
                // Create diverse specialist agents
                var securityExpert = new Agent<string, string>(modelo, "SecurityExpert")
                    .WithPersona("Cybersecurity specialist expert in threat analysis, security auditing, and risk assessment");
                var networkAdmin = new Agent<string, string>(modelo, "NetworkAdmin")
                    .WithPersona("Network administration specialist expert in infrastructure, connectivity, and system maintenance");
                var devOpsEngineer = new Agent<string, string>(modelo, "DevOpsEngineer")
                    .WithPersona("DevOps specialist expert in deployment, automation, CI/CD, and infrastructure as code");

                var teamAgents = new IAgent[] { securityExpert, networkAdmin, devOpsEngineer };

                // 1. Capability-Based Routing
                _console.WriteLine("🔍 1. CAPABILITY-BASED ROUTING");
                _console.WriteLine("Routing based on agent capabilities and keywords");

                var capabilityWorkflow = new AdvancedWorkflow<string, string>("CapabilityWorkflow")
                    .AddCapabilityBasedRouting("security_task", teamAgents, new Dictionary<string, string[]>
                    {
                        ["SecurityExpert"] = new[] { "security", "vulnerability", "threat", "audit", "breach", "encryption" },
                        ["NetworkAdmin"] = new[] { "network", "infrastructure", "connectivity", "server", "maintenance" },
                        ["DevOpsEngineer"] = new[] { "deployment", "automation", "pipeline", "docker", "kubernetes", "ci/cd" }
                    })
                    .WithMemory(new BasicMemory());

                var securityTask = "We need to conduct a security audit of our new web application to identify potential vulnerabilities.";
                var capabilityResult = await capabilityWorkflow.ExecuteAsync(securityTask);

                _console.WithColor(ConsoleColor.Green)
                    .WriteLine($"Task: {securityTask}")
                    .WriteLine($"Result: {capabilityResult}")
                    .WriteLine("")
                    .ResetColor();

                // 2. Performance-Based Routing
                _console.WriteLine("⚡ 2. PERFORMANCE-BASED ROUTING");
                _console.WriteLine("Routing to the highest performing agent");

                var performanceMetrics = new Dictionary<string, double>
                {
                    ["SecurityExpert"] = 9.2,
                    ["NetworkAdmin"] = 8.7,
                    ["DevOpsEngineer"] = 9.5
                };

                var performanceWorkflow = new AdvancedWorkflow<string, string>("PerformanceWorkflow")
                    .AddPerformanceBasedRouting("high_priority_task", teamAgents, performanceMetrics)
                    .WithMemory(new BasicMemory());

                var highPriorityTask = "Critical system issue needs immediate expert attention and resolution.";
                var performanceResult = await performanceWorkflow.ExecuteAsync(highPriorityTask);

                _console.WithColor(ConsoleColor.Green)
                    .WriteLine($"Task: {highPriorityTask}")
                    .WriteLine($"Routed to highest performing agent (DevOpsEngineer: 9.5)")
                    .WriteLine($"Result: {performanceResult}")
                    .WriteLine("")
                    .ResetColor();

                // 3. Round-Robin Routing
                _console.WriteLine("🔄 3. ROUND-ROBIN ROUTING");
                _console.WriteLine("Load balancing across team members");

                var roundRobinWorkflow = new AdvancedWorkflow<string, string>("RoundRobinWorkflow")
                    .AddRoundRobinRouting("load_balanced_task", teamAgents)
                    .WithMemory(new BasicMemory());

                var tasks = new[]
                {
                    "Review system logs for any anomalies.",
                    "Update documentation for recent changes.",
                    "Prepare monthly team status report."
                };

                foreach (var task in tasks)
                {
                    var result = await roundRobinWorkflow.ExecuteAsync(task);
                    _console.WithColor(ConsoleColor.Green)
                        .WriteLine($"Task: {task}")
                        .WriteLine($"Result: {result}")
                        .WriteLine("")
                        .ResetColor();
                }
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Error in Routing Strategies example: {ex.Message}")
                    .ResetColor();
            }
        }

        #endregion

        #region Interactive Demo Menu

        /// <summary>
        /// Interactive menu for selecting team orchestration examples
        /// </summary>
        public static async Task ExecutarMenuTeamOrchestration(IModel modelo)
        {
            while (true)
            {
                _console.WithColor(ConsoleColor.Cyan)
                    .WriteLine("\n🤖 TEAM ORCHESTRATION - Choose Example:")
                    .WriteLine("═══════════════════════════════════════════")
                    .ResetColor();
                _console.WriteLine("23. 🎯 Coordinate Mode - Development Team");
                _console.WriteLine("24. 🎯 Route Mode - Customer Service");
                _console.WriteLine("25. 🎯 Collaborate Mode - Research Team");
                _console.WriteLine("26. 🎯 Advanced Routing Strategies");
                _console.WriteLine("0. ← Return to Main Menu");
                _console.WriteLine("");

                Console.Write("Choose example (0, 23-26): ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "23":
                            await ExecutarExemploCoordinateMode(modelo);
                            break;
                        case "24":
                            await ExecutarExemploRouteMode(modelo);
                            break;
                        case "25":
                            await ExecutarExemploCollaborateMode(modelo);
                            break;
                        case "26":
                            await ExecutarExemploRoutingStrategies(modelo);
                            break;
                        case "0":
                            return;
                        default:
                            _console.WithColor(ConsoleColor.Red)
                                .WriteLine("❌ Invalid choice. Try again.")
                                .ResetColor();
                            continue;
                    }

                    Console.WriteLine("\nPress any key to continue...");
                    ConsoleHelper.SafeReadKey();
                }
                catch (Exception ex)
                {
                    _console.WithColor(ConsoleColor.Red)
                        .WriteLine($"❌ Error executing example: {ex.Message}")
                        .ResetColor();
                    
                    Console.WriteLine("Press any key to continue...");
                    ConsoleHelper.SafeReadKey();
                }
            }
        }

        #endregion
    }
}