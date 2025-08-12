using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HNSWCosineDistance = HNSW.Net.CosineDistance;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Distance function implementations for HNSW using HNSW.Net optimized functions
    /// </summary>
    public static class HNSWDistanceFunctions
    {
        /// <summary>
        /// Get distance function based on metric type
        /// </summary>
        /// <param name="metric">Distance metric</param>
        /// <returns>Distance function for use with SmallWorld</returns>
        public static Func<float[], float[], float> GetDistanceFunction(DistanceMetric metric)
        {
            switch (metric)
            {
                case DistanceMetric.Cosine:
                    return CosineDistance;
                case DistanceMetric.Euclidean:
                    return EuclideanDistance;
                case DistanceMetric.DotProduct:
                    return DotProductDistance;
                default:
                    throw new ArgumentOutOfRangeException(nameof(metric), $"Unsupported distance metric: {metric}");
            }
        }

        /// <summary>
        /// Cosine distance function using HNSW.Net optimized implementation
        /// </summary>
        public static float CosineDistance(float[] x, float[] y)
        {
            if (Vector.IsHardwareAccelerated && x.Length >= Vector<float>.Count)
            {
                return HNSWCosineDistance.SIMD(x, y);
            }
            else
            {
                return HNSWCosineDistance.NonOptimized(x, y);
            }
        }

        /// <summary>
        /// Euclidean distance function implementation
        /// </summary>
        public static float EuclideanDistance(float[] x, float[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Vectors must have the same dimensions");

            if (Vector.IsHardwareAccelerated && x.Length >= Vector<float>.Count)
            {
                return EuclideanDistanceSIMD(x, y);
            }
            else
            {
                return EuclideanDistanceBasic(x, y);
            }
        }

        /// <summary>
        /// Dot product distance function (negative dot product for HNSW compatibility)
        /// </summary>
        public static float DotProductDistance(float[] x, float[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Vectors must have the same dimensions");

            if (Vector.IsHardwareAccelerated && x.Length >= Vector<float>.Count)
            {
                return DotProductDistanceSIMD(x, y);
            }
            else
            {
                return DotProductDistanceBasic(x, y);
            }
        }

        private static float EuclideanDistanceBasic(float[] x, float[] y)
        {
            float sumSquaredDifferences = 0f;
            
            for (int i = 0; i < x.Length; i++)
            {
                var diff = x[i] - y[i];
                sumSquaredDifferences += diff * diff;
            }

            return (float)Math.Sqrt(sumSquaredDifferences);
        }

        private static float EuclideanDistanceSIMD(float[] x, float[] y)
        {
            float sumSquaredDifferences = 0f;
            int step = Vector<float>.Count;
            int i, to = x.Length - step;

            // SIMD processing
            for (i = 0; i <= to; i += step)
            {
                var vx = new Vector<float>(x, i);
                var vy = new Vector<float>(y, i);
                var diff = vx - vy;
                sumSquaredDifferences += Vector.Dot(diff, diff);
            }

            // Process remaining elements
            for (; i < x.Length; i++)
            {
                var diff = x[i] - y[i];
                sumSquaredDifferences += diff * diff;
            }

            return (float)Math.Sqrt(sumSquaredDifferences);
        }

        private static float DotProductDistanceBasic(float[] x, float[] y)
        {
            float dotProduct = 0f;
            
            for (int i = 0; i < x.Length; i++)
            {
                dotProduct += x[i] * y[i];
            }

            return -dotProduct; // Negative for distance (higher dot product = smaller distance)
        }

        private static float DotProductDistanceSIMD(float[] x, float[] y)
        {
            float dotProduct = 0f;
            int step = Vector<float>.Count;
            int i, to = x.Length - step;

            // SIMD processing
            for (i = 0; i <= to; i += step)
            {
                var vx = new Vector<float>(x, i);
                var vy = new Vector<float>(y, i);
                dotProduct += Vector.Dot(vx, vy);
            }

            // Process remaining elements
            for (; i < x.Length; i++)
            {
                dotProduct += x[i] * y[i];
            }

            return -dotProduct; // Negative for distance
        }
    }

    /// <summary>
    /// Utility functions for embedding operations
    /// </summary>
    public static class EmbeddingUtils
    {
        /// <summary>
        /// Normalize embedding vector to unit length
        /// </summary>
        /// <param name="embedding">Input embedding</param>
        /// <returns>Normalized embedding</returns>
        public static float[] Normalize(float[] embedding)
        {
            var norm = (float)Math.Sqrt(embedding.Sum(x => x * x));
            
            if (norm == 0f)
                return embedding; // Cannot normalize zero vector
                
            var normalized = new float[embedding.Length];
            for (int i = 0; i < embedding.Length; i++)
            {
                normalized[i] = embedding[i] / norm;
            }
            
            return normalized;
        }

        /// <summary>
        /// Convert List&lt;float&gt; to float[] for HNSW compatibility
        /// </summary>
        /// <param name="embedding">List embedding</param>
        /// <returns>Array embedding</returns>
        public static float[] ToArray(List<float> embedding)
        {
            return embedding?.ToArray() ?? throw new ArgumentNullException(nameof(embedding));
        }

        /// <summary>
        /// Convert float[] to List&lt;float&gt; for compatibility with existing interfaces
        /// </summary>
        /// <param name="embedding">Array embedding</param>
        /// <returns>List embedding</returns>
        public static List<float> ToList(float[] embedding)
        {
            return embedding?.ToList() ?? throw new ArgumentNullException(nameof(embedding));
        }

        /// <summary>
        /// Validate embedding dimensions
        /// </summary>
        /// <param name="embedding">Embedding to validate</param>
        /// <param name="expectedDimensions">Expected number of dimensions</param>
        /// <exception cref="ArgumentException">Thrown when dimensions don't match</exception>
        public static void ValidateDimensions(float[] embedding, int expectedDimensions)
        {
            if (embedding == null)
                throw new ArgumentNullException(nameof(embedding));
                
            if (embedding.Length != expectedDimensions)
                throw new ArgumentException($"Expected {expectedDimensions} dimensions, got {embedding.Length}");
        }

        /// <summary>
        /// Check if embedding contains valid values (no NaN or infinity)
        /// </summary>
        /// <param name="embedding">Embedding to validate</param>
        /// <returns>True if embedding is valid</returns>
        public static bool IsValidEmbedding(float[] embedding)
        {
            if (embedding == null || embedding.Length == 0)
                return false;
                
            return embedding.All(x => !float.IsNaN(x) && !float.IsInfinity(x));
        }

        /// <summary>
        /// Calculate cosine similarity between two embeddings
        /// </summary>
        /// <param name="x">First embedding</param>
        /// <param name="y">Second embedding</param>
        /// <returns>Cosine similarity (-1 to 1)</returns>
        public static float CosineSimilarity(float[] x, float[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Vectors must have the same dimensions");

            var dotProduct = 0f;
            var normX = 0f;
            var normY = 0f;

            for (int i = 0; i < x.Length; i++)
            {
                dotProduct += x[i] * y[i];
                normX += x[i] * x[i];
                normY += y[i] * y[i];
            }

            if (normX == 0f || normY == 0f)
                return 0f;

            return dotProduct / ((float)Math.Sqrt(normX) * (float)Math.Sqrt(normY));
        }
    }
}