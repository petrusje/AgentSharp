using System;
using AgentSharp.Core;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Fluent builder interface for configuring multiple agents
    /// </summary>
    public interface IAgentBuilder<TContext>
    {
        /// <summary>
        /// Adds an agent with expertise description
        /// </summary>
        IAgentBuilder<TContext> Add(string name, IAgent agent, string expertise);

        /// <summary>
        /// Sets an agent as the initial/default agent
        /// </summary>
        IAgentBuilder<TContext> SetInitial(string agentName);

        /// <summary>
        /// Configures an agent with advanced settings
        /// </summary>
        IAgentBuilder<TContext> Configure(string agentName, Action<TeamAgent> configure);
    }

    /// <summary>
    /// Implementation of the agent builder
    /// </summary>
    public class AgentBuilder<TContext> : IAgentBuilder<TContext>
    {
        private readonly TeamChat<TContext> _teamChat;

        public AgentBuilder(TeamChat<TContext> teamChat)
        {
            _teamChat = teamChat ?? throw new ArgumentNullException(nameof(teamChat));
        }

        public IAgentBuilder<TContext> Add(string name, IAgent agent, string expertise)
        {
            _teamChat.WithAgent(name, agent, expertise);
            return this;
        }

        public IAgentBuilder<TContext> SetInitial(string agentName)
        {
            // Mark the agent as initial by setting it as current
            if (_teamChat._agents.TryGetValue(agentName, out var agent))
            {
                _teamChat._currentAgent = agent;
            }
            return this;
        }

        public IAgentBuilder<TContext> Configure(string agentName, Action<TeamAgent> configure)
        {
            if (_teamChat._agents.TryGetValue(agentName, out var agent))
            {
                configure?.Invoke(agent);
            }
            return this;
        }
    }
}