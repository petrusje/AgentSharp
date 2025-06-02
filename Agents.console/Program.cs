using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Attributes;
using Agents.net.Tools;
using Agents.net.Utils;
using Agents.net.Exceptions;
using Agents.net.Examples;
using DotNetEnv;

namespace Agents.console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Carregamento do arquivo .env
            Env.TraversePath().Load();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              🤖 AGENTS.NET - EXEMPLOS PRÁTICOS               ║");
            Console.WriteLine("║                Sistema de Demonstração Interativo            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            // Verificar API Key
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://proxy.dta.totvs.ai/";
            
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Erro: Configure a variável de ambiente OPENAI_API_KEY");
                Console.WriteLine("   1. Copie o arquivo env.example para .env");
                Console.WriteLine("   2. Edite o arquivo .env com sua chave da OpenAI");
                Console.WriteLine("   3. Execute novamente o programa");
                Console.WriteLine($"   Arquivo .env deve estar em: {System.IO.Directory.GetCurrentDirectory()}");
                Console.ResetColor();
                return;
            }
            
            Console.WriteLine($"✅ API Key encontrada! Endpoint: {endpoint}");

            try
            {
                // Configurar modelo usando variáveis de ambiente
                var modelName = Environment.GetEnvironmentVariable("MODEL_NAME") ?? "gpt-4o-mini";
                var temperature = double.TryParse(Environment.GetEnvironmentVariable("TEMPERATURE"), out var temp) ? temp : 0.7;
                var maxTokens = int.TryParse(Environment.GetEnvironmentVariable("MAX_TOKENS"), out var tokens) ? tokens : 2048;
                
                Console.WriteLine($"🔧 Debug: MODEL_NAME env var = '{Environment.GetEnvironmentVariable("MODEL_NAME")}'");
                Console.WriteLine($"🔧 Debug: Using model name = '{modelName}'");
                
                var modelFactory = new ModelFactory();
                var modelOptions = new ModelOptions
                {
                    ModelName = modelName,
                    ApiKey = apiKey,
                    Endpoint = endpoint,
                    DefaultConfiguration = new ModelConfiguration
                    {
                        Temperature = temperature,
                        MaxTokens = maxTokens
                    }
                };

                IModel modelo = modelFactory.CreateModel("openai", modelOptions);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Modelo OpenAI inicializado com sucesso!");
                Console.WriteLine($"   Modelo: {modelName} | Temp: {temperature} | Max Tokens: {maxTokens}");
                Console.ResetColor();

                await ExibirMenu(modelo);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro fatal: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        static async Task ExibirMenu(IModel modelo)
        {
            while (true)
            {
                Console.WriteLine("📋 MENU PRINCIPAL - Escolha uma demonstração:");
                Console.WriteLine();
                Console.WriteLine("🔤 EXEMPLOS BÁSICOS:");
                Console.WriteLine("  1. 📰 Jornalista com Personalidade");
                Console.WriteLine("  2. 🔍 Jornalista com Busca Web");
                Console.WriteLine("  3. 💰 Analista Financeiro");
                Console.WriteLine();
                Console.WriteLine("🧠 EXEMPLOS DE RACIOCÍNIO:");
                Console.WriteLine("  4. 🔬 Resolvedor de Problemas");
                Console.WriteLine("  5. ⚖️ Avaliador de Soluções");
                Console.WriteLine("  6. 🛡️ Identificador de Obstáculos");
                Console.WriteLine();
                Console.WriteLine("📊 EXEMPLOS STRUCTURED OUTPUTS:");
                Console.WriteLine("  7. 📄 Análise de Documentos Empresariais");
                Console.WriteLine("  8. 👤 Análise de Currículos");
                Console.WriteLine();
                Console.WriteLine("🔄 EXEMPLOS WORKFLOW:");
                Console.WriteLine("  9. 📈 Workflow Multi-etapa");
                Console.WriteLine();
                Console.WriteLine("  0. ❌ Sair");
                Console.WriteLine();
                Console.Write("Digite sua escolha (0-9): ");

                var escolha = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (escolha)
                    {
                        case "1":
                            await ExecutarExemplo("Jornalista com Personalidade", () => ExemplosBasicos.ExecutarJornalistaMineiro(modelo));
                            break;
                        case "2":
                            await ExecutarExemplo("Jornalista com Busca Web", () => ExemplosBasicos.ExecutarReporterComFerramentas(modelo));
                            break;
                        case "3":
                            await ExecutarExemplo("Analista Financeiro", () => ExemplosBasicos.ExecutarAnalistaFinanceiroBH(modelo));
                            break;
                        case "4":
                            await ExecutarExemplo("Resolvedor de Problemas", () => ExemplosRaciocinio.ExecutarResolvedorProblemas(modelo));
                            break;
                        case "5":
                            await ExecutarExemplo("Avaliador de Soluções", () => ExemplosRaciocinio.ExecutarAvaliadorSolucoes(modelo));
                            break;
                        case "6":
                            await ExecutarExemplo("Identificador de Obstáculos", () => ExemplosRaciocinio.ExecutarIdentificadorObstaculos(modelo));
                            break;
                        case "7":
                            await ExecutarExemplo("Análise de Documentos Empresariais", () => ExemplosStructured.ExecutarAnaliseDocumento(modelo));
                            break;
                        case "8":
                            await ExecutarExemplo("Análise de Currículos", () => ExemplosStructured.ExecutarAnaliseCurriculo(modelo));
                            break;
                        case "9":
                            await ExecutarExemplo("Workflow Multi-etapa", () => ExemplosWorkflow.ExecutarWorkflowCompleto(modelo));
                            break;
                        case "0":
                            Console.WriteLine("👋 Obrigado por usar Agents.net!");
                            return;
                        default:
                            Console.WriteLine("❌ Opção inválida. Tente novamente.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao executar exemplo: {ex.Message}");
                    Console.WriteLine($"🔍 Detalhes: {ex}");
                }

                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.Read();
                Console.Clear();
            }
        }

        static async Task ExecutarExemplo(string nome, Func<Task> exemplo)
        {
            Console.WriteLine($"🚀 Executando: {nome}");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine();

            var inicio = DateTime.Now;
            await exemplo();
            var duracao = DateTime.Now - inicio;

            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"⏱️ Tempo de execução: {duracao.TotalSeconds:F2}s");
        }

    }
}