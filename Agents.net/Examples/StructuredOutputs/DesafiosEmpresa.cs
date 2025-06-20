using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Examples.StructuredOutputs
{
  public class DesafiosEmpresa
  {
    [JsonPropertyName("mercado")]
    public string Mercado { get; set; }

    [JsonPropertyName("internos")]
    public string Internos { get; set; }

    [JsonPropertyName("principais_obstaculos")]
    public Dictionary<string, string> PrincipaisObstaculos { get; set; }

    [JsonPropertyName("competicao")]
    public string Competicao { get; set; }

    [JsonPropertyName("retencao_talentos")]
    public string RetencaoTalentos { get; set; }

    [JsonPropertyName("investimento_marketing")]
    public string InvestimentoMarketing { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public DesafiosEmpresa()
    {
      Mercado = string.Empty;
      Internos = string.Empty;
      PrincipaisObstaculos = new Dictionary<string, string>();
    }
  }
}
