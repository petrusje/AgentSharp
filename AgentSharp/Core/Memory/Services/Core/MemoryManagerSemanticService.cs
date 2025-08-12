using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Semantic memory service implementation that delegates to MemoryManager.
    /// Provides semantic memory features when enabled.
    /// </summary>
    public class MemoryManagerSemanticService : ISemanticMemoryService
    {
        private readonly IMemoryManager _memoryManager;

        public MemoryManagerSemanticService(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public bool IsEnabled => _memoryManager != null;

        public async Task<List<UserMemory>> ExtractMemoriesAsync(string userMessage, string assistantResponse, MemoryContext context)
        {
            if (_memoryManager == null)
                return new List<UserMemory>();

            // Add the memory from the conversation
            await _memoryManager.AddMemoryAsync($"User: {userMessage} | Assistant: {assistantResponse}", context);
            
            // Return existing memories as a proxy for extraction
            return await _memoryManager.GetExistingMemoriesAsync(context, 5);
        }

        public async Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10)
        {
            if (_memoryManager == null)
                return new List<UserMemory>();

            // MemoryManager doesn't have direct search, use existing memories
            return await _memoryManager.GetExistingMemoriesAsync(context, limit);
        }

        public async Task<string> AddMemoryAsync(UserMemory memory)
        {
            if (_memoryManager == null)
                return memory.Id;

            return await _memoryManager.AddMemoryAsync(memory.Content, new MemoryContext
            {
                UserId = memory.UserId,
                SessionId = memory.SessionId
            });
        }

        public async Task<List<UserMemory>> GetExistingMemoriesAsync(MemoryContext context, int limit = 10)
        {
            if (_memoryManager == null)
                return new List<UserMemory>();

            return await _memoryManager.GetExistingMemoriesAsync(context, limit);
        }

        public async Task UpdateMemoryAsync(UserMemory memory)
        {
            if (_memoryManager == null)
                return;

            // MemoryManager doesn't have direct update, so we add as new
            // TODO: Implement proper update in MemoryManager
            await _memoryManager.AddMemoryAsync(memory.Content, new MemoryContext
            {
                UserId = memory.UserId,
                SessionId = memory.SessionId
            });
        }

        public async Task DeleteMemoryAsync(string memoryId)
        {
            if (_memoryManager == null)
                return;

            // TODO: Implement delete in MemoryManager
            await Task.CompletedTask;
        }

        public async Task<string> GenerateMemorySummaryAsync(MemoryContext context)
        {
            if (_memoryManager == null)
                return string.Empty;

            // MemoryManager doesn't have summary method, return basic summary
            var memories = await _memoryManager.GetExistingMemoriesAsync(context, 10);
            return $"Found {memories.Count} memories for user {context?.UserId}";
        }

        public async Task ClearMemoriesAsync(MemoryContext context)
        {
            if (_memoryManager == null)
                return;

            // MemoryManager doesn't have clear method - this is a limitation
            // TODO: Implement clear in MemoryManager interface
            await Task.CompletedTask;
        }
    }
}