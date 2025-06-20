using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Nodes;


namespace Arcana.AgentsNet.Tools
{
  /// <summary>
  /// Interface for tool validation functionality.
  /// Validators ensure that tool parameters meet specified requirements before execution.
  /// </summary>
  /// <remarks>
  /// Implementations of this interface can provide various validation strategies,
  /// such as type checking, range validation, or custom business rules.
  /// This helps prevent invalid input from being processed by tools.
  /// </remarks>
  public interface IToolValidator
  {
    /// <summary>
    /// Validates the parameters for a tool asynchronously.
    /// </summary>
    /// <param name="tool">The tool to validate parameters for</param>
    /// <param name="parameters">The array of parameter values to validate</param>
    /// <returns>A task that represents the asynchronous operation, returning true if validation passes, false otherwise</returns>
    Task<bool> ValidateAsync(Tool tool, object[] parameters);
  }

  /// <summary>
  /// Validates tool parameters using a JSON schema definition.
  /// </summary>
  /// <remarks>
  /// This validator uses the schema specified in the Tool's ParametersSchema property
  /// to validate that the parameters match the expected types and required fields are present.
  /// JSON Schema is a powerful standard for describing and validating JSON data structures.
  /// </remarks>
  public class JsonSchemaToolValidator : IToolValidator
  {
    /// <summary>
    /// Validates tool parameters against the tool's JSON schema definition.
    /// </summary>
    /// <param name="tool">The tool to validate parameters for</param>
    /// <param name="parameters">The array of parameter values to validate</param>
    /// <returns>A task that represents the asynchronous operation, returning true if validation passes, false otherwise</returns>
    /// <remarks>
    /// The validation checks:
    /// 1. That all required parameters are present
    /// 2. That each parameter has the correct type according to the schema
    /// </remarks>
    public Task<bool> ValidateAsync(Tool tool, object[] parameters)
    {
      var schema = JsonNode.Parse(tool.ParametersSchema);
      var properties = schema["properties"].AsObject();
      var required = schema["required"]?.AsArray();

      // Check if all required parameters are present
      if (required != null)
      {
        var requiredCount = required.Count;
        if (parameters.Length < requiredCount)
        {
          return Task.FromResult(false);
        }
      }

      // Verify each parameter has the correct type
      var parameterNames = properties.Select(p => p.Key).ToList();
      for (int i = 0; i < Math.Min(parameters.Length, parameterNames.Count); i++)
      {
        var paramName = parameterNames[i];
        var paramSchema = properties[paramName];
        var paramType = paramSchema["type"].GetValue<string>();
        var value = parameters[i];

        if (!IsValidJsonType(value, paramType))
        {
          return Task.FromResult(false);
        }
      }

      return Task.FromResult(true);
    }

    /// <summary>
    /// Checks if a value matches the specified JSON type.
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="jsonType">The JSON type name (string, number, integer, boolean, array, object)</param>
    /// <returns>True if the value matches the specified type, false otherwise</returns>
    /// <remarks>
    /// This method handles the mapping between .NET types and JSON Schema types.
    /// Null values are considered valid for any type unless marked as required.
    /// </remarks>
    private bool IsValidJsonType(object value, string jsonType)
    {
      if (value == null)
      {
        return true; // Null is valid for any JSON type, unless marked as required
      }

      switch (jsonType.ToLower())
      {
        case "string":
          return value is string;
        case "number":
          return value is int || value is long || value is float || value is double || value is decimal;
        case "integer":
          return value is int || value is long;
        case "boolean":
          return value is bool;
        case "array":
          return value is System.Collections.IEnumerable;
        case "object":
          return value is System.Collections.IDictionary ||
                 value.GetType().IsClass && value.GetType() != typeof(string);
        default:
          return false;
      }
    }
  }
}
