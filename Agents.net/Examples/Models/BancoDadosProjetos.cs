using System.Collections.Generic;

namespace Arcana.AgentsNet.Examples.Models
{
  public class BancoDadosProjetos
  {
    public Dictionary<string, Projeto> Projetos { get; set; } = new Dictionary<string, Projeto>();
    public Dictionary<string, EquipeProjeto> Equipes { get; set; } = new Dictionary<string, EquipeProjeto>();
    public Dictionary<string, List<EventoDeploy>> HistoricoDeploysDefeitos { get; set; } = new Dictionary<string, List<EventoDeploy>>();
  }
}
