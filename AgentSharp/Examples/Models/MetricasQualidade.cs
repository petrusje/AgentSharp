namespace AgentSharp.Examples.Models
{
  public class MetricasQualidade
  {
    public int BugsAbertos { get; set; }
    public int BugsFechados { get; set; }
    public double CoberturaTestes { get; set; } // %
    public int DeployFrequency { get; set; } // deploys por mês
  }
}
