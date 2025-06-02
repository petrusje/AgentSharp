using Agents.net.Attributes;

namespace Agents.net.Tools
{
    public class FinanciamentoToolPack : ToolPack
    {
        public FinanciamentoToolPack()
        {
            Name = "FinanciamentoToolPack";
            Description = "Ferramentas para simulação de financiamento imobiliário";
            Version = "1.0.0";
        }

        [FunctionCall("Consulta o score de crédito do cliente.")]
        [FunctionCallParameter("documento", "CPF do cliente")]
        private int ConsultarScore(string documento)
        {
            // Simulação: CPF terminado em 1 tem score alto
            return documento.EndsWith("1") ? 850 : 650;
        }

        [FunctionCall("Simula o valor aprovado para financiamento.")]
        [FunctionCallParameter("renda", "Renda mensal do cliente")]
        [FunctionCallParameter("score", "Score de crédito do cliente")]
        [FunctionCallParameter("valorImovel", "Valor do imóvel desejado")]
        private decimal SimularAprovacao(decimal renda, int score, decimal valorImovel)
        {
            decimal fator = score > 800 ? 40 : 25;
            decimal aprovado = renda * fator;
            return aprovado > valorImovel ? valorImovel : aprovado;
        }
    }
}
