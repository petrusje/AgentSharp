using System;

namespace Arcana.AgentsNet.Examples.Contexts
{
  public class CreativeBrief
  {
    public string Cliente { get; set; } = "Cliente";
    public string Produto { get; set; } = "App";
    public string Publico { get; set; } = "Geral";
    public string Vibe { get; set; } = "Moderno";
    public string[] Plataformas { get; set; } = new[] { "Web" };
    public TimeSpan Timeline { get; set; } = TimeSpan.FromDays(30);
    public string Budget { get; set; } = "R$ 50.000";
  }
}
