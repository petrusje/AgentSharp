using System;
using System.Text.RegularExpressions;

using Agents.net.Models;

namespace Agents.net.Utils
{
    /// <summary>
    /// Provides validation utilities for inputs used throughout the Agents.net library.
    /// </summary>
    /// <remarks>
    /// This utility class centralizes validation logic to ensure consistent
    /// validation across the library. It includes methods for validating
    /// API keys, endpoints, model configurations, and methods for sanitizing
    /// potentially unsafe inputs.
    /// </remarks>
    public static class InputValidator
    {
        /// <summary>
        /// Regular expression pattern used to validate API key formats.
        /// </summary>
        private static readonly Regex ApiKeyRegex = new Regex(@"^[a-zA-Z0-9_\-]{20,}$", RegexOptions.Compiled);
        
        /// <summary>
        /// Validates an API key against the expected format.
        /// </summary>
        /// <param name="apiKey">The API key to validate</param>
        /// <returns>True if the API key is valid; otherwise, false</returns>
        /// <remarks>
        /// A valid API key must be non-empty and match the pattern defined by ApiKeyRegex,
        /// which requires at least 20 alphanumeric characters, underscores, or hyphens.
        /// </remarks>
        public static bool IsValidApiKey(string apiKey)
        {
            return !string.IsNullOrWhiteSpace(apiKey) && ApiKeyRegex.IsMatch(apiKey);
        }
        
        /// <summary>
        /// Validates an endpoint URL.
        /// </summary>
        /// <param name="endpoint">The URL to validate</param>
        /// <returns>True if the URL is a valid HTTP or HTTPS URI; otherwise, false</returns>
        /// <remarks>
        /// This method ensures that the endpoint is a well-formed HTTP or HTTPS URL.
        /// It validates the URI format but does not check if the endpoint actually exists or is accessible.
        /// </remarks>
        public static bool IsValidEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return false;
                
            return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && 
                  (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        
        /// <summary>
        /// Validates model configuration parameters to ensure they are within acceptable ranges.
        /// </summary>
        /// <param name="config">The model configuration to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is outside its valid range</exception>
        /// <remarks>
        /// This method validates that all model parameters are within their valid ranges:
        /// - Temperature: between 0 and 2
        /// - MaxTokens: greater than 0
        /// - TopP: between 0 and 1
        /// - FrequencyPenalty and PresencePenalty: between 0 and 2
        /// </remarks>
        public static void ValidateModelConfig(ModelConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            if (config.Temperature < 0 || config.Temperature > 2.0)
                throw new ArgumentOutOfRangeException(nameof(config.Temperature), "Temperature must be between 0 and 2");
                
            if (config.MaxTokens <= 0)
                throw new ArgumentOutOfRangeException(nameof(config.MaxTokens), "MaxTokens must be greater than zero");
                
            if (config.TopP < 0 || config.TopP > 1.0)
                throw new ArgumentOutOfRangeException(nameof(config.TopP), "TopP must be between 0 and 1");
                
            if (config.FrequencyPenalty < 0 || config.FrequencyPenalty > 2.0)
                throw new ArgumentOutOfRangeException(nameof(config.FrequencyPenalty), "FrequencyPenalty must be between 0 and 2");
                
            if (config.PresencePenalty < 0 || config.PresencePenalty > 2.0)
                throw new ArgumentOutOfRangeException(nameof(config.PresencePenalty), "PresencePenalty must be between 0 and 2");
        }
        
        /// <summary>
        /// Sanitizes an input string to prevent injection attacks or malformed data.
        /// </summary>
        /// <param name="input">The string to sanitize</param>
        /// <returns>A sanitized version of the input string</returns>
        /// <remarks>
        /// This method removes potentially dangerous characters from the input string,
        /// keeping only alphanumeric characters, whitespace, common punctuation, and
        /// a limited set of special characters that are generally considered safe.
        /// 
        /// It's particularly important to sanitize inputs that will be used in tool calls
        /// or included in messages to AI models to prevent various forms of injection attacks.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Sanitize user input before using it
        /// string userInput = GetUserInput();
        /// string safeInput = InputValidator.SanitizeInput(userInput);
        /// 
        /// // Use the sanitized input in tool calls or model requests
        /// var result = myTool.Execute(safeInput);
        /// </code>
        /// </example>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            // Remove potentially dangerous characters
            return Regex.Replace(input, @"[^\w\s\-.,;:!?()[\]{}@#$%^&*+=|\\/<>'`~]", "");
        }
    }
}