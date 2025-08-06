using System.Text.Json.Serialization;

namespace AgentSharp.Examples.StructuredOutputs
{
  public class AnaliseCurriculo
  {
    [JsonPropertyName("dados_pessoais")]
    public DadosPessoais DadosPessoais { get; set; }

    [JsonPropertyName("anos_experiencia")]
    public int AnosExperiencia { get; set; }

    [JsonPropertyName("experiencias")]
    public ExperienciaProfissional[] Experiencias { get; set; }

    [JsonPropertyName("habilidades_principais")]
    public string[] HabilidadesPrincipais { get; set; }

    [JsonPropertyName("nivel_senioridade")]
    public NivelSenioridade NivelSenioridade { get; set; }

    [JsonPropertyName("score_geral")]
    public int ScoreGeral { get; set; }

    public AnaliseCurriculo()
    {
      DadosPessoais = new DadosPessoais();
      Experiencias = new ExperienciaProfissional[0];
      HabilidadesPrincipais = new string[0];
    }
  }

}

