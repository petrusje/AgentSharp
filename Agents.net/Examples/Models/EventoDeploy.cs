using System;

namespace Arcana.AgentsNet.Examples.Models
{
  public class EventoDeploy
  {
    public DateTime Data { get; set; }
    public bool Sucesso { get; set; }
    public TimeSpan TempoInatividade { get; set; }
  }
}
