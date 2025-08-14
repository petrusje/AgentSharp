# Installation

AgentSharp is a comprehensive .NET framework for building production-ready AI agents with advanced memory management, tool integration, and team orchestration capabilities.

## Requirements

- .NET Standard 2.0+ or .NET Core 3.0+
- C# 7.3+
- Supported platforms: Windows, macOS, Linux

## Installation

### NuGet Package

```bash
dotnet add package AgentSharp
```

### Package Manager Console

```powershell
Install-Package AgentSharp
```

## Optional Provider Packages

AgentSharp uses a modular provider architecture. Install only the providers you need:

### Model Providers

```bash
# OpenAI models (GPT-4, GPT-3.5, etc.)
dotnet add package AgentSharp.Providers.OpenAI

# Local models via Ollama
dotnet add package AgentSharp.Providers.Ollama
```

### Storage Providers

```bash
# SQLite with vector support for persistent memory
dotnet add package AgentSharp.Storage.SQLite

# In-memory storage for development/testing
dotnet add package AgentSharp.Storage.Memory
```

### Embedding Providers

```bash
# OpenAI embeddings (text-embedding-3-small, etc.)
dotnet add package AgentSharp.Embeddings.OpenAI

# Local embeddings via Ollama
dotnet add package AgentSharp.Embeddings.Ollama
```

## Basic Setup

### 1. Dependency Injection Registration

```csharp
using AgentSharp.Extensions.DependencyInjection;

// In Program.cs or Startup.cs
services.AddAgentSharp(builder => builder
    .WithOpenAI(Configuration["OpenAI:ApiKey"])
    .WithSqliteStorage(":memory:")  // or file path
    .WithTelemetry<ConsoleTelemetryService>()
);
```

### 2. Basic Agent Creation

```csharp
using AgentSharp.Core;

// Simple agent
var agent = new Agent<string, string>(model, "MyAgent")
    .WithPersona("You are a helpful assistant")
    .WithInstructions("Always be concise and accurate");

var result = await agent.ExecuteAsync("Hello, how are you?", context: "");
Console.WriteLine(result.Content);
```

### 3. Memory-Enabled Agent

```csharp
// Agent with persistent memory
var agent = new Agent<UserContext, string>(model, "MemoryAgent")
    .WithSemanticMemory(sqliteStorage, embeddingService)
    .WithUserMemories(enable: true)
    .WithMemorySearch(enable: true);

var context = new UserContext { UserId = "user123", SessionId = Guid.NewGuid().ToString() };
var result = await agent.ExecuteAsync("Remember that I prefer technical explanations", context);
```

## Configuration

### Environment Variables

Create a `.env` file or set environment variables:

```bash
OPENAI_API_KEY=your_openai_api_key_here
OLLAMA_BASE_URL=http://localhost:11434
```

### appsettings.json

```json
{
  "AgentSharp": {
    "OpenAI": {
      "ApiKey": "your_api_key",
      "Endpoint": "https://api.openai.com"
    },
    "Ollama": {
      "BaseUrl": "http://localhost:11434"
    },
    "Storage": {
      "UseSqlite": true,
      "SqliteConnectionString": "Data Source=agents.db"
    },
    "Telemetry": {
      "Enabled": true,
      "Provider": "console"
    }
  }
}
```

## Verification

Run the included examples to verify installation:

```bash
dotnet run --project Examples
```

This will launch an interactive menu showcasing AgentSharp's capabilities.

## Next Steps

- [Quick Start Guide](../tutorials/quick-start.md) - Build your first agent
- [Core Concepts](../core-concepts/agents.md) - Understand the architecture
- [Examples](../examples/basic-usage.md) - See practical implementations