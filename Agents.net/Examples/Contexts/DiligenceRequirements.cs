namespace Arcana.AgentsNet.Examples.Contexts
{
  public class DiligenceRequirements
  {
    public bool AnaliseMercado { get; set; } = true;
    public bool AvaliacaoFinanceira { get; set; } = true;
    public bool EstudoViabilidade { get; set; } = true;
    public bool PlanejamentoOperacional { get; set; } = false;
    public bool GestaoRiscos { get; set; } = false;
    public bool ComplianceRegulatorio { get; set; } = false;
  }
}
