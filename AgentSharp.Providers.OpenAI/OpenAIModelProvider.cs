using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models;
using AgentSharp.Models.Interfaces;

namespace AgentSharp.Providers.OpenAI
{
    /// <summary>
    /// OpenAI model provider implementation that creates OpenAI model instances
    /// </summary>
    public class OpenAIModelProvider : IModelProvider
    {
        private readonly string _apiKey;
        private readonly string _endpoint;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "openai";

        /// <summary>
        /// Initializes a new instance of the OpenAI model provider
        /// </summary>
        /// <param name="apiKey">OpenAI API key</param>
        /// <param name="endpoint">Optional custom endpoint (for Azure OpenAI)</param>
        public OpenAIModelProvider(string apiKey, string endpoint = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _endpoint = endpoint;
        }

        /// <summary>
        /// Creates an OpenAI model instance
        /// </summary>
        /// <param name="modelName">Name of the model (e.g., "gpt-4o-mini", "gpt-4")</param>
        /// <param name="config">Optional model configuration</param>
        /// <returns>Configured OpenAI model instance</returns>
        public IModel CreateModel(string modelName, ModelConfiguration config = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));

            var model = new OpenAIModel(modelName, _apiKey, _endpoint);
            
            // Apply any default configuration if needed
            if (config != null)
            {
                // The OpenAIModel will use the config in its execution methods
                // No need to apply it here since it's passed per-request
            }

            return model;
        }

        /// <summary>
        /// Checks if the OpenAI service is available
        /// </summary>
        /// <returns>True if the service is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                // Try to create a test model to validate the API key and endpoint
                var testModel = new OpenAIModel("gpt-3.5-turbo", _apiKey, _endpoint);
                
                // For now, we assume it's available if we can create the model
                // In a real implementation, you might want to make a simple API call
                return await Task.FromResult(true);
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
                Description = "OpenAI GPT models provider with support for GPT-4, GPT-3.5-turbo, and other OpenAI models",
                Version = "1.0.0",
                SupportedModels = new[]
                {
                    "gpt-4o",
                    "gpt-4o-mini", 
                    "gpt-4",
                    "gpt-4-turbo",
                    "gpt-3.5-turbo",
                    "gpt-3.5-turbo-16k"
                },
                SupportsStreaming = true,
                SupportsFunctionCalling = true,
                SupportsEmbeddings = false
            };
        }
    }
}