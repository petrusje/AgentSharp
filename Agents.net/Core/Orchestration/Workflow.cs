using Arcana.AgentsNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Core.Orchestration
{
  /// <summary>
  /// Interface base para workflows
  /// </summary>
  public interface IWorkflow<TContext>
  {
    IWorkflow<TContext> AddStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput);
    Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
  }

  /// <summary>
  /// Classe base para todos os workflows
  /// </summary>
  public abstract class Workflow<TContext> : IWorkflow<TContext>
  {
    protected readonly string _name;

    // Nome do workflow
    public string Name => _name;
    // Logger
    public ILogger Logger => _logger;
    // Lista de passos do workflow
    public List<WorkflowStep> Steps => _steps;
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

    protected Workflow(string name, ILogger logger = null)
    {
      _name = name;
      _logger = logger ?? new ConsoleLogger();


    }
    public IWorkflow<TContext> AddStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput)
    {
      _steps.Add(new WorkflowStep(name, agent, getInput, processOutput));
      return this;
    }

    public abstract Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
  }
}
