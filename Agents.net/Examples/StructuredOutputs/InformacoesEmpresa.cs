using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Examples.StructuredOutputs
{
  public class InformacoesEmpresa
  {
    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("cnpj")]
    public string CNPJ { get; set; }

    [JsonPropertyName("ceo")]
    public string CEO { get; set; }

    public InformacoesEmpresa()
    {
      Nome = string.Empty;
      CNPJ = string.Empty;
      CEO = string.Empty;
    }
  }
}
