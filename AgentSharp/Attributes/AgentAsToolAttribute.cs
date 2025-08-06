using System;

namespace AgentSharp.Attributes
{
  /// <summary>
  /// Attribute for marking a class as an agent that can be used as a tool by the AI model.
  /// </summary>
  /// <remarks>
  /// This attribute allows the AI model to treat the agent as a tool it can invoke during
  /// response generation. The agent must implement the necessary interfaces to be callable.
  /// The framework will scan for classes with this attribute to automatically register them
  /// as tools.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public class AgentAsToolAttribute : Attribute
  {
    /// <summary>
    /// Nome da ferramenta (opcional - usa nome do agente se não especificado)
    /// </summary>
    public string ToolName { get; set; }

    /// <summary>
    /// Descrição da ferramenta para outros agentes
    /// </summary>
    public string ToolDescription { get; }

    public AgentAsToolAttribute(string toolDescription)
    {
      ToolDescription = toolDescription ?? throw new ArgumentNullException(nameof(toolDescription));
    }
  }
}
