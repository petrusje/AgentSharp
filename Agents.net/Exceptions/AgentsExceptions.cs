using System;

namespace Arcana.AgentsNet.Exceptions
{
  /// <summary>
  /// Base exception for all Agents.net library exceptions
  /// </summary>
  [Serializable]
  public class AgentsException : Exception
  {
    public AgentsException() { }
    public AgentsException(string message) : base(message) { }
    public AgentsException(string message, Exception inner) : base(message, inner) { }
  }

  /// <summary>
  /// Exception thrown when there is a problem with the AI model
  /// </summary>
  [Serializable]
  public class ModelException : AgentsException
  {
    public ModelException() { }
    public ModelException(string message) : base(message) { }
    public ModelException(string message, Exception inner) : base(message, inner) { }
  }

  /// <summary>
  /// Exception thrown when there is a problem with authorization or API key
  /// </summary>
  [Serializable]
  public class AuthorizationException : AgentsException
  {
    public AuthorizationException() { }
    public AuthorizationException(string message) : base(message) { }
    public AuthorizationException(string message, Exception inner) : base(message, inner) { }
  }

  /// <summary>
  /// Exception thrown when a tool call error occurs
  /// </summary>
  [Serializable]
  public class ToolExecutionException : AgentsException
  {
    public string ToolName { get; }

    public ToolExecutionException(string toolName)
    {
      ToolName = toolName;
    }

    public ToolExecutionException(string toolName, string message)
        : base(message)
    {
      ToolName = toolName;
    }

    public ToolExecutionException(string toolName, string message, Exception inner)
        : base(message, inner)
    {
      ToolName = toolName;
    }
  }
}
