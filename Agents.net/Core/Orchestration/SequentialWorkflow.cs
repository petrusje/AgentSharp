using Arcana.AgentsNet.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Core.Orchestration
{
  /// <summary>
  /// Workflow que executa passos sequencialmente com gerenciamento thread-safe
  /// </summary>
  public class SequentialWorkflow<TContext, TResult> : Workflow<TContext>
  {
    private readonly PromptManager<TContext> _promptManager;
    private readonly object _contextLock = new object();
    private readonly object _executionLock = new object();

    private bool _hasInstructions = false;
    private TContext _context;
    private volatile bool _isExecuting = false;

    // propriedade do contexto com thread-safety
    public TContext Context
    {
      get
      {
        lock (_contextLock)
        {
          return _context;
        }
      }
      set
      {
        lock (_contextLock)
        {
          _context = value;
        }
      }
    }

    public string TaskGoal
    {
      get
      {
        lock (_contextLock)
        {
          return _context != null ? _promptManager.BuildSystemPrompt(_context) : string.Empty;
        }
      }
    }

    public SequentialWorkflow(string name, ILogger logger = null)
        : base(name, logger)
    {
      _promptManager = new PromptManager<TContext>();
    }

    public override async Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
      lock (_executionLock)
      {
        if (_isExecuting)
        {
          throw new InvalidOperationException("Workflow já está em execução. Execução paralela não é suportada.");
        }
        _isExecuting = true;
      }

      try
      {
        List<AIMessage> messages = new List<AIMessage>();

        _logger.Log(LogLevel.Info, $"Starting workflow {_name} with {_steps.Count} steps");

        // Adiciona o prompt do propósito do workflow ao contexto
        if (_hasInstructions)
        {
          AIMessage systemPrompt = AIMessage.System("<meta> Você é um Agente dentro de um workflow sequencial de Agentes, a meta final é:" + _promptManager.BuildSystemPrompt(context) + "</meta>");
          messages = new List<AIMessage> { systemPrompt };
        }

        // Definir contexto atual thread-safe
        Context = context;

        for (int i = 0; i < _steps.Count; i++)
        {
          var step = _steps[i];
          // Adiciona o prompt do passo atual ao contexto
          string input = step.GetInput(context);

          // Executar agente
          _logger.Log(LogLevel.Debug, $"Executing step {i + 1}/{_steps.Count}: {step.Name}");

          var result = await step.Agent.ExecuteAsync(input, context, messages, cancellationToken);
          string output = result?.ToString() ?? string.Empty;

          step.ProcessOutput(context, output);

          _logger.Log(LogLevel.Info, $"Completed step {i + 1}/{_steps.Count}: {step.Name}");

          // Atualizar contexto para próximo step
          Context = context;
        }

        _logger.Log(LogLevel.Info, $"Workflow {_name} completed");

        return context; // Retorna o contexto final processado, não o resultado
      }
      finally
      {
        lock (_executionLock)
        {
          _isExecuting = false;
        }
      }
    }

    public SequentialWorkflow<TContext, TResult> ForTask(Func<TContext, string> generator)
    {
      _promptManager.AddInstructions(generator);
      _hasInstructions = true;
      return this;
    }

    public SequentialWorkflow<TContext, TResult> RegisterStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput)
    {
      _steps.Add(new WorkflowStep(name, agent, getInput, processOutput));
      return this;
    }
  }
}
