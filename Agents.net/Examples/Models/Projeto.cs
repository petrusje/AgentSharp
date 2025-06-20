using System;

namespace Arcana.AgentsNet.Examples.Models
{
  public class Projeto
  {
    public string Id { get; set; }
    public string Nome { get; set; }
    public string Status { get; set; }
    public DateTime DataInicio { get; set; }
    public int TamanhoEquipe { get; set; }
    public string[] Tecnologias { get; set; }
    public MetricasVelocidade MetricasVelocidade { get; set; }
    public MetricasQualidade MetricasQualidade { get; set; }
  }
}
