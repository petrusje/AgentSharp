# 🛠️ Tool

> Sistema de ferramentas extensível para agentes de IA

## 📋 Sumário

- [Interfaces](#interfaces)
- [Classes Base](#classes-base)
- [Tool Packs](#tool-packs)
- [Exemplos](#exemplos)

## 🔌 Interfaces

### `ITool`
```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input);
}
```

### `IToolPack`
```csharp
public interface IToolPack
{
    IEnumerable<ITool> GetTools();
}
```

## 🏗️ Classes Base

### `ToolBase`
```csharp
public abstract class ToolBase : ITool
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    protected readonly ILogger _logger;

    protected ToolBase(ILogger logger = null);
    public abstract Task<string> ExecuteAsync(string input);
    protected virtual void ValidateInput(string input);
}
```

### `ToolPack`
```csharp
public abstract class ToolPack : IToolPack
{
    protected readonly ILogger _logger;
    protected readonly List<ITool> _tools;

    protected ToolPack(ILogger logger = null);
    public abstract IEnumerable<ITool> GetTools();
    protected void RegisterTool(ITool tool);
}
```

## 🧰 Tool Packs Inclusos

### `SearchToolPack`
```csharp
public class SearchToolPack : ToolPack
{
    public SearchToolPack(ILogger logger = null);

    public override IEnumerable<ITool> GetTools()
    {
        yield return new WebSearchTool();
        yield return new DocumentSearchTool();
        yield return new VectorSearchTool();
    }
}
```

### `ReasoningToolPack`
```csharp
public class ReasoningToolPack : ToolPack
{
    public ReasoningToolPack(ILogger logger = null);

    public override IEnumerable<ITool> GetTools()
    {
        yield return new DecompositionTool();
        yield return new AnalysisTool();
        yield return new ValidationTool();
    }
}
```

## 🔧 Ferramentas Comuns

### `WebSearchTool`
```csharp
public class WebSearchTool : ToolBase
{
    public override string Name => "web_search";
    public override string Description => "Realiza buscas na web";

    public override async Task<string> ExecuteAsync(string input)
    {
        // Implementação da busca web
    }
}
```

### `DocumentSearchTool`
```csharp
public class DocumentSearchTool : ToolBase
{
    public override string Name => "doc_search";
    public override string Description => "Busca em documentos";

    public override async Task<string> ExecuteAsync(string input)
    {
        // Implementação da busca em documentos
    }
}
```

### `VectorSearchTool`
```csharp
public class VectorSearchTool : ToolBase
{
    public override string Name => "vector_search";
    public override string Description => "Busca semântica em vetores";

    public override async Task<string> ExecuteAsync(string input)
    {
        // Implementação da busca vetorial
    }
}
```

## 📚 Exemplos

### Criar Tool Customizada
```csharp
public class MyCustomTool : ToolBase
{
    public override string Name => "my_tool";
    public override string Description => "Ferramenta customizada";

    public override async Task<string> ExecuteAsync(string input)
    {
        ValidateInput(input);
        
        try
        {
            // Implementação
            var result = await ProcessInput(input);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro em {Name}: {ex.Message}");
            throw new ToolExecutionException(Name, ex.Message);
        }
    }

    protected override void ValidateInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input não pode ser vazio");
    }
}
```

### Criar Tool Pack Customizado
```csharp
public class MyToolPack : ToolPack
{
    public MyToolPack(ILogger logger = null) : base(logger)
    {
        RegisterTool(new MyCustomTool());
        RegisterTool(new AnotherTool());
    }

    public override IEnumerable<ITool> GetTools()
    {
        return _tools;
    }
}
```

### Usar Tools em Agente
```csharp
var agent = new Agent<string, string>(model, "Assistente")
    .WithTools(new SearchToolPack())
    .WithTools(new MyToolPack())
    .WithReasoning(true);

var result = await agent.ExecuteAsync("Pesquise e analise");
```

### Tool com Configuração
```csharp
public class APITool : ToolBase
{
    private readonly string _apiKey;
    private readonly string _endpoint;

    public APITool(string apiKey, string endpoint)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
    }

    public override async Task<string> ExecuteAsync(string input)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", _apiKey);
        
        var response = await client.GetAsync($"{_endpoint}?q={input}");
        return await response.Content.ReadAsStringAsync();
    }
}
```

### Tool com Cache
```csharp
public class CachedSearchTool : ToolBase
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration;

    public CachedSearchTool(IMemoryCache cache)
    {
        _cache = cache;
        _cacheExpiration = TimeSpan.FromHours(1);
    }

    public override async Task<string> ExecuteAsync(string input)
    {
        var cacheKey = $"search:{input}";
        
        if (_cache.TryGetValue(cacheKey, out string cached))
            return cached;

        var result = await PerformSearch(input);
        _cache.Set(cacheKey, result, _cacheExpiration);
        
        return result;
    }
}
```

### Tool com Retry
```csharp
public class ResilientTool : ToolBase
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;

    public ResilientTool(int maxAttempts = 3)
    {
        _maxAttempts = maxAttempts;
        _delay = TimeSpan.FromSeconds(1);
    }

    public override async Task<string> ExecuteAsync(string input)
    {
        for (int i = 0; i < _maxAttempts; i++)
        {
            try
            {
                return await ExecuteInternalAsync(input);
            }
            catch (Exception ex)
            {
                if (i == _maxAttempts - 1)
                    throw;
                    
                _logger.LogWarning($"Tentativa {i + 1} falhou: {ex.Message}");
                await Task.Delay(_delay);
            }
        }
        
        throw new ToolExecutionException(Name, "Todas as tentativas falharam");
    }
}
```

## 🎯 Próximos Passos

1. **Explore os Tool Packs**
   - [Search Tools](../tools/search-tools.md)
   - [Reasoning Tools](../tools/reasoning-tools.md)
   - [Integration Tools](../tools/integration-tools.md)

2. **Crie suas Tools**
   - [Guia de Tool](../../guides/tool-basic.md)
   - [Tool Pack](../../guides/tool-pack.md)
   - [Melhores Práticas](../../best-practices.md)

---

## 📚 Recursos Relacionados

- [Agent](agent.md)
- [Workflow](workflow.md)
- [Model](model.md) 