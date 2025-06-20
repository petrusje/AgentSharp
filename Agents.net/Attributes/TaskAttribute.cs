using System;

namespace Arcana.AgentsNet.Attributes
{
  /// <summary>
  /// Attribute for marking a class as a task to be performed by an agent.
  /// </summary>
  /// <remarks>
  /// Tasks represent discrete work items that can be assigned to specific agents.
  /// The framework can use this metadata to schedule and coordinate work.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public class TaskAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the description of what this task does.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the name of the agent assigned to perform this task.
    /// </summary>
    public string Agent { get; set; }
  }
}
