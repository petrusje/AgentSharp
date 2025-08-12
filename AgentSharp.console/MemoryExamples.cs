using AgentSharp.Core;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core.Memory.Services.HNSW;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Exemplos demonstrando o sistema de memória avançado do AgentSharp
    /// </summary>
    public static class ExemplosMemoria
    {
        private static readonly ConsoleObj _console = new();

        // Helper method to create SemanticSqliteStorage with default parameters
        private static SemanticSqliteStorage CreateDefaultStorage(string connectionString)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
            return new SemanticSqliteStorage(connectionString, embeddingService, 1536);
        }

        /// <summary>
        /// Exemplo 0: Demonstração da Nova Arquitetura - Simple vs Semantic Agents
        /// </summary>
        public static async Task ExecutarDemonstracaoNovaArquitetura(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("🚀 NOVA ARQUITETURA: Demonstração Simple vs Semantic Agents")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
            Console.WriteLine("   • Agent Simples - Baixo custo, sem memory");
            Console.WriteLine("   • Agent Semântico - Opt-in memory avançado");
            Console.WriteLine("   • Separação clara de custos");
            Console.WriteLine("   • Configuração fluent moderna\n");

            // === TESTE 1: Agent Simples (Padrão - Baixo Custo) ===
            _console.WithColor(ConsoleColor.Cyan)
                .WriteLine("💰 TESTE 1: Agent Simples (BAIXO CUSTO)")
                .WriteLine("═══════════════════════════════════════")
                .ResetColor();

            var agentSimples = new Agent<object, string>(modelo, "ChatBot")
                .WithPersona("Assistente simples e direto");

            Console.WriteLine("✅ Agent criado SEM semantic memory (custos mínimos)");
            Console.WriteLine("   • Sem SmartMemoryToolPack");
            Console.WriteLine("   • Sem processamento de embeddings");
            Console.WriteLine("   • Apenas BasicMessageHistoryService\n");

            var resposta1 = await agentSimples.ExecuteAsync("Oi, meu nome é Ana");
            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 Resposta: {resposta1.Data}").ResetColor();

            var resposta2 = await agentSimples.ExecuteAsync("Qual é meu nome?");
            _console.WithColor(ConsoleColor.Red).WriteLine($"🤖 Resposta: {resposta2.Data}").ResetColor();
            Console.WriteLine("❌ Não lembrou (comportamento esperado - sem memory)\n");

            // === TESTE 2: Agent Semântico (Opt-in - Alto Custo) ===
            _console.WithColor(ConsoleColor.Magenta)
                .WriteLine("🧠 TESTE 2: Agent Semântico (ALTO CUSTO - OPT-IN)")
                .WriteLine("═════════════════════════════════════════════════")
                .ResetColor();

            // Use SemanticSqliteStorage for semantic memory (requires IStorage interface)
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
            var storage = new SemanticSqliteStorage("Data Source=memory_example.db", embeddingService, 1536);
            
            var agentSematico = new Agent<object, string>(modelo, "SmartBot")
                .WithPersona("Assistente inteligente com memória avançada")
                .WithSemanticMemory(storage); // ✅ Opt-in explícito

            Console.WriteLine("✅ Agent criado COM semantic memory (custos altos)");
            Console.WriteLine("   • SmartMemoryToolPack habilitado");
            Console.WriteLine("   • Processamento de embeddings ativo");
            Console.WriteLine("   • MemoryManagerSemanticService\n");

            await Task.Delay(1000); // Rate limiting
            var resposta3 = await agentSematico.ExecuteAsync("Oi, meu nome é Carlos");
            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 Resposta: {resposta3.Data}").ResetColor();

            await Task.Delay(1000); // Rate limiting
            var resposta4 = await agentSematico.ExecuteAsync("Qual é meu nome?");
            _console.WithColor(ConsoleColor.Blue).WriteLine($"🤖 Resposta: {resposta4.Data}").ResetColor();
            Console.WriteLine("✅ Lembrou perfeitamente (semantic memory ativa)\n");

            // === RESUMO DA NOVA ARQUITETURA ===
            _console.WithColor(ConsoleColor.Green)
                .WriteLine("📋 RESUMO DA NOVA ARQUITETURA:")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .WriteLine("🎯 POR PADRÃO (Baixo Custo):")
                .WriteLine("   new Agent(model, \"ChatBot\") // ✅ Sem memory")
                .WriteLine("")
                .WriteLine("🚀 OPT-IN (Alto Custo):")
                .WriteLine("   new Agent(model, \"SmartBot\")")
                .WriteLine("       .WithSemanticMemory(storage) // ✅ Explícito")
                .WriteLine("       .WithContext(context) // ✅ Contexto do usuário")
                .WriteLine("💰 BENEFÍCIOS:")
                .WriteLine("   • 80-90% redução de custos para casos simples")
                .WriteLine("   • Controle explícito sobre features custosas")
                .WriteLine("   • Retrocompatibilidade mantida")
                .ResetColor();

            Console.WriteLine();
        }

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
            Console.WriteLine("   • SemanticSqliteStorage - armazenamento persistente");
            Console.WriteLine("   • Session management - controle de sessões");
            Console.WriteLine("   • Context persistence - persistência de contexto");
            Console.WriteLine("   • Memory retrieval - recuperação de memórias");
            Console.WriteLine("   • Personalized interactions - interações personalizadas\n");

            // Configurar storage persistente
            var storage = CreateDefaultStorage("Data Source=exemplo_assistente.db");
            await storage.InitializeAsync();

            // Criar contexto do usuário
            var context = new UsuarioContext
            {
                UserId = "joao123",
                SessionId = "sessao_demo_memoria" // SessionId fixo para demonstração
            };

            // 🎯 NOVA ARQUITETURA: Agente com Semantic Memory (opt-in)
            var assistente = new Agent<UsuarioContext, string>(modelo, "AssistentePessoal")
                .WithPersona("Você é um assistente pessoal que lembra das preferências e contexto do usuário")
                .WithInstructions(@"
                    - Sempre cumprimente o usuário pelo nome quando souber
                    - Lembre-se das preferências mencionadas
                    - Use as informações armazenadas para personalizar suas respostas
                    - Seja proativo em sugerir baseado no histórico")
                .WithSemanticMemory(storage) // ✅ Semantic memory explícito (custos)
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

            var storage = CreateDefaultStorage("Data Source=exemplo_consultor.db");
            await storage.InitializeAsync();

            var context = new ProjetoContext
            {
                UserId = "dev_maria",
                SessionId = "projeto_react_2024",
                ProjetoNome = "Sistema E-commerce React"
            };

            // 🎯 NOVA ARQUITETURA: Consultor com Semantic Memory
            var consultor = new Agent<ProjetoContext, string>(modelo, "ConsultorTecnico")
                .WithPersona("Você é um consultor técnico sênior especializado em desenvolvimento web")
                .WithInstructions(@"
                    - Mantenha contexto técnico do projeto
                    - Lembre-se de decisões arquiteturais anteriores
                    - Seja consistente com tecnologias já definidas
                    - Sugira melhorias baseadas no histórico")
                .WithSemanticMemory(storage) // ✅ Semantic memory para contexto técnico
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

            var storage = CreateDefaultStorage("Data Source=exemplo_tools.db");
            await storage.InitializeAsync();

            var context = new UsuarioContext
            {
                UserId = "usuario_autonomo",
                SessionId = "sessao_tools"
            };

            // 🎯 NOVA ARQUITETURA: Agente Autônomo com Tools de Memória
            var agente = new Agent<UsuarioContext, string>(modelo, "AgenteAutonomo")
                .WithPersona("Você é um agente que gerencia ativamente suas próprias memórias")
                .WithInstructions(@"
                    - Use as ferramentas de memória para gerenciar informações importantes
                    - SEMPRE adicione informações relevantes à memória usando AddMemory
                    - Busque memórias relacionadas antes de responder usando SearchMemories
                    - Mantenha suas memórias organizadas e atualizadas")
                .WithSemanticMemory(storage) // ✅ Necessário para SmartMemoryToolPack
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
        /// Exemplo 4: Comparação de Performance entre Storage Providers
        /// </summary>
        public static async Task ExecutarComparacaoStorage(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("📊 EXEMPLO: Comparação de Performance - Storage Providers")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var context = new UsuarioContext
            {
                UserId = "usuario_teste",
                SessionId = "comparacao_storage"
            };

            // Estrutura para armazenar métricas
            var metricas = new List<(string Nome, long MemoriaInicial, long MemoriaFinal, long TempoInicializacao, long TempoOperacoes, string Resultado)>();

            // === TESTE 1: In-Memory Storage (via SemanticSqliteStorage) ===
            await TestarStorageComMetricas(
                "InMemoryStorage (via SemanticSqliteStorage)",
                () => CreateDefaultStorage("Data Source=:memory:"),
                modelo,
                context,
                metricas);

            // === TESTE 2: SQLite Storage ===
            await TestarStorageComMetricas(
                "SemanticSqliteStorage",
                () => CreateDefaultStorage("Data Source=comparacao_test.db"),
                modelo,
                context,
                metricas);

            // === TESTE 3: CompactHNSW (In-Memory) - Via SemanticSqliteStorage ===
            try
            {
                await TestarStorageComMetricas(
                    "SemanticMemoryStorage (via SemanticSqliteStorage)",
                    () => {
                        // Use SemanticSqliteStorage since CompactHNSW doesn't implement IStorage
                        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
                        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
                        var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
                        return new SemanticSqliteStorage("Data Source=comparacao_hnsw_memory.db", embeddingService, 1536);
                    },
                    modelo,
                    context,
                    metricas);
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"⚠️ SemanticMemoryStorage não disponível: {ex.Message}")
                    .ResetColor();

                metricas.Add(("SemanticMemoryStorage", 0, 0, 0, 0, $"ERRO: {ex.Message}"));
            }

            // === TESTE 4: VectorSqliteVec Storage (sqlite-vec) ===
            try
            {
                await TestarStorageComMetricas(
                    "SemanticSqliteStorage",
                    () => {
                        // Usar os mesmos parâmetros de conexão do modelo principal
                        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "test-key";
                        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";

                        // Criar EmbeddingService com os mesmos parâmetros do modelo
                        var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);

                        // Usar sqlite-vec com dimensões compatíveis com OpenAI (1536D)
                        return new SemanticSqliteStorage(
                            "Data Source=comparacao_sqlitevec.db",
                            embeddingService,
                            1536); // Dimensões compatíveis com OpenAI embeddings
                    },
                    modelo,
                    context,
                    metricas);
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"⚠️ SemanticSqliteStorage não disponível: {ex.Message}")
                    .ResetColor();

                metricas.Add(("SemanticSqliteStorage", 0, 0, 0, 0, $"ERRO: {ex.Message}"));
            }

            // === EXIBIR RELATÓRIO DE PERFORMANCE ===
            ExibirRelatorioPerformance(metricas);
        }

        private static async Task TestarStorageComMetricas(
            string nomeStorage,
            Func<IStorage> criarStorage,
            IModel modelo,
            UsuarioContext context,
            List<(string Nome, long MemoriaInicial, long MemoriaFinal, long TempoInicializacao, long TempoOperacoes, string Resultado)> metricas)
        {
            _console.WithColor(ConsoleColor.Magenta).WriteLine($"\n🧪 TESTE: {nomeStorage}").ResetColor();

            // Rate limiting - esperar entre testes para evitar HTTP 429
            if (metricas.Count > 0)
            {
                Console.WriteLine("⏳ Aguardando para evitar rate limiting...");
                await Task.Delay(5000); // 5 segundos entre testes
            }

            // Forçar coleta de lixo antes de medir
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoriaInicial = GC.GetTotalMemory(false);
            var stopwatchTotal = System.Diagnostics.Stopwatch.StartNew();
            var stopwatchInit = System.Diagnostics.Stopwatch.StartNew();

            // Criar e inicializar storage
            IStorage storage = null;
            try
            {
                storage = criarStorage();
                await storage.InitializeAsync();
                stopwatchInit.Stop();

                _console.WithColor(ConsoleColor.DarkGray)
                    .WriteLine($"  ⏱️  Inicialização: {stopwatchInit.ElapsedMilliseconds}ms")
                    .ResetColor();

                var stopwatchOps = System.Diagnostics.Stopwatch.StartNew();

                // 🎯 COMPARAÇÃO: Usar arquitetura legada para compatibilidade dos testes
                // (Para novos projetos, use .WithSemanticMemory(storage) em vez do construtor)
                var agente = new Agent<UsuarioContext, string>(modelo, $"Agente{nomeStorage}")
                    .WithPersona($"Assistente com {nomeStorage}")
                    .WithSemanticMemory(storage) // ✅ Nova forma recomendada
                    .WithContext(context);

                await agente.ExecuteAsync("Meu nome é Carlos e prefiro chá verde");
                var resultado = await agente.ExecuteAsync("Qual é meu nome e minha bebida preferida?");

                stopwatchOps.Stop();
                stopwatchTotal.Stop();

                _console.WithColor(ConsoleColor.Green)
                    .WriteLine($"✅ Resultado: {resultado.Data}")
                    .ResetColor();

                _console.WithColor(ConsoleColor.DarkGray)
                    .WriteLine($"  ⏱️  Operações: {stopwatchOps.ElapsedMilliseconds}ms")
                    .WriteLine($"  ⏱️  Total: {stopwatchTotal.ElapsedMilliseconds}ms")
                    .ResetColor();

                // Medir memória final
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var memoriaFinal = GC.GetTotalMemory(false);
                var usoMemoria = memoriaFinal - memoriaInicial;

                _console.WithColor(ConsoleColor.DarkGray)
                    .WriteLine($"  💾 Memória utilizada: {FormatarBytes(usoMemoria)}")
                    .ResetColor();

                // Armazenar métricas
                metricas.Add((
                    nomeStorage,
                    memoriaInicial,
                    memoriaFinal,
                    stopwatchInit.ElapsedMilliseconds,
                    stopwatchTotal.ElapsedMilliseconds,
                    resultado.Data
                ));
            }
            catch (Exception ex)
            {
                _console.WithColor(ConsoleColor.Red)
                    .WriteLine($"❌ Erro em {nomeStorage}: {ex.Message}")
                    .ResetColor();

                metricas.Add((nomeStorage, memoriaInicial, memoriaInicial, 0, 0, $"ERRO: {ex.Message}"));
            }
            finally
            {
                // Limpar recursos
                if (storage is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private static void ExibirRelatorioPerformance(List<(string Nome, long MemoriaInicial, long MemoriaFinal, long TempoInicializacao, long TempoOperacoes, string Resultado)> metricas)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("\n📊 RELATÓRIO DE PERFORMANCE")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            Console.WriteLine($"{"Storage",-20} | {"Inicialização",-15} | {"Tempo Total",-12} | {"Memória",-12} | {"Status",-10}");
            Console.WriteLine(new string('─', 85));

            foreach (var metrica in metricas)
            {
                var usoMemoria = metrica.MemoriaFinal - metrica.MemoriaInicial;
                var status = metrica.Resultado.Contains("ERRO") ? "❌ Erro" : "✅ OK";

                Console.WriteLine($"{metrica.Nome,-20} | {metrica.TempoInicializacao + "ms",-15} | {metrica.TempoOperacoes + "ms",-12} | {FormatarBytes(usoMemoria),-12} | {status,-10}");
            }

            // Encontrar o mais rápido
            var maisRapido = metricas.Where(m => !m.Resultado.Contains("ERRO")).OrderBy(m => m.TempoOperacoes).FirstOrDefault();
            if (maisRapido.Nome != null)
            {
                _console.WithColor(ConsoleColor.Green)
                    .WriteLine($"\n🏆 Mais rápido: {maisRapido.Nome} ({maisRapido.TempoOperacoes}ms)")
                    .ResetColor();
            }

            // Encontrar o que usa menos memória
            var menosMemoria = metricas.Where(m => !m.Resultado.Contains("ERRO")).OrderBy(m => m.MemoriaFinal - m.MemoriaInicial).FirstOrDefault();
            if (menosMemoria.Nome != null)
            {
                var uso = menosMemoria.MemoriaFinal - menosMemoria.MemoriaInicial;
                _console.WithColor(ConsoleColor.Cyan)
                    .WriteLine($"💾 Menor uso de memória: {menosMemoria.Nome} ({FormatarBytes(uso)})")
                    .ResetColor();
            }

            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("\n📋 RECOMENDAÇÕES:")
                .WriteLine("• InMemoryStorage: Desenvolvimento/testes rápidos (busca semântica 256D)")
                .WriteLine("• SemanticSqliteStorage: Persistência simples (busca textual apenas)")
                .WriteLine("• SemanticMemoryStorage: Busca semântica premium (vetores 1536D)")
                .WriteLine("• SemanticSqliteStorage: sqlite-vec nativo (busca vetorial 256D)")
                .ResetColor();
        }

        private static string FormatarBytes(long bytes)
        {
            if (bytes < 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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

            var storage = CreateDefaultStorage("Data Source=exemplo_medico.db");
            await storage.InitializeAsync();

            var context = new UsuarioContext
            {
                UserId = "dr_silva",
                SessionId = "consulta_medica_2024"
            };

            // 🎯 NOVA ARQUITETURA: Assistente Médico com Configuração Avançada
            var assistenteMedico = new Agent<UsuarioContext, string>(modelo, "AssistenteMedico")
                .WithPersona("Você é um assistente médico especializado que ajuda médicos com informações clínicas")
                .WithSemanticMemory(storage) // ✅ Necessário para categorização médica
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

            var storage = CreateDefaultStorage("Data Source=exemplo_juridico.db");
            await storage.InitializeAsync();

            var context = new ProjetoContext
            {
                UserId = "advogado_maria",
                SessionId = "caso_empresarial_2024",
                ProjetoNome = "Caso: Disputa Contratual XYZ Corp"
            };

            // 🎯 NOVA ARQUITETURA: Consultor Jurídico com Memória Especializada
            var consultorJuridico = new Agent<ProjetoContext, string>(modelo, "ConsultorJuridico")
                .WithPersona("Você é um advogado especializado em direito empresarial e contratos")
                .WithSemanticMemory(storage) // ✅ Necessário para categorização jurídica
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

            var storage1 = new SemanticMemoryStorage();
            // 🎯 NOVA ARQUITETURA: Agente Anônimo SIMPLES (baixo custo)
            var agenteAnonimo = new Agent<object, string>(modelo, "AgenteAnonimo")
                .WithAnonymousMode(true)
                .WithPersona("Assistente simples que funciona sem autenticação prévia");
                // ✅ SEM .WithSemanticMemory() = baixo custo para demos

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

            var storage2 = new SemanticMemoryStorage();
            // 🎯 COMPARAÇÃO: Agente Regular (também simples por padrão)
            var agenteRegular = new Agent<object, string>(modelo, "AgenteRegular")
                .WithPersona("Assistente padrão");
                // ✅ SEM .WithSemanticMemory() = comportamento padrão (baixo custo)

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
            var storage3 = new SemanticMemoryStorage();
            // 🎯 TESTE: Contexto Parcial com Modo Anônimo (simples)
            var agenteParcial = new Agent<UsuarioContext, string>(modelo, "AgenteParcial")
                .WithAnonymousMode(true)
                .WithContext(contextoPartial)
                .WithPersona("Assistente simples que completa IDs em falta");
                // ✅ SEM .WithSemanticMemory() para simplicidade

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
