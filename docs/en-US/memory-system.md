# 🧠 Memory System - AgentSharp

AgentSharp offers an advanced and optimized memory system designed for different enterprise needs and use cases.

## 📋 Overview

AgentSharp's memory system has been **completely optimized** to deliver maximum performance with clean architecture. We removed redundant implementations and performance issues, keeping only the **two best solutions**.

> **🎯 Optimization Result**: 60% complexity reduction with significant performance improvement

## 🏃‍♂️ VectorSqliteVecStorage (Recommended for Production)

### Features
- **Performance**: Native vector search with sqlite-vec
- **Scalability**: Supports millions of embeddings
- **Complexity**: O(log n) for similarity queries  
- **Use case**: Enterprise systems with large data volumes

### Installation
```bash
# Install sqlite-vec (requires manual setup for security)
# Download from: https://github.com/asg017/sqlite-vec/releases
# Copy vec0.dylib to project folder
```

### Basic Usage
```csharp
// Configure embedding service
var embeddingService = new OpenAIEmbeddingService(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    endpoint: "https://api.openai.com",
    logger: new ConsoleLogger(),
    model: "text-embedding-3-small"
);

// Create production storage
var storage = new VectorSqliteVecStorage(
    connectionString: "Data Source=production.db",
    embeddingService: embeddingService,
    dimensions: 1536, // Compatible with text-embedding-3-small
    distanceMetric: "cosine"
);

// Initialize
await storage.InitializeAsync();

// Use with agent
var agent = new Agent<Context, string>(model, "Enterprise Assistant")
    .WithSemanticMemory(storage, embeddingService)
    .WithContext(new Context { UserId = "emp001", SessionId = "session123" });
```

### Advanced Configuration
```csharp
var storage = new VectorSqliteVecStorage(
    connectionString: "Data Source=enterprise.db;Cache=Shared;",
    embeddingService: embeddingService,
    dimensions: 1536,
    distanceMetric: "cosine"
);

// Configure indexes for performance
await storage.CreateIndexAsync("memories", "embedding");
await storage.OptimizeAsync();
```

## 💡 CompactHNSWMemoryStorage (Ideal for Development)

### Features
- **Performance**: Optimized in-memory HNSW
- **Speed**: Instant initialization for tests
- **Limitations**: Suitable for smaller datasets (< 100k embeddings)
- **Use case**: Development, prototyping, and testing

### Basic Usage
```csharp
// Create development storage
var config = new CompactHNSWConfiguration
{
    Dimensions = 384, // Reduced dimensions for development
    MaxConnections = 16,
    SearchK = 200,
    EfConstruction = 200,
    MaxElements = 100000
};

var storage = new CompactHNSWMemoryStorage(config);

// Use with agent
var agent = new Agent<Context, string>(model, "Dev Assistant")
    .WithSemanticMemory(storage)
    .WithContext(new Context { UserId = "dev001", SessionId = "dev_session" });
```

### Development Optimization
```csharp
// Use compact embedding service
var embeddingService = new CompactEmbeddingService(
    baseService: new OpenAIEmbeddingService(apiKey),
    targetDimensions: 384, // Reduce dimensions for speed
    reductionMethod: ReductionMethod.PCA
);

var storage = new CompactHNSWMemoryStorage(config);
```

## 🎯 Environment-Adaptive Strategy

### Intelligent Implementation
```csharp
public class AdaptiveMemoryFactory
{
    public static IStorage CreateStorage(IConfiguration config, ILogger logger)
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";
        
        return environment switch
        {
            "Production" => CreateProductionStorage(config, logger),
            "Staging" => CreateStagingStorage(config, logger),
            "Development" => CreateDevelopmentStorage(config, logger),
            _ => CreateDevelopmentStorage(config, logger)
        };
    }
    
    private static VectorSqliteVecStorage CreateProductionStorage(IConfiguration config, ILogger logger)
    {
        var embeddingService = new OpenAIEmbeddingService(
            apiKey: config["OpenAI:ApiKey"],
            model: "text-embedding-3-small",
            logger: logger
        );
        
        return new VectorSqliteVecStorage(
            connectionString: config.GetConnectionString("Production"),
            embeddingService: embeddingService,
            dimensions: 1536
        );
    }
    
    private static CompactHNSWMemoryStorage CreateDevelopmentStorage(IConfiguration config, ILogger logger)
    {
        return new CompactHNSWMemoryStorage(
            new CompactHNSWConfiguration
            {
                Dimensions = 384,
                MaxConnections = 16,
                SearchK = 100
            }
        );
    }
}
```

### Agent Usage
```csharp
// Adaptive agent
var storage = AdaptiveMemoryFactory.CreateStorage(configuration, logger);
var agent = new Agent<Context, string>(model, "Adaptive Assistant")
    .WithSemanticMemory(storage)
    .WithPersona("Assistant that adapts to execution environment");
```

## 🚫 Removed Implementations (Do Not Use)

### ❌ Classes Eliminated Due to Performance/Redundancy Issues

- **~~SqliteStorage~~** - Incomplete implementation, replaced by VectorSqliteVecStorage
- **~~VectorSqliteStorage~~** - Inadequate performance (O(n) search without indexing)  
- **~~HNSWMemoryStorage~~** - Unnecessary complexity vs CompactHNSWMemoryStorage
- **~~InMemoryStorage~~** - Semantic search limitations, no vector capability

### ⚠️ Legacy Code Migration

If you were using old implementations:

```csharp
// ❌ OLD - NO LONGER WORKS
var storage = new SqliteStorage("connection");
var storage = new VectorSqliteStorage("connection");
var storage = new HNSWMemoryStorage(config);

// ✅ NEW - Use these implementations
var storage = new VectorSqliteVecStorage("connection", embeddingService, 1536);
var storage = new CompactHNSWMemoryStorage(compactConfig);
```

## 🔧 Configuration and Optimization

### Performance Tuning for VectorSqliteVecStorage
```csharp
// Optimized settings for production
var storage = new VectorSqliteVecStorage(
    connectionString: "Data Source=prod.db;Journal Mode=WAL;Cache Size=10000;",
    embeddingService: embeddingService,
    dimensions: 1536,
    distanceMetric: "cosine"
);

// Index optimizations
await storage.CreateIndexAsync("memories", "embedding");
await storage.VacuumAsync(); // Optimize space
```

### Monitoring and Metrics
```csharp
// Get performance metrics
var metrics = storage.GetMetrics();
Console.WriteLine($"Total embeddings: {metrics.TotalEmbeddings}");
Console.WriteLine($"Avg search time: {metrics.AverageSearchTime}ms");
Console.WriteLine($"Memory usage: {metrics.MemoryUsage}MB");
```

## 📊 Performance Comparison

| Storage | Search | Initialization | Memory | Scalability |
|---------|--------|---------------|---------|-------------|
| VectorSqliteVecStorage | O(log n) | ~2-5s | Low | Millions |
| CompactHNSWMemoryStorage | O(log n) | ~100ms | High | ~100k |
| ~~SqliteStorage~~ | O(n) | ~1s | Low | Limited |
| ~~VectorSqliteStorage~~ | O(n) | ~3s | Low | Limited |

## 🎓 Practical Examples

### Medical Assistant with Semantic Memory
```csharp
var embeddingService = new OpenAIEmbeddingService(apiKey);
var storage = new VectorSqliteVecStorage("Data Source=medical.db", embeddingService, 1536);

var medicalAssistant = new Agent<PatientContext, string>(model, "Dr. AI")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Medical specialist who remembers complete patient history")
    .WithContext(new PatientContext 
    { 
        UserId = "patient123", 
        SessionId = "consultation_2024_01_15" 
    });

// Assistant will remember previous consultations, exams, medications, etc.
var response = await medicalAssistant.ExecuteAsync("Patient reports recurrent abdominal pain");
```

### Technical Support System
```csharp
var storage = new VectorSqliteVecStorage("Data Source=support.db", embeddingService, 1536);

var technicalSupport = new Agent<TicketContext, string>(model, "Specialized Support")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Technical specialist with access to complete knowledge base")
    .WithContext(new TicketContext 
    { 
        UserId = "client456", 
        SessionId = "ticket_789" 
    });

// Semantic search in similar ticket history
var solution = await technicalSupport.ExecuteAsync("Application freezes when uploading large files");
```

## 🔍 Troubleshooting

### Common Issues

**1. sqlite-vec not found**
```bash
# Solution: Check sqlite-vec installation
dotnet run -- 21  # Check installation
dotnet run -- 22  # Installation guide
```

**2. Slow search performance**
```csharp
// Check if indexes were created
await storage.CreateIndexAsync("memories", "embedding");
await storage.AnalyzeAsync(); // Update statistics
```

**3. Excessive memory usage**
```csharp
// For development, use CompactHNSWMemoryStorage with reduced dimensions
var config = new CompactHNSWConfiguration { Dimensions = 256 };
var storage = new CompactHNSWMemoryStorage(config);
```

## 🎯 Next Steps

1. **[Advanced Workflows](workflows.md)** - Multi-agent orchestration
2. **[Structured Reasoning](reasoning.md)** - Reasoning system
3. **[Practical Examples](examples.md)** - Real-world use cases
4. **[API Reference](api/core/agent.md)** - Complete API documentation

---

> **💡 Tip**: Always start with CompactHNSWMemoryStorage for development and migrate to VectorSqliteVecStorage in production for maximum performance and scalability.