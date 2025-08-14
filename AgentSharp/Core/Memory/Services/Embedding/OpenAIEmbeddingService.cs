using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Utils;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Serviço de embedding usando OpenAI API
    /// </summary>
    public class OpenAIEmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly EmbeddingClient _embeddingClient;
        private readonly ILogger _logger;

        public OpenAIEmbeddingService(string apiKey, string endpoint = "https://api.openai.com", ILogger logger = null, string model = "text-embedding-3-small")
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _logger = logger ?? new ConsoleLogger();

            try
            {
                var options = new OpenAIClientOptions();
                if (!string.IsNullOrEmpty(endpoint) && endpoint != "https://api.openai.com")
                {
                    options.Endpoint = new Uri(endpoint);
                }

                var openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);
                _embeddingClient = openAIClient.GetEmbeddingClient(model);

                _logger.Log(LogLevel.Info, $"OpenAIEmbeddingService initialized with model {model}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error initializing OpenAIEmbeddingService: {ex.Message}");
                throw;
            }
        }

        public async Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return new List<float>();

                _logger.Log(LogLevel.Debug, $"Generating embedding for text: {text.Substring(0, Math.Min(50, text.Length))}...");

                var embeddingResult = await _embeddingClient.GenerateEmbeddingAsync(text.Trim());
                var embedding = embeddingResult.Value;
                var vector = embedding.ToFloats();

                _logger.Log(LogLevel.Debug, $"Successfully generated embedding with {vector.Length} dimensions");

                return new List<float>(vector.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error generating embedding: {ex.Message}");
                throw;
            }
        }

        public double CalculateSimilarity(List<float> embedding1, List<float> embedding2)
        {
            if (embedding1?.Count != embedding2?.Count || embedding1.Count == 0)
                return 0.0;

            // Estratégia baseada na implementação do SemanticSqliteStorage
            // mas com otimizações para diferentes cenários de uso
            return CalculateCosineSimilarityOptimized(embedding1, embedding2);
        }

        /// <summary>
        /// Cálculo otimizado de similaridade cosseno
        /// Inspirado no SemanticSqliteStorage mas com melhorias de performance
        /// </summary>
        private double CalculateCosineSimilarityOptimized(List<float> vector1, List<float> vector2)
        {
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            // Otimização: conversão única para arrays se necessário (>512 dimensões)
            if (vector1.Count > 512)
            {
                return CalculateWithArrays(vector1, vector2);
            }

            // Para embeddings típicos (<=512), usar acesso direto à lista
            int count = vector1.Count;
            for (int i = 0; i < count; i++)
            {
                double val1 = vector1[i];
                double val2 = vector2[i];

                dotProduct += val1 * val2;
                norm1 += val1 * val1;
                norm2 += val2 * val2;
            }

            // Evitar divisão por zero
            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;

            // Similaridade cosseno padrão (-1 a 1)
            double similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));

            // Retornar valor normalizado para 0-1 (consistente com interface)
            return Math.Max(0.0, Math.Min(1.0, (similarity + 1.0) * 0.5));
        }

        /// <summary>
        /// Versão para embeddings muito grandes usando arrays
        /// </summary>
        private double CalculateWithArrays(List<float> vector1, List<float> vector2)
        {
            // Para embeddings grandes, converter para arrays uma vez
            float[] arr1 = vector1.ToArray();
            float[] arr2 = vector2.ToArray();

            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            int length = arr1.Length;

            // Processamento unrolled para embeddings muito grandes
            int chunks = length / 8;
            int remainder = length % 8;

            // Processar em grupos de 8 para máxima eficiência
            for (int i = 0; i < chunks * 8; i += 8)
            {
                // Unrolled loop para melhor performance
                for (int j = 0; j < 8; j++)
                {
                    double val1 = arr1[i + j];
                    double val2 = arr2[i + j];
                    dotProduct += val1 * val2;
                    norm1 += val1 * val1;
                    norm2 += val2 * val2;
                }
            }

            // Processar elementos restantes
            for (int i = chunks * 8; i < length; i++)
            {
                double val1 = arr1[i];
                double val2 = arr2[i];
                dotProduct += val1 * val2;
                norm1 += val1 * val1;
                norm2 += val2 * val2;
            }

            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;

            double similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
            return Math.Max(0.0, Math.Min(1.0, (similarity + 1.0) * 0.5));
        }

        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<List<float>>();
            foreach (var text in texts)
            {
                embeddings.Add(await GenerateEmbeddingAsync(text));
            }
            return embeddings;
        }

        public void Dispose()
        {
            // O EmbeddingClient é gerenciado pelo OpenAIClient
            // Não há recursos adicionais para descartar nesta implementação
        }
    }
}
