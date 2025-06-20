using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Models;
using System;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Examples
{
  /// <summary>
  /// Teste simples do sistema de reasoning para verificar seu funcionamento
  /// </summary>
  public static class TesteReasoning
  {
    public static async Task TestarReasoningEstruturado(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🧠 TESTE: REASONING ESTRUTURADO");
      Console.WriteLine("═══════════════════════════════════════════════════");
      Console.ResetColor();

      // Agente com reasoning habilitado
      var agentReasoning = new Agent<object, string>(modelo, "TestAgent")
          .WithReasoning(true)
          .WithReasoningSteps(3, 5)
          .WithInstructions("Você é um especialista em resolução de problemas.");

      var problema = "Como posso aumentar as vendas de uma loja online em 30%?";

      Console.WriteLine($"📝 Problema: {problema}");
      Console.WriteLine("\n🔄 Executando com reasoning...\n");

      try
      {
        var resultado = await agentReasoning.ExecuteAsync(problema);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ RESULTADO PRINCIPAL:");
        Console.ResetColor();
        Console.WriteLine(resultado.Data);

        // Verificar reasoning content
        if (!string.IsNullOrEmpty(resultado.ReasoningContent))
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine("\n🧠 REASONING CONTENT (FORMATADO):");
          Console.WriteLine(new string('=', 50));
          Console.WriteLine(resultado.ReasoningContent);
          Console.ResetColor();
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("\n❌ Reasoning content está vazio!");
          Console.ResetColor();
        }

        // Verificar reasoning steps estruturados
        if (resultado.ReasoningSteps != null && resultado.ReasoningSteps.Count > 0)
        {
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.WriteLine("\n📊 REASONING STEPS ESTRUTURADOS:");
          Console.WriteLine(new string('=', 50));

          for (int i = 0; i < resultado.ReasoningSteps.Count; i++)
          {
            var step = resultado.ReasoningSteps[i];
            Console.WriteLine($"Step {i + 1}: {step.Title}");
            Console.WriteLine($"  Action: {step.Action}");
            Console.WriteLine($"  Reasoning: {step.Reasoning}");
            Console.WriteLine($"  Result: {step.Result}");
            if (step.Confidence.HasValue)
              Console.WriteLine($"  Confidence: {step.Confidence.Value:P0}");
            Console.WriteLine();
          }
          Console.ResetColor();
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("\n❌ Reasoning steps estruturados estão vazios!");
          Console.ResetColor();
        }

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🧠 Reasoning Steps: {resultado.ReasoningSteps.Count}");
        Console.WriteLine($"✅ Teste concluído");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro durante teste: {ex.Message}");
        Console.ResetColor();
      }
    }
  }
}
