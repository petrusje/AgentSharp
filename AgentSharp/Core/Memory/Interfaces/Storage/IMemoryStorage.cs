using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de memórias.
    /// </summary>
    public interface IMemoryStorage
    {
        Task InitializeAsync();
        Task<string> AddMemoryAsync(UserMemory memory);
        Task<UserMemory> GetMemoryAsync(string memoryId);
        Task<List<UserMemory>> GetMemoriesAsync(string userId, string sessionId = null);
        Task UpdateMemoryAsync(UserMemory memory);
        Task DeleteMemoryAsync(string memoryId);
        Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit = 10);
        void Clear();
    }
}
