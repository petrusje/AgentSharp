# Agentes no AgentSharp

Os Agentes são o núcleo do framework AgentSharp. São entidades inteligentes que encapsulam modelos de linguagem, ferramentas, memória e lógica de execução, proporcionando uma interface de alto nível para construção de sistemas de IA.

## O que é um Agente?

Um **Agente** no AgentSharp é uma classe genérica `Agent<TContext, TResult>` que:

- 🧠 **Encapsula um Modelo de IA** (OpenAI, Azure, Ollama, etc.)
- 🛠️ **Gerencia Ferramentas** (function calling, APIs externas)
- 💾 **Controla Memória** (conversação, conhecimento persistente)
- 🔄 **Executa Workflows** (raciocínio, processamento complexo)
- 📊 **Monitora Performance** (telemetria, métricas)

## Arquitetura de um Agente

```
┌─────────────────────────────────────────────────────────────┐
│                    Agent<TContext, TResult>                 │
├─────────────────────────────────────────────────────────────┤
│  🧠 ExecutionEngine  │  📝 PromptManager  │  🛠️ ToolManager   │
│  └─ IModel           │  └─ System Prompt  │  └─ Function Call │
│  └─ ModelConfig      │  └─ Instructions   │  └─ ToolPacks     │
├─────────────────────────────────────────────────────────────┤
│  💾 MemoryManager    │  📊 TelemetryService │ ⚙️ Configuration │
│  └─ IStorage         │  └─ Token Tracking  │  └─ Temperature  │
│  └─ Embeddings       │  └─ Performance     │  └─ Max Tokens   │
│  └─ Semantic Search  │  └─ Events          │  └─ Penalties    │
└─────────────────────────────────────────────────────────────┘
```

## Tipos de Agente

### 1. Agente Básico

O tipo mais simples de agente para interações diretas:

```csharp
// Agente básico sem contexto específico
var agente = new Agent<object, string>(modelo, "AssistenteBasico");

var resposta = await agente.ExecuteAsync("Explique o que é inteligência artificial");
Console.WriteLine(resposta.Data); // string com a resposta
```

### 2. Agente com Contexto Tipado

Agente que trabalha com estruturas de dados específicas:

```csharp
public class ContextoFinanceiro
{
    public decimal Orcamento { get; set; }
    public string Moeda { get; set; }
    public List<string> Categorias { get; set; }
}

public class RelatorioFinanceiro
{
    public decimal TotalGasto { get; set; }
    public Dictionary<string, decimal> GastorPorCategoria { get; set; }
    public List<string> Recomendacoes { get; set; }
}

// Agente especializado em finanças
var agenteFinanceiro = new Agent<ContextoFinanceiro, RelatorioFinanceiro>(
    modelo, 
    "AnalistaFinanceiro"
);

var contexto = new ContextoFinanceiro
{
    Orcamento = 5000m,
    Moeda = "BRL",
    Categorias = new[] { "Alimentação", "Transporte", "Lazer" }.ToList()
};

var relatorio = await agenteFinanceiro.ExecuteAsync(
    "Analise meus gastos e me dê recomendações", 
    contexto
);

// relatorio é do tipo RelatorioFinanceiro
Console.WriteLine($"Total gasto: {relatorio.Data.TotalGasto:C}");
```

### 3. Agente com Memória

Agente que mantém contexto entre conversas:

```csharp
using AgentSharp.Core.Memory.Storage.SqliteVector;
using AgentSharp.Core.Memory.Services.Embedding;

// Configurar armazenamento de memória
var embeddingService = new OpenAIEmbeddingService("sua-api-key");
var storage = new SemanticSqliteStorage(
    "Data Source=memoria.db", 
    embeddingService, 
    1536
);

// Criar agente com memória
var agenteComMemoria = new Agent<object, string>(
    model: modelo,
    name: "AssistenteComMemoria",
    storage: storage
);

// Habilitar controles granulares
agenteComMemoria
    .WithUserMemories()              // LLM pode criar memórias
    .WithMemorySearch(true)          // Busca semântica ativa
    .WithHistoryToMessages(true);    // Histórico de conversas

// Primeira conversa
await agenteComMemoria.ExecuteAsync(
    "Meu nome é João e gosto de café forte", 
    "user123", 
    "sessao1"
);

// Segunda conversa (lembra do usuário)
var resposta = await agenteComMemoria.ExecuteAsync(
    "Que tipo de café você me recomenda?", 
    "user123", 
    "sessao2"
);
// O agente lembrará que João gosta de café forte
```

## Configurações do Agente

### Configuração do Modelo

```csharp
var config = new ModelConfiguration
{
    ModelName = "gpt-4o-mini",
    Temperature = 0.7,              // Criatividade (0.0-1.0)
    MaxTokens = 2048,              // Limite de resposta
    FrequencyPenalty = 0.1,        // Reduz repetições
    PresencePenalty = 0.1,         // Incentiva novos tópicos
    TopP = 0.9,                    // Diversidade de vocabulário
    TimeoutSeconds = 60            // Timeout da requisição
};

var agente = new Agent<object, string>(
    model: modelo, 
    name: "AgenteConfigurado",
    modelConfig: config
);
```

### Configuração de Memória

```csharp
var memoryConfig = new MemoryConfiguration
{
    MaxMemories = 100,             // Limite de memórias por usuário
    MaxExecutions = 50,            // Limite de execuções lembradas
    MinRelevanceScore = 0.7,       // Relevância mínima para busca
    EnableAutoSummary = true,      // Resumos automáticos
    MinEntriesForSummary = 20      // Mín. entradas para resumo
};

var agente = new Agent<object, string>(
    model: modelo,
    name: "AgenteComMemoriaConfig",
    memoryConfig: memoryConfig
);
```

### Configuração de Domínio de Memória

```csharp
var domainConfig = new MemoryDomainConfiguration
{
    SaveMemoryPrompt = "Extraia informações importantes sobre o usuário:",
    SearchMemoryPrompt = "Busque memórias relevantes para:",
    ClassifyMemoryPrompt = "Classifique esta informação como:",
    Domain = "Assistente Médico"
};
```

## Interface Fluente

O AgentSharp oferece uma interface fluente para configuração elegante:

```csharp
var agente = new Agent<ContextoEmpresarial, RelatorioAnalise>(modelo, "AnalistaEmpresarial")
    .WithPersona("Você é um consultor empresarial sênior especializado em análise estratégica")
    .WithInstructions("Sempre forneça dados concretos e recomendações acionáveis")
    .WithUserMemories()
    .WithMemorySearch(true)
    .WithHistoryToMessages(10)      // Últimas 10 mensagens no contexto
    .WithKnowledgeSearch(false)     // Desabilita RAG externo
    .WithReasoning(modelo, 3, 10)   // Ativa raciocínio (min 3, max 10 passos)
    .WithTelemetry(telemetryService);

// Configurar ferramentas específicas
agente.RegisterToolPack(new FinanceToolPack());
agente.RegisterToolPack(new SearchToolPack());
```

## Controles Granulares

O AgentSharp permite controle fino sobre funcionalidades custosas:

```csharp
public class Agent<TContext, TResult>
{
    // 🎯 Controle de Memória
    public bool EnableUserMemories { get; set; } = false;    // LLM pode criar memórias
    public bool EnableMemorySearch { get; set; } = false;    // Busca semântica ativa
    
    // 🎯 Controle de Histórico  
    public bool AddHistoryToMessages { get; set; } = false;  // Adiciona histórico ao contexto
    public int NumHistoryMessages { get; set; } = 10;        // Quantidade de mensagens
    
    // 🎯 Controle de RAG
    public bool EnableKnowledgeSearch { get; set; } = false; // Busca em base de conhecimento
}

// Configurações de custo/funcionalidade
var agenteBaixoCusto = new Agent<object, string>(modelo, "EconomicoBot")
    .WithUserMemories(false)        // ❌ Não gera memórias (economiza tokens)
    .WithMemorySearch(false)        // ❌ Não busca memórias (economiza processamento)
    .WithHistoryToMessages(false);  // ❌ Não adiciona histórico (economiza tokens)

var agenteCompleto = new Agent<object, string>(modelo, "PremiumBot")
    .WithUserMemories()             // ✅ Gera e gerencia memórias
    .WithMemorySearch(true)         // ✅ Busca semântica ativa
    .WithHistoryToMessages(true);   // ✅ Contexto completo disponível
```

## Ciclo de Vida de Execução

```
1. 📝 Preparação do Prompt
   ├─ Sistema prompt base
   ├─ Instruções específicas
   ├─ Contexto do usuário (TContext)
   └─ Histórico de mensagens (se habilitado)

2. 🔍 Enriquecimento com Memória
   ├─ Busca memórias relevantes (se habilitado)
   ├─ Busca conhecimento/RAG (se habilitado)
   └─ Adiciona informações ao contexto

3. 🛠️ Registro de Ferramentas
   ├─ Tools automáticas do agente
   ├─ SmartMemoryToolPack (se habilitado)
   └─ ToolPacks customizados

4. 🧠 Execução do Modelo
   ├─ Processamento pelo LLM
   ├─ Function calling (se necessário)
   └─ Raciocínio (se habilitado)

5. 💾 Pós-processamento
   ├─ Extração de memórias (se habilitado)
   ├─ Salvamento de histórico
   ├─ Telemetria e métricas
   └─ Retorno tipado (TResult)
```

## Exemplo Completo: Agente Empresarial

```csharp
using AgentSharp.Core;
using AgentSharp.Models;
using AgentSharp.Tools;

public class ContextoEmpresarial
{
    public string NomeEmpresa { get; set; }
    public string Setor { get; set; }
    public decimal ReceitaAnual { get; set; }
    public int NumeroFuncionarios { get; set; }
    public List<string> PrincipaisDesafios { get; set; }
}

public class RelatorioAnalise
{
    public string ResumoExecutivo { get; set; }
    public List<string> PontosFortes { get; set; }
    public List<string> PontosDeAtencao { get; set; }
    public List<string> Recomendacoes { get; set; }
    public Dictionary<string, decimal> MetricasProjetadas { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Configuração do modelo
        var modelo = new OpenAIModel(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        
        // Configuração de memória (opcional)
        var embeddingService = new OpenAIEmbeddingService("sua-api-key");
        var storage = new SemanticSqliteStorage("Data Source=empresarial.db", embeddingService, 1536);
        
        // Criar agente especializado
        var consultor = new Agent<ContextoEmpresarial, RelatorioAnalise>(modelo, "ConsultorEmpresarial")
            .WithPersona(@"
                Você é um consultor empresarial sênior com 20 anos de experiência.
                Especialista em análise estratégica, finanças corporativas e transformação digital.
                Sempre base suas recomendações em dados concretos e melhores práticas do mercado.
            ")
            .WithInstructions(@"
                1. Analise todos os aspectos do contexto empresarial fornecido
                2. Identifique pontos fortes e áreas de oportunidade
                3. Forneça recomendações específicas e acionáveis
                4. Inclua projeções financeiras quando relevante
                5. Mantenha tom profissional mas acessível
            ")
            .WithUserMemories()                        // Lembrará de empresas anteriores
            .WithMemorySearch(true)                    // Buscará casos similares
            .WithHistoryToMessages(5)                  // Contexto de conversas anteriores
            .WithReasoning(modelo, 5, 15)              // Raciocínio estruturado
            .WithStorage(storage);                     // Memória persistente
        
        // Registrar ferramentas especializadas
        consultor.RegisterToolPack(new FinanceToolPack());
        consultor.RegisterToolPack(new SearchToolPack());
        
        // Contexto da empresa
        var empresa = new ContextoEmpresarial
        {
            NomeEmpresa = "TechCorp Solutions",
            Setor = "Tecnologia",
            ReceitaAnual = 50_000_000,
            NumeroFuncionarios = 250,
            PrincipaisDesafios = new List<string>
            {
                "Alta rotatividade de desenvolvedores",
                "Competição acirrada no mercado",
                "Necessidade de modernização tecnológica",
                "Expansão para novos mercados"
            }
        };
        
        // Executar análise
        Console.WriteLine("🔍 Analisando empresa...");
        
        var resultado = await consultor.ExecuteAsync(
            "Faça uma análise completa desta empresa e forneça recomendações estratégicas",
            empresa,
            userId: "consultor_senior",
            sessionId: "analise_techcorp_2024"
        );
        
        // Exibir resultados
        var relatorio = resultado.Data;
        
        Console.WriteLine($"\n📊 ANÁLISE EMPRESARIAL - {empresa.NomeEmpresa}");
        Console.WriteLine("═" + new string('═', 50));
        
        Console.WriteLine($"\n📋 RESUMO EXECUTIVO:");
        Console.WriteLine(relatorio.ResumoExecutivo);
        
        Console.WriteLine($"\n✅ PONTOS FORTES:");
        relatorio.PontosFortes.ForEach(p => Console.WriteLine($"   • {p}"));
        
        Console.WriteLine($"\n⚠️  PONTOS DE ATENÇÃO:");
        relatorio.PontosDeAtencao.ForEach(p => Console.WriteLine($"   • {p}"));
        
        Console.WriteLine($"\n🎯 RECOMENDAÇÕES:");
        relatorio.Recomendacoes.ForEach(r => Console.WriteLine($"   • {r}"));
        
        Console.WriteLine($"\n📈 MÉTRICAS PROJETADAS:");
        foreach (var metrica in relatorio.MetricasProjetadas)
        {
            Console.WriteLine($"   • {metrica.Key}: {metrica.Value:C}");
        }
        
        // Informações técnicas
        Console.WriteLine($"\n📊 INFORMAÇÕES TÉCNICAS:");
        Console.WriteLine($"   • Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"   • Tempo de resposta: {resultado.ExecutionTime.TotalSeconds:F2}s");
        Console.WriteLine($"   • Memórias criadas: {resultado.MemoriesCreated}");
        Console.WriteLine($"   • Ferramentas utilizadas: {string.Join(", ", resultado.ToolsUsed)}");
    }
}
```

## Próximos Passos

- **[Modelos](./models.md)** - Aprenda sobre integração com diferentes provedores de IA
- **[Memória](./memory.md)** - Compreenda o sistema de memória e contexto
- **[Ferramentas](./tools.md)** - Descubra como estender agentes com function calling
- **[Orquestração](./orchestration.md)** - Construa sistemas multi-agente complexos

## Referências

- **[API Reference - Agent](../api-reference/core/agent.md)** - Documentação completa da classe Agent
- **[Tutoriais Práticos](../tutorials/basic-agent.md)** - Guias passo a passo
- **[Exemplos Avançados](../examples/advanced-examples.md)** - Casos de uso complexos