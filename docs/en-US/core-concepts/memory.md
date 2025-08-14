# Memory Management

AgentSharp provides sophisticated memory management capabilities that allow agents to remember, learn, and build context across conversations and sessions.

## Memory Architecture

### Two-Layer Memory System

AgentSharp implements a dual-layer memory architecture:

1. **Session Memory**: Conversation history within a single session
2. **Semantic Memory**: Long-term, AI-managed memories with vector similarity search

### Core Components

- **IMemoryManager**: High-level intelligent memory operations
- **IStorage**: Low-level storage abstraction
- **IEmbeddingService**: Vector similarity calculations
- **MemoryConfiguration**: Behavioral controls and limits

## Memory Manager

### IMemoryManager Interface

The `IMemoryManager` provides intelligent, AI-powered memory operations:

```csharp
public interface IMemoryManager
{
    // Context loading
    Task<MemoryContext> LoadContextAsync(string userId, string sessionId);
    
    // Message enhancement with memories
    Task<List<AIMessage>> EnhanceMessagesAsync(List<AIMessage> messages, MemoryContext context);
    
    // Process completed interactions
    Task ProcessInteractionAsync(AIMessage userMessage, AIMessage assistantMessage, MemoryContext context);
    
    // Memory retrieval
    Task<List<UserMemory>> GetExistingMemoriesAsync(MemoryContext context, int? limit = null);
    
    // Manual memory management
    Task<string> AddMemoryAsync(string memory, MemoryContext context);
    Task UpdateMemoryAsync(string memoryId, string newContent, MemoryContext context);
    Task DeleteMemoryAsync(string memoryId, MemoryContext context);
    Task ClearMemoryAsync(MemoryContext context);
}
```

### Memory Context

```csharp
public class MemoryContext
{
    public string UserId { get; set; }
    public string SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

## Memory Configuration

### Basic Configuration

```csharp
var memoryConfig = new MemoryConfiguration
{
    // AI prompts for memory operations
    ExtractionPromptTemplate = (userMsg, assistantMsg) => 
        $"Extract key information from this conversation:\nUser: {userMsg}\nAssistant: {assistantMsg}",
    
    ClassificationPromptTemplate = (memory) => 
        $"Classify this memory by importance (0.0-1.0): {memory}",
    
    RetrievalPromptTemplate = (query, memories) => 
        $"Given these memories: {memories}\nAnswer: {query}",
    
    // Behavioral controls
    MaxMemoriesPerInteraction = 5,
    MinImportanceThreshold = 0.5,
    MaxMemoriesPerUser = 1000,
    EnableAutoSummary = true
};

agent.WithMemoryConfiguration(memoryConfig);
```

### Domain-Specific Configuration

```csharp
var domainConfig = new MemoryDomainConfiguration
{
    // Custom memory categories
    CustomCategories = new[] { "preferences", "history", "technical_specs", "complaints" },
    
    // Specialized extraction prompt
    ExtractionPromptTemplate = (userMsg, assistantMsg) => 
        $"Extract customer preferences and technical requirements from:\nUser: {userMsg}\nAssistant: {assistantMsg}",
    
    MaxMemoriesPerInteraction = 3,
    MinImportanceThreshold = 0.7
};

agent.WithMemoryDomainConfiguration(domainConfig);
```

## Granular Memory Controls

### Feature Toggles

AgentSharp provides fine-grained control over memory features for cost and performance optimization:

```csharp
// Enable AI-powered memory extraction (uses LLM tokens)
agent.WithUserMemories(enable: true);

// Enable semantic memory search via tools (uses embedding tokens)
agent.WithMemorySearch(enable: true);

// Include conversation history in context (uses LLM tokens)
agent.WithHistoryToMessages(enable: true);

// Enable knowledge base search (future feature)
agent.WithKnowledgeSearch(enable: true);
```

### Cost-Conscious Usage

```csharp
// Minimal memory usage - only session history
var agent = new Agent<Context, Result>(model)
    .WithHistoryToMessages(enable: true);  // Only conversation history

// Memory without AI extraction - manual only
var agent = new Agent<Context, Result>(model)
    .WithSemanticMemory(storage, embeddings)
    .WithMemorySearch(enable: true);       // Search only, no auto-extraction

// Full memory capabilities
var agent = new Agent<Context, Result>(model)
    .WithSemanticMemory(storage, embeddings)
    .WithUserMemories(enable: true)        // AI extraction
    .WithMemorySearch(enable: true)        // Semantic search
    .WithHistoryToMessages(enable: true);  // Session history
```

## Storage Implementations

### SemanticSqliteStorage

SQLite-based storage with vector similarity search:

```csharp
var embeddingService = new OpenAIEmbeddingService("text-embedding-3-small", apiKey);
var storage = new SemanticSqliteStorage("memories.db", embeddingService, dimensions: 1536);

agent.WithSemanticMemory(storage, embeddingService);
```

**Features:**
- Persistent storage
- Vector similarity search via sqlite-vec
- Full-text search capabilities
- Transaction support
- Automatic schema management

### InMemoryStore

Development and testing storage:

```csharp
var storage = new InMemoryStore();
agent.WithSemanticMemory(storage, embeddingService);
```

**Features:**
- Fast access
- No persistence
- Ideal for testing
- Minimal setup

## Memory Operations

### Automatic Memory Management

When memory is enabled, AgentSharp automatically:

1. **Extracts** important information from conversations
2. **Classifies** memories by importance and category
3. **Stores** memories with vector embeddings
4. **Retrieves** relevant memories for new queries
5. **Summarizes** old memories when limits are reached

### Manual Memory Operations

```csharp
// Add specific memory
await agent.MemoryManager.AddMemoryAsync(
    "Customer prefers technical documentation over video tutorials", 
    context
);

// Update existing memory
await agent.MemoryManager.UpdateMemoryAsync(
    memoryId, 
    "Customer prefers technical docs and CLI interfaces",
    context
);

// Search memories
var relevantMemories = await agent.MemoryManager.GetExistingMemoriesAsync(context, limit: 10);

// Clear all memories for user
await agent.MemoryManager.ClearMemoryAsync(context);
```

### Memory Search with Tools

When `WithMemorySearch(true)` is enabled, agents get access to memory search tools:

```csharp
// Agent can now use these tools automatically:
// - search_user_memories(query: string)
// - add_user_memory(content: string)
// - update_user_memory(memory_id: string, new_content: string)
// - delete_user_memory(memory_id: string)
```

## Session History

### Configuration

```csharp
// Include conversation history in model context
agent.WithHistoryToMessages(enable: true);

// Limit history size
agent.WithHistoryToMessages(enable: true, maxMessages: 20);
```

### Session Management

```csharp
// Sessions are automatically managed by user/session ID
var context = new UserContext 
{
    UserId = "user123",
    SessionId = "session456"
};

// Each session maintains separate history
var result = await agent.ExecuteAsync("What did we discuss last time?", context);
```

## Memory Types

### UserMemory

```csharp
public class UserMemory
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public string Category { get; set; }
    public double Importance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

### SessionMessage

```csharp
public class SessionMessage
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string SessionId { get; set; }
    public string Role { get; set; }  // "user" or "assistant"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Advanced Features

### Memory Summarization

When memory limits are reached, AgentSharp can automatically summarize older memories:

```csharp
var config = new MemoryConfiguration
{
    MaxMemoriesPerUser = 1000,
    EnableAutoSummary = true,
    SummaryThreshold = 950  // Start summarizing at 95% capacity
};
```

### Memory Categories

Organize memories by custom categories:

```csharp
var config = new MemoryDomainConfiguration
{
    CustomCategories = new[] 
    {
        "personal_preferences",
        "technical_requirements", 
        "business_context",
        "previous_issues",
        "feature_requests"
    }
};
```

### Importance Scoring

Control which memories are retained:

```csharp
var config = new MemoryConfiguration
{
    MinImportanceThreshold = 0.7,  // Only keep important memories
    MaxMemoriesPerInteraction = 3   // Limit extraction per conversation
};
```

## Performance Optimization

### Embedding Efficiency

```csharp
// Use smaller, faster embedding models for memory
var embeddingService = new OpenAIEmbeddingService("text-embedding-3-small", apiKey);

// Configure batch processing
var config = new EmbeddingConfiguration
{
    BatchSize = 100,
    Dimensions = 1536
};
```

### Memory Limits

```csharp
// Prevent unbounded growth
var config = new MemoryConfiguration
{
    MaxMemoriesPerUser = 500,           // Per-user limit
    MaxMemoriesPerInteraction = 2,      // Per-conversation limit
    MinImportanceThreshold = 0.6        // Quality threshold
};
```

### Storage Optimization

```csharp
// SQLite optimization
var storage = new SemanticSqliteStorage(
    connectionString: "Data Source=memories.db;Cache Size=10000;",
    embeddingService: embeddingService,
    dimensions: 1536
);
```

## Best Practices

### 1. Start Simple

```csharp
// Begin with basic session history
agent.WithHistoryToMessages(enable: true);

// Add semantic memory when needed
agent.WithSemanticMemory(storage, embeddings);

// Enable AI features when value is proven
agent.WithUserMemories(enable: true)
     .WithMemorySearch(enable: true);
```

### 2. Design for Your Domain

```csharp
// Customer service agent
var customerConfig = new MemoryDomainConfiguration
{
    CustomCategories = new[] { "preferences", "issues", "resolutions", "escalations" },
    MinImportanceThreshold = 0.8,  // High threshold for support
    MaxMemoriesPerInteraction = 2
};

// Learning assistant  
var tutorConfig = new MemoryDomainConfiguration
{
    CustomCategories = new[] { "knowledge_gaps", "learning_style", "progress", "interests" },
    MinImportanceThreshold = 0.5,  // Lower threshold for learning
    MaxMemoriesPerInteraction = 5
};
```

### 3. Monitor Token Usage

```csharp
// Track memory-related token consumption
var telemetry = new ConsoleTelemetryService();
telemetry.Enable();

// After execution
var summary = telemetry.GetSummary();
Console.WriteLine($"Memory tokens: {summary.MemoryTokens}");
Console.WriteLine($"Embedding tokens: {summary.EmbeddingTokens}");
```

### 4. Test Memory Behavior

```csharp
// Test memory extraction
await agent.ExecuteAsync("I prefer dark themes and technical documentation", context);

// Verify memory was created
var memories = await agent.MemoryManager.GetExistingMemoriesAsync(context);
Assert.Contains(memories, m => m.Content.Contains("dark themes"));

// Test memory retrieval
var result = await agent.ExecuteAsync("What UI preferences should you remember about me?", context);
// Should reference the previously stored preference
```

## Troubleshooting

### Common Issues

1. **No Memories Created**: Check `MinImportanceThreshold` - may be too high
2. **Too Many Memories**: Reduce `MaxMemoriesPerInteraction` or increase threshold
3. **Poor Search Results**: Verify embedding service and dimensions match
4. **High Token Usage**: Disable AI extraction or reduce interaction limits

### Debugging

```csharp
// Enable detailed logging
var memoryManager = new MemoryManager(storage, embeddingService, config)
{
    EnableDebugLogging = true
};

// Check memory statistics
var stats = await storage.GetMemoryStatsAsync(userId);
Console.WriteLine($"Total memories: {stats.TotalCount}");
Console.WriteLine($"Average importance: {stats.AverageImportance}");
```

## Next Steps

- [Tools & Functions](tools.md) - Memory-aware tool development
- [Team Orchestration](orchestration.md) - Shared memory across agents  
- [API Reference](../api-reference/memory.md) - Complete memory API
- [Examples](../examples/memory-examples.md) - Real-world memory scenarios