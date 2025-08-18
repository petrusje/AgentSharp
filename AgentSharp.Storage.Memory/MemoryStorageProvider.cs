using System;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Interfaces;

namespace AgentSharp.Storage.Memory
{
    /// <summary>
    /// In-memory storage provider implementation for development and testing
    /// </summary>
    public class MemoryStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Gets the unique name of this provider
        /// </summary>
        public string ProviderName => "memory";

        /// <summary>
        /// Creates a general in-memory storage instance
        /// </summary>
        /// <param name="connectionString">Ignored for in-memory storage</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>In-memory storage instance</returns>
        public object CreateStorage(string connectionString = null, StorageConfiguration config = null)
        {
            // Return a full IStorage implementation
            return new InMemoryStorage();
        }

        /// <summary>
        /// Creates an in-memory storage instance for memory operations
        /// </summary>
        /// <param name="connectionString">Ignored for in-memory storage</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>In-memory storage instance</returns>
        public object CreateMemoryStorage(string connectionString = null, StorageConfiguration config = null)
        {
            // For memory storage, return the same in-memory storage that implements IStorage
            return new InMemoryStorage();
        }

        /// <summary>
        /// Creates an in-memory semantic storage with embedding service
        /// </summary>
        /// <param name="embeddingService">Embedding service for vector operations</param>
        /// <param name="config">Storage configuration</param>
        /// <returns>In-memory semantic storage instance</returns>
        public object CreateSemanticMemoryStorage(
            object embeddingService,
            StorageConfiguration config = null)
        {
            if (embeddingService == null)
            {
                throw new ArgumentNullException(nameof(embeddingService));
            }

            // Create an in-memory storage with semantic capabilities
            // Note: The current InMemoryStorage provides basic functionality
            // In a full implementation, you might want enhanced vector operations with the embedding service
            return new InMemoryStorage();
        }

        /// <summary>
        /// Checks if in-memory storage is available (always true)
        /// </summary>
        /// <returns>Always true - in-memory storage is always available</returns>
        public async Task<bool> IsAvailableAsync()
        {
            return await Task.FromResult(true);
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
                Description = "In-memory storage provider for development, testing, and temporary data",
                Version = "1.0.0",
                SupportsTransactions = false,
                SupportsVectorSearch = false, // Basic in-memory doesn't support vector search
                SupportsFullTextSearch = false,
                IsPersistent = false,
                SupportsConcurrentAccess = true
            };
        }
    }
}