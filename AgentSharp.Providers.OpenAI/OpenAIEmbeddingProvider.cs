using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Memory.Services;

namespace AgentSharp.Providers.OpenAI
{
    /// <summary>
    /// OpenAI embedding provider implementation
    /// </summary>
    public class OpenAIEmbeddingProvider : IEmbeddingProvider
    {
        private readonly string _apiKey;
        private readonly string _endpoint;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "openai";

        /// <summary>
        /// Initializes a new instance of the OpenAI embedding provider
        /// </summary>
        /// <param name="apiKey">OpenAI API key</param>
        /// <param name="endpoint">Optional custom endpoint (for Azure OpenAI)</param>
        public OpenAIEmbeddingProvider(string apiKey, string endpoint = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _endpoint = endpoint;
        }

        /// <summary>
        /// Creates an OpenAI embedding service instance
        /// </summary>
        /// <param name="config">Embedding configuration</param>
        /// <returns>Configured embedding service</returns>
        public object CreateEmbeddingService(EmbeddingConfiguration config = null)
        {
            var modelName = config?.ModelName ?? "text-embedding-3-small";
            
            return new OpenAIEmbeddingService(
                apiKey: _apiKey,
                endpoint: _endpoint,
                logger: new Utils.ConsoleLogger(),
                model: modelName
            );
        }

        /// <summary>
        /// Checks if the OpenAI embedding service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                // Try to create a test embedding service to validate configuration
                var testService = CreateEmbeddingService();
                
                // For now, we assume it's available if we can create the service
                // In a real implementation, you might want to make a simple API call
                return await Task.FromResult(true);
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
                Description = "OpenAI embedding models provider with support for text-embedding-3-small, text-embedding-3-large, and ada-002",
                Version = "1.0.0",
                SupportedModels = new[]
                {
                    "text-embedding-3-small",
                    "text-embedding-3-large", 
                    "text-embedding-ada-002"
                },
                DefaultDimensions = 1536,
                MaxTextLength = 8192,
                SupportsBatchEmbedding = true,
                SupportsConfigurableDimensions = true,
                CostPer1000Tokens = 0.0001m // Approximate cost for text-embedding-3-small
            };
        }
    }
}