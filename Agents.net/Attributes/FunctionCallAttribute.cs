using System;

namespace Agents.net.Attributes
{
  /// <summary>
  /// Attribute for marking a method as callable by AI models through function calling.
  /// </summary>
  /// <remarks>
  /// This attribute enables the AI model to invoke .NET methods directly. Methods decorated
  /// with this attribute will be exposed to the model as tools it can use during response
  /// generation, allowing for integration with external systems and data sources.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
  public class FunctionCallAttribute : Attribute
  {
    /// <summary>
    /// Gets the descriptive text that explains the purpose and behavior of this function.
    /// </summary>
    /// <remarks>
    /// This description is provided to the AI model to help it understand when and how
    /// to use this function during response generation.
    /// </remarks>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the FunctionCallAttribute class.
    /// </summary>
    /// <param name="description">The description of the function to be exposed to the AI model</param>
    public FunctionCallAttribute(string description)
    {
      Description = description;
    }
  }
}
