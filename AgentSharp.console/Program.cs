using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using AgentSharp.Examples;
using AgentSharp.Models;
using AgentSharp.Utils;
using AgentSharp.console.Services;

namespace Agents_console
{
    /// <summary>
    /// AgentSharp Console Application - Educational Examples
    /// 
    /// This application provides a structured learning path for AgentSharp framework,
    /// progressing from basic concepts to advanced use cases.
    /// 
    /// Features:
    /// - Multilingual support (en-US, pt-BR)
    /// - Configurable telemetry for performance monitoring
    /// - Clean, didactic menu structure
    /// - Progressive learning path from foundations to advanced concepts
    /// </summary>
    class Program
    {
        private static readonly ConsoleObj _consoleObj = new();
        private static LocalizationService _localization = new();
        private static TelemetryService _telemetry;

        static async Task Main(string[] args)
        {
            try
            {
                // Initialize environment and services
                await InitializeApplication();

                // Configure user preferences
                ConfigureUserPreferences();

                // Validate API configuration
                var (apiKey, endpoint) = GetApiKeyAndEndpoint(args);
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    DisplayApiKeyError();
                    return;
                }

                Console.WriteLine(_localization.GetString("ApiKeyFound", endpoint));

                // Initialize AI model
                var modelo = InitializeModel(apiKey, endpoint);

                // Configure global telemetry for all agents and models
                AgentSharp.Core.Agent<object, object>.ConfigureGlobalTelemetry(_telemetry);
                AgentSharp.Models.OpenAIModel.ConfigureGlobalTelemetry(_telemetry);

                // Start main application loop
                await RunMainLoop(modelo);
            }
            catch (Exception ex)
            {
                DisplayFatalError(ex);
            }
        }

        /// <summary>
        /// Initializes the application environment and loads configuration
        /// </summary>
        private static async Task InitializeApplication()
        {
            // Load environment variables
            Env.TraversePath().Load();
            
            // Set console encoding for proper emoji/unicode display
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Display welcome header
            DisplayWelcomeHeader();
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Configures user preferences for language and telemetry
        /// </summary>
        private static void ConfigureUserPreferences()
        {
            // Language selection
            _localization.PromptForLanguageSelection();
            
            // Initialize telemetry with localization support
            _telemetry = new TelemetryService(_localization);
            
            // Configure telemetry
            _telemetry.PromptForTelemetryConfiguration();
        }

        /// <summary>
        /// Displays the application welcome header
        /// </summary>
        private static void DisplayWelcomeHeader()
        {
            _consoleObj.WithColor(ConsoleColor.Cyan)
                .WriteLine("╔══════════════════════════════════════════════════════════════╗")
                .WriteLine("║              🤖 AgentSharp - PRACTICAL EXAMPLES              ║")
                .WriteLine("║                Interactive Learning System                    ║")
                .WriteLine("╚══════════════════════════════════════════════════════════════╝")
                .ResetColor();
        }

        /// <summary>
        /// Retrieves API key and endpoint from arguments or environment variables
        /// </summary>
        private static (string apiKey, string endpoint) GetApiKeyAndEndpoint(string[] args)
        {
            const string OPENAI_API_KEY = "OPENAI_API_KEY";
            const string OPENAI_ENDPOINT = "OPENAI_ENDPOINT";
            const char VALUE_DELIMITER = '=';

            // Check command line arguments first
            var argKey = args.FirstOrDefault(x => x.StartsWith($"{OPENAI_API_KEY}{VALUE_DELIMITER}", StringComparison.OrdinalIgnoreCase));
            var argEndpoint = args.FirstOrDefault(x => x.StartsWith($"{OPENAI_ENDPOINT}{VALUE_DELIMITER}", StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrWhiteSpace(argKey) && !string.IsNullOrWhiteSpace(argEndpoint))
            {
                var keyValue = argKey.Split(VALUE_DELIMITER).LastOrDefault();
                var endpointValue = argEndpoint.Split(VALUE_DELIMITER).LastOrDefault();
                if (!string.IsNullOrWhiteSpace(keyValue) && !string.IsNullOrWhiteSpace(endpointValue))
                    return (keyValue, endpointValue);
            }

            // Fall back to environment variables
            var apiKey = Environment.GetEnvironmentVariable(OPENAI_API_KEY);
            var endpoint = Environment.GetEnvironmentVariable(OPENAI_ENDPOINT) ?? "https://proxy.dta.totvs.ai/";
            return (apiKey, endpoint);
        }

        /// <summary>
        /// Displays API key configuration error with instructions
        /// </summary>
        private static void DisplayApiKeyError()
        {
            _consoleObj.WithColor(ConsoleColor.Red)
                .WriteLine(_localization.GetString("ApiKeyError"))
                .WriteLine(_localization.GetString("ApiKeyInstructions1"))
                .WriteLine(_localization.GetString("ApiKeyInstructions2"))
                .WriteLine(_localization.GetString("ApiKeyInstructions3"))
                .WriteLine(_localization.GetString("EnvFileLocation", System.IO.Directory.GetCurrentDirectory()))
                .ResetColor();
        }

        /// <summary>
        /// Initializes the AI model with configuration
        /// </summary>
        private static IModel InitializeModel(string apiKey, string endpoint)
        {
            var modelName = Environment.GetEnvironmentVariable("MODEL_NAME") ?? "gpt-4o-mini";
            var temperature = double.TryParse(Environment.GetEnvironmentVariable("TEMPERATURE"), out var temp) ? temp : 0.7;
            var maxTokens = int.TryParse(Environment.GetEnvironmentVariable("MAX_TOKENS"), out var tokens) ? tokens : 2048;

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

            var modelFactory = new ModelFactory();
            IModel modelo = modelFactory.CreateModel("openai", modelOptions);
            
            _consoleObj.WithColor(ConsoleColor.Green)
                .WriteLine(_localization.GetString("ModelInitialized"))
                .WriteLine(_localization.GetString("ModelDetails", modelOptions.ModelName, 
                    modelOptions.DefaultConfiguration.Temperature, 
                    modelOptions.DefaultConfiguration.MaxTokens))
                .ResetColor();
                
            return modelo;
        }

        /// <summary>
        /// Runs the main application loop with menu interaction
        /// </summary>
        private static async Task RunMainLoop(IModel modelo)
        {
            while (true)
            {
                DisplayMainMenu();

                var userChoice = Console.ReadLine();
                Console.WriteLine();

                if (!await ProcessUserChoice(userChoice, modelo))
                {
                    break;
                }

                Console.WriteLine();
                Console.WriteLine(_localization.GetString("ContinuePrompt"));
                Console.ReadKey(true);
                Console.Clear();
            }
        }

        /// <summary>
        /// Displays the main menu with categorized options
        /// </summary>
        private static void DisplayMainMenu()
        {
            Console.WriteLine(_localization.GetString("MenuTitle"));
            Console.WriteLine();
            
            // LEVEL 1: FOUNDATIONS
            _consoleObj.WithColor(ConsoleColor.Green);
            Console.WriteLine(_localization.GetString("MenuFoundations"));
            _consoleObj.ResetColor();
            Console.WriteLine($"  {_localization.GetString("MenuOption1")}");
            Console.WriteLine($"  {_localization.GetString("MenuOption2")}");
            Console.WriteLine($"  {_localization.GetString("MenuOption3")}");
            Console.WriteLine();
            
            // LEVEL 2: INTERMEDIATE
            _consoleObj.WithColor(ConsoleColor.Yellow);
            Console.WriteLine(_localization.GetString("MenuIntermediate"));
            _consoleObj.ResetColor();
            Console.WriteLine($"  {_localization.GetString("MenuOption4")}");
            Console.WriteLine($"  {_localization.GetString("MenuOption5")}");
            Console.WriteLine($"  {_localization.GetString("MenuOption6")}");
            Console.WriteLine();
            
            // LEVEL 3: ADVANCED
            _consoleObj.WithColor(ConsoleColor.Cyan);
            Console.WriteLine(_localization.GetString("MenuAdvanced"));
            _consoleObj.ResetColor();
            Console.WriteLine($"  {_localization.GetString("MenuOption7")}");
            Console.WriteLine($"  {_localization.GetString("MenuOption8")}");
            Console.WriteLine();
            
            Console.WriteLine($"  {_localization.GetString("MenuOptionExit")}");
            Console.WriteLine();
            Console.Write(_localization.GetString("MenuPrompt"));
        }

        /// <summary>
        /// Processes user menu choice and executes corresponding example
        /// </summary>
        private static async Task<bool> ProcessUserChoice(string choice, IModel modelo)
        {
            try
            {
                switch (choice)
                {
                    // LEVEL 1: FOUNDATIONS
                    case "1":
                        await ExecuteExample(_localization.GetString("ExampleSimpleAgent"), 
                            () => ExemplosBasicos.ExecutarAgenteSimples(modelo));
                        break;
                    case "2":
                        await ExecuteExample(_localization.GetString("ExamplePersonalityAgent"), 
                            () => ExemplosBasicos.ExecutarJornalistaMineiro(modelo));
                        break;
                    case "3":
                        await ExecuteExample(_localization.GetString("ExampleToolsAgent"), 
                            () => ExemplosBasicos.ExecutarReporterComFerramentas(modelo));
                        break;
                        
                    // LEVEL 2: INTERMEDIATE
                    case "4":
                        await ExecuteExample(_localization.GetString("ExampleReasoningAgent"), 
                            () => ExemplosRaciocinio.ExecutarResolvedorProblemas(modelo));
                        break;
                    case "5":
                        await ExecuteExample(_localization.GetString("ExampleStructuredOutput"), 
                            () => ExemplosStructured.ExecutarAnaliseDocumento(modelo));
                        break;
                    case "6":
                        await ExecuteExample(_localization.GetString("ExampleMemoryAgent"), 
                            () => ExemplosMemoria.ExecutarAssistentePessoal(modelo));
                        break;
                        
                    // LEVEL 3: ADVANCED
                    case "7":
                        await ExecuteExample(_localization.GetString("ExampleWorkflows"), 
                            () => ExemplosWorkflow.ExecutarWorkflowCompleto(modelo));
                        break;
                    case "8":
                        await ExecuteExample(_localization.GetString("ExampleSemanticSearch"), 
                            () => VectorMemoryExample.ExecutarAssistenteComEmbeddings(modelo));
                        break;
                        
                    case "0":
                        Console.WriteLine(_localization.GetString("Goodbye"));
                        return false;
                        
                    default:
                        Console.WriteLine(_localization.GetString("InvalidOption"));
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(_localization.GetString("ExecutionError", ex.Message));
                Console.WriteLine(_localization.GetString("ErrorDetails", ex));
            }

            return true; // Continue in loop
        }

        /// <summary>
        /// Executes an example with telemetry tracking and error handling
        /// </summary>
        private static async Task ExecuteExample(string exampleName, Func<Task> example)
        {
            Console.WriteLine(_localization.GetString("Executing", exampleName));
            _consoleObj.WriteSeparator();
            Console.WriteLine();

            // Start telemetry tracking
            var operationId = $"example_{exampleName}";
            _telemetry.StartOperation(operationId);

            try
            {
                // Execute the example
                await example();
            }
            catch (Exception ex)
            {
                Console.WriteLine(_localization.GetString("ExecutionError", ex.Message));
                if (_telemetry.IsEnabled)
                {
                    Console.WriteLine(_localization.GetString("ErrorDetails", ex));
                }
            }
            finally
            {
                // Complete telemetry tracking
                var elapsed = _telemetry.EndOperation(operationId);
                
                Console.WriteLine();
                _consoleObj.WriteSeparator();
                Console.WriteLine(_localization.GetString("ExecutionTime", elapsed));
                
                // Display telemetry summary if enabled
                if (_telemetry.IsEnabled)
                {
                    DisplayTelemetrySummary();
                }
            }
        }

        /// <summary>
        /// Displays telemetry summary with detailed metrics
        /// </summary>
        private static void DisplayTelemetrySummary()
        {
            var summary = _telemetry.GetSummary();
            if (summary.TotalEvents == 0)
                return;

            Console.WriteLine();
            _consoleObj.WithColor(ConsoleColor.Magenta);
            Console.WriteLine("📊 TELEMETRIA DETALHADA");
            Console.WriteLine("═══════════════════════");
            _consoleObj.ResetColor();

            if (summary.LLMEvents > 0)
            {
                Console.WriteLine($"🤖 LLM: {summary.LLMEvents} chamadas | {summary.TotalLLMTime:F2}s | {summary.TotalTokens} tokens");
            }
            
            if (summary.MemoryEvents > 0)
            {
                Console.WriteLine($"💾 Memória: {summary.MemoryEvents} operações | {summary.TotalMemoryTime:F2}s total");
            }
            
            if (summary.ToolEvents > 0)
            {
                Console.WriteLine($"🔧 Tools: {summary.ToolEvents} execuções | {summary.TotalToolTime:F2}s total");
            }

            Console.WriteLine($"📈 Total: {summary.TotalEvents} eventos | {summary.TotalElapsedSeconds:F2}s | Média: {summary.AverageElapsedSeconds:F2}s");
            
            if (summary.TotalTokens > 0)
            {
                Console.WriteLine($"🎯 Tokens totais: {summary.TotalTokens} | Custo em tokens: {summary.TotalCostInTokens:F0}");
            }
            
            // Clear telemetry for next example
            _telemetry.Clear();
        }

        /// <summary>
        /// Displays fatal error information
        /// </summary>
        private static void DisplayFatalError(Exception ex)
        {
            _consoleObj.WithColor(ConsoleColor.Red)
                .WriteLine(_localization.GetString("FatalError", ex.Message))
                .WriteLine($"Stack trace: {ex.StackTrace}")
                .ResetColor();
        }
    }
}