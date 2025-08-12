using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using AgentSharp.Utils;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Extension methods to transform AdvancedWorkflow into team-based orchestration
    /// Provides team coordination, routing, and collaboration capabilities
    /// </summary>
    public static class TeamExtensions
    {
        /// <summary>
        /// Configures the workflow to operate as a team with specified agents and mode
        /// </summary>
        /// <typeparam name="TContext">The context type for the workflow</typeparam>
        /// <typeparam name="TResult">The result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to configure as a team</param>
        /// <param name="agents">Array of agents that form the team</param>
        /// <param name="mode">Team operation mode (Coordinate, Route, or Collaborate)</param>
        /// <returns>The configured workflow ready for team execution</returns>
        public static AdvancedWorkflow<TContext, TResult> AsTeam<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            IAgent[] agents,
            TeamMode mode)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (agents == null || agents.Length == 0)
                throw new ArgumentException("At least one agent is required for a team", nameof(agents));

            // Configure the workflow based on the selected team mode
            switch (mode)
            {
                case TeamMode.Coordinate:
                    return workflow.SetupCoordinateMode(agents);
                case TeamMode.Route:
                    return workflow.SetupRouteMode(agents);
                case TeamMode.Collaborate:
                    return workflow.SetupCollaborateMode(agents);
                default:
                    throw new ArgumentException($"Unknown team mode: {mode}", nameof(mode));
            }
        }

        /// <summary>
        /// Sets up coordinate mode where the first agent acts as coordinator
        /// managing task distribution to other team members
        /// </summary>
        private static AdvancedWorkflow<TContext, TResult> SetupCoordinateMode<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            IAgent[] agents)
        {
            var coordinator = agents[0];
            var members = agents.Skip(1).ToArray();

            // Add coordination step - coordinator plans and delegates work
            workflow.RegisterStep("coordinate_planning", coordinator,
                ctx => BuildCoordinatorPrompt(ctx, members),
                (ctx, output) => 
                {
                    // Store coordination plan in session
                    workflow.Session?.UpdateState("coordination_plan", output);
                });

            // Add delegation steps for each member
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var memberIndex = i;
                
                workflow.RegisterStep($"delegate_to_{member.Name}", member,
                    ctx => BuildDelegationPrompt(ctx, member, workflow.Session?.GetState<string>("coordination_plan")),
                    (ctx, output) => 
                    {
                        // Store member result in session
                        workflow.Session?.UpdateState($"member_{memberIndex}_result", output);
                    });
            }

            // Add final coordination step to aggregate results
            workflow.RegisterStep("coordinate_synthesis", coordinator,
                ctx => BuildSynthesisPrompt(ctx, members, workflow.Session),
                (ctx, output) => 
                {
                    // Store final synthesized result
                    workflow.Session?.UpdateState("team_final_result", output);
                });

            return workflow;
        }

        /// <summary>
        /// Sets up route mode where tasks are intelligently routed to the most suitable agent
        /// </summary>
        private static AdvancedWorkflow<TContext, TResult> SetupRouteMode<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            IAgent[] agents)
        {
            // Add routing step that selects the best agent for the task
            workflow.RegisterStep("intelligent_routing", agents[0], // Use first agent as router
                ctx => BuildRoutingPrompt(ctx, agents),
                (ctx, output) => 
                {
                    // Parse routing decision and store selected agent
                    var selectedAgentName = ExtractSelectedAgent(output, agents);
                    workflow.Session?.UpdateState("selected_agent", selectedAgentName);
                });

            // Add execution steps for each possible agent
            foreach (var agent in agents)
            {
                workflow.RegisterStep($"execute_via_{agent.Name}", agent,
                    ctx => ShouldExecuteAgent(agent, workflow.Session) 
                        ? BuildTaskPrompt(ctx) 
                        : null, // Skip if not selected
                    (ctx, output) => 
                    {
                        if (!string.IsNullOrEmpty(output))
                        {
                            workflow.Session?.UpdateState("routed_result", output);
                        }
                    });
            }

            return workflow;
        }

        /// <summary>
        /// Sets up collaborate mode where multiple agents work in parallel and results are synthesized
        /// </summary>
        private static AdvancedWorkflow<TContext, TResult> SetupCollaborateMode<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            IAgent[] agents)
        {
            // Add parallel collaboration step for each agent
            for (int i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                var agentIndex = i;
                
                workflow.RegisterStep($"collaborate_{agent.Name}", agent,
                    ctx => BuildCollaborationPrompt(ctx, agent, agents),
                    (ctx, output) => 
                    {
                        // Store each agent's contribution
                        workflow.Session?.UpdateState($"collaboration_{agentIndex}_result", output);
                    });
            }

            // Add synthesis step using the first agent as synthesizer
            workflow.RegisterStep("synthesize_collaboration", agents[0],
                ctx => BuildCollaborationSynthesisPrompt(ctx, agents, workflow.Session),
                (ctx, output) => 
                {
                    workflow.Session?.UpdateState("collaboration_final_result", output);
                });

            return workflow;
        }

        #region Helper Methods for Prompt Building

        private static string BuildCoordinatorPrompt<TContext>(TContext context, IAgent[] members)
        {
            var memberNames = string.Join(", ", members.Select(m => $"{m.Name} ({m.description})"));
            return $@"You are the team coordinator. Your team members are: {memberNames}
            
Your task is to analyze the following request and create a coordination plan:
Context: {context}

Please:
1. Break down the task into subtasks suitable for your team members
2. Assign specific subtasks to appropriate team members based on their capabilities
3. Define the expected deliverables from each member
4. Establish the order of execution if dependencies exist

Provide a clear coordination plan.";
        }

        private static string BuildDelegationPrompt<TContext>(TContext context, IAgent member, string coordinationPlan)
        {
            return $@"You are {member.Name}. The team coordinator has assigned you a specific task.

Original Context: {context}

Coordination Plan: {coordinationPlan}

Execute your assigned portion of the task. Focus on your area of expertise: {member.description}";
        }

        private static string BuildSynthesisPrompt<TContext>(TContext context, IAgent[] members, WorkflowSession session)
        {
            if (session == null) return "Please synthesize the team's work into a final result.";

            var memberResults = new List<string>();
            for (int i = 0; i < members.Length; i++)
            {
                var result = session.GetState<string>($"member_{i}_result");
                if (!string.IsNullOrEmpty(result))
                {
                    memberResults.Add($"{members[i].Name}: {result}");
                }
            }

            return $@"As the team coordinator, synthesize the work completed by your team members:

Original Context: {context}

Team Member Results:
{string.Join("\n", memberResults)}

Please provide a comprehensive final result that integrates all team member contributions.";
        }

        private static string BuildRoutingPrompt<TContext>(TContext context, IAgent[] agents)
        {
            var agentDescriptions = agents.Select(a => $"- {a.Name}: {a.description}").ToArray();
            
            return $@"You are an intelligent task router. Analyze the following task and select the most suitable agent.

Task Context: {context}

Available Agents:
{string.Join("\n", agentDescriptions)}

Based on the task requirements and agent capabilities, which agent is best suited for this task?
Respond with only the agent name from the list above.";
        }

        private static string ExtractSelectedAgent(string routingOutput, IAgent[] agents)
        {
            if (string.IsNullOrEmpty(routingOutput)) return agents[0].Name;

            // Try to find agent name in the routing output
            foreach (var agent in agents)
            {
                if (routingOutput.IndexOf(agent.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return agent.Name;
                }
            }

            // Fallback to first agent
            return agents[0].Name;
        }

        private static bool ShouldExecuteAgent(IAgent agent, WorkflowSession session)
        {
            if (session == null) return false;
            var selectedAgent = session.GetState<string>("selected_agent");
            return string.Equals(agent.Name, selectedAgent, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildTaskPrompt<TContext>(TContext context)
        {
            return $@"Execute the following task using your expertise:

Context: {context}

Please provide a comprehensive response based on your capabilities.";
        }

        private static string BuildCollaborationPrompt<TContext>(TContext context, IAgent agent, IAgent[] allAgents)
        {
            var teamNames = string.Join(", ", allAgents.Where(a => a != agent).Select(a => a.Name));
            
            return $@"You are {agent.Name}, working as part of a collaborative team with: {teamNames}

Task Context: {context}

Your expertise: {agent.description}

Provide your contribution to this collaborative effort. Focus on your area of expertise while considering how your work will integrate with your teammates' contributions.";
        }

        private static string BuildCollaborationSynthesisPrompt<TContext>(TContext context, IAgent[] agents, WorkflowSession session)
        {
            if (session == null) return "Please synthesize the collaborative work into a final result.";

            var contributions = new List<string>();
            for (int i = 0; i < agents.Length; i++)
            {
                var result = session.GetState<string>($"collaboration_{i}_result");
                if (!string.IsNullOrEmpty(result))
                {
                    contributions.Add($"{agents[i].Name} ({agents[i].description}): {result}");
                }
            }

            return $@"Synthesize the collaborative contributions from the team:

Original Context: {context}

Team Contributions:
{string.Join("\n\n", contributions)}

Please create a comprehensive final result that effectively combines and synthesizes all team member contributions into a cohesive response.";
        }

        #endregion
    }
}