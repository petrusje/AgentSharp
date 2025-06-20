using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Arcana.AgentsNet.Core
{
  public static class AgentExtensions
  {
    /// <summary>
    /// Converte um agente em uma Tool usando reflection para injetar atributos
    /// </summary>
    public static Tool AsTool(this IAgent agent)
    {
      if (agent == null)
        throw new ArgumentNullException(nameof(agent));

      var agentType = agent.GetType();
      var toolName = GetToolName(agent, agentType);
      var toolDescription = GetToolDescription(agent, agentType);

      var wrapperMethod = typeof(AgentToolWrapper).GetMethod(nameof(AgentToolWrapper.ExecuteAgent));
      var wrapper = new AgentToolWrapper(agent, toolDescription);

      return new Tool(toolName, toolDescription, wrapperMethod, wrapper, false, ToolKind.Agent);
    }

    public static Tool AsTool(this IAgent agent, string toolName, string toolDescription,
                             bool isFinalTool = false, bool isStreamable = false)
    {
      if (agent == null)
        throw new ArgumentNullException(nameof(agent));
      if (string.IsNullOrEmpty(toolName))
        throw new ArgumentException("Tool name cannot be empty", nameof(toolName));
      if (string.IsNullOrEmpty(toolDescription))
        throw new ArgumentException("Tool description cannot be empty", nameof(toolDescription));

      var wrapper = new AgentToolWrapper(agent, toolDescription);
      var wrapperMethod = typeof(AgentToolWrapper).GetMethod(nameof(AgentToolWrapper.ExecuteAgent));

      return new Tool(toolName, toolDescription, wrapperMethod, wrapper, isStreamable, ToolKind.Agent)
      {
        IsFinalResultTool = isFinalTool
      };
    }

    /// <summary>
    /// Obtém o nome da tool via reflection nos atributos
    /// </summary>
    private static string GetToolName(IAgent agent, Type agentType)
    {
      // 1. Tentar AgentAsToolAttribute.ToolName
      var toolAttr = agentType.GetCustomAttribute<AgentAsToolAttribute>();
      if (!string.IsNullOrEmpty(toolAttr?.ToolName))
        return toolAttr.ToolName;

      // 2. Tentar AgentAttribute.Name  
      var agentAttr = agentType.GetCustomAttribute<AgentAttribute>();
      if (!string.IsNullOrEmpty(agentAttr?.Name))
        return $"call_{SanitizeName(agentAttr.Name)}";

      // 3. Fallback para nome do agente
      return $"call_{SanitizeName(agent.Name)}";
    }

    /// <summary>
    /// Constrói descrição completa via reflection nos atributos
    /// </summary>
    private static string GetToolDescription(IAgent agent, Type agentType)
    {
      var parts = new List<string>();

      // 1. Descrição principal do AgentAsToolAttribute (prioridade)
      var toolAttr = agentType.GetCustomAttribute<AgentAsToolAttribute>();
      if (!string.IsNullOrEmpty(toolAttr?.ToolDescription))
      {
        parts.Add(toolAttr.ToolDescription);
      }

      // 2. Informações do AgentAttribute
      var agentAttr = agentType.GetCustomAttribute<AgentAttribute>();
      if (agentAttr != null)
      {
        if (!string.IsNullOrEmpty(agentAttr.Role))
          parts.Add($"Especialização: {agentAttr.Role}");

        if (!string.IsNullOrEmpty(agentAttr.Description) && toolAttr == null)
          parts.Add(agentAttr.Description);
      }

      // 3. Buscar FunctionCallAttribute se existir (para casos especiais)
      var functionAttr = agentType.GetCustomAttribute<FunctionCallAttribute>();
      if (functionAttr != null && !string.IsNullOrEmpty(functionAttr.Description) && toolAttr == null)
      {
        parts.Add($"Capacidades: {functionAttr.Description}");
      }

      // 4. Fallback para descrição do agente
      if (parts.Count == 0)
      {
        parts.Add($"Executa o agente '{agent.Name}': {agent.description}");
      }

      return string.Join(". ", parts);
    }

    private static string SanitizeName(string name)
    {
      if (string.IsNullOrEmpty(name))
        return "unnamed_agent";

      return name.ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");
    }
  }
}
