# 📚 API Reference

> Documentação completa da API do AgentSharp

## 🧩 Namespaces

### AgentSharp.Core
- [Agent](core/agent.md)
- [Workflow](core/workflow.md)
- [Tool](core/tool.md)
- [Model](core/model.md)

### AgentSharp.Models
- [ModelResponse](models/model-response.md)
- [ModelRequest](models/model-request.md)
- [AIMessage](models/ai-message.md)

### AgentSharp.Tools
- [ToolPack](tools/tool-pack.md)
- [SearchTools](tools/search-tools.md)
- [ReasoningTools](tools/reasoning-tools.md)

### AgentSharp.Utils
- [Logger](utils/logger.md)
- [StateManager](utils/state-manager.md)
- [Metrics](utils/metrics.md)

## 🔍 Índice Rápido

### Agentes
```csharp
// Criar agente básico
var agent = new Agent<TContext, TResult>(model, "Nome")
    .WithPersona("Descrição")
    .WithReasoning(true);

// Executar
var result = await agent.ExecuteAsync(input);
```

### Workflows
```csharp
// Criar workflow
var workflow = new SequentialWorkflow<TContext, TResult>("Nome")
    .RegisterStep("Passo", agent,
        ctx => "Input",
        (ctx, res) => ctx.Output = res);

// Executar
var result = await workflow.ExecuteAsync(context);
```

### Tools
```csharp
// Criar tool
public class MyTool : ITool
{
    public string Name => "nome";
    public string Description => "descrição";
    
    public async Task<string> ExecuteAsync(string input)
    {
        // Implementação
    }
}
```

### Models
```csharp
// Configurar modelo
var options = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = "chave",
    Endpoint = "url"
};

var model = new ModelFactory().CreateModel("openai", options);
```

## 🎯 Guias Rápidos

### 1. Criar Agente
- [Configuração Básica](guides/agent-basic.md)
- [Raciocínio](guides/agent-reasoning.md)
- [Tools](guides/agent-tools.md)

### 2. Criar Workflow
- [Workflow Básico](guides/workflow-basic.md)
- [Workflow Avançado](guides/workflow-advanced.md)
- [Sessões](guides/workflow-sessions.md)

### 3. Criar Tools
- [Tool Básica](guides/tool-basic.md)
- [Tool Pack](guides/tool-pack.md)
- [Integração](guides/tool-integration.md)

## 📊 Exemplos

### Agentes
```csharp
// Agente com personalidade
var reporter = new Agent<string, string>(model, "Reporter")
    .WithPersona("Você é um jornalista investigativo")
    .WithTools(new SearchToolPack())
    .WithReasoning(true);

// Agente com output estruturado
var analyzer = new Agent<Document, Analysis>(model, "Analyzer")
    .WithStructuredOutput<Analysis>()
    .WithReasoning(true);
```

### Workflows
```csharp
// Workflow avançado
var workflow = new AdvancedWorkflow<Context, Result>("Nome")
    .WithDebugMode(true)
    .WithTelemetry(true)
    .WithUserId("user123")
    .RegisterStep("Passo1", agent1, ...)
    .RegisterStep("Passo2", agent2, ...);

// Workflow com retry
var resilient = new AdvancedWorkflow<Context, Result>("Resiliente")
    .WithRetry(maxAttempts: 3)
    .RegisterStep("API", apiClient, ...);
```

### Tools
```csharp
// Tool pack customizado
public class MyToolPack : ToolPack
{
    public override IEnumerable<ITool> GetTools()
    {
        yield return new SearchTool();
        yield return new AnalysisTool();
        yield return new ValidationTool();
    }
}

// Uso
agent.WithTools(new MyToolPack());
```

## 🔧 Configurações

### Model
```csharp
var options = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = "chave",
    Endpoint = "url",
    DefaultConfiguration = new ModelConfiguration
    {
        Temperature = 0.7,
        MaxTokens = 2048
    }
};
```

### Logging
```csharp
var logger = new ConsoleLogger
{
    MinLevel = LogLevel.Debug,
    Format = "{Timestamp} [{Level}] {Message}"
};

agent.WithLogger(logger);
```

### Métricas
```csharp
workflow.OnStepComplete += (step, metrics) =>
{
    Console.WriteLine($"Passo: {step.Name}");
    Console.WriteLine($"Duração: {metrics.Duration}");
    Console.WriteLine($"Memória: {metrics.MemoryUsage}MB");
};
```

## 🎯 Próximos Passos

1. **Explore os Exemplos**
   - [Exemplos Básicos](../examples.md)
   - [Workflows](../workflows.md)
   - [Raciocínio](../reasoning.md)

2. **Aprofunde-se**
   - [Conceitos Fundamentais](../core-concepts.md)
   - [Melhores Práticas](../best-practices.md)
   - [Guias Avançados](../advanced.md)

---

## 📚 Recursos Relacionados

- [Guia de Início](../getting-started.md)
- [Exemplos](../examples.md)
- [Conceitos](../core-concepts.md) 