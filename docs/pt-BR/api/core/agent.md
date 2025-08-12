# 🤖 Agent

> Classe base para agentes de IA com arquitetura de memória moderna e ferramentas avançadas

## 📋 Sumário

- [Construtores](#construtores)
- [Propriedades](#propriedades)
- [Métodos de Configuração](#métodos-de-configuração)
- [Métodos de Execução](#métodos-de-execução)
- [Configuração de Memória](#configuração-de-memória)
- [Exemplos](#exemplos)

## 🏗️ Construtores

### `Agent<TContext, TResult>`

```csharp
public Agent(
    IModel model,
    string name = null,
    string instructions = null,
    ModelConfiguration modelConfig = null,
    ILogger logger = null,
    IMemoryManager memoryManager = null,
    IStorage storage = null)
```

Cria um novo agente com modelo e configurações especificados.

**Parâmetros:**
- `model`: Modelo de IA a ser usado (obrigatório)
- `name`: Nome do agente (opcional, usa nome da classe como padrão)
- `instructions`: Instruções iniciais do agente (opcional)
- `modelConfig`: Configuração do modelo (opcional, usa padrão se não fornecida)
- `logger`: Logger personalizado (opcional, usa ConsoleLogger como padrão)
- `memoryManager`: Gerenciador de memória customizado (opcional)
- `storage`: Sistema de armazenamento (opcional, usado apenas se memória semântica for habilitada)

**⚠️ Mudança Importante na Arquitetura:**
Por padrão, o agente agora vem com **memória semântica desabilitada** (baixo custo). Use métodos fluentes para habilitar recursos de memória conforme necessário.

**Exemplo:**
```csharp
// Agente simples (baixo custo, sem processamento semântico)
var simpleAgent = new Agent<string, string>(model, "Assistente Básico");

// Agente com memória semântica (processamento AI, custos extras)
var smartAgent = new Agent<string, string>(model, "Assistente Inteligente")
    .WithSemanticMemory(storage);
```

## 📊 Propriedades

### `Name`
```csharp
public string Name { get; }
```
Nome do agente.

### `Context`
```csharp
public TContext Context { get; }
```
Contexto atual do agente.

### `description`
```csharp
public string description { get; }
```
Descrição gerada automaticamente baseada no prompt do sistema.

## 🔧 Métodos de Configuração

### Configuração de Personalidade

#### `WithPersona`
```csharp
public Agent<TContext, TResult> WithPersona(string persona)
public Agent<TContext, TResult> WithPersona(Func<TContext, string> personaGenerator)
```

Define a personalidade/persona do agente.

**Exemplo:**
```csharp
agent.WithPersona("Você é um especialista em análise de dados financeiros com 10 anos de experiência.");

// Com gerador dinâmico
agent.WithPersona(ctx => $"Assistente especializado no domínio: {ctx.Domain}");
```

#### `WithInstructions`
```csharp
public Agent<TContext, TResult> WithInstructions(string instructions)
public Agent<TContext, TResult> WithInstructions(Func<TContext, string> instructionsGenerator)
```

Adiciona instruções específicas ao agente.

### Configuração de Ferramentas

#### `WithTools`
```csharp
public Agent<TContext, TResult> WithTools(ToolPack toolPack)
public Agent<TContext, TResult> WithToolPacks(params ToolPack[] toolPacks)
public Agent<TContext, TResult> AddTool(Tool tool)
```

Adiciona ferramentas ao agente.

**Exemplo:**
```csharp
agent.WithTools(new SearchToolPack())
     .WithTools(new FinanceToolPack());
```

### Configuração de Raciocínio

#### `WithReasoning`
```csharp
public Agent<TContext, TResult> WithReasoning(bool reasoning = true)
```

Habilita raciocínio estruturado step-by-step.

#### `WithReasoningModel`
```csharp
public Agent<TContext, TResult> WithReasoningModel(IModel reasoningModel)
```

Define modelo específico para reasoning (habilita reasoning automaticamente).

#### `WithReasoningSteps`
```csharp
public Agent<TContext, TResult> WithReasoningSteps(int minSteps = 1, int maxSteps = 10)
```

Configura número mínimo e máximo de passos de reasoning.

**Exemplo:**
```csharp
agent.WithReasoning(true)
     .WithReasoningSteps(minSteps: 3, maxSteps: 8);
```

## 🧠 Configuração de Memória

### **Arquitetura Nova: Separação de História vs Memória Semântica**

A nova arquitetura separa dois tipos de memória para otimizar custos:

1. **Message History** (baixo custo): Log simples de conversas
2. **Semantic Memory** (alto custo): Processamento AI com busca semântica

### `WithSemanticMemory` 🚨 **Alto Custo**
```csharp
public Agent<TContext, TResult> WithSemanticMemory(IStorage storage, IEmbeddingService embeddingService = null)
```

**⚠️ IMPORTANTE**: Habilita memória semântica com processamento AI. **Isso aumenta significativamente os custos** pois processa cada interação com IA para extrair e buscar memórias relevantes.

**Quando usar**: 
- Assistentes personalizados de longo prazo
- Cenários que requerem busca inteligente de contexto
- Aplicações que se beneficiam de relacionamento semântico

**Exemplo:**
```csharp
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("semantic.db", embeddingService);

var smartAgent = new Agent<string, string>(model, "Assistente Inteligente")
    .WithSemanticMemory(storage, embeddingService);
```

### `WithMessageHistory` 💰 **Baixo Custo**
```csharp
public Agent<TContext, TResult> WithMessageHistory(IMessageHistoryService historyService)
```

Configura apenas histórico simples de mensagens sem processamento semântico.

**Exemplo:**
```csharp
var historyService = new SqliteMessageHistoryService("conversations.db");

var agent = new Agent<string, string>(model, "Assistente")
    .WithMessageHistory(historyService);
```

### `WithFullMemory` 🚨 **Alto Custo**
```csharp
public Agent<TContext, TResult> WithFullMemory(IStorage storage, IMessageHistoryService historyService = null, IEmbeddingService embeddingService = null)
```

Habilita tanto memória semântica quanto histórico customizado.

### `WithAnonymousMode`
```csharp
public Agent<TContext, TResult> WithAnonymousMode(bool enableAnonymousMode = true)
```

Permite usar o agente sem fornecer UserId/SessionId explícitos. IDs são gerados automaticamente.

## 🚀 Métodos de Execução

### `ExecuteAsync`
```csharp
public virtual async Task<AgentResult<TResult>> ExecuteAsync(
    string prompt,
    TContext contextVar = default,
    List<AIMessage> messageHistory = null,
    CancellationToken cancellationToken = default)
```

Executa o agente com o prompt fornecido.

**Retorna:**
- `AgentResult<TResult>`: Resultado completo com dados, histórico, reasoning, etc.

### `ExecuteStreamingAsync`
```csharp
public async Task<AgentResult<TResult>> ExecuteStreamingAsync(
    string prompt,
    Action<string> handler,
    TContext contextVar = default,
    List<AIMessage> messageHistory = null,
    CancellationToken cancellationToken = default)
```

Executa o agente com streaming de resposta em tempo real.

## 📚 Exemplos

### Agente Simples (Baixo Custo)
```csharp
var simpleAgent = new Agent<string, string>(model, "Assistente")
    .WithPersona("Assistente prestativo e conciso")
    .WithAnonymousMode(true); // Não precisa gerenciar IDs

var result = await simpleAgent.ExecuteAsync("Como posso ajudar?");
```

### Agente com Raciocínio
```csharp
var reasoningAgent = new Agent<string, string>(model, "Analista")
    .WithPersona("Analista experiente que pensa step-by-step")
    .WithReasoning(true)
    .WithReasoningSteps(minSteps: 2, maxSteps: 6);

var result = await reasoningAgent.ExecuteAsync("Analise este problema complexo...");
```

### Agente com Memória Semântica (Alto Custo)
```csharp
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("smart_memory.db", embeddingService);

var smartAgent = new Agent<Context, string>(model, "Assistente Inteligente")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Assistente com memória semântica avançada")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });

// Primeira conversa - estabelece preferências
await smartAgent.ExecuteAsync("Gosto de café forte pela manhã");

// Segunda conversa - busca semântica automática
var result = await smartAgent.ExecuteAsync("Que bebida você recomenda agora?");
// Sistema encontra automaticamente a preferência por "café forte"
```

### Agente com Output Estruturado
```csharp
public class AnalysisResult
{
    public string Summary { get; set; }
    public List<string> KeyPoints { get; set; }
    public double Confidence { get; set; }
}

// Structured output é configurado automaticamente baseado no tipo TResult
var structuredAgent = new Agent<string, AnalysisResult>(model, "Analisador")
    .WithPersona("Especialista em análise estruturada de documentos");

var analysis = await structuredAgent.ExecuteAsync("Analise este documento...");
Console.WriteLine($"Resumo: {analysis.Data.Summary}");
```

### Comparação de Custos

```csharp
// ✅ BAIXO CUSTO: Agente simples
var cheapAgent = new Agent<string, string>(model, "Basic");

// 🚨 ALTO CUSTO: Agente com memória semântica
var expensiveAgent = new Agent<Context, string>(model, "Smart")
    .WithSemanticMemory(storage, embeddingService);

// 💡 MÉDIO CUSTO: Agente com raciocínio
var reasoningAgent = new Agent<string, string>(model, "Analyzer")
    .WithReasoning(true);
```

### Migração do Código Legado

```csharp
// ❌ ANTIGO: Memória sempre ativa (alto custo)
var oldAgent = new Agent<Context, string>(model, storage: storage);

// ✅ NOVO: Memória opt-in (controle de custos)
var newSimpleAgent = new Agent<Context, string>(model); // Sem storage
var newSmartAgent = new Agent<Context, string>(model)
    .WithSemanticMemory(storage); // Explicitamente habilitado
```

## 🎯 Próximos Passos

1. **Entenda a Nova Arquitetura**
   - [Arquitetura de Memória](../../memory-architecture.md)
   - [Comparação de Custos](../../cost-optimization.md)

2. **Explore os Exemplos**
   - [Exemplos Básicos](../../examples/basic-examples.md)
   - [Memória Semântica](../../examples/semantic-memory.md)

3. **Aprofunde-se**
   - [Melhores Práticas](../../best-practices.md)
   - [Guias Avançados](../../advanced-guides.md)

---

## 📚 Recursos Relacionados

- [Workflow](workflow.md)
- [Tool](tool.md)
- [Model](model.md)
- [Memory Services](../memory/memory-services.md)