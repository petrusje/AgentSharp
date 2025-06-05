using Agents.net.Attributes;
using Agents.net.Exceptions;
using Agents.net.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Agents.net.Tools
{

  public enum ToolKind
  {
    Function,
    Agent
  }

  /// <summary>
  /// Represents a tool that can be called by AI models through function calling.
  /// Tools encapsulate method information and handle parameter conversions from JSON.
  /// </summary>
  /// <remarks>
  /// The Tool class is the core of the function calling system in Agents.net.
  /// It wraps .NET methods and makes them available for AI models to invoke,
  /// handling parameter parsing, type conversion, and error management.
  /// </remarks>
  public class Tool
  {
    /// <summary>
    /// Gets or sets the name of the tool. This is the function name the AI model will use to call it.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a descriptive text about the tool's purpose and functionality.
    /// This helps the AI model decide when to use this tool.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the MethodInfo object that references the method to be invoked by this tool.
    /// </summary>
    public MethodInfo MethodInfo { get; set; }

    /// <summary>
    /// Gets or sets the object instance on which to invoke the method (null for static methods).
    /// </summary>
    public object Instance { get; set; }

    /// <summary>
    /// Gets or sets the JSON schema that describes the parameters this tool accepts.
    /// </summary>
    public string ParametersSchema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tool should be treated as a final result.
    /// When true, the execution flow typically ends after this tool is called.
    /// </summary>
    public bool IsFinalResultTool { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tool can be used in streaming mode.
    /// When false, the tool will only be available in non-streaming mode.
    /// </summary>
    public bool IsStreamable { get; set; }

    /// <summary>
    /// Gets or sets the type of the tool (Function or Agent).
    /// This is used to differentiate between different types of tools.
    /// </summary>
    public ToolKind ToolType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tool executes asynchronously.
    /// When true, the tool returns a Task that must be awaited.
    /// </summary>
    public bool IsAsync { get; protected set; }

    /// <summary>
    /// Maximum length limit for JSON arguments to prevent excessive memory usage or injection attacks.
    /// </summary>
    private const int MaxArgumentsLength = 10000;

    /// <summary>
    /// Initializes a new instance of the Tool class.
    /// </summary>
    /// <param name="name">The name of the tool</param>
    /// <param name="description">A description of what the tool does</param>
    /// <param name="methodInfo">The MethodInfo object representing the method to invoke</param>
    /// <param name="instance">The object instance on which to invoke the method</param>
    /// <param name="isStreamable">Whether this tool can be used in streaming mode</param>
    public Tool(string name, string description, MethodInfo methodInfo, object instance, bool isStreamable = false, ToolKind type = ToolKind.Function)
    {
      Name = name;
      Description = description;
      MethodInfo = methodInfo;
      Instance = instance;
      ToolType = type;
      IsAsync = false;
      IsFinalResultTool = false;
      IsStreamable = isStreamable;
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrEmpty(description))
        throw new ArgumentNullException(nameof(description));
      if (ToolType == ToolKind.Function)
      {
        if (methodInfo == null)
          throw new ArgumentNullException(nameof(methodInfo));
        if (instance == null)
          throw new ArgumentNullException(nameof(instance));

        ParametersSchema = GenerateParametersSchema();
        IsAsync = methodInfo.ReturnType == typeof(Task) ||
              (methodInfo.ReturnType.IsGenericType &&
               methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
      }
    }

    /// <summary>
    /// Invokes the tool with the given JSON arguments.
    /// </summary>
    /// <param name="argumentsJson">JSON string containing the arguments for the method call</param>
    /// <returns>The result of the method invocation</returns>
    /// <exception cref="ToolExecutionException">
    /// Thrown when the tool execution fails due to invalid arguments, method not found,
    /// or an error during execution.
    /// </exception>
    /// <remarks>
    /// This method handles parsing the JSON arguments, converting them to the appropriate
    /// types expected by the method, and invoking the method on the provided instance.
    /// It includes safety checks and automatic JSON format correction for common issues.
    /// </remarks>
    public object Execute(string argumentsJson)
    {
      if (IsAsync)
      {
        throw new InvalidOperationException($"This tool '{Name}' is asynchronous. Use ExecuteAsync instead.");
      }

      return Invoke(argumentsJson);
    }

    /// <summary>
    /// Invokes the tool with the given parameters dictionary.
    /// </summary>
    /// <param name="parameters">Dictionary containing the parameters for the method call</param>
    /// <returns>The result of the method invocation</returns>
    public object Execute(Dictionary<string, object> parameters)
    {
      if (IsAsync)
      {
        throw new InvalidOperationException($"This tool '{Name}' is asynchronous. Use ExecuteAsync instead.");
      }

      // Convert dictionary to JSON
      string argumentsJson = JsonSerializer.Serialize(parameters);
      return Invoke(argumentsJson);
    }

    /// <summary>
    /// Asynchronously invokes the tool with the given JSON arguments.
    /// </summary>
    /// <param name="argumentsJson">JSON string containing the arguments for the method call</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the result</returns>
    /// <exception cref="ToolExecutionException">
    /// Thrown when the tool execution fails due to invalid arguments, method not found,
    /// or an error during execution.
    /// </exception>
    public virtual async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
      if (!IsAsync)
      {
        // If tool is not async, run it synchronously but wrap the result
        var result = Execute(argumentsJson);
        return result?.ToString() ?? string.Empty;
      }

      try
      {
        var result = Invoke(argumentsJson);

        if (result is Task task)
        {
          // Handle cancellation
          if (cancellationToken.CanBeCanceled)
          {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
              var completedTask = await Task.WhenAny(task, tcs.Task);
              if (completedTask == tcs.Task)
              {
                throw new OperationCanceledException("Tool execution was canceled", cancellationToken);
              }
            }
          }
          else
          {
            await task;
          }

          // If it's a generic Task<T>
          if (task.GetType().IsGenericType)
          {
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty != null)
            {
              var taskResult = resultProperty.GetValue(task);
              return taskResult?.ToString() ?? string.Empty;
            }
          }

          return "Task completed successfully";
        }

        // This shouldn't happen if IsAsync is correctly set
        Logger.Warning($"Tool '{Name}' is marked as async but didn't return a Task");
        return result?.ToString() ?? string.Empty;
      }
      catch (Exception ex)
      {
        Logger.Error($"Error executing async tool {Name}", ex);
        throw new ToolExecutionException(Name, $"Error executing async tool: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Asynchronously invokes the tool with the given dictionary of parameters.
    /// </summary>
    /// <param name="parameters">Dictionary containing the parameters for the method call</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the result</returns>
    public virtual async Task<string> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
      // Convert parameters dictionary to JSON
      string argumentsJson = JsonSerializer.Serialize(parameters);
      return await ExecuteAsync(argumentsJson, cancellationToken);
    }

    /// <summary>
    /// Invokes the tool with the given JSON arguments.
    /// </summary>
    /// <param name="argumentsJson">JSON string containing the arguments for the method call</param>
    /// <returns>The result of the method invocation</returns>
    public object Invoke(string argumentsJson)
    {
      if (MethodInfo == null || Instance == null)
      {
        throw new ToolExecutionException(Name, "MethodInfo or Instance not defined for this tool.");
      }

      // Security validation for JSON arguments
      if (string.IsNullOrEmpty(argumentsJson))
      {
        argumentsJson = "{}";
      }
      else if (argumentsJson.Length > MaxArgumentsLength)
      {
        throw new ToolExecutionException(Name, $"Arguments exceed the maximum allowed size ({MaxArgumentsLength} characters)");
      }

      try
      {
        // Try to fix common malformed JSON (unquoted keys)
        argumentsJson = TryFixJsonFormat(argumentsJson);

        // Parse arguments and call method
        var arguments = CreateArguments(argumentsJson);

        Logger.Debug($"Invoking tool {Name} with arguments: {argumentsJson}");

        return MethodInfo.Invoke(Instance, arguments);
      }
      catch (JsonException ex)
      {
        Logger.Error($"Error parsing JSON for tool {Name}: '{argumentsJson}'", ex);
        throw new ToolExecutionException(Name, $"Invalid JSON for arguments: {ex.Message}", ex);
      }
      catch (TargetInvocationException ex)
      {
        // Extract inner exception
        Logger.Error($"Error executing tool {Name}", ex.InnerException ?? ex);
        throw new ToolExecutionException(Name, $"Error executing tool: {ex.InnerException?.Message ?? ex.Message}", ex.InnerException ?? ex);
      }
      catch (Exception ex)
      {
        Logger.Error($"Error invoking tool {Name}", ex);
        throw new ToolExecutionException(Name, $"Error invoking tool: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Attempts to fix common JSON formatting issues in model outputs using a robust, generic approach.
    /// </summary>
    /// <param name="json">The potentially malformed JSON string</param>
    /// <returns>Fixed JSON string if corrections were successful, otherwise the original string</returns>
    /// <remarks>
    /// This method uses a comprehensive approach to fix malformed JSON from AI models:
    /// 1. Tokenizes the content to identify keys and values
    /// 2. Applies proper JSON formatting rules generically
    /// 3. Handles complex cases like multiple parameters, mixed value types, etc.
    /// Examples: 
    /// - {key: value} -> {"key": "value"}
    /// - {ticker: CMIG4, periodo: 1M} -> {"ticker": "CMIG4", "periodo": "1M"}
    /// - {consulta:tecnologia Belo Horizonte} -> {"consulta": "tecnologia Belo Horizonte"}
    /// </remarks>
    private string TryFixJsonFormat(string json)
    {
      // If it's already valid JSON, return it
      try
      {
        JsonDocument.Parse(json);
        return json;
      }
      catch (JsonException)
      {
        // Continue with correction attempts
      }

      // If the string looks like a JSON object (starts with { and ends with })
      var trimmed = json.Trim();
      if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
      {
        return json; // Not a JSON object, can't fix
      }

      try
      {
        // Generic approach: Parse the content manually and rebuild as proper JSON
        var content = trimmed.Substring(1, trimmed.Length - 2).Trim(); // Remove { }

        if (string.IsNullOrEmpty(content))
        {
          return "{}"; // Empty object
        }

        var fixedPairs = new List<string>();
        var pairs = SplitJsonPairs(content);

        foreach (var pair in pairs)
        {
          var colonIndex = pair.IndexOf(':');
          if (colonIndex == -1) continue; // Invalid pair, skip

          var key = pair.Substring(0, colonIndex).Trim();
          var value = pair.Substring(colonIndex + 1).Trim();

          // Ensure key is properly quoted
          if (!key.StartsWith("\"") || !key.EndsWith("\""))
          {
            // Remove existing quotes if malformed and re-add
            key = key.Trim('"', '\'');
            key = "\"" + key + "\"";
          }

          // Ensure value is properly formatted
          value = FormatJsonValue(value);

          fixedPairs.Add(key + ": " + value);
        }

        var fixedJson = "{" + string.Join(", ", fixedPairs) + "}";

        // Validate the fixed JSON
        JsonDocument.Parse(fixedJson);
        Logger.Debug($"JSON automatically corrected: '{json}' -> '{fixedJson}'");
        return fixedJson;
      }
      catch (Exception ex)
      {
        Logger.Warning($"Generic JSON correction failed: {ex.Message}");
        return json; // Return original if all attempts fail
      }
    }

    /// <summary>
    /// Splits JSON content into key-value pairs, handling nested commas correctly.
    /// </summary>
    /// <param name="content">The content inside the JSON braces</param>
    /// <returns>List of key-value pair strings</returns>
    private List<string> SplitJsonPairs(string content)
    {
      var pairs = new List<string>();
      var current = new System.Text.StringBuilder();
      var depth = 0;
      var inQuotes = false;
      var quoteChar = '"';

      for (int i = 0; i < content.Length; i++)
      {
        var ch = content[i];

        if (!inQuotes)
        {
          if (ch == '"' || ch == '\'')
          {
            inQuotes = true;
            quoteChar = ch;
          }
          else if (ch == '{' || ch == '[')
          {
            depth++;
          }
          else if (ch == '}' || ch == ']')
          {
            depth--;
          }
          else if (ch == ',' && depth == 0)
          {
            // This comma separates pairs
            pairs.Add(current.ToString().Trim());
            current.Clear();
            continue;
          }
        }
        else
        {
          if (ch == quoteChar && (i == 0 || content[i - 1] != '\\'))
          {
            inQuotes = false;
          }
        }

        current.Append(ch);
      }

      // Add the last pair
      if (current.Length > 0)
      {
        pairs.Add(current.ToString().Trim());
      }

      return pairs;
    }

    /// <summary>
    /// Formats a JSON value according to its type, ensuring proper quoting.
    /// </summary>
    /// <param name="value">The raw value string</param>
    /// <returns>Properly formatted JSON value</returns>
    private string FormatJsonValue(string value)
    {
      if (string.IsNullOrEmpty(value))
        return "\"\"";

      value = value.Trim();

      // If already properly quoted, return as-is
      if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
          (value.StartsWith("'") && value.EndsWith("'")))
      {
        // Convert single quotes to double quotes if needed
        if (value.StartsWith("'") && value.EndsWith("'"))
        {
          return "\"" + value.Substring(1, value.Length - 2).Replace("\"", "\\\"") + "\"";
        }
        return value;
      }

      // Check if it's a number
      if (double.TryParse(value, out _))
      {
        return value; // Numbers don't need quotes
      }

      // Check if it's a boolean
      if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "false")
      {
        return value.ToLowerInvariant(); // Booleans don't need quotes
      }

      // Check if it's null
      if (value.ToLowerInvariant() == "null")
      {
        return "null"; // null doesn't need quotes
      }

      // Everything else should be treated as a string
      return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    /// <summary>
    /// Creates an array of arguments for method invocation from a JSON string.
    /// </summary>
    /// <param name="argumentsJson">JSON string containing the arguments</param>
    /// <returns>Array of objects to be passed as arguments to the method</returns>
    /// <exception cref="ToolExecutionException">Thrown when JSON parsing fails</exception>
    /// <remarks>
    /// This method matches JSON properties with method parameters by name,
    /// applies type conversion, and handles default values for missing parameters.
    /// </remarks>
    private object[] CreateArguments(string argumentsJson)
    {
      var parameters = MethodInfo.GetParameters();
      var arguments = new object[parameters.Length];

      if (!string.IsNullOrEmpty(argumentsJson))
      {
        JsonDocument jsonDocument;
        try
        {
          jsonDocument = JsonDocument.Parse(argumentsJson);
        }
        catch (JsonException ex)
        {
          Logger.Error($"Failed to parse JSON after correction attempts: {argumentsJson}", ex);
          throw new ToolExecutionException(Name, $"Invalid JSON for arguments: {ex.Message}", ex);
        }

        // Check if the root element is an object
        if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
        {
          Logger.Error($"Expected JSON object for tool arguments, got {jsonDocument.RootElement.ValueKind}: {argumentsJson}");

          // Handle special case where a single string parameter is passed as a simple value
          if (parameters.Length == 1 && jsonDocument.RootElement.ValueKind == JsonValueKind.String)
          {
            arguments[0] = ConvertValue(jsonDocument.RootElement, parameters[0].ParameterType);
            return arguments;
          }

          throw new ToolExecutionException(Name, $"Tool arguments must be a JSON object, got {jsonDocument.RootElement.ValueKind}");
        }

        for (int i = 0; i < parameters.Length; i++)
        {
          var parameter = parameters[i];
          if (jsonDocument.RootElement.TryGetProperty(parameter.Name, out var value))
          {
            arguments[i] = ConvertValue(value, parameter.ParameterType);
          }
          else
          {
            arguments[i] = parameter.HasDefaultValue ? parameter.DefaultValue : null;
          }
        }
      }

      return arguments;
    }

    /// <summary>
    /// Converts a JsonElement value to the target .NET type.
    /// </summary>
    /// <param name="value">The JsonElement to convert</param>
    /// <param name="targetType">The target .NET type</param>
    /// <returns>The converted value</returns>
    /// <exception cref="InvalidCastException">Thrown when conversion to the target type is not supported</exception>
    private static object ConvertValue(JsonElement value, Type targetType)
    {
      // Handle null case
      if (value.ValueKind == JsonValueKind.Null)
        return null;

      Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

      // Dictionary of type converters
      var typeConverters = new Dictionary<Type, Func<JsonElement, object>>
      {
          { typeof(int), element => element.TryGetInt32(out var result) ? result : default },
          { typeof(long), element => element.TryGetInt64(out var result) ? result : default },
          { typeof(double), element => element.TryGetDouble(out var result) ? result : default },
          { typeof(float), element => element.TryGetSingle(out var result) ? result : default },
          { typeof(decimal), element => element.TryGetDecimal(out var result) ? result : default },
          { typeof(bool), element => element.GetBoolean() },
          { typeof(string), element => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString() },
          { typeof(DateTime), element => element.TryGetDateTime(out var result) ? result : default }
      };

      // Simple type conversion
      if (typeConverters.TryGetValue(underlyingType, out var converter))
      {
        return converter(value);
      }

      // Handle arrays
      if (underlyingType.IsArray)
      {
        return ConvertArray(value, underlyingType);
      }

      // Handle generic collections like List<T>
      if (underlyingType.IsGenericType)
      {
        Type genericTypeDefinition = underlyingType.GetGenericTypeDefinition();
        if (genericTypeDefinition == typeof(List<>))
        {
          return ConvertList(value, underlyingType);
        }
      }

      // For any other types, try to deserialize using System.Text.Json
      try
      {
        return JsonSerializer.Deserialize(value.GetRawText(), targetType);
      }
      catch
      {
        throw new InvalidCastException($"Não é possível converter para o tipo '{targetType.FullName}'");
      }
    }

    private static object ConvertArray(JsonElement value, Type arrayType)
    {
      Type elementType = arrayType.GetElementType();
      if (value.ValueKind == JsonValueKind.Array)
      {
        var array = value.EnumerateArray().ToList();
        Array typedArray = Array.CreateInstance(elementType, array.Count);

        for (int i = 0; i < array.Count; i++)
        {
          typedArray.SetValue(ConvertValue(array[i], elementType), i);
        }

        return typedArray;
      }
      throw new InvalidOperationException($"Expected JSON array but found {value.ValueKind}.");
    }

    private static object ConvertList(JsonElement value, Type listType)
    {
      Type elementType = listType.GetGenericArguments()[0];
      var concreteListType = typeof(List<>).MakeGenericType(elementType);
      var list = Activator.CreateInstance(concreteListType);
      var addMethod = concreteListType.GetMethod("Add");

      if (value.ValueKind == JsonValueKind.Array)
      {
        foreach (var item in value.EnumerateArray())
        {
          addMethod.Invoke(list, new[] { ConvertValue(item, elementType) });
        }
      }

      return list;
    }

    /// <summary>
    /// Generates a JSON Schema that describes the parameters accepted by the tool.
    /// </summary>
    /// <returns>JSON Schema as a string</returns>
    /// <remarks>
    /// This schema follows the OpenAI function calling format and includes:
    /// - Property names and types
    /// - Description from FunctionCallParameter attributes
    /// - Required parameters list
    /// </remarks>
    protected string GenerateParametersSchema()
    {
      var parameters = MethodInfo.GetParameters();
      var memoryStream = new MemoryStream();
      var jsonWriter = new Utf8JsonWriter(memoryStream);

      try
      {
        jsonWriter.WriteStartObject();

        // Main object type
        jsonWriter.WriteString("type", "object");

        // Properties
        jsonWriter.WritePropertyName("properties");
        jsonWriter.WriteStartObject();

        // Get all FunctionCallParameter attributes from the method
        var paramAttrs = MethodInfo.GetCustomAttributes(typeof(FunctionCallParameterAttribute), false)
                         .Cast<FunctionCallParameterAttribute>()
                         .ToList(); // Compatible with C# 7.0

        foreach (var parameter in parameters)
        {
          jsonWriter.WritePropertyName(parameter.Name);
          jsonWriter.WriteStartObject();

          // Map .NET type to JSON type
          string jsonType = "string"; // Default

          if (parameter.ParameterType == typeof(int) ||
              parameter.ParameterType == typeof(long) ||
              parameter.ParameterType == typeof(double) ||
              parameter.ParameterType == typeof(float) ||
              parameter.ParameterType == typeof(decimal))
          {
            jsonType = "number";
          }
          else if (parameter.ParameterType == typeof(bool))
          {
            jsonType = "boolean";
          }

          jsonWriter.WriteString("type", jsonType);

          // Look for a FunctionCallParameter attribute corresponding to the parameter name
          var paramAttr = paramAttrs.FirstOrDefault(attr => attr.Name == parameter.Name);
          if (paramAttr != null)
          {
            jsonWriter.WriteString("description", paramAttr.Description);
          }

          jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject(); // Close properties

        // Required fields
        jsonWriter.WritePropertyName("required");
        jsonWriter.WriteStartArray();

        foreach (var parameter in parameters)
        {
          if (!parameter.HasDefaultValue)
          {
            jsonWriter.WriteStringValue(parameter.Name);
          }
        }

        jsonWriter.WriteEndArray(); // Close required
        jsonWriter.WriteEndObject(); // Close main object
        jsonWriter.Flush();

        return Encoding.UTF8.GetString(memoryStream.ToArray());
      }
      finally
      {
        jsonWriter.Dispose();
        memoryStream.Dispose();
      }
    }

    /// <summary>
    /// Creates a Tool instance from a method with FunctionCall attribute.
    /// </summary>
    /// <param name="method">The method to wrap as a tool</param>
    /// <param name="instance">The instance on which to invoke the method</param>
    /// <param name="attr">The FunctionCall attribute containing metadata</param>
    /// <returns>A configured Tool instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
    public static Tool FromMethod(MethodInfo method, object instance, FunctionCallAttribute attr)
    {
      if (method == null)
        throw new ArgumentNullException(nameof(method));
      if (instance == null)
        throw new ArgumentNullException(nameof(instance));
      if (attr == null)
        throw new ArgumentNullException(nameof(attr));

      return new Tool(method.Name, attr.Description, method, instance);
    }

    /// <summary>
    /// Creates a Tool instance from a method with FunctionCall attribute, with streaming configuration.
    /// </summary>
    /// <param name="method">The method to wrap as a tool</param>
    /// <param name="instance">The instance on which to invoke the method</param>
    /// <param name="attr">The FunctionCall attribute containing metadata</param>
    /// <param name="isStreamable">Whether this tool can be used in streaming mode</param>
    /// <returns>A configured Tool instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
    public static Tool FromMethod(MethodInfo method, object instance, FunctionCallAttribute attr, bool isStreamable)
    {
      if (method == null)
        throw new ArgumentNullException(nameof(method));
      if (instance == null)
        throw new ArgumentNullException(nameof(instance));
      if (attr == null)
        throw new ArgumentNullException(nameof(attr));

      return new Tool(method.Name, attr.Description, method, instance, isStreamable);
    }
  }
}