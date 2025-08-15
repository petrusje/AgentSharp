using AgentSharp.Utils;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Configuration;
using AgentSharp.Core.Validation;
using AgentSharp.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentSharp.Core.Orchestration
{
  /// <summary>
  /// Interface base para workflows
  /// </summary>
  public interface IWorkflow<TContext> : IAsyncDisposable, IDisposable
  {
    IWorkflow<TContext> AddStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput);
    Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
  }

  /// <summary>
  /// Classe base para todos os workflows com resource management completo
  /// </summary>
  public abstract class Workflow<TContext> : IWorkflow<TContext>
  {
    protected readonly string _name;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly CompositeDisposable _disposables;
    private volatile bool _disposed = false;
    private readonly IWorkflowLogger _workflowLogger;
    private readonly WorkflowResourceConfiguration _config;

    // Nome do workflow
    public string Name => _name;
    // Logger
    public ILogger Logger => _logger;
    // Lista de passos do workflow
    public List<WorkflowStep> Steps => _steps;
    // CancellationToken para todas as operações do workflow
    protected CancellationToken WorkflowCancellationToken => _cancellationTokenSource.Token;
    
    protected readonly ILogger _logger;
    protected readonly List<WorkflowStep> _steps = new List<WorkflowStep>();

    public class WorkflowStep
    {
      public string Name { get; }
      public IAgent Agent { get; }
      public Func<TContext, string> GetInput { get; }
      public Action<TContext, string> ProcessOutput { get; }

      // retorna o resultado do passo
      // ultima mensagem do agente
      public string Result
      {
        get
        {
          return Agent.GetMessageHistory().LastOrDefault()?.Content ?? string.Empty;
        }
      }



      public WorkflowStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput)
      {
        Name = name;
        Agent = agent;
        GetInput = getInput;
        ProcessOutput = processOutput;
      }
    }

    protected Workflow(string name, ILogger logger = null, 
                      WorkflowResourceConfiguration config = null,
                      IWorkflowLogger workflowLogger = null)
    {
      _name = name;
      _logger = logger ?? new ConsoleLogger();
      _config = config ?? WorkflowResourceConfiguration.Production;
      _workflowLogger = workflowLogger ?? new ProductionWorkflowLogger(_logger);
      
      // Inicializar CompositeDisposable com configuração
      _disposables = new CompositeDisposable(_workflowLogger, _config);

      // Registrar recursos básicos no CompositeDisposable
      _disposables.Add(_cancellationTokenSource);
      
      _workflowLogger.LogInformation("Workflow {0} initialized with {1} initial resources", 
                                     _name, _disposables.Count);
    }
    public IWorkflow<TContext> AddStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput)
    {
      _steps.Add(new WorkflowStep(name, agent, getInput, processOutput));
      return this;
    }

    public abstract Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);

    #region Resource Management

    /// <summary>
    /// Registra um recurso IDisposable para cleanup automático
    /// </summary>
    /// <param name="disposable">Recurso a ser gerenciado</param>
    protected void RegisterDisposable(IDisposable disposable)
    {
      ThrowIfDisposed();
      _disposables.Add(disposable);
    }

    /// <summary>
    /// Registra um recurso IAsyncDisposable para cleanup automático
    /// </summary>
    /// <param name="asyncDisposable">Recurso async a ser gerenciado</param>
    protected void RegisterAsyncDisposable(IAsyncDisposable asyncDisposable)
    {
      ThrowIfDisposed();
      _disposables.Add(asyncDisposable);
    }

    /// <summary>
    /// Verifica se o workflow foi disposed e lança exceção se sim
    /// </summary>
    protected void ThrowIfDisposed()
    {
      if (_disposed)
        throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>
    /// Dispose síncrono - cancela operações e limpa recursos
    /// </summary>
    public virtual void Dispose()
    {
      if (_disposed) return;

      _disposed = true;
      
      try
      {
        // Cancelar todas as operações em andamento
        _cancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
        // CancellationTokenSource já foi disposed
      }

      // Cleanup de todos os recursos registrados
      _disposables.Dispose();

      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose assíncrono - cancela operações e limpa recursos
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
      if (_disposed) return;

      _disposed = true;
      
      try
      {
        // Cancelar todas as operações em andamento
        _cancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
        // CancellationTokenSource já foi disposed
      }

      // Cleanup assíncrono de todos os recursos registrados
      await _disposables.DisposeAsync();
      
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer para cleanup se Dispose não foi chamado
    /// </summary>
    ~Workflow()
    {
      Dispose();
    }

    #endregion
  }
}
