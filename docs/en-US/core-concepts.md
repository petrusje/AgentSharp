# 📚 Core Concepts

> Understand the fundamental concepts of AgentSharp

## 📋 Table of Contents

- [Agents](#agents)
- [Memory Architecture](#memory-architecture)
- [Workflows](#workflows)
- [Tools](#tools)
- [Models](#models)
- [Observability](#observability)

## 🤖 Agents

### What is an Agent?

An agent is an autonomous processing unit that:
- Has a defined personality
- Has a specific objective
- Can use tools
- Maintains conversation context

```csharp
// ✅ Simple agent (low cost) - NEW ARCHITECTURE
var agent = new Agent<string, string>(model, "Assistant")
    .WithPersona("You are a helpful assistant")
    .WithAnonymousMode(true);

// 🧠 Specialized agent with semantic memory (high cost)
var analyst = new Agent<FinancialData, Analysis>(model, "Analyst")
    .WithPersona("You are a financial analyst with contextual memory")
    .WithSemanticMemory(storage, embeddingService) // Explicit opt-in
    .WithTools(new FinanceToolPack())
    .WithReasoning(true);
```

### Agent Types

1. **Simple Agents** (Low Cost - Recommended Default)
   - Direct processing
   - No semantic memory
   - Basic message history
   - Cost: ~$0.005 per interaction

2. **Reasoning Agents** (Medium Cost)
   - Step-by-step analysis
   - Problem decomposition
   - Result validation
   - Cost: +50% over simple agents

3. **Smart Agents** (High Cost - Use When ROI Justifies)
   - Semantic memory with AI processing
   - Intelligent context search
   - Advanced memory tools
   - Cost: 5x more than simple agents

4. **Specialized Agents**
   - Specific domain knowledge
   - Custom tools
   - Expert personas

## 🧠 Memory Architecture

### New Cost-Optimized Approach

AgentSharp separates memory into two types:

| Type | Cost | Purpose | When to Use |
|------|------|---------|-------------|
| **Message History** | 💰 Low | Simple conversation log | 80% of use cases |
| **Semantic Memory** | 🚨 High | AI-powered contextual search | When ROI justifies |

```csharp
// Default: Simple agent with basic history (low cost)
var simpleAgent = new Agent<string, string>(model, "Assistant");

// Explicit: Smart agent with semantic memory (high cost)
var smartAgent = new Agent<Context, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage, embeddingService);
```

### Cost Impact Examples

**100 interactions per day:**
- Simple Agent: ~$15/month
- Smart Agent: ~$84/month
- Difference: 460% more expensive

**When to use Semantic Memory:**
- ✅ Personal assistants that evolve over time
- ✅ Complex context that spans sessions
- ✅ ROI clearly justifies 5x cost increase
- ❌ Simple chatbots or FAQ systems
- ❌ Stateless interactions

## 🔄 Workflows

### What are Workflows?

Workflows orchestrate multiple agents to complete complex tasks:

```csharp
// Define context
public class AnalysisContext
{
    public string UserId { get; set; } = "user123";
    public string SessionId { get; set; } = "session456";
    public string Input { get; set; }
    public string Analysis { get; set; }
    public string Result { get; set; }
}

// Create workflow with simple agents (cost-efficient)
var workflow = new SequentialWorkflow<AnalysisContext, string>("Analysis")
    .RegisterStep("Data Analysis", analyzer,
        ctx => $"Analyze: {ctx.Input}",
        (ctx, res) => ctx.Analysis = res)
    .RegisterStep("Report Generation", reporter,
        ctx => $"Generate report from: {ctx.Analysis}",
        (ctx, res) => ctx.Result = res);
```

### Workflow Types

1. **Sequential Workflows**
   - Step-by-step execution
   - Each step depends on previous
   - Linear processing

2. **Parallel Workflows**  
   - Concurrent execution
   - Independent steps
   - Faster processing

3. **Conditional Workflows**
   - Dynamic routing
   - Decision-based execution
   - Complex logic support

## 🛠️ Tools

### Tool System

Tools extend agent capabilities:

```csharp
// Simple tool
public class CalculatorTool : ITool
{
    public string Name => "calculator";
    public string Description => "Performs mathematical calculations";
    
    public async Task<string> ExecuteAsync(string input)
    {
        // Tool implementation
        return PerformCalculation(input);
    }
}

// Using tools with agents
var agent = new Agent<string, string>(model, "Calculator Assistant")
    .WithTools(new MathToolPack())
    .WithPersona("You are a math expert with calculation capabilities");
```

### Tool Categories

1. **Basic Tools**
   - Mathematical operations
   - String manipulation  
   - Data formatting

2. **External API Tools**
   - Web searches
   - Database queries
   - Service integrations

3. **Smart Memory Tools** (High Cost)
   - Semantic memory management
   - Intelligent search
   - Context enhancement

## 🔧 Models

### Model Configuration

```csharp
// Basic model setup
var modelOptions = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    DefaultConfiguration = new ModelConfiguration
    {
        Temperature = 0.7,
        MaxTokens = 2048
    }
};

var model = new ModelFactory().CreateModel("openai", modelOptions);
```

### Model Types Supported

- **OpenAI Models**: GPT-4, GPT-3.5-turbo
- **Ollama Models**: Local model support
- **Custom Models**: Implement IModel interface

### Configuration Best Practices

```csharp
// For structured outputs (automatic configuration)
var structuredAgent = new Agent<string, AnalysisResult>(model, "Analyzer");
// Temperature automatically set to 0.1 for deterministic results

// For creative tasks
var creativeAgent = new Agent<string, string>(model, "Writer")
    .WithConfig(new ModelConfiguration
    {
        Temperature = 0.9,
        MaxTokens = 4096
    });
```

## 📊 Observability

### Logging and Monitoring

```csharp
// Custom logger implementation
public class StructuredLogger : ILogger
{
    public void Log(LogLevel level, string message, Exception ex = null)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = level.ToString(),
            Message = message,
            Exception = ex?.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(logEntry));
    }
}

// Using with agents
var agent = new Agent<string, string>(model, "Monitored Agent")
    .WithLogger(new StructuredLogger());
```

### Performance Metrics

Track key metrics:
- **Response Time**: Measure execution duration
- **Cost Tracking**: Monitor API usage and costs
- **Error Rates**: Track failures and retries
- **Memory Usage**: Monitor semantic memory costs

## 🔒 Security

### Best Practices

1. **API Key Management**
```csharp
// ✅ Use environment variables
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("API key not configured");

// ❌ Never hardcode keys
var apiKey = "sk-1234567890"; // DON'T DO THIS
```

2. **Input Validation**
```csharp
public class SecureAgent<TContext, TResult> : Agent<TContext, TResult>
{
    public override async Task<AgentResult<TResult>> ExecuteAsync(
        string prompt, 
        TContext context = default)
    {
        // Validate and sanitize input
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty");
            
        if (prompt.Length > 10000)
            throw new ArgumentException("Prompt too long");
            
        var sanitizedPrompt = SanitizeInput(prompt);
        return await base.ExecuteAsync(sanitizedPrompt, context);
    }
}
```

3. **Access Control**
```csharp
var secureAgent = new Agent<SecureContext, string>(model, "Secure Assistant")
    .WithPersona("You are a security-aware assistant")
    .WithContext(new SecureContext 
    { 
        UserId = authenticatedUser.Id,
        Permissions = authenticatedUser.Permissions 
    });
```

## 🎯 Best Practices Summary

### Cost Optimization
- **Start Simple**: Use basic agents for 80% of use cases
- **Opt-in Complexity**: Add semantic memory only when ROI is clear
- **Monitor Costs**: Track spending with structured logging

### Architecture Patterns
- **Single Responsibility**: One agent per specific task
- **Workflow Orchestration**: Use workflows for complex multi-step processes
- **Tool Composition**: Build reusable tool packs

### Development Workflow
1. **Prototype** with simple agents
2. **Measure** performance and costs
3. **Optimize** based on real usage data
4. **Scale** with proven patterns

---

## 📚 Next Steps

- [Getting Started](getting-started.md) - Quick start guide
- [Memory Architecture](memory-architecture.md) - Deep dive into cost optimization
- [API Reference](api/) - Complete API documentation
- [Examples](examples/) - Practical implementation examples