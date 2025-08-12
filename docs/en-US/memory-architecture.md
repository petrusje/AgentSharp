# 🧠 AgentSharp Memory Architecture

> Understand the new separation between Simple History and Semantic Memory for cost optimization

## 📋 Table of Contents

- [Overview](#overview)
- [Previous Problem](#previous-problem)
- [New Architecture](#new-architecture)
- [Cost Comparison](#cost-comparison)
- [When to Use Each Type](#when-to-use-each-type)
- [Migration Guide](#migration-guide)
- [Practical Examples](#practical-examples)

## 🎯 Overview

The new AgentSharp memory architecture separates two distinct concepts to optimize costs and performance:

| Type | Cost | Usage | When to Use |
|------|------|-------|-------------|
| **Message History** | 💰 Low | Conversation log | Simple agents, basic chat |
| **Semantic Memory** | 🚨 High | Intelligent search with AI | Personalized assistants, advanced analysis |

## ❌ Previous Problem

### Legacy Architecture (Expensive)
```csharp
// ❌ OLD: Memory always active by default
var agent = new Agent<Context, string>(model, storage: storage);

// Problems:
// ✗ Every interaction processed embeddings ($$$)
// ✗ Semantic search always active (unnecessary processing)
// ✗ High costs for simple cases
// ✗ No granular feature control
```

### Cost Impact
- **Embedding Processing**: Each message → API call → $0.0001 per 1K tokens
- **Semantic Search**: Memory query → more embeddings → $$
- **Memory Extraction**: LLM processes each interaction → $$$

**Result**: Simple agents cost 80-90% more than necessary.

## ✅ New Architecture

### 1. Separation of Concerns

```
┌─────────────────────────────────────────────────────────────┐
│                    Agent<TContext, TResult>                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────┐    ┌────────────────────────────┐  │
│  │ IMessageHistoryService │  │ ISemanticMemoryService     │  │
│  │ (LOW COST)          │  │ │ (HIGH COST - OPT-IN)       │  │
│  ├─────────────────────┤  │ ├────────────────────────────┤  │
│  │ • Message logging    │  │ │ • Semantic search          │  │
│  │ • Simple history     │  │ │ • AI processing            │  │
│  │ • No processing      │  │ │ • Vector embeddings        │  │
│  │ • Local SQLite       │  │ │ • Automatic extraction     │  │
│  └─────────────────────┘  │ └────────────────────────────┘  │
│                           │                                │
└─────────────────────────────────────────────────────────────┘
```

### 2. Default Agent (Low Cost)
```csharp
// ✅ NEW: Default without extra costs
var agent = new Agent<string, string>(model, "Assistant");

// Automatically included features:
// ✓ Basic message history (BasicMessageHistoryService)
// ✓ No semantic processing (NoOpSemanticMemoryService)
// ✓ Minimal cost - only direct LLM calls
```

### 3. Opt-In Configuration for Advanced Features

```csharp
// Enable only when necessary
var smartAgent = new Agent<Context, string>(model, "Smart Assistant")
    .WithSemanticMemory(storage, embeddingService); // Explicit opt-in
```

## 💰 Cost Comparison

### Practical Example: 100 Interactions per Day

| Scenario | LLM Calls | Embeddings | Cost/Day | Cost/Month |
|----------|-----------|------------|----------|------------|
| **Simple Agent** | 100 | 0 | $0.50 | $15.00 |
| **Semantic Memory** | 100 + 200* | 300** | $2.80 | $84.00 |
| **Difference** | +200% | +∞ | +460% | +460% |

*Extra calls to process and extract memories  
**Embeddings for search + storage

### Detailed Calculation

#### Simple Agent (100 messages/day)
```
• Cost per message: ~$0.005
• Total: 100 × $0.005 = $0.50/day
• Monthly: $15.00
```

#### Semantic Memory (100 messages/day)
```
• Basic messages: 100 × $0.005 = $0.50
• Memory processing: 100 × $0.010 = $1.00
• Embeddings (search): 100 × $0.0001 × 50 = $0.50
• Embeddings (storage): 100 × $0.0001 × 30 = $0.30
• LLM extraction: 100 × $0.005 = $0.50
─────────────────────────────────────────────
• Total: $2.80/day = $84.00/month
```

## 🎯 When to Use Each Type

### Use Message History (Low Cost) when:

- ✅ Simple chat without complex historical context
- ✅ Stateless agents or independent interactions  
- ✅ Prototyping and development
- ✅ Budget-constrained use cases
- ✅ High volume of simple interactions

```csharp
// Ideal examples for Message History
var chatBot = new Agent<string, string>(model, "ChatBot")
    .WithPersona("Simple customer service assistant");

var calculator = new Agent<string, double>(model, "Calculator")
    .WithPersona("Smart calculator");
```

### Use Semantic Memory (High Cost) when:

- 🚨 Long-term personalized assistants
- 🚨 Complex context that evolves over time
- 🚨 Intelligent search in extensive history
- 🚨 Conceptual relationships between information
- 🚨 ROI justifies the investment

```csharp
// Examples that justify Semantic Memory
var personalAssistant = new Agent<UserContext, string>(model, "Personal Assistant")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Personal assistant who knows me deeply");

var researchAssistant = new Agent<ResearchContext, Analysis>(model, "Researcher")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Researcher who maintains context across projects");
```

## 📈 Migration Guide

### Step 1: Evaluate Your Current Agents

```csharp
// ❓ Ask yourself for each agent:
// 1. Does it need to "remember" previous conversations intelligently?
// 2. Is historical context crucial for response quality?
// 3. Is there semantic search for related information?
// 4. Does ROI justify the 4-5x higher cost?
```

### Step 2: Migrate Gradually

#### Simple Agents → Keep Simple
```csharp
// ❌ BEFORE
var oldAgent = new Agent<string, string>(model, storage: storage);

// ✅ AFTER  
var newAgent = new Agent<string, string>(model); // No storage
```

#### Complex Agents → Enable Explicitly
```csharp
// ❌ BEFORE
var oldComplexAgent = new Agent<Context, string>(model, storage: storage);

// ✅ AFTER
var newComplexAgent = new Agent<Context, string>(model)
    .WithSemanticMemory(storage, embeddingService); // Conscious opt-in
```

### Step 3: Monitor Costs

```csharp
// Add logging to track costs
var agent = new Agent<Context, string>(model)
    .WithSemanticMemory(storage, embeddingService)
    .WithLogger(new CostTrackingLogger()); // Cost monitor
```

## 💡 Practical Examples

### Example 1: Customer Support Chat (Low Cost)

```csharp
public class CustomerSupportAgent
{
    public static Agent<SupportContext, string> Create(IModel model)
    {
        return new Agent<SupportContext, string>(model, "Support")
            .WithPersona(@"
                You are an efficient customer support agent.
                - Be direct and helpful
                - Resolve issues quickly
                - Escalate complex cases when necessary
            ")
            .WithTools(new SupportToolPack())
            .WithAnonymousMode(true); // No storage needed
    }
}

// Usage
var supportAgent = CustomerSupportAgent.Create(model);
var response = await supportAgent.ExecuteAsync("How do I cancel my account?");

// Cost: ~$0.005 per interaction
```

### Example 2: Personal Assistant (High Cost)

```csharp
public class PersonalAssistant
{
    public static Agent<PersonalContext, string> Create(
        IModel model, 
        IStorage storage, 
        IEmbeddingService embeddingService)
    {
        return new Agent<PersonalContext, string>(model, "Personal Assistant")
            .WithSemanticMemory(storage, embeddingService)
            .WithPersona(@"
                You are my intelligent personal assistant.
                - Know my preferences and history
                - Anticipate my needs
                - Connect information from different contexts
                - Evolve with me over time
            ")
            .WithTools(new PersonalToolPack())
            .WithReasoning(true);
    }
}

// Usage
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("personal_memory.db", embeddingService);

var personalAssistant = PersonalAssistant.Create(model, storage, embeddingService);

// First conversation
await personalAssistant.ExecuteAsync("I like meetings in the morning, after 9am");

// Weeks later... (automatic semantic search)
var result = await personalAssistant.ExecuteAsync("When should I schedule the client meeting?");
// Response: "Considering your preference for morning meetings after 9am..."

// Cost: ~$0.025 per interaction (5x more expensive, but much more intelligent)
```

### Example 3: Hybrid with Cost Control

```csharp
public class AdaptiveAgent
{
    private readonly Agent<Context, string> _simpleAgent;
    private readonly Agent<Context, string> _smartAgent;

    public AdaptiveAgent(IModel model, IStorage storage, IEmbeddingService embeddingService)
    {
        _simpleAgent = new Agent<Context, string>(model, "Simple");
        
        _smartAgent = new Agent<Context, string>(model, "Smart")
            .WithSemanticMemory(storage, embeddingService);
    }

    public async Task<string> ExecuteAsync(string prompt, Context context)
    {
        // Use simple agent for basic queries
        if (IsSimpleQuery(prompt))
        {
            var result = await _simpleAgent.ExecuteAsync(prompt, context);
            return result.Data;
        }

        // Use smart agent only when necessary
        var smartResult = await _smartAgent.ExecuteAsync(prompt, context);
        return smartResult.Data;
    }

    private bool IsSimpleQuery(string prompt)
    {
        var simpleIndicators = new[] { "how", "when", "what is", "define" };
        return simpleIndicators.Any(indicator => 
            prompt.ToLower().Contains(indicator));
    }
}
```

## 🔮 Architecture Future

### Planned Features

1. **Cost Monitoring Dashboard**
   - Automatic cost tracking per agent
   - Threshold alerts
   - ROI reports

2. **Memory Tiers**
   ```csharp
   .WithMemoryTier(MemoryTier.Basic)     // Message history only
   .WithMemoryTier(MemoryTier.Smart)     // Semantic search
   .WithMemoryTier(MemoryTier.Genius)    // Advanced AI + clustering
   ```

3. **Auto-Scaling Memory**
   ```csharp
   .WithAdaptiveMemory(costBudget: 100.00) // Auto-adjust based on budget
   ```

## 🎯 Final Recommendations

### For Maximum Economy
```csharp
// Use for 80% of cases
var agent = new Agent<string, string>(model)
    .WithAnonymousMode(true);
```

### For Maximum Intelligence
```csharp
// Use only when ROI is clear
var agent = new Agent<Context, string>(model)
    .WithSemanticMemory(storage, embeddingService)
    .WithReasoning(true);
```

### For Balanced Control
```csharp
// Start simple, evolve as needed
var agent = new Agent<Context, string>(model);

// Enable features as value proves itself
if (needsSemanticMemory)
    agent.WithSemanticMemory(storage, embeddingService);

if (needsReasoning)
    agent.WithReasoning(true);
```

---

The new AgentSharp architecture puts **you in control** of costs and features, enabling efficient agents for each specific use case.