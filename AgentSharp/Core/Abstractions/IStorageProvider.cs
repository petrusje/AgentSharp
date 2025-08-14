using System.Threading.Tasks;

namespace AgentSharp.Core.Abstractions
{
    /// <summary>
    /// Interface for storage providers that create and manage storage instances
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Gets the unique name of this provider (e.g., "sqlite", "memory", "postgresql")
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates a storage instance with the specified connection string and configuration
        /// </summary>
        /// <param name="connectionString">Connection string or path for the storage</param>
        /// <param name="config">Optional storage configuration</param>
        /// <returns>A storage instance ready for use</returns>
        object CreateStorage(string connectionString = null, StorageConfiguration config = null);

        /// <summary>
        /// Creates a memory storage instance for user memories and conversations
        /// </summary>
        /// <param name="connectionString">Connection string or path for the storage</param>
        /// <param name="config">Optional storage configuration</param>
        /// <returns>A memory storage instance ready for use</returns>
        object CreateMemoryStorage(string connectionString = null, StorageConfiguration config = null);

        /// <summary>
        /// Checks if the provider is available and can create storage instances
        /// </summary>
        /// <returns>True if the provider is available, false otherwise</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Gets information about the provider
        /// </summary>
        /// <returns>Provider information including capabilities</returns>
        StorageProviderInfo GetProviderInfo();
    }

    /// <summary>
    /// Configuration for storage providers
    /// </summary>
    public class StorageConfiguration
    {
        /// <summary>
        /// Whether to enable automatic schema migration
        /// </summary>
        public bool EnableMigration { get; set; } = true;

        /// <summary>
        /// Maximum number of connections in the pool
        /// </summary>
        public int MaxConnections { get; set; } = 10;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to enable connection pooling
        /// </summary>
        public bool EnablePooling { get; set; } = true;

        /// <summary>
        /// Whether to enable WAL mode for SQLite
        /// </summary>
        public bool EnableWalMode { get; set; } = true;

        /// <summary>
        /// Custom storage-specific options
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CustomOptions { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
    }

    /// <summary>
    /// Information about a storage provider
    /// </summary>
    public class StorageProviderInfo
    {
        /// <summary>
        /// Provider name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Provider description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Provider version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Whether the provider supports transactions
        /// </summary>
        public bool SupportsTransactions { get; set; }

        /// <summary>
        /// Whether the provider supports vector/semantic search
        /// </summary>
        public bool SupportsVectorSearch { get; set; }

        /// <summary>
        /// Whether the provider supports full-text search
        /// </summary>
        public bool SupportsFullTextSearch { get; set; }

        /// <summary>
        /// Whether the provider is persistent (vs in-memory)
        /// </summary>
        public bool IsPersistent { get; set; }

        /// <summary>
        /// Whether the provider supports concurrent access
        /// </summary>
        public bool SupportsConcurrentAccess { get; set; }
    }
}