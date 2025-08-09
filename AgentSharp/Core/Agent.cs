using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Tools;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json; // Added for JsonSerializer
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Core
{

  /// <summary>
  /// Classe base para agentes de IA com componentes especializados
  /// </summary>
  public class Agent<TContext, TResult> : IAgentCtxChannel, IAgent
  {
    public string Name { get; }
    public string description => _promptManager.BuildSystemPrompt(Context); // Para compatibilidade

    private readonly PromptManager<TContext> _promptManager;
    private readonly ToolManager _toolManager;
    private readonly ExecutionEngine<TContext, TResult> _executionEngine;
    private readonly IModel _model;
    private readonly ILogger _logger;
   // Configuração do modelo (parâmetros, temperatura, etc.)
    private ModelConfiguration _modelConfig;

    /// Configurações específicas de memória
    /// Como Quantidade máxima de memórias (sessão e usuário), max execuções, relevância mínima, auto summary, numero minimo para cria resumo.
    private MemoryConfiguration _memoryConfig;

    // Configuração de memória (prompts customizados para salvar e recuperar, etc.)
    private MemoryDomainConfiguration _memoryDomainConfig;

    // Gestão avançada de memória
    private IMemoryManager _memoryManager;

    // Serviço de armazenamento (pode ser InMemory, Sqlite, etc.)
    private readonly IStorage _storage;

    // Estado do agente
    private AgentContext<TContext> _agentContext;

    // Configurações de reasoning
    private bool _reasoning = false;
    private IModel _reasoningModel;
    private int _reasoningMinSteps = 1;
    private int _reasoningMaxSteps = 10;

    // Resumo inteligente gerado pela LLM (delegado ao MemoryManager)
    // private string _memorySummary; // Removido, agora gerenciado pelo MemoryManager

    // Controle de modo de memória
    public enum MemoryMode
    {
        SummaryOnly,
        FullHistory,
        SummaryAndRecent
    }

    private MemoryMode _memoryMode = MemoryMode.SummaryAndRecent;
    private int _recentMessagesCount = 5;

    // Configuração de modo anônimo
    private bool _enableAnonymousMode = false;
    private string _generatedUserId = null;
    private string _generatedSessionId = null;

    // Formato de contexto
    public enum ContextFormat
    {
        SystemMessage,
        FunctionCall,
        RequestField
    }
    private ContextFormat _contextFormat = ContextFormat.SystemMessage;
    private string _memorySummary = "";

    // Construtor principal
    public Agent(
        IModel model,
        string name = null,
        string instructions = null,
        ModelConfiguration modelConfig = null,
        ILogger logger = null,
        IMemoryManager memoryManager = null,
        IStorage storage = null)
    {
      Name = name ?? GetType().Name;
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _logger = logger ?? new ConsoleLogger();
      _modelConfig = modelConfig ?? new ModelConfiguration();

      // 🎯 AUTO-CONFIGURAÇÃO INTELIGENTE DE STRUCTURED OUTPUT (APENAS SE NÃO JÁ CONFIGURADO)
      // Se TResult não é um tipo primitivo/basic E structured output ainda não foi configurado
      if (typeof(TResult) != typeof(object) &&
          typeof(TResult) != typeof(string) &&
          !typeof(TResult).IsPrimitive &&
          typeof(TResult) != typeof(AgentResult<TResult>) &&
          !_modelConfig.EnableStructuredOutput) // ← CHAVE: só auto-configura se não já configurado
      {
          // Preservar todas as configurações existentes, apenas adicionar structured output
          _modelConfig.EnableStructuredOutput = true;
          _modelConfig.ResponseType = typeof(TResult);

          // Gerar schema apenas se não foi fornecido
          if (string.IsNullOrEmpty(_modelConfig.ResponseSchema))
          {
              _modelConfig.ResponseSchema = Utils.JsonSchemaGenerator.GenerateSchema<TResult>();
          }

          // Ajustar temperatura para mais determinística em structured outputs (se não customizada)
          if (_modelConfig.Temperature >= 0.7) // Se é o valor padrão
          {
              _modelConfig.Temperature = 0.1; // Mais determinística para dados estruturados
          }

          _logger.Log(LogLevel.Debug, $"Auto-configured structured extraction for type: {typeof(TResult).Name} (Temperature: {_modelConfig.Temperature})");
      }

      // Configurar sistema de memória
      _storage = storage ?? new InMemoryStorage();

      // Se o storage for VectorSqliteStorage, usar o embedding service dele
      IEmbeddingService embeddingService = new MockEmbeddingService();
      if (storage is VectorSqliteStorage vectorStorage)
      {
          // TODO: Extrair o embedding service do VectorSqliteStorage se necessário
          embeddingService = new MockEmbeddingService();
      }

      _memoryManager = memoryManager ?? new MemoryManager(
          _storage,
          _model,
          _logger,
          embeddingService,
          _memoryDomainConfig);

      _promptManager = new PromptManager<TContext>();
      _toolManager = new ToolManager();
      _executionEngine = new ExecutionEngine<TContext, TResult>(_model, _modelConfig, _logger, new InMemoryStore());

      // Adicionar instruções iniciais se fornecidas
      if (!string.IsNullOrEmpty(instructions))
      {
        _promptManager.AddInstructions(_ => instructions);
      }
      registerTools();
    }

    private void registerTools()
    {
      _toolManager.RegisterAgentMethods(this);

      // Registrar SmartMemoryToolPack por padrão
      _toolManager.RegisterToolPack(new SmartMemoryToolPack());

      // Disponibilizar MemoryManager para tools
      if (_memoryManager != null)
      {
          // TODO: Adicionar mecanismo para passar MemoryManager para tools
      }
    }

    #region Métodos de Configuração Fluente

    public Agent<TContext, TResult> WithPersona(string persona)
    {
      _promptManager.AddPersona(_ => persona);
      return this;
    }

    public Agent<TContext, TResult> WithPersona(Func<TContext, string> personaGenerator)
    {
      _promptManager.AddPersona(personaGenerator);
      return this;
    }

    public Agent<TContext, TResult> WithInstructions(string instructions)
    {
      _promptManager.AddInstructions(_ => instructions);
      return this;
    }

    public Agent<TContext, TResult> WithInstructions(Func<TContext, string> instructionsGenerator)
    {
      _promptManager.AddInstructions(instructionsGenerator);
      return this;
    }

    public Agent<TContext, TResult> WithGuardRails(string guardRails)
    {
      _promptManager.AddGuardRails(_ => guardRails);
      return this;
    }

    public Agent<TContext, TResult> WithGuardRails(Func<TContext, string> guardRailsGenerator)
    {
      _promptManager.AddGuardRails(guardRailsGenerator);
      return this;
    }

      public Agent<TContext, TResult> WithMemoryConfiguration(MemoryConfiguration config)
      {
          _memoryConfig = config;

          // Recriar MemoryManager com nova configuração
          _memoryManager = new MemoryManager(
              _storage,
              _model,
              _logger,
              new MockEmbeddingService(),
              _memoryDomainConfig); // <- Passar configuração

          return this;
      }
    public Agent<TContext, TResult> AddSystemPrompt(string prompt)
    {
      _promptManager.AddSystemPrompt(_ => prompt);
      return this;
    }

    public Agent<TContext, TResult> AddSystemPrompt(Func<TContext, string> promptGenerator)
    {
      _promptManager.AddSystemPrompt(promptGenerator);
      return this;
    }

    public Agent<TContext, TResult> RegisterSystemPromptFunction(Func<TContext, string> generator)
    {
      return AddSystemPrompt(generator); // Compatibilidade
    }

    public Agent<TContext, TResult> AddTool(Tool tool)
    {
      _toolManager.RegisterTool(tool);
      return this;
    }

    public Agent<TContext, TResult> WithTools(ToolPack toolPack)
    {
      _toolManager.RegisterToolPack(toolPack);
      return this;
    }

    public Agent<TContext, TResult> WithToolPacks(params ToolPack[] toolPacks)
    {
      foreach (var pack in toolPacks)
      {
        _toolManager.RegisterToolPack(pack);
      }
      return this;
    }

    public Agent<TContext, TResult> WithContext(TContext context)
    {
      if (_agentContext == null)
      {
        _agentContext = new AgentContext<TContext>(
            context,
            new List<AIMessage>(),
            _modelConfig
        );
      }
      else
      {
        _agentContext.ContextVar = context;
      }
      return this;
    }
    public Agent<TContext, TResult> WithConfig(ModelConfiguration config)
    {
      _modelConfig = config;
      return this;
    }
    public Agent<TContext, TResult> WithMaxExecutions(int maxExecutions)
    {
      // Configuração aplicada ao ExecutionContext quando criado
      return this;
    }

    /// <summary>
    /// Habilita reasoning step-by-step
    /// </summary>
    public Agent<TContext, TResult> WithReasoning(bool reasoning = true)
    {
      _reasoning = reasoning;
      return this;
    }

    /// <summary>
    /// Define modelo específico para reasoning
    /// </summary>
    public Agent<TContext, TResult> WithReasoningModel(IModel reasoningModel)
    {
      _reasoningModel = reasoningModel;
      _reasoning = true; // Habilita reasoning automaticamente
      return this;
    }

    /// <summary>
    /// Configura número mínimo e máximo de passos de reasoning
    /// </summary>
    public Agent<TContext, TResult> WithReasoningSteps(int minSteps = 1, int maxSteps = 10)
    {
      _reasoningMinSteps = minSteps;
      _reasoningMaxSteps = maxSteps;
      return this;
    }

    public Agent<TContext, TResult> WithMemoryManager(IMemoryManager memoryManager)
    {
        // Substituir o memory manager atual
        return this; // TODO: Implementar substituição se necessário
    }

    public Agent<TContext, TResult> WithStorage(IStorage storage)
    {
        // Substituir o storage atual
        return this; // TODO: Implementar substituição se necessário
    }

    public Agent<TContext, TResult> WithMemoryMode(MemoryMode mode, int recentMessagesCount = 5)
    {
        _memoryMode = mode;
        _recentMessagesCount = recentMessagesCount;
        return this;
    }

    public Agent<TContext, TResult> WithContextFormat(ContextFormat format)
    {
        _contextFormat = format;
        return this;
    }

    public Agent<TContext, TResult> WithAnonymousMode(bool enableAnonymousMode = true)
    {
        _enableAnonymousMode = enableAnonymousMode;
        return this;
    }
    #endregion

    #region Acesso a Context

    public TContext Context => _agentContext.ContextVar;

    public void setContext(object context)
    {
      if (context is TContext typedContext)
      {
        if (_agentContext == null)
        {
          _agentContext = new AgentContext<TContext>(typedContext, new List<AIMessage>(), _modelConfig);
        }
        else
        {
          _agentContext.ContextVar = typedContext;
        }
      }
    }

    public object GetProperty(string propertyName)
    {
      if (_agentContext != null && _agentContext.ContextVar != null)
      {
        var contextType = typeof(TContext);
        var property = contextType.GetProperty(propertyName);
        if (property != null)
        {
          return property.GetValue(_agentContext.ContextVar);
        }
      }
      return null;
    }

    #endregion

    #region Execução

    public virtual async Task<AgentResult<TResult>> ExecuteAsync(
        string prompt,
        TContext contextVar = default,
        List<AIMessage> messageHistory = null,
        CancellationToken cancellationToken = default)
    {
      // Verificações de estado
      if (string.IsNullOrEmpty(prompt))
        throw new ArgumentException("O prompt não pode ser vazio", nameof(prompt));

      try
      {
        // Preparar contexto
        PrepareContext(contextVar, messageHistory);

        // Processar reasoning PRIMEIRO se habilitado
        string reasoningContent = null;
        List<ReasoningStep> reasoningSteps = null;
        string finalPrompt = prompt;

        if (_reasoning)
        {
          var reasoningResult = await ProcessReasoningAsync(prompt, _agentContext.MessageHistory, cancellationToken);
          reasoningContent = reasoningResult.Content;
          reasoningSteps = reasoningResult.Steps;

          // Integrar reasoning no prompt final
          if (!string.IsNullOrEmpty(reasoningContent))
          {
            finalPrompt = $@"{prompt}

🧠 ANÁLISE ESTRUTURADA:
{reasoningContent}

Com base nesta análise, forneça sua resposta final:";
          }
        }

        // === DELEGADO: Gerar resumo automático do histórico via MemoryManager ===
        string userId = GetUserIdFromContext();
        string sessionId = GetSessionIdFromContext();
        _memoryManager.UserId = userId;
        _memoryManager.SessionId = sessionId;

        // Carregar contexto de memória
        var memoryContext = await _memoryManager.LoadContextAsync(userId, sessionId);

        // Construir mensagens base
        var systemPrompt = _promptManager.BuildSystemPrompt(Context);
        var baseMessages = new List<AIMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
          baseMessages.Add(AIMessage.System(systemPrompt));
        baseMessages.Add(AIMessage.User(finalPrompt));

        // Enriquecer mensagens com memórias relevantes
        var enhancedMessages = await _memoryManager.EnhanceMessagesAsync(baseMessages, memoryContext);

        // Criar requisição usando ToolManager
        var request = new ModelRequest
        {
            Messages = enhancedMessages,
            Tools = _toolManager.GetTools()
        };

        // Usar ExecutionEngine para executar
        var executionResult = await _executionEngine.ExecuteWithToolsAsync(
            request,
            _modelConfig,
            cancellationToken);

        // Processar interação para extrair memórias automaticamente
        var userMessage = AIMessage.User(finalPrompt);
        var assistantMessage = AIMessage.Assistant(executionResult.RawResponse.Content);
        await _memoryManager.ProcessInteractionAsync(userMessage, assistantMessage, memoryContext);

        // Atualizar histórico local
        if (!string.IsNullOrEmpty(executionResult.RawResponse.Content))
          _agentContext.MessageHistory.Add(assistantMessage);

        // Criar resultado do agente
        var sessionInfo = CreateSessionInfo();
        var agentResult = new AgentResult<TResult>(
            executionResult.Data,
            _agentContext.MessageHistory,
            _agentContext.MessageHistory.Count,
            executionResult.RawResponse.Usage,
            executionResult.ToolResults,
            reasoningContent,
            reasoningSteps,
            sessionInfo
        );

        return agentResult;
      }
      catch (Exception ex)
      {
        _logger.Log(LogLevel.Error, $"Erro durante execução do Agent {Name}: {ex.Message}", ex);
        throw;
      }
    }

    public async Task<AgentResult<TResult>> ExecuteStreamingAsync(
        string prompt,
        Action<string> handler,
        TContext contextVar = default,
        List<AIMessage> messageHistory = null,
        CancellationToken cancellationToken = default)
    {
      // Verificações de estado
      if (string.IsNullOrEmpty(prompt))
        throw new ArgumentException("O prompt não pode ser vazio", nameof(prompt));

      try
      {
        // Preparar contexto
        PrepareContext(contextVar, messageHistory);

        // Criar sistema de mensagens usando PromptManager
        var systemPrompt = _promptManager.BuildSystemPrompt(Context);
        var messages = new List<AIMessage>();

        if (!string.IsNullOrEmpty(systemPrompt))
          messages.Add(AIMessage.System(systemPrompt));

        messages.AddRange(_agentContext.MessageHistory);
        messages.Add(AIMessage.User(prompt));

        // Criar requisição usando ToolManager
        var request = new ModelRequest
        {
          Messages = messages,
          Tools = _toolManager.GetTools()
        };

        // Usar ExecutionEngine para executar em streaming
        var executionResult = await _executionEngine.ExecuteStreamingAsync(
            request,
            handler,
            _modelConfig,
            cancellationToken);

        // Atualizar histórico
        if (!string.IsNullOrEmpty(executionResult.RawResponse.Content))
          _agentContext.MessageHistory.Add(AIMessage.Assistant(executionResult.RawResponse.Content));

        // Criar resultado do agente
        var sessionInfo = CreateSessionInfo();
        var agentResult = new AgentResult<TResult>(
            executionResult.Data,
            _agentContext.MessageHistory,
            _agentContext.MessageHistory.Count,
            executionResult.RawResponse.Usage,
            executionResult.ToolResults,
            null, // reasoningContent
            null, // reasoningSteps
            sessionInfo
        );

        return agentResult;
      }
      catch (Exception ex)
      {
        _logger.Log(LogLevel.Error, $"Erro durante execução streaming do Agent {Name}: {ex.Message}", ex);
        throw;
      }
    }

    private void PrepareContext(TContext contextVar, List<AIMessage> messageHistory)
    {
      messageHistory = messageHistory ?? new List<AIMessage>();

      if (_agentContext == null)
      {
        _agentContext = new AgentContext<TContext>(
            contextVar,
            messageHistory,
            _modelConfig
        );
      }
      else
      {
        if (contextVar != null)
          _agentContext.ContextVar = contextVar;

        if (messageHistory.Count > 0)
          _agentContext.MessageHistory = messageHistory;
      }
    }

    /// <summary>
    /// Processa o reasoning step-by-step
    /// </summary>
    private async Task<ReasoningResult> ProcessReasoningAsync(string originalPrompt, List<AIMessage> messageHistory, CancellationToken cancellationToken)
    {
      var reasoningModel = _reasoningModel ?? _model;

      // Prompt estruturado para reasoning (com JSON schema)
      var reasoningPrompt = $@"Analise este problema step-by-step usando raciocínio estruturado.

PROMPT ORIGINAL: {originalPrompt}

Execute reasoning em {_reasoningMinSteps}-{_reasoningMaxSteps} etapas estruturadas.

RESPONDA EXCLUSIVAMENTE EM JSON seguindo esta estrutura:
{{
  ""reasoning_steps"": [
    {{
      ""title"": ""Nome da etapa"",
      ""action"": ""O que você está fazendo"",
      ""result"": ""Resultado desta etapa"",
      ""reasoning"": ""Seu processo de pensamento detalhado"",
      ""confidence"": 0.85,
      ""next_action"": ""Continue""
    }}
  ]
}}

ETAPAS OBRIGATÓRIAS:
1. COMPREENSÃO: Entenda o problema
2. ANÁLISE: Identifique elementos-chave
3. RACIOCÍNIO: Desenvolva solução lógica
4. VALIDAÇÃO: Verifique consistência
5. CONCLUSÃO: Apresente resultado final

IMPORTANTE: Responda APENAS com JSON válido, sem texto adicional.";

      try
      {
        var reasoningMessages = new List<AIMessage>
                {
                    AIMessage.System("Você é um especialista em reasoning step-by-step. Analise problemas de forma estruturada e responda EXCLUSIVAMENTE em JSON seguindo o schema fornecido."),
                    AIMessage.User(reasoningPrompt)
                };

        var reasoningRequest = new ModelRequest
        {
          Messages = reasoningMessages
        };

        var reasoningResponse = await reasoningModel.GenerateResponseAsync(
            reasoningRequest,
            _modelConfig,
            cancellationToken);

        // Tentar fazer parse do JSON estruturado
        if (!string.IsNullOrEmpty(reasoningResponse?.Content))
        {
          try
          {
            var reasoningData = JsonSerializer.Deserialize<ReasoningSteps>(reasoningResponse.Content);

            // Retornar reasoning formatado com steps estruturados
            var formattedContent = FormatReasoningOutput(reasoningData.Steps);
            return new ReasoningResult(formattedContent, reasoningData.Steps);
          }
          catch (JsonException)
          {
            // Fallback: se não conseguir fazer parse JSON, retorna texto bruto
            _logger.Log(LogLevel.Warning, "Reasoning response não está em formato JSON válido, usando texto bruto");
            return new ReasoningResult(reasoningResponse.Content, new List<ReasoningStep>());
          }
        }

        return new ReasoningResult("", new List<ReasoningStep>());
      }
      catch (Exception ex)
      {
        _logger.Log(LogLevel.Warning, $"Erro durante processamento de reasoning: {ex.Message}");
        return new ReasoningResult("", new List<ReasoningStep>());
      }
    }

    /// <summary>
    /// Formata a saída do reasoning de forma estruturada
    /// </summary>
    private string FormatReasoningOutput(List<ReasoningStep> steps)
    {
      if (steps == null || steps.Count == 0)
        return "";

      var output = "🧠 PROCESSO DE RACIOCÍNIO ESTRUTURADO:\n";
      output += new string('═', 50) + "\n\n";

      for (int i = 0; i < steps.Count; i++)
      {
        var step = steps[i];
        output += $"📍 ETAPA {i + 1}: {step.Title?.ToUpper()}\n";
        output += $"🎯 Ação: {step.Action}\n";
        output += $"💭 Raciocínio: {step.Reasoning}\n";
        output += $"📊 Resultado: {step.Result}\n";

        if (step.Confidence.HasValue)
        {
          output += $"🎯 Confiança: {step.Confidence.Value:P0}\n";
        }

        if (step.NextAction.HasValue)
        {
          output += $"➡️  Próxima ação: {step.NextAction.Value}\n";
        }

        output += "\n" + new string('-', 40) + "\n\n";
      }

      return output;
    }

    #endregion

    #region IAgent Implementation

    public string GetSystemPrompt()
    {
      return _promptManager.BuildSystemPrompt(Context);
    }

    async Task<object> IAgent.ExecuteAsync(string prompt, object context, List<AIMessage> messageHistory, CancellationToken cancellationToken)
    {
      if (context is TContext typedContext)
      {
        var result = await ExecuteAsync(prompt, typedContext, messageHistory, cancellationToken);
        return result;
      }
      throw new ArgumentException($"Context deve ser do tipo {typeof(TContext).Name}");
    }

    public List<AIMessage> GetMessageHistory()
    {
      return _agentContext?.MessageHistory ?? new List<AIMessage>();
    }

    /// <summary>
    /// Retorna o resumo inteligente da memória gerado pela LLM
    /// </summary>
    public string GetMemorySummary()
    {
      return _memorySummary;
    }

    /// <summary>
    /// Obtém o MemoryManager atual (para compatibilidade com tools)
    /// </summary>
    public IMemoryManager GetMemoryManager()
    {
        // Ensure MemoryManager has current context information
        if (_memoryManager != null)
        {
            string userId = GetUserIdFromContext();
            string sessionId = GetSessionIdFromContext();
            _memoryManager.UserId = userId;
            _memoryManager.SessionId = sessionId;
        }
        return _memoryManager;
    }

    #endregion

    #region Private Helper Methods

    private string GetUserIdFromContext()
    {
        // Tentar obter userId do contexto
        if (_agentContext != null && _agentContext.ContextVar != null)
        {
            var contextType = typeof(TContext);
            var userIdProperty = contextType.GetProperty("UserId");
            if (userIdProperty != null)
            {
                var userId = userIdProperty.GetValue(_agentContext.ContextVar)?.ToString();
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }
        }

        // Se modo anônimo está habilitado e não há userId, gerar um automaticamente
        if (_enableAnonymousMode)
        {
            if (string.IsNullOrEmpty(_generatedUserId))
            {
                _generatedUserId = GenerateAnonymousUserId();
            }
            SetUserIdInContext(_generatedUserId);
            return _generatedUserId;
        }

        // Fallback: usar um userId padrão
        return "default_user";
    }

    private string GetSessionIdFromContext()
    {
        // Tentar obter sessionId do contexto
        if (_agentContext != null && _agentContext.ContextVar != null)
        {
            var contextType = typeof(TContext);
            var sessionIdProperty = contextType.GetProperty("SessionId");
            if (sessionIdProperty != null)
            {
                var sessionId = sessionIdProperty.GetValue(_agentContext.ContextVar)?.ToString();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    return sessionId;
                }
            }
        }

        // Se modo anônimo está habilitado e não há sessionId, gerar um automaticamente
        if (_enableAnonymousMode)
        {
            if (string.IsNullOrEmpty(_generatedSessionId))
            {
                _generatedSessionId = GenerateAnonymousSessionId();
            }
            SetSessionIdInContext(_generatedSessionId);
            return _generatedSessionId;
        }

        // Fallback: usar um sessionId padrão baseado no nome do agente
        return $"{Name}_session_{DateTime.Now:yyyyMMdd}";
    }

    private string GenerateAnonymousUserId()
    {
        // Gerar userId anônimo: "anonymous_" + 8 caracteres aleatórios
        return $"anonymous_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    private string GenerateAnonymousSessionId()
    {
        // Gerar sessionId único
        return Guid.NewGuid().ToString();
    }

    private void SetUserIdInContext(string userId)
    {
        if (_agentContext != null && _agentContext.ContextVar != null)
        {
            var contextType = typeof(TContext);
            var userIdProperty = contextType.GetProperty("UserId");
            if (userIdProperty != null && userIdProperty.CanWrite)
            {
                userIdProperty.SetValue(_agentContext.ContextVar, userId);
            }
        }
    }

    private void SetSessionIdInContext(string sessionId)
    {
        if (_agentContext != null && _agentContext.ContextVar != null)
        {
            var contextType = typeof(TContext);
            var sessionIdProperty = contextType.GetProperty("SessionId");
            if (sessionIdProperty != null && sessionIdProperty.CanWrite)
            {
                sessionIdProperty.SetValue(_agentContext.ContextVar, sessionId);
            }
        }
    }

    private SessionInfo CreateSessionInfo()
    {
        string userId = GetUserIdFromContext();
        string sessionId = GetSessionIdFromContext();

        bool wasGenerated = _enableAnonymousMode &&
            (userId.StartsWith("anonymous_") || sessionId.Length == 36); // GUID length

        return new SessionInfo
        {
            UserId = userId,
            SessionId = sessionId,
            WasGenerated = wasGenerated
        };
    }

    #endregion

    /// <summary>
    /// Gera um resumo inteligente do histórico de mensagens usando a LLM
    /// </summary>
    private async Task<string> GenerateMemorySummaryAsync(List<AIMessage> messageHistory, CancellationToken cancellationToken)
    {
        if (messageHistory == null || messageHistory.Count == 0)
            return "";
        var historyLines = new List<string>();
        foreach (var m in messageHistory)
        {
            historyLines.Add($"[{m.Role}] {m.Content}");
        }
        var summaryPrompt = $@"Resuma o histórico de mensagens abaixo em até 5 frases, destacando os pontos principais, decisões e contexto relevante. Seja objetivo e claro.\n\nHISTÓRICO:\n{string.Join("\n", historyLines)}";
        var summaryRequest = new ModelRequest
        {
            Messages = new List<AIMessage>
            {
                AIMessage.System("Você é um agente especialista em sumarização de contexto. Responda apenas com o resumo solicitado."),
                AIMessage.User(summaryPrompt)
            }
        };
        var summaryResponse = await _model.GenerateResponseAsync(summaryRequest, _modelConfig, cancellationToken);
        _memorySummary = summaryResponse?.Content ?? "";
        return _memorySummary;
    }
   internal Agent<TContext, TResult> SetMemoryDomainConfiguration(MemoryDomainConfiguration config)
    {
        _memoryDomainConfig = config;

        // Recriar MemoryManager com nova configuração de domínio
        _memoryManager = new MemoryManager(
            _storage,
            _model,
            _logger,
            new MockEmbeddingService(),
            _memoryDomainConfig);

        return this;
    }

    internal MemoryDomainConfiguration GetMemoryDomainConfiguration()
    {
        return _memoryDomainConfig;
    }

  }
}
