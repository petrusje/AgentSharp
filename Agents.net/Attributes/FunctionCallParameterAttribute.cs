using System;

namespace Arcana.AgentsNet.Attributes
{
  /// <summary>
  /// Attribute for describing parameters of methods marked with FunctionCallAttribute.
  /// </summary>
  /// <remarks>
  /// Provides additional metadata about function parameters that helps the AI model
  /// understand how to correctly call the function with appropriate arguments.
  /// Multiple instances can be applied to a single method to describe each parameter.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public class FunctionCallParameterAttribute : Attribute
  {
    /// <summary>
    /// Gets the name of the parameter being described.
    /// </summary>
    /// <remarks>
    /// This must match the actual parameter name in the method signature.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the parameter that explains its purpose and expected values.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the FunctionCallParameterAttribute class.
    /// </summary>
    /// <param name="name">The name of the parameter (must match method parameter name)</param>
    /// <param name="description">The description of the parameter for the AI model</param>
    public FunctionCallParameterAttribute(string name, string description)
    {
      Name = name;
      Description = description;
    }
  }
}
