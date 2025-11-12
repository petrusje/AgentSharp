using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;
using AgentSharp.Providers.OpenAI;
using AgentSharp.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.console
{
    /// <summary>
    /// Exemplo refatorado do Supervisor usando a arquitetura nativa da AgentSharp
    /// Mostra as melhorias práticas no handoff entre agentes
    /// </summary>
    internal static class SupervisorExampleImproved
    {
        // Contextos tipados para melhor organização
        public class InternalAgentContext
        {
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string TaskDescription { get; set; }
        }

        public class ExternalAgentContext
        {
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string TaskDescription { get; set; }
        }

        public class ConversationContext
        {
            public string UserId { get; set; }
            public string SessionId { get; set; }
            public string TaskDescription { get; set; }
        }

        private static async Task Main()
        {
            Console.WriteLine("🤖 Supervisor Melhorado - AgentSharp Nativo\n");

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("OPENAI_API_KEY não encontrado. Exporte a variável antes de executar o exemplo.");
                Console.ResetColor();
                return;
            }

            // Configuração consolidada
            var (baseModel, repository) = SetupModelAndData(apiKey);

            // ========== MELHORIA 1: Builder Pattern Consolidado ==========
            var commonConfig = new AgentConfig
            {
                ReasoningEnabled = true,
                MinReasoningSteps = 2,
                MaxReasoningSteps = 4,
                Temperature = 0.2
            };

            var internalAgent = CreateInternalAgent(baseModel, repository, commonConfig);
            var externalAgent = CreateExternalAgent(baseModel, commonConfig);

            // ========== MELHORIA 2: Usar AgentTool em vez de ToolPack customizado ==========
            var supervisorAgent = CreateSupervisorWithAgentTools(baseModel, internalAgent, externalAgent, commonConfig);

            // Testar cenários
            var scenarios = new[]
            {
                "Liste os clientes enterprise e indique qual produto mais vendido para eles.",
                "Ache notícias rápidas sobre IA generativa no setor de seguros.",
                "Preciso do endereço completo para o CEP 01310-200 e, se possível, indique produtos relacionados ao setor."
            };

            var sessionId = Guid.NewGuid().ToString("N");
            var userId = "supervisor-improved";

            foreach (var scenario in scenarios)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nPergunta: {scenario}");
                Console.ResetColor();

                var context = new ConversationContext
                {
                    UserId = userId,
                    SessionId = sessionId,
                    TaskDescription = scenario
                };

                var result = await supervisorAgent.ExecuteAsync(scenario, context);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Resposta do Supervisor:\n");
                Console.ResetColor();
                Console.WriteLine(result.Data);

                if (!string.IsNullOrWhiteSpace(result.ReasoningContent))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\nRaciocínio estruturado:\n");
                    Console.ResetColor();
                    Console.WriteLine(result.ReasoningContent);
                }
            }

            Console.WriteLine("\n✅ Demonstração finalizada");
        }

        // ========== MELHORIA 3: Configuração consolidada ==========
        public class AgentConfig
        {
            public bool ReasoningEnabled { get; set; } = true;
            public int MinReasoningSteps { get; set; } = 2;
            public int MaxReasoningSteps { get; set; } = 4;
            public double Temperature { get; set; } = 0.2;
        }

        private static (IModel model, InternalDataRepository repository) SetupModelAndData(string apiKey)
        {
            var provider = new OpenAIModelProvider(apiKey);
            var modelFactory = new ModelFactory(new[] { provider });

            var modelOptions = new ModelOptions
            {
                ModelName = "gpt-4o-mini",
                ApiKey = apiKey,
                DefaultConfiguration = new ModelConfiguration
                {
                    Temperature = 0.2,
                    MaxTokens = 900
                }
            };

            var model = modelFactory.CreateModel("openai", modelOptions);
            var repository = new InternalDataRepository();

            return (model, repository);
        }

        // ========== MELHORIA 4: Factory methods com configuração comum ==========
        private static Agent<InternalAgentContext, string> CreateInternalAgent(
            IModel model,
            InternalDataRepository repository,
            AgentConfig config)
        {
            return new Agent<InternalAgentContext, string>(model, name: "AgenteBasesInternas")
                .WithPersona("Você é um analista de dados corporativos com acesso a clientes, produtos e vendas internas.")
                .WithInstructions(ctx =>
                    "Use as ferramentas disponíveis para consultar clientes, produtos e vendas. " +
                    "Responda em português, estruturando os resultados em listas ou tabelas quando fizer sentido.")
                .WithTools(new InternalDataToolPack(repository))
                .WithReasoning(config.ReasoningEnabled)
                .WithReasoningSteps(config.MinReasoningSteps, config.MaxReasoningSteps);
        }

        private static Agent<ExternalAgentContext, string> CreateExternalAgent(
            IModel model,
            AgentConfig config)
        {
            return new Agent<ExternalAgentContext, string>(model, name: "AgenteFontesExternas")
                .WithPersona("Você pesquisa rapidamente fontes públicas confiáveis.")
                .WithInstructions(ctx =>
                    "Priorize as ferramentas para buscar na web ou consultar CEPs. " +
                    "Sempre cite resumidamente as fontes encontradas e alerte sobre possíveis limitações.")
                .WithTools(new ExternalDataToolPack())
                .WithReasoning(config.ReasoningEnabled)
                .WithReasoningSteps(config.MinReasoningSteps, config.MaxReasoningSteps);
        }

        // ========== MELHORIA 5: Usar AgentTool nativo em vez de ToolPack customizado ==========
        private static Agent<ConversationContext, string> CreateSupervisorWithAgentTools(
            IModel model,
            Agent<InternalAgentContext, string> internalAgent,
            Agent<ExternalAgentContext, string> externalAgent,
            AgentConfig config)
        {
            var supervisor = new Agent<ConversationContext, string>(model, name: "SupervisorInteligente")
                .WithPersona("Você é um supervisor inteligente que analisa tarefas e direciona para o agente mais adequado.")
                .WithInstructions(ctx =>
                    "Analise a tarefa e escolha o agente apropriado:\n" +
                    "- Para dados internos (clientes, produtos, vendas): use consultar_AgenteBasesInternas\n" +
                    "- Para pesquisas externas (web, CEP, notícias): use consultar_AgenteFontesExternas\n" +
                    "Forneça uma resposta final bem estruturada.")
                .WithReasoning(config.ReasoningEnabled)
                .WithReasoningSteps(config.MinReasoningSteps + 1, config.MaxReasoningSteps + 2);

            // ========== CHAVE: Usar AgentTool nativo para encapsular outros agentes ==========
            var internalTool = internalAgent.AsTool(
                toolName: "consultar_AgenteBasesInternas",
                toolDescription: "Consulta dados internos da empresa: clientes, produtos, vendas, informações corporativas",
                isFinalTool: false
            );

            var externalTool = externalAgent.AsTool(
                toolName: "consultar_AgenteFontesExternas",
                toolDescription: "Pesquisa informações externas: web, CEP, notícias, dados públicos",
                isFinalTool: false
            );

            supervisor.AddTool(internalTool);
            supervisor.AddTool(externalTool);

            return supervisor;
        }
    }

    // ========== Classes de apoio simplificadas ==========
    public class InternalDataRepository
    {
        // Implementação de exemplo - você substituirá pela sua
        public string GetClients() => "Clientes enterprise: TechCorp, DataSolutions, CloudFirst";
        public string GetProducts() => "Produtos mais vendidos: AgentSharp Pro, DataAnalyzer, CloudSync";
    }

    public class InternalDataToolPack : ToolPack
    {
        private readonly InternalDataRepository _repository;

        public InternalDataToolPack(InternalDataRepository repository)
        {
            _repository = repository;
            Name = "InternalDataTools";
            Description = "Ferramentas para acesso a dados corporativos internos";
            Version = "1.0.0";
        }

        [AgentSharp.Attributes.FunctionCall("Consulta lista de clientes")]
        public string GetClients()
        {
            return _repository.GetClients();
        }

        [AgentSharp.Attributes.FunctionCall("Consulta produtos e vendas")]
        public string GetProducts()
        {
            return _repository.GetProducts();
        }
    }

    public class ExternalDataToolPack : ToolPack
    {
        public ExternalDataToolPack()
        {
            Name = "ExternalDataTools";
            Description = "Ferramentas para acesso a dados externos";
            Version = "1.0.0";
        }

        [AgentSharp.Attributes.FunctionCall("Busca notícias na web")]
        [AgentSharp.Attributes.FunctionCallParameter("topic", "Tópico para buscar")]
        public string SearchNews(string topic)
        {
            return $"Notícias sobre {topic}: [Simulado] IA generativa está transformando o setor de seguros com automação de claims e análise preditiva.";
        }

        [AgentSharp.Attributes.FunctionCall("Consulta CEP")]
        [AgentSharp.Attributes.FunctionCallParameter("cep", "CEP para consultar")]
        public string GetAddress(string cep)
        {
            return $"Endereço para CEP {cep}: [Simulado] Av. Paulista, 1000 - Bela Vista, São Paulo - SP";
        }
    }
}
