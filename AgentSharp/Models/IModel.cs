using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Models
{
  /// <summary>
  /// Interface for AI language model providers.
  /// Implementations connect to specific AI services like OpenAI, Azure OpenAI, etc.
  /// </summary>
  public interface IModel
  {
    /// <summary>
    /// Gets the unique name of the model being used.
    /// </summary>
    /// <remarks>
    /// This should match the model identifier used by the provider (e.g., "gpt-4", "claude-3", etc.)
    /// </remarks>
    string ModelName { get; }

    /// <summary>
    /// Generates a response based on the provided input and configuration.
    /// </summary>
    /// <param name="request">Request object containing messages and tools for the model to use</param>
    /// <param name="config">Model configuration parameters like temperature, max tokens, etc.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the model's response
    /// with generated content, tool calls, and usage statistics.
    /// </returns>

    /* Unmerged change from project 'AgentSharp (netstandard2.0)'
    Before:
            /// <exception cref="AgentSharp.Exceptions.ModelException">Thrown when the model fails to generate a response</exception>
            /// <exception cref="AgentSharp.Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
            Task<ModelResponse> GenerateResponseAsync(
    After:
            /// <exception cref="AgentSharp.Exceptions.ModelException">Thrown when the model fails to generate a response</exception>
            /// <exception cref="AgentSharp.Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
            Task<ModelResponse> GenerateResponseAsync(
    */
    /// <exception cref="Exceptions.ModelException">Thrown when the model fails to generate a response</exception>
    /// <exception cref="Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
    Task<ModelResponse> GenerateResponseAsync(
        ModelRequest request,
        ModelConfiguration config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming response for real-time processing of model output.
    /// </summary>
    /// <param name="request">Request object containing messages and tools for the model to use</param>
    /// <param name="config">Model configuration parameters like temperature, max tokens, etc.</param>
    /// <param name="handler">Callback action that processes each chunk of the generated content as it arrives</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the final model response
    /// after all streaming chunks have been processed.
    /// </returns>

    /* Unmerged change from project 'AgentSharp (netstandard2.0)'
    Before:
            /// <exception cref="AgentSharp.Exceptions.ModelException">Thrown when the model fails during streaming</exception>
            /// <exception cref="AgentSharp.Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
            /// <remarks>
    After:
            /// <exception cref="AgentSharp.Exceptions.ModelException">Thrown when the model fails during streaming</exception>
            /// <exception cref="AgentSharp.Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
            /// <remarks>
    */
    /// <exception cref="Exceptions.ModelException">Thrown when the model fails during streaming</exception>
    /// <exception cref="Exceptions.AuthorizationException">Thrown when there's an authentication or authorization error</exception>
    /// <remarks>
    /// This method allows for more interactive experiences where the model's output can be
    /// displayed to the user as it's generated, rather than waiting for the complete response.
    /// </remarks>
    Task<ModelResponse> GenerateStreamingResponseAsync(
        ModelRequest request,
        ModelConfiguration config,
        Action<string> handler,
        CancellationToken cancellationToken = default);
  }
}
