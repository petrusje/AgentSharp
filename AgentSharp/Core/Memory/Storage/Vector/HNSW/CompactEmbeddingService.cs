using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Embedding service wrapper that automatically reduces dimensionality for compact HNSW
    /// </summary>
    public class CompactEmbeddingService : IEmbeddingService
    {
        private readonly IEmbeddingService _baseService;
        private readonly CompactHNSWConfiguration _config;

        public CompactEmbeddingService(IEmbeddingService baseService, CompactHNSWConfiguration config)
        {
            _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Generate compact embedding with automatic dimensionality reduction
        /// </summary>
        public async Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            var fullEmbedding = await _baseService.GenerateEmbeddingAsync(text);
            return ReduceEmbedding(fullEmbedding);
        }

        /// <summary>
        /// Generate multiple compact embeddings
        /// </summary>
        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var fullEmbeddings = await _baseService.GenerateEmbeddingsAsync(texts);
            var compactEmbeddings = new List<List<float>>(fullEmbeddings.Count);

            foreach (var embedding in fullEmbeddings)
            {
                compactEmbeddings.Add(ReduceEmbedding(embedding));
            }

            return compactEmbeddings;
        }

        /// <summary>
        /// Calculate similarity between compact embeddings
        /// </summary>
        public double CalculateSimilarity(List<float> embedding1, List<float> embedding2)
        {
            return _baseService.CalculateSimilarity(embedding1, embedding2);
        }

        /// <summary>
        /// Reduce embedding dimensions according to configuration
        /// </summary>
        private List<float> ReduceEmbedding(List<float> embedding)
        {
            if (embedding == null)
                return null;

            var targetDimensions = _config.GetDimensions();
            
            // If embedding is already the target size or smaller, return as-is
            if (embedding.Count <= targetDimensions)
                return new List<float>(embedding);

            // Apply dimensionality reduction if enabled
            if (_config.EnableDimensionalityReduction)
            {
                return DimensionalityReducer.Reduce(embedding, targetDimensions, _config.ReductionMethod);
            }

            // Simple truncation fallback
            return embedding.GetRange(0, targetDimensions);
        }
    }

    /// <summary>
    /// Mock embedding service for testing compact HNSW functionality
    /// Generates deterministic embeddings of specified dimensions
    /// </summary>
    public class MockCompactEmbeddingService : IEmbeddingService
    {
        private readonly int _dimensions;
        private readonly Random _random;

        public MockCompactEmbeddingService(int dimensions = 256)
        {
            _dimensions = dimensions;
            _random = new Random(42); // Fixed seed for reproducible results
        }

        public async Task<List<float>> GenerateEmbeddingAsync(string text)
        {
            await Task.Delay(1); // Simulate async operation

            // Generate deterministic embeddings based on text hash
            var hash = text?.GetHashCode() ?? 0;
            var random = new Random(hash);
            
            var embedding = new List<float>(_dimensions);
            for (int i = 0; i < _dimensions; i++)
            {
                embedding.Add((float)(random.NextDouble() * 2 - 1)); // Range [-1, 1]
            }
            
            // Normalize the embedding
            var norm = 0f;
            foreach (var value in embedding)
            {
                norm += value * value;
            }
            norm = (float)Math.Sqrt(norm);
            
            if (norm > 0)
            {
                for (int i = 0; i < embedding.Count; i++)
                {
                    embedding[i] /= norm;
                }
            }

            return embedding;
        }

        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<List<float>>(texts.Count);
            foreach (var text in texts)
            {
                embeddings.Add(await GenerateEmbeddingAsync(text));
            }
            return embeddings;
        }

        public double CalculateSimilarity(List<float> embedding1, List<float> embedding2)
        {
            if (embedding1?.Count != embedding2?.Count)
                return 0.0;

            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            for (int i = 0; i < embedding1.Count; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                norm1 += embedding1[i] * embedding1[i];
                norm2 += embedding2[i] * embedding2[i];
            }

            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }

    /// <summary>
    /// Factory for creating appropriate embedding services
    /// </summary>
    public static class CompactEmbeddingServiceFactory
    {
        /// <summary>
        /// Create embedding service based on configuration
        /// </summary>
        public static IEmbeddingService Create(IEmbeddingService baseService, CompactHNSWConfiguration config)
        {
            if (baseService == null)
            {
                // Return mock service for testing
                return new MockCompactEmbeddingService(config.GetDimensions());
            }

            return new CompactEmbeddingService(baseService, config);
        }

        /// <summary>
        /// Create mock service for testing
        /// </summary>
        public static IEmbeddingService CreateMock(EmbeddingSize size = EmbeddingSize.Compact256)
        {
            int dimensions;
            switch (size)
            {
                case EmbeddingSize.Compact256:
                    dimensions = 256;
                    break;
                case EmbeddingSize.Balanced512:
                    dimensions = 512;
                    break;
                case EmbeddingSize.Full1536:
                    dimensions = 1536;
                    break;
                default:
                    dimensions = 256;
                    break;
            }

            return new MockCompactEmbeddingService(dimensions);
        }
    }
}