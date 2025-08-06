using System.Collections.Generic;

namespace AgentSharp.Examples.StructuredOutputs
{
  public class RelatorioAnalise
  {
    public string ProjetoId { get; set; }
    public List<string> GargalosIdentificados { get; set; } = new List<string>();
    public List<string> Recomendacoes { get; set; } = new List<string>();
    public string PrioridadeAcoes { get; set; }
    public Dictionary<string, double> MetricasChave { get; set; } = new Dictionary<string, double>();

    public override string ToString()
    {
      return $"Relatório de Análise para Projeto {ProjetoId}\n" +
             $"Gargalos Identificados: {GargalosIdentificados.Count}\n" +
             $"Recomendações: {Recomendacoes.Count}";
    }
  }
}
