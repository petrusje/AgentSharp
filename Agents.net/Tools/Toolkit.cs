using Arcana.AgentsNet.Exceptions;
using Arcana.AgentsNet.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Tools
{
  /// <summary>
  /// Toolkit class that extends ToolPack and provides functionality for managing tools.
  /// Provides a way to organize related tools with additional features like instructions and filtering.
  /// </summary>
  public class Toolkit : ToolPack
  {
    private readonly Dictionary<string, Tool> _tools = new Dictionary<string, Tool>();
    private readonly bool _cacheResults;
    private readonly int _cacheTtl;


    /// <summary>
    /// Initialize a new toolkit
    /// </summary>
    /// <param name="name">Name of the toolkit</param>
    /// <param name="instructions">Optional instructions for the toolkit</param>
    /// <param name="addInstructions">Whether to add instructions to the system prompt</param>
    /// <param name="cacheResults">Whether to cache tool results</param>
    /// <param name="cacheTtl">Cache time-to-live in seconds (default: 3600)</param>
    public Toolkit(
        string name,
        string instructions = null,
        bool addInstructions = false,
        bool cacheResults = false,
        int cacheTtl = 3600)

    {
      Name = name;
      Instructions = instructions;
      AddInstructions = addInstructions;
      _cacheResults = cacheResults;
      _cacheTtl = cacheTtl;

      // Set description in the base ToolPack class
      Description = instructions ?? $"Toolkit: {name}";
      Version = "1.0.0";
    }

    /// <summary>
    /// Instructions for using the toolkit
    /// </summary>
    public string Instructions { get; }

    /// <summary>
    /// Whether instructions should be added to the system prompt
    /// </summary>
    public bool AddInstructions { get; }

    /// <summary>
    /// List of available tools in this toolkit
    /// </summary>
    public IReadOnlyDictionary<string, Tool> Tools => new ReadOnlyDictionary<string, Tool>(_tools);

    /// <summary>
    /// Registers a tool in the toolkit
    /// </summary>
    public void Register(Tool tool)
    {
      if (tool == null)
      {
        throw new ArgumentNullException(nameof(tool));
      }

      // Apply cache configuration
      if (_cacheResults && tool is AsyncTool asyncTool)
      {
        asyncTool.EnableCache(_cacheTtl);
      }

      _tools[tool.Name] = tool;
      Logger.Debug($"Tool {tool.Name} registered with {Name} toolkit");
    }

    /// <summary>
    /// Add a tool to the toolkit (alias for Register to maintain ToolPack API)
    /// </summary>
    public void AddTool(Tool tool)
    {
      Register(tool);
    }

    /// <summary>
    /// Executes a tool from the toolkit
    /// </summary>
    public virtual async Task<string> ExecuteAsync(string toolName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrEmpty(toolName))
      {
        throw new ArgumentNullException(nameof(toolName));
      }

      if (!_tools.TryGetValue(toolName, out var tool))
      {
        throw new ToolExecutionException(toolName, $"Tool '{toolName}' not found in toolkit '{Name}'");
      }

      try
      {
        // Use the IsAsync property to determine how to execute the tool
        if (tool.IsAsync)
        {
          return await tool.ExecuteAsync(parameters, cancellationToken);
        }
        else
        {
          return tool.Execute(parameters)?.ToString() ?? string.Empty;
        }
      }
      catch (Exception ex)
      {
        throw new ToolExecutionException(toolName, $"Error executing tool '{toolName}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Gets a tool by name
    /// </summary>
    /// <param name="name">The name of the tool to get</param>
    /// <returns>The tool with the specified name</returns>
    public Tool GetTool(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      if (_tools.TryGetValue(name, out var tool))
      {
        return tool;
      }

      throw new KeyNotFoundException($"Tool with name '{name}' not found in toolkit '{Name}'");
    }
  }
}
