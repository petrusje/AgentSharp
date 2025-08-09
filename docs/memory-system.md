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
│    • LLM integration                │   • Enriquecimento de mensagens
│    • Smart classification           │   • Extração automática de memórias
├─────────────────────────────────────┤
│            IStorage                 │ ← Camada de Persistência (Médio Nível)
│    • SqliteStorage                  │   • Armazenamento permanente
│    • InMemoryStorage                │   • Múltiplos providers
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

**Storage Tradicional:**
- **Desenvolvimento/Testes**: `InMemoryStorage`
- **Produção simples**: `SqliteStorage` 
- **Alta escala**: Implemente `IStorage` com PostgreSQL/MongoDB

**Storage Vetorial (Busca Semântica):**
- **Todos os casos**: `VectorSqliteVecStorage` (moderna, simples, alta performance)
- **Datasets pequenos (< 1K)**: `VectorSqliteStorage` (busca linear, se simplicidade máxima)
- **Produção enterprise**: Considere Pinecone, Weaviate, ou Qdrant apenas se necessário

```csharp
// Configuração simples e moderna com sqlite-vec
IVectorStorage CreateVectorStorage()
{
    return new VectorSqliteVecStorage(
        connectionString: "Data Source=vectors.db;Cache Size=50000;Journal Mode=WAL",
        embeddingModel: "text-embedding-3-small",
        dimensions: 1536,
        distanceMetric: "cosine"  // cosine, l2, ou inner_product
    );
}
```

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

## Limitações Atuais

- **Embeddings**: Implementação postergada - busca apenas textual
- **Consolidação**: Algoritmo simples de detecção de duplicatas
- **Analytics**: Métricas básicas de uso

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