using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models;
using AgentSharp.Models.Interfaces;
using OllamaSDK = Ollama;

namespace AgentSharp.Providers.Ollama
{
    /// <summary>
    /// Ollama model provider implementation for local model serving using tryAGI/Ollama SDK
    /// </summary>
    public class OllamaModelProvider : IModelProvider
    {
        private readonly string _baseUrl;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "ollama";

        /// <summary>
        /// Initializes a new instance of the Ollama model provider
        /// </summary>
        /// <param name="baseUrl">Base URL of the Ollama server (e.g., "http://localhost:11434")</param>
        public OllamaModelProvider(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl ?? "http://localhost:11434";
        }

        /// <summary>
        /// Creates an Ollama model instance
        /// </summary>
        /// <param name="modelName">Name of the model (e.g., "llama3.2", "mistral", "codellama")</param>
        /// <param name="config">Optional model configuration</param>
        /// <returns>Configured Ollama model instance</returns>
        public IModel CreateModel(string modelName, ModelConfiguration config = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            // Create client and model using the new SDK
            var client = new OllamaClient(_baseUrl);
            return new OllamaModel(modelName, client);
        }

        /// <summary>
        /// Checks if the Ollama service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                using (var client = new OllamaSDK.OllamaApiClient(baseUri: new Uri(_baseUrl)))
                {
                    var models = await client.Models.ListModelsAsync();
                    return models != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets information about this provider
        /// </summary>
        /// <returns>Provider information</returns>
        public ProviderInfo GetProviderInfo()
        {
            return new ProviderInfo
            {
                Name = ProviderName,
                Description = "Ollama local model provider using tryAGI/Ollama SDK for running models locally including Llama 3.2, Mistral, CodeLlama, and other open-source models",
                Version = "1.0.3",
                SupportedModels = new[]
                {
                    "llama3.2",
                    "llama3.2:3b",
                    "llama3.1",
                    "llama3.1:8b",
                    "llama3.1:70b",
                    "mistral",
                    "mistral:7b",
                    "codellama",
                    "codellama:13b",
                    "phi3",
                    "phi3:mini",
                    "gemma2",
                    "qwen2.5",
                    "all-minilm", // For embeddings
                    "nomic-embed-text" // For embeddings
                },
                SupportsStreaming = true,
                SupportsFunctionCalling = true, // SDK supports function calling
                SupportsEmbeddings = true // Now supports embeddings
            };
        }
    }
}
