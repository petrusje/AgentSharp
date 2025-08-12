# 🤖 Agent

> Classe base para agentes de IA com raciocínio e ferramentas

## 📋 Sumário

- [Construtores](#construtores)
- [Propriedades](#propriedades)
- [Métodos](#métodos)
- [Eventos](#eventos)
- [Exemplos](#exemplos)

## 🏗️ Construtores

### `Agent<TContext, TResult>`

```csharp
public Agent(IModel model, string name)
```

Cria um novo agente com modelo e nome especificados.

**Parâmetros:**
- `model`: Modelo de IA a ser usado
- `name`: Nome do agente

**Exemplo:**
```csharp
var agent = new Agent<string, string>(model, "Assistente");
```

## 📊 Propriedades

### `Name`
```csharp
public string Name { get; }
```
Nome do agente.

### `Persona`
```csharp
public string Persona { get; }
```
Personalidade/instruções do agente.

### `UsesReasoning`
```csharp
public bool UsesReasoning { get; }
```
Indica se o agente usa raciocínio estruturado.

### `Tools`
```csharp
public List<ITool> Tools { get; }
```
Lista de ferramentas disponíveis.

### `Logger`
```csharp
public ILogger Logger { get; }
```
Logger configurado.

## 🔧 Métodos

### `ExecuteAsync`
```csharp
public async Task<TResult> ExecuteAsync(
    TContext context,
    CancellationToken cancellationToken = default)
```

Executa o agente com o contexto fornecido.

**Parâmetros:**
- `context`: Contexto de execução
- `cancellationToken`: Token de cancelamento (opcional)

**Retorno:**
- `Task<TResult>`: Resultado da execução

**Exemplo:**
```csharp
var result = await agent.ExecuteAsync("Analise este texto");
```

### `WithPersona`
```csharp
public Agent<TContext, TResult> WithPersona(string persona)
```

Define a personalidade do agente.

**Parâmetros:**
- `persona`: Descrição da personalidade

**Retorno:**
- `Agent<TContext, TResult>`: O próprio agente (fluent interface)

**Exemplo:**
```csharp
agent.WithPersona("Você é um especialista em análise de dados");
```

### `WithReasoning`
```csharp
public Agent<TContext, TResult> WithReasoning(bool enabled = true)
```

Habilita/desabilita raciocínio estruturado.

**Parâmetros:**
- `enabled`: Se deve usar raciocínio

**Retorno:**
- `Agent<TContext, TResult>`: O próprio agente

**Exemplo:**
```csharp
agent.WithReasoning(true);
```

### `WithTools`
```csharp
public Agent<TContext, TResult> WithTools(IToolPack toolPack)
```

Adiciona um pacote de ferramentas.

**Parâmetros:**
- `toolPack`: Pacote de ferramentas

**Retorno:**
- `Agent<TContext, TResult>`: O próprio agente

**Exemplo:**
```csharp
agent.WithTools(new SearchToolPack());
```

### `WithLogger`
```csharp
public Agent<TContext, TResult> WithLogger(ILogger logger)
```

Define o logger.

**Parâmetros:**
- `logger`: Logger a ser usado

**Retorno:**
- `Agent<TContext, TResult>`: O próprio agente

**Exemplo:**
```csharp
agent.WithLogger(new ConsoleLogger());
```

### `WithDebugMode`
```csharp
public Agent<TContext, TResult> WithDebugMode(bool enabled = true)
```

Habilita/desabilita modo debug.

**Parâmetros:**
- `enabled`: Se deve usar modo debug

**Retorno:**
- `Agent<TContext, TResult>`: O próprio agente

**Exemplo:**
```csharp
agent.WithDebugMode(true);
```

## 📡 Eventos

### `OnExecutionStart`
```csharp
public event EventHandler<ExecutionContext> OnExecutionStart;
```

Disparado quando uma execução inicia.

### `OnExecutionComplete`
```csharp
public event EventHandler<ExecutionResult<TResult>> OnExecutionComplete;
```

Disparado quando uma execução completa.

### `OnError`
```csharp
public event EventHandler<Exception> OnError;
```

Disparado quando ocorre um erro.

## 📚 Exemplos

### Agente Básico
```csharp
var agent = new Agent<string, string>(model, "Assistente")
    .WithPersona("Você é um assistente prestativo")
    .WithReasoning(true);

var result = await agent.ExecuteAsync("Como posso ajudar?");
```

### Agente com Tools
```csharp
var agent = new Agent<string, string>(model, "Pesquisador")
    .WithTools(new SearchToolPack())
    .WithReasoning(true)
    .WithDebugMode(true);

var result = await agent.ExecuteAsync("Pesquise sobre IA");
```

### Agente com Output Estruturado
```csharp
public class Analysis
{
    public string Title { get; set; }
    public List<string> Points { get; set; }
    public double Score { get; set; }
}

var agent = new Agent<string, Analysis>(model, "Analisador")
    .WithStructuredOutput<Analysis>()
    .WithReasoning(true);

var analysis = await agent.ExecuteAsync("Analise este documento");
```

### Agente com Eventos
```csharp
var agent = new Agent<string, string>(model, "Monitor")
    .WithReasoning(true);

agent.OnExecutionStart += (s, ctx) =>
    Console.WriteLine($"Iniciando: {ctx.ExecutionId}");

agent.OnExecutionComplete += (s, res) =>
    Console.WriteLine($"Completado: {res.Duration}");

agent.OnError += (s, ex) =>
    Console.WriteLine($"Erro: {ex.Message}");

var result = await agent.ExecuteAsync("Teste");
```

### Agente com Retry
```csharp
var agent = new Agent<APIContext, Response>(model, "API")
    .WithRetry(maxAttempts: 3)
    .WithReasoning(true);

try
{
    var response = await agent.ExecuteAsync(context);
}
catch (AgentException ex)
{
    Console.WriteLine($"Falha após 3 tentativas: {ex.Message}");
}
```

## 🎯 Próximos Passos

1. **Explore os Exemplos**
   - [Exemplos Básicos](../../examples.md)
   - [Raciocínio](../../reasoning.md)
   - [Tools](../../tools.md)

2. **Aprofunde-se**
   - [Conceitos Fundamentais](../../core-concepts.md)
   - [Melhores Práticas](../../best-practices.md)
   - [Guias Avançados](../../advanced.md)

---

## 📚 Recursos Relacionados

- [Workflow](workflow.md)
- [Tool](tool.md)
- [Model](model.md) 