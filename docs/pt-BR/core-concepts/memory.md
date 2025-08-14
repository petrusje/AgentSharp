# Sistema de Memória no AgentSharp

O sistema de memória do AgentSharp permite que agentes mantenham contexto, aprendam com interações e forneçam experiências personalizadas. É uma arquitetura multicamada que oferece desde histórico simples até busca semântica avançada.

## Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                     SISTEMA DE MEMÓRIA                     │
├─────────────────────────────────────────────────────────────┤
│  📱 Agent Layer                                             │
│  ├─ EnableUserMemories     ├─ EnableMemorySearch           │
│  ├─ AddHistoryToMessages   ├─ EnableKnowledgeSearch        │
│  └─ NumHistoryMessages     └─ Controles Granulares         │
├─────────────────────────────────────────────────────────────┤
│  🧠 Memory Manager Layer                                    │
│  ├─ IMemoryManager         ├─ Classificação Inteligente    │
│  ├─ Context Loading        ├─ Extração Automática          │
│  ├─ Message Enhancement    └─ CRUD de Memórias             │
├─────────────────────────────────────────────────────────────┤
│  🗄️ Storage Layer                                           │
│  ├─ IStorage               ├─ IMemoryStorage               │
│  ├─ ISessionStorage        ├─ IEmbeddingStorage            │
│  └─ Unified Interface      └─ Abstrações Independentes     │
├─────────────────────────────────────────────────────────────┤
│  🔧 Implementation Layer                                    │
│  ├─ SemanticSqliteStorage  ├─ SemanticMemoryStorage        │
│  ├─ SQLite + sqlite-vec    ├─ HNSW In-Memory              │
│  └─ Persistent Vector DB   └─ Fast Vector Search          │
└─────────────────────────────────────────────────────────────┘
```

## Tipos de Memória

### 1. **Histórico de Sessão** (Session History)
Armazena mensagens da conversa atual sem processamento de embedding.

```csharp
// Configuração apenas com histórico
var agente = new Agent<object, string>(modelo, "HistoricoSimples")
    .WithHistoryToMessages(true)        // ✅ Adiciona histórico ao contexto
    .WithUserMemories(false)            // ❌ Não extrai memórias
    .WithMemorySearch(false);           // ❌ Não faz busca semântica

// Primeira mensagem
await agente.ExecuteAsync("Meu nome é Maria", "user1", "sessao1");

// Segunda mensagem (lembrará do nome)
await agente.ExecuteAsync("Qual é o meu nome?", "user1", "sessao1");
// Resposta: "Seu nome é Maria"
```

**Vantagens:**
- ⚡ **Rápido**: Sem processamento de embeddings
- 💰 **Econômico**: Não consome tokens para extrair memórias
- 🎯 **Simples**: Ideal para conversas curtas

**Limitações:**
- 📊 **Limitado**: Apenas memória de curto prazo
- 🔍 **Sem busca**: Não encontra informações de sessões antigas

### 2. **Memórias de Usuário** (User Memories)
Sistema inteligente que extrai e armazena informações importantes sobre usuários.

```csharp
// Configuração com extração de memórias
var agente = new Agent<object, string>(modelo, "MemoriaInteligente")
    .WithUserMemories()             // ✅ LLM extrai memórias importantes
    .WithMemorySearch(false)            // ❌ Busca desabilitada por enquanto
    .WithHistoryToMessages(true);       // ✅ Histórico + memórias

// Primeira conversa - informações são extraídas automaticamente
await agente.ExecuteAsync(
    "Oi! Meu nome é João, tenho 30 anos, sou desenvolvedor em São Paulo e gosto muito de café forte", 
    "user123", 
    "sessao1"
);
// Memórias extraídas automaticamente:
// - "Nome: João"
// - "Idade: 30 anos" 
// - "Profissão: Desenvolvedor"
// - "Localização: São Paulo"
// - "Preferência: Café forte"

// Sessão diferente, mas lembra do usuário
await agente.ExecuteAsync(
    "Que tipo de café você recomenda para mim?", 
    "user123", 
    "sessao2"
);
// Resposta personalizada baseada na preferência por café forte
```

**Como Funciona:**
1. 🧠 **LLM Analisa** cada interação automáticamente
2. 🔍 **Extrai Informações** relevantes sobre o usuário
3. 🏷️ **Classifica Memórias** por tipo e relevância
4. 💾 **Armazena Persistentemente** no storage configurado
5. 🎯 **Reutiliza** em conversas futuras

### 3. **Busca Semântica** (Semantic Search)
Permite encontrar memórias relevantes usando similaridade de embeddings.

```csharp
// Configuração completa com busca semântica
var embeddingService = new OpenAIEmbeddingService("api-key");
var storage = new SemanticSqliteStorage(
    "Data Source=memorias.db", 
    embeddingService, 
    1536
);

var agente = new Agent<object, string>(modelo, "BuscaSemantica")
    .WithUserMemories()             // ✅ Extrai memórias
    .WithMemorySearch(true)             // ✅ Busca semântica ativa
    .WithStorage(storage);

// Armazenar informações diversas
await agente.ExecuteAsync("Adoro pizza de calabresa aos domingos", "user1", "s1");
await agente.ExecuteAsync("Minha esposa se chama Ana e é professora", "user1", "s2");
await agente.ExecuteAsync("Trabalho como engenheiro de software", "user1", "s3");

// Busca semântica encontra informações relacionadas
await agente.ExecuteAsync("Me fale sobre comida italiana", "user1", "s4");
// Encontrará a memória sobre pizza, mesmo sem mencionar "pizza" explicitamente
```

**Casos de Uso Ideais:**
- 🎭 **Assistentes Pessoais**: Lembram preferências e contexto
- 🏥 **Sistemas de Saúde**: Histórico médico e sintomas
- 🛒 **E-commerce**: Preferências de compra e comportamento
- 📚 **Educação**: Progresso e dificuldades do aluno

## Armazenamento (Storage)

### Interface Unificada `IStorage`

O AgentSharp usa uma interface única que abstrai diferentes tipos de armazenamento:

```csharp
public interface IStorage
{
    // Gerenciamento de sessões
    ISessionStorage Sessions { get; }
    
    // Armazenamento de memórias
    IMemoryStorage Memories { get; }
    
    // Armazenamento de embeddings/vetores
    IEmbeddingStorage Embeddings { get; }
    
    // Operações de alto nível
    Task SaveMemoryAsync(UserMemory memory);
    Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit);
    Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit);
    Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message);
}
```

### Implementações Disponíveis

#### 1. **SemanticSqliteStorage** (Recomendado para Produção)

Armazenamento persistente com busca vetorial usando SQLite + sqlite-vec:

```csharp
var embeddingService = new OpenAIEmbeddingService("api-key");
var storage = new SemanticSqliteStorage(
    connectionString: "Data Source=agentsharp.db",
    embeddingService: embeddingService,
    dimensions: 1536,                    // Dimensões do embedding
    maxConnections: 10                   // Pool de conexões
);

var agente = new Agent<object, string>(modelo, "Persistente")
    .WithStorage(storage)
    .WithUserMemories()
    .WithMemorySearch(true);
```

**Características:**
- ✅ **Persistente**: Dados sobrevivem ao reinício da aplicação
- ⚡ **Performance**: sqlite-vec é otimizado para busca vetorial
- 🔒 **ACID**: Transações seguras e consistência
- 📦 **Self-contained**: Um arquivo de banco de dados
- 🔍 **Busca Híbrida**: Texto + similaridade semântica

#### 2. **SemanticMemoryStorage** (HNSW In-Memory)

Armazenamento em memória com HNSW para busca vetorial ultra-rápida:

```csharp
var embeddingService = new OpenAIEmbeddingService("api-key");
var storage = new SemanticMemoryStorage(
    embeddingService: embeddingService,
    dimensions: 1536,
    maxConnections: 16,                  // HNSW M parameter
    efConstruction: 200                  // HNSW build quality
);

var agente = new Agent<object, string>(modelo, "RapidoMemoria")
    .WithStorage(storage)
    .WithUserMemories()
    .WithMemorySearch(true);
```

**Características:**
- ⚡ **Ultra-rápido**: Busca vetorial otimizada HNSW
- 💾 **Em memória**: Ideal para aplicações temporárias
- 🎯 **Baixa latência**: Perfeito para chatbots em tempo real
- ⚖️ **Trade-off**: Velocidade vs persistência

## Configurações Avançadas

### Controles Granulares

O AgentSharp oferece controles finos para otimizar custos e performance:

```csharp
// Configuração econômica (apenas histórico)
var agenteEconomico = new Agent<object, string>(modelo, "Economico")
    .WithUserMemories(false)            // ❌ Não extrai memórias (economiza tokens)
    .WithMemorySearch(false)            // ❌ Não busca memórias (economiza processamento)
    .WithHistoryToMessages(true)        // ✅ Apenas histórico simples
    .WithNumHistoryMessages(5);         // Apenas 5 mensagens recentes

// Configuração premium (funcionalidade completa)
var agentePremium = new Agent<object, string>(modelo, "Premium")
    .WithUserMemories()             // ✅ Extrai e gerencia memórias
    .WithMemorySearch(true)             // ✅ Busca semântica completa
    .WithHistoryToMessages(true)        // ✅ Histórico de conversas
    .WithNumHistoryMessages(20)         // Contexto extenso
    .WithKnowledgeSearch(true);         // ✅ Integração com RAG externo

// Configuração híbrida (balanceada)
var agenteHibrido = new Agent<object, string>(modelo, "Hibrido")
    .WithUserMemories()             // ✅ Memórias importantes
    .WithMemorySearch(false)            // ❌ Busca desabilitada (economia)
    .WithHistoryToMessages(true)        // ✅ Contexto de sessão
    .WithNumHistoryMessages(10);        // Contexto moderado
```

### Configuração de Memória

```csharp
var memoryConfig = new MemoryConfiguration
{
    MaxMemories = 500,                   // Máx. memórias por usuário
    MaxExecutions = 100,                 // Máx. execuções lembradas
    MinRelevanceScore = 0.75,           // Relevância mínima para busca
    EnableAutoSummary = true,           // Resumos automáticos
    MinEntriesForSummary = 50           // Mín. entradas para resumo
};

var agente = new Agent<object, string>(modelo, "ConfiguradoMemoria")
    .WithMemoryConfig(memoryConfig);
```

### Configuração de Domínio

Customize os prompts de memória para domínios específicos:

```csharp
var domainConfig = new MemoryDomainConfiguration
{
    SaveMemoryPrompt = @"
        Extraia informações médicas importantes sobre o paciente.
        Foque em: sintomas, histórico médico, medicamentos, alergias.
        Formato: 'Categoria: Informação'
    ",
    
    SearchMemoryPrompt = @"
        Busque informações médicas relevantes para:
        - Sintomas similares
        - Histórico relacionado
        - Medicamentos pertinentes
    ",
    
    ClassifyMemoryPrompt = @"
        Classifique a informação médica como:
        - sintoma, historico, medicamento, alergia, outros
    ",
    
    Domain = "Sistema de Saúde"
};
```

## SmartMemoryToolPack

O SmartMemoryToolPack fornece ferramentas que permitem ao LLM gerenciar memórias diretamente:

```csharp
public class SmartMemoryToolPack : ToolPack
{
    [FunctionCall("adicionar_memoria")]
    public async Task<string> AdicionarMemoria(string conteudo);
    
    [FunctionCall("buscar_memorias")]
    public async Task<List<UserMemory>> BuscarMemorias(string consulta, int limite = 5);
    
    [FunctionCall("atualizar_memoria")]
    public async Task<string> AtualizarMemoria(string id, string novoConteudo);
    
    [FunctionCall("remover_memoria")]
    public async Task<string> RemoverMemoria(string id);
    
    [FunctionCall("listar_memorias")]
    public async Task<List<UserMemory>> ListarMemorias(int limite = 10);
}
```

**Registro Automático:**
```csharp
// O SmartMemoryToolPack é registrado automaticamente quando:
var agente = new Agent<object, string>(modelo, "MemoriaInteligente")
    .WithUserMemories()     // OU
    .WithMemorySearch(true);    // Qualquer um destes habilita o ToolPack
```

## Exemplos Práticos

### Exemplo 1: Sistema de Atendimento ao Cliente

```csharp
public class ContextoAtendimento
{
    public string Protocolo { get; set; }
    public string TipoProblema { get; set; }
    public DateTime DataAbertura { get; set; }
}

class Program
{
    static async Task Main()
    {
        // Setup de memória persistente
        var embeddingService = new OpenAIEmbeddingService("api-key");
        var storage = new SemanticSqliteStorage("Data Source=atendimento.db", embeddingService, 1536);
        
        // Agente especializado em atendimento
        var atendente = new Agent<ContextoAtendimento, string>(modelo, "AtendenteVirtual")
            .WithPersona(@"
                Você é um atendente experiente e empático. 
                Sempre consulte o histórico do cliente antes de responder.
                Mantenha tom profissional mas acolhedor.
            ")
            .WithUserMemories()                 // Lembra de problemas anteriores
            .WithMemorySearch(true)                 // Busca casos similares
            .WithHistoryToMessages(10)              // Contexto da conversa atual
            .WithStorage(storage);

        // Primeiro atendimento
        var contexto = new ContextoAtendimento
        {
            Protocolo = "AT001234",
            TipoProblema = "Problema de Login",
            DataAbertura = DateTime.Now
        };
        
        await atendente.ExecuteAsync(
            "Não estou conseguindo fazer login na minha conta. Sempre que coloco a senha, dá erro.",
            contexto,
            userId: "cliente_joao_silva",
            sessionId: "atendimento_2024_001"
        );
        // Sistema lembra: problema de login, erro com senha
        
        // Atendimento futuro do mesmo cliente
        var novoContexto = new ContextoAtendimento
        {
            Protocolo = "AT001890",
            TipoProblema = "Dúvida sobre Funcionalidade",
            DataAbertura = DateTime.Now.AddDays(15)
        };
        
        await atendente.ExecuteAsync(
            "Olá, tenho uma dúvida sobre como usar a nova funcionalidade X",
            novoContexto,
            userId: "cliente_joao_silva",
            sessionId: "atendimento_2024_002"
        );
        // Atendente lembrará do problema anterior e pode perguntar se foi resolvido
    }
}
```

### Exemplo 2: Tutor Educacional Personalizado

```csharp
public class PerfilEstudante
{
    public string Nome { get; set; }
    public string Nivel { get; set; }
    public List<string> DificuldadesIdentificadas { get; set; }
    public List<string> TemasDeInteresse { get; set; }
}

class TutorExample
{
    static async Task Main()
    {
        // Configuração de memória para educação
        var embeddingService = new OpenAIEmbeddingService("api-key");
        var storage = new SemanticSqliteStorage("Data Source=educacao.db", embeddingService, 1536);
        
        var domainConfig = new MemoryDomainConfiguration
        {
            SaveMemoryPrompt = @"
                Extraia informações educacionais sobre o estudante:
                - Dificuldades de aprendizado
                - Áreas de interesse
                - Progresso em tópicos
                - Estilo de aprendizagem preferido
                Formato: 'Categoria: Detalhes'
            ",
            Domain = "Educação Personalizada"
        };
        
        var tutor = new Agent<PerfilEstudante, string>(modelo, "TutorIA")
            .WithPersona(@"
                Você é um tutor dedicado e paciente. 
                Adapte suas explicações ao nível e estilo de cada estudante.
                Use exemplos práticos e seja encorajador.
            ")
            .WithUserMemories()                 // Lembra do progresso
            .WithMemorySearch(true)                 // Conecta conceitos
            .WithHistoryToMessages(15)              // Contexto da aula
            .WithMemoryDomainConfig(domainConfig)
            .WithStorage(storage);
        
        // Primeira aula
        var perfil = new PerfilEstudante
        {
            Nome = "Ana",
            Nivel = "Ensino Médio",
            DificuldadesIdentificadas = new() { "Matemática", "Física" },
            TemasDeInteresse = new() { "Astronomia", "Tecnologia" }
        };
        
        await tutor.ExecuteAsync(
            "Oi! Eu sou a Ana. Tenho muita dificuldade com matemática, especialmente com equações. Mas adoro astronomia!",
            perfil,
            userId: "estudante_ana",
            sessionId: "aula_matematica_001"
        );
        // Tutor registra dificuldades e interesses
        
        // Aulas futuras adaptadas
        await tutor.ExecuteAsync(
            "Podemos ver mais sobre funções quadráticas?",
            perfil,
            userId: "estudante_ana", 
            sessionId: "aula_matematica_015"
        );
        // Tutor lembrará das dificuldades e usará exemplos de astronomia
    }
}
```

## Monitoramento e Telemetria

```csharp
using AgentSharp.console.Services;

// Configurar telemetria para monitorar memória
var telemetria = new TelemetryService(new LocalizationService("pt-BR"))
{
    IsEnabled = true
};

var agente = new Agent<object, string>(modelo, "MonitoradoMemoria")
    .WithUserMemories()
    .WithMemorySearch(true)
    .WithTelemetry(telemetria);

// Executar com monitoramento
await agente.ExecuteAsync("Teste de memória", "user1", "sessao1");

// Verificar métricas
var summary = telemetria.GetSummary();
Console.WriteLine($"Tokens de memória utilizados: {summary.MemoryTokens}");
Console.WriteLine($"Operações de embedding: {summary.EmbeddingTokens}");
Console.WriteLine($"Tempo total de memória: {summary.TotalMemoryTime}s");
```

## Boas Práticas

### 1. **Escolha do Armazenamento**

```csharp
// Para desenvolvimento e testes
var development = new SemanticMemoryStorage(embeddingService, 1536);

// Para produção com poucos usuários
var small_production = new SemanticSqliteStorage("Data Source=app.db", embeddingService, 1536);

// Para produção com muitos usuários
var enterprise = new SemanticSqliteStorage(
    connectionString: "Data Source=prod.db;Cache=Shared;Mode=ReadWriteCreate",
    embeddingService: embeddingService,
    dimensions: 1536,
    maxConnections: 50
);
```

### 2. **Otimização de Custos**

```csharp
// Análise de custo por funcionalidade
var analises = new[]
{
    new { Config = "Histórico apenas", Cost = "Baixo", Tokens = "~50-100" },
    new { Config = "Histórico + Memórias", Cost = "Médio", Tokens = "~200-400" },
    new { Config = "Completo com busca", Cost = "Alto", Tokens = "~400-800" }
};

// Escolha baseada no caso de uso
var chatbotSimples = agente.WithHistoryToMessages(true).WithUserMemories(false);
var assistentePessoal = agente.WithUserMemories().WithMemorySearch(false);
var sistemaCompleto = agente.WithUserMemories().WithMemorySearch(true);
```

### 3. **Configurações de Produção**

```csharp
// Configuração robusta para produção
var config = new MemoryConfiguration
{
    MaxMemories = 1000,                 // Limite por usuário
    MaxExecutions = 500,                // Histórico de execuções
    MinRelevanceScore = 0.8,           // Alta relevância
    EnableAutoSummary = true,          // Resumos para economia
    MinEntriesForSummary = 100         // Resumir após 100 entradas
};

var productionAgent = new Agent<object, string>(modelo, "Producao")
    .WithMemoryConfig(config)
    .WithUserMemories()
    .WithMemorySearch(true)
    .WithHistoryToMessages(10)
    .WithStorage(enterpriseStorage);
```

## Próximos Passos

- **[Ferramentas](./tools.md)** - Aprenda sobre function calling e ToolPacks
- **[Orquestração](./orchestration.md)** - Sistemas multi-agente e workflows
- **[API Reference - Memory](../api-reference/memory/memory-manager.md)** - Documentação detalhada
- **[Tutoriais Avançados](../tutorials/advanced-memory.md)** - Exemplos práticos complexos

## Troubleshooting

### Problemas Comuns

1. **sqlite-vec não encontrado**
   ```bash
   # Instalar automaticamente
   await SqliteVecInstaller.InstallAsync();
   ```

2. **Embeddings custosos**
   ```csharp
   // Use modelo local ou cache embeddings
   var cacheService = new CachedEmbeddingService(baseService);
   ```

3. **Memória crescendo muito**
   ```csharp
   // Configure limites e auto-limpeza
   config.MaxMemories = 500;
   config.EnableAutoSummary = true;
   ```

4. **Performance lenta**
   ```csharp
   // Use HNSW in-memory para aplicações temporárias
   var fastStorage = new SemanticMemoryStorage(embeddingService, 1536);
   ```