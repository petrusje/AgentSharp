using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using AgentSharp.Core;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models.Interfaces;

namespace AgentSharp.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder for configuring AgentSharp services using a fluent interface
    /// </summary>
    public class AgentSharpBuilder
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Initializes a new instance of AgentSharpBuilder
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        public AgentSharpBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the underlying service collection
        /// </summary>
        public IServiceCollection Services => _services;

        /// <summary>
        /// Adds OpenAI provider to the services
        /// </summary>
        /// <param name="apiKey">OpenAI API key</param>
        /// <param name="endpoint">Optional custom endpoint (for Azure OpenAI)</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithOpenAI(string apiKey, string endpoint = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            // Note: This will be implemented when OpenAI provider DLL is created
            // For now, we'll add a placeholder registration
            _services.AddSingleton<IModelProvider>(provider =>
                CreateOpenAIModelProvider(apiKey, endpoint));

            _services.AddSingleton<IEmbeddingProvider>(provider =>
                CreateOpenAIEmbeddingProvider(apiKey, endpoint));

            return this;
        }

        /// <summary>
        /// Adds Ollama provider to the services
        /// </summary>
        /// <param name="baseUrl">Ollama server base URL</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithOllama(string baseUrl = "http://localhost:11434")
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

            // Register IOllamaClient
            _services.AddSingleton<IOllamaClient>(provider =>
                CreateOllamaClient(baseUrl));

            // Register providers
            _services.AddSingleton<IModelProvider>(provider =>
                CreateOllamaModelProvider(provider.GetRequiredService<IOllamaClient>()));

            return this;
        }

        /// <summary>
        /// Adds SQLite storage provider to the services
        /// </summary>
        /// <param name="connectionString">SQLite connection string or file path</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithSqliteStorage(string connectionString = ":memory:")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            _services.AddSingleton<IStorageProvider>(provider =>
                CreateSqliteStorageProvider(connectionString));

            return this;
        }

        /// <summary>
        /// Adds in-memory storage provider to the services
        /// </summary>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithMemoryStorage()
        {
            _services.AddSingleton<IStorageProvider>(provider =>
                CreateMemoryStorageProvider());

            return this;
        }

        /// <summary>
        /// Adds a custom telemetry service to the services
        /// </summary>
        /// <typeparam name="T">The telemetry service type</typeparam>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithTelemetry<T>() where T : class, ITelemetryService
        {
            _services.AddSingleton<ITelemetryService, T>();
            return this;
        }

        /// <summary>
        /// Adds a custom telemetry service instance to the services
        /// </summary>
        /// <param name="telemetryService">The telemetry service instance</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithTelemetry(ITelemetryService telemetryService)
        {
            if (telemetryService == null)
                throw new ArgumentNullException(nameof(telemetryService));

            _services.AddSingleton(telemetryService);
            return this;
        }

        /// <summary>
        /// Adds a custom model provider to the services
        /// </summary>
        /// <param name="provider">The model provider instance</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithModelProvider(IModelProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _services.AddSingleton(provider);
            return this;
        }

        /// <summary>
        /// Adds a custom storage provider to the services
        /// </summary>
        /// <param name="provider">The storage provider instance</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithStorageProvider(IStorageProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _services.AddSingleton(provider);
            return this;
        }

        /// <summary>
        /// Adds a custom embedding provider to the services
        /// </summary>
        /// <param name="provider">The embedding provider instance</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder WithEmbeddingProvider(IEmbeddingProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _services.AddSingleton(provider);
            return this;
        }

        /// <summary>
        /// Configures services from configuration
        /// </summary>
        /// <param name="configureAction">Configuration action</param>
        /// <returns>The builder for chaining</returns>
        public AgentSharpBuilder Configure(Action<AgentSharpOptions> configureAction)
        {
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));

            var options = new AgentSharpOptions();
            configureAction(options);

            // Apply configuration
            ApplyOptions(options);

            return this;
        }

        #region Private Helper Methods

        private void ApplyOptions(AgentSharpOptions options)
        {
            // Apply OpenAI configuration
            if (options.OpenAI != null && !string.IsNullOrWhiteSpace(options.OpenAI.ApiKey))
            {
                WithOpenAI(options.OpenAI.ApiKey, options.OpenAI.Endpoint);
            }

            // Apply Ollama configuration
            if (options.Ollama != null && !string.IsNullOrWhiteSpace(options.Ollama.BaseUrl))
            {
                WithOllama(options.Ollama.BaseUrl);
            }

            // Apply storage configuration
            if (options.Storage != null)
            {
                if (options.Storage.UseSqlite && !string.IsNullOrWhiteSpace(options.Storage.SqliteConnectionString))
                {
                    WithSqliteStorage(options.Storage.SqliteConnectionString);
                }

                if (options.Storage.UseMemory)
                {
                    WithMemoryStorage();
                }
            }
        }

        // Factory methods for creating concrete provider instances using reflection
        private static IModelProvider CreateOpenAIModelProvider(string apiKey, string endpoint)
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Providers.OpenAI.dll");
                var type = assembly.GetType("AgentSharp.Providers.OpenAI.OpenAIModelProvider");
                if (type != null)
                {
                    return (IModelProvider)Activator.CreateInstance(type, apiKey, endpoint);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.OpenAI.OpenAIModelProvider");
                    if (type != null)
                    {
                        return (IModelProvider)Activator.CreateInstance(type, apiKey, endpoint);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("OpenAI provider not found. Ensure AgentSharp.Providers.OpenAI.dll is available.");
        }

        private static IEmbeddingProvider CreateOpenAIEmbeddingProvider(string apiKey, string endpoint)
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Providers.OpenAI.dll");
                var type = assembly.GetType("AgentSharp.Providers.OpenAI.OpenAIEmbeddingProvider");
                if (type != null)
                {
                    return (IEmbeddingProvider)Activator.CreateInstance(type, apiKey, endpoint);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.OpenAI.OpenAIEmbeddingProvider");
                    if (type != null)
                    {
                        return (IEmbeddingProvider)Activator.CreateInstance(type, apiKey, endpoint);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("OpenAI embedding provider not found. Ensure AgentSharp.Providers.OpenAI.dll is available.");
        }

        private static IOllamaClient CreateOllamaClient(string baseUrl)
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Providers.Ollama.dll");
                var type = assembly.GetType("AgentSharp.Providers.Ollama.OllamaClient");
                if (type != null)
                {
                    return (IOllamaClient)Activator.CreateInstance(type, baseUrl);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.Ollama.OllamaClient");
                    if (type != null)
                    {
                        return (IOllamaClient)Activator.CreateInstance(type, baseUrl);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("Ollama client not found. Ensure AgentSharp.Providers.Ollama.dll is available.");
        }

        private static IModelProvider CreateOllamaModelProvider(IOllamaClient client)
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Providers.Ollama.dll");
                var type = assembly.GetType("AgentSharp.Providers.Ollama.OllamaModelProvider");
                if (type != null)
                {
                    return (IModelProvider)Activator.CreateInstance(type, client);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.Ollama.OllamaModelProvider");
                    if (type != null)
                    {
                        return (IModelProvider)Activator.CreateInstance(type, client);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("Ollama model provider not found. Ensure AgentSharp.Providers.Ollama.dll is available.");
        }

        private static IStorageProvider CreateSqliteStorageProvider(string connectionString)
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Storage.SQLite.dll");
                var type = assembly.GetType("AgentSharp.Storage.SQLite.SqliteStorageProvider");
                if (type != null)
                {
                    return (IStorageProvider)Activator.CreateInstance(type, connectionString);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.Storage.SqliteStorageProvider");
                    if (type != null)
                    {
                        return (IStorageProvider)Activator.CreateInstance(type, connectionString);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("SQLite storage provider not found. Ensure AgentSharp.Storage.SQLite.dll is available.");
        }

        private static IStorageProvider CreateMemoryStorageProvider()
        {
            try
            {
                var assembly = Assembly.LoadFrom("AgentSharp.Storage.Memory.dll");
                var type = assembly.GetType("AgentSharp.Storage.Memory.MemoryStorageProvider");
                if (type != null)
                {
                    return (IStorageProvider)Activator.CreateInstance(type);
                }
            }
            catch
            {
                // Fallback: try to load from current assembly (for backward compatibility)
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var type = currentAssembly.GetType("AgentSharp.Providers.Storage.MemoryStorageProvider");
                    if (type != null)
                    {
                        return (IStorageProvider)Activator.CreateInstance(type);
                    }
                }
                catch { }
            }
            
            throw new InvalidOperationException("Memory storage provider not found. Ensure AgentSharp.Storage.Memory.dll is available.");
        }

        #endregion
    }

    /// <summary>
    /// Options for configuring AgentSharp services
    /// </summary>
    public class AgentSharpOptions
    {
        /// <summary>
        /// OpenAI provider options
        /// </summary>
        public OpenAIOptions OpenAI { get; set; }

        /// <summary>
        /// Ollama provider options
        /// </summary>
        public OllamaOptions Ollama { get; set; }

        /// <summary>
        /// Storage options
        /// </summary>
        public StorageOptions Storage { get; set; }

        /// <summary>
        /// Telemetry options
        /// </summary>
        public TelemetryOptions Telemetry { get; set; }
    }

    /// <summary>
    /// OpenAI provider configuration options
    /// </summary>
    public class OpenAIOptions
    {
        /// <summary>
        /// OpenAI API key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Custom endpoint (for Azure OpenAI)
        /// </summary>
        public string Endpoint { get; set; }
    }

    /// <summary>
    /// Ollama provider configuration options
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Ollama server base URL
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:11434";
    }

    /// <summary>
    /// Storage configuration options
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Whether to use SQLite storage
        /// </summary>
        public bool UseSqlite { get; set; }

        /// <summary>
        /// SQLite connection string
        /// </summary>
        public string SqliteConnectionString { get; set; } = ":memory:";

        /// <summary>
        /// Whether to use in-memory storage
        /// </summary>
        public bool UseMemory { get; set; } = true;
    }

    /// <summary>
    /// Telemetry configuration options
    /// </summary>
    public class TelemetryOptions
    {
        /// <summary>
        /// Whether telemetry is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Telemetry provider type
        /// </summary>
        public string Provider { get; set; } = "console";
    }
}