using System.Collections.Generic;
using System.Threading;
using Agents.net.Models;

namespace Agents.net.Core
{
/// <summary>
/// Represents the context for an AI agent, containing all the necessary information
/// for the agent to process requests and generate responses.
/// </summary>
/// <typeparam name="T">The type of the context variable used by the agent</typeparam>
public class AgentContext<T>
{
    /// <summary>
    /// Gets or sets the typed context variable used by the agent.
    /// </summary>
    public T ContextVar { get; set; }

    /// <summary>
    /// Gets or sets the message history for the current conversation.
    /// </summary>
    public List<AIMessage> MessageHistory { get; set; }

    /// <summary>
    /// Gets the configuration settings for the model.
    /// </summary>
    public ModelConfiguration Config { get; }

    /// <summary>
    /// Gets the cancellation token that can be used to cancel operations.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new instance of the AgentContext class with the specified parameters.
    /// </summary>
    /// <param name="contextVar">The typed context variable for the agent</param>
    /// <param name="history">The message history for the conversation</param>
    /// <param name="config">The configuration for the model</param>
    /// <param name="cancellationToken">A token that can be used to cancel operations</param>
    public AgentContext(T contextVar, List<AIMessage> history, 
                        ModelConfiguration config, CancellationToken cancellationToken = default)
    {
        ContextVar = contextVar;
        MessageHistory = history ?? new List<AIMessage>();
        Config = config;
        CancellationToken = cancellationToken;
    }
    
    /// <summary>
    /// Creates a new AgentContext with updated context variable but preserving other settings
    /// </summary>
    /// <param name="newContextVar">The new context variable to use</param>
    /// <returns>A new AgentContext instance with the updated context variable</returns>
    public AgentContext<T> WithContextVar(T newContextVar)
    {
        ContextVar = newContextVar;
        return this;
    }
    
    /// <summary>
    /// Creates a new AgentContext with updated message history but preserving other settings
    /// </summary>
    /// <param name="newHistory">The new message history to use</param>
    /// <returns>A new AgentContext instance with the updated message history</returns>
    public AgentContext<T> WithMessageHistory(List<AIMessage> newHistory)
    {
        MessageHistory = newHistory;
        return this;
    }
}

/// <summary>
/// Non-generic implementation of AgentContext for simpler usage scenarios using Dictionary<string, object>.
/// </summary>
public class AgentContext : AgentContext<Dictionary<string, object>>
{
    /// <summary>
    /// Initializes a new instance of the non-generic AgentContext class.
    /// </summary>
    /// <param name="history">The message history for the conversation</param>
    /// <param name="config">The configuration for the model</param>
    /// <param name="cancellationToken">A token that can be used to cancel operations</param>
    public AgentContext(List<AIMessage> history, 
                      ModelConfiguration config, CancellationToken cancellationToken = default)
        : base(new Dictionary<string, object>(), history, config, cancellationToken)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the non-generic AgentContext class with a context object.
    /// </summary>
    /// <param name="contextObj">The context object for the agent</param>
    /// <param name="history">The message history for the conversation</param>
    /// <param name="config">The configuration for the model</param>
    /// <param name="cancellationToken">A token that can be used to cancel operations</param>
    public AgentContext(Dictionary<string, object> contextObj, List<AIMessage> history, 
                      ModelConfiguration config, CancellationToken cancellationToken = default)
        : base(contextObj ?? new Dictionary<string, object>(), history, config, cancellationToken)
    {
    }
}
}