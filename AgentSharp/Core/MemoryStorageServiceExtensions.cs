using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace AgentSharp.Core
{
    /// <summary>
    /// Extensões para configuração de serviços de memória e storage
    /// </summary>
    public static class MemoryStorageServiceExtensions
    {
        /// <summary>
        /// Adiciona os serviços de memória e storage ao container de DI
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configuration">Configuração do sistema</param>
        /// <returns>Collection de serviços configurada</returns>
        public static IServiceCollection AddMemoryStorage(
            this IServiceCollection services,
            MemoryStorageConfiguration configuration = null)
        {
            // Usar configuração padrão se não fornecida
            if (configuration == null)
            {
                configuration = new MemoryStorageConfiguration();
            }

            // Registrar configuração como singleton
            services.AddSingleton(configuration);

            // Registrar serviços base
            // services.AddScoped<IMemoryService, MemoryService>();
            // services.AddScoped<IMemoryManager, MemoryManager>();
            // services.AddScoped<IMemoryClassifier, MemoryClassifier>();
            // services.AddScoped<IStorageService, StorageService>();
            // services.AddScoped<IStorageFactory, StorageFactory>();

            // Registrar auditoria se habilitada
            if (configuration.Audit.Enabled)
            {
                // services.AddScoped<IAuditService, AuditService>();
            }

            return services;
        }

        /// <summary>
        /// Adiciona os serviços de memória e storage usando configuração do IConfiguration
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configurationSection">Seção de configuração</param>
        /// <returns>Collection de serviços configurada</returns>
        public static IServiceCollection AddMemoryStorage(
            this IServiceCollection services,
            IConfigurationSection configurationSection)
        {
            var config = new MemoryStorageConfiguration();

            // Bind manual para compatibilidade com netstandard2.0
            if (configurationSection != null)
            {
                // Implementar bind manual se necessário
                // Por enquanto, usar configuração padrão
            }

            return services.AddMemoryStorage(config);
        }

        /// <summary>
        /// Adiciona os serviços de memória e storage usando uma ação de configuração
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configureOptions">Ação para configurar as opções</param>
        /// <returns>Collection de serviços configurada</returns>
        public static IServiceCollection AddMemoryStorage(
            this IServiceCollection services,
            Action<MemoryStorageConfiguration> configureOptions)
        {
            var config = new MemoryStorageConfiguration();
            if (configureOptions != null)
            {
                configureOptions(config);
            }

            return services.AddMemoryStorage(config);
        }
    }
}
