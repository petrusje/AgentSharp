using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Models;
using Arcana.AgentsNet.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agents_console
{
  public class MyCtx
  {
    public string UserName { get; set; }
    public string UserLanguage { get; set; }
    public Dictionary<string, string> Database { get; set; } = new Dictionary<string, string>();
  }

  public class ClimaToolPack(
      string name = "clima",
      bool cacheResults = true,
      int cacheTtl = 300) : Toolkit(
          name,
          "Pacote de ferramentas para consulta de clima",
          addInstructions: true,
          cacheResults: cacheResults,
          cacheTtl: cacheTtl)
  {
    [FunctionCall("Consulta o clima atual em uma cidade brasileira")]
    [FunctionCallParameter("cidade", "Cidade brasileira para consultar o clima")]
    private static string ConsultarClima(string cidade)
    {
      return $"Clima em {cidade}";
    }
  }

  public class InheritedAgent(IModel model) : Agent<MyCtx, string>(model, "InheritedAgent")
  {
  }

  public static class RetryHelper
  {
    public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
      for (int i = 0; i < maxRetries; i++)
      {
        try
        {
          return await operation();
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
          Console.WriteLine($"Tentativa {i + 1} falhou: {ex.Message}. Tentando novamente...");
          // Delay removido para evitar atrasos desnecessários
        }
      }

      // Última tentativa sem catch
      return await operation();
    }
  }
}
