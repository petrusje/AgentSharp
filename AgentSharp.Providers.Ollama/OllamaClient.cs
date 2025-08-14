using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Models.Interfaces;
using AgentSharp.Utils;

namespace AgentSharp.Providers.Ollama
{
    /// <summary>
    /// Concrete implementation of IOllamaClient for communicating with Ollama server
    /// </summary>
    public class OllamaClient : IOllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private bool _disposed = false;

        /// <summary>
        /// Gets the base URL of the Ollama server
        /// </summary>
        public string BaseUrl => _baseUrl;

        /// <summary>
        /// Initializes a new instance of the Ollama client
        /// </summary>
        /// <param name="baseUrl">Base URL of the Ollama server (e.g., "http://localhost:11434")</param>
        /// <param name="httpClient">Optional HTTP client instance</param>
        public OllamaClient(string baseUrl, HttpClient httpClient = null)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _httpClient = httpClient ?? new HttpClient();
            
            // Set reasonable timeouts for local model generation
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Generates a chat completion response from the Ollama model
        /// </summary>
        public async Task<OllamaResponse> GenerateCompletionAsync(
            string modelName,
            IEnumerable<OllamaMessage> messages,
            OllamaOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            var request = new
            {
                model = modelName,
                messages = messages,
                stream = false,
                options = CreateOptionsObject(options)
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaApiResponse>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                return new OllamaResponse
                {
                    Content = ollamaResponse?.Message?.Content ?? "",
                    Done = ollamaResponse?.Done ?? true,
                    PromptEvalCount = ollamaResponse?.PromptEvalCount,
                    EvalCount = ollamaResponse?.EvalCount,
                    PromptEvalDuration = ollamaResponse?.PromptEvalDuration,
                    EvalDuration = ollamaResponse?.EvalDuration,
                    TotalDuration = ollamaResponse?.TotalDuration
                };
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"HTTP error calling Ollama API", ex);
                throw new InvalidOperationException($"Failed to call Ollama API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Logger.Error($"Timeout calling Ollama API", ex);
                throw new TimeoutException("Ollama API call timed out", ex);
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
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            var request = new
            {
                model = modelName,
                messages = messages,
                stream = true,
                options = CreateOptionsObject(options)
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[8192];
                var responseBuilder = new StringBuilder();
                var finalResponse = new OllamaResponse();

                while (true)
                {
                    var bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var lines = chunk.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var streamResponse = JsonSerializer.Deserialize<OllamaApiResponse>(line, new JsonSerializerOptions 
                            { 
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                            });

                            if (streamResponse?.Message?.Content != null)
                            {
                                var contentChunk = streamResponse.Message.Content;
                                responseBuilder.Append(contentChunk);
                                onChunk?.Invoke(contentChunk);
                            }

                            if (streamResponse?.Done == true)
                            {
                                finalResponse.Done = true;
                                finalResponse.PromptEvalCount = streamResponse.PromptEvalCount;
                                finalResponse.EvalCount = streamResponse.EvalCount;
                                finalResponse.PromptEvalDuration = streamResponse.PromptEvalDuration;
                                finalResponse.EvalDuration = streamResponse.EvalDuration;
                                finalResponse.TotalDuration = streamResponse.TotalDuration;
                                break;
                            }
                        }
                        catch (JsonException)
                        {
                            // Skip malformed JSON lines
                            continue;
                        }
                    }

                    if (finalResponse.Done) break;
                }

                finalResponse.Content = responseBuilder.ToString();
                return finalResponse;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"HTTP error calling Ollama streaming API", ex);
                throw new InvalidOperationException($"Failed to call Ollama streaming API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Logger.Error($"Timeout calling Ollama streaming API", ex);
                throw new TimeoutException("Ollama streaming API call timed out", ex);
            }
        }

        /// <summary>
        /// Lists available models on the Ollama server
        /// </summary>
        public async Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags", cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonSerializer.Deserialize<OllamaModelsResponse>(responseJson, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                var modelNames = new List<string>();
                if (modelsResponse?.Models != null)
                {
                    foreach (var model in modelsResponse.Models)
                    {
                        if (!string.IsNullOrEmpty(model.Name))
                        {
                            modelNames.Add(model.Name);
                        }
                    }
                }

                return modelNames;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"HTTP error listing Ollama models", ex);
                throw new InvalidOperationException($"Failed to list Ollama models: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if the Ollama server is running and accessible
        /// </summary>
        public async Task<bool> IsServerAccessibleAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an options object for the Ollama API
        /// </summary>
        private object CreateOptionsObject(OllamaOptions options)
        {
            if (options == null) return null;

            var optionsObj = new Dictionary<string, object>();

            if (options.Temperature.HasValue)
                optionsObj["temperature"] = options.Temperature.Value;

            if (options.TopP.HasValue)
                optionsObj["top_p"] = options.TopP.Value;

            if (options.MaxTokens.HasValue)
                optionsObj["num_predict"] = options.MaxTokens.Value;

            if (options.Stop != null && options.Stop.Length > 0)
                optionsObj["stop"] = options.Stop;

            return optionsObj.Count > 0 ? optionsObj : null;
        }

        /// <summary>
        /// Disposes the HTTP client
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        #region Internal API Response Classes

        private class OllamaApiResponse
        {
            public OllamaApiMessage Message { get; set; }
            public bool Done { get; set; }
            public int? PromptEvalCount { get; set; }
            public int? EvalCount { get; set; }
            public long? PromptEvalDuration { get; set; }
            public long? EvalDuration { get; set; }
            public long? TotalDuration { get; set; }
        }

        private class OllamaApiMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        private class OllamaModelsResponse
        {
            public OllamaModelInfo[] Models { get; set; }
        }

        private class OllamaModelInfo
        {
            public string Name { get; set; }
            public long Size { get; set; }
            public string Digest { get; set; }
        }

        #endregion
    }
}