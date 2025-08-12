using AgentSharp.Core.Memory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Advanced semantic memory service with embeddings and contextual storage.
    /// High-cost service that processes semantic meaning and relationships.
    /// </summary>
    public interface ISemanticMemoryService
    {
        /// <summary>
        /// Extract and store semantic memories from conversation
        /// </summary>
        Task<List<UserMemory>> ExtractMemoriesAsync(string userMessage, string assistantResponse, MemoryContext context);

        /// <summary>
        /// Search memories by semantic similarity
        /// </summary>
        Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10);

        /// <summary>
        /// Add a specific memory with semantic processing
        /// </summary>
        Task<string> AddMemoryAsync(UserMemory memory);

        /// <summary>
        /// Get existing memories for context loading
        /// </summary>
        Task<List<UserMemory>> GetExistingMemoriesAsync(MemoryContext context, int limit = 10);

        /// <summary>
        /// Update memory with new information
        /// </summary>
        Task UpdateMemoryAsync(UserMemory memory);

        /// <summary>
        /// Delete a specific memory
        /// </summary>
        Task DeleteMemoryAsync(string memoryId);

        /// <summary>
        /// Generate intelligent summary of user's memories
        /// </summary>
        Task<string> GenerateMemorySummaryAsync(MemoryContext context);

        /// <summary>
        /// Clear all memories for a context
        /// </summary>
        Task ClearMemoriesAsync(MemoryContext context);

        /// <summary>
        /// Check if semantic memory is enabled and functional
        /// </summary>
        bool IsEnabled { get; }
    }
}