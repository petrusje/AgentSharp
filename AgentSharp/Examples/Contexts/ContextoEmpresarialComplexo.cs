namespace AgentSharp.Examples.Contexts
{
  public class ContextoEmpresarialComplexo
  {
    public EmpresaInfo EmpresaInfo { get; set; } = new EmpresaInfo();
    public CenarioAnalise CenarioAnalise { get; set; } = new CenarioAnalise();
    public DiligenceRequirements RequisitosDiligencia { get; set; } = new DiligenceRequirements();
  }
}
