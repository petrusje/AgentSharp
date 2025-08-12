# 🚀 Quick Start Guide

> Get started with AgentSharp in minutes with the new cost-optimized architecture

## 📋 Prerequisites

- .NET Standard 2.0 or higher
- OpenAI API key (for semantic memory)

## 🎯 Installation

1. **Create new project**
```bash
dotnet new console -n MyProject
cd MyProject
```

2. **Add AgentSharp package**
```bash
dotnet add package AgentSharp
```

3. **Configure environment variables** (optional for simple agents)
```bash
# Linux/macOS
export OPENAI_API_KEY="your-key-here"

# Windows PowerShell
$env:OPENAI_API_KEY="your-key-here"
```

## 🔥 First Agent (Low Cost)

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Create model
var modelOptions = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
};
var model = new ModelFactory().CreateModel("openai", modelOptions);

// ✅ Simple agent - NO EXTRA COSTS
var agent = new Agent<string, string>(model, "Assistant")
    .WithPersona("You are a helpful and friendly assistant")
    .WithAnonymousMode(true); // No need to manage IDs

// Execute - minimal cost
var result = await agent.ExecuteAsync("Hello! How can I help you?");
Console.WriteLine(result.Data);

// Cost: ~$0.005 per interaction
```

## 💰 Cost Comparison - Conscious Choice

### Simple vs Smart Agent

```csharp
// 💚 LOW COST: For 80% of use cases
var simpleAgent = new Agent<string, string>(model, "Basic Assistant")
    .WithPersona("Direct and efficient assistant")
    .WithAnonymousMode(true);

// 🚨 HIGH COST: Only when ROI justifies
var smartAgent = new Agent<Context, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage, embeddingService) // +400% cost
    .WithReasoning(true) // +50% cost
    .WithPersona("Assistant with advanced contextual memory");

Console.WriteLine("Simple Agent: ~$15/month for 100 interactions/day");
Console.WriteLine("Smart Agent: ~$84/month for 100 interactions/day");
```

## 🧠 When to Use Semantic Memory

```csharp
// ✅ Use semantic memory ONLY when:
// • Need to remember context across sessions
// • Intelligent search in extensive history  
// • Conceptual relationships are important
// • ROI justifies 5x higher cost

// Example: Personal Assistant
var embeddingService = new OpenAIEmbeddingService(
    Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    "https://api.openai.com");

var storage = new VectorSqliteStorage("personal_memory.db", embeddingService);

var personalAssistant = new Agent<Context, string>(model, "Personal Assistant")
    .WithSemanticMemory(storage, embeddingService) // Conscious opt-in
    .WithPersona("Assistant who knows me and evolves with me")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });

// First conversation - establish preferences
await personalAssistant.ExecuteAsync("I like meetings in the morning, after 9am");

// Weeks later - automatic semantic search
var response = await personalAssistant.ExecuteAsync("When should I schedule the meeting?");
// System automatically finds: "Considering your preference for mornings..."
```

## 🔄 First Workflow

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

// Specialized agents (individual low cost)
var analyzer = new Agent<AnalysisContext, string>(model, "Analyzer")
    .WithPersona("Data analysis specialist");

var finalizer = new Agent<AnalysisContext, string>(model, "Finalizer")
    .WithPersona("Structured conclusions specialist");

// Create workflow
var workflow = new SequentialWorkflow<AnalysisContext, string>("Analysis")
    .RegisterStep("Analysis", analyzer,
        ctx => $"Analyze this data: {ctx.Input}",
        (ctx, res) => ctx.Analysis = res)
    .RegisterStep("Conclusion", finalizer,
        ctx => $"Conclude based on analysis: {ctx.Analysis}",
        (ctx, res) => ctx.Result = res);

// Execute
var context = new AnalysisContext { Input = "Data to analyze..." };
var result = await workflow.ExecuteAsync(context);
Console.WriteLine(result);
```

## 🎯 Examples by Use Case

### 1. Customer Support Chat (Low Cost)
```csharp
var chatBot = new Agent<string, string>(model, "Support")
    .WithPersona(@"
        You are an efficient customer support agent.
        - Be direct and helpful
        - Resolve quickly
        - Escalate complex cases
    ")
    .WithAnonymousMode(true);

var response = await chatBot.ExecuteAsync("How do I cancel my account?");
// Cost: ~$0.005 per interaction
```

### 2. Smart Calculator (Low Cost)
```csharp
public class CalculationResult
{
    public double Value { get; set; }
    public string Explanation { get; set; }
    public List<string> Steps { get; set; }
}

var calculator = new Agent<string, CalculationResult>(model, "Calculator")
    .WithPersona("Calculator that explains reasoning");

var calculation = await calculator.ExecuteAsync("What is 15% of 2,450?");
Console.WriteLine($"Result: {calculation.Data.Value}");
Console.WriteLine($"Explanation: {calculation.Data.Explanation}");
```

### 3. Code Assistant (Medium Cost)
```csharp
var codeAssistant = new Agent<string, string>(model, "Code Assistant")
    .WithPersona("C# and .NET specialist")
    .WithReasoning(true) // Adds reasoning cost
    .WithTools(new CodeAnalysisToolPack());

var code = await codeAssistant.ExecuteAsync(
    "Review this code and suggest improvements: ...");
```

### 4. Academic Researcher (High Cost)
```csharp
public class ResearchContext
{
    public string UserId { get; set; } = "researcher123";
    public string SessionId { get; set; } = "phd_project";
    public string Domain { get; set; }
}

var researcher = new Agent<ResearchContext, string>(model, "Researcher")
    .WithSemanticMemory(storage, embeddingService) // Memory across projects
    .WithReasoning(true) // Deep analysis
    .WithPersona(@"
        PhD researcher who maintains context across projects.
        - Connects information from different papers
        - Identifies patterns over time
        - Suggests research directions based on history
    ")
    .WithTools(new AcademicToolPack());

var result = await researcher.ExecuteAsync(
    "Analyze machine learning trends from the last 6 months",
    new ResearchContext { Domain = "ML" });

// Cost: ~$0.025 per interaction (5x more, but deep context)
```

## 🛠️ Advanced Configurations

### Structured Reasoning
```csharp
var reasoningAgent = new Agent<string, string>(model, "Analyst")
    .WithReasoning(true) // Enable step-by-step reasoning
    .WithReasoningSteps(minSteps: 3, maxSteps: 8)
    .WithPersona("Analyst who thinks methodically");

var analysis = await reasoningAgent.ExecuteAsync("Analyze pros and cons of this decision...");

// Result includes detailed reasoning process
Console.WriteLine("Reasoning:");
foreach (var step in analysis.ReasoningSteps)
{
    Console.WriteLine($"- {step.Title}: {step.Reasoning}");
}
```

### Automatic Structured Output
```csharp
public class AnalysisReport
{
    public string Summary { get; set; }
    public List<string> KeyPoints { get; set; }
    public double Confidence { get; set; }
    public List<string> Recommendations { get; set; }
}

// Structured output automatically configured based on type
var analyzer = new Agent<string, AnalysisReport>(model, "Analyzer");

var report = await analyzer.ExecuteAsync("Analyze this document...");

// Automatic typed access
Console.WriteLine($"Summary: {report.Data.Summary}");
Console.WriteLine($"Confidence: {report.Data.Confidence:P}");
```

### Cost Control with Hybrid Approach
```csharp
public class HybridAgent
{
    private readonly Agent<string, string> _simple;
    private readonly Agent<Context, string> _smart;

    public HybridAgent(IModel model, IStorage storage, IEmbeddingService embeddingService)
    {
        _simple = new Agent<string, string>(model, "Simple");
        _smart = new Agent<Context, string>(model, "Smart")
            .WithSemanticMemory(storage, embeddingService);
    }

    public async Task<string> ExecuteAsync(string prompt, Context context = null)
    {
        // Use simple agent for basic queries (80% of cases)
        if (IsSimpleQuery(prompt))
        {
            var result = await _simple.ExecuteAsync(prompt);
            return result.Data;
        }

        // Use smart agent only when necessary (20% of cases)
        var smartResult = await _smart.ExecuteAsync(prompt, context);
        return smartResult.Data;
    }

    private bool IsSimpleQuery(string prompt)
    {
        var indicators = new[] { "how", "when", "what is", "define" };
        return indicators.Any(i => prompt.ToLower().Contains(i));
    }
}

// Balanced usage: economy + intelligence when needed
var agent = new HybridAgent(model, storage, embeddingService);
```

## 📊 Cost Monitoring

```csharp
public class CostTrackingLogger : ILogger
{
    private decimal _totalCost = 0;

    public void Log(LogLevel level, string message, Exception ex = null)
    {
        if (message.Contains("API call cost"))
        {
            // Extract cost from message
            var cost = ExtractCostFromMessage(message);
            _totalCost += cost;
            
            Console.WriteLine($"💰 Current cost: ${_totalCost:F4}");
        }
    }

    // Implementation of other methods...
}

var agent = new Agent<string, string>(model, "Monitor")
    .WithLogger(new CostTrackingLogger());
```

## 🎯 Next Steps

### 1. Start Simple (Recommended)
```csharp
// For 80% of cases - maximum economy
var agent = new Agent<string, string>(model)
    .WithAnonymousMode(true);
```

### 2. Evolve as Needed
```csharp
// Add features gradually
if (needsMemory)
    agent.WithSemanticMemory(storage, embeddingService);

if (needsReasoning)
    agent.WithReasoning(true);
```

### 3. Explore Advanced Features
- [Memory Architecture](memory-architecture.md) - Understand costs vs benefits
- [Practical Examples](examples/) - Real cases with ROI analysis
- [API Reference](api/) - Complete documentation

### 4. Monitor and Optimize
- Implement cost tracking
- Analyze usage patterns
- Optimize based on real ROI

## 📚 Resources by Category

### Essential Documentation
- [Memory Architecture](memory-architecture.md) - **READ FIRST**
- [Agent API](api/core/agent.md) - Complete reference
- [Best Practices](best-practices.md) - Cost optimization

### Practical Examples
- [Simple Agents](examples/basic-agents.md)
- [Semantic Memory](examples/semantic-memory.md)
- [Advanced Workflows](examples/advanced-workflows.md)

### Use Cases
- [Chat/Support](use-cases/customer-support.md)
- [Personal Assistants](use-cases/personal-assistants.md)
- [Data Analysis](use-cases/data-analysis.md)

## ❓ Support

- 📖 [Complete Documentation](https://agentsharp.dev/docs)
- 🐛 [GitHub Issues](https://github.com/agentsharp/issues)
- 💬 [Discussions](https://github.com/agentsharp/discussions)

---

## 🎊 Congratulations!

You're ready to build intelligent agents with complete cost control. Remember:

- **Start simple** (low cost)
- **Evolve as needed** (ROI guides decisions)
- **Monitor costs** always
- **Optimize based on real data**