# 🚀 Guia de Início Rápido

> Comece a usar o AgentSharp em minutos

## 📋 Pré-requisitos

- .NET SDK 6.0 ou superior
- Chave de API da OpenAI
- Editor de código (recomendado: VS Code com C# extension)

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

3. **Configurar variáveis de ambiente**
```bash
# Linux/macOS
export OPENAI_API_KEY="sua-chave-aqui"

# Windows PowerShell
$env:OPENAI_API_KEY="sua-chave-aqui"
```

## 🔥 Primeiro Agente

```csharp
using AgentSharp;
using AgentSharp.Core;
using AgentSharp.Models;

// Criar modelo
var modelOptions = new ModelOptions
{
    ModelName = "gpt-4",
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
};
var model = new ModelFactory().CreateModel("openai", modelOptions);

// Criar agente
var agent = new Agent<string, string>(model, "Assistente")
    .WithPersona("Você é um assistente prestativo e amigável")
    .WithReasoning(true);

// Executar
var resultado = await agent.ExecuteAsync("Olá! Como posso ajudar?");
Console.WriteLine(resultado);
```

## 🔄 Primeiro Workflow

```csharp
// Definir contexto
public class AnaliseContext
{
    public string Entrada { get; set; }
    public string Analise { get; set; }
    public string Resultado { get; set; }
}

// Criar workflow
var workflow = new SequentialWorkflow<AnaliseContext, string>("Análise")
    .RegisterStep("Análise", analista,
        ctx => $"Analise: {ctx.Entrada}",
        (ctx, res) => ctx.Analise = res)
    .RegisterStep("Conclusão", finalizador,
        ctx => $"Conclua com base na análise: {ctx.Analise}",
        (ctx, res) => ctx.Resultado = res);

// Executar
var contexto = new AnaliseContext { Entrada = "Dados para análise" };
var resultado = await workflow.ExecuteAsync(contexto);
```

## 🛠️ Configurações Avançadas

### Debug Mode
```csharp
var agent = new Agent<string, string>(model, "Assistente")
    .WithDebugMode(true)
    .WithLogger(new ConsoleLogger());
```

### Telemetria
```csharp
var workflow = new AdvancedWorkflow<Context, string>("Workflow")
    .WithTelemetry(true)
    .WithUserId("user123");
```

### Sessões Persistentes
```csharp
workflow.CreateNewSession("Sessão-001");
var snapshot = workflow.GetSessionSnapshot();
```

## 🎯 Próximos Passos

1. **Explore os Exemplos**
   - [Exemplos Básicos](examples.md#básicos)
   - [Workflows](examples.md#workflows)
   - [Raciocínio](examples.md#raciocínio)

2. **Aprofunde-se**
   - [Conceitos Fundamentais](core-concepts.md)
   - [Sistema de Workflows](workflows.md)
   - [API Reference](api/index.md)

3. **Contribua**
   - [Guia de Contribuição](contributing.md)
   - [Código de Conduta](code-of-conduct.md)

## ❓ Suporte

- [Issues no GitHub](https://github.com/seu-repo/issues)
- [Documentação Completa](https://seu-repo.github.io/docs)
- [Comunidade Discord](https://discord.gg/seu-servidor)

---

## 📚 Recursos Relacionados

- [Conceitos Fundamentais](core-concepts.md)
- [Exemplos](examples.md)
- [API Reference](api/index.md) 