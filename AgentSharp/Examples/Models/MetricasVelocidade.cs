namespace AgentSharp.Examples.Models
{
  public class MetricasVelocidade
  {
    public double VelocidadeMedia { get; set; } // story points por sprint
    public double BurndownRate { get; set; } // 0-1
    public int TasksCompletadas { get; set; }
    public int TasksPendentes { get; set; }
  }
}
