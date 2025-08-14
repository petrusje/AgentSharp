using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models;
using AgentSharp.Models.Interfaces;

namespace AgentSharp.Providers.Ollama
{
    /// <summary>
    /// Ollama model provider implementation for local model serving
    /// </summary>
    public class OllamaModelProvider : IModelProvider
    {
        private readonly IOllamaClient _client;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "ollama";

        /// <summary>
        /// Initializes a new instance of the Ollama model provider
        /// </summary>
        /// <param name="client">Ollama client instance</param>
        public OllamaModelProvider(IOllamaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Creates an Ollama model instance
        /// </summary>
        /// <param name="modelName">Name of the model (e.g., "llama2", "mistral", "codellama")</param>
        /// <param name="config">Optional model configuration</param>
        /// <returns>Configured Ollama model instance</returns>
        public IModel CreateModel(string modelName, ModelConfiguration config = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            // The OllamaModel constructor expects the client to be injected
            return new OllamaModel(modelName, _client);
        }

        /// <summary>
        /// Checks if the Ollama service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                return await _client.IsServerAccessibleAsync();
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
                Description = "Ollama local model provider for running models locally including Llama 2, Mistral, CodeLlama, and other open-source models",
                Version = "1.0.0",
                SupportedModels = new[]
                {
                    "llama2",
                    "llama2:13b",
                    "llama2:70b",
                    "mistral",
                    "mistral:7b",
                    "codellama",
                    "codellama:13b",
                    "phi",
                    "neural-chat",
                    "orca-mini"
                },
                SupportsStreaming = true,
                SupportsFunctionCalling = false, // Most local models don't support function calling yet
                SupportsEmbeddings = false
            };
        }
    }
}