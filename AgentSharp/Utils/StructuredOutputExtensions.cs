using AgentSharp.Models;
using System;

namespace AgentSharp.Utils
{
  /// <summary>
  /// Métodos de extensão para facilitar o uso de Structured Outputs
  /// </summary>
  public static class StructuredOutputExtensions
  {
    /// <summary>
    /// Configura a configuração do modelo para usar structured output com um tipo específico
    /// </summary>
    /// <typeparam name="T">Tipo para structured output</typeparam>
    /// <param name="config">Configuração do modelo</param>
    /// <returns>A mesma configuração para encadeamento fluente</returns>
    public static ModelConfiguration WithStructuredOutput<T>(this ModelConfiguration config)
    {
      config.EnableStructuredOutput = true;
      config.ResponseType = typeof(T);
      config.ResponseSchema = JsonSchemaGenerator.GenerateSchema<T>();
      return config;
    }

    /// <summary>
    /// Configura a configuração do modelo para usar structured output com um schema manual
    /// </summary>
    /// <param name="config">Configuração do modelo</param>
    /// <param name="schema">Schema JSON manual</param>
    /// <returns>A mesma configuração para encadeamento fluente</returns>
    public static ModelConfiguration WithStructuredOutput(this ModelConfiguration config, string schema)
    {
      if (string.IsNullOrEmpty(schema))
        throw new ArgumentNullException(nameof(schema));

      config.EnableStructuredOutput = true;
      config.ResponseSchema = schema;
      config.ResponseType = null; // Schema manual
      return config;
    }

    /// <summary>
    /// Configura a configuração do modelo para usar structured output com um tipo específico e temperatura baixa
    /// (ideal para extrações de dados e análises estruturadas)
    /// </summary>
    /// <typeparam name="T">Tipo para structured output</typeparam>
    /// <param name="config">Configuração do modelo</param>
    /// <param name="temperature">Temperatura (padrão: 0.1)</param>
    /// <returns>A mesma configuração para encadeamento fluente</returns>
    public static ModelConfiguration WithStructuredExtraction<T>(this ModelConfiguration config, double temperature = 0.1)
    {
      config.EnableStructuredOutput = true;
      config.ResponseType = typeof(T);
      config.ResponseSchema = JsonSchemaGenerator.GenerateSchema<T>();
      config.Temperature = temperature;
      return config;
    }

    /// <summary>
    /// Configura a configuração do modelo para usar structured output com um tipo específico e temperatura criativa
    /// (ideal para geração criativa estruturada)
    /// </summary>
    /// <typeparam name="T">Tipo para structured output</typeparam>
    /// <param name="config">Configuração do modelo</param>
    /// <param name="temperature">Temperatura (padrão: 0.7)</param>
    /// <returns>A mesma configuração para encadeamento fluente</returns>
    public static ModelConfiguration WithStructuredGeneration<T>(this ModelConfiguration config, double temperature = 0.7)
    {
      config.EnableStructuredOutput = true;
      config.ResponseType = typeof(T);
      config.ResponseSchema = JsonSchemaGenerator.GenerateSchema<T>();
      config.Temperature = temperature;
      return config;
    }

    /// <summary>
    /// Verifica se a resposta contém dados estruturados e os obtém de forma segura
    /// </summary>
    /// <typeparam name="T">Tipo esperado dos dados estruturados</typeparam>
    /// <param name="response">Resposta do modelo</param>
    /// <param name="data">Dados estruturados se disponíveis</param>
    /// <returns>True se os dados estruturados estão disponíveis e são do tipo esperado</returns>
    public static bool TryGetStructuredData<T>(this ModelResponse response, out T data)
    {
      return response.TryGetStructuredData(out data);
    }

    /// <summary>
    /// Obtém dados estruturados ou retorna um valor padrão
    /// </summary>
    /// <typeparam name="T">Tipo esperado dos dados estruturados</typeparam>
    /// <param name="response">Resposta do modelo</param>
    /// <param name="defaultValue">Valor padrão se os dados não estiverem disponíveis</param>
    /// <returns>Dados estruturados ou valor padrão</returns>
    public static T GetStructuredDataOrDefault<T>(this ModelResponse response, T defaultValue = default)
    {
      if (response.TryGetStructuredData(out T data))
      {
        return data;
      }
      return defaultValue;
    }

    /// <summary>
    /// Executa uma ação apenas se os dados estruturados estiverem disponíveis
    /// </summary>
    /// <typeparam name="T">Tipo esperado dos dados estruturados</typeparam>
    /// <param name="response">Resposta do modelo</param>
    /// <param name="action">Ação a ser executada com os dados estruturados</param>
    public static void IfStructuredData<T>(this ModelResponse response, Action<T> action)
    {
      if (response.TryGetStructuredData(out T data))
      {
        action?.Invoke(data);
      }
    }

    /// <summary>
    /// Transforma dados estruturados em outro tipo usando uma função
    /// </summary>
    /// <typeparam name="T">Tipo dos dados estruturados</typeparam>
    /// <typeparam name="TResult">Tipo do resultado</typeparam>
    /// <param name="response">Resposta do modelo</param>
    /// <param name="transform">Função de transformação</param>
    /// <param name="defaultValue">Valor padrão se os dados não estiverem disponíveis</param>
    /// <returns>Resultado da transformação ou valor padrão</returns>
    public static TResult MapStructuredData<T, TResult>(this ModelResponse response, Func<T, TResult> transform, TResult defaultValue = default)
    {
      if (response.TryGetStructuredData(out T data))
      {
        return transform(data);
      }
      return defaultValue;
    }
  }
}
