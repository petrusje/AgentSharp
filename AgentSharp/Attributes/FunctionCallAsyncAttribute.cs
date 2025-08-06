using System;

namespace AgentSharp.Attributes
{
  /// <summary>
  /// Attribute for marking an asynchronous method as callable by AI models through function calling.
  /// </summary>
  /// <remarks>
  /// Similar to FunctionCallAttribute, but specifically designed for async methods that return Task.
  /// This attribute enables the AI model to invoke asynchronous .NET methods directly, with the framework
  /// handling the await operation automatically.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
  public class FunctionCallAsyncAttribute : Attribute
  {
    /// <summary>
    /// Gets the descriptive text that explains the purpose and behavior of this async function.
    /// </summary>
    /// <remarks>
    /// This description is provided to the AI model to help it understand when and how
    /// to use this function during response generation.
    /// </remarks>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the FunctionCallAsyncAttribute class.
    /// </summary>
    /// <param name="description">The description of the async function to be exposed to the AI model</param>
    public FunctionCallAsyncAttribute(string description)
    {
      Description = description;
    }
  }
}
