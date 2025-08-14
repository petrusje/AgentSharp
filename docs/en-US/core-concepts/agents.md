# Agents

The `Agent<TContext, TResult>` class is the core component of AgentSharp, representing an intelligent AI agent with customizable behavior, memory, tools, and structured output capabilities.

## Agent Architecture

### Generic Type Parameters

```csharp
Agent<TContext, TResult>
```

- **`TContext`**: Custom context object containing domain-specific data
- **`TResult`**: Expected response type with automatic structured output extraction

### Core Components

Every agent contains these internal components:

- **PromptManager**: Manages system prompts, persona, and instructions
- **ToolManager**: Handles tool registration and execution
- **ExecutionEngine**: Core request/response processing
- **MemoryManager**: Advanced memory operations (optional)

## Creating Agents

### Basic Agent Creation

```csharp
var agent = new Agent<MyContext, string>(model, "AgentName")
    .WithPersona("You are a helpful assistant")
    .WithInstructions("Always provide clear, concise answers");
```

### With Custom Types

```csharp
public class AnalysisContext
{
    public string CompanyName { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal[] Data { get; set; }
}

public class AnalysisResult
{
    public string Summary { get; set; }
    public decimal TotalValue { get; set; }
    public string Recommendation { get; set; }
    public int ConfidenceScore { get; set; }
}

var analyst = new Agent<AnalysisContext, AnalysisResult>(model, "FinancialAnalyst")
    .WithPersona("You are an experienced financial analyst")
    .WithInstructions(ctx => $"Analyze {ctx.CompanyName} data for {ctx.ReportDate:yyyy-MM-dd}");
```

## Fluent Configuration Methods

### Persona and Instructions

```csharp
// Static persona
.WithPersona("You are a customer service representative")

// Dynamic persona based on context
.WithPersona(ctx => $"You are a {ctx.Role} specialist for {ctx.Department}")

// Static instructions
.WithInstructions("Always be helpful and professional")

// Dynamic instructions
.WithInstructions(ctx => $"Focus on {ctx.Priority} priority issues for {ctx.CustomerType} customers")
```

### Guard Rails

```csharp
// Set behavioral constraints
.WithGuardRails("Never share sensitive information. Always ask for verification before account changes.")

// Dynamic guard rails
.WithGuardRails(ctx => ctx.IsHighRisk ? "Require manager approval for all actions" : "Standard protocols apply")
```

### Temperature Control

```csharp
// Creative responses (0.0 - 2.0)
.WithTemperature(0.8)

// Deterministic responses
.WithTemperature(0.0)

// Context-based temperature
.WithTemperature(ctx => ctx.RequiresCreativity ? 0.9 : 0.1)
```

## Memory Configuration

### Basic Memory Setup

```csharp
.WithSemanticMemory(storage, embeddingService)
```

### Granular Memory Controls

```csharp
// Enable AI-powered memory extraction
.WithUserMemories(enable: true)

// Enable semantic memory search tools  
.WithMemorySearch(enable: true)

// Include conversation history in context
.WithHistoryToMessages(enable: true)

// Enable knowledge base search (future feature)
.WithKnowledgeSearch(enable: true)
```

### Memory Configuration

```csharp
var memoryConfig = new MemoryConfiguration
{
    MaxMemoriesPerInteraction = 5,
    MinImportanceThreshold = 0.7,
    MaxMemoriesPerUser = 1000,
    EnableAutoSummary = true
};

.WithMemoryConfiguration(memoryConfig)
```

## Tool Integration

### Tool Registration

```csharp
// Register individual tool pack
.RegisterToolPack(new FinanceToolPack())

// Register multiple tool packs
.RegisterToolPack(new SearchToolPack())
.RegisterToolPack(new CalculatorToolPack())
```

### Built-in Tool Packs

```csharp
// Memory management tools
.RegisterToolPack(new SmartMemoryToolPack())

// Step-by-step reasoning
.RegisterToolPack(new ReasoningToolPack())

// Team coordination
.RegisterToolPack(new TeamHandoffToolPack())
```

## Advanced Features

### Structured Output

AgentSharp automatically detects complex return types and enables structured output:

```csharp
public class ProductRecommendation
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Reasoning { get; set; }
    public int ConfidenceScore { get; set; }
}

// Automatic structured output - no configuration needed
var agent = new Agent<CustomerProfile, ProductRecommendation>(model, "ProductAdvisor");
```

### Reasoning Mode

```csharp
.WithReasoning(enable: true)
.WithReasoningModel(specializedModel)  // Optional different model for reasoning
.WithReasoningSteps(minSteps: 3, maxSteps: 10)
```

### Anonymous Mode

```csharp
// Auto-generates user and session IDs
.WithAnonymousMode(enable: true)
```

### Context Format

```csharp
// How context is provided to the model
.WithContextFormat(ContextFormat.SystemMessage)  // Default
.WithContextFormat(ContextFormat.FunctionCall)   // Via function calling
.WithContextFormat(ContextFormat.RequestField)   // In request structure
```

## Execution Methods

### Basic Execution

```csharp
var result = await agent.ExecuteAsync("User message", context);
Console.WriteLine(result.Content);  // Structured output available
```

### Streaming Execution

```csharp
await agent.ExecuteStreamingAsync(
    "User message", 
    context,
    chunk => Console.Write(chunk)  // Real-time output handler
);
```

### With Configuration Override

```csharp
var config = new ModelConfiguration
{
    Temperature = 0.0,
    MaxTokens = 1000
};

var result = await agent.ExecuteAsync("User message", context, config);
```

## Agent Properties

### Context Information

```csharp
// Access current context information
string userId = agent.UserId;
string sessionId = agent.SessionId;
```

### Memory Access

```csharp
// Check if memory is enabled
bool hasMemory = agent.HasMemoryManager;

// Access memory manager directly
if (agent.MemoryManager != null)
{
    var memories = await agent.MemoryManager.GetExistingMemoriesAsync(context);
}
```

### Tool Information

```csharp
// Get registered tools
var toolCount = agent.ToolManager.RegisteredTools.Count;
```

## Best Practices

### 1. Type Safety

Always use strongly-typed contexts and results:

```csharp
// Good
Agent<OrderContext, OrderAnalysis>

// Avoid
Agent<object, object>
```

### 2. Context Design

Design contexts to include all relevant information:

```csharp
public class CustomerServiceContext
{
    public string CustomerId { get; set; }
    public string CustomerTier { get; set; }
    public DateTime LastContact { get; set; }
    public List<string> PreviousIssues { get; set; }
    public bool IsVIP { get; set; }
}
```

### 3. Progressive Enhancement

Start simple and add features as needed:

```csharp
// Start basic
var agent = new Agent<Context, Result>(model, "Agent")
    .WithPersona("You are helpful");

// Add memory when needed
agent.WithSemanticMemory(storage, embeddings)
     .WithUserMemories(true);

// Add tools when needed
agent.RegisterToolPack(new CustomToolPack());
```

### 4. Error Handling

```csharp
try
{
    var result = await agent.ExecuteAsync(prompt, context);
    return result.Content;
}
catch (ModelException ex)
{
    // Handle model-specific errors
}
catch (MemoryException ex)
{
    // Handle memory-related errors
}
```

## Performance Considerations

### Memory Usage

- Enable memory features only when needed
- Use appropriate memory limits
- Consider storage type (in-memory vs persistent)

### Token Optimization

- Use temperature controls wisely
- Set appropriate max token limits
- Monitor token usage with telemetry

### Caching

- AgentSharp automatically caches tool definitions
- Memory embeddings are cached per session
- Consider external caching for expensive operations

## Next Steps

- [Memory Management](memory.md) - Deep dive into memory features
- [Tools & Functions](tools.md) - Building custom tool packs
- [Team Orchestration](orchestration.md) - Multi-agent workflows
- [API Reference](../api-reference/agent.md) - Complete API documentation