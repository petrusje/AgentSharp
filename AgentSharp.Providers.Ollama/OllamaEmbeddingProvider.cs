using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Memory.Services;
using OllamaSDK = Ollama;

namespace AgentSharp.Providers.Ollama
{
    /// <summary>
    /// Ollama embedding provider implementation using tryAGI/Ollama SDK
    /// </summary>
    public class OllamaEmbeddingProvider : IEmbeddingProvider
    {
        private readonly string _baseUrl;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "ollama";

        /// <summary>
        /// Initializes a new instance of the Ollama embedding provider
        /// </summary>
        /// <param name="baseUrl">Base URL of the Ollama server (e.g., "http://localhost:11434")</param>
        public OllamaEmbeddingProvider(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl ?? "http://localhost:11434";
        }

        /// <summary>
        /// Creates an Ollama embedding service instance
        /// </summary>
        /// <param name="config">Embedding configuration</param>
        /// <returns>Configured embedding service</returns>
        public object CreateEmbeddingService(EmbeddingConfiguration config = null)
        {
            var modelName = config?.ModelName ?? "all-minilm";

            return new OllamaEmbeddingService(
                baseUrl: _baseUrl,
                logger: new AgentSharp.Utils.ConsoleLogger(),
                model: modelName
            );
        }

        /// <summary>
        /// Checks if the Ollama embedding service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                using (var client = new OllamaSDK.OllamaApiClient(baseUri: new Uri(_baseUrl)))
                {
                    var models = await client.Models.ListModelsAsync();
                    return models?.Models?.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets information about this embedding provider
        /// </summary>
        /// <returns>Provider information</returns>
        public EmbeddingProviderInfo GetProviderInfo()
        {
            return new EmbeddingProviderInfo
            {
                Name = ProviderName,
                Description = "Ollama embedding models provider using tryAGI/Ollama SDK with support for all-minilm, nomic-embed-text, and other local embedding models",
                Version = "1.0.3",
                SupportedModels = new[]
                {
                    "all-minilm",
                    "nomic-embed-text",
                    "mxbai-embed-large",
                    "snowflake-arctic-embed"
                },
                DefaultDimensions = 384, // all-minilm default
                MaxTextLength = 512,
                SupportsBatchEmbedding = true,
                SupportsConfigurableDimensions = false, // Most local models have fixed dimensions
                CostPer1000Tokens = 0.0m // Local models are free to use
            };
        }
    }
}
