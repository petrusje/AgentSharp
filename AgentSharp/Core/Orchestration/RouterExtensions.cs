using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Extension methods for advanced routing capabilities in workflows.
    /// Provides smart routing, capability-based selection, and performance-based routing.
    /// </summary>
    public static class RouterExtensions
    {
        /// <summary>
        /// Adds smart routing step that selects the best agent based on a custom selector function.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to add routing to</param>
        /// <param name="stepName">Name of the routing step</param>
        /// <param name="availableAgents">Array of agents available for selection</param>
        /// <param name="selector">Function to select the appropriate agent based on context</param>
        /// <param name="fallbackAgent">Agent to use if selector fails</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> AddSmartRouting<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            string stepName,
            IAgent[] availableAgents,
            Func<TContext, IAgent> selector,
            IAgent fallbackAgent = null)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (availableAgents == null || availableAgents.Length == 0)
                throw new ArgumentException("At least one agent is required", nameof(availableAgents));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            fallbackAgent = fallbackAgent ?? availableAgents[0];

            // Add routing decision step
            workflow.RegisterStep($"{stepName}_routing_decision", fallbackAgent,
                ctx =>
                {
                    try
                    {
                        var selectedAgent = selector(ctx);
                        var agentName = selectedAgent?.Name ?? fallbackAgent.Name;
                        workflow.Session?.UpdateState($"{stepName}_selected_agent", agentName);
                        return $"Route analysis complete. Selected agent: {agentName}";
                    }
                    catch (Exception ex)
                    {
                        workflow.Session?.UpdateState($"{stepName}_selected_agent", fallbackAgent.Name);
                        workflow.Session?.UpdateState($"{stepName}_routing_error", ex.Message);
                        return $"Routing failed, using fallback agent: {fallbackAgent.Name}. Error: {ex.Message}";
                    }
                },
                (ctx, output) => 
                {
                    // Store routing decision details
                    workflow.Session?.UpdateState($"{stepName}_routing_output", output);
                });

            // Add conditional execution steps for each agent
            foreach (var agent in availableAgents)
            {
                workflow.RegisterStep($"{stepName}_execute_via_{agent.Name}", agent,
                    ctx =>
                    {
                        var selectedAgentName = workflow.Session?.GetState<string>($"{stepName}_selected_agent");
                        return string.Equals(agent.Name, selectedAgentName, StringComparison.OrdinalIgnoreCase)
                            ? BuildTaskExecutionPrompt(ctx, agent)
                            : null; // Skip execution if not selected
                    },
                    (ctx, output) =>
                    {
                        if (!string.IsNullOrEmpty(output))
                        {
                            workflow.Session?.UpdateState($"{stepName}_final_result", output);
                            workflow.Session?.UpdateState($"{stepName}_executed_by", agent.Name);
                        }
                    });
            }

            return workflow;
        }

        /// <summary>
        /// Adds capability-based routing that matches tasks to agents based on their descriptions and capabilities.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to add routing to</param>
        /// <param name="stepName">Name of the routing step</param>
        /// <param name="availableAgents">Array of agents available for selection</param>
        /// <param name="capabilityKeywords">Optional dictionary mapping agents to their capability keywords</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> AddCapabilityBasedRouting<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            string stepName,
            IAgent[] availableAgents,
            Dictionary<string, string[]> capabilityKeywords = null)
        {
            return workflow.AddSmartRouting(stepName, availableAgents, ctx =>
            {
                var contextText = ctx?.ToString()?.ToLowerInvariant() ?? "";
                
                // If capability keywords are provided, use them for matching
                if (capabilityKeywords != null)
                {
                    var bestMatch = FindBestCapabilityMatch(contextText, availableAgents, capabilityKeywords);
                    if (bestMatch != null) return bestMatch;
                }

                // Fallback to description-based matching
                return FindBestDescriptionMatch(contextText, availableAgents);
            });
        }

        /// <summary>
        /// Adds performance-based routing that tracks agent performance and routes to the best performing agent.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to add routing to</param>
        /// <param name="stepName">Name of the routing step</param>
        /// <param name="availableAgents">Array of agents available for selection</param>
        /// <param name="performanceMetrics">Dictionary of agent performance scores (higher is better)</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> AddPerformanceBasedRouting<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            string stepName,
            IAgent[] availableAgents,
            Dictionary<string, double> performanceMetrics = null)
        {
            performanceMetrics = performanceMetrics ?? availableAgents.ToDictionary(a => a.Name, _ => 1.0);

            return workflow.AddSmartRouting(stepName, availableAgents, ctx =>
            {
                // Select agent with highest performance score
                var bestAgent = availableAgents
                    .OrderByDescending(a => performanceMetrics.TryGetValue(a.Name, out var value) ? value : 0.0)
                    .First();

                return bestAgent;
            });
        }

        /// <summary>
        /// Adds round-robin routing for load balancing across team members.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to add routing to</param>
        /// <param name="stepName">Name of the routing step</param>
        /// <param name="availableAgents">Array of agents available for selection</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> AddRoundRobinRouting<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            string stepName,
            IAgent[] availableAgents)
        {
            var selector = new ThreadSafeRoundRobinSelector();

            return workflow.AddSmartRouting(stepName, availableAgents, ctx =>
            {
                return selector.SelectNext(availableAgents);
            });
        }

        /// <summary>
        /// Adds conditional routing based on context evaluation.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to add routing to</param>
        /// <param name="stepName">Name of the routing step</param>
        /// <param name="routingRules">Dictionary of conditions and corresponding agents</param>
        /// <param name="defaultAgent">Default agent if no conditions match</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> AddConditionalRouting<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            string stepName,
            Dictionary<Func<TContext, bool>, IAgent> routingRules,
            IAgent defaultAgent)
        {
            if (routingRules == null || !routingRules.Any())
                throw new ArgumentException("At least one routing rule is required", nameof(routingRules));
            if (defaultAgent == null)
                throw new ArgumentNullException(nameof(defaultAgent));

            var allAgents = routingRules.Values.Concat(new[] { defaultAgent }).Distinct().ToArray();

            return workflow.AddSmartRouting(stepName, allAgents, ctx =>
            {
                // Evaluate conditions in order
                foreach (var rule in routingRules)
                {
                    try
                    {
                        if (rule.Key(ctx))
                        {
                            return rule.Value;
                        }
                    }
                    catch
                    {
                        // Continue to next rule if evaluation fails
                        continue;
                    }
                }

                // Return default agent if no conditions match
                return defaultAgent;
            });
        }

        #region Private Helper Methods

        private static string BuildTaskExecutionPrompt<TContext>(TContext context, IAgent agent)
        {
            return $@"You have been selected to execute this task based on your capabilities.

Your expertise: {agent.description}

Task Context: {context}

Please execute this task using your specialized knowledge and skills. Provide a comprehensive response.";
        }

        private static IAgent FindBestCapabilityMatch(string contextText, IAgent[] agents, Dictionary<string, string[]> capabilityKeywords)
        {
            var bestAgent = agents[0];
            var bestScore = 0;

            foreach (var agent in agents)
            {
                if (!capabilityKeywords.TryGetValue(agent.Name, out var keywords))
                    continue;

                var score = keywords.Count(keyword => 
                    contextText.ToLowerInvariant().Contains(keyword.ToLowerInvariant()));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAgent = agent;
                }
            }

            return bestAgent;
        }

        private static IAgent FindBestDescriptionMatch(string contextText, IAgent[] agents)
        {
            var bestAgent = agents[0];
            var bestScore = 0;

            foreach (var agent in agents)
            {
                var description = agent.description?.ToLowerInvariant() ?? "";
                var descriptionWords = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                var score = descriptionWords.Count(word => 
                    contextText.ToLowerInvariant().Contains(word.ToLowerInvariant()));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAgent = agent;
                }
            }

            return bestAgent;
        }

        #endregion
    }
}