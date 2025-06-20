using Arcana.AgentsNet.Examples;
using Arcana.AgentsNet.Models;
using Arcana.AgentsNet.Utils;
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
        .WriteLine("║              🤖 AGENTS.NET - EXEMPLOS PRÁTICOS               ║")
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
          case "1":
            await ExecuteExample("Jornalista com Personalidade", () => ExemplosBasicos.ExecutarJornalistaMineiro(modelo));
            break;
          case "2":
            await ExecuteExample("Jornalista com Busca Web", () => ExemplosBasicos.ExecutarReporterComFerramentas(modelo));
            break;
          case "3":
            await ExecuteExample("Analista Financeiro", () => ExemplosBasicos.ExecutarAnalistaFinanceiroRealData(modelo));
            break;
          case "4":
            await ExecuteExample("Resolvedor de Problemas", () => ExemplosRaciocinio.ExecutarResolvedorProblemas(modelo));
            break;
          case "5":
            await ExecuteExample("Avaliador de Soluções", () => ExemplosRaciocinio.ExecutarAvaliadorSolucoes(modelo));
            break;
          case "6":
            await ExecuteExample("Identificador de Obstáculos", () => ExemplosRaciocinio.ExecutarIdentificadorObstaculos(modelo));
            break;
          case "7":
            await ExecuteExample("Análise de Documentos Empresariais", () => ExemplosStructured.ExecutarAnaliseDocumento(modelo));
            break;
          case "8":
            await ExecuteExample("Análise de Currículos", () => ExemplosStructured.ExecutarAnaliseCurriculo(modelo));
            break;
          case "9":
            await ExecuteExample("Workflow Multi-etapa", () => ExemplosWorkflow.ExecutarWorkflowCompleto(modelo));
            break;
          case "0":
            Console.WriteLine("👋 Obrigado por usar Agents.net!");
            return false; // Sair do loop
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

      return true; // Continuar no loop
    }

    private static void DisplayMenuOptions()
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
