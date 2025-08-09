using AgentSharp.Examples;
using AgentSharp.console;
using AgentSharp.Models;
using AgentSharp.Utils;
using DotNetEnv;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Agents_console
{
  class Program
  {
    static readonly ConsoleObj _consoleObj = new();

    static async Task Main(string[] args)
    {
      Env.TraversePath().Load();

      Console.OutputEncoding = System.Text.Encoding.UTF8;
      DisplayWelcomeMessage();

      var (apiKey, endpoint) = GetApiKeyAndEndpoint(args);
      if (string.IsNullOrWhiteSpace(apiKey))
      {
        DisplayApiKeyError();
        return;
      }

      Console.WriteLine($"✅ API Key encontrada! Endpoint: {endpoint}");

      try
      {
        var modelOptions = GetModelOptions(apiKey, endpoint);
        IModel modelo = InitializeModel(modelOptions);
        await DisplayMenu(modelo);
      }
      catch (Exception ex)
      {
        DisplayFatalError(ex);
      }
    }

    private static void DisplayWelcomeMessage()
    {
      _consoleObj.WithColor(ConsoleColor.Cyan)
        .WriteLine("╔══════════════════════════════════════════════════════════════╗")
        .WriteLine("║              🤖 AgentSharp - EXEMPLOS PRÁTICOS               ║")
        .WriteLine("║                Sistema de Demonstração Interativo            ║")
        .WriteLine("╚══════════════════════════════════════════════════════════════╝")
        .ResetColor();
    }

    private static (string apiKey, string endpoint) GetApiKeyAndEndpoint(string[] args)
    {
      const string OPENAI_API_KEY = "OPENAI_API_KEY";
      const string OPENAI_ENDPOINT = "OPENAI_ENDPOINT";
      const char VALUE_DELIMITER = '=';

      var argKey = args.FirstOrDefault(x => x.StartsWith($"{OPENAI_API_KEY}{VALUE_DELIMITER}", StringComparison.OrdinalIgnoreCase));
      var argEndpoint = args.FirstOrDefault(x => x.StartsWith($"{OPENAI_ENDPOINT}{VALUE_DELIMITER}", StringComparison.OrdinalIgnoreCase));
      if (!string.IsNullOrWhiteSpace(argKey) && !string.IsNullOrWhiteSpace(argEndpoint))
      {
        var keyValue = argKey.Split(VALUE_DELIMITER).LastOrDefault();
        var endpointValue = argEndpoint.Split(VALUE_DELIMITER).LastOrDefault();
        if (!string.IsNullOrWhiteSpace(keyValue) && !string.IsNullOrWhiteSpace(endpointValue))
          return (keyValue, endpointValue);
      }

      var apiKey = Environment.GetEnvironmentVariable(OPENAI_API_KEY);
      var endpoint = Environment.GetEnvironmentVariable(OPENAI_ENDPOINT) ?? "https://proxy.dta.totvs.ai/";
      return (apiKey, endpoint);
    }

    private static void DisplayApiKeyError()
    {
      _consoleObj.WithColor(ConsoleColor.Red)
        .WriteLine("❌ Erro: Variável de ambiente OPENAI_API_KEY não configurada!")
        .WriteLine("   1. Copie o arquivo env.example para .env")
        .WriteLine("   2. Edite o arquivo .env com sua chave da OpenAI")
        .WriteLine("   3. Execute novamente o programa")
        .WriteLine($"   Arquivo .env deve estar em: {System.IO.Directory.GetCurrentDirectory()}")
        .ResetColor();
    }

    private static ModelOptions GetModelOptions(string apiKey, string endpoint)
    {
      var modelName = Environment.GetEnvironmentVariable("MODEL_NAME") ?? "gpt-4o-mini";
      var temperature = double.TryParse(Environment.GetEnvironmentVariable("TEMPERATURE"), out var temp) ? temp : 0.7;
      var maxTokens = int.TryParse(Environment.GetEnvironmentVariable("MAX_TOKENS"), out var tokens) ? tokens : 2048;

      Console.WriteLine($"🔧 Debug: Using model name = '{modelName}'");

      return new ModelOptions
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
    }

    private static IModel InitializeModel(ModelOptions modelOptions)
    {
      var modelFactory = new ModelFactory();
      IModel modelo = modelFactory.CreateModel("openai", modelOptions);
      _consoleObj.WithColor(ConsoleColor.Green)
        .WriteLine("✅ Modelo OpenAI inicializado com sucesso!")
        .WriteLine($"   Modelo: {modelOptions.ModelName} | Temp: {modelOptions.DefaultConfiguration.Temperature} | Max Tokens: {modelOptions.DefaultConfiguration.MaxTokens}")
        .ResetColor();
      return modelo;
    }

    static async Task DisplayMenu(IModel modelo)
    {
      while (true)
      {
        DisplayMenuOptions();

        var userChoice = Console.ReadLine();
        Console.WriteLine();

        if (!await ProcessUserChoice(userChoice, modelo))
        {
          break;
        }

        Console.WriteLine();
        Console.WriteLine("Pressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
      }
    }

    private static async Task<bool> ProcessUserChoice(string choice, IModel modelo)
    {
      try
      {
        switch (choice)
        {
          // NÍVEL 1: FUNDAMENTOS
          case "1":
            await ExecuteExample("🎯 FUNDAMENTOS: Agente Simples", () => ExemplosBasicos.ExecutarAgenteSimples(modelo));
            break;
          case "2":
            await ExecuteExample("🎭 FUNDAMENTOS: Agente com Personalidade", () => ExemplosBasicos.ExecutarJornalistaMineiro(modelo));
            break;
          case "3":
            await ExecuteExample("🔧 FUNDAMENTOS: Agente com Tools", () => ExemplosBasicos.ExecutarReporterComFerramentas(modelo));
            break;
          // NÍVEL 2: INTERMEDIÁRIO
          case "4":
            await ExecuteExample("🧠 INTERMEDIÁRIO: Agente com Raciocínio", () => ExemplosRaciocinio.ExecutarResolvedorProblemas(modelo));
            break;
          case "5":
            await ExecuteExample("📊 INTERMEDIÁRIO: Outputs Estruturados", () => ExemplosStructured.ExecutarAnaliseDocumento(modelo));
            break;
          case "6":
            await ExecuteExample("💾 INTERMEDIÁRIO: Agente com Memória", () => ExemplosMemoria.ExecutarAssistentePessoal(modelo));
            break;
          // NÍVEL 3: AVANÇADO
          case "7":
            await ExecuteExample("🔄 AVANÇADO: Workflows Multi-agente", () => ExemplosWorkflow.ExecutarWorkflowCompleto(modelo));
            break;
          case "8":
            await ExecuteExample("🔍 AVANÇADO: Busca Semântica", () => VectorMemoryExample.ExecutarAssistenteComEmbeddings(modelo));
            break;
          case "9":
            await ExecuteExample("🏢 AVANÇADO: Sistema Empresarial Completo", () => ExemplosBasicos.ExecutarAnalistaFinanceiroRealData(modelo));
            break;
          // EXEMPLOS ESPECIALIZADOS (10-20)
          case "10":
            await ExecuteExample("🤖 ESPECIALIZADO: Assistente Pessoal com Memória", () => ExemplosMemoria.ExecutarAssistentePessoal(modelo));
            break;
          case "11":
            await ExecuteExample("🔧 ESPECIALIZADO: Consultor Técnico com Conhecimento", () => ExemplosMemoria.ExecutarConsultorTecnico(modelo));
            break;
          case "12":
            await ExecuteExample("🛠️ ESPECIALIZADO: LLM Gerenciando Memórias", () => ExemplosMemoria.ExecutarDemonstracaoMemoryTools(modelo));
            break;
          case "13":
            await ExecuteExample("📊 ESPECIALIZADO: Comparação Storage Providers", () => ExemplosMemoria.ExecutarComparacaoStorage(modelo));
            break;
          case "14":
            await ExecuteExample("🏥 ESPECIALIZADO: Assistente Médico Customizado", () => ExemplosMemoria.ExecutarAssistenteMedicoCustomizado(modelo));
            break;
          case "15":
            await ExecuteExample("⚖️ ESPECIALIZADO: Consultor Jurídico Especializado", () => ExemplosMemoria.ExecutarConsultorJuridico(modelo));
            break;
          case "16":
            await ExecuteExample("🎭 ESPECIALIZADO: Modo Anônimo - IDs Automáticos", () => ExemplosMemoria.ExecutarModoAnonimo(modelo));
            break;
          case "17":
            await ExecuteExample("🔍 ESPECIALIZADO: Busca Semântica Avançada", () => VectorMemoryExample.ExecutarAssistenteComEmbeddings(modelo));
            break;
          case "18":
            await ExecuteExample("📊 ESPECIALIZADO: Comparação Busca Textual vs Semântica", () => VectorMemoryExample.CompararBuscaTextualVsSemantica(modelo));
            break;
          case "19":
            await ExecuteExample("🚀 MODERNO: Busca Vetorial com sqlite-vec", () => VectorVecExample.ExecutarMenuVectorVec());
            break;
          case "20":
            await ExecuteExample("⚡ MODERNO: Exemplos Avançados sqlite-vec", () => VectorVecExample.ExecutarMenuAvancadoVectorVec());
            break;
          case "21":
            SqliteVecInstallationHelper.CheckAndGuideInstallation();
            break;
          case "22":
            SqliteVecInstallationHelper.ShowInstallationGuide();
            break;
          case "0":
            Console.WriteLine("👋 Obrigado por usar AgentSharp!");
            return false;
          default:
            Console.WriteLine("❌ Opção inválida. Tente novamente (0-22).");
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Erro ao executar exemplo: {ex.Message}");
        Console.WriteLine($"🔍 Detalhes: {ex}");
      }

      return true; // Continuar no loop
    }

    private static void DisplayMenuOptions()
    {
      Console.WriteLine("📋 MENU PRINCIPAL - Aprenda AgentSharp do Básico ao Avançado:");
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🌱 NÍVEL 1: FUNDAMENTOS - Conceitos Básicos");
      Console.ResetColor();
      Console.WriteLine("  1. 🎯 Agente Simples - Primeira Interação");
      Console.WriteLine("  2. 🎭 Agente com Personalidade - Customização Básica");
      Console.WriteLine("  3. 🔧 Agente com Tools - Ferramentas Integradas");
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🚀 NÍVEL 2: INTERMEDIÁRIO - Recursos Avançados");
      Console.ResetColor();
      Console.WriteLine("  4. 🧠 Agente com Raciocínio - Reasoning Chains");
      Console.WriteLine("  5. 📊 Outputs Estruturados - Dados Tipados");
      Console.WriteLine("  6. 💾 Agente com Memória - Persistência de Estado");
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("⚡ NÍVEL 3: AVANÇADO - Casos Complexos");
      Console.ResetColor();
      Console.WriteLine("  7. 🔄 Workflows Multi-agente - Orquestração");
      Console.WriteLine("  8. 🔍 Busca Semântica - Embeddings e Vetores");
      Console.WriteLine("  9. 🏢 Sistema Empresarial - Caso Real Completo");
      Console.WriteLine();
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine("🎓 EXEMPLOS ESPECIALIZADOS - Casos Específicos");
      Console.ResetColor();
      Console.WriteLine("  10. 🤖 Assistente Pessoal com Memória");
      Console.WriteLine("  11. 🔧 Consultor Técnico com Conhecimento");
      Console.WriteLine("  12. 🛠️ LLM Gerenciando Memórias");
      Console.WriteLine("  13. 📊 Comparação Storage Providers");
      Console.WriteLine("  14. 🏥 Assistente Médico Customizado");
      Console.WriteLine("  15. ⚖️ Consultor Jurídico Especializado");
      Console.WriteLine("  16. 🎭 Modo Anônimo - IDs Automáticos");
      Console.WriteLine("  17. 🔍 Assistente com Busca Semântica Avançada");
      Console.WriteLine("  18. 📊 Comparação Busca Textual vs Semântica");
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🚀 SQLITE-VEC - Busca Vetorial Moderna");
      Console.ResetColor();
      Console.WriteLine("  19. 🚀 sqlite-vec - Introdução e Exemplos Básicos");
      Console.WriteLine("  20. ⚡ sqlite-vec - Performance e Casos Avançados");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("🔧 INSTALAÇÃO E CONFIGURAÇÃO");
      Console.ResetColor();
      Console.WriteLine("  21. 🔍 Verificar Instalação sqlite-vec");
      Console.WriteLine("  22. 📋 Guia de Instalação Segura");
      Console.WriteLine();
      Console.WriteLine("  0. ❌ Sair");
      Console.WriteLine();
      Console.Write("Digite sua escolha (0-22): ");
    }

    static async Task ExecuteExample(string exampleName, Func<Task> example)
    {
      Console.WriteLine($"🚀 Executando: {exampleName}");
      _consoleObj.WriteSeparator();
      Console.WriteLine();

      var startTime = DateTime.Now;
      await example();
      var duration = DateTime.Now - startTime;

      Console.WriteLine();
      _consoleObj.WriteSeparator();
      Console.WriteLine($"⏱️ Tempo de execução: {duration.TotalSeconds:F2}s");
    }

    private static void DisplayFatalError(Exception ex)
    {
      _consoleObj.WithColor(ConsoleColor.Red)
        .WriteLine($"❌ Erro fatal: {ex.Message}")
        .WriteLine($"Stack trace: {ex.StackTrace}")
        .ResetColor();
    }
  }
}
