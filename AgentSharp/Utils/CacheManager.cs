using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AgentSharp.Utils
{
  /// <summary>
  /// Generic cache manager with automatic expiration of items.
  /// Provides memory caching to improve performance and reduce redundant operations.
  /// </summary>
  /// <typeparam name="TKey">The type of keys in the cache</typeparam>
  /// <typeparam name="TValue">The type of values in the cache</typeparam>
  /// <remarks>
  /// This is a lightweight in-memory cache implementation with automatic cleanup of expired items.
  /// It's particularly useful for caching expensive AI model responses or API results.
  /// The implementation is thread-safe using ConcurrentDictionary for the underlying storage.
  /// </remarks>
  public class CacheManager<TKey, TValue>
  {
    private readonly ConcurrentDictionary<TKey, CacheItem> _cache = new ConcurrentDictionary<TKey, CacheItem>();
    private readonly TimeSpan _defaultExpiration;
    private readonly Timer _cleanupTimer;
    private const int DefaultCleanupIntervalMinutes = 5;

    /// <summary>
    /// Represents a cache item with its value and expiration information.
    /// </summary>
    private class CacheItem
    {
      /// <summary>
      /// Gets or sets the cached value.
      /// </summary>
      public TValue Value { get; set; }

      /// <summary>
      /// Gets or sets the UTC time when this cache item expires.
      /// </summary>
      public DateTime ExpirationTime { get; set; }

      /// <summary>
      /// Gets a value indicating whether this cache item has expired.
      /// </summary>
      public bool IsExpired => DateTime.UtcNow >= ExpirationTime;
    }

    /// <summary>
    /// Initializes a new instance of the CacheManager class with specified expiration settings.
    /// </summary>
    /// <param name="defaultExpirationMinutes">Default expiration time for cache items in minutes (default: 30)</param>
    /// <param name="cleanupIntervalMinutes">Interval for cleanup of expired items in minutes (default: 5)</param>
    /// <remarks>
    /// The cleanup timer will automatically remove expired items from the cache at the
    /// specified interval, preventing memory leaks from accumulated stale items.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a cache with 10-minute expiration and 2-minute cleanup
    /// var cache = new CacheManager&lt;string, UserProfile&gt;(10, 2);
    /// </code>
    /// </example>
    public CacheManager(int defaultExpirationMinutes = 30, int cleanupIntervalMinutes = DefaultCleanupIntervalMinutes)
    {
      _defaultExpiration = TimeSpan.FromMinutes(defaultExpirationMinutes);
      _cleanupTimer = new Timer(CleanupExpiredItems, null,
          TimeSpan.FromMinutes(cleanupIntervalMinutes),
          TimeSpan.FromMinutes(cleanupIntervalMinutes));
    }

    /// <summary>
    /// Adds or updates an item in the cache with optional custom expiration time.
    /// </summary>
    /// <param name="key">The key to identify the cache item</param>
    /// <param name="value">The value to be cached</param>
    /// <param name="expirationMinutes">Custom expiration time in minutes (null for default expiration)</param>
    /// <remarks>
    /// If the key already exists in the cache, its value and expiration will be updated.
    /// Expiration is calculated from the current time plus the specified expiration period.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Cache a value with default expiration
    /// cache.Set("user:123", userProfile);
    ///
    /// // Cache a value with custom expiration (60 minutes)
    /// cache.Set("api:response", apiData, 60);
    /// </code>
    /// </example>
    public void Set(TKey key, TValue value, int? expirationMinutes = null)
    {
      var expiration = DateTime.UtcNow + (expirationMinutes.HasValue
          ? TimeSpan.FromMinutes(expirationMinutes.Value)
          : _defaultExpiration);

      _cache[key] = new CacheItem
      {
        Value = value,
        ExpirationTime = expiration
      };
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache.
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <param name="value">When this method returns, contains the object from the cache that has the specified key,
    /// or the default value of the TValue type if the operation failed</param>
    /// <returns>true if the key was found and value is not expired; otherwise, false</returns>
    /// <remarks>
    /// This method only returns a value if it exists in the cache and has not expired.
    /// It doesn't remove expired items - that happens in the cleanup cycle.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (cache.TryGet("user:123", out var userProfile))
    /// {
    ///     // Use the cached profile
    ///     Console.WriteLine($"Found user: {userProfile.Name}");
    /// }
    /// else
    /// {
    ///     // Cache miss, need to fetch the data
    ///     userProfile = await dataService.GetUserProfileAsync("123");
    ///     cache.Set("user:123", userProfile);
    /// }
    /// </code>
    /// </example>
    public bool TryGet(TKey key, out TValue value)
    {
      value = default;

      if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
      {
        value = item.Value;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove</param>
    /// <returns>true if the item was successfully removed; otherwise, false</returns>
    /// <remarks>
    /// Returns false if the key was not found in the cache.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Invalidate a cached item manually
    /// cache.Remove("user:123");
    /// </code>
    /// </example>
    public bool Remove(TKey key)
    {
      return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Removes all items from the cache.
    /// </summary>
    /// <remarks>
    /// This operation immediately clears the entire cache, regardless of expiration times.
    /// Use this method when you need to invalidate all cached data at once.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear entire cache, for example after a major data update
    /// cache.Clear();
    /// </code>
    /// </example>
    public void Clear()
    {
      _cache.Clear();
    }

    /// <summary>
    /// Automatically removes expired items from the cache at regular intervals.
    /// </summary>
    /// <param name="state">State object (not used)</param>
    /// <remarks>
    /// This method is invoked by the internal timer to perform periodic cache maintenance.
    /// The cleanup process prevents the cache from growing indefinitely with expired items.
    /// </remarks>
    private void CleanupExpiredItems(object state)
    {
      var keysToRemove = new List<TKey>();

      foreach (var kvp in _cache)
      {
        if (kvp.Value.IsExpired)
        {
          keysToRemove.Add(kvp.Key);
        }
      }

      foreach (var key in keysToRemove)
      {
        _cache.TryRemove(key, out _);
      }

      Logger.Debug($"Cache cleanup: {keysToRemove.Count} expired items removed");
    }

    /// <summary>
    /// Releases resources used by the cache manager.
    /// </summary>
    /// <remarks>
    /// This disposes the cleanup timer to prevent memory leaks.
    /// Call this method when the cache is no longer needed.
    /// </remarks>
    public void Dispose()
    {
      _cleanupTimer?.Dispose();
    }
  }
}
