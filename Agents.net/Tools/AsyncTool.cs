using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agents.net.Utils;

namespace Agents.net.Tools
{
    /// <summary>
    /// Base class for asynchronous tools that can be called by AI models.
    /// </summary>
    /// <remarks>
    /// AsyncTool extends the Tool class to support asynchronous operations,
    /// allowing for non-blocking function calls that can perform I/O operations
    /// or other long-running tasks.
    /// </remarks>
    public class AsyncTool : Tool
    {
        private readonly Func<Dictionary<string, object>, CancellationToken, Task<string>> _asyncFunc;
        private bool _enableCache;
        private int _cacheTtl;
        private readonly CacheManager<string, string> _cache;

        /// <summary>
        /// Initializes a new instance of the AsyncTool class with a method info.
        /// </summary>
        /// <param name="name">The name of the tool</param>
        /// <param name="description">A description of what the tool does</param>
        /// <param name="methodInfo">The MethodInfo object representing the method to invoke</param>
        /// <param name="instance">The object instance on which to invoke the method</param>
        public AsyncTool(string name, string description, MethodInfo methodInfo, object instance) 
            : base(name, description, methodInfo, instance)
        {
            // Forcing IsAsync to true to maintain compatibility
            IsAsync = true;
            _cache = new CacheManager<string, string>(300); // Default 5 minutes
        }
        
        /// <summary>
        /// Initializes a new instance of the AsyncTool class with an async function.
        /// </summary>
        /// <param name="name">The name of the tool</param>
        /// <param name="description">A description of what the tool does</param>
        /// <param name="func">The async function to execute</param>
        public AsyncTool(
            string name, 
            string description,
            Func<Dictionary<string, object>, CancellationToken, Task<string>> func)
            : base(name, description, typeof(AsyncTool).GetMethod(nameof(ExecuteFunc), BindingFlags.Instance | BindingFlags.NonPublic), null)
        {
            _asyncFunc = func ?? throw new ArgumentNullException(nameof(func));
            Instance = this; // Set instance to self for method invocation
            IsAsync = true;
            _cache = new CacheManager<string, string>(300); // Default 5 minutes
        }
        
        /// <summary>
        /// Enables caching for this async tool
        /// </summary>
        /// <param name="ttlSeconds">Time-to-live in seconds</param>
        public void EnableCache(int ttlSeconds = 300)
        {
            _enableCache = true;
            _cacheTtl = ttlSeconds;
        }
        
        /// <summary>
        /// Disables caching for this async tool
        /// </summary>
        public void DisableCache()
        {
            _enableCache = false;
        }
        
        /// <summary>
        /// Executes the async function with the given parameters.
        /// </summary>
        /// <param name="parameters">Dictionary of parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the async function call</returns>
        private async Task<string> ExecuteFunc(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await _asyncFunc(parameters, cancellationToken);
        }
        
        /// <summary>
        /// Executes the tool asynchronously with the provided parameters.
        /// </summary>
        /// <param name="argumentsJson">JSON string containing the arguments</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the method call</returns>
        public override async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            if (_enableCache)
            {
                // Check cache first
                if (_cache.TryGet(argumentsJson, out string cachedResult))
                {
                    Logger.Debug($"Cache hit for {Name} with args: {argumentsJson}");
                    return cachedResult;
                }
            }
            
            string result = await base.ExecuteAsync(argumentsJson, cancellationToken);
            
            if (_enableCache)
            {
                // Store result in cache
                _cache.Set(argumentsJson, result, _cacheTtl);
                Logger.Debug($"Cached result for {Name} with args: {argumentsJson}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute the tool asynchronously with the provided parameters dictionary.
        /// </summary>
        /// <param name="parameters">Dictionary containing the parameters</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the method call</returns>
        public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            if (_asyncFunc != null)
            {
                if (_enableCache)
                {
                    // Generate a cache key
                    StringBuilder keyBuilder = new StringBuilder();
                    foreach (var pair in parameters)
                    {
                        keyBuilder.Append($"{pair.Key}={pair.Value};");
                    }
                    string cacheKey = keyBuilder.ToString();
                    
                    // Check cache first
                    if (_cache.TryGet(cacheKey, out string cachedResult))
                    {
                        Logger.Debug($"Cache hit for {Name} with parameters dictionary");
                        return cachedResult;
                    }
                    
                    // Execute function
                    string result = await _asyncFunc(parameters, cancellationToken);
                    
                    // Store result in cache
                    _cache.Set(cacheKey, result, _cacheTtl);
                    Logger.Debug($"Cached result for {Name} with parameters dictionary");
                    
                    return result;
                }
                else
                {
                    return await _asyncFunc(parameters, cancellationToken);
                }
            }
            
            return await base.ExecuteAsync(parameters, cancellationToken);
        }
        
        /// <summary>
        /// Creates an AsyncTool instance from a method with async return type.
        /// </summary>
        /// <param name="method">The method to wrap as a tool</param>
        /// <param name="instance">The instance on which to invoke the method</param>
        /// <param name="description">The description of the tool</param>
        /// <returns>A configured AsyncTool instance</returns>
        public static AsyncTool FromMethod(MethodInfo method, object instance, string description)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));
                
            // Verify this is an async method
            bool isAsync = method.ReturnType == typeof(Task) ||
                          (method.ReturnType.IsGenericType && 
                           method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
                           
            if (!isAsync)
            {
                throw new ArgumentException("Method must have an async return type (Task or Task<T>)", nameof(method));
            }

            return new AsyncTool(method.Name, description, method, instance);
        }
    }
}