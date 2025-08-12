# 🚀 Guia de Início Rápido

> Comece a usar o AgentSharp em minutos com a nova arquitetura otimizada para custos

## 📋 Pré-requisitos

- .NET Standard 2.0 ou superior
- Chave de API da OpenAI (para memória semântica)

## 🎯 Instalação

1. **Criar novo projeto**
```bash
dotnet new console -n MeuProjeto
cd MeuProjeto
```

2. **Adicionar o pacote AgentSharp**
```bash
dotnet add package AgentSharp
```

3. **Configurar variáveis de ambiente** (opcional para agentes simples)
```bash
# Linux/macOS
export OPENAI_API_KEY="sua-chave-aqui"

# Windows PowerShell
$env:OPENAI_API_KEY="sua-chave-aqui"
```

## 🔥 Primeiro Agente (Baixo Custo)

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Criar modelo
var modelOptions = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
};
var model = new ModelFactory().CreateModel("openai", modelOptions);

// ✅ Agente simples - SEM CUSTOS EXTRAS
var agent = new Agent<string, string>(model, "Assistente")
    .WithPersona("Você é um assistente prestativo e amigável")
    .WithAnonymousMode(true); // Não precisa gerenciar IDs

// Executar - custo mínimo
var resultado = await agent.ExecuteAsync("Olá! Como posso ajudar?");
Console.WriteLine(resultado.Data);

// Custo: ~$0.005 por interação
```

## 💰 Comparação de Custos - Escolha Consciente

### Agente Simples vs Inteligente

```csharp
// 💚 BAIXO CUSTO: Para 80% dos casos de uso
var agenteSimples = new Agent<string, string>(model, "Assistente Básico")
    .WithPersona("Assistente direto e eficiente")
    .WithAnonymousMode(true);

// 🚨 ALTO CUSTO: Apenas quando ROI justifica
var agenteInteligente = new Agent<Context, string>(model, "Assistente Inteligente")
    .WithSemanticMemory(storage, embeddingService) // +400% de custo
    .WithReasoning(true) // +50% de custo
    .WithPersona("Assistente com memória contextual avançada");

Console.WriteLine("Agente Simples: ~$15/mês para 100 interações/dia");
Console.WriteLine("Agente Inteligente: ~$84/mês para 100 interações/dia");
```

## 🧠 Quando Usar Memória Semântica

```csharp
// ✅ Use memória semântica APENAS quando:
// • Precisa lembrar contexto entre sessões
// • Busca inteligente em histórico extenso  
// • Relacionamentos conceituais são importantes
// • ROI justifica custo 5x maior

// Exemplo: Assistente Pessoal
var embeddingService = new OpenAIEmbeddingService(
    Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    "https://api.openai.com");

var storage = new VectorSqliteStorage("memoria_pessoal.db", embeddingService);

var assistentePessoal = new Agent<Context, string>(model, "Assistente Pessoal")
    .WithSemanticMemory(storage, embeddingService) // Opt-in consciente
    .WithPersona("Assistente que me conhece e evolui comigo")
    .WithContext(new Context { UserId = "usuario123", SessionId = "sessao456" });

// Primeira conversa - estabelece preferências
await assistentePessoal.ExecuteAsync("Gosto de reuniões de manhã, depois das 9h");

// Semanas depois - busca semântica automática
var resposta = await assistentePessoal.ExecuteAsync("Quando devo agendar a reunião?");
// Sistema encontra automaticamente: "Considerando sua preferência por manhãs..."
```

## 🔄 Primeiro Workflow

```csharp
// Definir contexto
public class AnaliseContext
{
    public string UserId { get; set; } = "user123";
    public string SessionId { get; set; } = "session456";
    public string Entrada { get; set; }
    public string Analise { get; set; }
    public string Resultado { get; set; }
}

// Agentes especializados (baixo custo individual)
var analisador = new Agent<AnaliseContext, string>(model, "Analisador")
    .WithPersona("Especialista em análise de dados");

var finalizador = new Agent<AnaliseContext, string>(model, "Finalizador")
    .WithPersona("Especialista em conclusões estruturadas");

// Criar workflow
var workflow = new SequentialWorkflow<AnaliseContext, string>("Análise")
    .RegisterStep("Análise", analisador,
        ctx => $"Analise estes dados: {ctx.Entrada}",
        (ctx, res) => ctx.Analise = res)
    .RegisterStep("Conclusão", finalizador,
        ctx => $"Conclua com base na análise: {ctx.Analise}",
        (ctx, res) => ctx.Resultado = res);

// Executar
var contexto = new AnaliseContext { Entrada = "Dados para análise..." };
var resultado = await workflow.ExecuteAsync(contexto);
Console.WriteLine(resultado);
```

## 🎯 Exemplos por Caso de Uso

### 1. Chat de Atendimento (Baixo Custo)
```csharp
var chatBot = new Agent<string, string>(model, "Atendimento")
    .WithPersona(@"
        Você é um agente de atendimento eficiente.
        - Seja direto e prestativo
        - Resolva rapidamente
        - Encaminhe casos complexos
    ")
    .WithAnonymousMode(true);

var resposta = await chatBot.ExecuteAsync("Como cancelar minha conta?");
// Custo: ~$0.005 por interação
```

### 2. Calculadora Inteligente (Baixo Custo)
```csharp
public class CalculationResult
{
    public double Value { get; set; }
    public string Explanation { get; set; }
    public List<string> Steps { get; set; }
}

var calculadora = new Agent<string, CalculationResult>(model, "Calculadora")
    .WithPersona("Calculadora que explica o raciocínio");

var calculo = await calculadora.ExecuteAsync("Qual é 15% de 2.450?");
Console.WriteLine($"Resultado: {calculo.Data.Value}");
Console.WriteLine($"Explicação: {calculo.Data.Explanation}");
```

### 3. Assistente de Código (Médio Custo)
```csharp
var assistenteCodigo = new Agent<string, string>(model, "Assistente Código")
    .WithPersona("Especialista em C# e .NET")
    .WithReasoning(true) // Adiciona custo de reasoning
    .WithTools(new CodeAnalysisToolPack());

var codigo = await assistenteCodigo.ExecuteAsync(
    "Revise este código e sugira melhorias: ...");
```

### 4. Pesquisador Acadêmico (Alto Custo)
```csharp
public class PesquisaContext
{
    public string UserId { get; set; } = "pesquisador123";
    public string SessionId { get; set; } = "projeto_phd";
    public string Dominio { get; set; }
}

var pesquisador = new Agent<PesquisaContext, string>(model, "Pesquisador")
    .WithSemanticMemory(storage, embeddingService) // Memória entre projetos
    .WithReasoning(true) // Análise profunda
    .WithPersona(@"
        Pesquisador PhD que mantém contexto entre projetos.
        - Conecta informações de diferentes papers
        - Identifica patterns ao longo do tempo
        - Sugere direções de pesquisa baseado em histórico
    ")
    .WithTools(new AcademicToolPack());

var resultado = await pesquisador.ExecuteAsync(
    "Analise as tendências em machine learning dos últimos 6 meses",
    new PesquisaContext { Dominio = "ML" });

// Custo: ~$0.025 por interação (5x mais, mas contexto profundo)
```

## 🛠️ Configurações Avançadas

### Raciocínio Estruturado
```csharp
var agenteRaciocinio = new Agent<string, string>(model, "Analista")
    .WithReasoning(true) // Habilita reasoning step-by-step
    .WithReasoningSteps(minSteps: 3, maxSteps: 8)
    .WithPersona("Analista que pensa metodicamente");

var analise = await agenteRaciocinio.ExecuteAsync("Analise os prós e contras desta decisão...");

// Resultado inclui processo de reasoning detalhado
Console.WriteLine("Reasoning:");
foreach (var step in analise.ReasoningSteps)
{
    Console.WriteLine($"- {step.Title}: {step.Reasoning}");
}
```

### Output Estruturado Automático
```csharp
public class RelatorioAnalise
{
    public string Resumo { get; set; }
    public List<string> PontosChave { get; set; }
    public double Confianca { get; set; }
    public List<string> Recomendacoes { get; set; }
}

// Structured output configurado automaticamente baseado no tipo
var analisador = new Agent<string, RelatorioAnalise>(model, "Analisador");

var relatorio = await analisador.ExecuteAsync("Analise este documento...");

// Acesso tipado automático
Console.WriteLine($"Resumo: {relatorio.Data.Resumo}");
Console.WriteLine($"Confiança: {relatorio.Data.Confianca:P}");
```

### Controle de Custos com Híbrido
```csharp
public class AgenteHibrido
{
    private readonly Agent<string, string> _simples;
    private readonly Agent<Context, string> _inteligente;

    public AgenteHibrido(IModel model, IStorage storage, IEmbeddingService embeddingService)
    {
        _simples = new Agent<string, string>(model, "Simples");
        _inteligente = new Agent<Context, string>(model, "Inteligente")
            .WithSemanticMemory(storage, embeddingService);
    }

    public async Task<string> ExecuteAsync(string prompt, Context context = null)
    {
        // Use agente simples para queries básicas (80% dos casos)
        if (IsQuerySimples(prompt))
        {
            var resultado = await _simples.ExecuteAsync(prompt);
            return resultado.Data;
        }

        // Use agente inteligente apenas quando necessário (20% dos casos)
        var resultadoInteligente = await _inteligente.ExecuteAsync(prompt, context);
        return resultadoInteligente.Data;
    }

    private bool IsQuerySimples(string prompt)
    {
        var indicadores = new[] { "como", "quando", "o que é", "defina" };
        return indicadores.Any(i => prompt.ToLower().Contains(i));
    }
}

// Uso balanceado: economia + inteligência quando necessário
var agente = new AgenteHibrido(model, storage, embeddingService);
```

## 📊 Monitoramento de Custos

```csharp
public class CostTrackingLogger : ILogger
{
    private decimal _totalCost = 0;

    public void Log(LogLevel level, string message, Exception ex = null)
    {
        if (message.Contains("API call cost"))
        {
            // Extrair custo da mensagem
            var cost = ExtractCostFromMessage(message);
            _totalCost += cost;
            
            Console.WriteLine($"💰 Custo atual: ${_totalCost:F4}");
        }
    }

    // Implementação dos outros métodos...
}

var agent = new Agent<string, string>(model, "Monitor")
    .WithLogger(new CostTrackingLogger());
```

## 🎯 Próximos Passos

### 1. Comece Simples (Recomendado)
```csharp
// Para 80% dos casos - economia máxima
var agente = new Agent<string, string>(model)
    .WithAnonymousMode(true);
```

### 2. Evolua Conforme Necessidade
```csharp
// Adicione recursos gradualmente
if (precisaDeMemoria)
    agente.WithSemanticMemory(storage, embeddingService);

if (precisaDeRaciocinio)
    agente.WithReasoning(true);
```

### 3. Explore Recursos Avançados
- [Arquitetura de Memória](memory-architecture.md) - Entenda custos vs benefícios
- [Exemplos Práticos](examples/) - Casos reais com análise de ROI
- [API Reference](api/) - Documentação completa

### 4. Monitore e Otimize
- Implemente tracking de custos
- Analise padrões de uso
- Otimize baseado no ROI real

## 📚 Recursos por Categoria

### Documentação Essencial
- [Arquitetura de Memória](memory-architecture.md) - **LEIA PRIMEIRO**
- [Agent API](api/core/agent.md) - Referência completa
- [Best Practices](best-practices.md) - Otimização de custos

### Exemplos Práticos
- [Agentes Simples](examples/basic-agents.md)
- [Memória Semântica](examples/semantic-memory.md)
- [Workflows Avançados](examples/advanced-workflows.md)

### Casos de Uso
- [Chat/Atendimento](use-cases/customer-support.md)
- [Assistentes Pessoais](use-cases/personal-assistants.md)
- [Análise de Dados](use-cases/data-analysis.md)

## ❓ Suporte

- 📖 [Documentação Completa](https://agentsharp.dev/docs)
- 🐛 [Issues no GitHub](https://github.com/agentsharp/issues)
- 💬 [Discussions](https://github.com/agentsharp/discussions)

---

## 🎊 Parabéns!

Você está pronto para construir agentes inteligentes com controle total de custos. Lembre-se:

- **Comece simples** (baixo custo)
- **Evolua conforme necessário** (ROI guia decisões)
- **Monitore custos** sempre
- **Otimize baseado em dados reais**