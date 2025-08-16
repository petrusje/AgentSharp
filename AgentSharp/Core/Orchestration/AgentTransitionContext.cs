using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Rich context provided to agents for intelligent decision-making
    /// </summary>
    public class AgentTransitionContext<TContext>
    {
        /// <summary>
        /// Application-specific context data
        /// </summary>
        public TContext Data { get; set; }

        /// <summary>
        /// Global variables collection with full access
        /// </summary>
        public GlobalVariableCollection Variables { get; set; }

        /// <summary>
        /// Complete conversation history
        /// </summary>
        public List<ConversationMessage> History { get; set; } = new List<ConversationMessage>();

        /// <summary>
        /// Available agents in this conversation
        /// </summary>
        public string[] AvailableAgents { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Currently active agent name
        /// </summary>
        public string CurrentAgent { get; set; }

        /// <summary>
        /// Callback for agent to request transition to another agent
        /// </summary>
        public Action<string> OnTransition { get; set; }

        /// <summary>
        /// Callback for agent to mark conversation as complete
        /// </summary>
        public Action<string> OnComplete { get; set; }

        /// <summary>
        /// Additional metadata for context
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Requests transition to another agent
        /// </summary>
        public void TransitionTo(string agentName)
        {
            OnTransition?.Invoke(agentName);
        }

        /// <summary>
        /// Marks the conversation as complete
        /// </summary>
        public void CompleteConversation(string reason = null)
        {
            OnComplete?.Invoke(reason ?? "conversation_completed");
        }

        /// <summary>
        /// Gets the conversation progress
        /// </summary>
        public ConversationProgress GetProgress()
        {
            return Variables?.GetProgress() ?? new ConversationProgress();
        }

        /// <summary>
        /// Gets variables missing for current agent
        /// </summary>
        public List<GlobalVariable> GetMissingVariables()
        {
            return Variables?.GetMissingVariables(CurrentAgent) ?? new List<GlobalVariable>();
        }

        /// <summary>
        /// Gets all collected variables
        /// </summary>
        public List<GlobalVariable> GetCollectedVariables()
        {
            return Variables?.GetFilledVariables() ?? new List<GlobalVariable>();
        }
    }
}