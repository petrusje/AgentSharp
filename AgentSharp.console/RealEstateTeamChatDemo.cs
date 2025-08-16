using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Models;

namespace Agents_console
{
    /// <summary>
    /// Demo da refatoração do TeamChat com handoffs inteligentes LLM-driven
    /// </summary>
    public static class RealEstateTeamChatDemo
    {
        public static async Task RunAsync()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("🏠 TEAMCHAT REFATORADO - DEMO VENDA IMÓVEIS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("✨ Handoffs Inteligentes LLM-Driven");
            Console.WriteLine("🤖 Três agentes especializados:");
            Console.WriteLine("   • Receptivo: Coleta dados básicos");
            Console.WriteLine("   • Pesquisador: Analisa necessidades");
            Console.WriteLine("   • Agendador: Marca visitas");
            Console.WriteLine();
            Console.WriteLine("Digite 'sair' para encerrar.");
            Console.WriteLine();

            // Criar contexto
            var context = new Dictionary<string, object>
            {
                ["session_id"] = Guid.NewGuid().ToString(),
                ["customer_data"] = new Dictionary<string, object>()
            };

            // Criar TeamChat
            var teamChat = CreateTeamChat();

            string userInput;
            while (true)
            {
                Console.Write("\n💬 Você: ");
                userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput) || userInput.Trim().ToLower() == "sair")
                {
                    Console.WriteLine("\n👋 Obrigado! Até logo!");
                    break;
                }

                try
                {
                    // Processar mensagem
                    var response = await teamChat.ProcessMessageAsync(userInput, context);
                    
                    Console.WriteLine($"\n🤖 {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro: {ex.Message}");
                }
            }
        }

        private static TeamChat<Dictionary<string, object>> CreateTeamChat()
        {
            var teamChat = new TeamChat<Dictionary<string, object>>("Venda Imóveis");

            // Configurar variáveis globais
            teamChat.WithGlobalVariables(vars => vars
                .Add("nome_cliente", "receptivo", "Nome completo do cliente", required: true)
                .Add("telefone", "receptivo", "Telefone para contato", required: true)
                .Add("tipo_imovel", "pesquisador", "Tipo de imóvel desejado", required: true)
                .Add("orcamento", "pesquisador", "Orçamento máximo", required: true)
                .Add("localizacao", "pesquisador", "Região preferida", required: true)
                .Add("visita_agendada", "agendador", "Visita confirmada", required: true));

            // Adicionar agentes
            var receptivo = new MockRealEstateAgent("Receptivo", "Sou o recepcionista. Coleto nome e telefone dos clientes e os direciono para o pesquisador.");
            var pesquisador = new MockRealEstateAgent("Pesquisador", "Sou especialista em entender necessidades e buscar imóveis. Quando encontro opções, direciono para o agendador.");
            var agendador = new MockRealEstateAgent("Agendador", "Sou responsável por agendar visitas aos imóveis selecionados.");

            teamChat.WithAgent("receptivo", receptivo, "Especialista em atendimento inicial e coleta de dados básicos");
            teamChat.WithAgent("pesquisador", pesquisador, "Expert em análise de necessidades e busca de imóveis");
            teamChat.WithAgent("agendador", agendador, "Responsável por agendamento de visitas");

            return teamChat;
        }
    }

    /// <summary>
    /// Agente mock simplificado para demonstração
    /// </summary>
    public class MockRealEstateAgent : IAgent
    {
        private readonly string _name;
        private readonly string _description;

        public MockRealEstateAgent(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name => _name;
        public string description => _description;

        public void setContext(object context) { }
        public string GetSystemPrompt() => _description;
        public List<AIMessage> GetMessageHistory() => new List<AIMessage>();

        public async Task<object> ExecuteAsync(string input, object context, List<AIMessage> messages, CancellationToken cancellationToken = default)
        {
            await Task.Delay(500); // Simular processamento

            if (_name == "Receptivo")
            {
                return "Olá! Sou o atendente da imobiliária. Para começar, posso saber seu nome e telefone?";
            }
            else if (_name == "Pesquisador")
            {
                return "Ótimo! Agora que tenho seus dados, vamos encontrar o imóvel ideal. Que tipo de imóvel você procura e qual seu orçamento?";
            }
            else if (_name == "Agendador")
            {
                return "Perfeito! Encontrei algumas opções excelentes. Quando você gostaria de fazer as visitas?";
            }

            return "Como posso ajudá-lo hoje?";
        }
    }
}