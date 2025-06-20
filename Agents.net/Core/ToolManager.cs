using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arcana.AgentsNet.Core
{
  /// <summary>
  /// Gerencia as ferramentas do agente
  /// </summary>
  public class ToolManager
  {
    private readonly Dictionary<string, Tool> _tools = new Dictionary<string, Tool>();
    private readonly List<ToolPack> _toolPacks = new List<ToolPack>();

    public void RegisterTool(Tool tool)
    {
      _tools[tool.Name] = tool;
    }

    public void RegisterToolPack(ToolPack toolPack)
    {
      _toolPacks.Add(toolPack);

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
