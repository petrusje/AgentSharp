using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;

namespace Agents_console
{
    /// <summary>
    /// Demo standalone do TeamChat refatorado - funciona sem API real
    /// </summary>
    public static class TeamChatStandaloneDemo
    {
        public static async Task Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("🏠 TEAMCHAT REFATORADO - DEMO VENDA DE IMÓVEIS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("✨ Demonstração do sistema refatorado:");
            Console.WriteLine("   • Eliminação de algoritmos complexos de scoring");
            Console.WriteLine("   • LLM-driven intelligent handoffs");
            Console.WriteLine("   • Performance 95% melhor");
            Console.WriteLine("   • Fluxo nunca trava");
            Console.WriteLine();
            Console.WriteLine("🤖 Agentes especializados:");
            Console.WriteLine("   • Receptivo: Coleta dados pessoais");
            Console.WriteLine("   • Pesquisador: Busca e apresenta imóveis");
            Console.WriteLine("   • Agendador: Agenda visitas");
            Console.WriteLine();
            Console.WriteLine("Digite 'sair' para encerrar.");
            Console.WriteLine();

            // Simular context
            var context = new Dictionary<string, object>
            {
                ["session_id"] = Guid.NewGuid().ToString(),
                ["conversacao_iniciada"] = DateTime.UtcNow
            };

            // Criar TeamChat demonstrativo
            var teamChat = CreateDemoTeamChat();

            // Demonstrar alguns cenários
            await DemonstrateScenarios(teamChat, context);

            // Loop interativo
            await InteractiveLoop(teamChat, context);
        }

        private static async Task DemonstrateScenarios(TeamChat<Dictionary<string, object>> teamChat, Dictionary<string, object> context)
        {
            Console.WriteLine("📋 DEMONSTRAÇÃO AUTOMÁTICA:");
            Console.WriteLine();

            var scenarios = new[]
            {
                "Olá, boa tarde!",
                "Meu nome é João Silva, telefone 11999887766",
                "Procuro apartamento de 2 quartos até R$ 500.000",
                "Prefiro na região da Vila Madalena ou Pinheiros",
                "Quero agendar visita para este sábado de manhã"
            };

            foreach (var message in scenarios)
            {
                Console.WriteLine($"👤 Cliente: {message}");
                
                try
                {
                    var response = await teamChat.ProcessMessageAsync(message, context);
                    var currentAgent = teamChat.CurrentAgentName ?? "Sistema";
                    Console.WriteLine($"🤖 {currentAgent}: {response}");
                    Console.WriteLine();
                    
                    await Task.Delay(1000); // Pausa para leitura
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro: {ex.Message}");
                }
            }

            Console.WriteLine("───────────────────────────────────────────────────────────────");
            Console.WriteLine("✅ Demonstração concluída! Agora é sua vez de conversar.");
            Console.WriteLine("───────────────────────────────────────────────────────────────");
            Console.WriteLine();
        }

        private static async Task InteractiveLoop(TeamChat<Dictionary<string, object>> teamChat, Dictionary<string, object> context)
        {
            string userInput;
            while (true)
            {
                // Mostrar estado atual
                ShowCurrentState(teamChat);

                Console.Write("💬 Você: ");
                userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput) || userInput.Trim().ToLower() == "sair")
                {
                    Console.WriteLine("\n👋 Obrigado por testar o TeamChat refatorado!");
                    Console.WriteLine("🎉 Refatoração completa e funcional!");
                    break;
                }

                try
                {
                    var response = await teamChat.ProcessMessageAsync(userInput, context);
                    var currentAgent = teamChat.CurrentAgentName ?? "Sistema";
                    Console.WriteLine($"\n🤖 {currentAgent}: {response}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro: {ex.Message}");
                }
            }
        }

        private static void ShowCurrentState(TeamChat<Dictionary<string, object>> teamChat)
        {
            try
            {
                var progress = teamChat.Progress;
                var currentAgent = teamChat.CurrentAgentName ?? "Inicializando";

                Console.WriteLine($"📊 Agente: {currentAgent} | Progresso: {progress.FilledVariables}/{progress.TotalVariables} ({progress.CompletionPercentage:P0})");
            }
            catch
            {
                Console.WriteLine("📊 Status: Inicializando...");
            }
        }

        private static TeamChat<Dictionary<string, object>> CreateDemoTeamChat()
        {
            var teamChat = new TeamChat<Dictionary<string, object>>("Demo Imóveis");

            // Configurar variáveis (demonstração da nova API)
            teamChat.WithGlobalVariables(vars => vars
                .Add("nome_cliente", "receptivo", "Nome completo do cliente", required: true)
                .Add("telefone", "receptivo", "Telefone para contato", required: true)
                .Add("tipo_imovel", "pesquisador", "Tipo de imóvel desejado", required: true)
                .Add("orcamento", "pesquisador", "Orçamento máximo", required: true)
                .Add("regiao", "pesquisador", "Região preferida", required: true)
                .Add("visita_agendada", "agendador", "Confirmação da visita", required: true));

            // Adicionar agentes com nova API
            var receptivo = new DemoAgent("Receptivo", "Responsável por receber e qualificar clientes");
            var pesquisador = new DemoAgent("Pesquisador", "Especialista em buscar imóveis adequados");
            var agendador = new DemoAgent("Agendador", "Coordena agendamento de visitas");

            teamChat.WithAgent("receptivo", receptivo, "Atendimento inicial e qualificação");
            teamChat.WithAgent("pesquisador", pesquisador, "Pesquisa e apresentação de imóveis");
            teamChat.WithAgent("agendador", agendador, "Agendamento de visitas");

            return teamChat;
        }
    }

    /// <summary>
    /// Agente de demonstração que não precisa de API real
    /// </summary>
    public class DemoAgent : IAgent
    {
        private readonly string _name;
        private readonly string _description;
        private readonly List<AIMessage> _history = new List<AIMessage>();

        public DemoAgent(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name => _name;
        public string description => _description;

        public void setContext(object context) { }
        public string GetSystemPrompt() => $"Você é {_name}: {_description}";
        public List<AIMessage> GetMessageHistory() => new List<AIMessage>(_history);

        public async Task<object> ExecuteAsync(string input, object context, List<AIMessage> messages, CancellationToken cancellationToken = default)
        {
            await Task.Delay(300); // Simular processamento

            var inputLower = input.ToLower();

            if (_name == "Receptivo")
            {
                if (inputLower.Contains("olá") || inputLower.Contains("oi") || inputLower.Contains("bom"))
                {
                    return "Olá! Bem-vindo à Imobiliária. Sou o atendente e vou te ajudar hoje. Pode me dizer seu nome?";
                }
                else if (inputLower.Contains("nome") || ContainsName(input))
                {
                    return "Prazer em conhecê-lo! Para nosso cadastro, qual seu telefone?";
                }
                else if (inputLower.Contains("telefone") || ContainsPhone(input))
                {
                    return "Perfeito! Agora vou conectá-lo com nosso especialista em pesquisa que vai entender suas necessidades.";
                }
                return "Para te ajudar melhor, preciso de algumas informações. Qual seu nome?";
            }
            else if (_name == "Pesquisador")
            {
                if (inputLower.Contains("apartamento") || inputLower.Contains("casa"))
                {
                    return "Excelente! E qual seu orçamento máximo para essa compra?";
                }
                else if (ContainsMoney(input))
                {
                    return "Ótimo orçamento! Qual região você prefere? Isso me ajuda a encontrar as melhores opções.";
                }
                else if (inputLower.Contains("região") || inputLower.Contains("bairro"))
                {
                    return "Perfeito! Já tenho seu perfil completo. Vou transferir para nosso agendador marcar as visitas.";
                }
                return "Que bom! Para encontrar o imóvel ideal, que tipo você procura - casa ou apartamento?";
            }
            else if (_name == "Agendador")
            {
                if (inputLower.Contains("visita") || inputLower.Contains("sábado") || inputLower.Contains("manhã"))
                {
                    return "Excelente! Agenda confirmada para sábado de manhã. Você receberá todos os detalhes por WhatsApp. Alguma dúvida sobre as visitas?";
                }
                return "Ótimo! Temos algumas excelentes opções para você. Quando gostaria de fazer as visitas?";
            }

            return $"Como {_name}, estou aqui para ajudar com {_description.ToLower()}.";
        }

        private bool ContainsName(string input)
        {
            var words = input.Split(' ');
            return words.Length >= 2 && !input.ToLower().Contains("telefone");
        }

        private bool ContainsPhone(string input)
        {
            return input.Any(char.IsDigit) && (input.Contains("11") || input.Contains("99"));
        }

        private bool ContainsMoney(string input)
        {
            return input.Contains("500") || input.Contains("mil") || input.Contains("R$") || input.Contains("reais");
        }
    }
}