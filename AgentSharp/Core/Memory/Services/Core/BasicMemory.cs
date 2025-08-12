using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Basic in-memory implementation of IMemory for simple caching and temporary storage.
    /// Used primarily for workflow orchestration and lightweight memory requirements.
    /// </summary>
    public class BasicMemory : IMemory
    {
        private readonly List<MemoryItem> _items = new List<MemoryItem>();
        private readonly object _lock = new object();

        public Task AddItemAsync(MemoryItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_lock)
            {
                _items.Add(item);
            }

            return Task.CompletedTask;
        }

        public Task<List<MemoryItem>> GetItemsAsync(string type = null, int limit = 10)
        {
            lock (_lock)
            {
                var query = _items.AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(item => item.Type == type);
                }

                var result = query
                    .OrderByDescending(item => item.Timestamp)
                    .Take(limit)
                    .ToList();

                return Task.FromResult(result);
            }
        }

        public Task RemoveItemAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Task.CompletedTask;

            lock (_lock)
            {
                var item = _items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _items.Remove(item);
                }
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            lock (_lock)
            {
                _items.Clear();
            }

            return Task.CompletedTask;
        }
    }
}