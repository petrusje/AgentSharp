using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para serviço de embeddings para busca semântica
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Gera embedding vetorial para um texto
        /// </summary>
        /// <param name="text">Texto para gerar embedding</param>
        /// <returns>Vetor de embedding</returns>
        Task<List<float>> GenerateEmbeddingAsync(string text);

        /// <summary>
        /// Calcula similaridade entre dois embeddings
        /// </summary>
        /// <param name="embedding1">Primeiro embedding</param>
        /// <param name="embedding2">Segundo embedding</param>
        /// <returns>Score de similaridade (0.0 a 1.0)</returns>
        double CalculateSimilarity(List<float> embedding1, List<float> embedding2);

        /// <summary>
        /// Gera embeddings para múltiplos textos
        /// </summary>
        /// <param name="texts">Lista de textos</param>
        /// <returns>Lista de embeddings correspondentes</returns>
        Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts);
    }

    /// <summary>
    /// Implementação mock do serviço de embeddings para testes
    /// </summary>
    public class MockEmbeddingService : IEmbeddingService
    {
        private readonly Random _random = new Random();

        public Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            // Gera embedding mock baseado no hash do texto para consistência
            var hash = text.GetHashCode();
            var random = new Random(hash);

            var embedding = new List<float>();
            for (int i = 0; i < 384; i++) // Dimensão típica de embeddings
            {
                embedding.Add((float)(random.NextDouble() * 2.0 - 1.0)); // -1.0 a 1.0
            }

            return Task.FromResult(embedding);
        }

        public double CalculateSimilarity(List<float> embedding1, List<float> embedding2)
        {
            if (embedding1?.Count != embedding2?.Count)
                return 0.0;

            // Similaridade do cosseno
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

        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<List<float>>();
            foreach (var text in texts)
            {
                embeddings.Add(await GenerateEmbeddingAsync(text));
            }
            return embeddings;
        }
    }
}
