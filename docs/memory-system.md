# Sistema de Memória do AgentSharp

## Visão Geral

O AgentSharp possui um sistema de memória sofisticado e hierárquico que permite aos agentes lembrarem de informações importantes, contexto de usuário e histórico de conversas. O sistema é composto por duas camadas principais que trabalham em conjunto.

## Arquitetura do Sistema

```
┌─────────────────────────────────────┐
│           Agent                     │ 
├─────────────────────────────────────┤
│         IMemoryManager              │ ← Camada Inteligente (Alto Nível)
│    • Context management             │   • Classificação automática por IA
│    • LLM integration               │   • Enriquecimento de mensagens
│    • Smart classification          │   • Extração automática de memórias
├─────────────────────────────────────┤
│            IStorage                 │ ← Camada de Persistência (Médio Nível)
│    • SqliteStorage                  │   • Armazenamento permanente
│    • InMemoryStorage               │   • Múltiplos providers
├─────────────────────────────────────┤
│            IMemory                  │ ← Cache Simples (Baixo Nível)
│    • Basic CRUD                     │   • ExecutionEngine logging
│    • Simple items                  │   • Cache temporário
└─────────────────────────────────────┘
```

## Componentes Principais

### 1. IMemory - Cache de Baixo Nível

Interface simples para cache temporário e logging básico:

```csharp
// Usado principalmente pelo ExecutionEngine
var memory = new InMemoryStore();
await memory.AddItemAsync(new MemoryItem("Tool result", "tool_result"));
var items = await memory.GetItemsAsync("tool_result", 5);
```

**Casos de uso:**
- Cache de resultados de ferramentas
- Log de respostas do modelo
- Armazenamento temporário durante execução

### 2. IMemoryManager - Gestão Inteligente

Interface avançada com classificação automática e integração com LLM:

```csharp
// Configuração
var storage = new SqliteStorage("Data Source=memory.db");
var memoryManager = new MemoryManager(storage, model, logger);

// Carregamento de contexto
var context = await memoryManager.LoadContextAsync(userId, sessionId);

// Enriquecimento automático de mensagens
var enrichedMessages = await memoryManager.EnhanceMessagesAsync(messages, context);
```

**Funcionalidades principais:**
- **Classificação Automática**: IA classifica memórias em tipos (Fact, Preference, Task, etc.)
- **Extração Automática**: Analisa conversas e extrai informações relevantes
- **Enriquecimento**: Adiciona memórias relevantes às mensagens automaticamente
- **Tools para LLM**: Permite que o LLM gerencie suas próprias memórias

### 3. IStorage - Persistência Configurável

Interface para diferentes provedores de armazenamento:

```csharp
// SQLite (Produção)
var storage = new SqliteStorage("Data Source=agent_memory.db");
await storage.InitializeAsync();

// InMemory (Desenvolvimento/Testes)
var storage = new InMemoryStorage();
```

## Tipos de Memória

O sistema classifica automaticamente as memórias em:

| Tipo | Descrição | Exemplo |
|------|-----------|---------|
| **Fact** | Fatos objetivos sobre o usuário | "Mora em São Paulo", "Trabalha como desenvolvedor" |
| **Preference** | Preferências e gostos | "Prefere café sem açúcar", "Gosta de música clássica" |
| **Conversation** | Contexto de conversa | "Estava discutindo sobre IA", "Pediu ajuda com Python" |
| **Task** | Tarefas e objetivos | "Precisa finalizar projeto até sexta", "Quer aprender React" |
| **Skill** | Habilidades e conhecimentos | "Conhece JavaScript avançado", "Experiência em AWS" |
| **Personal** | Informações pessoais | "Aniversário em março", "Tem dois filhos" |

## Funcionalidades Avançadas

### Classificação Automática via IA

O sistema usa o próprio LLM para classificar memórias:

```csharp
// Automático - não precisa classificar manualmente
await memoryManager.AddMemoryAsync("João prefere trabalhar de manhã");
// Sistema classifica automaticamente como 'Preference'
```

### Extração Automática de Conversas

Analisa interações e extrai informações importantes:

```csharp
var userMessage = AIMessage.User("Preciso terminar o projeto React até sexta-feira");
var assistantMessage = AIMessage.Assistant("Vou te ajudar a organizar as tarefas...");

// Extração automática
await memoryManager.ProcessInteractionAsync(userMessage, assistantMessage, context);
// Sistema extrai automaticamente: "Tem projeto React com deadline sexta-feira" (Task)
```

### Enriquecimento Inteligente

Adiciona contexto relevante às mensagens automaticamente:

```csharp
var messages = new List<AIMessage> 
{
    AIMessage.System("Você é um assistente útil"),
    AIMessage.User("Como posso melhorar meu código React?")
};

// Enriquecimento automático
var enrichedMessages = await memoryManager.EnhanceMessagesAsync(messages, context);

// Resultado: Sistema adiciona contexto relevante baseado em memórias do usuário
// "CONTEXTO DE MEMÓRIA: [Task] Projeto React deadline sexta-feira..."
```

### Tools para LLM

O SmartMemoryToolPack fornece ferramentas para o LLM gerenciar memórias:

```csharp
// Registrado automaticamente no Agent
var agent = new Agent<MyContext, string>(model)
    .WithToolPacks(new SmartMemoryToolPack());

// LLM pode usar as tools:
// - AddMemory("informação importante")
// - SearchMemories("React")
// - UpdateMemory(id, "nova informação")
// - DeleteMemory(id)
// - ListMemories("Task")
```

## Storage Providers

### SQLite Storage (Produção)

Armazenamento persistente completo:

```csharp
var storage = new SqliteStorage("Data Source=agent_memory.db");
await storage.InitializeAsync(); // Cria tabelas automaticamente

var agent = new Agent<MyContext, string>(model, storage: storage);
```

**Funcionalidades:**
- ✅ Persistência entre reinicializações
- ✅ Sessões de usuário
- ✅ Busca textual otimizada
- ✅ Soft delete
- ✅ Metadados JSON

### InMemory Storage (Desenvolvimento)

Cache em memória para desenvolvimento e testes:

```csharp
var storage = new InMemoryStorage();
var agent = new Agent<MyContext, string>(model, storage: storage);
```

## Configuração e Uso

### Configuração Básica

```csharp
// 1. Configurar storage
var storage = new SqliteStorage("Data Source=memory.db");
await storage.InitializeAsync();

// 2. Configurar memory manager
var memoryManager = new MemoryManager(
    storage, 
    model, 
    logger, 
    embeddingService: null // Opcional - para busca semântica
);

// 3. Configurar agent
var agent = new Agent<MyContext, string>(model)
    .WithStorage(storage)
    .WithMemoryManager(memoryManager);
```

### Exemplo Completo

```csharp
public async Task ExemploMemoriaCompleto()
{
    // Setup
    var model = new OpenAIModel("gpt-4o", apiKey);
    var storage = new SqliteStorage("Data Source=agent_memory.db");
    await storage.InitializeAsync();
    
    var agent = new Agent<UserContext, string>(model, storage: storage)
        .WithInstructions("Você é um assistente que lembra das preferências do usuário")
        .WithContext(new UserContext { UserId = "user123", SessionId = "session456" });

    // Primeira conversa
    var result1 = await agent.ExecuteAsync("Meu nome é João e prefiro café sem açúcar");
    
    // Segunda conversa (em outra sessão)
    var result2 = await agent.ExecuteAsync("Você pode preparar um café para mim?");
    // Sistema automaticamente lembra: "João prefere café sem açúcar"
}
```

## Monitoramento e Debug

### Verificar Memórias Armazenadas

```csharp
var memories = await memoryManager.GetExistingMemoriesAsync();
foreach (var memory in memories)
{
    Console.WriteLine($"[{memory.Type}] {memory.Content}");
}
```

### Estatísticas

```csharp
// Usando SmartMemoryToolPack
var stats = await smartMemoryTools.GetMemoryStats();
// Retorna contagem por tipo, memórias mais antigas/recentes, etc.
```

## Melhores Práticas

### 1. Escolha do Storage

- **Desenvolvimento/Testes**: `InMemoryStorage`
- **Produção**: `SqliteStorage` 
- **Alta escala**: Implemente `IStorage` com PostgreSQL/MongoDB

### 2. Contexto de Usuário

```csharp
// Sempre forneça UserId e SessionId
public class MyContext 
{
    public string UserId { get; set; }
    public string SessionId { get; set; }
    // ... outros campos
}
```

### 3. Classificação Manual (quando necessário)

```csharp
// Sistema classifica automaticamente, mas pode ser manual se necessário
await memoryManager.AddMemoryAsync("João gosta de café forte", context);
```

### 4. Limpeza de Memórias

```csharp
// Implementar limpeza periódica de memórias antigas
await storage.CleanupMemoriesAsync(userId, retentionDays: 30, maxMemories: 1000);
```

## Extensibilidade

### Custom Storage Provider

```csharp
public class PostgreSqlStorage : IStorage
{
    // Implementar interface completa
    public ISessionStorage Sessions { get; }
    public IMemoryStorage Memories { get; }
    public IEmbeddingStorage Embeddings { get; }
    
    // ... implementação
}
```

### Custom Memory Classifier

```csharp
public class CustomMemoryClassifier : IMemoryClassifier
{
    public Task<AgentMemoryType> ClassifyAsync(string content)
    {
        // Lógica customizada de classificação
    }
}
```

## Funcionalidades Avançadas

### Sistema de Deduplicação Inteligente

Previne memórias duplicadas usando similaridade semântica:

```csharp
// Sistema automaticamente detecta e previne duplicações
await memoryManager.AddMemoryAsync("João prefere café forte", context);
await memoryManager.AddMemoryAsync("João gosta de café forte", context); // Similar detectada
// Resultado: Apenas uma memória é salva
```

**Características:**
- **Normalização de conteúdo**: Remove pontuação e padroniza formato
- **Similaridade semântica**: Calcula similaridade usando algoritmo de Jaccard
- **Threshold configurável**: 75% de similaridade por padrão
- **Fallback seguro**: Em caso de erro, permite adicionar para não bloquear

### Busca Híbrida Inteligente

Sistema que combina múltiplas estratégias:

```csharp
// Busca inteligente com sinônimos e palavras-chave
var memories = await memoryManager.GetRelevantMemoriesAsync("estudar hoje", context);
// Encontra memórias sobre "trabalhar pela manhã" automaticamente
```

**Estratégias utilizadas:**
1. **Busca textual direta**: Palavras exatas no conteúdo
2. **Busca por palavras-chave**: Extração de termos importantes
3. **Sinônimos semânticos**: "estudar" ↔ "trabalhar" ↔ "manhã"
4. **Scoring inteligente**: Relevância + recência + importância

### Mapeamento de Sinônimos

Sistema que entende relacionamentos semânticos:

```csharp
// Configuração de sinônimos
var synonymMap = new Dictionary<string, List<string>>
{
    { "estudar", new[] { "trabalhar", "manhã", "morning" } },
    { "café", new[] { "coffee", "forte", "bebida" } },
    { "trabalhar", new[] { "estudar", "manhã", "cedo" } }
};
```

### Queries SQL Otimizadas

Sistema de busca com scoring:

```sql
SELECT *, 
       (RelevanceScore * 0.7 + 
        CASE WHEN datetime(UpdatedAt) > datetime('now', '-1 day') THEN 0.3 ELSE 0.1 END) as SearchScore
FROM UserMemory 
WHERE UserId = ? AND IsActive = 1 AND (
    LOWER(Content) LIKE '%estudar%' OR 
    LOWER(Content) LIKE '%trabalhar%' OR 
    LOWER(Content) LIKE '%manhã%'
)
ORDER BY SearchScore DESC, RelevanceScore DESC, UpdatedAt DESC;
```

## 🧠 Sistema de Busca Semântica (Embeddings Vetoriais)

### VectorSqliteStorage

Storage avançado com suporte a embeddings:

```csharp
// Configuração com serviço de embeddings
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var vectorStorage = new VectorSqliteStorage("Data Source=vector.db", embeddingService);
await vectorStorage.InitializeAsync();

var agent = new Agent<Context, string>(model, storage: vectorStorage)
    .WithPersona("Assistente com busca semântica avançada");
```

### OpenAIEmbeddingService

Serviço robusto com fallback inteligente:

```csharp
// Configuração do serviço
var embeddingService = new OpenAIEmbeddingService(
    apiKey: "sk-...", 
    endpoint: "https://api.openai.com",
    model: "text-embedding-ada-002"
);

// Funcionalidades:
// ✅ Embeddings via OpenAI API
// ✅ Fallback inteligente se API falhar  
// ✅ Cálculo de similaridade cosseno
// ✅ Cache e otimizações
// ✅ Embeddings em lote
```

### Busca por Similaridade

Sistema que entende contexto semântico:

```csharp
// Busca semântica - entende relacionamentos conceituais
var query = "Como fazer uma bebida energizante matinal?";
var memories = await vectorStorage.SearchMemoriesAsync(query, context, 5);

// Resultado: Encontra memórias sobre "café forte pela manhã"
// mesmo sem palavras exatas em comum!
```

### Comparação: Textual vs Semântica

```csharp
// Exemplo prático das diferenças:

// BUSCA TEXTUAL
// Query: "bebida energizante matinal"  
// Memórias: "gosto de café forte pela manhã"
// Resultado: Não encontra (palavras diferentes)

// BUSCA SEMÂNTICA  
// Query: "bebida energizante matinal"
// Memórias: "gosto de café forte pela manhã"  
// Resultado: Encontra (conceitos relacionados)
```

## Implementação Técnica

### Arquitetura de Storage

```
┌─────────────────────────────────────┐
│           Agent                     │ 
├─────────────────────────────────────┤
│         MemoryManager               │ ← Deduplicação + Busca Híbrida
│    • Duplicate detection            │   • Sinônimos semânticos  
│    • Hybrid search                 │   • Keyword extraction
│    • Semantic mapping              │   • Relevance scoring
├─────────────────────────────────────┤
│       VectorSqliteStorage           │ ← Busca Semântica (Novo!)
│    • Embedding vectors              │   • Cosine similarity
│    • Semantic search               │   • Vector indexes  
│    • Fallback to textual           │   • OpenAI integration
├─────────────────────────────────────┤
│         SqliteStorage               │ ← Busca Textual Otimizada
│    • Optimized queries             │   • Multi-word search
│    • Relevance scoring             │   • Recency weighting
│    • Indexed searches              │   • Performance optimization
└─────────────────────────────────────┘
```

### Exemplo de Uso Completo

```csharp
public async Task ExemploSistemaMelhorado()
{
    // 1. Setup com embeddings
    var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
    var storage = new VectorSqliteStorage("memory.db", embeddingService);
    await storage.InitializeAsync();

    // 2. Agent configurado
    var agent = new Agent<Context, string>(model, storage: storage)
        .WithPersona("Assistente com memória semântica avançada")
        .WithContext(new Context { UserId = "user123", SessionId = "session456" });

    // 3. Primeira conversa
    await agent.ExecuteAsync("Sou desenvolvedor Python e adoro machine learning");

    // 4. Segunda conversa - busca semântica em ação
    var result = await agent.ExecuteAsync("Preciso de ajuda com redes neurais");
    // ✅ Encontra automaticamente memórias sobre "Python" e "machine learning"
    // ✅ Faz conexão semântica: redes neurais ↔ machine learning
    
    // 5. Terceira conversa - sinônimos funcionando  
    var result2 = await agent.ExecuteAsync("Que linguagem usar para data science?");
    // ✅ Conecta "data science" com "machine learning" e "Python"
    // ✅ Sugere Python baseado no histórico do usuário
}
```


## Modo Anônimo 🎭

O AgentSharp suporta **modo anônimo** que permite funcionamento sem autenticação prévia, gerando automaticamente IDs únicos para usuários e sessões.

### Configuração

```csharp
// Modo anônimo habilitado - IDs gerados automaticamente
var agent = new Agent<object, string>(model, "Assistant")
    .WithAnonymousMode(true)
    .WithStorage(storage);

var result = await agent.ExecuteAsync("Olá!");

// Acesso às informações da sessão
Console.WriteLine($"User ID: {result.SessionInfo.UserId}");        // anonymous_a1b2c3d4
Console.WriteLine($"Session ID: {result.SessionInfo.SessionId}");  // guid único
Console.WriteLine($"Foi gerado: {result.SessionInfo.WasGenerated}"); // true
Console.WriteLine($"É anônimo: {result.SessionInfo.IsAnonymous}");   // true
```

### Características

- **IDs Únicos**: Gera `anonymous_xxxxxxxx` para userId e GUID para sessionId
- **Consistência**: Mantém os mesmos IDs durante toda a vida do agent
- **Flexibilidade**: Funciona com contexto parcial ou sem contexto
- **Compatibilidade**: Funciona com todos os storage providers

### Casos de Uso

```csharp
// 1. Sem contexto (modo totalmente anônimo)
var agent = new Agent<object, string>(model, "Assistant")
    .WithAnonymousMode(true);

// 2. Contexto parcial (completar IDs em falta)
var context = new { UserId = "john_doe" }; // SessionId será gerado
var agent = new Agent<dynamic, string>(model, "Assistant")
    .WithAnonymousMode(true)
    .WithContext(context);

// 3. Web applications sem login obrigatório
var agent = new Agent<WebContext, string>(model, "Assistant")
    .WithAnonymousMode(true)
    .WithStorage(new SqliteStorage("Data Source=web_sessions.db"));
```

### SessionInfo na Resposta

Toda execução retorna informações da sessão:

```csharp
public class SessionInfo
{
    public string UserId { get; set; }      // ID do usuário
    public string SessionId { get; set; }   // ID da sessão
    public bool WasGenerated { get; set; }  // Se foi gerado automaticamente
    public bool IsAnonymous => UserId?.StartsWith("anonymous_") == true;
}
```

### Benefícios

- ✅ **Funcionamento sem autenticação**: Zero configuração necessária
- ✅ **Memória persistente**: Funciona com SQLite para sessões anônimas
- ✅ **Desenvolvimento rápido**: Ideal para protótipos e demos
- ✅ **Testes automatizados**: Simplifica cenários de teste
- ✅ **Flexibilidade**: Pode complementar sistemas de autenticação existentes

## Roadmap

- [ ] Implementação completa de embeddings para busca semântica
- [ ] Algoritmos avançados de consolidação de memórias
- [ ] Analytics e métricas avançadas
- [ ] Suporte a múltiplos agentes compartilhando memórias
- [ ] Interface web para visualização de memórias
- [ ] **Modo anônimo com TTL**: Expiração automática de sessões anônimas