# 🤖 Agent

> Base class for AI agents with modern memory architecture and advanced tools

## 📋 Table of Contents

- [Constructors](#constructors)
- [Properties](#properties)
- [Configuration Methods](#configuration-methods)
- [Execution Methods](#execution-methods)
- [Memory Configuration](#memory-configuration)
- [Examples](#examples)

## 🏗️ Constructors

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

Creates a new agent with specified model and configurations.

**Parameters:**
- `model`: AI model to be used (required)
- `name`: Agent name (optional, defaults to class name)
- `instructions`: Initial agent instructions (optional)
- `modelConfig`: Model configuration (optional, uses default if not provided)
- `logger`: Custom logger (optional, uses ConsoleLogger as default)
- `memoryManager`: Custom memory manager (optional)
- `storage`: Storage system (optional, only used if semantic memory is enabled)

**⚠️ Important Architecture Change:**
By default, agents now come with **semantic memory disabled** (low cost). Use fluent methods to enable memory features as needed.

**Example:**
```csharp
// Simple agent (low cost, no semantic processing)
var simpleAgent = new Agent<string, string>(model, "Basic Assistant");

// Agent with semantic memory (AI processing, extra costs)
var smartAgent = new Agent<string, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage);
```

## 📊 Properties

### `Name`
```csharp
public string Name { get; }
```
Agent name.

### `Context`
```csharp
public TContext Context { get; }
```
Current agent context.

### `description`
```csharp
public string description { get; }
```
Auto-generated description based on system prompt.

## 🔧 Configuration Methods

### Personality Configuration

#### `WithPersona`
```csharp
public Agent<TContext, TResult> WithPersona(string persona)
public Agent<TContext, TResult> WithPersona(Func<TContext, string> personaGenerator)
```

Defines the agent's personality/persona.

**Example:**
```csharp
agent.WithPersona("You are a financial data analysis expert with 10 years of experience.");

// With dynamic generator
agent.WithPersona(ctx => $"Assistant specialized in domain: {ctx.Domain}");
```

#### `WithInstructions`
```csharp
public Agent<TContext, TResult> WithInstructions(string instructions)
public Agent<TContext, TResult> WithInstructions(Func<TContext, string> instructionsGenerator)
```

Adds specific instructions to the agent.

### Tools Configuration

#### `WithTools`
```csharp
public Agent<TContext, TResult> WithTools(ToolPack toolPack)
public Agent<TContext, TResult> WithToolPacks(params ToolPack[] toolPacks)
public Agent<TContext, TResult> AddTool(Tool tool)
```

Adds tools to the agent.

**Example:**
```csharp
agent.WithTools(new SearchToolPack())
     .WithTools(new FinanceToolPack());
```

### Reasoning Configuration

#### `WithReasoning`
```csharp
public Agent<TContext, TResult> WithReasoning(bool reasoning = true)
```

Enables structured step-by-step reasoning.

#### `WithReasoningModel`
```csharp
public Agent<TContext, TResult> WithReasoningModel(IModel reasoningModel)
```

Sets specific model for reasoning (automatically enables reasoning).

#### `WithReasoningSteps`
```csharp
public Agent<TContext, TResult> WithReasoningSteps(int minSteps = 1, int maxSteps = 10)
```

Configures minimum and maximum reasoning steps.

**Example:**
```csharp
agent.WithReasoning(true)
     .WithReasoningSteps(minSteps: 3, maxSteps: 8);
```

## 🧠 Memory Configuration

### **New Architecture: Separation of History vs Semantic Memory**

The new architecture separates two types of memory to optimize costs:

1. **Message History** (low cost): Simple conversation logging
2. **Semantic Memory** (high cost): AI processing with semantic search

### `WithSemanticMemory` 🚨 **High Cost**
```csharp
public Agent<TContext, TResult> WithSemanticMemory(IStorage storage, IEmbeddingService embeddingService = null)
```

**⚠️ IMPORTANT**: Enables semantic memory with AI processing. **This significantly increases costs** as it processes each interaction with AI to extract and search relevant memories.

**When to use**: 
- Long-term personalized assistants
- Scenarios requiring intelligent context search
- Applications benefiting from semantic relationships

**Example:**
```csharp
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("semantic.db", embeddingService);

var smartAgent = new Agent<string, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage, embeddingService);
```

### `WithMessageHistory` 💰 **Low Cost**
```csharp
public Agent<TContext, TResult> WithMessageHistory(IMessageHistoryService historyService)
```

Configures simple message history without semantic processing.

**Example:**
```csharp
var historyService = new SqliteMessageHistoryService("conversations.db");

var agent = new Agent<string, string>(model, "Assistant")
    .WithMessageHistory(historyService);
```

### `WithFullMemory` 🚨 **High Cost**
```csharp
public Agent<TContext, TResult> WithFullMemory(IStorage storage, IMessageHistoryService historyService = null, IEmbeddingService embeddingService = null)
```

Enables both semantic memory and custom history.

### `WithAnonymousMode`
```csharp
public Agent<TContext, TResult> WithAnonymousMode(bool enableAnonymousMode = true)
```

Allows using the agent without providing explicit UserId/SessionId. IDs are auto-generated.

## 🚀 Execution Methods

### `ExecuteAsync`
```csharp
public virtual async Task<AgentResult<TResult>> ExecuteAsync(
    string prompt,
    TContext contextVar = default,
    List<AIMessage> messageHistory = null,
    CancellationToken cancellationToken = default)
```

Executes the agent with the provided prompt.

**Returns:**
- `AgentResult<TResult>`: Complete result with data, history, reasoning, etc.

### `ExecuteStreamingAsync`
```csharp
public async Task<AgentResult<TResult>> ExecuteStreamingAsync(
    string prompt,
    Action<string> handler,
    TContext contextVar = default,
    List<AIMessage> messageHistory = null,
    CancellationToken cancellationToken = default)
```

Executes the agent with real-time response streaming.

## 📚 Examples

### Simple Agent (Low Cost)
```csharp
var simpleAgent = new Agent<string, string>(model, "Assistant")
    .WithPersona("Helpful and concise assistant")
    .WithAnonymousMode(true); // No need to manage IDs

var result = await simpleAgent.ExecuteAsync("How can I help you?");
```

### Agent with Reasoning
```csharp
var reasoningAgent = new Agent<string, string>(model, "Analyst")
    .WithPersona("Experienced analyst who thinks step-by-step")
    .WithReasoning(true)
    .WithReasoningSteps(minSteps: 2, maxSteps: 6);

var result = await reasoningAgent.ExecuteAsync("Analyze this complex problem...");
```

### Agent with Semantic Memory (High Cost)
```csharp
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("smart_memory.db", embeddingService);

var smartAgent = new Agent<Context, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Assistant with advanced semantic memory")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });

// First conversation - establishes preferences
await smartAgent.ExecuteAsync("I like strong coffee in the morning");

// Second conversation - automatic semantic search
var result = await smartAgent.ExecuteAsync("What drink do you recommend now?");
// System automatically finds the "strong coffee" preference
```

### Agent with Structured Output
```csharp
public class AnalysisResult
{
    public string Summary { get; set; }
    public List<string> KeyPoints { get; set; }
    public double Confidence { get; set; }
}

// Structured output is automatically configured based on TResult type
var structuredAgent = new Agent<string, AnalysisResult>(model, "Analyzer")
    .WithPersona("Document analysis specialist");

var analysis = await structuredAgent.ExecuteAsync("Analyze this document...");
Console.WriteLine($"Summary: {analysis.Data.Summary}");
```

### Cost Comparison

```csharp
// ✅ LOW COST: Simple agent
var cheapAgent = new Agent<string, string>(model, "Basic");

// 🚨 HIGH COST: Agent with semantic memory
var expensiveAgent = new Agent<Context, string>(model, "Smart")
    .WithSemanticMemory(storage, embeddingService);

// 💡 MEDIUM COST: Agent with reasoning
var reasoningAgent = new Agent<string, string>(model, "Analyzer")
    .WithReasoning(true);
```

### Legacy Code Migration

```csharp
// ❌ OLD: Memory always active (high cost)
var oldAgent = new Agent<Context, string>(model, storage: storage);

// ✅ NEW: Opt-in memory (cost control)
var newSimpleAgent = new Agent<Context, string>(model); // No storage
var newSmartAgent = new Agent<Context, string>(model)
    .WithSemanticMemory(storage); // Explicitly enabled
```

## 🎯 Next Steps

1. **Understand the New Architecture**
   - [Memory Architecture](../../memory-architecture.md)
   - [Cost Optimization](../../cost-optimization.md)

2. **Explore Examples**
   - [Basic Examples](../../examples/basic-examples.md)
   - [Semantic Memory](../../examples/semantic-memory.md)

3. **Go Deeper**
   - [Best Practices](../../best-practices.md)
   - [Advanced Guides](../../advanced-guides.md)

---

## 📚 Related Resources

- [Workflow](workflow.md)
- [Tool](tool.md)
- [Model](model.md)
- [Memory Services](../memory/memory-services.md)