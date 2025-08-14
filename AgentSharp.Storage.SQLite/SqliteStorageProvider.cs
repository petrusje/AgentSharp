using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Storage;
using AgentSharp.Core.Memory.Interfaces;

namespace AgentSharp.Storage.SQLite
{
    /// <summary>
    /// SQLite storage provider implementation for persistent storage with vector support
    /// </summary>
    public class SqliteStorageProvider : IStorageProvider
    {
        private readonly string _defaultConnectionString;

        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "sqlite";

        /// <summary>
        /// Initializes a new instance of the SQLite storage provider
        /// </summary>
        /// <param name="defaultConnectionString">Default connection string for SQLite database</param>
        public SqliteStorageProvider(string defaultConnectionString = null)
        {
            _defaultConnectionString = defaultConnectionString ?? ":memory:";
        }

        /// <summary>
        /// Creates a general storage instance
        /// </summary>
        /// <param name="connectionString">SQLite connection string or path</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>SQLite storage instance</returns>
        public object CreateStorage(string connectionString = null, StorageConfiguration config = null)
        {
            var connString = connectionString ?? _defaultConnectionString;
            
            // For general storage, we return a simple SQLite session storage
            return new SqliteSessionStorage(connString);
        }

        /// <summary>
        /// Creates a memory storage instance for semantic search
        /// </summary>
        /// <param name="connectionString">SQLite connection string or path</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>Semantic SQLite storage instance</returns>
        public object CreateMemoryStorage(string connectionString = null, StorageConfiguration config = null)
        {
            var connString = connectionString ?? _defaultConnectionString;
            
            // Para compatibilidade com StorageFactory, primeiro tentamos usar EmbeddingService se disponível na config
            if (config?.CustomOptions != null && config.CustomOptions.ContainsKey("EmbeddingService"))
            {
                var embeddingService = config.CustomOptions["EmbeddingService"];
                var dimensions = config.CustomOptions.ContainsKey("Dimensions") ? 
                    (int)config.CustomOptions["Dimensions"] : 1536;
                
                if (embeddingService is IEmbeddingService embeddings)
                {
                    return new SemanticSqliteStorage(connString, embeddings, dimensions);
                }
            }
            
            // Fallback: criar um SqliteSessionStorage básico se não temos embedding service
            // Isso permite que o StorageFactory funcione, mesmo sem recursos semânticos
            return new SqliteSessionStorage(connString);
        }

        /// <summary>
        /// Creates a semantic memory storage with embedding service
        /// </summary>
        /// <param name="connectionString">SQLite connection string or path</param>
        /// <param name="embeddingService">Embedding service for vector operations</param>
        /// <param name="dimensions">Vector dimensions (default: 1536)</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>Semantic SQLite storage instance</returns>
        public object CreateSemanticMemoryStorage(
            string connectionString,
            object embeddingService,
            int dimensions = 1536,
            StorageConfiguration config = null)
        {
            var connString = connectionString ?? _defaultConnectionString;
            
            if (!(embeddingService is IEmbeddingService embeddings))
            {
                throw new ArgumentException("Embedding service must implement IEmbeddingService", nameof(embeddingService));
            }

            return new SemanticSqliteStorage(connString, embeddings, dimensions);
        }

        /// <summary>
        /// Checks if SQLite and sqlite-vec are available
        /// </summary>
        /// <returns>True if SQLite storage is available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                // Test basic SQLite availability
                var testStorage = new SqliteSessionStorage(":memory:");
                
                // Test sqlite-vec availability
                var status = Utils.SqliteVecHelper.CheckInstallationStatus();
                var isVecAvailable = status.IsInstalled;
                
                return await Task.FromResult(true); // Basic SQLite is always available
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets information about this storage provider
        /// </summary>
        /// <returns>Provider information</returns>
        public StorageProviderInfo GetProviderInfo()
        {
            return new StorageProviderInfo
            {
                Name = ProviderName,
                Description = "SQLite storage provider with vector search support via sqlite-vec extension",
                Version = "1.0.0",
                SupportsTransactions = true,
                SupportsVectorSearch = Utils.SqliteVecHelper.CheckInstallationStatus().IsInstalled,
                SupportsFullTextSearch = true,
                IsPersistent = true,
                SupportsConcurrentAccess = true
            };
        }
    }
}