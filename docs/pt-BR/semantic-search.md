# Busca Semântica e Embeddings no AgentSharp

O AgentSharp implementa um sistema avançado de busca semântica que permite aos agentes encontrarem informações relacionadas mesmo quando não há correspondência exata de palavras. Este sistema combina embeddings vetoriais com algoritmos de fallback inteligente.

## 🧠 Visão Geral

A busca semântica no AgentSharp funciona em três níveis:

1. **Busca Vetorial**: Usando embeddings para similaridade semântica
2. **Busca Híbrida**: Combinação de busca textual + palavras-chave + sinônimos  
3. **Fallback Inteligente**: Sistema robusto que funciona mesmo sem API externa

## 🔧 Componentes Principais

### OpenAIEmbeddingService

Serviço principal para geração de embeddings usando a API da OpenAI:

```csharp
// Configuração básica
var embeddingService = new OpenAIEmbeddingService(
    apiKey: "sk-proj-...", 
    endpoint: "https://api.openai.com",
    model: "text-embedding-ada-002"
);

// Gerar embedding para texto
var embedding = await embeddingService.GenerateEmbeddingAsync("João prefere café forte");
// Resultado: [0.1, -0.3, 0.7, ...] (1536 dimensões)

// Calcular similaridade entre textos
var similarity = embeddingService.CalculateSimilarity(embedding1, embedding2);
// Resultado: 0.85 (85% similar)
```

**Funcionalidades:**
- ✅ Integração completa com OpenAI API
- ✅ Fallback inteligente quando API não disponível
- ✅ Cálculo de similaridade cosseno
- ✅ Processamento em lote para eficiência
- ✅ Cache automático para evitar chamadas desnecessárias

### VectorSqliteStorage

Storage avançado que combina SQLite tradicional com busca vetorial:

```csharp
// Setup do storage vetorial
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("Data Source=vector_memory.db", embeddingService);
await storage.InitializeAsync();

// Uso com Agent
var agent = new Agent<Context, string>(model, storage: storage)
    .WithPersona("Assistente com busca semântica")
    .WithContext(new Context { UserId = "user123", SessionId = "session456" });
```

**Arquitetura do Storage:**
```
┌─────────────────────────────────────┐
│         VectorSqliteStorage         │
├─────────────────────────────────────┤
│ UserMemory Table:                   │
│ ├─ Id, UserId, Content, Type        │
│ ├─ CreatedAt, UpdatedAt, Tags       │  
│ └─ Embedding BLOB (vetores)         │ ← Novo!
├─────────────────────────────────────┤
│ Busca Estratégica:                  │
│ 1. Semantic Search (embeddings)     │
│ 2. Textual Search (fallback)        │
│ 3. Hybrid Ranking                   │
└─────────────────────────────────────┘
```

## 🔍 Como Funciona a Busca

### 1. Busca Semântica (Embeddings)

Quando um embedding service está disponível:

```csharp
// Query do usuário
string query = "Como fazer uma bebida energizante matinal?";

// 1. Gerar embedding da query  
var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

// 2. Buscar memórias por similaridade
var memories = await storage.SearchBySimilarityAsync(queryEmbedding, userId, limit: 5);

// 3. Resultado: Encontra "João prefere café forte pela manhã"
// Similaridade: 0.78 (78%) - mesmo sem palavras exatas em comum!
```

### 2. Busca Híbrida (Fallback)

Quando embeddings não estão disponíveis:

```csharp
// Query: "estudar hoje" 
// 1. Extração de palavras-chave: ["estudar"]
// 2. Mapeamento de sinônimos: ["estudar", "trabalhar", "manhã"]  
// 3. Busca SQL otimizada:

SELECT *, (RelevanceScore * 0.7 + 
           CASE WHEN datetime(UpdatedAt) > datetime('now', '-1 day') 
           THEN 0.3 ELSE 0.1 END) as SearchScore
FROM UserMemory 
WHERE UserId = ? AND IsActive = 1 AND (
    LOWER(Content) LIKE '%estudar%' OR 
    LOWER(Content) LIKE '%trabalhar%' OR 
    LOWER(Content) LIKE '%manhã%'
)
ORDER BY SearchScore DESC;
```

### 3. Sistema de Deduplicação

Previne memórias duplicadas usando similaridade:

```csharp
// Antes de salvar nova memória
var newMemory = "João gosta de café forte";
var isDuplicate = await IsDuplicateMemoryAsync(newMemory, context);

if (isDuplicate) {
    // Similaridade > 75% com memória existente "João prefere café forte"
    // Sistema automaticamente previne duplicação
    return "Memória similar já existe, não adicionada.";
}
```

## 📊 Comparação: Textual vs Semântica

### Exemplo Prático

```csharp
// Setup dos dois sistemas
var textualStorage = new SqliteStorage("textual.db");
var vectorStorage = new VectorSqliteStorage("vector.db", embeddingService);

// Memória armazenada: "João prefere café expresso forte pela manhã"

// TESTE: Query = "Como preparar bebida energizante matinal?"

// BUSCA TEXTUAL
var textualResults = await textualStorage.SearchMemoriesAsync(query, userId, 5);
// Resultado: [] (vazio - nenhuma palavra em comum)

// BUSCA SEMÂNTICA  
var semanticResults = await vectorStorage.SearchMemoriesAsync(query, userId, 5);
// Resultado: ["João prefere café expresso forte pela manhã"] 
// Similaridade: 0.73 (conceitos relacionados!)
```

### Vantagens da Busca Semântica

| Característica | Busca Textual | Busca Semântica |
|---------------|---------------|-----------------|
| **Correspondência exata** | ✅ Rápida | ✅ Rápida |
| **Sinônimos** | ❌ Limitado | ✅ Automático |
| **Conceitos relacionados** | ❌ Não suporta | ✅ Excelente |
| **Contexto semântico** | ❌ Básico | ✅ Avançado |
| **Dependência externa** | ❌ Nenhuma | ⚠️ API OpenAI |
| **Performance** | ✅ Muito rápida | ⚠️ Moderada |

## 🚀 Configuração e Uso

### Configuração Básica

```csharp
public async Task ConfigurarBuscaSemantica()
{
    // 1. Configurar serviço de embeddings
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://api.openai.com";
    
    var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
    
    // 2. Configurar storage vetorial
    var storage = new VectorSqliteStorage("Data Source=semantic.db", embeddingService);
    await storage.InitializeAsync();
    
    // 3. Configurar agent
    var agent = new Agent<UserContext, string>(model, storage: storage)
        .WithPersona("Assistente com memória semântica avançada")
        .WithInstructions(@"
            Você tem acesso a um sistema de memória semântica que entende 
            relacionamentos conceituais. Use esse contexto para fornecer 
            respostas mais relevantes e personalizadas.")
        .WithContext(new UserContext { 
            UserId = "semantic_user", 
            SessionId = "semantic_session" 
        });
    
    return agent;
}
```

### Exemplo Completo de Uso

```csharp
public async Task ExemploCompletoSemantica()
{
    // Setup
    var agent = await ConfigurarBuscaSemantica();
    
    // === CONVERSA 1: Estabelecer preferências ===
    await agent.ExecuteAsync(@"
        Olá! Sou desenvolvedor Python especializado em machine learning. 
        Trabalho principalmente com redes neurais e deep learning.
        Adoro projetos de visão computacional.");
    
    // Sistema salva memórias com embeddings:
    // - "desenvolvedor Python especializado em machine learning"  
    // - "trabalha com redes neurais e deep learning"
    // - "adora projetos de visão computacional"
    
    // === CONVERSA 2: Busca semântica em ação ===
    var response = await agent.ExecuteAsync("Que frameworks recomendam para IA?");
    
    // Sistema encontra memórias relacionadas automaticamente:
    // "machine learning" ↔ "IA"
    // "redes neurais" ↔ "frameworks para IA"
    // "Python" ↔ "frameworks"
    
    // Resposta contextualizada: "Para IA e machine learning em Python, 
    // recomendo TensorFlow ou PyTorch, especialmente para redes neurais..."
    
    // === CONVERSA 3: Conexões conceituais avançadas ===
    var response2 = await agent.ExecuteAsync("Como processar imagens automaticamente?");
    
    // Sistema conecta: "processar imagens" ↔ "visão computacional"
    // Resposta: "Para visão computacional, que é sua área de interesse..."
}
```

### Exemplo com Fallback

```csharp
public async Task ExemploComFallback()
{
    // Storage sem embedding service (fallback automático)
    var storage = new VectorSqliteStorage("fallback.db", embeddingService: null);
    var agent = new Agent<Context, string>(model, storage: storage);
    
    // Primeira conversa
    await agent.ExecuteAsync("Adoro trabalhar pela manhã com café forte");
    
    // Segunda conversa - busca híbrida com sinônimos
    var result = await agent.ExecuteAsync("Quando é melhor estudar?");
    
    // Sistema conecta: "estudar" → sinônimo → "trabalhar" → "manhã"  
    // Resposta: "Baseado no seu perfil, recomendo estudar pela manhã..."
}
```

## 🎛️ Configurações Avançadas

### Personalizar Modelo de Embedding

```csharp
// Diferentes modelos de embedding
var embedding_ada = new OpenAIEmbeddingService(apiKey, endpoint, model: "text-embedding-ada-002");      // 1536 dim
var embedding_3_large = new OpenAIEmbeddingService(apiKey, endpoint, model: "text-embedding-3-large");  // 3072 dim
var embedding_3_small = new OpenAIEmbeddingService(apiKey, endpoint, model: "text-embedding-3-small");  // 1536 dim
```

### Ajustar Threshold de Similaridade

```csharp
public class CustomVectorStorage : VectorSqliteStorage
{
    protected override double SimilarityThreshold => 0.6; // 60% em vez de 50% padrão
    
    protected override async Task<List<UserMemory>> FilterBySimilarity(
        List<(UserMemory memory, double similarity)> candidates, 
        int limit)
    {
        return candidates
            .Where(c => c.similarity > SimilarityThreshold)
            .OrderByDescending(c => c.similarity * 0.7 + GetRecencyScore(c.memory) * 0.3) // Peso customizado
            .Take(limit)
            .Select(c => c.memory)
            .ToList();
    }
}
```

### Configurar Sinônimos Customizados

```csharp
public class CustomMemoryManager : MemoryManager
{
    protected override Dictionary<string, List<string>> GetSynonymMap()
    {
        return new Dictionary<string, List<string>>
        {
            // Sinônimos técnicos
            { "javascript", new[] { "js", "node", "frontend", "web" } },
            { "inteligencia", new[] { "ia", "ai", "artificial", "machine", "ml" } },
            { "backend", new[] { "servidor", "api", "rest", "microservice" } },
            
            // Sinônimos de domínio específico
            { "paciente", new[] { "cliente", "usuario", "pessoa", "individuo" } },
            { "sintoma", new[] { "sinal", "manifestacao", "queixa", "problema" } }
        };
    }
}
```

## 📈 Performance e Otimizações

### Benchmarks

```csharp
// Tempo de busca (média de 100 consultas)
// Dataset: 1000 memórias

Busca Textual:     ~2ms   (sem embeddings)
Busca Híbrida:     ~5ms   (com sinônimos)  
Busca Semântica:   ~50ms  (com embeddings OpenAI)
Busca Fallback:    ~3ms   (embedding service indisponível)
```

### Estratégias de Cache

```csharp
// Cache de embeddings para evitar recalcular
public class CachedEmbeddingService : IEmbeddingService  
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IEmbeddingService _inner;
    
    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        var key = $"embedding_{text.GetHashCode()}";
        if (_cache.TryGetValue(key, out List<float> cached))
            return cached;
            
        var embedding = await _inner.GenerateEmbeddingAsync(text);
        _cache.Set(key, embedding, TimeSpan.FromHours(1));
        return embedding;
    }
}
```

### Processamento em Lote

```csharp
// Otimização para múltiplas memórias
var texts = new List<string> { "texto1", "texto2", "texto3" };
var embeddings = await embeddingService.GenerateEmbeddingsAsync(texts); // Lote único
```

## 🚨 Tratamento de Erros

### Fallback Robusto

```csharp
public async Task<List<UserMemory>> SearchMemoriesAsync(string query, string userId, int limit)
{
    try 
    {
        // Tentar busca semântica primeiro
        if (_embeddingService != null)
        {
            return await SemanticSearchAsync(query, userId, limit);
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning($"Busca semântica falhou: {ex.Message}");
    }
    
    // Fallback para busca híbrida
    return await HybridSearchAsync(query, userId, limit);
}
```

### Monitoring e Logs

```csharp
// Logs estruturados para debugging
_logger.LogInformation("Busca semântica: {Query} → {ResultCount} resultados em {Duration}ms", 
    query, results.Count, stopwatch.ElapsedMilliseconds);

_logger.LogDebug("Top similaridades: {Similarities}", 
    results.Take(3).Select(r => $"{r.Content}: {r.Similarity:P1}"));
```

## 🔮 Casos de Uso Avançados

### 1. Assistente Médico

```csharp
var medicalStorage = new VectorSqliteStorage("medical.db", embeddingService);
var medicalAgent = new Agent<MedicalContext, string>(model, storage: medicalStorage)
    .WithMemoryCategories("Symptom", "Diagnosis", "Treatment", "Medication")
    .WithPersona("Assistente médico especializado");

// Consulta: "dor de cabeça intensa"
// Encontra: "paciente com cefaleia severa", "enxaqueca recorrente"  
// Conexão semântica: dor de cabeça ↔ cefaleia ↔ enxaqueca
```

### 2. Suporte Técnico  

```csharp
// Consulta: "erro de conexão com banco"
// Encontra: "falha na database", "timeout SQL", "problema JDBC"
// Conexão: banco ↔ database ↔ SQL ↔ JDBC
```

### 3. Assistente de Pesquisa

```csharp
// Consulta: "artigos sobre redes neurais"  
// Encontra: "papers deep learning", "estudos machine learning", "CNN research"
// Conexão: redes neurais ↔ deep learning ↔ CNN ↔ machine learning
```

## 🛣️ Roadmap

### Funcionalidades Planejadas

- [ ] **Índices Vetoriais Nativos**: Integração com sqlite-vec ou FAISS
- [ ] **Embeddings Locais**: Modelos rodando localmente sem API externa  
- [ ] **Clustering Semântico**: Agrupamento automático de memórias relacionadas
- [ ] **Ranking Learning**: ML para otimizar relevância dos resultados
- [ ] **Multi-Modal**: Suporte a embeddings de imagens e áudio

### Integrações Futuras

- [ ] **Weaviate/Pinecone**: Suporte para vector databases especializados
- [ ] **Hugging Face**: Modelos de embedding open source
- [ ] **Azure Cognitive Search**: Integração com serviços Microsoft
- [ ] **Elasticsearch**: Busca híbrida textual + vetorial

## ⚡ Quick Start

```csharp
// Setup mínimo para busca semântica
var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
var storage = new VectorSqliteStorage("semantic.db", embeddingService);
await storage.InitializeAsync();

var agent = new Agent<object, string>(model, storage: storage)
    .WithAnonymousMode(true); // Funciona sem autenticação

// Usar normalmente - busca semântica funciona automaticamente!
var result = await agent.ExecuteAsync("Sua mensagem aqui");
```

Este sistema permite que os agentes do AgentSharp tenham uma compreensão muito mais rica do contexto e relacionamentos entre informações, resultando em experiências muito mais inteligentes e personalizadas para os usuários.