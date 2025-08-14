using System;
using System.Collections.Generic;
using System.Linq;
using AgentSharp.Core.Abstractions;

namespace AgentSharp.Models
{
  /// <summary>
  /// Fábrica PURA para criação de modelos usando APENAS Dependency Injection
  /// NÃO mantém backward compatibility - FORÇA o uso de DI para arquitetura limpa
  /// </summary>
  public class ModelFactory : IModelFactory
  {
    private readonly IEnumerable<IModelProvider> _providers;

    /// <summary>
    /// Construtor com Dependency Injection (ÚNICO construtor disponível)
    /// </summary>
    /// <param name="providers">Provedores de modelo injetados via DI (OBRIGATÓRIO)</param>
    /// <exception cref="ArgumentException">Se providers for null ou vazio</exception>
    public ModelFactory(IEnumerable<IModelProvider> providers)
    {
      if (providers == null || !providers.Any())
        throw new ArgumentException("ModelFactory REQUER providers configurados via DI. Configure pelo menos um IModelProvider.", nameof(providers));
        
      _providers = providers;
    }

    /// <summary>
    /// Cria uma instância de modelo baseado no tipo usando APENAS DI providers
    /// FALHA EXPLICITAMENTE se provider não configurado - força arquitetura limpa
    /// </summary>
    /// <param name="modelType">Tipo do modelo (deve corresponder a um provider configurado)</param>
    /// <param name="options">Opções de configuração do modelo</param>
    /// <returns>Instância do modelo</returns>
    /// <exception cref="ArgumentException">Se provider não encontrado</exception>
    public IModel CreateModel(string modelType, ModelOptions options)
    {
      if (string.IsNullOrWhiteSpace(modelType))
        throw new ArgumentNullException(nameof(modelType));

      if (options == null)
        throw new ArgumentNullException(nameof(options));

      // Valida opções
      options.Validate();

      // ARQUITETURA LIMPA: APENAS DI, sem fallback
      var provider = _providers.FirstOrDefault(p => 
        p.ProviderName.Equals(modelType, StringComparison.OrdinalIgnoreCase));
        
      if (provider == null)
      {
        var availableProviders = string.Join(", ", _providers.Select(p => p.ProviderName));
        throw new ArgumentException(
          $"Provider '{modelType}' não configurado. Providers disponíveis: [{availableProviders}]. " +
          "Configure o provider via DI antes de usar.", nameof(modelType));
      }

      var config = ConvertToModelConfiguration(options);
      return provider.CreateModel(options.ModelName, config);
    }

    /// <summary>
    /// Converte ModelOptions para ModelConfiguration (para DI providers)
    /// </summary>
    private static ModelConfiguration ConvertToModelConfiguration(ModelOptions options)
    {
      return options.DefaultConfiguration ?? new ModelConfiguration
      {
        Temperature = 0.7,
        MaxTokens = 2048
      };
    }

    // *** TODO O CÓDIGO LEGACY FOI REMOVIDO ***
    // ModelFactory agora funciona APENAS com DI - arquitetura limpa!

    // *** MÉTODOS ESTÁTICOS LEGACY REMOVIDOS ***
    // Use DI para configurar providers explicitamente!
    // 
    // EXEMPLO DE USO CORRETO:
    // var providers = new List<IModelProvider> { new OpenAIModelProvider(apiKey) };
    // var factory = new ModelFactory(providers);
    // var model = factory.CreateModel("openai", options);

    // *** MÉTODOS DE REFLEXÃO REMOVIDOS ***
    // Não devemos usar reflexão - isso é anti-pattern!
    // 
    // ARQUITETURA CORRETA:
    // 1. Aplicação referencia AgentSharp.Providers.OpenAI
    // 2. Configura providers explicitamente via DI
    // 3. Injeta na ModelFactory via construtor
    //
    // EXEMPLO:
    // var provider = new OpenAIModelProvider(apiKey, endpoint);
    // var factory = new ModelFactory(new[] { provider });
    // var model = factory.CreateModel("openai", options);
  }
}
