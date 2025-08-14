using System;
using System.Collections.Generic;
using System.Linq;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;

namespace AgentSharp.Core.Storage
{
    /// <summary>
    /// Fábrica PURA para criação de storage usando APENAS Dependency Injection
    /// NÃO mantém backward compatibility - FORÇA o uso de DI para arquitetura limpa
    /// </summary>
    public class StorageFactory : IStorageFactory
    {
        private readonly IEnumerable<IStorageProvider> _providers;

        /// <summary>
        /// Construtor com Dependency Injection (ÚNICO construtor disponível)
        /// </summary>
        /// <param name="providers">Provedores de storage injetados via DI (OBRIGATÓRIO)</param>
        /// <exception cref="ArgumentException">Se providers for null ou vazio</exception>
        public StorageFactory(IEnumerable<IStorageProvider> providers)
        {
            if (providers == null || !providers.Any())
                throw new ArgumentException("StorageFactory REQUER providers configurados via DI. Configure pelo menos um IStorageProvider.", nameof(providers));
                
            _providers = providers;
        }

        /// <summary>
        /// Cria uma instância de storage baseado no tipo usando APENAS DI providers
        /// FALHA EXPLICITAMENTE se provider não configurado - força arquitetura limpa
        /// </summary>
        /// <param name="storageType">Tipo do storage (deve corresponder a um provider configurado)</param>
        /// <param name="options">Opções de configuração do storage</param>
        /// <returns>Instância do storage</returns>
        /// <exception cref="ArgumentException">Se provider não encontrado</exception>
        public IStorage CreateStorage(string storageType, StorageOptions options)
        {
            if (string.IsNullOrWhiteSpace(storageType))
                throw new ArgumentNullException(nameof(storageType));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Valida opções
            options.Validate();

            // ARQUITETURA LIMPA: APENAS DI, sem fallback
            var provider = _providers.FirstOrDefault(p => 
                p.ProviderName.Equals(storageType, StringComparison.OrdinalIgnoreCase));
                
            if (provider == null)
            {
                var availableProviders = string.Join(", ", _providers.Select(p => p.ProviderName));
                throw new ArgumentException(
                    $"Provider '{storageType}' não configurado. Providers disponíveis: [{availableProviders}]. " +
                    "Configure o provider via DI antes de usar.", nameof(storageType));
            }

            // Configura opções adicionais para o provider
            var config = PrepareDIConfiguration(options);
            
            // Como o provider retorna object, fazemos cast para IStorage
            var storage = provider.CreateMemoryStorage(options.ConnectionString, config);
            if (!(storage is IStorage storageInterface))
            {
                throw new InvalidOperationException(
                    $"Provider '{provider.ProviderName}' retornou um objeto que não implementa IStorage. " +
                    "Verifique a implementação do provider.");
            }

            return storageInterface;
        }

        // *** TODO O CÓDIGO LEGACY FOI REMOVIDO ***
        // StorageFactory agora funciona APENAS com DI - arquitetura limpa!

        /// <summary>
        /// Prepara configuração para providers DI
        /// </summary>
        private static StorageConfiguration PrepareDIConfiguration(StorageOptions options)
        {
            var config = options.Configuration ?? new StorageConfiguration();
            
            // Transfere informações do StorageOptions para StorageConfiguration.CustomOptions
            if (options.EmbeddingService != null)
            {
                config.CustomOptions = config.CustomOptions ?? new System.Collections.Generic.Dictionary<string, object>();
                config.CustomOptions["EmbeddingService"] = options.EmbeddingService;
                config.CustomOptions["Dimensions"] = options.Dimensions;
            }
            
            return config;
        }

        // *** MÉTODOS ESTÁTICOS LEGACY REMOVIDOS ***
        // Use DI para configurar providers explicitamente!
        // 
        // EXEMPLO DE USO CORRETO:
        // var providers = new List<IStorageProvider> { new SqliteStorageProvider() };
        // var factory = new StorageFactory(providers);
        // var storage = factory.CreateStorage("sqlite", options);
    }
}