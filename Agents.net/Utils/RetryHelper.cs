using System;
using System.Threading;
using System.Threading.Tasks;
using Agents.net.Exceptions;

namespace Agents.net.Utils
{
    /// <summary>
    /// Helper class for executing operations with automated retry logic.
    /// Provides resilience against transient failures in network operations and API calls.
    /// </summary>
    /// <remarks>
    /// This utility implements exponential backoff retry pattern for handling temporary failures,
    /// particularly useful for network requests to AI model providers that might experience
    /// temporary unavailability or rate limiting.
    /// </remarks>
    public static class RetryHelper
    {
        /// <summary>
        /// Executes an asynchronous operation with configurable retry policies.
        /// </summary>
        /// <typeparam name="T">Type of operation result</typeparam>
        /// <param name="operation">The asynchronous operation to execute</param>
        /// <param name="retryCount">Maximum number of retry attempts (default: 3)</param>
        /// <param name="retryDelay">Base wait time between attempts in milliseconds (default: 1000ms)</param>
        /// <param name="shouldRetry">Predicate function that determines if an exception justifies retry</param>
        /// <param name="cancellationToken">Cancellation token to abort the operation</param>
        /// <returns>The result of the operation if successful</returns>
        /// <exception cref="ModelException">Thrown when all retry attempts fail</exception>
        /// <exception cref="AgentsException">Any library-specific exception is immediately propagated without retry</exception>
        /// <remarks>
        /// The retry delay uses exponential backoff, where each subsequent retry waits
        /// longer than the previous one (delay * 2^retryAttempt). This helps prevent
        /// overwhelming services that may be experiencing high load.
        /// 
        /// By default, the operation will retry on common transient errors like HttpRequestException,
        /// WebException, and TimeoutException. You can customize this behavior by providing your own
        /// shouldRetry function.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Basic usage with default retry settings
        /// var result = await RetryHelper.ExecuteWithRetryAsync(
        ///     async () => await _client.GetDataAsync()
        /// );
        /// 
        /// // Custom retry settings
        /// var result = await RetryHelper.ExecuteWithRetryAsync(
        ///     operation: async () => await _client.GetDataAsync(),
        ///     retryCount: 5,
        ///     retryDelay: 2000,
        ///     shouldRetry: ex => ex is CustomTransientException
        /// );
        /// </code>
        /// </example>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int retryCount = 3,
            int retryDelay = 1000,
            Func<Exception, bool> shouldRetry = null,
            CancellationToken cancellationToken = default)
        {
            if (shouldRetry == null)
            {
                shouldRetry = ex => ex is System.Net.Http.HttpRequestException 
                                || ex is System.Net.WebException
                                || ex is TimeoutException;
            }
            int currentRetry = 0;
            
            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    
                    if (currentRetry > retryCount || !shouldRetry(ex) || cancellationToken.IsCancellationRequested)
                    {
                        if (ex is AgentsException)
                            throw;
                            
                        throw new ModelException($"Operation failed after {currentRetry} attempts", ex);
                    }
                    
                    // Wait before next attempt (exponential backoff)
                    int delay = retryDelay * (int)Math.Pow(2, currentRetry - 1);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}