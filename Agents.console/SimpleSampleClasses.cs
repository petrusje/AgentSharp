using Agents.net.Attributes;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;
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

  public class ClimaToolPack : Toolkit
  {
    public ClimaToolPack(
        string name = "clima",
        bool cacheResults = true,
        int cacheTtl = 300)
        : base(
            name,
            "Pacote de ferramentas para consulta de clima",
            addInstructions: true,
            cacheResults: cacheResults,
            cacheTtl: cacheTtl)
    {
    }

    [FunctionCall("Consulta o clima atual em uma cidade brasileira")]
    [FunctionCallParameter("cidade", "Cidade brasileira para consultar o clima")]
    private string ConsultarClima(string cidade)
    {
      return $"Clima em {cidade}";
    }
  }

  public class InheritedAgent : Agent<MyCtx, string>
  {
    public InheritedAgent(IModel model) : base(model, "InheritedAgent")
    {
    }
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