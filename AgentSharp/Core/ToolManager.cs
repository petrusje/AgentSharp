using AgentSharp.Attributes;
using AgentSharp.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AgentSharp.Core
{
  /// <summary>
  /// Gerencia as ferramentas do agente
  /// </summary>
  public class ToolManager
  {
    private readonly Dictionary<string, Tool> _tools = new Dictionary<string, Tool>();
    private readonly List<ToolPack> _toolPacks = new List<ToolPack>();
    private IAgentCtxChannel _agentContext;

    public void RegisterTool(Tool tool)
    {
      _tools[tool.Name] = tool;
    }

    public void RegisterToolPack(ToolPack toolPack)
    {
      _toolPacks.Add(toolPack);

      // Registrar o contexto do agente no ToolPack se disponível
      if (_agentContext != null)
      {
        toolPack.RegisterContext(_agentContext);
        _ = toolPack.InitializeAsync(); // Inicializar o ToolPack assincronamente
      }

      if(toolPack is Toolkit)
      {
        // Registra as ferramentas do kit
        foreach (var tool in (toolPack as Toolkit).Tools)
        {
          RegisterTool(tool.Value);
        }
      }

      // Registra as ferramentas do tool pack
      foreach (var method in toolPack.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          .Where(m => m.GetCustomAttribute<FunctionCallAttribute>() != null))
      {
        var functionCall = method.GetCustomAttribute<FunctionCallAttribute>();
        if (functionCall != null)
        {
          var tool = Tool.FromMethod(method, toolPack, functionCall);
          RegisterTool(tool);
        }
      }
    }

    public void RegisterAgentMethods(IAgent agent)
    {
      // Armazenar referência do agente como contexto se ele implementar IAgentCtxChannel
      if (agent is IAgentCtxChannel agentContext)
      {
        _agentContext = agentContext;
        
        // Registrar contexto em todos os ToolPacks já registrados
        foreach (var toolPack in _toolPacks)
        {
          toolPack.RegisterContext(_agentContext);
          _ = toolPack.InitializeAsync();
        }
      }

      // Registra os métodos do agente como ferramentas
      foreach (var method in agent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          .Where(m => m.GetCustomAttribute<FunctionCallAttribute>() != null))
      {
        var functionCall = method.GetCustomAttribute<FunctionCallAttribute>();
        if (functionCall != null)
        {
          var tool = Tool.FromMethod(method, agent, functionCall);
          RegisterTool(tool);
        }
      }
    }

    public List<Tool> GetTools()
    {
      return _tools.Values.ToList();
    }
  }

}
