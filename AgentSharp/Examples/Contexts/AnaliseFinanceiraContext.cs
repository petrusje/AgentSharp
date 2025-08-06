using System;

namespace AgentSharp.Examples.Contexts
{
  public class AnaliseFinanceiraContext
  {
    public string FocoMercado { get; set; } = "Minas Gerais";
    public string TipoAnalise { get; set; } = "Investimento";
    public string PeriodoAnalise { get; set; } = "Últimos 30 dias";
    public DateTime DataAnalise { get; set; } = DateTime.Now;
  }
}
