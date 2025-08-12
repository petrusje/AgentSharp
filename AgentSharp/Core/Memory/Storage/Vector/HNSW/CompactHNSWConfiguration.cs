using System;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Configuration for Compact HNSW implementation optimized for memory usage
    /// </summary>
    public class CompactHNSWConfiguration
    {
        /// <summary>
        /// Target embedding dimensions (256, 512, or 1536)
        /// </summary>
        public EmbeddingSize EmbeddingSize { get; set; } = EmbeddingSize.Compact256;

        /// <summary>
        /// Number of bi-directional links created for each node (default: 8 for memory efficiency)
        /// </summary>
        public int M { get; set; } = 8;

        /// <summary>
        /// Size of the dynamic candidate list for construction (default: 100)
        /// </summary>
        public int EfConstruction { get; set; } = 100;

        /// <summary>
        /// Size of the dynamic candidate list for search (default: 32)
        /// </summary>
        public int EfSearch { get; set; } = 32;

        /// <summary>
        /// Maximum number of candidates to consider for re-ranking (default: 200)
        /// </summary>
        public int MaxCandidates { get; set; } = 200;

        /// <summary>
        /// Distance metric to use for vector comparison
        /// </summary>
        public DistanceMetric DistanceMetric { get; set; } = DistanceMetric.Cosine;

        /// <summary>
        /// Enable quantization to reduce memory usage further (default: false)
        /// </summary>
        public bool UseQuantization { get; set; } = false;

        /// <summary>
        /// Quantization type when UseQuantization is enabled
        /// </summary>
        public QuantizationType QuantizationType { get; set; } = QuantizationType.Int8;

        /// <summary>
        /// Enable dimensionality reduction if source embeddings are larger
        /// </summary>
        public bool EnableDimensionalityReduction { get; set; } = true;

        /// <summary>
        /// Method to use for dimensionality reduction
        /// </summary>
        public DimensionalityReductionMethod ReductionMethod { get; set; } = DimensionalityReductionMethod.Truncation;

        /// <summary>
        /// Get the actual number of dimensions for this configuration
        /// </summary>
        public int GetDimensions()
        {
            switch (EmbeddingSize)
            {
                case EmbeddingSize.Compact256:
                    return 256;
                case EmbeddingSize.Balanced512:
                    return 512;
                case EmbeddingSize.Full1536:
                    return 1536;
                default:
                    return 256;
            }
        }

        /// <summary>
        /// Get estimated memory usage per vector in bytes
        /// </summary>
        public long GetEstimatedBytesPerVector()
        {
            var dimensions = GetDimensions();
            var bytesPerDim = UseQuantization && QuantizationType == QuantizationType.Int8 ? 1 : 4;
            var embeddingBytes = dimensions * bytesPerDim;
            var overheadBytes = M * 4 + 50; // Graph connections + metadata
            
            return embeddingBytes + overheadBytes;
        }

        /// <summary>
        /// Validate configuration and provide warnings for performance issues
        /// </summary>
        public void Validate()
        {
            if (M <= 0 || M > 32)
                throw new HNSWConfigurationException(nameof(M), M, "M must be between 1 and 32 for compact HNSW");

            if (EfConstruction <= 0)
                throw new HNSWConfigurationException(nameof(EfConstruction), EfConstruction, "EfConstruction must be positive");

            if (EfSearch <= 0)
                throw new HNSWConfigurationException(nameof(EfSearch), EfSearch, "EfSearch must be positive");

            if (MaxCandidates <= 0)
                throw new HNSWConfigurationException(nameof(MaxCandidates), MaxCandidates, "MaxCandidates must be positive");
        }

        /// <summary>
        /// Create optimized configuration for development/testing
        /// </summary>
        public static CompactHNSWConfiguration ForDevelopment() => new CompactHNSWConfiguration()
        {
            EmbeddingSize = EmbeddingSize.Compact256,
            M = 4,
            EfConstruction = 50,
            EfSearch = 16,
            UseQuantization = true,
            QuantizationType = QuantizationType.Int8
        };

        /// <summary>
        /// Create balanced configuration for production use
        /// </summary>
        public static CompactHNSWConfiguration ForProduction() => new CompactHNSWConfiguration()
        {
            EmbeddingSize = EmbeddingSize.Balanced512,
            M = 8,
            EfConstruction = 100,
            EfSearch = 32,
            UseQuantization = false
        };

        /// <summary>
        /// Create high-quality configuration for enterprise use
        /// </summary>
        public static CompactHNSWConfiguration ForEnterprise() => new CompactHNSWConfiguration()
        {
            EmbeddingSize = EmbeddingSize.Full1536,
            M = 16,
            EfConstruction = 200,
            EfSearch = 50,
            UseQuantization = false
        };
    }

    /// <summary>
    /// Supported embedding sizes
    /// </summary>
    public enum EmbeddingSize
    {
        /// <summary>
        /// 256 dimensions - ultra-compact for development/testing (~1KB per vector)
        /// </summary>
        Compact256,

        /// <summary>
        /// 512 dimensions - balanced for production (~2KB per vector)
        /// </summary>
        Balanced512,

        /// <summary>
        /// 1536 dimensions - full quality for enterprise (~6KB per vector)
        /// </summary>
        Full1536
    }

    /// <summary>
    /// Quantization types for further memory reduction
    /// </summary>
    public enum QuantizationType
    {
        /// <summary>
        /// 8-bit integer quantization (75% memory reduction)
        /// </summary>
        Int8,

        /// <summary>
        /// 16-bit integer quantization (50% memory reduction)
        /// </summary>
        Int16
    }

    /// <summary>
    /// Methods for dimensionality reduction
    /// </summary>
    public enum DimensionalityReductionMethod
    {
        /// <summary>
        /// Simple truncation - take first N dimensions
        /// </summary>
        Truncation,

        /// <summary>
        /// Principal Component Analysis reduction
        /// </summary>
        PCA,

        /// <summary>
        /// Random projection reduction
        /// </summary>
        RandomProjection
    }
}