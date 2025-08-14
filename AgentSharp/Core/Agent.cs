using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Configuration;
using AgentSharp.Models;
using AgentSharp.Tools;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq; // Added for Any() and FirstOrDefault()
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
    public string description
    {
        get
        {
            try
            {
                return _agentContext != null ? _promptManager.BuildSystemPrompt(Context) : Name ?? "Agent";
            }
            catch
            {
                return Name ?? "Agent";
            }
        }
    } // Para compatibilidade

    private readonly PromptManager<TContext> _promptManager;
    private readonly ToolManager _toolManager;
    private readonly ExecutionEngine<TContext, TResult> _executionEngine;
    private readonly IModel _model;
    private readonly ILogger _logger;
    private ITelemetryService _telemetry;

    // Static telemetry service for global injection
    private static ITelemetryService _globalTelemetry;

    /// <summary>
    /// Configures global telemetry service for all Agent instances
    /// </summary>
    /// <param name="telemetry">Telemetry service to use globally</param>
    public static void ConfigureGlobalTelemetry(ITelemetryService telemetry)
    {
        _globalTelemetry = telemetry;
    }

   // Configuração do modelo (parâmetros, temperatura, etc.)
    private ModelConfiguration _modelConfig;

    /// Configurações específicas de memória
    /// Como Quantidade máxima de memórias (sessão e usuário), max execuções, relevância mínima, auto summary, numero minimo para cria resumo.
    private AgentSharp.Core.Memory.Configuration.MemoryConfiguration _memoryConfig;

    // Configuração de memória (prompts customizados para salvar e recuperar, etc.)
    private MemoryDomainConfiguration _memoryDomainConfig;

    // Gestão avançada de memória
    private IMemoryManager _memoryManager;

    // Serviços separados de memória (removidos - funcionalidade integrada no MemoryManager)

    // Serviço de armazenamento (pode ser InMemory, Sqlite, etc.)
    private IStorage _storage;

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

    // 🎯 CONTROLES GRANULARES
    // Permitem configuração fine-tuned de funcionalidades custosas

    /// <summary>
    /// Habilita extração automática de memórias pela LLM (opt-in explícito para custos)
    /// Habilita geração automática de memórias do usuário
    /// </summary>
    public bool EnableUserMemories { get; set; } = false;

    /// <summary>
    /// Habilita busca semântica via SmartMemoryToolPack (function calling)
    /// Permite controlar quando LLM pode buscar memórias existentes
    /// </summary>
    public bool EnableMemorySearch { get; set; } = false;

    /// <summary>
    /// Inclui histórico de mensagens no context enviado para o LLM
    /// Adiciona histórico da sessão às mensagens
    /// </summary>
    public bool AddHistoryToMessages { get; set; } = false;

    /// <summary>
    /// Número de mensagens históricas a incluir no context
    /// Número de mensagens do histórico a carregar
    /// </summary>
    public int NumHistoryMessages { get; set; } = 10;

    /// <summary>
    /// Habilita busca em Knowledge/RAG externo (futuro)
    /// Para integração com serviços RAG customizados
    /// </summary>
    public bool EnableKnowledgeSearch { get; set; } = false;

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

      // 🎯 SEPARAÇÃO DE SERVIÇOS DE MEMÓRIA
      // Funcionalidade agora integrada no MemoryManager

      // Storage legado mantido para compatibilidade (mas não usado por padrão)
      _storage = storage; // null por padrão - sem armazenamento automático

      // MemoryManager só é criado se semantic memory estiver habilitado
      if (storage != null)
      {
          // Se o storage for SemanticSqliteStorage, usar o embedding service dele
          IEmbeddingService embeddingService = new MockEmbeddingService();
          if (storage is SemanticSqliteStorage vectorStorage)
          {
              // TODO: Extrair o embedding service do SemanticSqliteStorage se necessário
              embeddingService = new MockEmbeddingService();
          }

          _memoryManager = memoryManager ?? new MemoryManager(
              _storage,
              _model,
              _logger,
              embeddingService,
              _memoryDomainConfig);
      }
      else
      {
          _memoryManager = null; // Sem semantic memory por padrão
      }

      _promptManager = new PromptManager<TContext>();
      _toolManager = new ToolManager();
      _telemetry = _globalTelemetry; // Use global telemetry if available
      _executionEngine = new ExecutionEngine<TContext, TResult>(_model, _modelConfig, _logger, null, _telemetry);

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

      // 🎯 REGISTRAR SmartMemoryToolPack APENAS SE CONTROLES PERMITIREM
      // Controle granular: enable_user_memories + enable_memory_search
      // Evita custos desnecessários de processamento semântico
      if (_memoryManager != null && (EnableUserMemories || EnableMemorySearch))
      {
          _toolManager.RegisterToolPack(new SmartMemoryToolPack());

          _logger.Log(LogLevel.Debug,
              $"SmartMemoryToolPack registrado - LLM pode gerenciar memórias (EnableUserMemories: {EnableUserMemories}, EnableMemorySearch: {EnableMemorySearch})");
      }
      else
      {
          _logger.Log(LogLevel.Debug,
              "SmartMemoryToolPack NÃO registrado - controles desabilitados ou memory manager não configurado (operação zero-cost)");
      }

      // TODO: FUTURO - Registrar KnowledgeToolPack se EnableKnowledgeSearch = true
      // if (EnableKnowledgeSearch && _knowledgeService != null)
      // {
      //     _toolManager.RegisterToolPack(new KnowledgeToolPack(_knowledgeService));
      // }
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

      public Agent<TContext, TResult> WithMemoryConfiguration(AgentSharp.Core.Memory.Configuration.MemoryConfiguration config)
      {
          if (config == null)
              throw new ArgumentNullException(nameof(config));

          _memoryConfig = config;

          // Aplicar configuração ao _memoryDomainConfig existente
          if (_memoryDomainConfig == null)
              _memoryDomainConfig = new MemoryDomainConfiguration();

          // Copiar configurações da MemoryConfiguration para MemoryDomainConfiguration
          _memoryDomainConfig.ExtractionPromptTemplate = config.ExtractionPromptTemplate;
          _memoryDomainConfig.ClassificationPromptTemplate = config.ClassificationPromptTemplate;
          _memoryDomainConfig.RetrievalPromptTemplate = config.RetrievalPromptTemplate;
          _memoryDomainConfig.CustomCategories = config.CustomCategories;
          _memoryDomainConfig.MaxMemoriesPerInteraction = config.MaxMemoriesPerInteraction;
          _memoryDomainConfig.MinImportanceThreshold = config.MinImportanceThreshold;

          // Se já existe um memory manager, recriar com nova configuração
          if (_memoryManager != null && _storage != null)
          {
              // Extrair configurações do memory manager atual
              string currentUserId = _memoryManager.UserId;
              string currentSessionId = _memoryManager.SessionId;
              int? currentLimit = _memoryManager.Limit;

              // Recriar MemoryManager com nova configuração
              _memoryManager = new MemoryManager(
                  _storage,
                  _model,
                  _logger,
                  new MockEmbeddingService(),
                  _memoryDomainConfig);

              // Restaurar configurações
              _memoryManager.UserId = currentUserId;
              _memoryManager.SessionId = currentSessionId;
              _memoryManager.Limit = currentLimit;

              // Semantic memory service integrado no MemoryManager

              // Re-registrar tools pois memory manager foi recriado
              registerTools();
          }

          _logger.Log(LogLevel.Debug, "MemoryConfiguration aplicada com sucesso");
          return this;
      }

    /// <summary>
    /// Enable semantic memory with advanced AI-powered memory features (incurs processing costs)
    /// </summary>
    public Agent<TContext, TResult> WithSemanticMemory(IStorage storage, IEmbeddingService embeddingService = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));

        // Replace NoOp with real semantic memory service
        // TODO: Create real implementation that uses storage + MemoryManager
        var realEmbeddingService = embeddingService ?? new MockEmbeddingService();

        _memoryManager = new MemoryManager(
            _storage,
            _model,
            _logger,
            realEmbeddingService,
            _memoryDomainConfig);

        // Semantic memory service integrado no MemoryManager

        // Re-register tools with semantic memory enabled
        registerTools();

        return this;
    }

    /// <summary>
    /// Configure custom message history service (lightweight conversation logging)
    /// </summary>
    public Agent<TContext, TResult> WithMessageHistory(IMessageHistoryService historyService)
    {
        // Funcionalidade de message history agora integrada no MemoryManager
        // Este método mantido para compatibilidade mas não tem efeito
        if (historyService == null)
            throw new ArgumentNullException(nameof(historyService));
        
        _logger.Log(LogLevel.Warning, "WithMessageHistory está obsoleto - funcionalidade integrada no MemoryManager");
        return this;
    }

    /// <summary>
    /// Enable both semantic memory and custom message history
    /// </summary>
    public Agent<TContext, TResult> WithFullMemory(IStorage storage, IMessageHistoryService historyService = null, IEmbeddingService embeddingService = null)
    {
        WithSemanticMemory(storage, embeddingService);

        if (historyService != null)
        {
            WithMessageHistory(historyService);
        }

        return this;
    }

    /// <summary>
    /// Habilita extração automática de memórias pela LLM (opt-in para custos de embedding)
    /// Habilita geração automática de memórias do usuário
    /// </summary>
    public Agent<TContext, TResult> WithUserMemories(bool enable = true)
    {
        EnableUserMemories = enable;

        // Re-registrar tools se necessário
        if (_memoryManager != null)
        {
            registerTools();
        }

        _logger.Log(LogLevel.Debug, $"EnableUserMemories definido como: {enable}");
        return this;
    }

    /// <summary>
    /// Habilita busca semântica via SmartMemoryToolPack (function calling)
    /// Permite controlar quando LLM pode buscar memórias
    /// </summary>
    public Agent<TContext, TResult> WithMemorySearch(bool enable = true)
    {
        EnableMemorySearch = enable;

        // Re-registrar tools se necessário
        if (_memoryManager != null)
        {
            registerTools();
        }

        _logger.Log(LogLevel.Debug, $"EnableMemorySearch definido como: {enable}");
        return this;
    }

    /// <summary>
    /// Inclui histórico de mensagens no context do LLM
    /// Adiciona histórico da sessão às mensagens
    /// </summary>
    public Agent<TContext, TResult> WithHistoryToMessages(bool enable = true, int numMessages = 10)
    {
        AddHistoryToMessages = enable;
        NumHistoryMessages = numMessages;

        _logger.Log(LogLevel.Debug, $"AddHistoryToMessages: {enable}, NumHistoryMessages: {numMessages}");
        return this;
    }

    /// <summary>
    /// Habilita busca em Knowledge/RAG externo (futuro)
    /// Para integração com serviços RAG customizados
    /// </summary>
    public Agent<TContext, TResult> WithKnowledgeSearch(bool enable = true)
    {
        EnableKnowledgeSearch = enable;

        _logger.Log(LogLevel.Debug, $"EnableKnowledgeSearch definido como: {enable}");
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
        if (memoryManager == null)
            throw new ArgumentNullException(nameof(memoryManager));

        // Substituir o memory manager atual
        _memoryManager = memoryManager;

        // Configurar contexto se disponível
        string userId = GetUserIdFromContext();
        string sessionId = GetSessionIdFromContext();
        
        if (!string.IsNullOrEmpty(userId))
        {
            _memoryManager.UserId = userId;
        }
        
        if (!string.IsNullOrEmpty(sessionId))
        {
            _memoryManager.SessionId = sessionId;
        }

        // Semantic memory service integrado no MemoryManager

        // Re-registrar tools pois memory manager mudou
        registerTools();

        _logger.Log(LogLevel.Debug, "MemoryManager substituído com sucesso");
        return this;
    }

    public Agent<TContext, TResult> WithStorage(IStorage storage)
    {
        if (storage == null)
            throw new ArgumentNullException(nameof(storage));

        // Substituir o storage atual
        _storage = storage;

        // Se já existe um memory manager, recriar com novo storage
        if (_memoryManager != null)
        {
            // Extrair configurações do memory manager atual
            string currentUserId = _memoryManager.UserId;
            string currentSessionId = _memoryManager.SessionId;
            int? currentLimit = _memoryManager.Limit;

            // Determinar embedding service (reutilizar ou criar mock)
            IEmbeddingService embeddingService = new MockEmbeddingService();
            if (storage is SemanticSqliteStorage sqliteStorage)
            {
                // Se o storage for SQLite, pode ter seu próprio embedding service
                embeddingService = new MockEmbeddingService(); // TODO: Extrair do SQLite se disponível
            }

            // Recriar memory manager com novo storage
            _memoryManager = new MemoryManager(
                _storage,
                _model,
                _logger,
                embeddingService,
                _memoryDomainConfig);

            // Restaurar configurações
            _memoryManager.UserId = currentUserId;
            _memoryManager.SessionId = currentSessionId;
            _memoryManager.Limit = currentLimit;

            // Semantic memory service integrado no MemoryManager

            // Re-registrar tools pois memory manager foi recriado
            registerTools();
        }
        else
        {
            // Se não havia memory manager, criar um novo (opt-in)
            // Só cria se controles estão habilitados
            if (EnableUserMemories || EnableMemorySearch)
            {
                IEmbeddingService embeddingService = new MockEmbeddingService();
                
                _memoryManager = new MemoryManager(
                    _storage,
                    _model,
                    _logger,
                    embeddingService,
                    _memoryDomainConfig);

                // Configurar contexto se disponível
                string userId = GetUserIdFromContext();
                string sessionId = GetSessionIdFromContext();
                
                if (!string.IsNullOrEmpty(userId))
                {
                    _memoryManager.UserId = userId;
                }
                
                if (!string.IsNullOrEmpty(sessionId))
                {
                    _memoryManager.SessionId = sessionId;
                }

                // Semantic memory service integrado no MemoryManager

                // Re-registrar tools
                registerTools();
            }
        }

        _logger.Log(LogLevel.Debug, $"Storage substituído com sucesso: {storage.GetType().Name}");
        return this;
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

        // Construir mensagens base
        var systemPrompt = _promptManager.BuildSystemPrompt(Context);
        var baseMessages = new List<AIMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
          baseMessages.Add(AIMessage.System(systemPrompt));
        baseMessages.Add(AIMessage.User(finalPrompt));

        List<AIMessage> enhancedMessages;

        // 🎯 CONDICIONAL: Usar MemoryManager apenas se semantic memory estiver ativo
        if (_memoryManager != null)
        {
            _memoryManager.UserId = userId;
            _memoryManager.SessionId = sessionId;

            // Carregar contexto de memória
            var memoryContext = await _memoryManager.LoadContextAsync(userId, sessionId);

            // Enriquecer mensagens com memórias relevantes
            enhancedMessages = await _memoryManager.EnhanceMessagesAsync(baseMessages, memoryContext);
        }
        else if (AddHistoryToMessages && _storage != null)
        {
            // 🎯 HISTÓRICO DE SESSÃO: Como no /refs, carregar mensagens da sessão quando não usar memory extraction
            // Isso permite manter contexto conversacional sem custos de embedding/semantic search
            enhancedMessages = await AddSessionHistoryToMessages(baseMessages, userId, sessionId);
        }
        else
        {
            // Usar apenas as mensagens básicas (baixo custo)
            enhancedMessages = baseMessages;
        }

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

        // 🎯 PROCESSAMENTO DE MEMÓRIA CONDICIONAL
        if (_memoryManager != null)
        {
            var memoryContext = new MemoryContext { UserId = userId, SessionId = sessionId };
            await _memoryManager.ProcessInteractionAsync(userMessage, assistantMessage, memoryContext);
        }
        else if (AddHistoryToMessages && _storage != null)
        {
            // 🎯 HISTÓRICO DE SESSÃO: Salvar mensagens no storage mesmo sem memory extraction
            await SaveSessionMessagesToStorage(userMessage, assistantMessage, userId, sessionId);
        }

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

    /// <summary>
    /// Adiciona histórico de sessão às mensagens quando não usar memory extraction
    /// </summary>
    private async Task<List<AIMessage>> AddSessionHistoryToMessages(List<AIMessage> baseMessages, string userId, string sessionId)
    {
        try
        {
            // Buscar mensagens da sessão no storage
            var sessionMessages = await _storage.GetSessionHistoryAsync(userId, sessionId, NumHistoryMessages);

            if (sessionMessages != null && sessionMessages.Count > 0)
            {
                var enhancedMessages = new List<AIMessage>();

                // Adicionar system message se existir
                var systemMessage = baseMessages.FirstOrDefault(m => m.Role == Role.System);
                if (systemMessage != null)
                    enhancedMessages.Add(systemMessage);

                // Adicionar histórico da sessão
                enhancedMessages.AddRange(sessionMessages);

                // Adicionar mensagem atual do usuário
                var userMessage = baseMessages.FirstOrDefault(m => m.Role == Role.User);
                if (userMessage != null)
                    enhancedMessages.Add(userMessage);

                _logger.Log(LogLevel.Debug, $"Adicionadas {sessionMessages.Count} mensagens do histórico da sessão");
                return enhancedMessages;
            }
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Warning, $"Erro ao carregar histórico da sessão: {ex.Message}");
        }

        return baseMessages;
    }

    /// <summary>
    /// Salva mensagens no storage para histórico de sessão
    /// </summary>
    private async Task SaveSessionMessagesToStorage(AIMessage userMessage, AIMessage assistantMessage, string userId, string sessionId)
    {
        try
        {
            await _storage.SaveSessionMessageAsync(userId, sessionId, userMessage);
            await _storage.SaveSessionMessageAsync(userId, sessionId, assistantMessage);

            _logger.Log(LogLevel.Debug, $"Mensagens salvas no histórico da sessão: {sessionId}");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Warning, $"Erro ao salvar mensagens no histórico: {ex.Message}");
        }
    }

  }
}
