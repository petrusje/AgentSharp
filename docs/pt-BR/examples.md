# 📚 Exemplos Práticos

> Casos de uso reais do AgentSharp em ação

## 🎯 Exemplos Básicos

### 1. Jornalista com Personalidade
Demonstra como criar um agente com personalidade definida.

```csharp
var jornalista = new Agent<string, string>(model, "Jornalista")
    .WithPersona(@"Você é um jornalista mineiro, use expressões típicas de Minas Gerais.
                  Seja objetivo mas mantenha o sotaque característico.")
    .WithReasoning(true);

var noticia = await jornalista.ExecuteAsync(
    "Escreva uma notícia sobre o aumento do preço do pão de queijo");
```

### 2. Jornalista com Busca Web
Integra ferramentas de busca para enriquecer o conteúdo.

```csharp
var reporter = new Agent<string, string>(model, "Reporter")
    .WithTools(new SearchToolPack())
    .WithPersona("Você é um repórter investigativo que busca fontes confiáveis");

var reportagem = await reporter.ExecuteAsync(
    "Faça uma reportagem sobre startups em Belo Horizonte");
```

### 3. Analista Financeiro
Utiliza ferramentas financeiras para análises precisas.

```csharp
var analista = new Agent<AnaliseContext, Report>(model, "Analista")
    .WithTools(new FinanceToolPack())
    .WithReasoning(true);

var relatorio = await analista.ExecuteAsync(new AnaliseContext 
{
    Ticker = "PETR4",
    Periodo = "1Y"
});
```

## 🧠 Exemplos de Raciocínio

### 1. Resolvedor de Problemas
Demonstra o processo de decomposição e análise.

```csharp
var solver = new Agent<ProblemContext, Solution>(model, "Solver")
    .WithReasoning(true)
    .WithTools(new ReasoningToolPack());

var solucao = await solver.ExecuteAsync(new ProblemContext 
{
    Problem = "Como otimizar o processo de vendas?",
    Constraints = new[] { "Orçamento limitado", "Equipe pequena" }
});
```

### 2. Avaliador de Soluções
Analisa criticamente propostas de solução.

```csharp
var avaliador = new Agent<ProposalContext, Evaluation>(model, "Avaliador")
    .WithReasoning(true)
    .ForTask(ctx => $"Avalie criticamente esta proposta: {ctx.Proposal}");

var avaliacao = await avaliador.ExecuteAsync(new ProposalContext 
{
    Proposal = "Sistema de IA para atendimento",
    Criteria = new[] { "Custo", "Viabilidade", "ROI" }
});
```

### 3. Identificador de Obstáculos
Mapeia riscos e desafios em projetos.

```csharp
var identificador = new Agent<ProjectContext, RiskAnalysis>(model, "Identificador")
    .WithReasoning(true)
    .WithTools(new RiskAnalysisToolPack());

var analise = await identificador.ExecuteAsync(new ProjectContext 
{
    Project = "Migração para Cloud",
    Scope = "Infraestrutura completa"
});
```

## 📊 Exemplos Structured Outputs

### 1. Análise de Documentos
Processamento estruturado de documentos empresariais.

```csharp
public class DocumentAnalysis
{
    public string Title { get; set; }
    public List<string> KeyPoints { get; set; }
    public Dictionary<string, double> Metrics { get; set; }
}

var analisador = new Agent<string, DocumentAnalysis>(model, "Analisador")
    .WithStructuredOutput<DocumentAnalysis>();

var analise = await analisador.ExecuteAsync(documentoTexto);
```

### 2. Análise de Currículos
Extração estruturada de informações de CVs.

```csharp
public class CurriculumAnalysis
{
    public Candidate Candidate { get; set; }
    public List<Experience> Experiences { get; set; }
    public List<Skill> Skills { get; set; }
    public double MatchScore { get; set; }
}

var analisadorCV = new Agent<CVContext, CurriculumAnalysis>(model, "AnalisadorCV")
    .WithStructuredOutput<CurriculumAnalysis>();

var resultado = await analisadorCV.ExecuteAsync(new CVContext 
{
    CV = cvText,
    JobDescription = jobDesc
});
```

## 🔄 Exemplos Workflow

### 1. Workflow Multi-etapa
Pipeline completo de processamento.

```csharp
public class ResearchContext
{
    public string Topic { get; set; }
    public string Research { get; set; }
    public string Analysis { get; set; }
    public string Report { get; set; }
}

var workflow = new AdvancedWorkflow<ResearchContext, string>("Pesquisa")
    .WithDebugMode(true)
    .WithTelemetry(true)
    .RegisterStep("Pesquisa", pesquisador,
        ctx => $"Pesquise sobre: {ctx.Topic}",
        (ctx, res) => ctx.Research = res)
    .RegisterStep("Análise", analista,
        ctx => $"Analise: {ctx.Research}",
        (ctx, res) => ctx.Analysis = res)
    .RegisterStep("Relatório", relator,
        ctx => $"Crie relatório baseado em:\n{ctx.Analysis}",
        (ctx, res) => ctx.Report = res);

var resultado = await workflow.ExecuteAsync(new ResearchContext 
{
    Topic = "Impacto da IA no mercado de trabalho"
});
```

### 2. Workflow com Retry
Implementação resiliente com retry automático.

```csharp
var workflow = new AdvancedWorkflow<APIContext, Response>("API")
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .RegisterStep("Chamada", apiClient,
        ctx => $"Call: {ctx.Endpoint}",
        (ctx, res) => ctx.Response = res,
        onError: async (ctx, ex) => await HandleError(ex));
```

### 3. Workflow com Validação
Validação entre passos do workflow.

```csharp
var workflow = new AdvancedWorkflow<DataContext, Report>("Validação")
    .RegisterStep("Coleta", coletor,
        ctx => "Coletar dados",
        (ctx, res) => 
        {
            if (!ValidateData(res))
                throw new ValidationException("Dados inválidos");
            ctx.Data = res;
        })
    .RegisterStep("Processamento", processador,
        ctx => $"Processar: {ctx.Data}",
        (ctx, res) => ctx.Result = res);
```

## 🎯 Próximos Passos

1. **Experimente os Exemplos**
   - Clone o repositório
   - Execute cada exemplo
   - Modifique e adapte

2. **Crie Seus Agentes**
   - Defina personalidades
   - Adicione ferramentas
   - Implemente raciocínio

3. **Desenvolva Workflows**
   - Planeje os passos
   - Implemente validações
   - Adicione resiliência

---

## 📚 Recursos Relacionados

- [Conceitos Fundamentais](core-concepts.md)
- [Sistema de Workflows](workflows.md)
- [API Reference](api/index.md) 