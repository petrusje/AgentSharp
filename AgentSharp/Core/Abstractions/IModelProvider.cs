using System.Threading.Tasks;
using AgentSharp.Models;

namespace AgentSharp.Core.Abstractions
{
    /// <summary>
    /// Interface for model providers that create and manage language model instances
    /// </summary>
    public interface IModelProvider
    {
        /// <summary>
        /// Gets the unique name of this provider (e.g., "openai", "ollama", "anthropic")
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates a model instance with the specified name and configuration
        /// </summary>
        /// <param name="modelName">The name/ID of the model to create</param>
        /// <param name="config">Optional model configuration</param>
        /// <returns>An IModel instance ready for use</returns>
        IModel CreateModel(string modelName, ModelConfiguration config = null);

        /// <summary>
        /// Checks if the provider is available and can create models
        /// </summary>
        /// <returns>True if the provider is available, false otherwise</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Gets information about the provider
        /// </summary>
        /// <returns>Provider information including supported models</returns>
        ProviderInfo GetProviderInfo();
    }

    /// <summary>
    /// Information about a model provider
    /// </summary>
    public class ProviderInfo
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
        /// List of supported model names/patterns
        /// </summary>
        public string[] SupportedModels { get; set; }

        /// <summary>
        /// Whether the provider supports streaming
        /// </summary>
        public bool SupportsStreaming { get; set; }

        /// <summary>
        /// Whether the provider supports function calling
        /// </summary>
        public bool SupportsFunctionCalling { get; set; }

        /// <summary>
        /// Whether the provider supports embeddings
        /// </summary>
        public bool SupportsEmbeddings { get; set; }
    }
}