using System;

namespace Agents.net.Attributes
{
  /// <summary>
  /// Attribute for marking a class as an agent with specific role and capabilities.
  /// </summary>
  /// <remarks>
  /// Used to declaratively define agent properties without requiring manual configuration.
  /// The framework can scan for classes with this attribute to automatically register agents.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public class AgentAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the name of the agent.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the role or purpose of the agent in the system.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets a detailed description of the agent's capabilities and behavior.
    /// </summary>
    public string Description { get; set; }
  }
}
