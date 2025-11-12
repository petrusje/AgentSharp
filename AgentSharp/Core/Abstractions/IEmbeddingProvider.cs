using System.Threading.Tasks;

namespace AgentSharp.Core.Abstractions
{
    /// <summary>
    /// Interface for embedding providers that create and manage embedding service instances
    /// </summary>
    public interface IEmbeddingProvider
    {
        /// <summary>
        /// Gets the unique name of this provider (e.g., "openai", "ollama", "huggingface")
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates an embedding service instance with the specified configuration
        /// </summary>
        /// <param name="config">Embedding service configuration</param>
        /// <returns>An embedding service instance ready for use</returns>
        object CreateEmbeddingService(EmbeddingConfiguration config = null);

        /// <summary>
        /// Checks if the provider is available and can create embedding services
        /// </summary>
        /// <returns>True if the provider is available, false otherwise</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Gets information about the provider
        /// </summary>
        /// <returns>Provider information including supported models and capabilities</returns>
        EmbeddingProviderInfo GetProviderInfo();
    }

    /// <summary>
    /// Configuration for embedding providers
    /// </summary>
    public class EmbeddingConfiguration
    {
        /// <summary>
        /// The embedding model to use (e.g., "text-embedding-3-small", "nomic-embed-text")
        /// </summary>
        public string ModelName { get; set; } = "text-embedding-3-small";

        /// <summary>
        /// Dimensions of the embedding vectors (if configurable)
        /// </summary>
        public int? Dimensions { get; set; }

        /// <summary>
        /// Maximum number of texts to embed in a single batch
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// API endpoint URL (for custom deployments)
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// API key or authentication token
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to normalize embeddings to unit length
        /// </summary>
        public bool NormalizeEmbeddings { get; set; } = true;

        /// <summary>
        /// Custom provider-specific options
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CustomOptions { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
    }

    /// <summary>
    /// Information about an embedding provider
    /// </summary>
    public class EmbeddingProviderInfo
    {
        /// <summary>
        /// Provider name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Provider description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Provider version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// List of supported embedding models
        /// </summary>
        public string[] SupportedModels { get; set; }

        /// <summary>
        /// Default embedding dimensions
        /// </summary>
        public int DefaultDimensions { get; set; }

        /// <summary>
        /// Maximum text length for embeddings
        /// </summary>
        public int MaxTextLength { get; set; }

        /// <summary>
        /// Whether the provider supports batch embedding
        /// </summary>
        public bool SupportsBatchEmbedding { get; set; }

        /// <summary>
        /// Whether the provider supports configurable dimensions
        /// </summary>
        public bool SupportsConfigurableDimensions { get; set; }

        /// <summary>
        /// Cost per 1000 tokens (if applicable)
        /// </summary>
        public decimal? CostPer1000Tokens { get; set; }
    }
    
}