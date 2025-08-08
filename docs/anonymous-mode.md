# Modo Anônimo 🎭

O **Modo Anônimo** do AgentSharp permite que agentes funcionem sem necessidade de autenticação prévia, gerando automaticamente identificadores únicos para usuários e sessões.

## Visão Geral

O modo anônimo resolve a necessidade comum de ter agentes funcionais em cenários onde:
- Não há sistema de autenticação implementado
- Queremos permitir acesso sem login obrigatório
- Precisamos de sessões únicas para demonstrações ou protótipos
- Testes automatizados requerem IDs consistentes mas únicos

## Configuração Básica

### Modo Totalmente Anônimo

```csharp
// Sem contexto - tudo será gerado automaticamente
var agent = new Agent<object, string>(model, "Assistant")
    .WithAnonymousMode(true)
    .WithStorage(storage);

var result = await agent.ExecuteAsync("Olá! Como você pode me ajudar?");

// Informações da sessão geradas automaticamente
Console.WriteLine($"User ID: {result.SessionInfo.UserId}");        // anonymous_a1b2c3d4
Console.WriteLine($"Session ID: {result.SessionInfo.SessionId}");  // 550e8400-e29b-41d4-a716-446655440000
Console.WriteLine($"Foi gerado: {result.SessionInfo.WasGenerated}"); // true
Console.WriteLine($"É anônimo: {result.SessionInfo.IsAnonymous}");   // true
```

### Modo Híbrido (Contexto Parcial)

```csharp
// Fornecendo apenas UserId - SessionId será gerado
var context = new UserContext { UserId = "john_doe" };

var agent = new Agent<UserContext, string>(model, "Assistant")
    .WithAnonymousMode(true)
    .WithContext(context)
    .WithStorage(storage);

var result = await agent.ExecuteAsync("Lembra de mim?");
// result.SessionInfo.UserId = "john_doe" (fornecido)
// result.SessionInfo.SessionId = "guid-gerado" (automático)
// result.SessionInfo.WasGenerated = true (SessionId foi gerado)
// result.SessionInfo.IsAnonymous = false (UserId não é anônimo)
```

## Funcionalidades

### 1. Geração Automática de IDs

- **User ID**: Formato `anonymous_xxxxxxxx` (8 caracteres aleatórios)
- **Session ID**: GUID completo para garantir unicidade
- **Consistência**: Mesmos IDs durante toda a vida do agent

### 2. SessionInfo na Resposta

Toda execução de agent retorna informações detalhadas da sessão:

```csharp
public class SessionInfo
{
    public string UserId { get; set; }      // ID do usuário
    public string SessionId { get; set; }   // ID da sessão  
    public bool WasGenerated { get; set; }  // Se algum ID foi gerado automaticamente
    public bool IsAnonymous => UserId?.StartsWith("anonymous_") == true;
}

var result = await agent.ExecuteAsync("Teste");
var info = result.SessionInfo;

// Verificações úteis
if (info.IsAnonymous) 
{
    Console.WriteLine("Usuário anônimo detectado");
}

if (info.WasGenerated)
{
    Console.WriteLine($"IDs para rastreamento: {info.UserId} / {info.SessionId}");
}
```

### 3. Compatibilidade com Storage

O modo anônimo funciona com todos os storage providers:

```csharp
// Com InMemoryStorage (desenvolvimento)
var memStorage = new InMemoryStorage();
var devAgent = new Agent<object, string>(model, "DevAssistant")
    .WithAnonymousMode(true)
    .WithStorage(memStorage);

// Com SQLiteStorage (produção)
var sqlStorage = new SqliteStorage("Data Source=anonymous_sessions.db");
await sqlStorage.InitializeAsync();

var prodAgent = new Agent<object, string>(model, "ProdAssistant")
    .WithAnonymousMode(true)
    .WithStorage(sqlStorage);
```

### 4. Persistência de Memória

Mesmo em modo anônimo, as memórias são preservadas:

```csharp
var agent = new Agent<object, string>(model, "MemoryAssistant")
    .WithAnonymousMode(true)
    .WithStorage(new SqliteStorage("Data Source=anon_memory.db"));

// Primeira conversa
await agent.ExecuteAsync("Meu nome é João e gosto de café forte");

// Segunda conversa (mesma sessão) 
var result = await agent.ExecuteAsync("Qual é meu nome e minha preferência?");
// Agent lembra: "João gosta de café forte"

Console.WriteLine($"Session: {result.SessionInfo.SessionId}"); // Mesmo ID
```

## Casos de Uso

### 1. Aplicações Web sem Login Obrigatório

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        // Agent anônimo por requisição
        var agent = new Agent<object, string>(_model, "WebAssistant")
            .WithAnonymousMode(true)
            .WithStorage(_storage);

        var result = await agent.ExecuteAsync(request.Message);
        
        return Ok(new ChatResponse 
        {
            Message = result.Data,
            SessionId = result.SessionInfo.SessionId,  // Cliente pode usar para continuidade
            UserId = result.SessionInfo.UserId
        });
    }
}
```

### 2. Demonstrações e Protótipos

```csharp
public async Task DemonstrarRecursos()
{
    var demoAgent = new Agent<object, string>(model, "DemoAssistant")
        .WithAnonymousMode(true)
        .WithPersona("Assistente para demonstrações que mostra todos os recursos");

    Console.WriteLine("=== DEMO INTERATIVA ===");
    
    while (true)
    {
        Console.Write("Você: ");
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) break;

        var result = await demoAgent.ExecuteAsync(input);
        Console.WriteLine($"Assistant: {result.Data}");
        Console.WriteLine($"[Sessão: {result.SessionInfo.SessionId[..8]}...]");
    }
}
```

### 3. Testes Automatizados

```csharp
[TestMethod]
public async Task TesteConsistenciaAnonimaAgent()
{
    // Arrange
    var agent = new Agent<object, string>(_mockModel, "TestAgent")
        .WithAnonymousMode(true)
        .WithStorage(_storage);

    // Act
    var result1 = await agent.ExecuteAsync("Primeira mensagem");
    var result2 = await agent.ExecuteAsync("Segunda mensagem");

    // Assert - IDs devem ser consistentes na mesma instância
    Assert.AreEqual(result1.SessionInfo.UserId, result2.SessionInfo.UserId);
    Assert.AreEqual(result1.SessionInfo.SessionId, result2.SessionInfo.SessionId);
    Assert.IsTrue(result1.SessionInfo.IsAnonymous);
    Assert.IsTrue(result2.SessionInfo.WasGenerated);
}
```

### 4. Sistemas com Autenticação Opcional

```csharp
public class FlexibleAgent
{
    private readonly IModel _model;
    private readonly IStorage _storage;

    public async Task<AgentResult<string>> ProcessRequest(string message, UserContext context = null)
    {
        var agent = new Agent<UserContext, string>(_model, "FlexibleAssistant")
            .WithStorage(_storage);

        // Se não há contexto de usuário, ativar modo anônimo
        if (context == null || string.IsNullOrEmpty(context.UserId))
        {
            agent.WithAnonymousMode(true);
        }
        else
        {
            agent.WithContext(context);
        }

        return await agent.ExecuteAsync(message);
    }
}
```

## Considerações de Segurança

### 1. Isolamento de Sessões

```csharp
// ❌ Problemático - sharing de agent entre requests
private static Agent<object, string> _sharedAgent; // NÃO fazer isso

// ✅ Correto - agent por request/usuário
public async Task<string> ProcessUserRequest(string message)
{
    var agent = new Agent<object, string>(_model, "RequestAgent")
        .WithAnonymousMode(true)
        .WithStorage(_storage);
        
    var result = await agent.ExecuteAsync(message);
    return result.Data;
}
```

### 2. Limpeza de Dados

```csharp
// Implementar limpeza periódica de sessões anônimas antigas
public async Task CleanupAnonymousSessions()
{
    var storage = new SqliteStorage("Data Source=sessions.db");
    
    // Limpar sessões anônimas mais antigas que 7 dias
    await storage.Memories.DeleteOldMemoriesAsync(
        userId: "anonymous_%", // Padrão para usuários anônimos
        olderThanDays: 7
    );
}
```

### 3. Rate Limiting

```csharp
public class AnonymousRateLimiter
{
    private readonly Dictionary<string, DateTime> _lastAccess = new();
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    public bool CanProcess(string sessionId)
    {
        if (!_lastAccess.TryGetValue(sessionId, out var lastTime))
        {
            _lastAccess[sessionId] = DateTime.Now;
            return true;
        }

        if (DateTime.Now - lastTime >= _cooldown)
        {
            _lastAccess[sessionId] = DateTime.Now;
            return true;
        }

        return false; // Rate limited
    }
}
```

## Limitações

### 1. Gestão de Estado

- **Escopo**: IDs são únicos apenas dentro da instância do agent
- **Persistência**: Reiniciar a aplicação gera novos IDs (exceto se salvos externamente)
- **Concorrência**: Múltiplas instâncias geram IDs independentes

### 2. Tracking Cross-Session

```csharp
// Para tracking cross-session, salve os IDs gerados
public class SessionTracker
{
    public async Task<SessionInfo> GetOrCreateSession(string externalId = null)
    {
        if (externalId != null)
        {
            // Recuperar sessão existente
            var existing = await LoadSessionAsync(externalId);
            if (existing != null) return existing;
        }

        // Criar nova sessão anônima
        var agent = new Agent<object, string>(_model, "Tracker")
            .WithAnonymousMode(true);
            
        var result = await agent.ExecuteAsync("Inicializando sessão...");
        
        // Salvar para uso futuro
        await SaveSessionAsync(externalId, result.SessionInfo);
        return result.SessionInfo;
    }
}
```

## Melhores Práticas

### 1. Configuração por Ambiente

```csharp
public class AgentFactory
{
    public Agent<TContext, TResult> CreateAgent<TContext, TResult>(
        IModel model, 
        string name,
        bool isDevelopment = false)
    {
        var agent = new Agent<TContext, TResult>(model, name);

        if (isDevelopment)
        {
            // Desenvolvimento: sempre anônimo com InMemory
            return agent
                .WithAnonymousMode(true)
                .WithStorage(new InMemoryStorage());
        }
        else
        {
            // Produção: configuração mais específica
            return agent.WithStorage(new SqliteStorage("Data Source=prod.db"));
        }
    }
}
```

### 2. Logging e Monitoramento

```csharp
public async Task<AgentResult<string>> ProcessWithLogging(string message)
{
    var agent = new Agent<object, string>(_model, "MonitoredAgent")
        .WithAnonymousMode(true)
        .WithStorage(_storage);

    var result = await agent.ExecuteAsync(message);

    // Log para analytics
    _logger.LogInformation("Session processed: {SessionId}, Anonymous: {IsAnonymous}", 
        result.SessionInfo.SessionId, 
        result.SessionInfo.IsAnonymous);

    return result;
}
```

### 3. Fallback Graceful

```csharp
public async Task<AgentResult<string>> ProcessWithFallback(
    string message, 
    UserContext context = null)
{
    try
    {
        var agent = new Agent<UserContext, string>(_model, "PrimaryAgent");

        if (context?.IsValid == true)
        {
            agent.WithContext(context);
        }
        else
        {
            // Fallback para modo anônimo
            agent.WithAnonymousMode(true);
            _logger.LogWarning("Falling back to anonymous mode for message: {Message}", 
                message[..Math.Min(50, message.Length)]);
        }

        return await agent.WithStorage(_storage).ExecuteAsync(message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing message, using simple response");
        
        // Fallback final
        return new AgentResult<string>(
            "Desculpe, ocorreu um erro. Tente novamente.", 
            new List<AIMessage>(),
            0,
            new UsageInfo()
        );
    }
}
```

## Conclusão

O modo anônimo do AgentSharp oferece uma solução robusta e flexível para cenários onde a autenticação tradicional não é necessária ou desejada. Com geração automática de IDs únicos, persistência de memória e total compatibilidade com o sistema de storage, permite criar experiências de usuário fluidas sem comprometer a funcionalidade avançada do framework.