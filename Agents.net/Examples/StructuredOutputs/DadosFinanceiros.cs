using System.Text.Json.Serialization;

namespace Agents.net.Examples
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
