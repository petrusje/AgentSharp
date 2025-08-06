namespace AgentSharp.Examples.Models
{
  public class EquipeProjeto
  {
    public string ProjetoId { get; set; }
    public string TechLead { get; set; }
    public string[] Desenvolvedores { get; set; }
    public string QA { get; set; }
    public double SatisfacaoEquipe { get; set; } // 0-10
    public int RotatividadeUltimos6Meses { get; set; }
  }
}
