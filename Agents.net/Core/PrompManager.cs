using System;
using System.Collections.Generic;
using System.Text;

namespace Arcana.AgentsNet.Core
{
  /// <summary>
  /// Gerencia os prompts do agente
  /// </summary>
  public class PromptManager<TContext>
  {
    private readonly List<Func<TContext, string>> _personaGenerators = new List<Func<TContext, string>>();
    private readonly List<Func<TContext, string>> _instructionGenerators = new List<Func<TContext, string>>();
    private readonly List<Func<TContext, string>> _guardRailGenerators = new List<Func<TContext, string>>();
    private readonly List<Func<TContext, string>> _systemPromptGenerators = new List<Func<TContext, string>>();

    public void AddPersona(Func<TContext, string> generator)
    {
      _personaGenerators.Add(generator);
    }

    public void AddInstructions(Func<TContext, string> generator)
    {
      _instructionGenerators.Add(generator);
    }

    public void AddGuardRails(Func<TContext, string> generator)
    {
      _guardRailGenerators.Add(generator);
    }

    public void AddSystemPrompt(Func<TContext, string> generator)
    {
      _systemPromptGenerators.Add(generator);
    }

    public string BuildSystemPrompt(TContext context)
    {
      var sb = new StringBuilder();

      // Instruções
      foreach (var generator in _instructionGenerators)
      {
        AppendGeneratedText(sb, generator, context);
      }

      // Persona
      foreach (var generator in _personaGenerators)
      {
        AppendGeneratedText(sb, generator, context);
      }

      // GuardRails
      foreach (var generator in _guardRailGenerators)
      {
        AppendGeneratedText(sb, generator, context);
      }

      // System Prompts
      foreach (var generator in _systemPromptGenerators)
      {
        AppendGeneratedText(sb, generator, context);
      }

      return sb.ToString().Trim();
    }

    private void AppendGeneratedText(StringBuilder sb, Func<TContext, string> generator, TContext context)
    {
      try
      {
        string text = generator(context);
        if (!string.IsNullOrWhiteSpace(text))
        {
          sb.AppendLine(text);
        }
      }
      catch (Exception)
      {
        // Ignora erros de geração
      }
    }
  }
}
