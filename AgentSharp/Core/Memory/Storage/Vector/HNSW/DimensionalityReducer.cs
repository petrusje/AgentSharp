using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Utilities for reducing embedding dimensionality while preserving semantic meaning
    /// </summary>
    public static class DimensionalityReducer
    {
        // Cache for reusable projection matrices with thread-safe lazy initialization
        private static readonly ConcurrentDictionary<(int inputDims, int outputDims), Lazy<float[,]>> _projectionMatrixCache = new ConcurrentDictionary<(int, int), Lazy<float[,]>>();
        /// <summary>
        /// Reduce embedding dimensions using the specified method
        /// </summary>
        /// <param name="embedding">Source embedding vector</param>
        /// <param name="targetDimensions">Target number of dimensions</param>
        /// <param name="method">Reduction method to use</param>
        /// <returns>Reduced embedding vector</returns>
        public static List<float> Reduce(List<float> embedding, int targetDimensions, DimensionalityReductionMethod method = DimensionalityReductionMethod.Truncation)
        {
            if (embedding == null)
                throw new ArgumentNullException(nameof(embedding));

            if (targetDimensions <= 0)
                throw new ArgumentException("Target dimensions must be positive", nameof(targetDimensions));

            if (embedding.Count <= targetDimensions)
                return new List<float>(embedding); // No reduction needed

            switch (method)
            {
                case DimensionalityReductionMethod.Truncation:
                    return TruncateEmbedding(embedding, targetDimensions);
                case DimensionalityReductionMethod.PCA:
                    return ReduceWithPCA(embedding, targetDimensions);
                case DimensionalityReductionMethod.RandomProjection:
                    return ReduceWithRandomProjection(embedding, targetDimensions);
                default:
                    return TruncateEmbedding(embedding, targetDimensions);
            }
        }

        /// <summary>
        /// Simple truncation - takes the first N dimensions
        /// Fast but may lose some semantic information
        /// </summary>
        private static List<float> TruncateEmbedding(List<float> embedding, int targetDimensions)
        {
            return embedding.Take(targetDimensions).ToList();
        }

        /// <summary>
        /// Magnitude-based dimension selection - takes dimensions with highest absolute values
        /// Note: This is NOT true PCA, which requires a training corpus
        /// Better semantic preservation than truncation for single vectors
        /// </summary>
        private static List<float> ReduceWithPCA(List<float> embedding, int targetDimensions)
        {
            // For single vector, we can't compute true PCA, so we use magnitude-based selection
            // In a real implementation, you'd need a corpus to compute PCA transformation matrix

            var indexed = embedding.Select((value, index) => new { Value = Math.Abs(value), Index = index })
                                  .OrderByDescending(x => x.Value)
                                  .Take(targetDimensions)
                                  .OrderBy(x => x.Index) // Maintain original order
                                  .ToList();

            var reduced = new List<float>(targetDimensions);
            foreach (var item in indexed)
            {
                reduced.Add(embedding[item.Index]);
            }

            // Normalize to maintain similar magnitude
            var norm = (float)Math.Sqrt(reduced.Sum(x => x * x));
            if (norm > 0)
            {
                for (int i = 0; i < reduced.Count; i++)
                {
                    reduced[i] /= norm;
                }
            }

            return reduced;
        }

        /// <summary>
        /// Random projection reduction using Johnson-Lindenstrauss lemma
        /// Good balance between speed and semantic preservation
        /// Uses cached projection matrices for consistency
        /// </summary>
        private static List<float> ReduceWithRandomProjection(List<float> embedding, int targetDimensions)
        {
            // Get or create cached projection matrix with thread-safe lazy initialization
            var key = (embedding.Count, targetDimensions);
            // O uso de Random(42) é seguro e intencional aqui.
            // A semente fixa garante reprodutibilidade e consistência na matriz de projeção.
            // Não há risco de vulnerabilidade ou comportamento inesperado neste contexto.
            var lazyMatrix = _projectionMatrixCache.GetOrAdd(key, k =>
                new Lazy<float[,]>(() => GenerateGaussianMatrix(k.inputDims, k.outputDims, new Random(42))));
            var projectionMatrix = lazyMatrix.Value;

            var reduced = new List<float>(targetDimensions);
            for (int i = 0; i < targetDimensions; i++)
            {
                float sum = 0f;
                for (int j = 0; j < embedding.Count; j++)
                {
                    sum += embedding[j] * projectionMatrix[j, i];
                }
                reduced.Add(sum);
            }

            // Normalize the result
            var norm = (float)Math.Sqrt(reduced.Sum(x => x * x));
            if (norm > 0)
            {
                for (int i = 0; i < reduced.Count; i++)
                {
                    reduced[i] /= norm;
                }
            }

            return reduced;
        }

        /// <summary>
        /// Clear the projection matrix cache to free memory
        /// </summary>
        public static void ClearCache()
        {
            _projectionMatrixCache.Clear();
        }

        /// <summary>
        /// Generate a random Gaussian matrix for projection
        /// </summary>
        private static float[,] GenerateGaussianMatrix(int inputDims, int outputDims, Random random)
        {
            var matrix = new float[inputDims, outputDims];
            for (int i = 0; i < inputDims; i++)
            {
                for (int j = 0; j < outputDims; j++)
                {
                    // Box-Muller transform for Gaussian distribution
                    var u1 = 1.0 - random.NextDouble();
                    var u2 = 1.0 - random.NextDouble();
                    var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                    matrix[i, j] = (float)(normal / Math.Sqrt(outputDims));
                }
            }
            return matrix;
        }
    }

    /// <summary>
    /// Quantization utilities for further memory reduction
    /// </summary>
    public static class VectorQuantizer
    {
        /// <summary>
        /// Quantize float vector to 8-bit integers
        /// Reduces memory usage by 75% with minimal quality loss
        /// </summary>
        public static List<byte> QuantizeToInt8(List<float> vector)
        {
            if (vector == null || vector.Count == 0)
                return new List<byte>();

            // Find min/max for scaling
            var min = vector.Min();
            var max = vector.Max();
            var range = max - min;
            const float epsilon = 1e-6f;

            if (Math.Abs(range) < epsilon)
                return vector.Select(_ => (byte)127).ToList();

            var quantized = new List<byte>(vector.Count);
            foreach (var value in vector)
            {
                var normalized = (value - min) / range; // [0, 1]
                var quantizedValue = (int)(normalized * 255); // [0, 255]
                quantized.Add((byte)Math.Max(0, Math.Min(255, quantizedValue)));
            }

            return quantized;
        }

        /// <summary>
        /// Dequantize 8-bit integers back to floats
        /// </summary>
        public static List<float> DequantizeFromInt8(List<byte> quantized, float min, float max)
        {
            if (quantized == null || quantized.Count == 0)
                return new List<float>();

            var range = max - min;
            var dequantized = new List<float>(quantized.Count);

            foreach (var value in quantized)
            {
                var normalized = value / 255.0f; // [0, 1]
                var originalValue = min + normalized * range;
                dequantized.Add(originalValue);
            }

            return dequantized;
        }

        /// <summary>
        /// Quantize float vector to 16-bit integers
        /// Reduces memory usage by 50% with better quality than int8
        /// </summary>
        public static List<short> QuantizeToInt16(List<float> vector)
        {
            if (vector == null || vector.Count == 0)
                return new List<short>();

            var min = vector.Min();
            var max = vector.Max();
            var range = max - min;
            const float epsilon = 1e-6f;

            if (Math.Abs(range) < epsilon)
                return vector.Select(_ => (short)0).ToList();

            var quantized = new List<short>(vector.Count);
            foreach (var value in vector)
            {
                var normalized = (value - min) / range; // [0, 1]
                var quantizedValue = (int)(normalized * 65535) - 32768; // [-32768, 32767]
                quantized.Add((short)Math.Max(-32768, Math.Min(32767, quantizedValue)));
            }

            return quantized;
        }
    }
}
