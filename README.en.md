# 🤖 AgentSharp

> Modern .NET framework for creating AI agents with advanced workflows, intelligent memory, and multi-agent orchestration

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4)](https://docs.microsoft.com/dotnet/standard/net-standard)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-512BD4)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-12.0-239120)](https://docs.microsoft.com/dotnet/csharp/)
[![NuGet](https://img.shields.io/badge/NuGet-Available-blue)](https://www.nuget.org/packages/AgentSharp)

## 📋 Overview

**AgentSharp** is a .NET framework that facilitates the creation of robust, scalable, and extensible AI agents. With optimized architecture, it offers granular cost control, advanced memory system with vector search, team agent orchestration, and a flexible platform for enterprise use cases.

## ✨ Key Features

### 🎯 **Intelligent Agents**
- **Typed Agents**: Clear contracts with input and output types
- **Structured Reasoning**: Step-by-step analysis with reasoning chains
- **Structured Output**: Automatic deserialization to complex types
- **Extensible Tools**: Plugin system via Tool Packs

### 🧠 **Advanced Memory System**
- **Semantic Memory**: Intelligent context search with embeddings
- **Vector Search**: Optimized implementation with sqlite-vec
- **Multiple Storage**: SemanticSqliteStorage (persistent), SemanticMemoryStorage (in-memory)
- **Cost Control**: Low and high-cost options as needed

### 🤖 **Multi-Agent Orchestration**
- **Team Orchestration**: Coordinate, Route, and Collaborate modes
- **Advanced Workflows**: Sequential, Parallel, and Conditional
- **Handoff System**: Intelligent transfer between agents
- **Load Balancing**: Automatic load distribution

### 🏢 **Production Ready**
- **Anonymous Mode**: Works without authentication system
- **Observability**: Integrated logging and metrics
- **Performance**: Architecture optimized for high performance
- **Extensibility**: Open APIs for enterprise customization

## 🚀 Quick Start

### Installation

```bash
dotnet add package AgentSharp
```

### Basic Agent

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Configure model
var model = new ModelFactory().CreateModel("openai", new ModelOptions
{
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    ModelName = "gpt-4"
});

// Create agent
var agent = new Agent<string, string>(model, "Assistant")
    .WithPersona("You are a helpful and direct assistant");

// Execute
var result = await agent.ExecuteAsync("How can I help you?");
Console.WriteLine(result.Data);
```

### Agent with Vector Memory

```csharp
// Configure storage with optimized vector search
var embeddingService = new OpenAIEmbeddingService(apiKey);
var storage = new SemanticSqliteStorage("Data Source=memory.db", embeddingService, 1536);
await storage.InitializeAsync();

// Create agent with semantic memory
var agent = new Agent<Context, string>(model, "Intelligent Assistant")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Assistant that remembers conversation context and learns from each interaction")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });

var result = await agent.ExecuteAsync("I prefer strong coffee in the morning, but light in the afternoon");
```

### Multi-Agent Workflow

```csharp
// Define context
public class AnalysisContext
{
    public string Input { get; set; }
    public string Data { get; set; }
    public string Result { get; set; }
}

// Create specialized agents
var collector = new Agent<AnalysisContext, string>(model, "Collector");
var analyst = new Agent<AnalysisContext, string>(model, "Analyst");

// Configure workflow
var workflow = new SequentialWorkflow<AnalysisContext, string>("Analysis")
    .RegisterStep("Collection", collector,
        ctx => $"Collect data about: {ctx.Input}",
        (ctx, res) => ctx.Data = res)
    .RegisterStep("Analysis", analyst,
        ctx => $"Analyze the data: {ctx.Data}",
        (ctx, res) => ctx.Result = res);

// Execute
var context = new AnalysisContext { Input = "financial market" };
var result = await workflow.ExecuteAsync(context);
```

### Structured Output

```csharp
public class Report
{
    public string Title { get; set; }
    public List<string> Points { get; set; }
    public double Confidence { get; set; }
}

// Agent with typed output
var generator = new Agent<string, Report>(model, "Report Generator");
var report = await generator.ExecuteAsync("Create a report about AI");

Console.WriteLine($"Title: {report.Data.Title}");
Console.WriteLine($"Confidence: {report.Data.Confidence:P}");
```

### Agent Team Orchestration

```csharp
// Configure team context
public class ProjectContext
{
    public string Requirements { get; set; }
    public string UserId { get; set; }
    public string SessionId { get; set; }
}

// Create specialized agents
var architect = new Agent<ProjectContext, string>(model, "Architect")
    .WithPersona("Software architecture specialist")
    .WithSemanticMemory(storage);

var developer = new Agent<ProjectContext, string>(model, "Developer") 
    .WithPersona("Experienced full-stack developer")
    .WithSemanticMemory(storage);

var reviewer = new Agent<ProjectContext, string>(model, "Reviewer")
    .WithPersona("Code review and quality specialist")
    .WithSemanticMemory(storage);

// Configure orchestration in Coordinate mode
var team = new TeamOrchestrator<ProjectContext, string>()
    .AddAgent("architect", architect)
    .AddAgent("developer", developer) 
    .AddAgent("reviewer", reviewer)
    .WithMode(TeamMode.Coordinate)
    .WithCoordinator("architect")
    .WithHandoffRules("development -> review -> delivery");

// Execute collaborative project
var context = new ProjectContext 
{ 
    Requirements = "Order management system",
    UserId = "pm001",
    SessionId = Guid.NewGuid().ToString()
};

var result = await team.ExecuteAsync("Develop complete system", context);
```

## 🧠 Advanced Memory System

AgentSharp offers optimized memory architecture with two main implementations:

### 🏃‍♂️ **SemanticSqliteStorage** (Recommended for Production)
- **Performance**: Native vector search with sqlite-vec
- **Scalability**: Supports millions of embeddings
- **Complexity**: O(log n) for similarity queries  
- **Use case**: Enterprise systems with large data volumes

### 💡 **SemanticMemoryStorage** (Ideal for Development)
- **Performance**: Optimized in-memory HNSW
- **Speed**: Instant initialization for tests
- **Limitations**: Suitable for smaller datasets
- **Use case**: Development, prototyping, and testing

```csharp
// Production - with sqlite-vec
var prodStorage = new SemanticSqliteStorage(
    connectionString: "Data Source=production.db",
    embeddingService: new OpenAIEmbeddingService(apiKey),
    dimensions: 1536
);

// Development - HNSW in memory  
var devStorage = new SemanticMemoryStorage(
    new CompactHNSWConfiguration
    {
        Dimensions = 384, // Reduced dimensions for development
        MaxConnections = 16,
        SearchK = 200
    }
);

// Adaptive agent for different environments
var agent = new Agent<Context, string>(model, "Assistant")
    .WithSemanticMemory(Environment.IsProduction() ? prodStorage : devStorage)
    .WithPersona("Assistant that adapts to execution environment");
```

## 🔧 Tools and Extensibility

```csharp
// Custom tool
public class CalculatorTool : ITool
{
    public string Name => "calculator";
    public string Description => "Performs mathematical calculations";
    
    public async Task<string> ExecuteAsync(string input)
    {
        // Calculator implementation
        return PerformCalculation(input);
    }
}

// Use with agent
var agent = new Agent<string, string>(model, "Mathematician")
    .WithTools(new MathToolPack())
    .AddTool(new CalculatorTool());
```

## 📚 Complete Documentation

### 📝 English (en-US)  
- [🚀 Quick Start](docs/en-US/getting-started.md) - Installation and first steps
- [🎯 Core Concepts](docs/en-US/core-concepts.md) - Architecture and base concepts
- [🧠 Memory System](docs/en-US/memory-system.md) - Complete memory and storage guide
- [🤖 Agent API](docs/en-US/api/core/agent.md) - Complete API reference
- [🔄 Advanced Workflows](docs/en-US/workflows.md) - Orchestration and multi-agents
- [🧠 Structured Reasoning](docs/en-US/reasoning.md) - Reasoning system
- [📊 Practical Examples](docs/en-US/examples.md) - Real-world use cases
- [🏢 Enterprise Cases](docs/en-US/enterprise.md) - Production implementation

### 📝 Português (pt-BR)
- [🚀 Início Rápido](docs/pt-BR/getting-started.md) - Primeiros passos e instalação
- [🎯 Conceitos Fundamentais](docs/pt-BR/core-concepts.md) - Arquitetura e conceitos base
- [🧠 Sistema de Memória](docs/pt-BR/memory-system.md) - Guia completo de memória e storage
- [🤖 API do Agent](docs/pt-BR/api/core/agent.md) - Referência completa da API
- [🔄 Workflows Avançados](docs/pt-BR/workflows.md) - Orquestração e multi-agentes
- [🧠 Raciocínio Estruturado](docs/pt-BR/reasoning.md) - Sistema de reasoning
- [📊 Exemplos Práticos](docs/pt-BR/examples.md) - Casos de uso reais
- [🏢 Casos Empresariais](docs/pt-BR/enterprise.md) - Implementação em produção

### 🎮 Example Console
Explore **27+ interactive examples** in the `AgentSharp.console` project:
- **Level 1**: Fundamentals (Basic agents, personality, tools)  
- **Level 2**: Intermediate (Reasoning, structured outputs, memory)
- **Level 3**: Advanced (Workflows, semantic search, enterprise systems)
- **Specialized**: Medical assistants, legal, technical consultants
- **Team Orchestration**: Agent team coordination

```bash
# Run interactive console
cd AgentSharp.console
dotnet run
```

## 🎯 Enterprise Use Cases

### 🏥 **Healthcare and Medicine**
- **Medical Assistants**: AI-aided diagnosis with patient history
- **Exam Analysis**: Structured processing of medical reports
- **Hospital Management**: Optimization of workflows and resources

### ⚖️ **Legal and Compliance**
- **Legal Consulting**: Analysis of contracts and legal documents
- **Due Diligence**: Automated auditing of business documents
- **Compliance**: Regulatory compliance monitoring

### 🏢 **Business and Finance**
- **Virtual Assistants**: High-quality customer service
- **Financial Analysis**: Data processing with historical context
- **Knowledge Management**: Intelligent enterprise knowledge base

### 🔧 **Technology and Development**
- **Code Review**: Automated code review with context
- **DevOps**: Complex workflow automation with decision making
- **Documentation**: Automatic technical documentation generation

### 🎓 **Education and Training**
- **Virtual Tutors**: Personalized teaching with student history
- **Assessment**: Automated grading and feedback
- **Academic Research**: Scientific literature analysis

## 🛠️ Modern Architecture

```
┌──────────────────────┐
│   Team Orchestrator  │  🤖 Multi-Agent Orchestration
├──────────────────────┤  ├─ Coordinate Mode
│ • TeamMode.Route     │  ├─ Route Mode  
│ • TeamMode.Coordinate│  └─ Collaborate Mode
│ • TeamMode.Collaborate│
└──────┬───────────────┘
       │
┌──────▼───────────────┐
│       Agent          │  🎯 Intelligent Agent
├──────────────────────┤  ├─ Typed Inputs/Outputs
│ • Persona & Context  │  ├─ Structured Reasoning
│ • Tool System        │  ├─ Memory Integration
│ • Memory Manager     │  └─ Extensible Tools
│ • Reasoning Engine   │
└──────┬───────────────┘
       │
┌──────▼───────────────┐    ┌─────────────────────┐
│   Memory System      │    │   Vector Storage    │
├──────────────────────┤    ├─────────────────────┤
│ • Semantic Search    │◄──►│ • VectorSqliteVec   │  🧠 Optimized Memory
│ • Context Loading    │    │ • CompactHNSW       │     System
│ • Smart Summarization│    │ • Embedding Service │
│ • Cost Control       │    │ • Similarity Search │
└──────────────────────┘    └─────────────────────┘
       │
┌──────▼───────────────┐    ┌─────────────────────┐
│     Workflows        │    │   Model Factory     │
├──────────────────────┤    ├─────────────────────┤
│ • Sequential         │    │ • OpenAI GPT-4      │  ⚡ Extensibility
│ • Parallel           │    │ • Azure OpenAI      │     and Integration
│ • Conditional        │    │ • Custom Models     │
│ • Advanced Routing   │    │ • Provider Agnostic │
└──────────────────────┘    └─────────────────────┘
```

## 📦 Optimized Storage Providers

### ✅ **Recommended Implementations (Post-Optimization)**
- **🏃‍♂️ SemanticSqliteStorage**: High-performance native vector search
- **💡 SemanticMemoryStorage**: Optimized HNSW for development

### ❌ **Removed Implementations (Redundant/Problematic)**
- ~~SqliteStorage~~ - Replaced by SemanticSqliteStorage
- ~~VectorSqliteStorage~~ - Inadequate performance  
- ~~HNSWMemoryStorage~~ - Unnecessary complexity
- ~~InMemoryStorage~~ - Semantic search limitations

> **🎯 Optimization Result**: 60% complexity reduction with significant performance improvement

## ⚙️ Configuration

### Environment Variables

```bash
# API Configuration
OPENAI_API_KEY=your-key-here
OPENAI_ENDPOINT=https://api.openai.com  # optional

# Model Configuration  
MODEL_NAME=gpt-4o-mini  # or gpt-4, gpt-3.5-turbo
TEMPERATURE=0.7         # 0.0 to 1.0
MAX_TOKENS=2048        # token limit

# Memory Configuration
EMBEDDING_MODEL=text-embedding-3-small  # or text-embedding-ada-002
VECTOR_DIMENSIONS=1536                   # embedding model dimensions
```

### Programmatic Configuration

```csharp
var options = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = apiKey,
    DefaultConfiguration = new ModelConfiguration
    {
        Temperature = 0.7,
        MaxTokens = 2048
    }
};
```

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the project
2. Create a branch for your feature
3. Add tests for new functionality
4. Keep documentation updated
5. Submit a Pull Request

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for more details.

## 🙏 Credits

- [OpenAI](https://openai.com) for the language model API
- [sqlite-vec](https://github.com/asg017/sqlite-vec) for the vector search extension
- .NET Community for the robust ecosystem

---

**AgentSharp** - .NET Framework for modern AI agents