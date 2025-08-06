using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSharp.Examples.StructuredOutputs
{
  public class ProximosPassosEmpresa
  {
    [JsonPropertyName("programa_retencao")]
    public string ProgramaRetencao { get; set; }

    [JsonPropertyName("investimento_pd")]
    public string InvestimentoPD { get; set; }

    [JsonPropertyName("metas_expansao")]
    public string MetasExpansao { get; set; }

    [JsonPropertyName("transformacao_digital")]
    public string TransformacaoDigital { get; set; }

    [JsonPropertyName("expandir_vendas")]
    public string ExpandirVendas { get; set; }

    [JsonPropertyName("campanha_marketing")]
    public string CampanhaMarketing { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public ProximosPassosEmpresa()
    {
      ProgramaRetencao = string.Empty;
      InvestimentoPD = string.Empty;
      MetasExpansao = string.Empty;
      TransformacaoDigital = string.Empty;
    }
  }
}
