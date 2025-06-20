using System;

namespace Arcana.AgentsNet.Attributes
{
  /// <summary>
  /// Attribute for marking a class as a team of agents that work together.
  /// </summary>
  /// <remarks>
  /// Teams provide a way to group related agents that collaborate on complex tasks.
  /// The framework can use this metadata for orchestration and management.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public class TeamAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the name of the team.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a detailed description of the team's purpose and capabilities.
    /// </summary>
    public string Description { get; set; }
  }
}
