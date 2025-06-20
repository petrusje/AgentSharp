using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Arcana.AgentsNet.Core.Memory
{
  /// <summary>
  /// Implementação de memória em memória RAM
  /// </summary>
  public class InMemoryStore : IMemory
  {
    private readonly ConcurrentDictionary<string, IMemoryItem> _items = new ConcurrentDictionary<string, IMemoryItem>();

    public Task<string> AddItemAsync(IMemoryItem item)
    {
      _items[item.Id] = item;
      return Task.FromResult(item.Id);
    }

    public Task<IMemoryItem> GetItemAsync(string id)
    {
      _items.TryGetValue(id, out var item);
      return Task.FromResult(item);
    }

    public Task<IEnumerable<IMemoryItem>> SearchAsync(string query, int limit = 10)
    {
      var results = _items.Values
          .Where(i => i.Content != null && i.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
          .OrderByDescending(i => i.Relevance)
          .ThenByDescending(i => i.Timestamp)
          .Take(limit);

      return Task.FromResult(results);
    }

    public Task<IEnumerable<IMemoryItem>> GetRecentItemsAsync(int limit = 10, string itemType = null)
    {
      var query = _items.Values.AsEnumerable();

      if (!string.IsNullOrEmpty(itemType))
        query = query.Where(i => i.Type == itemType);

      var results = query
          .OrderByDescending(i => i.Timestamp)
          .Take(limit);

      return Task.FromResult(results);
    }

    public Task ClearAsync()
    {
      _items.Clear();
      return Task.CompletedTask;
    }
  }
}
