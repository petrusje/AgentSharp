using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Orchestration;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Configuration;
using AgentSharp.Models;
using AgentSharp.Utils;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Console application demonstrating intelligent TeamChat for real estate sales
    /// Shows LLM-driven agent handoffs and natural conversation flow
    /// </summary>
    public class RealEstateTeamChatConsole
    {
        private readonly TeamChat<RealEstateContext> _teamChat;
        private readonly RealEstateContext _context;
        private readonly string _sessionId;

        public RealEstateTeamChatConsole()
        {
            _sessionId = Guid.NewGuid().ToString();
            _context = new RealEstateContext();

            // Initialize TeamChat with enhanced storage and global features
            _teamChat = CreateRealEstateTeamChat();
        }

        public async Task RunInteractiveConsoleAsync()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("🏠 IMOBILIÁRIA SUCESSO - ATENDIMENTO INTELIGENTE");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Bem-vindo! Nossa equipe está pronta para ajudar você.");
            Console.WriteLine("Digite 'sair' a qualquer momento para encerrar.");
            Console.WriteLine();

            string userInput;
            while (true)
            {
                // Display current state
                await DisplayCurrentStateAsync();

                // Get user input
                Console.Write("\n💬 Você: ");
                userInput = Console.ReadLine();

                // Check for exit
                if (string.IsNullOrWhiteSpace(userInput) || 
                    userInput.Trim().ToLower() == "sair")
                {
                    Console.WriteLine("\n👋 Obrigado por nos visitar! Até logo!");
                    break;
                }

                try
                {
                    // Process message through TeamChat
                    Console.WriteLine();
                    var response = await _teamChat.ProcessMessageAsync(userInput);
                    
                    // Display agent response
                    var currentAgent = _teamChat.GetCurrentAgent();
                    Console.WriteLine($"🤖 {currentAgent?.Name ?? "Sistema"}: {response}");

                    // Check if conversation completed
                    if (_teamChat.IsConversationComplete)
                    {
                        Console.WriteLine("\n🎉 Conversa finalizada com sucesso!");
                        Console.WriteLine("Pressione qualquer tecla para continuar ou digite 'sair' para encerrar...");
                        
                        var finalInput = Console.ReadLine();
                        if (finalInput?.Trim().ToLower() == "sair")
                            break;
                            
                        // Reset for new conversation
                        await _teamChat.CreateNewSessionAsync();
                        Console.WriteLine("\n🔄 Nova conversa iniciada!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro: {ex.Message}");
                    Console.WriteLine("Tente novamente ou digite 'sair' para encerrar.");
                }
            }

            // Cleanup
            try
            {
                await _teamChat.DisposeAsync();
                Console.WriteLine("\n✅ Sistema encerrado corretamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️ Aviso durante finalização: {ex.Message}");
            }
        }

        private async Task DisplayCurrentStateAsync()
        {
            try
            {
                var currentAgent = _teamChat.GetCurrentAgent();
                var variables = _teamChat.GetGlobalVariables();
                var progress = variables.GetProgress();

                Console.WriteLine("───────────────────────────────────────────────────────────────");
                Console.WriteLine($"👤 Agente Atual: {currentAgent?.Name ?? "Inicializando..."} - {currentAgent?.Expertise ?? ""}");
                Console.WriteLine($"📊 Progresso: {progress.FilledVariables}/{progress.TotalVariables} informações coletadas ({progress.CompletionPercentage:P0})");
                
                if (progress.RequiredVariables > 0)
                {
                    Console.WriteLine($"🎯 Obrigatórias: {progress.RequiredFilled}/{progress.RequiredVariables} completas ({progress.RequiredCompletionPercentage:P0})");
                }

                // Show collected information
                var collectedVars = variables.GetFilledVariables();
                if (collectedVars.Count > 0)
                {
                    Console.WriteLine("\n✅ Informações Coletadas:");
                    foreach (var variable in collectedVars)
                    {
                        var confidence = variable.Confidence < 1.0 ? $" ({variable.Confidence:P0})" : "";
                        Console.WriteLine($"   • {variable.Name}: {variable.Value}{confidence}");
                    }
                }

                // Show missing required information
                var missingRequired = variables.GetMissingVariables("any").FindAll(v => v.IsRequired);
                if (missingRequired.Count > 0)
                {
                    Console.WriteLine("\n❌ Informações Pendentes (Obrigatórias):");
                    foreach (var variable in missingRequired)
                    {
                        Console.WriteLine($"   • {variable.Name}: {variable.Description}");
                    }
                }

                Console.WriteLine("───────────────────────────────────────────────────────────────");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao exibir estado: {ex.Message}");
            }
        }

        private TeamChat<RealEstateContext> CreateRealEstateTeamChat()
        {
            // Create enhanced storage for persistence
            var storage = new SqliteEnhancedStorage("Data Source=:memory:");

            // Create and configure TeamChat
            var teamChat = new TeamChat<RealEstateContext>(
                name: "Real Estate Sales Team",
                logger: new ConsoleLogger(),
                config: WorkflowResourceConfiguration.Development)
                .WithUserId("demo-user")
                .WithEnhancedStorage(storage)
                .EnableGlobalMemory(enabled: true)
                .EnableGlobalMessageHistory(enabled: true, maxMessages: 50);

            // Configure global variables for real estate process
            ConfigureRealEstateVariables(teamChat);

            // Add specialized agents
            AddRealEstateAgents(teamChat);

            return teamChat;
        }

        private void ConfigureRealEstateVariables(TeamChat<RealEstateContext> teamChat)
        {
            teamChat.WithGlobalVariables(builder =>
            {
                // Customer Information (collected by receptivo)
                builder.Add("nome_cliente", "receptivo", "Nome completo do cliente", required: true);
                builder.Add("telefone_cliente", "receptivo", "Telefone para contato", required: true);
                builder.Add("email_cliente", "receptivo", "Email do cliente", required: false);

                // Property Requirements (collected by pesquisador)
                builder.Add("tipo_imovel", "pesquisador", "Tipo de imóvel desejado (casa, apartamento, etc.)", required: true);
                builder.Add("orcamento", "pesquisador", "Orçamento máximo do cliente", required: true);
                builder.Add("localizacao_preferida", "pesquisador", "Região ou bairro preferido", required: true);
                builder.Add("quartos", "pesquisador", "Número de quartos desejados", required: false);
                builder.Add("características_especiais", "pesquisador", "Características específicas desejadas", required: false);

                // Visit Scheduling (collected by agendador)
                builder.Add("imoveis_selecionados", "agendador", "Imóveis escolhidos para visita", required: true);
                builder.Add("data_visita", "agendador", "Data agendada para visitas", required: true);
                builder.Add("horario_visita", "agendador", "Horário da visita", required: true);
                builder.Add("observacoes_visita", "agendador", "Observações especiais para a visita", required: false);

                // Process control
                builder.AddShared("etapa_processo", "Etapa atual do processo de venda", required: true, defaultValue: "inicial");
            });
        }

        private void AddRealEstateAgents(TeamChat<RealEstateContext> teamChat)
        {
            // For demonstration, create mock agents
            // In production, you would inject IModel instances via dependency injection

            // 1. Receptionist Agent - First contact and basic information
            var recepcionistaAgent = CreateMockAgent("Recepcionista", @"
Você é o RECEPCIONISTA da Imobiliária Sucesso, responsável pelo primeiro contato com clientes.

SUA FUNÇÃO:
- Receber clientes com cordialidade e profissionalismo
- Coletar informações básicas de contato (nome, telefone, email)
- Entender a necessidade inicial do cliente
- Transferir para o especialista apropriado quando tiver as informações básicas

ESTILO DE ATENDIMENTO:
- Cordial e acolhedor
- Eficiente na coleta de dados
- Explicar o processo da imobiliária
- Sempre identificar o próximo passo

Quando coletar nome e telefone do cliente, transfira para o 'pesquisador' para analisar necessidades específicas.
");

            // 2. Property Researcher Agent - Analyze needs and find properties
            var pesquisadorAgent = CreateMockAgent("Pesquisador", @"
Você é o PESQUISADOR DE IMÓVEIS da Imobiliária Sucesso, especialista em identificar as necessidades dos clientes.

SUA FUNÇÃO:
- Analisar detalhadamente as necessidades do cliente
- Coletar informações sobre: tipo de imóvel, orçamento, localização, características
- Pesquisar e sugerir imóveis compatíveis com o perfil
- Preparar opções para apresentação

ESTILO DE ATENDIMENTO:
- Analítico e detalhista
- Fazer perguntas estratégicas
- Apresentar opções baseadas em dados
- Explicar benefícios de cada opção

Quando tiver coletado as necessidades e apresentado opções, transfira para o 'agendador' para marcar visitas.
");

            // 3. Visit Scheduler Agent - Schedule property visits
            var agendadorAgent = CreateMockAgent("Agendador", @"
Você é o AGENDADOR DE VISITAS da Imobiliária Sucesso, responsável por organizar visitas aos imóveis.

SUA FUNÇÃO:
- Agendar visitas aos imóveis selecionados
- Coordenar horários e datas
- Confirmar informações logísticas
- Preparar o cliente para as visitas

ESTILO DE ATENDIMENTO:
- Organizativo e prático
- Flexível com horários
- Confirmar todos os detalhes
- Dar orientações para as visitas

Quando agendar as visitas e confirmar todos os detalhes, finalize a conversa explicando os próximos passos.
");

            // Add agents to TeamChat
            teamChat.AddAgent("receptivo", recepcionistaAgent, "Atendimento inicial e coleta de dados básicos", priority: 10);
            teamChat.AddAgent("pesquisador", pesquisadorAgent, "Análise de necessidades e pesquisa de imóveis", priority: 8);
            teamChat.AddAgent("agendador", agendadorAgent, "Agendamento de visitas e organização logística", priority: 6);

            // Set initial agent
            teamChat.SetInitialAgent("receptivo");
        }

        /// <summary>
        /// Creates a mock agent for demonstration purposes
        /// In production, you would use real IModel instances
        /// </summary>
        private IAgent CreateMockAgent(string name, string instructions)
        {
            return new MockAgent(name, instructions);
        }

        public static async Task Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                
                var demo = new RealEstateTeamChatConsole();
                await demo.RunInteractiveConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n💥 Erro crítico: {ex.Message}");
                Console.WriteLine("\nPressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// Context specific to real estate scenarios
    /// </summary>
    public class RealEstateContext
    {
        public string CurrentProcess { get; set; } = "initial_contact";
        public Dictionary<string, object> CustomerData { get; set; } = new Dictionary<string, object>();
        public List<string> SelectedProperties { get; set; } = new List<string>();
        public DateTime SessionStarted { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Mock agent implementation for demonstration purposes
    /// In production, use real agents with IModel instances
    /// </summary>
    public class MockAgent : IAgent
    {
        private readonly string _name;
        private readonly string _instructions;
        private readonly List<AIMessage> _messageHistory = new List<AIMessage>();
        private object _context;
        
        public MockAgent(string name, string instructions)
        {
            _name = name;
            _instructions = instructions;
        }

        public string Name => _name;
        public string description => _instructions;

        public void setContext(object context)
        {
            _context = context;
        }

        public string GetSystemPrompt()
        {
            return _instructions;
        }

        public async Task<object> ExecuteAsync(string input, object context, List<AIMessage> messages, CancellationToken cancellationToken = default)
        {
            // Simulate processing delay
            await Task.Delay(500, cancellationToken);
            
            // Add user message to history
            _messageHistory.Add(AIMessage.User(input));
            
            // Generate mock response based on agent role
            var response = GenerateMockResponse(input);
            
            // Add agent response to history
            _messageHistory.Add(AIMessage.Assistant(response));
            
            return response;
        }

        private string GenerateMockResponse(string input)
        {
            var inputLower = input.ToLower();
            
            if (_name == "Recepcionista")
            {
                if (inputLower.Contains("olá") || inputLower.Contains("oi") || inputLower.Contains("bom dia"))
                {
                    return "Olá! Bem-vindo à Imobiliária Sucesso! Eu sou o recepcionista e vou te ajudar hoje. " +
                           "Para começar, posso saber seu nome completo?";
                }
                else if (inputLower.Contains("nome") || inputLower.Contains("chamo"))
                {
                    return "Muito prazer! Agora preciso do seu telefone para contato. Qual é o seu telefone?";
                }
                else if (inputLower.Contains("telefone") || inputLower.Contains("celular"))
                {
                    return "Perfeito! Agora vou conectá-lo com nosso especialista em pesquisa de imóveis " +
                           "que vai entender melhor suas necessidades específicas.";
                }
                else
                {
                    return "Entendi. Para te ajudar melhor, preciso de algumas informações básicas. " +
                           "Pode me dizer seu nome completo?";
                }
            }
            else if (_name == "Pesquisador")
            {
                if (inputLower.Contains("apartamento") || inputLower.Contains("casa"))
                {
                    return "Excelente! Vejo que você tem preferência por esse tipo de imóvel. " +
                           "Qual é o seu orçamento máximo para a compra?";
                }
                else if (inputLower.Contains("orçamento") || inputLower.Contains("valor") || inputLower.Contains("preço"))
                {
                    return "Ótimo! E qual região ou bairro você prefere? Isso me ajuda a filtrar " +
                           "os melhores imóveis para você.";
                }
                else if (inputLower.Contains("região") || inputLower.Contains("bairro") || inputLower.Contains("zona"))
                {
                    return "Perfeito! Com base no que conversamos, posso apresentar algumas opções excelentes. " +
                           "Vou transferir você para nosso agendador que vai marcar as visitas aos imóveis.";
                }
                else
                {
                    return "Que bom! Para encontrar o imóvel ideal, preciso entender suas necessidades. " +
                           "Que tipo de imóvel você está procurando? Casa ou apartamento?";
                }
            }
            else if (_name == "Agendador")
            {
                if (inputLower.Contains("visita") || inputLower.Contains("ver") || inputLower.Contains("conhecer"))
                {
                    return "Excelente! Temos horários disponíveis para esta semana. " +
                           "Qual dia seria melhor para você? Manhã ou tarde?";
                }
                else if (inputLower.Contains("manhã") || inputLower.Contains("tarde") || inputLower.Contains("horário"))
                {
                    return "Perfeito! Agendei sua visita. Você receberá um SMS com todos os detalhes. " +
                           "Tenho certeza que você vai gostar dos imóveis que selecionamos. " +
                           "Alguma pergunta sobre as visitas?";
                }
                else
                {
                    return "Que ótimo! Nosso pesquisador já identificou alguns imóveis ideais para você. " +
                           "Gostaria de agendar visitas para conhecê-los pessoalmente?";
                }
            }
            
            return $"Como {_name}, estou aqui para ajudar. {_instructions}";
        }

        public List<AIMessage> GetMessageHistory()
        {
            return new List<AIMessage>(_messageHistory);
        }

        public void ClearHistory()
        {
            _messageHistory.Clear();
        }
    }
}