using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Examples.StructuredOutputs
{
  public class AnaliseDocumento
  {
    [JsonPropertyName("informacoes_empresa")]
    public InformacoesEmpresa InformacoesEmpresa { get; set; }

    [JsonPropertyName("dados_financeiros")]
    public DadosFinanceiros DadosFinanceiros { get; set; }

    [JsonPropertyName("conquistas")]
    public string[] Conquistas { get; set; }

    [JsonPropertyName("desafios")]
    public DesafiosEmpresa Desafios { get; set; }

    [JsonPropertyName("proximos_passos")]
    public ProximosPassosEmpresa ProximosPassos { get; set; }

    [JsonPropertyName("periodo")]
    public string Periodo { get; set; }

    [JsonPropertyName("classificacao_geral")]
    public ClassificacaoEmpresa ClassificacaoGeral { get; set; }

    [JsonPropertyName("score_financeiro")]
    public int ScoreFinanceiro { get; set; }

    public AnaliseDocumento()
    {
      InformacoesEmpresa = new InformacoesEmpresa();
      DadosFinanceiros = new DadosFinanceiros();
      Conquistas = new string[0]; // Compatível com .NET Standard 2.0
      Desafios = new DesafiosEmpresa();
      ProximosPassos = new ProximosPassosEmpresa();
      Periodo = string.Empty;
    }
  }
}
