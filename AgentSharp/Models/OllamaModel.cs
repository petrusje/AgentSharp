using AgentSharp.Core;
using AgentSharp.Exceptions;
using AgentSharp.Models.Interfaces;
using AgentSharp.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Models
{
    /// <summary>
    /// Implementation of the IModel interface for Ollama's language models using dependency injection.
    /// </summary>
    /// <remarks>
    /// This class provides integration with Ollama's local language models through an injected client.
    /// It supports both standard completions and streaming responses.
    ///
    /// The implementation handles:
    /// - Dependency injection for IOllamaClient abstraction
    /// - Streaming responses for real-time output
    /// - Error handling and retries for resilient operation
    /// - Token usage tracking based on Ollama response data
    /// 
    /// Note: Requires IOllamaClient implementation to be registered in DI container.
    /// The Ollama client is only loaded when this model is actually used.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register IOllamaClient in your DI container
    /// services.AddSingleton&lt;IOllamaClient&gt;(provider => new OllamaSharpClient("http://localhost:11434"));
    /// 
    /// // Create model with injected client
    /// var model = new OllamaModel("llama2", ollamaClient);
    /// </code>
    /// </example>
    public class OllamaModel : ModelBase
    {
        private readonly IOllamaClient _client;
        private readonly string _modelName;

        // Static telemetry service for global instrumentation
        private static ITelemetryService _globalTelemetry;

        /// <summary>
        /// Configures global telemetry service for all OllamaModel instances
        /// </summary>
        /// <param name="telemetry">Telemetry service to use globally</param>
        public static void ConfigureGlobalTelemetry(ITelemetryService telemetry)
        {
            _globalTelemetry = telemetry;
        }

        /// <summary>
        /// Initializes a new instance of the OllamaModel class with dependency injection.
        /// </summary>
        /// <param name="modelName">The name of the Ollama model to use (e.g., "llama2", "mistral", "codellama")</param>
        /// <param name="client">The injected IOllamaClient implementation</param>
        /// <exception cref="ArgumentException">Thrown if the model name is invalid</exception>
        /// <exception cref="ArgumentNullException">Thrown if the client is null</exception>
        /// <exception cref="ModelException">Thrown if there's an error initializing the Ollama client</exception>
        public OllamaModel(string modelName, IOllamaClient client) : base(modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name is required", nameof(modelName));

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _modelName = modelName;

            try
            {
                Logger.Info($"OllamaModel initialized with model {modelName} using injected client at {_client.BaseUrl}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing OllamaModel", ex);
                throw new ModelException("Failed to initialize Ollama model", ex);
            }
        }

        /// <summary>
        /// Generates a response from the Ollama model based on the provided request.
        /// </summary>
        /// <param name="request">The model request containing messages and tools</param>
        /// <param name="config">Configuration parameters for the model</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The model's response</returns>
        /// <exception cref="ModelException">Thrown if there's an error during model execution</exception>
        public override async Task<ModelResponse> GenerateResponseAsync(ModelRequest request, ModelConfiguration config = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Track operation start
                var operationId = $"ollama_generate_{_modelName}";
                _globalTelemetry?.StartOperation(operationId);

                // Convert request messages to Ollama format
                var ollamaMessages = ConvertToOllamaMessages(request);
                var options = ConvertToOllamaOptions(config);

                // Call injected Ollama client
                var ollamaResponse = await _client.GenerateCompletionAsync(
                    _modelName, 
                    ollamaMessages, 
                    options, 
                    cancellationToken);

                // Convert response back to framework format
                var response = ConvertToModelResponse(ollamaResponse);

                // Track completion
                var elapsedSeconds = _globalTelemetry?.EndOperation(operationId) ?? 0;
                _globalTelemetry?.CompleteLLMRequest(operationId, response.Usage.TotalTokens);

                Logger.Info($"OllamaModel generated response in {elapsedSeconds:F2}s with {response.Usage.TotalTokens} tokens");

                return response;
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Ollama generation operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in Ollama GenerateResponseAsync", ex);
                throw new ModelException($"Error generating response from Ollama: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a streaming response from the Ollama model.
        /// </summary>
        /// <param name="request">The model request containing messages and tools</param>
        /// <param name="config">Configuration parameters for the model</param>
        /// <param name="handler">Callback for handling partial responses</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The final model response</returns>
        public override async Task<ModelResponse> GenerateStreamingResponseAsync(
            ModelRequest request,
            ModelConfiguration config,
            Action<string> handler,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Track operation start
                var operationId = $"ollama_stream_{_modelName}";
                _globalTelemetry?.StartOperation(operationId);

                // Convert request messages to Ollama format
                var ollamaMessages = ConvertToOllamaMessages(request);
                var options = ConvertToOllamaOptions(config);

                // Call injected Ollama client for streaming
                var ollamaResponse = await _client.GenerateStreamingCompletionAsync(
                    _modelName, 
                    ollamaMessages, 
                    handler, 
                    options, 
                    cancellationToken);

                // Convert response back to framework format
                var response = ConvertToModelResponse(ollamaResponse);

                // Track completion
                var elapsedSeconds = _globalTelemetry?.EndOperation(operationId) ?? 0;
                _globalTelemetry?.CompleteLLMRequest(operationId, response.Usage.TotalTokens);

                Logger.Info($"OllamaModel completed streaming in {elapsedSeconds:F2}s with {response.Usage.TotalTokens} tokens");

                return response;
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Ollama streaming operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in Ollama GenerateStreamingResponseAsync", ex);
                throw new ModelException($"Error in streaming response from Ollama: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts AgentSharp messages to Ollama format
        /// </summary>
        private static System.Collections.Generic.IEnumerable<OllamaMessage> ConvertToOllamaMessages(ModelRequest request)
        {
            if (request?.Messages == null) yield break;

            foreach (var message in request.Messages)
            {
                yield return new OllamaMessage
                {
                    Role = message.Role.ToString().ToLowerInvariant(),
                    Content = message.Content ?? string.Empty
                };
            }
        }

        /// <summary>
        /// Converts AgentSharp configuration to Ollama options
        /// </summary>
        private static OllamaOptions ConvertToOllamaOptions(ModelConfiguration config)
        {
            if (config == null) return null;

            return new OllamaOptions
            {
                Temperature = config.Temperature,
                TopP = config.TopP,
                MaxTokens = config.MaxTokens
            };
        }

        /// <summary>
        /// Converts Ollama response to AgentSharp format
        /// </summary>
        private static ModelResponse ConvertToModelResponse(OllamaResponse ollamaResponse)
        {
            return new ModelResponse
            {
                Content = ollamaResponse.Content ?? string.Empty,
                Usage = new UsageInfo
                {
                    PromptTokens = ollamaResponse.PromptEvalCount ?? 0,
                    CompletionTokens = ollamaResponse.EvalCount ?? 0,
                    EstimatedCost = 0 // Ollama is typically free
                }
            };
        }

    }
}