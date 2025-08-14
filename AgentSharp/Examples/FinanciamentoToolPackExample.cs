using AgentSharp.Core;
using AgentSharp.Core.Abstractions;
using AgentSharp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
  public class FinancingAgent : Agent<PropostaContexto, string>
  {
    public FinancingAgent(IModel model, string name, string description)
        : base(model, name, description) { }

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
    /// <summary>
    /// Exemplo usando Dependency Injection PURA (RECOMENDADO)
    /// Demonstra como configurar DI corretamente - SEM shortcuts ou métodos estáticos
    /// </summary>
    public static void RunWithDIAsync(string apiKey, string endpoint = "https://api.openai.com/")
    {
      // ARQUITETURA LIMPA: Configure providers explicitamente
      // NOTA: Precisa referenciar AgentSharp.Providers.OpenAI no seu projeto!

      Console.WriteLine("\u26a0️  EXEMPLO DI REQUER REFERÊNCIA: AgentSharp.Providers.OpenAI");
      Console.WriteLine("Configure assim em seu projeto:");
      Console.WriteLine();
      Console.WriteLine("var providers = new List<IModelProvider>");
      Console.WriteLine("{");
      Console.WriteLine("    new OpenAIModelProvider(apiKey, endpoint)");
      Console.WriteLine("};");
      Console.WriteLine("var factory = new ModelFactory(providers);");
      Console.WriteLine("var model = factory.CreateModel(\"openai\", options);");
      Console.WriteLine();
      Console.WriteLine("📝 Para demonstração, usando método legacy...");

      // Para demonstração, vamos usar mock model
      RunLegacyAsync();
    }

    /// <summary>
    /// Exemplo TEMPORÁRIO usando MockModel (para demonstração)
    /// Em produção: use DI com providers reais!
    /// </summary>
    public static void RunLegacyAsync()
    {
      // ATENÇÃO: ModelFactory agora REQUER DI!
      // Isso VAI FALHAR se não tiver providers configurados

      Console.WriteLine("⚠️  AVISO: ModelFactory agora requer DI!");
      Console.WriteLine("Este exemplo vai falhar - isso é INTENCIONAL!");
      Console.WriteLine("Configure providers via DI para usar em produção.");

      try
      {
        // Isso VAI FALHAR - e é assim que deve ser!
        var modelFactory = new ModelFactory(new List<IModelProvider>());
        Console.WriteLine("Não deveria chegar aqui!");
      }
      catch (ArgumentException ex)
      {
        Console.WriteLine($"✅ FALHOU COMO ESPERADO: {ex.Message}");
        Console.WriteLine("\n🎯 ARQUITETURA LIMPA: DI é OBRIGATÓRIO!");
        Console.WriteLine("Para usar em produção:");
        Console.WriteLine("1. Referencie: AgentSharp.Providers.OpenAI");
        Console.WriteLine("2. Configure: var provider = new OpenAIModelProvider(apiKey)");
        Console.WriteLine("3. Injete: var factory = new ModelFactory([provider])");
        Console.WriteLine("4. Use: factory.CreateModel(\"openai\", options)");
      }
    }

    /// <summary>
    /// Método principal que executava o exemplo (mantém compatibilidade)
    /// </summary>
    public static void RunAsync()
    {
      // Por padrão, usa a abordagem legacy para manter compatibilidade
      RunLegacyAsync();
    }

    private static async Task ExecuteFinancingExample(Agent<PropostaContexto, string> agente)
    {
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

  // *** MOCK PROVIDER REMOVIDO ***
  // Examples agora demonstram APENAS conceitos de DI
  // Para implementação real: referencie AgentSharp.Providers.*
}
