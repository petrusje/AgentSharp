using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Models.Interfaces;
using AgentSharp.Utils;
using OllamaSDK = Ollama;

namespace AgentSharp.Providers.Ollama
{
    /// <summary>
    /// Ollama client implementation using tryAGI/Ollama SDK - C# 7.3 compatible
    /// </summary>
    public class OllamaClient : IOllamaClient
    {
        private readonly OllamaSDK.OllamaApiClient _client;
        private readonly string _baseUrl;
        private bool _disposed = false;

        /// <summary>
        /// Gets the base URL of the Ollama server
        /// </summary>
        public string BaseUrl => _baseUrl;

        /// <summary>
        /// Initializes a new instance of the Ollama client using tryAGI/Ollama SDK
        /// </summary>
        /// <param name="baseUrl">Base URL of the Ollama server (e.g., "http://localhost:11434")</param>
        public OllamaClient(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl ?? "http://localhost:11434";
            _client = new OllamaSDK.OllamaApiClient(baseUri: new Uri(_baseUrl));
        }

        /// <summary>
        /// Generates a chat completion response from the Ollama model
        /// </summary>
        public async Task<OllamaResponse> GenerateCompletionAsync(
            string modelName,
            IEnumerable<OllamaMessage> messages,
            OllamaOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            try
            {
                // Convert AgentSharp messages to Ollama SDK format
                var ollamaMessages = new List<OllamaSDK.Message>();
                foreach (var msg in messages)
                {
                    var role = GetMessageRole(msg.Role);
                    ollamaMessages.Add(new OllamaSDK.Message
                    {
                        Role = role,
                        Content = msg.Content
                    });
                }

                // Create chat request using the new API structure
                var request = new OllamaSDK.GenerateChatCompletionRequest
                {
                    Model = modelName,
                    Messages = ollamaMessages,
                    Stream = false
                };

                // Apply options if provided
                if (options != null && options.Temperature.HasValue)
                {
                    request.Options = new OllamaSDK.RequestOptions
                    {
                        Temperature = (float)options.Temperature.Value
                    };
                }

                // For non-streaming, get the first (and only) response
                var responseEnumerable = _client.Chat.GenerateChatCompletionAsync(request, cancellationToken);
                var enumerator = responseEnumerable.GetAsyncEnumerator(cancellationToken);

                OllamaSDK.GenerateChatCompletionResponse response = null;
                try
                {
                    if (await enumerator.MoveNextAsync())
                    {
                        response = enumerator.Current;
                    }
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }

                return new OllamaResponse
                {
                    Content = response?.Message?.Content ?? "",
                    Done = response?.Done ?? true,
                    PromptEvalCount = response?.PromptEvalCount,
                    EvalCount = response?.EvalCount,
                    PromptEvalDuration = response?.PromptEvalDuration,
                    EvalDuration = response?.EvalDuration,
                    TotalDuration = response?.TotalDuration
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error calling Ollama API: {ex.Message}", ex);
                throw new InvalidOperationException($"Failed to call Ollama API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a streaming chat completion response from the Ollama model
        /// </summary>
        public async Task<OllamaResponse> GenerateStreamingCompletionAsync(
            string modelName,
            IEnumerable<OllamaMessage> messages,
            Action<string> onChunk = null,
            OllamaOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            try
            {
                // Convert AgentSharp messages to Ollama SDK format
                var ollamaMessages = new List<OllamaSDK.Message>();
                foreach (var msg in messages)
                {
                    var role = GetMessageRole(msg.Role);
                    ollamaMessages.Add(new OllamaSDK.Message
                    {
                        Role = role,
                        Content = msg.Content
                    });
                }

                var request = new OllamaSDK.GenerateChatCompletionRequest
                {
                    Model = modelName,
                    Messages = ollamaMessages,
                    Stream = true
                };

                // Apply options if provided
                if (options != null && options.Temperature.HasValue)
                {
                    request.Options = new OllamaSDK.RequestOptions
                    {
                        Temperature = (float)options.Temperature.Value
                    };
                }

                var responseBuilder = new System.Text.StringBuilder();
                var finalResponse = new OllamaResponse { Done = false };

                // Use GetAsyncEnumerator for C# 7.3 compatibility
                var asyncEnumerator = _client.Chat.GenerateChatCompletionAsync(request, cancellationToken).GetAsyncEnumerator(cancellationToken);
                try
                {
                    while (await asyncEnumerator.MoveNextAsync())
                    {
                        var response = asyncEnumerator.Current;
                        if (response?.Message?.Content != null)
                        {
                            var contentChunk = response.Message.Content;
                            responseBuilder.Append(contentChunk);
                            onChunk?.Invoke(contentChunk);
                        }

                        if (response?.Done == true)
                        {
                            finalResponse.Done = true;
                            finalResponse.PromptEvalCount = response.PromptEvalCount;
                            finalResponse.EvalCount = response.EvalCount;
                            finalResponse.PromptEvalDuration = response.PromptEvalDuration;
                            finalResponse.EvalDuration = response.EvalDuration;
                            finalResponse.TotalDuration = response.TotalDuration;
                            break;
                        }
                    }
                }
                finally
                {
                    await asyncEnumerator.DisposeAsync();
                }

                finalResponse.Content = responseBuilder.ToString();
                return finalResponse;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error calling Ollama streaming API: {ex.Message}", ex);
                throw new InvalidOperationException($"Failed to call Ollama streaming API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lists available models on the Ollama server
        /// </summary>
        public async Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var response = await _client.Models.ListModelsAsync(cancellationToken);
                if (response?.Models != null)
                {
                    return response.Models.Select(m => GetModelName(m)).Where(name => !string.IsNullOrEmpty(name));
                }
                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error listing Ollama models: {ex.Message}", ex);
                throw new InvalidOperationException($"Failed to list Ollama models: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if the Ollama server is running and accessible
        /// </summary>
        public async Task<bool> IsServerAccessibleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new OllamaSDK.OllamaApiClient(baseUri: new Uri(_baseUrl)))
                {
                    var models = await client.Models.ListModelsAsync(cancellationToken);
                    return models != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes the client
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Helper method to convert string role to SDK MessageRole
        /// </summary>
        private OllamaSDK.MessageRole GetMessageRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return OllamaSDK.MessageRole.User;

            switch (role.ToLower())
            {
                case "user":
                    return OllamaSDK.MessageRole.User;
                case "assistant":
                    return OllamaSDK.MessageRole.Assistant;
                case "system":
                    return OllamaSDK.MessageRole.System;
                default:
                    return OllamaSDK.MessageRole.User;
            }
        }

        /// <summary>
        /// Helper method to get model name from SDK Model object
        /// </summary>
        private string GetModelName(OllamaSDK.Model model)
        {
            if (model == null)
                return null;

            // Try different properties that might contain the model name
            try
            {
                // The SDK might have different property names
                var modelType = model.GetType();

                // Try "Name" property first
                var nameProperty = modelType.GetProperty("Name");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(model)?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }

                // Try "Model" property
                var modelProperty = modelType.GetProperty("Model");
                if (modelProperty != null)
                {
                    var name = modelProperty.GetValue(model)?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }

                // Fallback to ToString()
                return model.ToString();
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
