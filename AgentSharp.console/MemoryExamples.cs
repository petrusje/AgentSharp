using AgentSharp.Core;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Exemplos demonstrando o sistema de memória avançado do AgentSharp
    /// </summary>
    public static class ExemplosMemoria
    {
        private static readonly ConsoleObj _console = new();

        /// <summary>
        /// Exemplo 1: Assistente pessoal que lembra de preferências
        /// </summary>
        public static async Task ExecutarAssistentePessoal(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("💾 NÍVEL 2 - AGENTE COM MEMÓRIA: Persistência de Estado")
                .WriteLine("════════════════════════════════════════════════════════")
                .ResetColor();

            Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
            Console.WriteLine("   • SqliteStorage - armazenamento persistente");
            Console.WriteLine("   • Session management - controle de sessões");
            Console.WriteLine("   • Context persistence - persistência de contexto");
            Console.WriteLine("   • Memory retrieval - recuperação de memórias");
            Console.WriteLine("   • Personalized interactions - interações personalizadas\n");

            // Configurar storage persistente
            var storage = new SqliteStorage("Data Source=exemplo_assistente.db");
            await storage.InitializeAsync();

            // Criar contexto do usuário
            var context = new UsuarioContext 
            { 
                UserId = "joao123", 
                SessionId = "sessao_demo_memoria" // SessionId fixo para demonstração
            };

            // Configurar agente com memória
            var assistente = new Agent<UsuarioContext, string>(modelo, "AssistentePessoal", storage: storage)
                .WithPersona("Você é um assistente pessoal que lembra das preferências e contexto do usuário")
                .WithInstructions(@"
                    - Sempre cumprimente o usuário pelo nome quando souber
                    - Lembre-se das preferências mencionadas
                    - Use as informações armazenadas para personalizar suas respostas
                    - Seja proativo em sugerir baseado no histórico")
                .WithContext(context);

            Console.WriteLine("💬 Simulando múltiplas conversas...\n");

            // === PRIMEIRA CONVERSA: Estabelecer preferências ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Oi, meu nome é João. Prefiro café forte e gosto de trabalhar pela manhã.").ResetColor();
            
            var resposta1 = await assistente.ExecuteAsync(
                "Oi, meu nome é João. Prefiro café forte e gosto de trabalhar pela manhã."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta1.Data}").ResetColor();
            Console.WriteLine();

            // === SEGUNDA CONVERSA ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Que horas você recomenda para eu estudar hoje?").ResetColor();
            
            var resposta2 = await assistente.ExecuteAsync(
                "Que horas você recomenda para eu estudar hoje?"
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta2.Data}").ResetColor();
            Console.WriteLine();

            // === TERCEIRA CONVERSA ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Bom dia! Como você prepararia um café para mim?").ResetColor();
            
            var resposta3 = await assistente.ExecuteAsync(
                "Bom dia! Como você prepararia um café para mim?"
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta3.Data}").ResetColor();

            // Mostrar memórias armazenadas
            await MostrarMemorias(assistente);
            
        }

        /// <summary>
        /// Exemplo 2: Consultor técnico que acumula conhecimento
        /// </summary>
        public static async Task ExecutarConsultorTecnico(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🔧 EXEMPLO: Consultor Técnico com Acúmulo de Conhecimento")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var storage = new SqliteStorage("Data Source=exemplo_consultor.db");
            await storage.InitializeAsync();

            var context = new ProjetoContext 
            { 
                UserId = "dev_maria", 
                SessionId = "projeto_react_2024",
                ProjetoNome = "Sistema E-commerce React"
            };

            var consultor = new Agent<ProjetoContext, string>(modelo, "ConsultorTecnico", storage: storage)
                .WithPersona("Você é um consultor técnico sênior especializado em desenvolvimento web")
                .WithInstructions(@"
                    - Mantenha contexto técnico do projeto
                    - Lembre-se de decisões arquiteturais anteriores
                    - Seja consistente com tecnologias já definidas
                    - Sugira melhorias baseadas no histórico")
                .WithContext(context);

            // === CONVERSA 1: Definições iniciais ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👩‍💻 DEV: Estou iniciando um e-commerce em React. Preciso escolher estado global: Redux ou Zustand?").ResetColor();
            
            var resposta1 = await consultor.ExecuteAsync(
                "Estou iniciando um e-commerce em React. Preciso escolher estado global: Redux ou Zustand?"
            );

            _console.WithColor(ConsoleColor.Blue).WriteLine($"👨‍💼 CONSULTOR: {resposta1.Data}").ResetColor();
            Console.WriteLine();

            // === CONVERSA 2: Implementação ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👩‍💻 DEV: Agora preciso implementar autenticação. JWT ou Session-based?").ResetColor();
            
            var resposta2 = await consultor.ExecuteAsync(
                "Agora preciso implementar autenticação. JWT ou Session-based?"
            );

            _console.WithColor(ConsoleColor.Blue).WriteLine($"👨‍💼 CONSULTOR: {resposta2.Data}").ResetColor();
            Console.WriteLine();

            // === CONVERSA 3: Consistência ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👩‍💻 DEV: Para o carrinho de compras, que solução recomenda considerando o que já definimos?").ResetColor();
            
            var resposta3 = await consultor.ExecuteAsync(
                "Para o carrinho de compras, que solução recomenda considerando o que já definimos?"
            );

            _console.WithColor(ConsoleColor.Blue).WriteLine($"👨‍💼 CONSULTOR: {resposta3.Data}").ResetColor();

            await MostrarMemorias(consultor);
        }

        /// <summary>
        /// Exemplo 3: Demonstração de Tools de Memória para LLM
        /// </summary>
        public static async Task ExecutarDemonstracaoMemoryTools(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🛠️ EXEMPLO: LLM Gerenciando Suas Próprias Memórias")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var storage = new SqliteStorage("Data Source=exemplo_tools.db");
            await storage.InitializeAsync();

            var context = new UsuarioContext 
            { 
                UserId = "usuario_autonomo", 
                SessionId = "sessao_tools"
            };

            var agente = new Agent<UsuarioContext, string>(modelo, "AgenteAutonomo", storage: storage)
                .WithPersona("Você é um agente que gerencia ativamente suas próprias memórias")
                .WithInstructions(@"
                    - Use as ferramentas de memória para gerenciar informações importantes
                    - SEMPRE adicione informações relevantes à memória usando AddMemory
                    - Busque memórias relacionadas antes de responder usando SearchMemories
                    - Mantenha suas memórias organizadas e atualizadas")
                .WithContext(context);

            Console.WriteLine("🤖 Agente com controle ativo de suas memórias...\n");

            // O agente vai usar automaticamente as tools para gerenciar memória
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Meu projeto principal é um sistema CRM em .NET. Preciso implementar relatórios.").ResetColor();
            
            var resposta1 = await agente.ExecuteAsync(
                "Meu projeto principal é um sistema CRM em .NET. Preciso implementar relatórios."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 AGENTE: {resposta1.Data}").ResetColor();
            Console.WriteLine();

            // Segunda pergunta - agente deve buscar memórias anteriores
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Que bibliotecas recomenda para gráficos considerando meu projeto?").ResetColor();
            
            var resposta2 = await agente.ExecuteAsync(
                "Que bibliotecas recomenda para gráficos considerando meu projeto?"
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 AGENTE: {resposta2.Data}").ResetColor();

            await MostrarMemorias(agente);
        }

        /// <summary>
        /// Exemplo 4: Comparação InMemory vs SQLite Storage
        /// </summary>
        public static async Task ExecutarComparacaoStorage(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("📊 EXEMPLO: Comparação de Storage Providers")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var context = new UsuarioContext 
            { 
                UserId = "usuario_teste", 
                SessionId = "comparacao_storage"
            };

            // === TESTE 1: InMemory Storage ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("🧪 TESTE 1: InMemoryStorage").ResetColor();
            
            var inMemoryStorage = new InMemoryStorage();
            var agenteMemoria = new Agent<UsuarioContext, string>(modelo, "AgenteMemoria", storage: inMemoryStorage)
                .WithPersona("Assistente com memória temporária")
                .WithContext(context);

            await agenteMemoria.ExecuteAsync("Meu nome é Carlos e prefiro chá verde");
            var resultado1 = await agenteMemoria.ExecuteAsync("Qual é meu nome e minha bebida preferida?");
            
            _console.WithColor(ConsoleColor.Green).WriteLine($"✅ InMemory: {resultado1.Data}").ResetColor();

            // === TESTE 2: SQLite Storage ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("\n🧪 TESTE 2: SqliteStorage").ResetColor();
            
            var sqliteStorage = new SqliteStorage("Data Source=comparacao_test.db");
            await sqliteStorage.InitializeAsync();
            
            var agentePersistente = new Agent<UsuarioContext, string>(modelo, "AgentePersistente", storage: sqliteStorage)
                .WithPersona("Assistente com memória persistente")
                .WithContext(context);

            await agentePersistente.ExecuteAsync("Meu nome é Carlos e prefiro chá verde");
            var resultado2 = await agentePersistente.ExecuteAsync("Qual é meu nome e minha bebida preferida?");
            
            _console.WithColor(ConsoleColor.Green).WriteLine($"✅ SQLite: {resultado2.Data}").ResetColor();

            // === DEMONSTRAR PERSISTÊNCIA ===
            Console.WriteLine("\n🔄 Simulando reinicialização...");
            
            // Criar novo agente (simula restart da aplicação)
            var agenteReiniciado = new Agent<UsuarioContext, string>(modelo, "AgenteReiniciado", storage: sqliteStorage)
                .WithPersona("Assistente após reinicialização")
                .WithContext(context);

            var resultado3 = await agenteReiniciado.ExecuteAsync("Você lembra de mim? Qual minha bebida preferida?");
            
            _console.WithColor(ConsoleColor.Green).WriteLine($"✅ Após Restart: {resultado3.Data}").ResetColor();

            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("\n📋 RESUMO:")
                .WriteLine("• InMemoryStorage: Rápido, mas perde dados ao reiniciar")
                .WriteLine("• SqliteStorage: Persistente, mantém dados entre sessões")
                .ResetColor();
        }

        // === MÉTODOS AUXILIARES ===

        private static async Task MostrarMemorias<TContext, TResult>(Agent<TContext, TResult> agente)
        {
            try
            {
                _console.WithColor(ConsoleColor.DarkGray)
                    .WriteLine("\n📚 MEMÓRIAS ARMAZENADAS:")
                    .WriteLine("─────────────────────────");

                var memoryManager = agente.GetMemoryManager();
                var memorias = await memoryManager.GetExistingMemoriesAsync(limit: 10);

                if (memorias?.Count > 0)
                {
                    foreach (var memoria in memorias)
                    {
                        Console.WriteLine($"[{memoria.Type}] {memoria.Content}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma memória encontrada.");
                }

                _console.ResetColor();
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Erro ao buscar memórias: {ex.Message}")
                    .ResetColor();
            }
        }

        /// <summary>
        /// Exemplo 5: Assistente Médico com Configuração Customizada
        /// </summary>
        public static async Task ExecutarAssistenteMedicoCustomizado(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🏥 EXEMPLO: Assistente Médico com Configuração Customizada de Memória")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var storage = new SqliteStorage("Data Source=exemplo_medico.db");
            await storage.InitializeAsync();

            var context = new UsuarioContext 
            { 
                UserId = "dr_silva", 
                SessionId = "consulta_medica_2024"
            };

            var assistenteMedico = new Agent<UsuarioContext, string>(modelo, "AssistenteMedico", storage: storage)
                .WithPersona("Você é um assistente médico especializado que ajuda médicos com informações clínicas")
                .WithMemoryCategories("Symptom", "Diagnosis", "Medication", "Treatment", "Allergy", "TestResult")
                .WithMemoryExtraction((userMsg, assistantMsg) => $@"
                    Como especialista médico, extraia APENAS informações clinicamente relevantes desta consulta:
                    
                    Paciente/Médico: {userMsg}
                    Resposta: {assistantMsg}
                    
                    EXTRAIR:
                    - Sintomas específicos mencionados
                    - Medicamentos prescritos ou mencionados  
                    - Diagnósticos ou hipóteses diagnósticas
                    - Exames solicitados ou resultados
                    - Alergias ou contraindicações
                    - Tratamentos recomendados
                    
                    IGNORAR: cumprimentos, conversas casuais, informações não-médicas
                    
                    JSON: {{""memories"": [{{""content"": ""Paciente relata dor de cabeça há 3 dias"", ""type"": ""Symptom"", ""importance"": 0.9}}]}}")
                .WithMemoryClassification(content => $@"
                    Classifique esta informação médica em uma categoria:
                    {content}
                    
                    Categorias: Symptom, Diagnosis, Medication, Treatment, Allergy, TestResult
                    Responda APENAS o nome da categoria.")
                .WithMemoryThresholds(maxMemories: 8, minImportance: 0.7) // Mais rigoroso para medicina
                .WithContext(context);

            Console.WriteLine("🩺 Simulando consulta médica...\n");

            // === CONSULTA 1: Sintomas ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 PACIENTE: Doutor, estou com dor de cabeça forte há 3 dias, principalmente pela manhã. Também sinto náuseas.").ResetColor();
            
            var resposta1 = await assistenteMedico.ExecuteAsync(
                "Doutor, estou com dor de cabeça forte há 3 dias, principalmente pela manhã. Também sinto náuseas."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🩺 MÉDICO: {resposta1.Data}").ResetColor();
            Console.WriteLine();

            // === CONSULTA 2: Medicação ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 PACIENTE: Estou tomando paracetamol 500mg de 8 em 8 horas, mas não alivia muito. Sou alérgico a dipirona.").ResetColor();
            
            var resposta2 = await assistenteMedico.ExecuteAsync(
                "Estou tomando paracetamol 500mg de 8 em 8 horas, mas não alivia muito. Sou alérgico a dipirona."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🩺 MÉDICO: {resposta2.Data}").ResetColor();
            Console.WriteLine();

            // === CONSULTA 3: Exames ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 PACIENTE: Trouxe os resultados dos exames que o senhor pediu. Hemograma normal, mas a pressão estava 140x90.").ResetColor();
            
            var resposta3 = await assistenteMedico.ExecuteAsync(
                "Trouxe os resultados dos exames que o senhor pediu. Hemograma normal, mas a pressão estava 140x90."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🩺 MÉDICO: {resposta3.Data}").ResetColor();

            await MostrarMemorias(assistenteMedico);
        }

        /// <summary>
        /// Exemplo 6: Consultor Jurídico com Memória Especializada  
        /// </summary>
        public static async Task ExecutarConsultorJuridico(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("⚖️ EXEMPLO: Consultor Jurídico com Configuração Especializada")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var storage = new SqliteStorage("Data Source=exemplo_juridico.db");
            await storage.InitializeAsync();

            var context = new ProjetoContext 
            { 
                UserId = "advogado_maria", 
                SessionId = "caso_empresarial_2024",
                ProjetoNome = "Caso: Disputa Contratual XYZ Corp"
            };

            var consultorJuridico = new Agent<ProjetoContext, string>(modelo, "ConsultorJuridico", storage: storage)
                .WithPersona("Você é um advogado especializado em direito empresarial e contratos")
                .WithMemoryCategories("CaseDetail", "LegalPrecedent", "ClientInfo", "Deadline", "Contract", "Evidence")
                .WithMemoryExtraction((userMsg, assistantMsg) => $@"
                    Extraia informações jurídicas relevantes desta consulta:
                    
                    Cliente: {userMsg}
                    Advogado: {assistantMsg}
                    
                    PRIORIZAR:
                    - Detalhes específicos do caso
                    - Prazos legais mencionados
                    - Precedentes ou jurisprudência citada
                    - Informações contratuais
                    - Evidências ou documentos
                    - Dados do cliente relevantes ao caso
                    
                    JSON: {{""memories"": [{{""content"": ""Contrato assinado em 15/03/2024 com cláusula de rescisão"", ""type"": ""Contract"", ""importance"": 0.8}}]}}")
                .WithMemoryThresholds(maxMemories: 12, minImportance: 0.6) // Mais permissivo para contexto legal
                .WithContext(context);

            Console.WriteLine("⚖️ Simulando consulta jurídica...\n");

            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 CLIENTE: Nossa empresa tem um contrato com a XYZ Corp assinado em março de 2024. Eles querem rescindir alegando força maior.").ResetColor();
            
            var resposta1 = await consultorJuridico.ExecuteAsync(
                "Nossa empresa tem um contrato com a XYZ Corp assinado em março de 2024. Eles querem rescindir alegando força maior."
            );

            _console.WithColor(ConsoleColor.Blue).WriteLine($"⚖️ ADVOGADO: {resposta1.Data}").ResetColor();

            await MostrarMemorias(consultorJuridico);
        }


        /// <summary>
        /// Exemplo 7: Modo Anônimo - IDs Automáticos
        /// </summary>
        public static async Task ExecutarModoAnonimo(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🎭 EXEMPLO: Modo Anônimo - IDs Automáticos")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            // === TESTE 1: Modo Anônimo Habilitado ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("👤 TESTE 1: Modo Anônimo - IDs Gerados Automaticamente").ResetColor();
            
            var storage1 = new InMemoryStorage();
            var agenteAnonimo = new Agent<object, string>(modelo, "AgenteAnonimo", storage: storage1)
                .WithAnonymousMode(true)
                .WithPersona("Assistente que funciona sem autenticação prévia");

            var resultado1 = await agenteAnonimo.ExecuteAsync("Olá! Sou um novo usuário.");
            
            _console.WithColor(ConsoleColor.Cyan)
                .WriteLine($"📝 Resposta: {resultado1.Data}")
                .WriteLine($"👤 User ID: {resultado1.SessionInfo.UserId}")
                .WriteLine($"🔗 Session ID: {resultado1.SessionInfo.SessionId}")
                .WriteLine($"🔄 IDs Gerados: {resultado1.SessionInfo.WasGenerated}")
                .WriteLine($"🎭 É Anônimo: {resultado1.SessionInfo.IsAnonymous}")
                .ResetColor();

            // Segundo pedido para verificar consistência
            var resultado2 = await agenteAnonimo.ExecuteAsync("Você lembra de mim?");
            
            _console.WithColor(ConsoleColor.Green)
                .WriteLine($"📝 Segunda resposta: {resultado2.Data}")
                .WriteLine($"👤 User ID (consistente): {resultado2.SessionInfo.UserId}")
                .WriteLine($"🔗 Session ID (consistente): {resultado2.SessionInfo.SessionId}")
                .ResetColor();

            Console.WriteLine();

            // === TESTE 2: Modo Regular vs Anônimo ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("⚖️ TESTE 2: Comparação Modo Regular vs Anônimo").ResetColor();
            
            var storage2 = new InMemoryStorage();
            var agenteRegular = new Agent<object, string>(modelo, "AgenteRegular", storage: storage2)
                .WithPersona("Assistente padrão");

            var resultadoRegular = await agenteRegular.ExecuteAsync("Hello from regular mode");
            
            _console.WithColor(ConsoleColor.Blue)
                .WriteLine("🔒 MODO REGULAR:")
                .WriteLine($"   👤 User ID: {resultadoRegular.SessionInfo.UserId}")
                .WriteLine($"   🔗 Session ID: {resultadoRegular.SessionInfo.SessionId}")
                .WriteLine($"   🔄 IDs Gerados: {resultadoRegular.SessionInfo.WasGenerated}")
                .WriteLine($"   🎭 É Anônimo: {resultadoRegular.SessionInfo.IsAnonymous}")
                .ResetColor();

            Console.WriteLine();

            // === TESTE 3: Contexto Parcial com Modo Anônimo ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("🧩 TESTE 3: Contexto Parcial + Modo Anônimo").ResetColor();
            
            var contextoPartial = new UsuarioContext { UserId = "user_fornecido" }; // SessionId ausente
            var storage3 = new InMemoryStorage();
            var agenteParcial = new Agent<UsuarioContext, string>(modelo, "AgenteParcial", storage: storage3)
                .WithAnonymousMode(true)
                .WithContext(contextoPartial)
                .WithPersona("Assistente que completa IDs em falta");

            var resultadoParcial = await agenteParcial.ExecuteAsync("Teste com contexto parcial");
            
            _console.WithColor(ConsoleColor.DarkYellow)
                .WriteLine("🧩 CONTEXTO PARCIAL:")
                .WriteLine($"   👤 User ID (fornecido): {resultadoParcial.SessionInfo.UserId}")
                .WriteLine($"   🔗 Session ID (gerado): {resultadoParcial.SessionInfo.SessionId}")
                .WriteLine($"   🔄 IDs Gerados: {resultadoParcial.SessionInfo.WasGenerated}")
                .WriteLine($"   🎭 É Anônimo: {resultadoParcial.SessionInfo.IsAnonymous}")
                .ResetColor();

            Console.WriteLine();

            // === RESUMO ===
            _console.WithColor(ConsoleColor.Green)
                .WriteLine("✅ VANTAGENS DO MODO ANÔNIMO:")
                .WriteLine("   • Funcionamento sem autenticação prévia")
                .WriteLine("   • IDs únicos gerados automaticamente")
                .WriteLine("   • Consistência dentro da mesma sessão")
                .WriteLine("   • Compatível com storage persistente")
                .WriteLine("   • Flexibilidade para sistemas de demonstração")
                .ResetColor();

            _console.WithColor(ConsoleColor.Cyan)
                .WriteLine("💡 CASOS DE USO:")
                .WriteLine("   • Aplicações web sem login obrigatório")
                .WriteLine("   • Demos e protótipos rápidos")
                .WriteLine("   • Testes de integração")
                .WriteLine("   • Sistemas com autenticação opcional")
                .ResetColor();
        }
    }

    // === CLASSES DE CONTEXTO ===

    public class UsuarioContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
    }

    public class ProjetoContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string ProjetoNome { get; set; }
    }
}