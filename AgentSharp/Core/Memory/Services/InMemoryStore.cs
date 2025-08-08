using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory
{
    /// <summary>
    /// Implementação em memória da interface IMemory
    /// </summary>
    public class InMemoryStore : IMemory
    {
        private readonly ConcurrentDictionary<string, MemoryItem> _items = new ConcurrentDictionary<string, MemoryItem>();

        public Task AddItemAsync(MemoryItem item)
        {
            if (item == null) return Task.CompletedTask;

            _items[item.Id] = item;
            return Task.CompletedTask;
        }

        public Task<List<MemoryItem>> GetItemsAsync(string type = null, int limit = 10)
        {
            var items = _items.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(type))
            {
                items = items.Where(i => i.Type == type);
            }

            var result = items.Take(limit).ToList();
            return Task.FromResult(result);
        }

        public Task RemoveItemAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return Task.CompletedTask;

            _items.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            _items.Clear();
            return Task.CompletedTask;
        }
    }
}
