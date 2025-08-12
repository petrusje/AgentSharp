# 🔄 Workflow

> Sistema de orquestração de agentes com gerenciamento de estado e observabilidade

## 📋 Sumário

- [Interfaces](#interfaces)
- [Classes Base](#classes-base)
- [Implementações](#implementações)
- [Exemplos](#exemplos)

## 🔌 Interfaces

### `IWorkflow<TContext>`
```csharp
public interface IWorkflow<TContext>
{
    IWorkflow<TContext> AddStep(
        string name,
        IAgent agent,
        Func<TContext, string> getInput,
        Action<TContext, string> processOutput);

    Task<TContext> ExecuteAsync(
        TContext context,
        CancellationToken cancellationToken = default);
}
```

## 🏗️ Classes Base

### `Workflow<TContext>`
```csharp
public abstract class Workflow<TContext>
{
    protected readonly string _name;
    protected readonly ILogger _logger;
    protected readonly List<WorkflowStep> _steps;

    public string Name => _name;
    public ILogger Logger => _logger;
    public List<WorkflowStep> Steps => _steps;

    protected Workflow(string name, ILogger logger = null);
}
```

### `WorkflowStep`
```csharp
public class WorkflowStep
{
    public string Name { get; }
    public IAgent Agent { get; }
    public Func<TContext, string> GetInput { get; }
    public Action<TContext, string> ProcessOutput { get; }
    public string Result { get; }
}
```

## 🔄 Implementações

### `SequentialWorkflow<TContext, TResult>`
```csharp
public class SequentialWorkflow<TContext, TResult> : Workflow<TContext>
{
    private readonly PromptManager<TContext> _promptManager;
    private readonly object _contextLock = new object();
    private readonly object _executionLock = new object();

    public TContext Context { get; set; }
    public string TaskGoal { get; }

    public SequentialWorkflow(string name, ILogger logger = null);

    public SequentialWorkflow<TContext, TResult> ForTask(
        Func<TContext, string> generator);

    public SequentialWorkflow<TContext, TResult> RegisterStep(
        string name,
        IAgent agent,
        Func<TContext, string> getInput,
        Action<TContext, string> processOutput);
}
```

### `AdvancedWorkflow<TContext, TResult>`
```csharp
public class AdvancedWorkflow<TContext, TResult> : Workflow<TContext>
{
    public string WorkflowId { get; }
    public string SessionId { get; }
    public string UserId { get; set; }
    public bool DebugMode { get; set; }
    public WorkflowSession Session { get; }

    public AdvancedWorkflow(string name, ILogger logger = null);

    public AdvancedWorkflow<TContext, TResult> ForTask(
        Func<TContext, string> generator);

    public AdvancedWorkflow<TContext, TResult> WithUserId(
        string userId);

    public AdvancedWorkflow<TContext, TResult> WithDebugMode(
        bool enabled = true);

    public AdvancedWorkflow<TContext, TResult> WithTelemetry(
        bool enabled = true);

    public AdvancedWorkflow<TContext, TResult> WithMemory(
        IMemory memory);

    public void CreateNewSession(string sessionName = null);
    public void LoadSession(WorkflowSession session);
    public WorkflowSession GetSessionSnapshot();
    public WorkflowMetrics GetMetrics();
}
```

## 📊 Componentes de Estado

### `WorkflowSession`
```csharp
public class WorkflowSession
{
    public string SessionId { get; set; }
    public string SessionName { get; set; }
    public string WorkflowId { get; set; }
    public string UserId { get; set; }
    public Dictionary<string, object> SessionState { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkflowRun> Runs { get; set; }
    public bool IsActive { get; set; }

    public void AddRun(WorkflowRun run);
    public void UpdateState(string key, object value);
    public T GetState<T>(string key, T defaultValue = default);
    public WorkflowSession DeepCopy();
}
```

### `WorkflowRun`
```csharp
public class WorkflowRun
{
    public string RunId { get; set; }
    public string SessionId { get; set; }
    public Dictionary<string, object> Input { get; set; }
    public object Result { get; set; }
    public WorkflowRunStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; }
    public Dictionary<string, object> Metadata { get; set; }

    public void Complete(object result);
    public void Fail(string errorMessage);
}
```

## 📈 Métricas

### `WorkflowMetrics`
```csharp
public class WorkflowMetrics
{
    public string WorkflowId { get; set; }
    public string SessionId { get; set; }
    public int TotalRuns { get; set; }
    public int SuccessfulRuns { get; set; }
    public int FailedRuns { get; set; }
    public TimeSpan? AverageExecutionTime { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public double SuccessRate { get; }
}
```

## 📚 Exemplos

### Workflow Básico
```csharp
var workflow = new SequentialWorkflow<Context, string>("Análise")
    .RegisterStep("Pesquisa", pesquisador,
        ctx => $"Pesquise sobre: {ctx.Topic}",
        (ctx, res) => ctx.Dados = res)
    .RegisterStep("Análise", analista,
        ctx => $"Analise: {ctx.Dados}",
        (ctx, res) => ctx.Resultado = res);

var resultado = await workflow.ExecuteAsync(contexto);
```

### Workflow Avançado
```csharp
var workflow = new AdvancedWorkflow<Context, Result>("Workflow")
    .WithUserId("user123")
    .WithDebugMode(true)
    .WithTelemetry(true)
    .ForTask(ctx => $"Objetivo: {ctx.Meta}")
    .RegisterStep("Step1", agent1,
        ctx => "Input 1",
        (ctx, res) => ctx.Output1 = res)
    .RegisterStep("Step2", agent2,
        ctx => $"Input 2: {ctx.Output1}",
        (ctx, res) => ctx.Output2 = res);

// Criar sessão
workflow.CreateNewSession("Sessão-001");

// Executar
var resultado = await workflow.ExecuteAsync(contexto);

// Obter métricas
var metrics = workflow.GetMetrics();
Console.WriteLine($"Taxa de sucesso: {metrics.SuccessRate:P2}");
```

### Workflow com Retry
```csharp
var workflow = new AdvancedWorkflow<APIContext, Response>("API")
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .RegisterStep("Chamada", apiClient,
        ctx => $"Call: {ctx.Endpoint}",
        (ctx, res) => ctx.Response = res,
        onError: async (ctx, ex) => await HandleError(ex));

try
{
    var response = await workflow.ExecuteAsync(context);
}
catch (WorkflowException ex)
{
    Console.WriteLine($"Falha após 3 tentativas: {ex.Message}");
}
```

### Workflow com Validação
```csharp
var workflow = new AdvancedWorkflow<DataContext, Report>("Validação")
    .RegisterStep("Coleta", coletor,
        ctx => "Coletar dados",
        (ctx, res) => 
        {
            if (!ValidateData(res))
                throw new ValidationException("Dados inválidos");
            ctx.Data = res;
        })
    .RegisterStep("Processamento", processador,
        ctx => $"Processar: {ctx.Data}",
        (ctx, res) => ctx.Result = res);

// Executar com tratamento de erro
try
{
    var result = await workflow.ExecuteAsync(context);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validação falhou: {ex.Message}");
}
```

## 🎯 Próximos Passos

1. **Explore os Exemplos**
   - [Exemplos de Workflow](../../examples.md#workflows)
   - [Sessões](../../examples.md#sessões)
   - [Métricas](../../examples.md#métricas)

2. **Aprofunde-se**
   - [Conceitos de Workflow](../../workflows.md)
   - [Melhores Práticas](../../best-practices.md)
   - [Guias Avançados](../../advanced.md)

---

## 📚 Recursos Relacionados

- [Agent](agent.md)
- [Tool](tool.md)
- [Model](model.md) 