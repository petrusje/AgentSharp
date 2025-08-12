using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Simple in-memory store for temporary data
    /// </summary>
    public class InMemoryStore
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        public T Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var value) && value is T)
            {
                return (T)value;
            }
            return default(T);
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _data.Remove(key);
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}