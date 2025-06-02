# 🎯 Melhores Práticas

> Guia de melhores práticas para o desenvolvimento com Agents.net

## 📋 Sumário

- [Agentes](#agentes)
- [Workflows](#workflows)
- [Tools](#tools)
- [Modelos](#modelos)
- [Observabilidade](#observabilidade)
- [Segurança](#segurança)

## 🤖 Agentes

### Design de Agentes

1. **Responsabilidade Única**
   - Cada agente deve ter um propósito claro
   - Evite agentes que fazem muitas coisas
   - Divida tarefas complexas em múltiplos agentes

```csharp
// ❌ Agente fazendo muito
var superAgent = new Agent<Context, Result>(model, "SuperAgent")
    .WithReasoning(true)
    .ForTask(ctx => "Pesquise, analise e gere relatório");

// ✅ Agentes especializados
var researcher = new Agent<string, Research>(model, "Researcher")
    .WithReasoning(true)
    .ForTask(ctx => "Pesquise sobre o tema");

var analyzer = new Agent<Research, Analysis>(model, "Analyzer")
    .WithReasoning(true)
    .ForTask(ctx => "Analise os dados");

var reporter = new Agent<Analysis, Report>(model, "Reporter")
    .WithReasoning(true)
    .ForTask(ctx => "Gere o relatório");
```

2. **Personalidade Clara**
   - Defina instruções específicas
   - Use exemplos quando necessário
   - Mantenha consistência no tom

```csharp
// ❌ Instruções vagas
agent.WithPersona("Seja prestativo");

// ✅ Instruções específicas
agent.WithPersona(@"
    Você é um especialista em análise de dados financeiros.
    - Use linguagem técnica apropriada
    - Cite fontes relevantes
    - Forneça insights acionáveis
    - Mantenha foco em métricas chave
");
```

3. **Gerenciamento de Estado**
   - Use tipos fortemente tipados
   - Evite estado mutável quando possível
   - Documente o fluxo de dados

```csharp
// ❌ Estado implícito
public class Context
{
    public Dictionary<string, object> Data { get; set; }
}

// ✅ Estado explícito
public class AnalysisContext
{
    public string Topic { get; init; }
    public List<DataPoint> DataPoints { get; init; }
    public AnalysisParameters Parameters { get; init; }
    public AnalysisResult Result { get; init; }
}
```

## 🔄 Workflows

### Design de Workflows

1. **Composição Clara**
   - Defina steps com nomes descritivos
   - Use tipos fortemente tipados
   - Documente o fluxo de dados

```csharp
// ❌ Workflow confuso
var workflow = new SequentialWorkflow<Context, Result>("Workflow")
    .RegisterStep("Step1", agent1, ...)
    .RegisterStep("Step2", agent2, ...)
    .RegisterStep("Step3", agent3, ...);

// ✅ Workflow claro
var analysisWorkflow = new SequentialWorkflow<AnalysisContext, Report>("Análise")
    .RegisterStep("Coleta de Dados", dataCollector,
        ctx => $"Colete dados sobre: {ctx.Topic}",
        (ctx, res) => ctx.Data = res)
    .RegisterStep("Análise Estatística", statisticsAnalyzer,
        ctx => $"Analise: {ctx.Data}",
        (ctx, res) => ctx.Statistics = res)
    .RegisterStep("Geração de Relatório", reportGenerator,
        ctx => $"Gere relatório com: {ctx.Statistics}",
        (ctx, res) => ctx.Report = res);
```

2. **Validação de Dados**
   - Valide inputs e outputs
   - Use tipos específicos
   - Trate erros apropriadamente

```csharp
// ❌ Sem validação
workflow.RegisterStep("Análise", analyzer,
    ctx => ctx.Data,
    (ctx, res) => ctx.Result = res);

// ✅ Com validação
workflow.RegisterStep("Análise", analyzer,
    ctx =>
    {
        if (ctx.Data == null)
            throw new ArgumentException("Dados não podem ser nulos");
        return ctx.Data;
    },
    (ctx, res) =>
    {
        if (!ValidateAnalysis(res))
            throw new ValidationException("Análise inválida");
        ctx.Result = res;
    });
```

3. **Tratamento de Erros**
   - Implemente retry quando apropriado
   - Log detalhado de erros
   - Fallback para casos críticos

```csharp
var workflow = new AdvancedWorkflow<Context, Result>("Resiliente")
    .WithRetry(maxAttempts: 3)
    .RegisterStep("API", apiClient,
        ctx => ctx.Request,
        (ctx, res) => ctx.Response = res,
        onError: async (ctx, ex) =>
        {
            _logger.LogError($"Erro na API: {ex.Message}");
            ctx.Response = await FallbackAPI.CallAsync(ctx.Request);
        });
```

## 🛠️ Tools

### Design de Tools

1. **Interface Clara**
   - Nome descritivo
   - Documentação clara
   - Validação de input

```csharp
// ❌ Tool confusa
public class MyTool : ITool
{
    public string Name => "tool";
    public string Description => "faz algo";
    
    public async Task<string> ExecuteAsync(string input)
    {
        return DoSomething(input);
    }
}

// ✅ Tool clara
public class DocumentAnalysisTool : ITool
{
    public string Name => "document_analysis";
    public string Description => @"
        Analisa documentos em vários formatos.
        Suporta: PDF, DOCX, TXT
        Retorna: Análise estruturada em JSON
    ";
    
    public async Task<string> ExecuteAsync(string input)
    {
        ValidateInput(input);
        var doc = await ParseDocument(input);
        var analysis = await AnalyzeDocument(doc);
        return JsonSerializer.Serialize(analysis);
    }
    
    private void ValidateInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input não pode ser vazio");
    }
}
```

2. **Gerenciamento de Recursos**
   - Implemente IDisposable quando necessário
   - Use using statements
   - Libere recursos explicitamente

```csharp
public class DatabaseTool : ITool, IDisposable
{
    private readonly DbConnection _connection;
    private bool _disposed;

    public DatabaseTool(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
    }

    public async Task<string> ExecuteAsync(string input)
    {
        using var command = _connection.CreateCommand();
        // Implementação
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
```

3. **Composição de Tools**
   - Use Tool Packs para organização
   - Implemente interfaces comuns
   - Reutilize funcionalidades

```csharp
public class AnalysisToolPack : ToolPack
{
    public override IEnumerable<ITool> GetTools()
    {
        yield return new TextAnalysisTool();
        yield return new StatisticsAnalysisTool();
        yield return new SentimentAnalysisTool();
    }
}

// Uso
agent.WithTools(new AnalysisToolPack());
```

## 🧠 Modelos

### Uso de Modelos

1. **Configuração Apropriada**
   - Ajuste parâmetros por caso
   - Use temperaturas apropriadas
   - Configure limites de tokens

```csharp
// ❌ Configuração genérica
var model = new ModelFactory().CreateModel("openai", new ModelOptions());

// ✅ Configuração específica
var options = new ModelOptions
{
    ModelName = "gpt-4",
    DefaultConfiguration = new ModelConfiguration
    {
        Temperature = 0.2, // Baixa para análise
        MaxTokens = 2048,
        TopP = 0.9,
        FrequencyPenalty = 0.5,
        PresencePenalty = 0.5
    }
};

var model = new ModelFactory().CreateModel("openai", options);
```

2. **Prompts Efetivos**
   - Seja específico
   - Use exemplos
   - Formate a saída

```csharp
// ❌ Prompt vago
var prompt = "Analise os dados";

// ✅ Prompt específico
var prompt = @"
Analise estes dados financeiros e forneça:
1. Principais métricas (ROI, ROE, Margem)
2. Tendências identificadas
3. Recomendações

Formato esperado:
{
    'metrics': { ... },
    'trends': [ ... ],
    'recommendations': [ ... ]
}
";
```

3. **Tratamento de Respostas**
   - Valide o formato
   - Trate erros
   - Parse estruturado

```csharp
try
{
    var response = await model.ExecuteAsync(request);
    var result = JsonSerializer.Deserialize<Analysis>(response.Text);
    
    if (!ValidateAnalysis(result))
        throw new ValidationException("Análise inválida");
        
    return result;
}
catch (JsonException ex)
{
    _logger.LogError("Erro no parsing: " + ex.Message);
    throw new ModelException("Resposta em formato inválido");
}
```

## 📊 Observabilidade

### Logging e Métricas

1. **Logging Estruturado**
   - Use níveis apropriados
   - Inclua contexto
   - Formate para análise

```csharp
// ❌ Log simples
_logger.Log("Erro na análise");

// ✅ Log estruturado
_logger.LogStructured(LogLevel.Error, new
{
    Event = "AnalysisError",
    Context = new
    {
        AgentName = "Analyzer",
        InputSize = input.Length,
        ErrorType = ex.GetType().Name,
        Message = ex.Message
    },
    Timestamp = DateTime.UtcNow
});
```

2. **Métricas Detalhadas**
   - Meça performance
   - Monitore recursos
   - Trace operações

```csharp
public class MetricsCollector
{
    private readonly IMetricsClient _metrics;

    public async Task<T> TrackOperation<T>(
        string name,
        Func<Task<T>> operation)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            _metrics.Increment($"{name}.success");
            return result;
        }
        catch (Exception)
        {
            _metrics.Increment($"{name}.error");
            throw;
        }
        finally
        {
            sw.Stop();
            _metrics.Timing($"{name}.duration", sw.ElapsedMilliseconds);
        }
    }
}
```

3. **Telemetria**
   - Trace workflows
   - Monitore agentes
   - Analise uso

```csharp
workflow.OnStepComplete += (step, metrics) =>
{
    _telemetry.TrackEvent("WorkflowStep", new Dictionary<string, string>
    {
        ["workflow"] = workflow.Name,
        ["step"] = step.Name,
        ["agent"] = step.Agent.Name,
        ["duration"] = metrics.Duration.ToString(),
        ["status"] = metrics.Status.ToString()
    });
};
```

## 🔒 Segurança

### Práticas Seguras

1. **Gerenciamento de Credenciais**
   - Use variáveis de ambiente
   - Nunca hardcode secrets
   - Rotacione chaves regularmente

```csharp
// ❌ Hardcoded
var apiKey = "sk-1234567890";

// ✅ Configuração segura
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new ConfigurationException("API Key não configurada");
```

2. **Validação de Input**
   - Sanitize entradas
   - Valide formatos
   - Limite tamanhos

```csharp
public class SecureInput
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input vazio");
            
        if (input.Length > 1000)
            throw new ArgumentException("Input muito grande");
            
        return HttpUtility.HtmlEncode(input.Trim());
    }
}
```

3. **Controle de Acesso**
   - Implemente autenticação
   - Use autorização
   - Limite recursos

```csharp
public class SecureWorkflow : AdvancedWorkflow<Context, Result>
{
    private readonly IAuthorizationService _auth;

    public override async Task<Result> ExecuteAsync(
        Context context,
        CancellationToken cancellationToken = default)
    {
        if (!await _auth.CanExecute(context.UserId, Name))
            throw new UnauthorizedException();
            
        return await base.ExecuteAsync(context, cancellationToken);
    }
}
```

## 🎯 Próximos Passos

1. **Implemente Estas Práticas**
   - Revise código existente
   - Aplique gradualmente
   - Monitore resultados

2. **Mantenha-se Atualizado**
   - Acompanhe atualizações
   - Participe da comunidade
   - Compartilhe experiências

---

## 📚 Recursos Relacionados

- [Conceitos Fundamentais](core-concepts.md)
- [Guias Avançados](advanced.md)
- [API Reference](api/index.md) 