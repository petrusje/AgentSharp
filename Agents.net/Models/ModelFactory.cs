using System;

namespace Agents.net.Models
{
    /// <summary>
    /// Fábrica padrão para criação de modelos
    /// </summary>
    public class ModelFactory : IModelFactory
    {
        /// <summary>
        /// Cria uma instância de modelo baseado no tipo
        /// </summary>
        public IModel CreateModel(string modelType, ModelOptions options)
        {
            if (string.IsNullOrWhiteSpace(modelType))
                throw new ArgumentNullException(nameof(modelType));
                
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            // Valida e prepara as opções antes de criar o modelo    
            options.Validate();
                
            switch (modelType.ToLowerInvariant())
            {
                case "openai":
                    var model = new OpenAIModel(options.ModelName, options.ApiKey, options.Endpoint);
                    return model;
                
                // Futuros provedores podem ser adicionados aqui
                // case "anthropic":
                //    return new AnthropicModel(...);
                
                default:
                    throw new ArgumentException($"Tipo de modelo não suportado: {modelType}", nameof(modelType));
            }
        }
        
        /// <summary>
        /// Cria uma instância do modelo OpenAI com as configurações padrão
        /// </summary>
        /// <param name="modelName">Nome do modelo OpenAI a ser usado</param>
        /// <param name="apiKey">Chave de API (opcional se definida como variável de ambiente)</param>
        /// <returns>Instância do modelo OpenAI</returns>
        public static IModel CreateOpenAIModel(string modelName, string apiKey = null)
        {
            var options = new ModelOptions
            {
                ModelName = modelName,
                ApiKey = apiKey
            };
            
            return new ModelFactory().CreateModel("openai", options);
        }
        
        /// <summary>
        /// Cria uma instância de modelo para simulação/testes
        /// </summary>
        /// <param name="modelName">Nome do modelo mock</param>
        /// <returns>Instância do modelo simulado</returns>
        public static IModel CreateMockModel(string modelName = "mock-model")
        {
            var options = new ModelOptions
            {
                ModelName = modelName
            };
            
            return new ModelFactory().CreateModel("mock", options);
        }
    }
}