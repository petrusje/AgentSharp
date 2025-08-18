# 🤖 AgentSharp

> Framework .NET moderno para criação de agentes de IA com workflows avançados, memória inteligente e orquestração multi-agente

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4)](https://docs.microsoft.com/pt-br/dotnet/standard/net-standard)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-512BD4)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-12.0-239120)](https://docs.microsoft.com/pt-br/dotnet/csharp/)
[![NuGet](https://img.shields.io/badge/NuGet-Available-blue)](https://www.nuget.org/packages/AgentSharp)

## 📋 Visão Geral

O **AgentSharp** é um framework para .NET que facilita a criação de agentes de IA robustos, escaláveis e extensíveis. Com arquitetura otimizada, oferece controle granular sobre custos, sistema de memória avançado com busca vetorial, orquestração de equipes de agentes e uma plataforma flexível para casos de uso empresariais.

## ✨ Características Principais

### 🎯 **Agentes Inteligentes**
- **Agentes Tipados**: Contratos claros com tipos de entrada e saída
- **Raciocínio Estruturado**: Análise step-by-step com reasoning chains
- **Output Estruturado**: Desserialização automática para tipos complexos
- **Ferramentas Extensíveis**: Sistema de plugins via Tool Packs

### 🧠 **Sistema de Memória Avançado**
- **Memória Semântica**: Busca inteligente por contexto com embeddings
- **Busca Vetorial**: Implementação otimizada com sqlite-vec
- **Multiple Storage**: SemanticSqliteStorage (persistent), SemanticMemoryStorage (in-memory)
- **Controle de Custos**: Opções de baixo e alto custo conforme necessidade

### 🤖 **Orquestração Multi-Agente**
- **Team Orchestration**: Coordinate, Route e Collaborate modes
- **Workflows Avançados**: Sequential, Parallel e Conditional
- **Handoff System**: Transferência inteligente entre agentes
- **Load Balancing**: Distribuição automática de cargas

### 🏢 **Pronto para Produção**
- **Modo Anônimo**: Funciona sem sistema de autenticação
- **Observabilidade**: Logging e métricas integrados
- **Performance**: Arquitetura otimizada para alto desempenho
- **Extensibilidade**: APIs abertas para customização empresarial

## 🚀 Início Rápido

### Instalação

```bash
dotnet add package AgentSharp
```

### Agente Básico

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Configurar modelo
var model = new ModelFactory().CreateModel("openai", new ModelOptions
{
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    ModelName = "gpt-4"
});

// Criar agente
var agent = new Agent<string, string>(model, "Assistente")
    .WithPersona("Você é um assistente útil e direto");

// Executar
var resultado = await agent.ExecuteAsync("Como posso ajudar?");
Console.WriteLine(resultado.Data);
```

### Agente com Memória Vetorial

```csharp
// Configurar storage com busca vetorial otimizada
var embeddingService = new OpenAIEmbeddingService(apiKey);
var storage = new SemanticSqliteStorage("Data Source=memoria.db", embeddingService, 1536);
await storage.InitializeAsync();

// Criar agente com memória semântica
var agent = new Agent<Context, string>(model, "Assistente Inteligente")
    .WithSemanticMemory(storage, embeddingService)
    .WithPersona("Assistente que lembra do contexto das conversas e aprende com cada interação")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });

var resultado = await agent.ExecuteAsync("Prefiro café forte pela manhã, mas leve à tarde");
```

### Workflow Multi-Agentes

```csharp
// Definir contexto
public class AnaliseContext
{
    public string Entrada { get; set; }
    public string Dados { get; set; }
    public string Resultado { get; set; }
}

// Criar agentes especializados
var coletor = new Agent<AnaliseContext, string>(model, "Coletor");
var analista = new Agent<AnaliseContext, string>(model, "Analista");

// Configurar workflow
var workflow = new SequentialWorkflow<AnaliseContext, string>("Análise")
    .RegisterStep("Coleta", coletor,
        ctx => $"Colete dados sobre: {ctx.Entrada}",
        (ctx, res) => ctx.Dados = res)
    .RegisterStep("Análise", analista,
        ctx => $"Analise os dados: {ctx.Dados}",
        (ctx, res) => ctx.Resultado = res);

// Executar
var contexto = new AnaliseContext { Entrada = "mercado financeiro" };
var resultado = await workflow.ExecuteAsync(contexto);
```

### Output Estruturado

```csharp
public class Relatorio
{
    public string Titulo { get; set; }
    public List<string> Pontos { get; set; }
    public double Confianca { get; set; }
}

// Agente com output tipado
var gerador = new Agent<string, Relatorio>(model, "Gerador de Relatório");
var relatorio = await gerador.ExecuteAsync("Crie um relatório sobre IA");

Console.WriteLine($"Título: {relatorio.Data.Titulo}");
Console.WriteLine($"Confiança: {relatorio.Data.Confianca:P}");
```

### Orquestração de Equipes de Agentes

```csharp
// Configurar contexto para equipe
public class ProjetoContext
{
    public string Requisitos { get; set; }
    public string UserId { get; set; }
    public string SessionId { get; set; }
}

// Criar agentes especializados
var arquiteto = new Agent<ProjetoContext, string>(model, "Arquiteto")
    .WithPersona("Especialista em arquitetura de software")
    .WithSemanticMemory(storage);

var desenvolvedor = new Agent<ProjetoContext, string>(model, "Desenvolvedor") 
    .WithPersona("Desenvolvedor full-stack experiente")
    .WithSemanticMemory(storage);

var revisor = new Agent<ProjetoContext, string>(model, "Revisor")
    .WithPersona("Especialista em code review e qualidade")
    .WithSemanticMemory(storage);

// Configurar orquestração em modo Coordinate
var team = new TeamOrchestrator<ProjetoContext, string>()
    .AddAgent("arquiteto", arquiteto)
    .AddAgent("desenvolvedor", desenvolvedor) 
    .AddAgent("revisor", revisor)
    .WithMode(TeamMode.Coordinate)
    .WithCoordinator("arquiteto")
    .WithHandoffRules("desenvolvimento -> revisao -> entrega");

// Executar projeto colaborativo
var contexto = new ProjetoContext 
{ 
    Requisitos = "Sistema de gestão de pedidos",
    UserId = "pm001",
    SessionId = Guid.NewGuid().ToString()
};

var resultado = await team.ExecuteAsync("Desenvolver sistema completo", contexto);
```

## 🧠 Sistema de Memória Avançado

O AgentSharp oferece arquitetura de memória otimizada com duas implementações principais:

### 🏃‍♂️ **SemanticSqliteStorage** (Recomendado para Produção)
- **Performance**: Busca vetorial nativa com sqlite-vec
- **Escalabilidade**: Suporta milhões de embeddings
- **Complexidade**: O(log n) para consultas de similaridade  
- **Caso de uso**: Sistemas empresariais com grandes volumes de dados

### 💡 **SemanticMemoryStorage** (Ideal para Desenvolvimento)
- **Performance**: HNSW em memória otimizado
- **Rapidez**: Inicialização instantânea para testes
- **Limitações**: Adequado para datasets menores
- **Caso de uso**: Desenvolvimento, prototipação e testes

```csharp
// Produção - com sqlite-vec
var prodStorage = new SemanticSqliteStorage(
    connectionString: "Data Source=production.db",
    embeddingService: new OpenAIEmbeddingService(apiKey),
    dimensions: 1536
);

// Desenvolvimento - HNSW em memória  
var devStorage = new SemanticMemoryStorage(
    new CompactHNSWConfiguration
    {
        Dimensions = 384, // Dimensões reduzidas para desenvolvimento
        MaxConnections = 16,
        SearchK = 200
    }
);

// Agente adaptativo para diferentes ambientes
var agent = new Agent<Context, string>(model, "Assistant")
    .WithSemanticMemory(Environment.IsProduction() ? prodStorage : devStorage)
    .WithPersona("Assistente que se adapta ao ambiente de execução");
```

## 🔧 Ferramentas e Extensibilidade

```csharp
// Tool customizada
public class CalculadoraTool : ITool
{
    public string Name => "calculadora";
    public string Description => "Realiza cálculos matemáticos";
    
    public async Task<string> ExecuteAsync(string input)
    {
        // Implementação da calculadora
        return RealizarCalculo(input);
    }
}

// Usar com agente
var agente = new Agent<string, string>(model, "Matemático")
    .WithTools(new MathToolPack())
    .AddTool(new CalculadoraTool());
```

## 📚 Documentação Completa

### 📖 Português (pt-BR)
- [🚀 Início Rápido](docs/pt-BR/getting-started.md) - Primeiros passos e instalação
- [🎯 Conceitos Fundamentais](docs/pt-BR/core-concepts.md) - Arquitetura e conceitos base
- [🧠 Sistema de Memória](docs/pt-BR/memory-system.md) - Guia completo de memória e storage
- [🤖 API do Agent](docs/pt-BR/api/core/agent.md) - Referência completa da API
- [🔄 Workflows Avançados](docs/pt-BR/workflows.md) - Orquestração e multi-agentes
- [🧮 Raciocínio Estruturado](docs/pt-BR/reasoning.md) - Sistema de reasoning
- [📊 Exemplos Práticos](docs/pt-BR/examples.md) - Casos de uso reais
- [🏢 Casos Empresariais](docs/pt-BR/enterprise.md) - Implementação em produção

### 📖 English (en-US)  
- [🚀 Quick Start](docs/en-US/getting-started.md) - Installation and first steps
- [🎯 Core Concepts](docs/en-US/core-concepts.md) - Architecture and base concepts
- [🧠 Memory System](docs/en-US/memory-system.md) - Complete memory and storage guide
- [🤖 Agent API](docs/en-US/api/core/agent.md) - Complete API reference
- [🔄 Advanced Workflows](docs/en-US/workflows.md) - Orchestration and multi-agents
- [🧮 Structured Reasoning](docs/en-US/reasoning.md) - Reasoning system
- [📊 Practical Examples](docs/en-US/examples.md) - Real-world use cases
- [🏢 Enterprise Cases](docs/en-US/enterprise.md) - Production implementation

### 🎮 Console de Exemplos
Explore **27+ exemplos interativos** no projeto `AgentSharp.console`:
- **Nível 1**: Fundamentos (Agentes básicos, personalidade, tools)  
- **Nível 2**: Intermediário (Reasoning, outputs estruturados, memória)
- **Nível 3**: Avançado (Workflows, busca semântica, sistemas empresariais)
- **Especializados**: Assistentes médicos, jurídicos, consultores técnicos
- **Team Orchestration**: Coordenação de equipes de agentes

```bash
# Executar console interativo
cd AgentSharp.console
dotnet run
```

## 🎯 Casos de Uso Empresariais

### 🏥 **Saúde e Medicina**
- **Assistentes Médicos**: Diagnóstico auxiliado com histórico do paciente
- **Análise de Exames**: Processamento estruturado de laudos médicos
- **Gestão Hospitalar**: Otimização de fluxos e recursos

### ⚖️ **Jurídico e Compliance**
- **Consultoria Jurídica**: Análise de contratos e documentos legais
- **Due Diligence**: Auditoria automatizada de documentos empresariais
- **Compliance**: Monitoramento de conformidade regulatória

### 🏢 **Empresarial e Financeiro**
- **Assistentes Virtuais**: Atendimento ao cliente de alta qualidade
- **Análise Financeira**: Processamento de dados com contexto histórico
- **Gestão de Conhecimento**: Base de conhecimento empresarial inteligente

### 🔧 **Tecnologia e Desenvolvimento**
- **Code Review**: Revisão automatizada de código com contexto
- **DevOps**: Automação de workflows complexos com tomada de decisão
- **Documentação**: Geração automática de documentação técnica

### 🎓 **Educação e Treinamento**
- **Tutores Virtuais**: Ensino personalizado com histórico do estudante
- **Avaliação**: Correção e feedback automatizado
- **Pesquisa Acadêmica**: Análise de literatura científica

## 🛠️ Arquitetura Moderna

```
┌──────────────────────┐
│   Team Orchestrator  │  🤖 Orquestração Multi-Agente
├──────────────────────┤  ├─ Coordinate Mode
│ • TeamMode.Route     │  ├─ Route Mode  
│ • TeamMode.Coordinate│  └─ Collaborate Mode
│ • TeamMode.Collaborate│
└──────┬───────────────┘
       │
┌──────▼───────────────┐
│       Agent          │  🎯 Agente Inteligente
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
│ • Semantic Search    │◄──►│ • VectorSqliteVec   │  🧠 Sistema de Memória
│ • Context Loading    │    │ • CompactHNSW       │     Otimizado
│ • Smart Summarization│    │ • Embedding Service │
│ • Cost Control       │    │ • Similarity Search │
└──────────────────────┘    └─────────────────────┘
       │
┌──────▼───────────────┐    ┌─────────────────────┐
│     Workflows        │    │   Model Factory     │
├──────────────────────┤    ├─────────────────────┤
│ • Sequential         │    │ • OpenAI GPT-4      │  ⚡ Extensibilidade
│ • Parallel           │    │ • Azure OpenAI      │     e Integração
│ • Conditional        │    │ • Custom Models     │
│ • Advanced Routing   │    │ • Provider Agnostic │
└──────────────────────┘    └─────────────────────┘
```

## 📦 Storage Providers Otimizados

### ✅ **Implementações Recomendadas (Pós-Otimização)**
- **🏃‍♂️ SemanticSqliteStorage**: Busca vetorial nativa de alta performance
- **💡 SemanticMemoryStorage**: HNSW otimizado para desenvolvimento

### ❌ **Implementações Removidas (Redundantes/Problemáticas)**
- ~~SqliteStorage~~ - Substituído por SemanticSqliteStorage
- ~~VectorSqliteStorage~~ - Performance inadequada  
- ~~HNSWMemoryStorage~~ - Complexidade desnecessária
- ~~InMemoryStorage~~ - Limitações de busca semântica

> **🎯 Resultado da Otimização**: 60% redução na complexidade com melhoria significativa de performance

## ⚙️ Configuração

### Variáveis de Ambiente

```bash
# API Configuration
OPENAI_API_KEY=sua-chave-aqui
OPENAI_ENDPOINT=https://api.openai.com  # opcional

# Model Configuration  
MODEL_NAME=gpt-4o-mini  # ou gpt-4, gpt-3.5-turbo
TEMPERATURE=0.7         # 0.0 a 1.0
MAX_TOKENS=2048        # limite de tokens

# Memory Configuration
EMBEDDING_MODEL=text-embedding-3-small  # ou text-embedding-ada-002
VECTOR_DIMENSIONS=1536                   # dimensões do modelo de embedding
```

### Configuração Programática

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

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. Faça fork do projeto
2. Crie uma branch para sua feature
3. Adicione testes para novas funcionalidades
4. Mantenha a documentação atualizada
5. Envie um Pull Request

## 📄 Licença

Este projeto está licenciado sob a Licença MIT. Veja [LICENSE](LICENSE) para mais detalhes.

## 🙏 Créditos

- [OpenAI](https://openai.com) pela API de modelos de linguagem
- [sqlite-vec](https://github.com/asg017/sqlite-vec) pela extensão de busca vetorial
- Comunidade .NET pelo ecossistema robusto

---

**AgentSharp** - Framework .NET para agentes de IA modernos