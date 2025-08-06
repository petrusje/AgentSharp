namespace AgentSharp.Examples.Contexts
{
  public class CenarioAnalise
  {
    public string TipoAnalise { get; set; } = "Análise Geral";
    public string[] MercadosAlvo { get; set; } = new[] { "Brasil" };
    public int HorizonteTemporal { get; set; } = 12;
    public decimal InvestimentoDisponivel { get; set; } = 1_000_000;
    public string RiscoAceitavel { get; set; } = "Médio";
  }
}
