using AgentSharp.Attributes;
using AgentSharp.Core;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/* Unmerged change from project 'AgentSharp (netstandard2.0)'
Added:
using Agents;
using AgentSharp;
using AgentSharp.Examples;
using AgentSharp.Examples;
*/

namespace AgentSharp.Examples
{
  /// <summary>
  /// Example of a custom context class.
  /// </summary>
  public class AgentContextSample
  {
    /// <summary>
    /// Name of the user or client.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// User preferences.
    /// </summary>
    public Dictionary<string, string> UserPreferences { get; set; }

    /// <summary>
    /// History of previous interactions.
    /// </summary>
    public List<string> InteractionHistory { get; set; }

    /// <summary>
    /// Creates a new instance of the context with default values.
    /// </summary>
    public AgentContextSample()
    {
      UserPreferences = new Dictionary<string, string>();
      InteractionHistory = new List<string>();
    }
  }

  /// <summary>
  /// Custom agent that uses a specific context.
  /// </summary>
  public class AdvancedAgent : Agent<AgentContextSample, string>
  {
    /// <summary>
    /// Initializes a new instance of the custom agent.
    /// </summary>
    /// <param name="model">Model to be used.</param>
    public AdvancedAgent(IModel model)
        : base(model, name: "CustomAgent",
               instructions: "You are a personalized assistant that analyzes user preferences.")
    {
      // Register dynamic prompts that access the context
      RegisterSystemPromptFunction(ctx =>
          $"The user's name is {ctx.UserName}. " +
          $"They have {ctx.UserPreferences.Count} preferences registered."
      );

      // Methods with attributes are registered automatically
    }

    /// <summary>
    /// Gets the user preferences.
    /// </summary>
    [FunctionCall("Gets the user preferences")]
    public string GetUserPreferences(string category = null)
    {
      var preferences = Context.UserPreferences;

      if (string.IsNullOrEmpty(category))
        return $"Preferences: {string.Join(", ", preferences)}";

      if (preferences.TryGetValue(category, out var value))
        return $"Preference for {category}: {value}";

      return $"No preference found for {category}";
    }

    /// <summary>
    /// Adds a preference to the user context.
    /// </summary>
    [FunctionCall("Adds a preference to the user profile")]
    public string AddPreference(string category, string value)
    {
      Context.UserPreferences[category] = value;
      Context.InteractionHistory.Add($"Preference added: {category}={value}");
      return $"Preference '{category}' set to '{value}'";
    }

    /// <summary>
    /// Summary of user interactions.
    /// </summary>
    [FunctionCall("Gets a summary of user interactions")]
    public string GetInteractionSummary()
    {
      return $"Summary of {Context.InteractionHistory.Count} interactions:\n" +
             $"{string.Join("\n", Context.InteractionHistory)}";
    }

    /// <summary>
    /// Asynchronous example.
    /// </summary>
    [FunctionCallAsync("Example of an asynchronous operation")]
    public Task<string> ProcessAsync(string input)
    {
      Context.InteractionHistory.Add($"Processed: {input}");
      return Task.FromResult($"Processing of '{input}' completed");
    }
  }

  /// <summary>
  /// Usage example to illustrate the use of custom context.
  /// </summary>
  public static class CustomContextExample
  {
    /// <summary>
    /// Demonstra como DI PURA funciona (vai falhar propositalmente)
    /// </summary>
    private static IModel CreateModelWithDI(string modelName)
    {
      Console.WriteLine("⚠️  DI PURA requer referência a AgentSharp.Providers.OpenAI");
      Console.WriteLine("Tentando criar ModelFactory sem providers...");

      try
      {
        // ISSO VAI FALHAR - e é assim que deve ser!
        var modelFactory = new ModelFactory(new List<IModelProvider>());
        return null; // Não vai chegar aqui
      }
      catch (ArgumentException ex)
      {
        Console.WriteLine($"✅ FALHOU COMO ESPERADO: {ex.Message}");
        Console.WriteLine("\n🎯 ARQUITETURA LIMPA: DI é OBRIGATÓRIO!");
        Console.WriteLine("Para usar em produção:");
        Console.WriteLine("1. Referencie: AgentSharp.Providers.OpenAI");
        Console.WriteLine("2. Configure: var provider = new OpenAIModelProvider(apiKey)");
        Console.WriteLine("3. Injete: var factory = new ModelFactory([provider])");
        return null;
      }
    }

    /// <summary>
    /// Demonstration of using the agent with custom context.
    /// </summary>
    public static void RunExample()
    {
      // Create the custom context
      var context = new AgentContextSample
      {
        UserName = "John Doe",
        UserPreferences = new Dictionary<string, string>
                {
                    { "theme", "dark" },
                    { "language", "english" }
                }
      };

      // DEMONSTRAÇÃO: Como DI pura funciona (vai falhar)
      var model = CreateModelWithDI("demo-model");

      // SEM FALLBACK: DI pura força arquitetura limpa!
      Console.WriteLine("\nEste example NÃO executará o agente - isso é INTENCIONAL!");
      Console.WriteLine("Configure DI corretamente em seu projeto para usar AgentSharp.");
      return; // Sai sem executar o agente
    }
  }

  // *** MOCK PROVIDER REMOVIDO ***
  // Examples agora demonstram APENAS conceitos de DI
  // Para implementação real: referencie AgentSharp.Providers.*
}
