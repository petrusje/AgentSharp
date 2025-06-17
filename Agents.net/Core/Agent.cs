using Agents.net.Core.Memory; // Added for IMemoryStore
using Agents.net.Models;
using Agents.net.Tools;
using Agents.net.Utils;

using System;
using System.Collections.Generic;
using System.Text.Json; // Added for JsonSerializer
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Agents.net.Core
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
    private ModelConfiguration _modelConfig;

    private readonly IMemory _memory;

    // Estado do agente
    private AgentContext<TContext> _agentContext;

    // Configurações de reasoning
    private bool _reasoning = false;
    private IModel _reasoningModel;
    private int _reasoningMinSteps = 1;
    private int _reasoningMaxSteps = 10;

    // Construtor principal
    public Agent(
        IModel model,
        string name = null,
        string instructions = null,
        ModelConfiguration modelConfig = null,
        ILogger logger = null,
        IMemory memory = null)
    {
      Name = name ?? GetType().Name;
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _logger = logger ?? new ConsoleLogger();
      _modelConfig = modelConfig ?? new ModelConfiguration();
      _memory = memory ?? new InMemoryStore();

      _promptManager = new PromptManager<TContext>();
      _toolManager = new ToolManager();
      _executionEngine = new ExecutionEngine<TContext, TResult>(_model, _modelConfig, _logger, _memory);

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
        TContext contextVar = default(TContext),
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

        // Criar sistema de mensagens usando PromptManager
        var systemPrompt = _promptManager.BuildSystemPrompt(Context);
        var messages = new List<AIMessage>();

        if (!string.IsNullOrEmpty(systemPrompt))
          messages.Add(AIMessage.System(systemPrompt));

        messages.AddRange(_agentContext.MessageHistory);
        messages.Add(AIMessage.User(finalPrompt));

        // Criar requisição usando ToolManager
        var request = new ModelRequest
        {
          Messages = messages,
          Tools = _toolManager.GetTools()
        };

        // Usar ExecutionEngine para executar
        var executionResult = await _executionEngine.ExecuteWithToolsAsync(
            request,
            _modelConfig,
            cancellationToken);

        // Atualizar histórico
        if (!string.IsNullOrEmpty(executionResult.RawResponse.Content))
          _agentContext.MessageHistory.Add(AIMessage.Assistant(executionResult.RawResponse.Content));

        // Criar resultado do agente
        var agentResult = new AgentResult<TResult>(
            executionResult.Data,
            _agentContext.MessageHistory,
            _agentContext.MessageHistory.Count,
            executionResult.RawResponse.Usage,
            executionResult.ToolResults,
            reasoningContent,
            reasoningSteps
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
        TContext contextVar = default(TContext),
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
        var agentResult = new AgentResult<TResult>(
            executionResult.Data,
            _agentContext.MessageHistory,
            _agentContext.MessageHistory.Count,
            executionResult.RawResponse.Usage,
            executionResult.ToolResults
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
      var reasoningMemory = new ReasoningMemory(_reasoningMaxSteps);

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

<next_action>
Continue, Validate, FinalAnswer, Reset
</next_action>

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
            string cleanedResponse = RemoveCodeDelimiters(reasoningResponse.Content);
            var reasoningData = JsonSerializer.Deserialize<ReasoningSteps>(cleanedResponse);

            // Adicionar steps à memória
            foreach (var step in reasoningData.Steps)
            {
              reasoningMemory.AddStep(step);
            }

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

    static string RemoveCodeDelimiters(string response)
    {
      try
      {
        const string pattern = @"```(csharp|html|json|sql|xml)([\s\S]*?)```";
        var regex = new Regex(pattern);
        if (regex.IsMatch(response))
        {
          string cleanedResponse = regex.Replace(response, "$2").Trim();
          return cleanedResponse;
        }
        return response.Trim();
      }
      catch (Exception)
      {
        return response;
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

    #endregion
    /// <summary>
    /// Adiciona outro agente como uma ferramenta
    /// </summary>
    public Agent<TContext, TResult> WithToolAsAgent(IAgent toolAgent)
    {
      if (toolAgent == null)
        throw new ArgumentNullException(nameof(toolAgent));

      var tool = toolAgent.AsTool(); // Usa AgentExtensions.AsTool()
      return this.AddTool(tool);
    }

    /// <summary>
    /// Adiciona agente como tool com configuração customizada
    /// </summary>
    public Agent<TContext, TResult> WithToolAsAgent(
         IAgent toolAgent, string toolName,
         string toolDescription)
    {
      if (toolAgent == null)
        throw new ArgumentNullException(nameof(toolAgent));

      var tool = toolAgent.AsTool(toolName, toolDescription);
      return this.AddTool(tool);
    }

    /// <summary>
    /// Adiciona múltiplos agentes como tools
    /// </summary>
    public Agent<TContext, TResult> WithToolAsAgents(params IAgent[] agents)
    {
      if (agents == null)
        throw new ArgumentNullException(nameof(agents));

      foreach (var toolAgent in agents)
      {
        if (toolAgent != null)
          WithToolAsAgent(toolAgent);
      }
      return this;
    }
  }
}
