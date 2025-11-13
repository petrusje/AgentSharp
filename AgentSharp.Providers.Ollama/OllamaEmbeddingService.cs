using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Utils;
using OllamaSDK = Ollama;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Serviço de embedding usando Ollama API com tryAGI/Ollama SDK
    /// </summary>
    public class OllamaEmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly OllamaSDK.OllamaApiClient _client;
        private readonly ILogger _logger;
        private readonly string _model;
        private bool _disposed = false;

        /// <summary>
        /// Inicializa uma nova instância do serviço de embedding Ollama
        /// </summary>
        /// <param name="baseUrl">URL base do servidor Ollama</param>
        /// <param name="logger">Logger opcional</param>
        /// <param name="model">Modelo de embedding a ser usado (padrão: all-minilm)</param>
        public OllamaEmbeddingService(string baseUrl = "http://localhost:11434", ILogger logger = null, string model = "all-minilm")
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            _logger = logger ?? new ConsoleLogger();
            _model = model ?? "all-minilm";

            try
            {
                _client = new OllamaSDK.OllamaApiClient(baseUri: new Uri(baseUrl));
                _logger.Log(LogLevel.Info, $"OllamaEmbeddingService initialized with model {_model} at {baseUrl}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error initializing OllamaEmbeddingService: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gera embedding para um texto
        /// </summary>
        /// <param name="text">Texto para gerar embedding</param>
        /// <returns>Lista de floats representando o embedding</returns>
        public async Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.Log(LogLevel.Warning, "Empty text provided for embedding generation");
                    return new List<float>();
                }

                _logger.Log(LogLevel.Debug, $"Generating embedding for text: {text.Substring(0, Math.Min(text.Length, 50))}...");

                var embeddingResponse = await _client.Embeddings.GenerateEmbeddingAsync(
                    model: _model,
                    prompt: text
                );

                if (embeddingResponse?.Embedding != null)
                {
                    // Convert double array to List<float>
                    var embedding = embeddingResponse.Embedding.Select(x => (float)x).ToList();

                    _logger.Log(LogLevel.Debug, $"Embedding generated successfully with {embedding.Count} dimensions");
                    return embedding;
                }

                _logger.Log(LogLevel.Warning, "Empty embedding response received");
                return new List<float>();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error generating embedding: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gera embeddings para múltiplos textos
        /// </summary>
        /// <param name="texts">Lista de textos</param>
        /// <returns>Lista de embeddings correspondentes</returns>
        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var results = new List<List<float>>();

            foreach (var text in texts)
            {
                try
                {
                    var embedding = await GenerateEmbeddingAsync(text);
                    results.Add(embedding);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Error generating embedding for text '{text}': {ex.Message}");
                    // Add empty embedding to maintain consistency
                    results.Add(new List<float>());
                }
            }

            return results;
        }

        /// <summary>
        /// Calcula a similaridade do cosseno entre dois embeddings
        /// </summary>
        /// <param name="embedding1">Primeiro embedding</param>
        /// <param name="embedding2">Segundo embedding</param>
        /// <returns>Score de similaridade (0.0 a 1.0)</returns>
        public double CalculateSimilarity(List<float> embedding1, List<float> embedding2)
        {
            if (embedding1?.Count != embedding2?.Count || embedding1.Count == 0)
                return 0.0;

            // Cálculo de similaridade do cosseno
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            for (int i = 0; i < embedding1.Count; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                norm1 += embedding1[i] * embedding1[i];
                norm2 += embedding2[i] * embedding2[i];
            }

            norm1 = Math.Sqrt(norm1);
            norm2 = Math.Sqrt(norm2);

            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;

            var similarity = dotProduct / (norm1 * norm2);
            return Math.Max(0.0, Math.Min(1.0, (similarity + 1.0) / 2.0)); // Normalizar para 0-1
        }

        /// <summary>
        /// Verifica se o serviço está disponível
        /// </summary>
        /// <returns>True se o serviço estiver disponível</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                var models = await _client.Models.ListModelsAsync();
                if (models?.Models != null)
                {
                    foreach (var model in models.Models)
                    {
                        var modelName = GetModelName(model);
                        if (modelName == _model)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Service availability check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper method to get model name from SDK Model object
        /// </summary>
        private string GetModelName(OllamaSDK.Model model)
        {
            if (model == null)
                return null;

            try
            {
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

                return model.ToString();
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Obtém as dimensões do embedding para o modelo atual
        /// </summary>
        /// <returns>Número de dimensões</returns>
        public async Task<int> GetEmbeddingDimensionsAsync()
        {
            try
            {
                // Generate a test embedding to determine dimensions
                var testEmbedding = await GenerateEmbeddingAsync("test");
                return testEmbedding?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Could not determine embedding dimensions: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Libera os recursos utilizados
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _client?.Dispose();
                    _logger?.Log(LogLevel.Info, "OllamaEmbeddingService disposed");
                }
                catch (Exception ex)
                {
                    _logger?.Log(LogLevel.Warning, $"Error during disposal: {ex.Message}");
                }

                _disposed = true;
            }
        }
    }
}
