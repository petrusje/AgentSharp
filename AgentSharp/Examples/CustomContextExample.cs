using AgentSharp.Attributes;
using AgentSharp.Core;
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
    /// Demonstration of using the agent with custom context.
    /// </summary>
    public static async Task RunExample()
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

      // Create the model via factory
      var options = new ModelOptions
      {
        ModelName = "gpt-4o-mini"
      };
      var model = new ModelFactory().CreateModel("openai", options);

      // Create the agent with custom context
      var config = new ModelConfiguration
      {
        Temperature = 0.3,
        MaxTokens = 1000
      };

      var agent = new AdvancedAgent(model)
          .WithContext(context) // Use the fluent method to set the context
          .WithConfig(config);

      // Execute the agent
      var result = await agent.ExecuteAsync(
          "What is my preferred theme and what can you tell me about my interactions?");

      // Access the result
      Console.WriteLine($"Response: {result.Value}");

      // Execute with streaming
      await agent.ExecuteStreamingAsync(
          "Add a new notification preference as 'email'",
          chunk => Console.Write(chunk),
          context);

      // View the updated history
      Console.WriteLine("\nInteraction history:");
      foreach (var item in context.InteractionHistory)
      {
        Console.WriteLine($"- {item}");
      }
    }
  }
}
