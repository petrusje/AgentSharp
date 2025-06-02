using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agents.net.Core.Memory
{
    /// <summary>
    /// Interface principal para armazenamento de memória
    /// </summary>
    public interface IMemory
    {
        Task<string> AddItemAsync(IMemoryItem item);
        Task<IMemoryItem> GetItemAsync(string id);
        Task<IEnumerable<IMemoryItem>> SearchAsync(string query, int limit = 10);
        Task<IEnumerable<IMemoryItem>> GetRecentItemsAsync(int limit = 10, string itemType = null);
        Task ClearAsync();
    }
}