using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Examples.StructuredOutputs
{
  public class DadosFinanceiros
  {
    [JsonPropertyName("receita_total")]
    public decimal ReceitaTotal { get; set; }

    [JsonPropertyName("custos_operacionais")]
    public decimal CustosOperacionais { get; set; }

    [JsonPropertyName("lucro_liquido")]
    public decimal LucroLiquido { get; set; }

    [JsonPropertyName("margem_lucro")]
    public decimal MargemLucro { get; set; }
  }
}
