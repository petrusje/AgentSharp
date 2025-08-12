# 🧠 Raciocínio Estruturado

> Sistema avançado de análise step-by-step para agentes de IA

## 📖 Visão Geral

O sistema de raciocínio estruturado do AgentSharp permite que agentes decomponham problemas complexos em etapas lógicas, analisem cada componente e sintetizem uma solução coerente.

## 🎯 Componentes Principais

### 1. Decomposição
```csharp
public class ProblemDecomposition
{
    public string MainProblem { get; set; }
    public List<SubProblem> Components { get; set; }
    public Dictionary<string, string> Dependencies { get; set; }
}

// Uso
var decomposer = new Agent<string, ProblemDecomposition>(model, "Decomposer")
    .WithReasoning(true)
    .WithTools(new ReasoningToolPack());

var breakdown = await decomposer.ExecuteAsync(
    "Como implementar um sistema de recomendação?");
```

### 2. Análise
```csharp
public class ComponentAnalysis
{
    public string Component { get; set; }
    public List<string> Requirements { get; set; }
    public List<string> Challenges { get; set; }
    public Dictionary<string, double> Metrics { get; set; }
}

// Uso
var analyzer = new Agent<SubProblem, ComponentAnalysis>(model, "Analyzer")
    .WithReasoning(true)
    .ForTask(ctx => $"Analise profundamente: {ctx.Description}");
```

### 3. Síntese
```csharp
public class Solution
{
    public string Problem { get; set; }
    public List<string> Steps { get; set; }
    public Dictionary<string, object> Components { get; set; }
    public List<string> Recommendations { get; set; }
}

// Uso
var synthesizer = new Agent<List<ComponentAnalysis>, Solution>(model, "Synthesizer")
    .WithReasoning(true)
    .ForTask(ctx => "Sintetize uma solução completa");
```

## 🔄 Processo de Raciocínio

### 1. Preparação
```csharp
public class ReasoningContext
{
    public string Input { get; set; }
    public List<string> Constraints { get; set; }
    public Dictionary<string, object> Resources { get; set; }
}

var reasoner = new Agent<ReasoningContext, Solution>(model, "Reasoner")
    .WithReasoning(true)
    .WithSystemPrompt(@"
        Você é um especialista em análise estruturada.
        Siga estas etapas:
        1. Compreenda o problema completamente
        2. Identifique componentes principais
        3. Analise cada componente
        4. Sintetize uma solução
    ");
```

### 2. Execução
```csharp
// Workflow de raciocínio
var workflow = new SequentialWorkflow<ReasoningContext, Solution>("Análise")
    .RegisterStep("Decomposição", decomposer,
        ctx => $"Decomponha: {ctx.Input}",
        (ctx, res) => ctx.Components = res.Components)
    .RegisterStep("Análise", analyzer,
        ctx => $"Analise: {ctx.Components}",
        (ctx, res) => ctx.Analysis = res)
    .RegisterStep("Síntese", synthesizer,
        ctx => $"Sintetize baseado em: {ctx.Analysis}",
        (ctx, res) => ctx.Solution = res);
```

### 3. Validação
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public double Confidence { get; set; }
    public List<string> Issues { get; set; }
    public Dictionary<string, double> Scores { get; set; }
}

var validator = new Agent<Solution, ValidationResult>(model, "Validator")
    .WithReasoning(true)
    .ForTask(ctx => "Valide esta solução criticamente");
```

## 🛠️ Tools de Raciocínio

### 1. Decomposição
```csharp
public class DecompositionTool : ITool
{
    public string Name => "decompose";
    public string Description => "Decompõe problemas em componentes";

    public async Task<string> ExecuteAsync(string input)
    {
        // Lógica de decomposição
        return JsonSerializer.Serialize(new ProblemDecomposition
        {
            MainProblem = input,
            Components = IdentifyComponents(input)
        });
    }
}
```

### 2. Análise Crítica
```csharp
public class CriticalAnalysisTool : ITool
{
    public string Name => "analyze";
    public string Description => "Analisa criticamente componentes";

    public async Task<string> ExecuteAsync(string input)
    {
        var analysis = new ComponentAnalysis();
        // Lógica de análise
        return JsonSerializer.Serialize(analysis);
    }
}
```

### 3. Validação
```csharp
public class ValidationTool : ITool
{
    public string Name => "validate";
    public string Description => "Valida soluções propostas";

    public async Task<string> ExecuteAsync(string input)
    {
        var validation = new ValidationResult();
        // Lógica de validação
        return JsonSerializer.Serialize(validation);
    }
}
```

## 📊 Métricas e Observabilidade

### 1. Métricas de Raciocínio
```csharp
public class ReasoningMetrics
{
    public int StepsCount { get; set; }
    public TimeSpan ThinkingTime { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, double> ComponentScores { get; set; }
}

// Uso
workflow.OnStepComplete += (step, metrics) =>
{
    Console.WriteLine($"Passo: {step.Name}");
    Console.WriteLine($"Tempo: {metrics.ThinkingTime}");
    Console.WriteLine($"Confiança: {metrics.Confidence:P2}");
};
```

### 2. Logs Estruturados
```csharp
public class ReasoningLog
{
    public string Step { get; set; }
    public string Thought { get; set; }
    public string Action { get; set; }
    public string Result { get; set; }
}

// Uso
_logger.LogStructured(LogLevel.Debug, new ReasoningLog
{
    Step = "Decomposição",
    Thought = "Identificando componentes principais",
    Action = "Aplicando análise hierárquica",
    Result = "3 componentes identificados"
});
```

## 🎯 Melhores Práticas

### 1. Design de Prompts
- Use instruções claras e estruturadas
- Defina critérios de sucesso
- Forneça exemplos quando necessário

```csharp
var prompt = @"
Analise este problema seguindo estas etapas:
1. Identifique o objetivo principal
2. Liste componentes chave
3. Analise dependências
4. Proponha solução estruturada

Formato esperado:
{
    'objetivo': 'string',
    'componentes': ['string'],
    'dependencias': {'chave': 'valor'},
    'solucao': ['string']
}
";
```

### 2. Validação de Resultados
- Verifique completude
- Valide coerência
- Meça confiança

```csharp
public class ResultValidator
{
    public bool Validate(Solution solution)
    {
        return solution.Steps.Count > 0 &&
               solution.Components.Any() &&
               ValidateCoherence(solution);
    }
}
```

### 3. Tratamento de Erros
- Detecte falhas de raciocínio
- Implemente retry com ajustes
- Mantenha logs detalhados

```csharp
try
{
    var result = await reasoner.ExecuteAsync(context);
    if (!validator.Validate(result))
    {
        result = await RetryWithAdjustments(context);
    }
}
catch (ReasoningException ex)
{
    _logger.LogError("Falha no raciocínio", ex);
    // Implementar fallback
}
```

## 🎯 Próximos Passos

1. **Explore os Exemplos**
   - Veja `ExemplosRaciocinio.cs`
   - Teste diferentes cenários
   - Analise os logs

2. **Customize o Raciocínio**
   - Ajuste prompts
   - Adicione tools específicas
   - Implemente validações

3. **Integre em Workflows**
   - Combine com outros agentes
   - Adicione métricas
   - Implemente retry

---

## 📚 Recursos Relacionados

- [Conceitos Fundamentais](core-concepts.md)
- [Sistema de Workflows](workflows.md)
- [API Reference](api/index.md) 