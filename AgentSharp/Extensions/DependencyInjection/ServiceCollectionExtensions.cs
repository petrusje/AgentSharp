using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AgentSharp.Core;
using AgentSharp.Core.Abstractions;

namespace AgentSharp.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering AgentSharp services in dependency injection container
    /// Compatible with .NET Standard 2.0 using Microsoft.Extensions.DependencyInjection package
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds core AgentSharp services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgentSharp(
            this IServiceCollection services,
            Action<AgentSharpBuilder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));


            // Register default telemetry service if none provided
            services.AddSingleton<ITelemetryService>(provider =>
            {
                // Try to get existing telemetry service, otherwise create default
                return provider.GetService<ITelemetryService>() ?? new DefaultTelemetryService();
            });

            // Configure using builder pattern
            var builder = new AgentSharpBuilder(services);
            configure?.Invoke(builder);

            return services;
        }

        /// <summary>
        /// Adds a custom model provider to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="provider">The model provider instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddModelProvider(
            this IServiceCollection services,
            IModelProvider provider)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a custom storage provider to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="provider">The storage provider instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddStorageProvider(
            this IServiceCollection services,
            IStorageProvider provider)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a custom embedding provider to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="provider">The embedding provider instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddEmbeddingProvider(
            this IServiceCollection services,
            IEmbeddingProvider provider)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            services.AddSingleton(provider);
            return services;
        }

        /// <summary>
        /// Adds a custom telemetry service to the service collection
        /// </summary>
        /// <typeparam name="T">The telemetry service type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTelemetryService<T>(
            this IServiceCollection services)
            where T : class, ITelemetryService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ITelemetryService, T>();
            return services;
        }

        /// <summary>
        /// Adds a custom telemetry service instance to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="telemetryService">The telemetry service instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTelemetryService(
            this IServiceCollection services,
            ITelemetryService telemetryService)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (telemetryService == null)
                throw new ArgumentNullException(nameof(telemetryService));

            services.AddSingleton(telemetryService);
            return services;
        }
    }

    /// <summary>
    /// Default telemetry service implementation for .NET Standard 2.0
    /// </summary>
    internal class DefaultTelemetryService : ITelemetryService
    {
        private readonly Dictionary<string, DateTime> _operations = new Dictionary<string, DateTime>();
        private readonly object _lock = new object();

        public bool IsEnabled { get; } = true;

        public void StartOperation(string operationId)
        {
            lock (_lock)
            {
                _operations[operationId] = DateTime.UtcNow;
            }
        }

        public double EndOperation(string operationId, bool displayResult = false)
        {
            lock (_lock)
            {
                if (_operations.TryGetValue(operationId, out var startTime))
                {
                    _operations.Remove(operationId);
                    var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                    if (displayResult)
                    {
                        Console.WriteLine($"Operation {operationId} completed in {elapsed:F2}s");
                    }
                    return elapsed;
                }
                return 0;
            }
        }

        public void TrackLLMRequest(string operationId) { }

        public void CompleteLLMRequest(string operationId, int tokenCount, double costInTokens = 0.0) { }

        public void TrackMemoryOperation(string operation, double elapsedSeconds) { }

        public void TrackMemoryOperation(string operation, double elapsedSeconds, int tokenCount) { }

        public void TrackEmbeddingOperation(string operation, double elapsedSeconds, int tokenCount) { }

        public void TrackToolExecution(string toolName, double elapsedSeconds) { }

        public void TrackToolExecution(string toolName, double elapsedSeconds, int tokenCount) { }

        public TelemetrySummary GetSummary()
        {
            return new TelemetrySummary
            {
                TotalEvents = 0,
                TotalElapsedSeconds = 0,
                AverageElapsedSeconds = 0,
                LLMEvents = 0,
                MemoryEvents = 0,
                ToolEvents = 0,
                TotalLLMTime = 0,
                TotalMemoryTime = 0,
                TotalToolTime = 0,
                TotalTokens = 0,
                LLMTokens = 0,
                MemoryTokens = 0,
                EmbeddingTokens = 0,
                ToolTokens = 0,
                TotalCostInTokens = 0
            };
        }

        public void Clear()
        {
            lock (_lock)
            {
                _operations.Clear();
            }
        }
    }
}