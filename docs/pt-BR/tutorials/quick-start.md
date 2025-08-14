# Guia de Início Rápido

Este guia ajudará você a construir seu primeiro agente AgentSharp em apenas alguns minutos.

## Pré-requisitos

- AgentSharp instalado ([Guia de Instalação](../getting-started/installation.md))
- Chave da API OpenAI (para este exemplo)

## Etapa 1: Crie Seu Primeiro Agente

Vamos criar um agente simples de atendimento ao cliente:

```csharp
using AgentSharp.Core;
using AgentSharp.Models;

// Criar modelo com sua chave da API
var model = new OpenAIModel("gpt-4o-mini", "sua_chave_api_aqui");

// Criar agente com contexto e resultado tipados
var agente = new Agent<ContextoCliente, RespostaCliente>(model, "AgenteAtendimento")
    .WithPersona("Você é um representante simpático de atendimento ao cliente")
    .WithInstructions(ctx => $"Ajude {ctx.NomeCliente} com sua consulta sobre {ctx.TipoProduto}")
    .WithGuardRails("Sempre seja educado e profissional. Nunca faça promessas sobre reembolsos sem aprovação do gerente.");

// Definir seus tipos de contexto e resposta
public class ContextoCliente
{
    public string NomeCliente { get; set; }
    public string TipoProduto { get; set; }
    public string DescricaoProblema { get; set; }
}

public class RespostaCliente
{
    public string Resposta { get; set; }
    public string Sentimento { get; set; }
    public bool RequerEscalonamento { get; set; }
    public List<string> AcoesSugeridas { get; set; }
}
```

## Etapa 2: Execute o Agente

```csharp
// Criar contexto para a interação
var contexto = new ContextoCliente
{
    NomeCliente = "João Silva",
    TipoProduto = "Assinatura Premium",
    DescricaoProblema = "Não consegue acessar recursos avançados após pagamento"
};

// Executar o agente
var resultado = await agente.ExecuteAsync(
    "Paguei pela assinatura premium ontem, mas ainda não consigo acessar os recursos avançados. Pode me ajudar?",
    contexto
);

// AgentSharp extrai automaticamente dados estruturados
Console.WriteLine($"Resposta: {resultado.Content.Resposta}");
Console.WriteLine($"Sentimento: {resultado.Content.Sentimento}");
Console.WriteLine($"Necessita Escalonamento: {resultado.Content.RequerEscalonamento}");
Console.WriteLine($"Ações Sugeridas: {string.Join(", ", resultado.Content.AcoesSugeridas)}");
```

## Etapa 3: Adicionar Memória (Opcional)

Faça seu agente lembrar de interações passadas:

```csharp
using AgentSharp.Core.Memory.Services;
using AgentSharp.Embeddings;

// Configurar armazenamento e embeddings
var storage = new SemanticSqliteStorage("clientes.db", embeddingService);
var embeddingService = new OpenAIEmbeddingService("text-embedding-3-small", "sua_api_key");

// Criar agente com memória habilitada
var agente = new Agent<ContextoCliente, RespostaCliente>(model, "AgenteAtendimento")
    .WithPersona("Você é um representante simpático de atendimento ao cliente")
    .WithSemanticMemory(storage, embeddingService)
    .WithUserMemories(enable: true)      // Lembrar informações importantes do cliente
    .WithMemorySearch(enable: true)      // Buscar interações passadas
    .WithHistoryToMessages(enable: true); // Incluir histórico da conversa

// Agora o agente lembrará deste cliente em sessões futuras
```

## Etapa 4: Adicionar Ferramentas

Estenda seu agente com funções chamáveis:

```csharp
public class AtendimentoToolPack : ToolPack
{
    [FunctionCall("Verificar status da assinatura de um cliente")]
    [FunctionCallParameter("clienteId", "Identificador único do cliente")]
    public async Task<StatusAssinatura> VerificarStatusAssinatura(string clienteId)
    {
        // Sua lógica de negócio aqui
        return await ServicoAssinatura.GetStatusAsync(clienteId);
    }

    [FunctionCall("Criar ticket de suporte")]
    [FunctionCallParameter("problema", "Descrição do problema do cliente")]
    [FunctionCallParameter("prioridade", "Nível de prioridade: baixa, média, alta, urgente")]
    public async Task<string> CriarTicketSuporte(string problema, string prioridade)
    {
        // Integração com seu sistema de tickets
        var ticket = await SistemaTickets.CreateAsync(problema, prioridade);
        return $"Ticket {ticket.Id} criado com sucesso";
    }
}

// Registrar as ferramentas
var agente = new Agent<ContextoCliente, RespostaCliente>(model, "AgenteAtendimento")
    .WithPersona("Você é um representante simpático de atendimento ao cliente")
    .RegisterToolPack(new AtendimentoToolPack());
```

## Etapa 5: Monitorar Desempenho

Acompanhe o desempenho do seu agente com telemetria:

```csharp
using AgentSharp.Core.Telemetry;

// Habilitar telemetria
var telemetria = new ConsoleTelemetryService();
telemetria.Enable();

// Seu código de execução do agente aqui...

// Ver métricas de desempenho
var resumo = telemetria.GetSummary();
Console.WriteLine($"Total de tokens usados: {resumo.TotalTokens}");
Console.WriteLine($"Chamadas LLM: {resumo.LLMEvents}");
Console.WriteLine($"Tempo médio de resposta: {resumo.AverageElapsedTime:F2}ms");
```

## Exemplo Completo

Aqui está um exemplo completo funcionando:

```csharp
using AgentSharp.Core;
using AgentSharp.Models;
using AgentSharp.Core.Telemetry;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuração
        var model = new OpenAIModel("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        var telemetria = new ConsoleTelemetryService();
        telemetria.Enable();

        // Criar agente
        var agente = new Agent<ContextoCliente, RespostaCliente>(model, "AgenteAtendimento")
            .WithPersona("Você é um representante simpático de atendimento ao cliente")
            .WithInstructions(ctx => $"Ajude {ctx.NomeCliente} com sua consulta sobre {ctx.TipoProduto}")
            .WithGuardRails("Sempre seja educado. Escalone problemas de cobrança para a gerência.");

        // Executar
        var contexto = new ContextoCliente
        {
            NomeCliente = "Maria Santos",
            TipoProduto = "Plano Premium",
            DescricaoProblema = "Discrepância na cobrança"
        };

        var resultado = await agente.ExecuteAsync(
            "Fui cobrada duas vezes pelo plano premium este mês. Pode me ajudar?",
            contexto
        );

        // Resultados
        Console.WriteLine($"Resposta do Agente: {resultado.Content.Resposta}");
        Console.WriteLine($"Escalonamento Necessário: {resultado.Content.RequerEscalonamento}");
        
        // Métricas de desempenho
        var metricas = telemetria.GetSummary();
        Console.WriteLine($"Tokens Usados: {metricas.TotalTokens}");
    }
}

public class ContextoCliente
{
    public string NomeCliente { get; set; }
    public string TipoProduto { get; set; }
    public string DescricaoProblema { get; set; }
}

public class RespostaCliente
{
    public string Resposta { get; set; }
    public bool RequerEscalonamento { get; set; }
    public List<string> AcoesSugeridas { get; set; } = new List<string>();
}
```

## O Que Vem Depois?

- [Conceitos Centrais](../core-concepts/agents.md) - Mergulhe profundamente na arquitetura AgentSharp
- [Gerenciamento de Memória](../core-concepts/memory.md) - Recursos avançados de memória
- [Ferramentas & Funções](../core-concepts/tools.md) - Construindo pacotes de ferramentas personalizados
- [Orquestração de Equipes](../core-concepts/orchestration.md) - Fluxos de trabalho multi-agente
- [Exemplos Avançados](../examples/) - Cenários do mundo real