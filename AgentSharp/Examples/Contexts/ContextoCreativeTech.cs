namespace AgentSharp.Examples.Contexts
{
  public class ContextoCreativeTech
  {
    public CreativeBrief BriefProjeto { get; set; } = new CreativeBrief();
    public DesignPreferences EstiloPreferencias { get; set; } = new DesignPreferences();
    public string[] TechStack { get; set; } = new[] { "React", "Node.js" };
    public string[] Deliverables { get; set; } = new[] { "Wireframe", "Protótipo" };
  }
}
