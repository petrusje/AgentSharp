using System;

namespace AgentSharp.Models
{
  /// <summary>
  /// Opções de configuração para criar instâncias de modelos
  /// </summary>
  public class ModelOptions
  {
    /// <summary>
    /// Nome do modelo a ser utilizado (ex: "gpt-4o", "gpt-3.5-turbo")
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Chave de API para autenticação com o provedor do modelo
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Endpoint opcional para o serviço do modelo (ex: URL para Azure OpenAI)
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Configuração padrão do modelo
    /// </summary>
    public ModelConfiguration DefaultConfiguration { get; set; }

    /// <summary>
    /// Timeout para requisições em segundos (opcional)
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Número máximo de tentativas para requisições com falha (opcional)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Organização para serviços que suportam múltiplas organizações (ex: OpenAI)
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// Indica se deve utilizar streaming quando disponível
    /// </summary>
    public bool UseStreaming { get; set; } = true;

    /// <summary>
    /// Cria uma nova instância de ModelOptions com valores padrão
    /// </summary>
    public ModelOptions()
    {
      DefaultConfiguration = new ModelConfiguration
      {
        Temperature = 0.7,
        MaxTokens = 2048
      };
    }

    /// <summary>
    /// Valida as opções fornecidas
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrWhiteSpace(ModelName))
      {
        throw new ArgumentException("Nome do modelo não pode ser vazio", nameof(ModelName));
      }

      if (string.IsNullOrWhiteSpace(ApiKey))
      {
        // Tenta carregar do ambiente se não fornecido explicitamente
        var envApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                       Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(envApiKey))
        {
          throw new ArgumentException("ApiKey não fornecida e não encontrada nas variáveis de ambiente", nameof(ApiKey));
        }

        ApiKey = envApiKey;
      }

      if (DefaultConfiguration == null)
      {
        DefaultConfiguration = new ModelConfiguration();
      }
    }
  }
}
