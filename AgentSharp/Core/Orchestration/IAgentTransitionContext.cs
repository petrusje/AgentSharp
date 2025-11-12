using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Interface for agent transition context used by ToolPacks
    /// </summary>
    public interface IAgentTransitionContext
    {
        /// <summary>
        /// Currently active agent name
        /// </summary>
        string CurrentAgent { get; }

        /// <summary>
        /// Available agents in this conversation
        /// </summary>
        string[] AvailableAgents { get; }

        /// <summary>
        /// Complete conversation history
        /// </summary>
        List<ConversationMessage> History { get; }

        /// <summary>
        /// Requests transition to another agent
        /// </summary>
        void TransitionTo(string agentName);

        /// <summary>
        /// Marks the conversation as complete
        /// </summary>
        void CompleteConversation(string reason = null);

        /// <summary>
        /// Gets the conversation progress
        /// </summary>
        ConversationProgress GetProgress();

        /// <summary>
        /// Gets variables missing for current agent
        /// </summary>
        List<GlobalVariable> GetMissingVariables();

        /// <summary>
        /// Gets all collected variables
        /// </summary>
        List<GlobalVariable> GetCollectedVariables();
    }
}