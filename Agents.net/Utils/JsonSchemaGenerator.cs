using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Utils
{
  /// <summary>
  /// Generates JSON schemas from .NET types for structured output configuration.
  /// </summary>
  /// <remarks>
  /// This class provides automatic JSON schema generation for use with structured outputs.
  /// It supports common .NET types and basic JSON schema features compatible with
  /// OpenAI's Structured Outputs API.
  /// </remarks>
  public static class JsonSchemaGenerator
  {
    /// <summary>
    /// Generates a JSON schema string from the specified .NET type.
    /// </summary>
    /// <param name="type">The .NET type to generate a schema for</param>
    /// <returns>JSON schema string compatible with structured outputs</returns>
    /// <exception cref="ArgumentNullException">Thrown when type is null</exception>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported for schema generation</exception>
    /// <example>
    /// <code>
    /// public class Person
    /// {
    ///     public string Name { get; set; }
    ///     public int Age { get; set; }
    /// }
    /// 
    /// var schema = JsonSchemaGenerator.GenerateSchema(typeof(Person));
    /// // Returns: {"type":"object","properties":{"Name":{"type":"string"},"Age":{"type":"integer"}},"required":["Name","Age"]}
    /// </code>
    /// </example>
    public static string GenerateSchema(Type type)
    {
      if (type == null)
        throw new ArgumentNullException(nameof(type));

      var schemaBuilder = new StringBuilder();
      GenerateSchemaInternal(type, schemaBuilder, new HashSet<Type>());
      return schemaBuilder.ToString();
    }

    /// <summary>
    /// Generates a JSON schema for the specified generic type T.
    /// </summary>
    /// <typeparam name="T">The type to generate a schema for</typeparam>
    /// <returns>JSON schema string</returns>
    public static string GenerateSchema<T>()
    {
      return GenerateSchema(typeof(T));
    }

    private static void GenerateSchemaInternal(Type type, StringBuilder builder, HashSet<Type> visitedTypes)
    {
      // Handle nullable types first (before checking visited types)
      if (IsNullableType(type))
      {
        type = Nullable.GetUnderlyingType(type);
      }

      // Handle primitive types (before checking visited types - primitives should always be handled directly)
      if (IsPrimitiveType(type))
      {
        builder.Append(GetPrimitiveSchema(type));
        return;
      }

      // Prevent infinite recursion for complex types only
      if (visitedTypes.Contains(type))
      {
        builder.Append("{\"type\":\"object\"}");
        return;
      }

      visitedTypes.Add(type);

      // Handle arrays and collections
      if (IsArrayOrCollection(type))
      {
        GenerateArraySchema(type, builder, visitedTypes);
        return;
      }

      // Handle object types
      GenerateObjectSchema(type, builder, visitedTypes);
    }

    private static bool IsNullableType(Type type)
    {
      return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static bool IsPrimitiveType(Type type)
    {
      return type == typeof(string) ||
             type == typeof(int) || type == typeof(long) || type == typeof(short) ||
             type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
             type == typeof(float) || type == typeof(double) || type == typeof(decimal) ||
             type == typeof(bool) ||
             type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
             type == typeof(Guid) ||
             type.IsEnum;
    }

    private static string GetPrimitiveSchema(Type type)
    {
      if (type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid))
        return "{\"type\":\"string\"}";

      if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
          type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort))
        return "{\"type\":\"integer\"}";

      if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        return "{\"type\":\"number\"}";

      if (type == typeof(bool))
        return "{\"type\":\"boolean\"}";

      if (type.IsEnum)
      {
        var enumValues = Enum.GetNames(type);
        var enumSchema = new StringBuilder();
        enumSchema.Append("{\"type\":\"string\",\"enum\":[");
        for (int i = 0; i < enumValues.Length; i++)
        {
          if (i > 0) enumSchema.Append(",");
          enumSchema.Append("\"").Append(enumValues[i]).Append("\"");
        }
        enumSchema.Append("]}");
        return enumSchema.ToString();
      }

      return "{\"type\":\"string\"}"; // fallback
    }

    private static bool IsArrayOrCollection(Type type)
    {
      return type.IsArray ||
             type.IsGenericType &&
              (type.GetGenericTypeDefinition() == typeof(List<>) ||
               type.GetGenericTypeDefinition() == typeof(IList<>) ||
               type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
               type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private static void GenerateArraySchema(Type type, StringBuilder builder, HashSet<Type> visitedTypes)
    {
      builder.Append("{\"type\":\"array\",\"items\":");

      Type elementType;
      if (type.IsArray)
      {
        elementType = type.GetElementType();
      }
      else
      {
        elementType = type.GetGenericArguments()[0];
      }

      GenerateSchemaInternal(elementType, builder, visitedTypes);
      builder.Append("}");
    }

    private static void GenerateObjectSchema(Type type, StringBuilder builder, HashSet<Type> visitedTypes)
    {
      builder.Append("{\"type\":\"object\",\"properties\":{");

      var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var requiredProperties = new List<string>();
      bool first = true;

      foreach (var property in properties)
      {
        // Skip properties that can't be read or written
        if (!property.CanRead || !property.CanWrite)
          continue;

        // Skip properties marked with JsonIgnore
        if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
          continue;

        if (!first)
          builder.Append(",");
        first = false;

        // Get property name (check for JsonPropertyName attribute)
        var jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        var propertyName = jsonPropertyName?.Name ?? property.Name;

        builder.Append("\"").Append(propertyName).Append("\":");
        GenerateSchemaInternal(property.PropertyType, builder, visitedTypes);

        // Check if property is required (not nullable)
        if (!IsNullableProperty(property))
        {
          requiredProperties.Add(propertyName);
        }
      }

      builder.Append("}");

      // Add required properties
      if (requiredProperties.Count > 0)
      {
        builder.Append(",\"required\":[");
        for (int i = 0; i < requiredProperties.Count; i++)
        {
          if (i > 0) builder.Append(",");
          builder.Append("\"").Append(requiredProperties[i]).Append("\"");
        }
        builder.Append("]");
      }

      builder.Append("}");
    }

    private static bool IsNullableProperty(PropertyInfo property)
    {
      var propertyType = property.PropertyType;

      // Check if it's a nullable value type
      if (IsNullableType(propertyType))
        return true;

      // Check if it's a reference type (always nullable in C# 7)
      if (!propertyType.IsValueType)
        return true;

      return false;
    }
  }
}
