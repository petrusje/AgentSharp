using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Tools;
using System.Collections.Generic;


namespace Arcana.AgentsNet.Models
{
  /// <summary>
  /// Represents a request to an AI language model.
  /// Contains the messages to be processed and tools that the model can use.
  /// </summary>
  /// <remarks>
  /// ModelRequest objects are passed to IModel implementations to generate responses.
  /// The request structure follows the standard chat completion format with messages
  /// and optional function calling capabilities through tools.
  /// </remarks>
  public class ModelRequest
  {
    /// <summary>
    /// Gets or sets the conversation messages to be processed by the model.
    /// Messages typically include system instructions, user inputs, and previous assistant responses.
    /// </summary>
    /// <remarks>
    /// Messages are processed in the order they appear in the list. The first message
    /// is typically a system message setting the context and behavior guidelines.
    /// </remarks>
    public List<AIMessage> Messages { get; set; } = new List<AIMessage>();

    /// <summary>
    /// Gets or sets the collection of tools that the model can use during response generation.
    /// </summary>
    /// <remarks>
    /// Tools enable the model to call external functions, retrieve information,
    /// or perform actions outside its context. The model may decide which tools
    /// to use based on the conversation and requirements.
    /// </remarks>
    public List<Tool> Tools { get; set; } = new List<Tool>();

    /// <summary>
    /// Adds a tool to the request's available tools collection.
    /// </summary>
    /// <param name="tool">The tool to add to the request</param>
    /// <remarks>
    /// This is a convenience method for adding a single tool to the Tools collection.
    /// Multiple tools can be added by calling this method repeatedly.
    /// </remarks>
    public void AddTool(Tool tool)
    {
      Tools.Add(tool);
    }
  }
}
