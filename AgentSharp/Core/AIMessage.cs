namespace AgentSharp.Core
{
  /// <summary>
  /// Defines the possible roles for messages in a conversation with an AI model.
  /// </summary>
  /// <remarks>
  /// These roles follow the standard chat completion format used by most
  /// modern AI language models like GPT and Claude.
  /// </remarks>
  public enum Role
  {
    /// <summary>
    /// Represents a message from the user or end-client.
    /// </summary>
    User,

    /// <summary>
    /// Represents a system instruction that sets context and behavior guidelines.
    /// </summary>
    System,

    /// <summary>
    /// Represents a response from the AI assistant.
    /// </summary>
    Assistant
  }

  /// <summary>
  /// Represents a message in a conversation with an AI language model.
  /// </summary>
  /// <remarks>
  /// AIMessage is the fundamental unit of communication in the AgentSharp framework.
  /// It encapsulates the role of the speaker and the content of the message,
  /// following the standard format used by most AI model providers.
  /// </remarks>
  public class AIMessage
  {
    /// <summary>
    /// Gets or sets the role of the entity that created this message.
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Gets or sets the text content of the message.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Creates a new message with the User role.
    /// </summary>
    /// <param name="content">The message content</param>
    /// <returns>A new AIMessage with User role</returns>
    public static AIMessage User(string content) => new AIMessage { Role = Role.User, Content = content };

    /// <summary>
    /// Creates a new message with the System role.
    /// </summary>
    /// <param name="content">The message content, typically instructions or guidelines</param>
    /// <returns>A new AIMessage with System role</returns>
    /// <remarks>
    /// System messages are typically used to set the context and behavior of the AI.
    /// They provide instructions that guide how the AI should respond.
    /// </remarks>
    public static AIMessage System(string content) => new AIMessage { Role = Role.System, Content = content };

    /// <summary>
    /// Creates a new message with the Assistant role.
    /// </summary>
    /// <param name="content">The message content, typically an AI response</param>
    /// <returns>A new AIMessage with Assistant role</returns>
    public static AIMessage Assistant(string content) => new AIMessage { Role = Role.Assistant, Content = content };
  }

  /// <summary>
  /// Represents a message that contains the result of a tool invocation.
  /// </summary>
  /// <remarks>
  /// When an AI model uses a tool, this message type is used to send
  /// the result back to the model for further processing.
  /// This is part of the function calling mechanism that enables
  /// AI models to invoke external functionality.
  /// </remarks>
  public class ToolResultMessage : AIMessage
  {
    /// <summary>
    /// Gets or sets the unique identifier of the tool call that generated this result.
    /// </summary>
    /// <remarks>
    /// This ID is provided by the model in its tool call request and must be
    /// included with the result to correctly associate results with requests.
    /// </remarks>
    public string ToolCallId { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool that was invoked.
    /// </summary>
    public string ToolName { get; set; }

    /// <summary>
    /// Initializes a new instance of the ToolResultMessage class.
    /// </summary>
    /// <param name="toolCallId">Unique identifier of the tool call</param>
    /// <param name="toolName">Name of the invoked tool</param>
    /// <param name="content">The result of the tool execution as a string</param>
    public ToolResultMessage(string toolCallId, string toolName, string content)
    {
      Role = Role.Assistant;
      Content = content;
      ToolCallId = toolCallId;
      ToolName = toolName;
    }
  }
}
