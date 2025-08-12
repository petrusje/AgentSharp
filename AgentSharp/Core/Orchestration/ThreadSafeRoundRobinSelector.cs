using System;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Thread-safe round-robin selector for agent routing.
    /// Prevents memory leaks and race conditions in concurrent scenarios.
    /// </summary>
    public class ThreadSafeRoundRobinSelector
    {
        private int _counter = 0;
        private readonly object _lock = new object();

        /// <summary>
        /// Selects the next item in round-robin fashion
        /// </summary>
        /// <typeparam name="T">Type of items to select from</typeparam>
        /// <param name="items">Array of items to select from</param>
        /// <returns>The selected item</returns>
        /// <exception cref="ArgumentNullException">Thrown when items is null</exception>
        /// <exception cref="ArgumentException">Thrown when items is empty</exception>
        public T SelectNext<T>(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                throw new ArgumentException("Items array cannot be empty", nameof(items));

            lock (_lock)
            {
                var index = _counter % items.Length;
                
                // Prevent integer overflow by resetting counter
                _counter = (_counter + 1) % items.Length;
                
                return items[index];
            }
        }

        /// <summary>
        /// Gets the current counter value (for debugging purposes)
        /// </summary>
        /// <returns>Current counter value</returns>
        public int GetCurrentCounter()
        {
            lock (_lock)
            {
                return _counter;
            }
        }

        /// <summary>
        /// Resets the counter to zero
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _counter = 0;
            }
        }
    }
}