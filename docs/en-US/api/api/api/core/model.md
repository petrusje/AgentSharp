# 🧠 Model

> Interface com modelos de linguagem para agentes de IA

## 📋 Sumário

- [Interfaces](#interfaces)
- [Classes Base](#classes-base)
- [Configurações](#configurações)
- [Exemplos](#exemplos)

## 🔌 Interfaces

### `IModel`
```csharp
public interface IModel
{
    string Name { get; }
    ModelCapabilities Capabilities { get; }
    Task<ModelResponse> ExecuteAsync(ModelRequest request);
}
```

### `IModelFactory`
```csharp
public interface IModelFactory
{
    IModel CreateModel(string provider, ModelOptions options);
}
```

## 🏗️ Classes Base

### `ModelBase`
```csharp
public abstract class ModelBase : IModel
{
    protected readonly ModelOptions _options;
    protected readonly ILogger _logger;

    public abstract string Name { get; }
    public abstract ModelCapabilities Capabilities { get; }

    protected ModelBase(ModelOptions options, ILogger logger = null);
    public abstract Task<ModelResponse> ExecuteAsync(ModelRequest request);
}
```

### `ModelFactory`
```csharp
public class ModelFactory : IModelFactory
{
    private readonly Dictionary<string, Func<ModelOptions, IModel>> _factories;

    public ModelFactory()
    {
        _factories = new Dictionary<string, Func<ModelOptions, IModel>>
        {
            ["openai"] = options => new OpenAIModel(options),
            ["anthropic"] = options => new AnthropicModel(options),
            ["local"] = options => new LocalModel(options)
        };
    }

    public IModel CreateModel(string provider, ModelOptions options)
    {
        if (!_factories.ContainsKey(provider))
            throw new ArgumentException($"Provider não suportado: {provider}");

        return _factories[provider](options);
    }
}
```

## ⚙️ Configurações

### `ModelOptions`
```csharp
public class ModelOptions
{
    public string ModelName { get; set; }
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }
    public ModelConfiguration DefaultConfiguration { get; set; }
    public Dictionary<string, object> AdditionalOptions { get; set; }
}
```

### `ModelConfiguration`
```csharp
public class ModelConfiguration
{
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2048;
    public double TopP { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;
    public List<string> StopSequences { get; set; }
}
```

### `ModelCapabilities`
```csharp
public class ModelCapabilities
{
    public bool SupportsStreaming { get; set; }
    public bool SupportsVision { get; set; }
    public bool SupportsEmbeddings { get; set; }
    public int MaxContextLength { get; set; }
    public Dictionary<string, object> AdditionalCapabilities { get; set; }
}
```

## 📝 Requisições e Respostas

### `ModelRequest`
```csharp
public class ModelRequest
{
    public string Prompt { get; set; }
    public ModelConfiguration Configuration { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; }
}
```

### `ModelResponse`
```csharp
public class ModelResponse
{
    public string Text { get; set; }
    public double TokensUsed { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

## 📚 Exemplos

### Modelo OpenAI
```csharp
var options = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = "sua-chave-aqui",
    DefaultConfiguration = new ModelConfiguration
    {
        Temperature = 0.7,
        MaxTokens = 2048
    }
};

var model = new ModelFactory().CreateModel("openai", options);

var response = await model.ExecuteAsync(new ModelRequest
{
    Prompt = "Analise este texto",
    Configuration = new ModelConfiguration
    {
        Temperature = 0.5
    }
});
```

### Modelo com Streaming
```csharp
public class StreamingModel : ModelBase
{
    public override async Task<ModelResponse> ExecuteAsync(ModelRequest request)
    {
        var response = new ModelResponse();
        var buffer = new StringBuilder();

        await foreach (var chunk in StreamResponseAsync(request))
        {
            buffer.Append(chunk);
            OnChunkReceived?.Invoke(this, chunk);
        }

        response.Text = buffer.ToString();
        return response;
    }

    private async IAsyncEnumerable<string> StreamResponseAsync(ModelRequest request)
    {
        // Implementação do streaming
    }

    public event EventHandler<string> OnChunkReceived;
}
```

### Modelo com Cache
```csharp
public class CachedModel : ModelBase
{
    private readonly IMemoryCache _cache;
    private readonly IModel _innerModel;

    public CachedModel(IModel model, IMemoryCache cache)
    {
        _innerModel = model;
        _cache = cache;
    }

    public override async Task<ModelResponse> ExecuteAsync(ModelRequest request)
    {
        var cacheKey = ComputeCacheKey(request);

        if (_cache.TryGetValue(cacheKey, out ModelResponse cached))
            return cached;

        var response = await _innerModel.ExecuteAsync(request);
        _cache.Set(cacheKey, response, TimeSpan.FromHours(1));

        return response;
    }
}
```

### Modelo com Retry
```csharp
public class ResilientModel : ModelBase
{
    private readonly IModel _innerModel;
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;

    public ResilientModel(IModel model, int maxAttempts = 3)
    {
        _innerModel = model;
        _maxAttempts = maxAttempts;
        _delay = TimeSpan.FromSeconds(1);
    }

    public override async Task<ModelResponse> ExecuteAsync(ModelRequest request)
    {
        for (int i = 0; i < _maxAttempts; i++)
        {
            try
            {
                return await _innerModel.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                if (i == _maxAttempts - 1)
                    throw;

                _logger.LogWarning($"Tentativa {i + 1} falhou: {ex.Message}");
                await Task.Delay(_delay);
            }
        }

        throw new ModelExecutionException("Todas as tentativas falharam");
    }
}
```

### Modelo Local
```csharp
public class LocalModel : ModelBase
{
    private readonly string _modelPath;
    private readonly object _modelLock = new object();

    public LocalModel(ModelOptions options)
    {
        _modelPath = options.ModelPath;
    }

    public override async Task<ModelResponse> ExecuteAsync(ModelRequest request)
    {
        lock (_modelLock)
        {
            // Implementação do modelo local
        }
    }
}
```

## 🎯 Próximos Passos

1. **Explore os Modelos**
   - [OpenAI](../models/openai.md)
   - [Anthropic](../models/anthropic.md)
   - [Local](../models/local.md)

2. **Aprofunde-se**
   - [Configurações Avançadas](../../advanced.md#modelos)
   - [Melhores Práticas](../../best-practices.md#modelos)
   - [Otimização](../../optimization.md#modelos)

---

## 📚 Recursos Relacionados

- [Agent](agent.md)
- [Workflow](workflow.md)
- [Tool](tool.md) 