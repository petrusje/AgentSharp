using System;
using System.Collections.Generic;
using AgentSharp.Core;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Represents an agent in a TeamChat with expertise and capabilities
    /// </summary>
    public class TeamAgent
    {
        /// <summary>
        /// Unique agent name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Agent instance
        /// </summary>
        public IAgent Agent { get; set; }

        /// <summary>
        /// Agent's area of expertise and responsibilities
        /// </summary>
        public string Expertise { get; set; }

        /// <summary>
        /// Whether the agent is currently active and available
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Additional metadata for the agent
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Agent's priority level (1-10, with 10 being highest)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Timestamp when agent was last active
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Number of messages processed by this agent
        /// </summary>
        public int MessageCount { get; set; } = 0;

        public override string ToString()
        {
            return $"{Name}: {Expertise} (Active: {IsActive}, Priority: {Priority})";
        }
    }
}