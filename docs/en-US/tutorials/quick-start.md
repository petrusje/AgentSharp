# Quick Start Guide

This guide will help you build your first AgentSharp agent in just a few minutes.

## Prerequisites

- AgentSharp installed ([Installation Guide](../getting-started/installation.md))
- OpenAI API key (for this example)

## Step 1: Create Your First Agent

Let's create a simple customer service agent:

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Create model with your API key
var model = new OpenAIModel("gpt-4o-mini", "your_api_key_here");

// Create agent with typed context and result
var agent = new Agent<CustomerContext, CustomerResponse>(model, "CustomerServiceAgent")
    .WithPersona("You are a friendly customer service representative")
    .WithInstructions(ctx => $"Help {ctx.CustomerName} with their inquiry about {ctx.ProductType}")
    .WithGuardRails("Always be polite and professional. Never make promises about refunds without manager approval.");

// Define your context and response types
public class CustomerContext
{
    public string CustomerName { get; set; }
    public string ProductType { get; set; }
    public string IssueDescription { get; set; }
}

public class CustomerResponse
{
    public string Response { get; set; }
    public string Sentiment { get; set; }
    public bool RequiresEscalation { get; set; }
    public List<string> SuggestedActions { get; set; }
}
```

## Step 2: Execute the Agent

```csharp
// Create context for the interaction
var context = new CustomerContext
{
    CustomerName = "John Smith",
    ProductType = "Premium Subscription",
    IssueDescription = "Cannot access advanced features after payment"
};

// Execute the agent
var result = await agent.ExecuteAsync(
    "I paid for the premium subscription yesterday but I still can't access the advanced features. Can you help?",
    context
);

// AgentSharp automatically extracts structured data
Console.WriteLine($"Response: {result.Content.Response}");
Console.WriteLine($"Sentiment: {result.Content.Sentiment}");
Console.WriteLine($"Needs Escalation: {result.Content.RequiresEscalation}");
Console.WriteLine($"Suggested Actions: {string.Join(", ", result.Content.SuggestedActions)}");
```

## Step 3: Add Memory (Optional)

Make your agent remember past interactions:

```csharp
using AgentSharp.Core.Memory.Services;
using AgentSharp.Embeddings;

// Set up storage and embeddings
var storage = new SemanticSqliteStorage("customers.db", embeddingService);
var embeddingService = new OpenAIEmbeddingService("text-embedding-3-small", "your_api_key");

// Create memory-enabled agent
var agent = new Agent<CustomerContext, CustomerResponse>(model, "CustomerServiceAgent")
    .WithPersona("You are a friendly customer service representative")
    .WithSemanticMemory(storage, embeddingService)
    .WithUserMemories(enable: true)      // Remember important customer info
    .WithMemorySearch(enable: true)      // Search past interactions
    .WithHistoryToMessages(enable: true); // Include conversation history

// Now the agent will remember this customer across sessions
```

## Step 4: Add Tools

Extend your agent with callable functions:

```csharp
public class CustomerServiceToolPack : ToolPack
{
    [FunctionCall("Check subscription status for a customer")]
    [FunctionCallParameter("customerId", "The customer's unique identifier")]
    public async Task<SubscriptionStatus> CheckSubscriptionStatus(string customerId)
    {
        // Your business logic here
        return await SubscriptionService.GetStatusAsync(customerId);
    }

    [FunctionCall("Create support ticket")]
    [FunctionCallParameter("issue", "Description of the customer issue")]
    [FunctionCallParameter("priority", "Priority level: low, medium, high, urgent")]
    public async Task<string> CreateSupportTicket(string issue, string priority)
    {
        // Your ticketing system integration
        var ticket = await TicketingSystem.CreateAsync(issue, priority);
        return $"Ticket {ticket.Id} created successfully";
    }
}

// Register the tools
var agent = new Agent<CustomerContext, CustomerResponse>(model, "CustomerServiceAgent")
    .WithPersona("You are a friendly customer service representative")
    .RegisterToolPack(new CustomerServiceToolPack());
```

## Step 5: Monitor Performance

Track your agent's performance with telemetry:

```csharp
using AgentSharp.Core.Telemetry;

// Enable telemetry
var telemetry = new ConsoleTelemetryService();
telemetry.Enable();

// Your agent execution code here...

// View performance metrics
var summary = telemetry.GetSummary();
Console.WriteLine($"Total tokens used: {summary.TotalTokens}");
Console.WriteLine($"LLM calls: {summary.LLMEvents}");
Console.WriteLine($"Average response time: {summary.AverageElapsedTime:F2}ms");
```

## Complete Example

Here's a complete working example:

```csharp
using AgentSharp.Core;
using AgentSharp.Models;
using AgentSharp.Core.Telemetry;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup
        var model = new OpenAIModel("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        var telemetry = new ConsoleTelemetryService();
        telemetry.Enable();

        // Create agent
        var agent = new Agent<CustomerContext, CustomerResponse>(model, "CustomerServiceAgent")
            .WithPersona("You are a friendly customer service representative")
            .WithInstructions(ctx => $"Help {ctx.CustomerName} with their {ctx.ProductType} inquiry")
            .WithGuardRails("Always be polite. Escalate billing issues to management.");

        // Execute
        var context = new CustomerContext
        {
            CustomerName = "Jane Doe",
            ProductType = "Premium Plan",
            IssueDescription = "Billing discrepancy"
        };

        var result = await agent.ExecuteAsync(
            "I was charged twice for my premium plan this month. Can you help?",
            context
        );

        // Results
        Console.WriteLine($"Agent Response: {result.Content.Response}");
        Console.WriteLine($"Escalation Needed: {result.Content.RequiresEscalation}");
        
        // Performance metrics
        var metrics = telemetry.GetSummary();
        Console.WriteLine($"Tokens Used: {metrics.TotalTokens}");
    }
}

public class CustomerContext
{
    public string CustomerName { get; set; }
    public string ProductType { get; set; }
    public string IssueDescription { get; set; }
}

public class CustomerResponse
{
    public string Response { get; set; }
    public bool RequiresEscalation { get; set; }
    public List<string> SuggestedActions { get; set; } = new List<string>();
}
```

## What's Next?

- [Core Concepts](../core-concepts/agents.md) - Deep dive into AgentSharp architecture
- [Memory Management](../core-concepts/memory.md) - Advanced memory features
- [Tools & Functions](../core-concepts/tools.md) - Building custom tool packs
- [Team Orchestration](../core-concepts/orchestration.md) - Multi-agent workflows
- [Advanced Examples](../examples/) - Real-world scenarios