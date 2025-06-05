using System.Text.Json.Serialization;

namespace Agents.net.Examples
{
   public class DadosPessoais
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("cargo_atual")]
        public string CargoAtual { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        public DadosPessoais()
        {
            Nome = string.Empty;
            CargoAtual = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
        }
    }
}
