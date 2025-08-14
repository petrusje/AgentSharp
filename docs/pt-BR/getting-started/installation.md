# Instalação

AgentSharp é um framework .NET abrangente para construção de agentes de IA prontos para produção com gerenciamento avançado de memória, integração de ferramentas e capacidades de orquestração de equipes.

## Requisitos

- .NET Standard 2.0+ ou .NET Core 3.0+
- C# 7.3+
- Plataformas suportadas: Windows, macOS, Linux

## Instalação

### Pacote NuGet

```bash
dotnet add package AgentSharp
```

### Console do Gerenciador de Pacotes

```powershell
Install-Package AgentSharp
```

## Pacotes de Provedores Opcionais

AgentSharp usa uma arquitetura modular de provedores. Instale apenas os provedores que você precisa:

### Provedores de Modelos

```bash
# Modelos OpenAI (GPT-4, GPT-3.5, etc.)
dotnet add package AgentSharp.Providers.OpenAI

# Modelos locais via Ollama
dotnet add package AgentSharp.Providers.Ollama
```

### Provedores de Armazenamento

```bash
# SQLite com suporte a vetores para memória persistente
dotnet add package AgentSharp.Storage.SQLite

# Armazenamento em memória para desenvolvimento/testes
dotnet add package AgentSharp.Storage.Memory
```

### Provedores de Embeddings

```bash
# Embeddings OpenAI (text-embedding-3-small, etc.)
dotnet add package AgentSharp.Embeddings.OpenAI

# Embeddings locais via Ollama
dotnet add package AgentSharp.Embeddings.Ollama
```

## Configuração Básica

### 1. Registro da Injeção de Dependência

```csharp
using AgentSharp.Extensions.DependencyInjection;

// No Program.cs ou Startup.cs
services.AddAgentSharp(builder => builder
    .WithOpenAI(Configuration["OpenAI:ApiKey"])
    .WithSqliteStorage(":memory:")  // ou caminho do arquivo
    .WithTelemetry<ConsoleTelemetryService>()
);
```

### 2. Criação Básica de Agente

```csharp
using AgentSharp.Core;

// Agente simples
var agente = new Agent<string, string>(model, "MeuAgente")
    .WithPersona("Você é um assistente útil")
    .WithInstructions("Sempre seja conciso e preciso");

var resultado = await agente.ExecuteAsync("Olá, como está?", context: "");
Console.WriteLine(resultado.Content);
```

### 3. Agente com Memória

```csharp
// Agente com memória persistente
var agente = new Agent<UserContext, string>(model, "AgenteMemoria")
    .WithSemanticMemory(sqliteStorage, embeddingService)
    .WithUserMemories(enable: true)
    .WithMemorySearch(enable: true);

var context = new UserContext { UserId = "user123", SessionId = Guid.NewGuid().ToString() };
var resultado = await agente.ExecuteAsync("Lembre-se que prefiro explicações técnicas", context);
```

## Configuração

### Variáveis de Ambiente

Crie um arquivo `.env` ou defina variáveis de ambiente:

```bash
OPENAI_API_KEY=sua_chave_openai_aqui
OLLAMA_BASE_URL=http://localhost:11434
```

### appsettings.json

```json
{
  "AgentSharp": {
    "OpenAI": {
      "ApiKey": "sua_api_key",
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

## Verificação

Execute os exemplos incluídos para verificar a instalação:

```bash
dotnet run --project Examples
```

Isso abrirá um menu interativo mostrando as capacidades do AgentSharp.

## Próximos Passos

- [Guia de Início Rápido](../tutorials/quick-start.md) - Construa seu primeiro agente
- [Conceitos Centrais](../core-concepts/agents.md) - Entenda a arquitetura
- [Exemplos](../examples/basic-usage.md) - Veja implementações práticas