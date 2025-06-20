using System;

namespace Arcana.AgentsNet.Models
{
  /// <summary>
  /// Configuration for AI model execution
  /// </summary>
  [Serializable]
  public class ModelConfiguration
  {
    /// <summary>
    /// Name of the model to use (e.g., "gpt-4", "gpt-3.5-turbo")
    /// </summary>
    public string ModelName { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// System prompt that provides initial instructions to the model
    /// </summary>
    public string SystemPrompt { get; set; } = "You are a helpful AI assistant.";

    /// <summary>
    /// Text generation temperature (0.0 - 1.0).
    /// Controls the randomness of the responses. Lower values produce 
    /// more deterministic and focused results, while higher values produce
    /// more diverse and creative results.
    /// </summary>
    /// <example>
    /// // For more precise and factual responses
    /// config.Temperature = 0.2;
    /// 
    /// // For more creative responses 
    /// config.Temperature = 0.8;
    /// </example>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Maximum number of tokens to generate in the response.
    /// A token corresponds to approximately 4 characters in languages like English.
    /// Common values range from 256 to 4096, depending on the desired response complexity.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Penalty for repetitions (0.0 - 2.0).
    /// Higher values reduce the likelihood of the model repeating the same words
    /// or phrases. Useful to prevent repetition loops.
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Presence penalty (0.0 - 2.0).
    /// Higher values increase the likelihood of the model talking about new topics.
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Top-p (nucleus) sampling (0.0 - 1.0).
    /// Controls diversity by considering only tokens whose cumulative probability
    /// is less than the specified value. Lower values lead to more deterministic responses.
    /// </summary>
    /// <example>
    /// // For more focused and safer responses
    /// config.TopP = 0.3;
    /// 
    /// // For greater diversity
    /// config.TopP = 0.9;
    /// </example>
    public double TopP { get; set; } = 1.0;

    /// <summary>
    /// Timeout in seconds for model requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Enables structured output mode for models that support it.
    /// When enabled, the model will return responses in a structured format
    /// based on the provided ResponseSchema.
    /// </summary>
    /// <remarks>
    /// Currently supported by OpenAI models (GPT-4o and later).
    /// When enabled, ensure ResponseSchema is properly configured.
    /// </remarks>
    public bool EnableStructuredOutput { get; set; } = false;

    /// <summary>
    /// JSON schema for structured output responses.
    /// Defines the expected structure of the model's response when
    /// EnableStructuredOutput is true.
    /// </summary>
    /// <remarks>
    /// Should be a valid JSON schema string. For OpenAI models,
    /// this will be converted to the appropriate format for
    /// the Structured Outputs API.
    /// 
    /// Example:
    /// {
    ///   "type": "object",
    ///   "properties": {
    ///     "name": { "type": "string" },
    ///     "age": { "type": "integer" }
    ///   },
    ///   "required": ["name", "age"]
    /// }
    /// </remarks>
    public string ResponseSchema { get; set; }

    /// <summary>
    /// The expected .NET type for structured output deserialization.
    /// When set, the model response will attempt to deserialize
    /// the JSON output to this type.
    /// </summary>
    /// <remarks>
    /// This should be set to the Type you want the structured
    /// output to be deserialized to (e.g., typeof(MyResponseClass)).
    /// If null, the response will remain as JSON string.
    /// </remarks>
    public Type ResponseType { get; set; }

    /// <summary>
    /// Preserves backward compatibility com ModelId property
    /// </summary>
    [Obsolete("Use ModelName instead")]
    public string ModelId
    {
      get => ModelName;
      set => ModelName = value;
    }

    /// <summary>
    /// Merges configurations with another, prioritizing settings
    /// from the instance passed as a parameter.
    /// </summary>
    /// <param name="other">Configuration to be merged</param>
    /// <returns>New combined configuration</returns>
    /// <example>
    /// // Default agent configuration
    /// var defaultConfig = new ModelConfiguration { 
    ///     Temperature = 0.7, 
    ///     MaxTokens = 2048 
    /// };
    /// 
    /// // Configuration specific to a run
    /// var requestConfig = new ModelConfiguration { 
    ///     Temperature = 0.3 
    /// };
    /// 
    /// // Combine, with requestConfig having priority
    /// var finalConfig = defaultConfig.Merge(requestConfig);
    /// // finalConfig.Temperature == 0.3
    /// // finalConfig.MaxTokens == 2048
    /// </example>
    public ModelConfiguration Merge(ModelConfiguration other)
    {
      if (other == null)
        return this;

      return new ModelConfiguration
      {
        ModelName = other.ModelName ?? ModelName,
        SystemPrompt = other.SystemPrompt ?? SystemPrompt,
        Temperature = other.Temperature,
        MaxTokens = other.MaxTokens,
        FrequencyPenalty = other.FrequencyPenalty,
        PresencePenalty = other.PresencePenalty,
        TopP = other.TopP,
        TimeoutSeconds = other.TimeoutSeconds,
        EnableStructuredOutput = other.EnableStructuredOutput,
        ResponseSchema = other.ResponseSchema ?? ResponseSchema,
        ResponseType = other.ResponseType ?? ResponseType
      };
    }
  }
}
