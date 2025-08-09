using AgentSharp.Core;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Exemplo demonstrando o sistema de memória vetorial avançado
    /// </summary>
    public static class VectorMemoryExample
    {
        private static readonly ConsoleObj _console = new();

        /// <summary>
        /// Exemplo: Assistente com busca semântica usando embeddings
        /// </summary>
        public static async Task ExecutarAssistenteComEmbeddings(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Cyan)
                .WriteLine("🔍 NÍVEL 3 - BUSCA SEMÂNTICA: Embeddings e Vetores")
                .WriteLine("═══════════════════════════════════════════════════════")
                .ResetColor();

            Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
            Console.WriteLine("   • OpenAIEmbeddingService - geração de embeddings");
            Console.WriteLine("   • VectorSqliteStorage - armazenamento vetorial");
            Console.WriteLine("   • Semantic search - busca por significado");
            Console.WriteLine("   • Similarity scoring - pontuação de similaridade");
            Console.WriteLine("   • Knowledge base - base de conhecimento vetorial\n");

            // Configurar serviço de embedding
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint, new ConsoleLogger(), "text-embedding-ada-002");

            // Configurar storage vetorial
            var storage = new VectorSqliteStorage("Data Source=exemplo_vetorial.db", embeddingService, new ConsoleLogger());
            await storage.InitializeAsync();

            var context = new UsuarioContext 
            { 
                UserId = "user_vector", 
                SessionId = "session_semantic_search" 
            };

            // Configurar agente com memória vetorial
            var assistente = new Agent<UsuarioContext, string>(modelo, "AssistenteSemantico", storage: storage)
                .WithPersona("Você é um assistente inteligente que usa busca semântica para lembrar de informações relevantes")
                .WithInstructions(@"
                    - Use suas memórias para contextualizar suas respostas
                    - Demonstre que você entende conexões semânticas entre conceitos
                    - Seja específico ao referenciar informações passadas")
                .WithContext(context);

            Console.WriteLine("🤖 Assistente com busca semântica avançada...\n");

            // === CONVERSA 1: Estabelecer preferências ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Olá! Sou desenvolvedor Python e adoro inteligência artificial. Trabalho principalmente com machine learning.").ResetColor();
            
            var resposta1 = await assistente.ExecuteAsync(
                "Olá! Sou desenvolvedor Python e adoro inteligência artificial. Trabalho principalmente com machine learning."
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta1.Data}").ResetColor();
            Console.WriteLine();

            // === CONVERSA 2: Testar busca semântica ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Preciso de ajuda com redes neurais. Que framework você recomenda?").ResetColor();
            
            var resposta2 = await assistente.ExecuteAsync(
                "Preciso de ajuda com redes neurais. Que framework você recomenda?"
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta2.Data}").ResetColor();
            Console.WriteLine();

            // === CONVERSA 3: Testar conexão semântica mais sutil ===
            _console.WithColor(ConsoleColor.Cyan).WriteLine("👤 USUÁRIO: Quais linguagens de programação são boas para ciência de dados?").ResetColor();
            
            var resposta3 = await assistente.ExecuteAsync(
                "Quais linguagens de programação são boas para ciência de dados?"
            );

            _console.WithColor(ConsoleColor.Green).WriteLine($"🤖 ASSISTENTE: {resposta3.Data}").ResetColor();
            Console.WriteLine();

            // Mostrar memórias armazenadas
            await MostrarMemorias(assistente);
            
            _console.WithColor(ConsoleColor.Magenta)
                .WriteLine("\n💡 VANTAGENS DA BUSCA SEMÂNTICA:")
                .WriteLine("✅ Encontra memórias relacionadas mesmo sem palavras exatas")
                .WriteLine("✅ 'Redes neurais' conecta com 'machine learning' e 'IA'")
                .WriteLine("✅ 'Ciência de dados' relaciona com 'Python' e 'ML'")
                .WriteLine("✅ Compreensão de contexto muito mais rica")
                .ResetColor();
        }

        /// <summary>
        /// Exemplo: Comparação entre busca textual e semântica
        /// </summary>
        public static async Task CompararBuscaTextualVsSemantica(IModel modelo)
        {
            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("📊 EXEMPLO: Comparação Busca Textual vs Semântica")
                .WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .ResetColor();

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";

            var context = new UsuarioContext 
            { 
                UserId = "test_comparison", 
                SessionId = "comparison_session" 
            };

            // === TESTE 1: Busca Textual Tradicional ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("🔤 TESTE 1: Busca Textual (SqliteStorage)").ResetColor();
            
            var textualStorage = new SqliteStorage("Data Source=textual_test.db");
            await textualStorage.InitializeAsync();
            
            var agenteTextual = new Agent<UsuarioContext, string>(modelo, "AgenteTextual", storage: textualStorage)
                .WithPersona("Assistente com busca textual tradicional")
                .WithContext(context);

            await agenteTextual.ExecuteAsync("Gosto muito de café expresso forte pela manhã");
            await Task.Delay(1000); // Pequena pausa para processamento
            
            var resultadoTextual = await agenteTextual.ExecuteAsync("Como preparar uma bebida energizante matinal?");
            _console.WithColor(ConsoleColor.Blue).WriteLine($"📝 Textual: {resultadoTextual.Data}").ResetColor();

            // === TESTE 2: Busca Semântica ===
            _console.WithColor(ConsoleColor.Magenta).WriteLine("\n🧠 TESTE 2: Busca Semântica (VectorStorage)").ResetColor();
            
            var embeddingService = new OpenAIEmbeddingService(apiKey, endpoint);
            var vectorStorage = new VectorSqliteStorage("Data Source=vector_test.db", embeddingService);
            await vectorStorage.InitializeAsync();
            
            var agenteSemantico = new Agent<UsuarioContext, string>(modelo, "AgenteSemantico", storage: vectorStorage)
                .WithPersona("Assistente com busca semântica avançada")
                .WithContext(context);

            await agenteSemantico.ExecuteAsync("Gosto muito de café expresso forte pela manhã");
            await Task.Delay(1000); // Pequena pausa para processamento
            
            var resultadoSemantico = await agenteSemantico.ExecuteAsync("Como preparar uma bebida energizante matinal?");
            _console.WithColor(ConsoleColor.Green).WriteLine($"🧠 Semântico: {resultadoSemantico.Data}").ResetColor();

            _console.WithColor(ConsoleColor.Yellow)
                .WriteLine("\n📋 ANÁLISE:")
                .WriteLine("• Busca Textual: Busca palavras exatas como 'café', 'forte', 'manhã'")
                .WriteLine("• Busca Semântica: Entende que 'bebida energizante matinal' se relaciona com 'café expresso forte pela manhã'")
                .WriteLine("• Resultado: Busca semântica oferece contexto muito mais rico e relevante")
                .ResetColor();
        }

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
    }
}