using System.Collections.Generic;
using Agents.net.Models;

namespace Agents.net.Core
{
    /// <summary>
    /// Represents the result of an agent execution, including data, message history,
    /// message count, and usage information.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the agent</typeparam>
    public class AgentResult<T>
    {
        /// <summary>
        /// Gets the data returned by the agent.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets the data returned by the agent (alias for Data property for backward compatibility).
        /// </summary>
        public T Value => Data;

        /// <summary>
        /// Gets the message history from the conversation.
        /// </summary>
        public List<AIMessage> Messages { get; }

        /// <summary>
        /// Gets the total count of messages in the conversation.
        /// </summary>
        public int MessageCount { get; }

        /// <summary>
        /// Gets information about token usage for this execution.
        /// </summary>
        public UsageInfo Usage { get; }

        /// <summary>
        /// Gets the list of tool results from the execution.
        /// </summary>
        public List<ToolResult> Tools { get; }

        /// <summary>
        /// Gets the reasoning content from the execution (step-by-step thought process).
        /// </summary>
        public string ReasoningContent { get; }

        /// <summary>
        /// Gets the structured reasoning steps from the execution.
        /// </summary>
        public List<ReasoningStep> ReasoningSteps { get; }

        /// <summary>
        /// Initializes a new instance of the AgentResult class with the specified parameters.
        /// </summary>
        /// <param name="data">The data returned by the agent</param>
        /// <param name="messages">The message history from the conversation</param>
        /// <param name="messageCount">The total count of messages in the conversation</param>
        /// <param name="usage">Information about token usage for this execution</param>
        /// <param name="tools">The list of tool results from the execution</param>
        /// <param name="reasoningContent">The step-by-step reasoning content</param>
        /// <param name="reasoningSteps">The structured reasoning steps</param>
        public AgentResult(T data, List<AIMessage> messages, int messageCount, UsageInfo usage, List<ToolResult> tools = null, string reasoningContent = null, List<ReasoningStep> reasoningSteps = null)
        {
            Data = data;
            Messages = messages;
            MessageCount = messageCount;
            Usage = usage;
            Tools = tools ?? new List<ToolResult>();
            ReasoningContent = reasoningContent;
            ReasoningSteps = reasoningSteps ?? new List<ReasoningStep>();
        }

        /// <summary>
        /// Gets all messages from the conversation history.
        /// </summary>
        /// <returns>The complete list of messages from the conversation</returns>
        public List<AIMessage> GetAllMessages() => Messages;
        
        /// <summary>
        /// Converte implicitamente o resultado para o tipo T, facilitando o uso
        /// </summary>
        /// <param name="result">O resultado a ser convertido</param>
        public static implicit operator T(AgentResult<T> result) => result.Data;
    }
}