using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Models.Interfaces
{
    /// <summary>
    /// Interface for Ollama client abstraction to enable dependency injection and testing
    /// </summary>
    public interface IOllamaClient : IDisposable
    {
        /// <summary>
        /// Gets the base URL of the Ollama server
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Generates a chat completion response from the Ollama model
        /// </summary>
        /// <param name="modelName">The name of the model to use</param>
        /// <param name="messages">The conversation messages</param>
        /// <param name="options">Optional generation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The completion response</returns>
        Task<OllamaResponse> GenerateCompletionAsync(
            string modelName,
            IEnumerable<OllamaMessage> messages,
            OllamaOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a streaming chat completion response from the Ollama model
        /// </summary>
        /// <param name="modelName">The name of the model to use</param>
        /// <param name="messages">The conversation messages</param>
        /// <param name="onChunk">Callback for each streaming chunk</param>
        /// <param name="options">Optional generation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The final completion response</returns>
        Task<OllamaResponse> GenerateStreamingCompletionAsync(
            string modelName,
            IEnumerable<OllamaMessage> messages,
            Action<string> onChunk = null,
            OllamaOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists available models on the Ollama server
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available models</returns>
        Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the Ollama server is running and accessible
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if server is accessible</returns>
        Task<bool> IsServerAccessibleAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a message in an Ollama conversation
    /// </summary>
    public class OllamaMessage
    {
        /// <summary>
        /// The role of the message sender (user, assistant, system)
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// Options for Ollama model generation
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Controls randomness in generation (0.0 to 1.0)
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Controls nucleus sampling (0.0 to 1.0)
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Stop sequences for generation
        /// </summary>
        public string[] Stop { get; set; }
    }

    /// <summary>
    /// Response from Ollama model generation
    /// </summary>
    public class OllamaResponse
    {
        /// <summary>
        /// The generated content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Whether the generation is complete
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// Number of tokens in the prompt
        /// </summary>
        public int? PromptEvalCount { get; set; }

        /// <summary>
        /// Number of tokens generated
        /// </summary>
        public int? EvalCount { get; set; }

        /// <summary>
        /// Time taken for prompt evaluation (nanoseconds)
        /// </summary>
        public long? PromptEvalDuration { get; set; }

        /// <summary>
        /// Time taken for generation (nanoseconds)
        /// </summary>
        public long? EvalDuration { get; set; }

        /// <summary>
        /// Total time taken (nanoseconds)
        /// </summary>
        public long? TotalDuration { get; set; }

        /// <summary>
        /// Gets the total token count
        /// </summary>
        public int TotalTokens => (PromptEvalCount ?? 0) + (EvalCount ?? 0);
    }
}