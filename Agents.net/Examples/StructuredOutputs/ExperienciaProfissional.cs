using System.Text.Json.Serialization;

namespace Agents.net.Examples
{
       public class ExperienciaProfissional
    {
        [JsonPropertyName("cargo")]
        public string Cargo { get; set; }

        [JsonPropertyName("empresa")]
        public string Empresa { get; set; }

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        public ExperienciaProfissional()
        {
            Cargo = string.Empty;
            Empresa = string.Empty;
            Periodo = string.Empty;
        }
    }

}
