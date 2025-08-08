using AgentSharp.Core.Memory;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Core
{
  /// <summary>
  /// Motor de execução que gerencia a comunicação com modelos,
  /// execução de ferramentas e fluxo de processamento.
  /// </summary>
  public class ExecutionEngine<TContext, TResult>
  {
    private readonly IModel _model;
    private readonly ILogger _logger;
    private readonly IMemory _memory;
    private readonly ModelConfiguration _defaultConfig;

    public ExecutionEngine(
        IModel model,
        ModelConfiguration config = null,
        ILogger logger = null,
        IMemory memory = null)
    {
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _defaultConfig = config ?? new ModelConfiguration();
      _logger = logger ?? new ConsoleLogger();
      _memory = memory ?? new InMemoryStore();
    }

    /// <summary>
    /// Executa uma chamada ao modelo com possibilidade de execução de ferramentas
    /// </summary>
    public async Task<ExecutionResult<TResult>> ExecuteWithToolsAsync(
        ModelRequest request,
        ModelConfiguration config = null,
        CancellationToken cancellationToken = default)
    {
      // Obter ou criar contexto de execução
      var executionContext = ExecutionContext.Current ?? ExecutionContext.CreateNew();

      // Verificar limite de chamadas para evitar loops infinitos
      executionContext.IncrementCallCount();
      var finalConfig = config ?? _defaultConfig;
      var toolResults = new List<ToolResult>();
      var stopwatch = new Stopwatch();

      try
      {
        // Verificações
        if (request == null || request.Messages == null || request.Messages.Count == 0)
          throw new ArgumentException("Requisição inválida ou sem mensagens");

        // Salvar histórico na memória
        await SaveMessageHistoryAsync(request.Messages);

        // Executar modelo
        stopwatch.Start();
        _logger.Log(LogLevel.Debug, $"Enviando requisição para o modelo {_model.ModelName}");
        var response = await _model.GenerateResponseAsync(request, finalConfig, cancellationToken);
        stopwatch.Stop();

        // Registrar uso e tempo de resposta
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        _logger.Log(LogLevel.Debug, $"Resposta do modelo recebida em {elapsedMs}ms. Tokens: {response?.Usage?.TotalTokens ?? 0}");


        // Verificar se há resultados de ferramentas (o OpenAIModel já as executou)
        if (response?.Tools != null && response.Tools.Count > 0)
        {
          // As ferramentas já foram executadas pelo modelo, apenas registrar os resultados
          toolResults = response.Tools;

          // Registrar resultados na memória para referência futura
          foreach (var toolResult in toolResults)
          {
            await _memory.AddItemAsync(new MemoryItem(
                toolResult.Result,
                "tool_result",
                0.8,
                new Dictionary<string, object> { ["tool"] = toolResult.Name }
            ));
          }

          _logger.Log(LogLevel.Debug, $"Processados {toolResults.Count} resultados de ferramentas pelo modelo");
        }

        // Sem chamadas de ferramentas, processar resultado final
        var result = ProcessResponse<TResult>(response);

        // Registrar resposta na memória
        if (!string.IsNullOrEmpty(response?.Content))
        {
          await _memory.AddItemAsync(new MemoryItem(
              response.Content,
              "response",
              1.0,
              new Dictionary<string, object> { ["tokens"] = response?.Usage?.TotalTokens ?? 0 }
          ));
        }

        return new ExecutionResult<TResult>
        {
          Data = result,
          RawResponse = response,
          ToolResults = toolResults,
          ElapsedMilliseconds = elapsedMs
        };
      }
      catch (Exception ex)
      {
        _logger.Log(LogLevel.Error, $"Erro durante execução: {ex.Message}", ex);

        throw;
      }
      finally
      {
        executionContext.CallCount--;

        if (!stopwatch.IsRunning)
          stopwatch.Stop();
      }
    }

    /// <summary>
    /// Executa chamada ao modelo em modo streaming
    /// </summary>
    public async Task<ExecutionResult<TResult>> ExecuteStreamingAsync(
        ModelRequest request,
        Action<string> streamHandler,
        ModelConfiguration config = null,
        CancellationToken cancellationToken = default)
    {
      // Obter ou criar contexto de execução
      var executionContext = ExecutionContext.Current ?? ExecutionContext.CreateNew();

      // Verificar limite de chamadas para evitar loops infinitos
      executionContext.IncrementCallCount();
      var finalConfig = config ?? _defaultConfig;
      var stopwatch = new Stopwatch();

      try
      {
        // Verificações
        if (request == null || request.Messages == null || request.Messages.Count == 0)
          throw new ArgumentException("Requisição inválida ou sem mensagens");

        if (streamHandler == null)
          throw new ArgumentNullException(nameof(streamHandler));

        // Salvar histórico na memória
        await SaveMessageHistoryAsync(request.Messages);

        // Executar modelo em streaming
        stopwatch.Start();
        _logger.Log(LogLevel.Debug, $"Enviando requisição de streaming para o modelo {_model.ModelName}");

        var response = await _model.GenerateStreamingResponseAsync(
            request, finalConfig, streamHandler, cancellationToken);

        stopwatch.Stop();

        // Registrar uso e tempo de resposta
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        _logger.Log(LogLevel.Debug, $"Streaming concluído em {elapsedMs}ms. Tokens estimados: {response?.Usage?.TotalTokens ?? 0}");

        // Processar resultado
        var result = ProcessResponse<TResult>(response);

        // Registrar resposta na memória
        if (!string.IsNullOrEmpty(response?.Content))
        {
          await _memory.AddItemAsync(new MemoryItem(
              response.Content,
              "streaming_response",
              1.0,
              new Dictionary<string, object> { ["tokens"] = response?.Usage?.TotalTokens ?? 0 }
          ));
        }

        return new ExecutionResult<TResult>
        {
          Data = result,
          RawResponse = response,
          ToolResults = response?.Tools ?? new List<ToolResult>(),
          ElapsedMilliseconds = elapsedMs
        };
      }
      catch (Exception ex)
      {
        _logger.Log(LogLevel.Error, $"Erro durante streaming: {ex.Message}", ex);

        throw;
      }
      finally
      {
        executionContext.CallCount--;

        if (!stopwatch.IsRunning)
          stopwatch.Stop();
      }
    }

    /// <summary>
    /// Processa chamadas de ferramentas a partir da resposta do modelo
    /// </summary>
    private async Task<List<AIMessage>> ProcessToolCallsAsync(
        ModelRequest request,
        List<ToolResult> toolResults,
        CancellationToken cancellationToken)
    {
      var toolMessages = new List<AIMessage>();

      // Dicionário para mapeamento rápido de nomes de ferramentas para objetos Tool
      var toolsMap = request.Tools.ToDictionary(t => t.Name, t => t);

      foreach (var result in toolResults)
      {
        _logger.Log(LogLevel.Debug, $"Processando chamada de ferramenta: {result.Name}");

        // Verificar se a ferramenta existe
        if (!toolsMap.TryGetValue(result.Name, out var tool))
        {
          _logger.Log(LogLevel.Warning, $"Ferramenta não encontrada: {result.Name}");
          continue;
        }

        try
        {
          // Preparar argumentos em formato JSON
          string argsJson = JsonSerializer.Serialize(result.Arguments);

          // Executar a ferramenta
          string toolResult;
          if (tool.IsAsync)
          {
            toolResult = await tool.ExecuteAsync(argsJson, cancellationToken);
          }
          else
          {
            var syncResult = tool.Execute(argsJson);
            toolResult = syncResult?.ToString() ?? string.Empty;
          }

          // Registrar resultado na memória
          await _memory.AddItemAsync(new MemoryItem(
              toolResult,
              "tool_result",
              0.8,
              new Dictionary<string, object> { ["tool"] = result.Name }
          ));

          // Criar mensagem para o modelo usando o ID correto da chamada da ferramenta
          var toolCallId = !string.IsNullOrEmpty(result.ToolCallId)
              ? result.ToolCallId
              : "t" + Guid.NewGuid().ToString("N").Substring(0, 8);
          toolMessages.Add(new ToolResultMessage(
              toolCallId,  // ID da chamada da ferramenta fornecido pelo modelo
              result.Name,
              toolResult
          ));

          _logger.Log(LogLevel.Debug, $"Ferramenta {result.Name} executada com sucesso");
        }
        catch (Exception ex)
        {
          _logger.Log(LogLevel.Error, $"Erro ao executar ferramenta {result.Name}: {ex.Message}", ex);

          // Adicionar mensagem de erro como resultado da ferramenta
          var errorToolCallId = !string.IsNullOrEmpty(result.ToolCallId)
              ? result.ToolCallId
              : "e" + Guid.NewGuid().ToString("N").Substring(0, 8);
          toolMessages.Add(new ToolResultMessage(
              errorToolCallId,
              result.Name,
              $"Erro ao executar ferramenta: {ex.Message}"
          ));
        }
      }

      return toolMessages;
    }

    /// <summary>
    /// Processa a resposta do modelo para o tipo de resultado esperado
    /// </summary>
    public T ProcessResponse<T>(ModelResponse response)
    {
      if (response == null)
        return default;

      // Para tipo string, sempre retornar o Content
      if (typeof(T) == typeof(string))
        return (T)(object)(response.Content ?? string.Empty);

      // Se a resposta é structured output, usar os dados já processados
      if (response.IsStructuredResponse && response.TryGetStructuredData(out T structuredData))
      {
        _logger.Log(LogLevel.Debug, $"Usando dados structured output já processados para o tipo {typeof(T).Name}");
        return structuredData;
      }

      // Fallback: tentar deserializar o JSON manualmente
      if (string.IsNullOrEmpty(response.Content))
        return default;

      try
      {
        var result = JsonSerializer.Deserialize<T>(response.Content);
        _logger.Log(LogLevel.Debug, $"Deserialização manual do JSON realizada com sucesso para o tipo {typeof(T).Name}");
        return result;
      }
      catch (JsonException ex)
      {
        _logger.Log(LogLevel.Warning, $"Não foi possível deserializar a resposta para o tipo {typeof(T).Name}. Content: {response.Content?.Substring(0, Math.Min(100, response.Content?.Length ?? 0))}... Erro: {ex.Message}");
        return default;
      }
    }

    /// <summary>
    /// Salva o histórico de mensagens na memória
    /// </summary>
    private async Task SaveMessageHistoryAsync(List<AIMessage> messages)
    {
      if (messages == null || messages.Count == 0)
        return;

      foreach (var msg in messages)
      {
        // Evitar duplicatas verificando se já existe na memória
        var existingItems = await _memory.GetItemsAsync(msg.Content, 1);
        if (existingItems.Any())
          continue;

        double relevance = msg.Role == Role.System ? 0.9 : 0.7;
        string type = "message_" + msg.Role.ToString().ToLowerInvariant();

        await _memory.AddItemAsync(new MemoryItem(
            msg.Content,
            type,
            relevance
        ));
      }
    }
  }

  /// <summary>
  /// Representa o resultado de uma execução do motor
  /// </summary>
  public class ExecutionResult<T>
  {
    /// <summary>
    /// Dados processados no tipo esperado
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Resposta bruta do modelo
    /// </summary>
    public ModelResponse RawResponse { get; set; }

    /// <summary>
    /// Resultados das ferramentas executadas
    /// </summary>
    public List<ToolResult> ToolResults { get; set; } = new List<ToolResult>();

    /// <summary>
    /// Tempo de execução em milissegundos
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
  }
}

/*
// Inicializar componentes
var model = new OpenAIModel("gpt-4o", apiKey);
var memory = new InMemoryStore();
var logger = new ConsoleLogger();

// Criar o motor de execução
var engine = new ExecutionEngine<MyContext, string>(
    model,
    new ModelConfiguration { Temperature = 0.7 },
    logger,
    memory
);

// Preparar requisição
var request = new ModelRequest
{
    Messages = new List<AIMessage>
    {
        AIMessage.System("Você é um assistente útil"),
        AIMessage.User("Como está o tempo hoje?")
    },
    Tools = new List<Tool> { new WeatherTool() }
};


// Executar com ferramentas
var result = await engine.ExecuteWithToolsAsync(
    request
);

// Usar o resultado
Console.WriteLine($"Resposta: {result.Data}");
Console.WriteLine($"Ferramentas usadas: {result.ToolResults.Count}");
Console.WriteLine($"Tempo de execução: {result.ElapsedMilliseconds}ms");
*/
