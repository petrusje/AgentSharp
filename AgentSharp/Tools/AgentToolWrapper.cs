using AgentSharp.Attributes;
using AgentSharp.Core;
using AgentSharp.Utils;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tools
{
  public class AgentToolWrapper
  {
    private readonly IAgent _agent;
    private readonly string _toolDescription;

    public AgentToolWrapper(IAgent agent, string toolDescription)
    {
      _agent = agent ?? throw new ArgumentNullException(nameof(agent));
      _toolDescription = toolDescription;
    }

    /// <summary>
    /// Executa agente com PROMPT ORIGINAL + PROMPT ESPECÍFICO
    /// Os atributos FunctionCall são injetados dinamicamente via Tool
    /// </summary>

    [FunctionCallParameter("promptToAgent", "Prompt específico para o agente")]
    [FunctionCallParameter("HistoryContext", "Contexto adicional para o Agente entender sobre a tarefa (opcional)")]
    public string ExecuteAgent(string promptToAgent, string HistoryContext = null)
    {
      if (string.IsNullOrEmpty(promptToAgent))
        throw new ArgumentException("Prompt cannot be empty", nameof(promptToAgent));

      try
      {
        // COMO VOCÊ PEDIU: Prompt original + prompt específico
        var originalPrompt = _agent.GetSystemPrompt(); // Prompt original do agente
        var finalPrompt = BuildCombinedPrompt(originalPrompt, promptToAgent, HistoryContext);

        if (Logger.Instance != null)
          Logger.Debug($"Executando agente {_agent.Name} com prompt combinado");

        // Executar de forma síncrona
        var task = ExecuteAgentAsync(finalPrompt, HistoryContext);
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        var errorMsg = $"Erro ao executar agente {_agent.Name}: {ex.Message}";

        if (Logger.Instance != null)
          Logger.Error($"Erro ao executar agente {_agent.Name} como tool", ex);
        else
          Console.WriteLine($"[ERROR] {errorMsg}");

        return errorMsg;
      }
    }

    /// <summary>
    /// Combina prompt original do agente + tarefa específica + contexto
    /// </summary>
    private string BuildCombinedPrompt(string originalPrompt, string promptToAgent, string context)
    {
      var combinedPrompt = promptToAgent; // Prompt específico da tarefa

      // Adicionar contexto se fornecido
      if (!string.IsNullOrEmpty(context))
      {
        combinedPrompt = $@"CONTEXTO ADICIONAL: {context}

TAREFA: {promptToAgent}";
      }

      // O prompt original já está no sistema prompt do agente
      // Então só passamos a tarefa específica
      return combinedPrompt;
    }

    [FunctionCallParameter("Prompt", "Prompt específico para o agente")]
    [FunctionCallParameter("HistoryContext", "Contexto adicional para o Agente entender sobre a tarefa (opcional)")]
    private async Task<string> ExecuteAgentAsync(string prompt, string HistoryContext = null)
    {
      if (string.IsNullOrEmpty(prompt))
        throw new ArgumentException("Prompt cannot be empty", nameof(prompt));
      string promptWithContext = prompt;
      if (HistoryContext != null)
        promptWithContext = string.Format("Tarefa:{0}\n\nContexto{1}", prompt, HistoryContext);
      string result = (string)await _agent.ExecuteAsync(promptWithContext);
      return result;
    }
  }
}
