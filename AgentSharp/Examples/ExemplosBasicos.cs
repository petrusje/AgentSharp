using AgentSharp.Core;
using AgentSharp.Examples.Agents;
using AgentSharp.Examples.Contexts;
using AgentSharp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
  /// <summary>
  /// Exemplos básicos demonstrando personality-driven agents,
  /// agents com ferramentas e análise financeira no contexto de Belo Horizonte
  /// </summary>
  public static class ExemplosBasicos
  {
    /// <summary>
    /// Demonstra um agente com personalidade distinta (jornalista mineiro)
    /// Contexto regional de Belo Horizonte
    /// </summary>
    public static async Task ExecutarJornalistaMineiro(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("⛰️  EXEMPLO 1: JORNALISTA MINEIRO - PERSONALITY-DRIVEN AGENT");
      Console.WriteLine("════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📄 Contexto regional de Belo Horizonte e Minas Gerais");
      Console.WriteLine("Demonstra como criar um agente com personalidade distinta\n");

      var contexto = new JornalistaMineiroContext
      {
        RegiaoFoco = "Belo Horizonte",
        IdiomaPreferido = "pt-BR",
        UltimaAtualizacao = DateTime.Now,
        seuNome = "Mauricio Mauro"
      };

      // Agente com personalidade de jornalista mineiro de BH
      var jornalista = new Agent<JornalistaMineiroContext, string>(modelo, "JornalistaMineiro")
          .WithPersona(ctx => @"
Você é um repórter mineiro com talento para contar histórias! ⛰️
Pense em si mesmo como uma mistura entre um contador de causos e um jornalista afiado.

SEU GUIA DE ESTILO:
- Comece com uma manchete chamativa usando emoji apropriados
- Compartilhe notícias com o jeito mineiro de ser: acolhedor e inteligente
- Mantenha suas respostas concisas mas envolventes (2-3 parágrafos)
- Use referências locais de BH e expressões mineiras quando apropriado
- Termine com uma assinatura marcante como 'Uai, sô!' ou 'Reportando da terra do pão de açúcar!'

Lembre-se de verificar todos os fatos enquanto mantém essa hospitalidade mineira!
Seja criativo mas responsável no jornalismo!
Seu nome é {ctx.seuNome}, um repórter apaixonado por Belo Horizonte e Minas Gerais.")
          .WithContext(contexto)
          .WithInstructions("Seja criativo, envolvente e mantenha o estilo jornalístico mineiro. Use emojis apropriados.")
          .WithGuardRails(" Nunca fale direto da terra do pão de açúcar, pois Minas é terra do Pão de Queijo!");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Pergunta: 'Me conte sobre uma história interessante que poderia estar acontecendo na Praça da Liberdade agora'");
      Console.ResetColor();
      Console.WriteLine("\n📻 Resposta do Jornalista:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await jornalista.ExecuteAsync(
            "Me conte sobre uma história interessante que poderia estar acontecendo na Praça da Liberdade em Belo Horizonte agora"
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"⏱️  Tempo: Não disponível");

        // Exemplo adicional
        Console.WriteLine("\n🔄 Testando outra pergunta com busca duckduckgo...\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🔥 Pergunta: 'Qual seria a última tendência gastronômica na Savassi?'");
        Console.ResetColor();

        //adicionando ferramentas de busca web
        jornalista.WithTools(new SearchToolPack());

        var resultado2 = await jornalista.ExecuteAsync(
            "Qual seria a última tendência gastronômica no bairro Savassi em Belo Horizonte? Pesquise na web por informações atuais."
        );

        Console.WriteLine("\n📻 Resposta do Jornalista:");
        Console.WriteLine(new string('-', 50));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado2.Data);
        resultado2.Tools.ForEach(tool =>
            Console.WriteLine($"🔧 Ferramenta utilizada: {tool.Name}"));
        Console.ResetColor();
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }

    /// <summary>
    /// Demonstra um agente com ferramentas de busca web
    /// Contexto regional de Belo Horizonte
    /// </summary>
    public static async Task ExecutarReporterComFerramentas(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🔍 EXEMPLO 2: REPÓRTER COM WEB SEARCH CUSTOMIZADA - AGENT WITH TOOLS");
      Console.WriteLine("═══════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📄 Contexto de reportagem em Belo Horizonte");
      Console.WriteLine("Demonstra como adicionar ferramentas de busca web ao agente\n");

      var contexto = new JornalistaContext
      {
        RegiaoFoco = "Belo Horizonte",
        IdiomaPreferido = "pt-BR"
      };

      // Agente com ferramentas de busca
      var reporterBusca = new JornalistaComBusca(modelo)
          .WithContext(contexto)
          .WithTools(new SearchToolPack());

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Pergunta: 'Quais são as últimas notícias sobre tecnologia no Brasil?'");
      Console.ResetColor();
      Console.WriteLine("\n📻 Resposta do Jornalista (com busca web):");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await reporterBusca.ExecuteAsync(
            "Quais são as últimas notícias sobre tecnologia em Belo Horizonte e Minas Gerais?"
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
        Console.WriteLine($"⏱️  Tempo: Não disponível");

        // Mostrar quais ferramentas foram usadas
        if (resultado.Tools.Count > 0)
        {
          Console.WriteLine("\n🛠️  Ferramentas utilizadas:");
          foreach (var tool in resultado.Tools)
          {
            Console.WriteLine($"   🔹 {tool.Name}");
          }
        }
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }

      // Exemplo adicional
      Console.WriteLine("\n🔄 Testando outra pergunta...\n");
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Pergunta: 'Quais são as últimas inovações em inteligência artificial no Brasil?'");
      Console.ResetColor();
    }

    /// <summary>
    /// Demonstra análise financeira com dados estruturados
    /// Contexto de mercado financeiro em Minas Gerais
    /// </summary>
    public static async Task ExecutarAnalistaFinanceiroBH(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("📈 EXEMPLO 3: ANALISTA FINANCEIRO BH - FINANCE DATA AGENT");
      Console.WriteLine("═════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📄 Análise de mercado financeiro em Minas Gerais");
      Console.WriteLine("Demonstra análise financeira com dados estruturados\n");

      var contexto = new AnaliseFinanceiraContext
      {
        FocoMercado = "Minas Gerais",
        TipoAnalise = "Investimento",
        PeriodoAnalise = "Últimos 3 meses"
      };

      var analistaFinanceiro = new AnalistaFinanceiro(modelo)
          .WithContext(contexto);

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Pergunta: 'Faça uma análise das ações da Petrobras (PETR4) considerando o cenário atual'");
      Console.ResetColor();
      Console.WriteLine("\n📊 Análise do Especialista:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await analistaFinanceiro.ExecuteAsync(
            "Faça uma análise das ações da Cemig (CMIG4) considerando o cenário atual do mercado mineiro e setor elétrico"
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
        Console.WriteLine($"⏱️  Tempo: Não disponível");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }

    /// <summary>
    /// Demonstra análise financeira com dados reais
    /// Contexto de mercado financeiro em Minas Gerais
    /// </summary>
    public static async Task ExecutarAnalistaFinanceiroRealData(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("📈 EXEMPLO 4: ANALISTA FINANCEIRO COM DADOS REAIS - REAL DATA AGENT");
      Console.WriteLine("═══════════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📄 Análise de mercado financeiro com dados reais em Minas Gerais");
      Console.WriteLine("Demonstra análise financeira com dados obtidos de APIs reais\n");

      var contexto = new AnaliseFinanceiraContext
      {
        FocoMercado = "Minas Gerais",
        TipoAnalise = "Investimento",
        PeriodoAnalise = "Últimos 3 meses"
      };

      var analistaFinanceiro = new AnalistaFinanceiroRealData(modelo)
          .WithContext(contexto);

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Pergunta: 'Faça uma análise das ações da Cemig (CMIG4) considerando o cenário atual'");
      Console.ResetColor();
      Console.WriteLine("\n📊 Análise do Especialista:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await analistaFinanceiro.ExecuteAsync(
            "Faça uma análise das ações da Cemig (CMIG4) considerando o cenário atual do mercado mineiro e setor elétrico"
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
        Console.WriteLine($"⏱️  Tempo: Não disponível");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }
  }

  // Todas as classes de contexto e agentes especializados foram separadas em arquivos individuais:
  // - Contexts/JornalistaContext.cs
  // - Contexts/JornalistaMineiroContext.cs
  // - Contexts/AnaliseFinanceiraContext.cs
  // - Agents/JornalistaComBusca.cs
  // - Agents/AnalistaFinanceiro.cs
  // - Agents/AnalistaFinanceiroRealData.cs
  // - DTOs/FinancialDataDTOs.cs
}
