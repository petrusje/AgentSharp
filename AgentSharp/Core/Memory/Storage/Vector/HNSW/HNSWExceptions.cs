using System;

namespace AgentSharp.Core.Memory.Services.HNSW
{
    /// <summary>
    /// Base exception for HNSW-related errors
    /// </summary>
    public abstract class HNSWException : Exception
    {
        protected HNSWException(string message) : base(message) { }
        protected HNSWException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when HNSW index is corrupted or invalid
    /// </summary>
    public class HNSWIndexCorruptedException : HNSWException
    {
        public HNSWIndexCorruptedException(string message) : base(message) { }
        public HNSWIndexCorruptedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when HNSW configuration is invalid
    /// </summary>
    public class HNSWConfigurationException : HNSWException
    {
        public string ParameterName { get; }
        public object Value { get; }

        public HNSWConfigurationException(string parameterName, object value, string message) 
            : base($"{message}. Parameter: {parameterName}, Value: {value}")
        {
            ParameterName = parameterName;
            Value = value;
        }
    }

    /// <summary>
    /// Exception thrown when HNSW operations fail due to concurrency issues
    /// </summary>
    public class HNSWConcurrencyException : HNSWException
    {
        public HNSWConcurrencyException(string message) : base(message) { }
        public HNSWConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when HNSW index persistence operations fail
    /// </summary>
    public class HNSWPersistenceException : HNSWException
    {
        public string FilePath { get; }

        public HNSWPersistenceException(string filePath, string message) : base(message)
        {
            FilePath = filePath;
        }

        public HNSWPersistenceException(string filePath, string message, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }
    }

    /// <summary>
    /// Exception thrown when embedding operations fail
    /// </summary>
    public class HNSWEmbeddingException : HNSWException
    {
        public int ExpectedDimensions { get; }
        public int ActualDimensions { get; }

        public HNSWEmbeddingException(int expectedDimensions, int actualDimensions, string message) : base(message)
        {
            ExpectedDimensions = expectedDimensions;
            ActualDimensions = actualDimensions;
        }
    }
}