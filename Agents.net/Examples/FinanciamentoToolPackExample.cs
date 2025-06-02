using System;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;

namespace Agents.net.Examples
{
    public class FinancingAgent : Agent<PropostaContexto, string>
    {
        public FinancingAgent(IModel model, string name, string description)
            : base(model, name, description){}

    }


    public class PropostaContexto
    {
        public string NomeCliente { get; set; }
        public string Documento { get; set; }
        public string LocalizacaoDesejada { get; set; }
        public decimal RendaMensal { get; set; }
        public decimal ValorImovel { get; set; }
    }

    public static class FinanciamentoToolPackExample
    {
        public static async Task RunAsync()
        {
            var model = new ModelFactory().CreateModel("openai", new ModelOptions { ModelName = "gpt-4o" });
            var toolPack = new FinanciamentoToolPack();
            var agente = new FinancingAgent(model, "financiamento", "Agente de financiamento")
                .WithToolPacks(toolPack);

            var contexto = new PropostaContexto
            {
                NomeCliente = "Ana Souza",
                Documento = "12345678901",
                LocalizacaoDesejada = "Moema, São Paulo",
                RendaMensal = 12000,
                ValorImovel = 350000
            };

            string prompt = $"Simule uma proposta de financiamento para {contexto.NomeCliente}, CPF {contexto.Documento}, " +
                           $"renda mensal R$ {contexto.RendaMensal}, imóvel de R$ {contexto.ValorImovel} em {contexto.LocalizacaoDesejada}. " +
                           $"Use as ferramentas disponíveis para consultar score e simular aprovação.";

            var resultado = await agente.ExecuteAsync(prompt, contexto);
            Console.WriteLine("=== Proposta Gerada ===\n" + resultado.Data);
            Console.WriteLine("\nChamadas de ferramentas:");
            foreach (var tool in resultado.Tools)
            {
                Console.WriteLine($"- {tool.Name}: {tool.Result}");
            }
        }
    }
}
