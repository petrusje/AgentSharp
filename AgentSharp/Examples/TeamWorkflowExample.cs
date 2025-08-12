using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Tools;
using AgentSharp.Utils;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating team orchestration capabilities.
    /// Shows how to use different team modes, handoffs, and shared memory.
    /// </summary>
    public class TeamWorkflowExample
    {
        private readonly IModel _model;
        private readonly IMemoryManager _memoryManager;
        private readonly ILogger _logger;

        public TeamWorkflowExample(IModel model, IMemoryManager memoryManager, ILogger logger)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            _logger = logger ?? new ConsoleLogger();
        }

        /// <summary>
        /// Demonstrates coordinate mode where a project manager coordinates a development team.
        /// </summary>
        public async Task<string> RunDevelopmentTeamCoordinationExample()
        {
            _logger.Log(LogLevel.Info, "Starting Development Team Coordination Example");

            // Create specialized agents
            var projectManager = new Agent<ProjectContext, string>(_model, "ProjectManager", 
                "You are an experienced project manager. You coordinate development teams, break down complex requirements into tasks, and ensure project success.");

            var backendDeveloper = new Agent<ProjectContext, string>(_model, "BackendDeveloper", 
                "You are a senior backend developer. You design APIs, databases, and server-side logic using best practices.");

            var frontendDeveloper = new Agent<ProjectContext, string>(_model, "FrontendDeveloper", 
                "You are a skilled frontend developer. You create user interfaces, ensure responsive design, and implement user experience best practices.");

            var qaEngineer = new Agent<ProjectContext, string>(_model, "QAEngineer", 
                "You are a quality assurance engineer. You design test strategies, create test cases, and ensure product quality.");

            var teamAgents = new[] { projectManager, backendDeveloper, frontendDeveloper, qaEngineer };

            // Create team handoff tools
            var handoffTools = new TeamHandoffToolPack(teamAgents, _memoryManager, "dev_team_001");

            // Create development team workflow in coordinate mode
            var devTeamWorkflow = new AdvancedWorkflow<ProjectContext, string>("Development Team", _logger)
                .WithUserId("team_lead")
                .WithDebugMode(true)
                .AsTeam(teamAgents, TeamMode.Coordinate);

            // Add handoff capabilities to all agents
            foreach (var agent in teamAgents)
            {
                agent.WithTools(handoffTools);
            }

            // Test context
            var projectContext = new ProjectContext
            {
                ProjectName = "E-commerce Platform",
                Requirements = "Build a modern e-commerce platform with user authentication, product catalog, shopping cart, and payment processing",
                Timeline = "8 weeks",
                Budget = "$50,000",
                TeamSize = 4
            };

            var result = await devTeamWorkflow.ExecuteAsync(projectContext);
            
            _logger.Log(LogLevel.Info, "Development Team Coordination completed");
            return result.ToString();
        }

        /// <summary>
        /// Demonstrates route mode where customer service requests are intelligently routed.
        /// </summary>
        public async Task<string> RunCustomerServiceRoutingExample()
        {
            _logger.Log(LogLevel.Info, "Starting Customer Service Routing Example");

            // Create specialized customer service agents
            var technicalSupport = new Agent<CustomerIssueContext, string>(_model, "TechnicalSupport", 
                "You are a technical support specialist. You resolve software issues, troubleshoot technical problems, and provide technical guidance.");

            var billingSupport = new Agent<CustomerIssueContext, string>(_model, "BillingSupport", 
                "You are a billing support specialist. You handle payment issues, subscription changes, and billing inquiries.");

            var generalSupport = new Agent<CustomerIssueContext, string>(_model, "GeneralSupport", 
                "You are a general customer support representative. You handle general inquiries, account issues, and provide product information.");

            var supportTeam = new[] { technicalSupport, billingSupport, generalSupport };

            // Create routing workflow
            var customerServiceWorkflow = new AdvancedWorkflow<CustomerIssueContext, string>("Customer Service", _logger)
                .WithUserId("customer_service")
                .WithDebugMode(true)
                // Shared memory handled by extensions
                .AsTeam(supportTeam, TeamMode.Route);

            // Add handoff tools
            var csHandoffTools = new TeamHandoffToolPack(supportTeam, _memoryManager, "cs_team_001");
            foreach (var agent in supportTeam)
            {
                agent.WithTools(csHandoffTools);
            }

            // Test different types of customer issues
            var testIssues = new[]
            {
                new CustomerIssueContext
                {
                    CustomerName = "John Doe",
                    Issue = "My application keeps crashing when I try to upload files",
                    Priority = "High",
                    Category = "Technical"
                },
                new CustomerIssueContext
                {
                    CustomerName = "Jane Smith",
                    Issue = "I was charged twice for my subscription this month",
                    Priority = "Medium",
                    Category = "Billing"
                },
                new CustomerIssueContext
                {
                    CustomerName = "Mike Johnson",
                    Issue = "I need information about your premium features",
                    Priority = "Low",
                    Category = "General"
                }
            };

            var results = new List<string>();
            foreach (var issue in testIssues)
            {
                var result = await customerServiceWorkflow.ExecuteAsync(issue);
                results.Add($"Issue: {issue.Issue}\nResolution: {result}\n");
            }

            return string.Join("\n---\n", results);
        }

        /// <summary>
        /// Demonstrates collaborate mode where research agents work together on a complex analysis.
        /// </summary>
        public async Task<string> RunResearchTeamCollaborationExample()
        {
            _logger.Log(LogLevel.Info, "Starting Research Team Collaboration Example");

            // Create research team agents
            var marketResearcher = new Agent<ResearchContext, string>(_model, "MarketResearcher", 
                "You are a market research analyst. You analyze market trends, competitor analysis, and consumer behavior.");

            var dataAnalyst = new Agent<ResearchContext, string>(_model, "DataAnalyst", 
                "You are a data analyst. You analyze quantitative data, create statistical models, and extract insights from data sets.");

            var industryExpert = new Agent<ResearchContext, string>(_model, "IndustryExpert", 
                "You are an industry domain expert. You provide deep industry knowledge, regulatory insights, and strategic perspectives.");

            var researchTeam = new[] { marketResearcher, dataAnalyst, industryExpert };

            // Create collaboration workflow
            var researchWorkflow = new AdvancedWorkflow<ResearchContext, string>("Research Team", _logger)
                .WithUserId("research_lead")
                .WithDebugMode(true)
                // Shared memory handled by extensions
                .AsTeam(researchTeam, TeamMode.Collaborate);

            // Add team tools
            var researchHandoffTools = new TeamHandoffToolPack(researchTeam, _memoryManager, "research_team_001");
            foreach (var agent in researchTeam)
            {
                agent.WithTools(researchHandoffTools);
            }

            // Research context
            var researchContext = new ResearchContext
            {
                Topic = "AI-Powered Healthcare Solutions Market Analysis",
                Scope = "North American market, focusing on diagnostic imaging and patient monitoring solutions",
                Timeline = "Q1 2024 analysis with 5-year projections",
                Budget = "$25,000",
                Stakeholders = new[] { "Healthcare CIOs", "Technology Vendors", "Regulatory Bodies", "Patients" }
            };

            var result = await researchWorkflow.ExecuteAsync(researchContext);
            
            _logger.Log(LogLevel.Info, "Research Team Collaboration completed");
            return result.ToString();
        }

        /// <summary>
        /// Demonstrates advanced routing with custom capability matching.
        /// </summary>
        public async Task<string> RunAdvancedRoutingExample()
        {
            _logger.Log(LogLevel.Info, "Starting Advanced Routing Example");

            // Create specialized agents
            var securityExpert = new Agent<SecurityTaskContext, string>(_model, "SecurityExpert", 
                "You are a cybersecurity expert. You analyze security threats, design security protocols, and provide security recommendations.");

            var complianceSpecialist = new Agent<SecurityTaskContext, string>(_model, "ComplianceSpecialist", 
                "You are a compliance specialist. You ensure regulatory compliance, audit requirements, and policy adherence.");

            var riskAnalyst = new Agent<SecurityTaskContext, string>(_model, "RiskAnalyst", 
                "You are a risk analyst. You assess business risks, create risk models, and recommend mitigation strategies.");

            var securityTeam = new[] { securityExpert, complianceSpecialist, riskAnalyst };

            // Create workflow with advanced routing
            var securityWorkflow = new AdvancedWorkflow<SecurityTaskContext, string>("Security Team", _logger)
                .WithUserId("security_lead")
                .WithDebugMode(true);
                // Shared memory handled by extensions

            // Add capability-based routing
            var capabilityKeywords = new Dictionary<string, string[]>
            {
                ["SecurityExpert"] = new[] { "vulnerability", "threat", "attack", "security", "breach", "malware", "firewall" },
                ["ComplianceSpecialist"] = new[] { "compliance", "audit", "regulation", "policy", "gdpr", "hipaa", "sox" },
                ["RiskAnalyst"] = new[] { "risk", "assessment", "probability", "impact", "mitigation", "analysis", "evaluation" }
            };

            securityWorkflow.AddCapabilityBasedRouting("security_routing", securityTeam, capabilityKeywords);

            // Test different security tasks
            var securityTasks = new[]
            {
                new SecurityTaskContext
                {
                    Task = "Assess vulnerability in our web application after recent penetration testing",
                    Priority = "Critical",
                    Deadline = DateTime.Now.AddDays(2)
                },
                new SecurityTaskContext
                {
                    Task = "Prepare for upcoming SOX audit and ensure compliance with financial regulations",
                    Priority = "High",
                    Deadline = DateTime.Now.AddDays(30)
                },
                new SecurityTaskContext
                {
                    Task = "Evaluate the risk impact of migrating to cloud infrastructure",
                    Priority = "Medium",
                    Deadline = DateTime.Now.AddDays(14)
                }
            };

            var results = new List<string>();
            foreach (var task in securityTasks)
            {
                var result = await securityWorkflow.ExecuteAsync(task);
                results.Add($"Task: {task.Task}\nResult: {result}\n");
            }

            return string.Join("\n---\n", results);
        }

        /// <summary>
        /// Demonstrates workflow with handoff chains and complex team interactions.
        /// </summary>
        public async Task<string> RunComplexHandoffChainExample()
        {
            _logger.Log(LogLevel.Info, "Starting Complex Handoff Chain Example");

            // Create a sales and support pipeline team
            var salesAgent = new Agent<SalesContext, string>(_model, "SalesAgent", 
                "You are a sales representative. You qualify leads, present solutions, and close deals.");

            var solutionArchitect = new Agent<SalesContext, string>(_model, "SolutionArchitect", 
                "You are a solution architect. You design technical solutions, create implementation plans, and provide technical expertise.");

            var implementationManager = new Agent<SalesContext, string>(_model, "ImplementationManager", 
                "You are an implementation manager. You manage project delivery, coordinate resources, and ensure successful deployments.");

            var customerSuccess = new Agent<SalesContext, string>(_model, "CustomerSuccess", 
                "You are a customer success manager. You ensure customer satisfaction, drive adoption, and manage relationships.");

            var salesTeam = new[] { salesAgent, solutionArchitect, implementationManager, customerSuccess };

            // Create handoff tools with the full team
            var salesHandoffTools = new TeamHandoffToolPack(salesTeam, _memoryManager, "sales_pipeline_001");

            // Configure each agent with handoff capabilities
            foreach (var agent in salesTeam)
            {
                agent.WithTools(salesHandoffTools);
            }

            // Create a simple workflow that starts with sales
            var salesWorkflow = new AdvancedWorkflow<SalesContext, string>("Sales Pipeline", _logger)
                .WithUserId("sales_lead")
                .WithDebugMode(true)
                // Shared memory handled by extensions
                .RegisterStep("initial_qualification", salesAgent,
                    ctx => $"Qualify this lead: {ctx.LeadInfo}. If qualified, hand off to solution architect for technical assessment.",
                    (ctx, output) => { /* Store qualification result */ });

            // Sales context representing a qualified lead
            var salesContext = new SalesContext
            {
                LeadInfo = "Enterprise client (500+ employees) looking for CRM solution with advanced analytics and integration capabilities. Budget: $100K annually.",
                CompanyName = "TechCorp Industries",
                ContactPerson = "Sarah Johnson, CTO",
                Requirements = new[] { "CRM", "Analytics", "Integrations", "Mobile Support", "Advanced Reporting" },
                Timeline = "3 months implementation",
                Budget = 100000
            };

            var result = await salesWorkflow.ExecuteAsync(salesContext);
            
            _logger.Log(LogLevel.Info, "Complex Handoff Chain completed");
            return result.ToString();
        }
    }

    #region Context Classes

    public class ProjectContext
    {
        public string ProjectName { get; set; }
        public string Requirements { get; set; }
        public string Timeline { get; set; }
        public string Budget { get; set; }
        public int TeamSize { get; set; }
        
        public override string ToString()
        {
            return $"Project: {ProjectName}, Requirements: {Requirements}, Timeline: {Timeline}, Budget: {Budget}, Team Size: {TeamSize}";
        }
    }

    public class CustomerIssueContext
    {
        public string CustomerName { get; set; }
        public string Issue { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        
        public override string ToString()
        {
            return $"Customer: {CustomerName}, Issue: {Issue}, Priority: {Priority}, Category: {Category}";
        }
    }

    public class ResearchContext
    {
        public string Topic { get; set; }
        public string Scope { get; set; }
        public string Timeline { get; set; }
        public string Budget { get; set; }
        public string[] Stakeholders { get; set; }
        
        public override string ToString()
        {
            return $"Research Topic: {Topic}, Scope: {Scope}, Timeline: {Timeline}, Budget: {Budget}, Stakeholders: [{string.Join(", ", Stakeholders)}]";
        }
    }

    public class SecurityTaskContext
    {
        public string Task { get; set; }
        public string Priority { get; set; }
        public DateTime Deadline { get; set; }
        
        public override string ToString()
        {
            return $"Security Task: {Task}, Priority: {Priority}, Deadline: {Deadline:yyyy-MM-dd}";
        }
    }

    public class SalesContext
    {
        public string LeadInfo { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string[] Requirements { get; set; }
        public string Timeline { get; set; }
        public decimal Budget { get; set; }
        
        public override string ToString()
        {
            return $"Lead: {LeadInfo}, Company: {CompanyName}, Contact: {ContactPerson}, Requirements: [{string.Join(", ", Requirements)}], Timeline: {Timeline}, Budget: ${Budget:N0}";
        }
    }

    #endregion
}