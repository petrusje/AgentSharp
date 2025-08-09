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
  /// Exemplos básicos demonstrando conceitos fundamentais do AgentSharp:
  /// 1. Agente Simples - primeira interação com LLM
  /// 2. Agente com Personalidade - customização de behavior
  /// 3. Agente com Tools - integração com ferramentas
  /// 4. Sistema Empresarial - caso real avançado
  /// </summary>
  public static class ExemplosBasicos
  {
    /// <summary>
    /// NÍVEL 1 - FUNDAMENTOS: Demonstra a criação de um agente básico
    /// Este é o exemplo mais simples possível com AgentSharp
    /// Aprenda: criação de agente, execução básica, estrutura de resposta
    /// </summary>
    public static async Task ExecutarAgenteSimples(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🎯 NÍVEL 1 - AGENTE SIMPLES: Primeira Interação com LLM");
      Console.WriteLine("════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
      Console.WriteLine("   • Criação básica de agente");
      Console.WriteLine("   • Execução de prompt simples");
      Console.WriteLine("   • Estrutura de resposta AgentResult");
      Console.WriteLine("   • Contagem de tokens\n");

      // Este é o código mais simples possível para criar um agente
      var agenteSimples = new Agent<object, string>(modelo, "MeuPrimeiroAgente");

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("💻 CÓDIGO USADO:");
      Console.WriteLine("   var agenteSimples = new Agent<object, string>(modelo, \"MeuPrimeiroAgente\");");
      Console.WriteLine("   var resultado = await agenteSimples.ExecuteAsync(pergunta);");
      Console.ResetColor();

      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("\n🔥 PERGUNTA: 'O que é .NET Framework e para que serve?'");
      Console.ResetColor();
      Console.WriteLine("\n🤖 Resposta do Agente:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await agenteSimples.ExecuteAsync(
            "O que é .NET Framework e para que serve? Explique de forma clara e objetiva."
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📊 INFORMAÇÕES TÉCNICAS:");
        Console.WriteLine($"   ✅ Status: Sucesso");
        Console.WriteLine($"   🎯 Agente: {agenteSimples.Name}");
        Console.WriteLine($"   📊 Tokens Totais: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"   📥 Input: {resultado.Usage.PromptTokens}");
        Console.WriteLine($"   📤 Output: {resultado.Usage.CompletionTokens}");
        Console.WriteLine($"   ⏱️  Resposta recebida com sucesso!");
        Console.ResetColor();

        Console.WriteLine("\n💡 PRÓXIMO PASSO: Experimente o Exemplo 2 para aprender sobre personalização!");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }
    /// <summary>
    /// NÍVEL 1 - FUNDAMENTOS: Demonstra personalização avançada de agentes
    /// Aprenda: persona customizada, context objects, guard rails, instruções
    /// Este exemplo mostra como criar agentes com comportamento específico
    /// </summary>
    public static async Task ExecutarJornalistaMineiro(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🎭 NÍVEL 1 - AGENTE COM PERSONALIDADE: Customização Avançada");
      Console.WriteLine("═══════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
      Console.WriteLine("   • Context Objects - dados tipados (JornalistaMineiroContext)");
      Console.WriteLine("   • Persona dinâmica - usa ctx.seuNome na personalidade");
      Console.WriteLine("   • Instructions dinâmicas - ctx.IdiomaPreferido nas instruções");
      Console.WriteLine("   • Guard Rails - restrições de comportamento");
      Console.WriteLine("   • Context binding - como o contexto alimenta o comportamento\n");

      var contexto = new JornalistaMineiroContext
      {
        RegiaoFoco = "Belo Horizonte",
        IdiomaPreferido = "pt-BR",
        UltimaAtualizacao = DateTime.Now,
        seuNome = "Mauricio Mauro"
      };

      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("💻 CÓDIGO USADO:");
      Console.WriteLine("   var contexto = new JornalistaMineiroContext {");
      Console.WriteLine("       RegiaoFoco = \"Belo Horizonte\",");
      Console.WriteLine("       seuNome = \"Mauricio Mauro\",");
      Console.WriteLine("       IdiomaPreferido = \"pt-BR\"");
      Console.WriteLine("   };");
      Console.WriteLine("   var jornalista = new Agent<JornalistaMineiroContext, string>(modelo)");
      Console.WriteLine("       .WithPersona(ctx => \"...\")  // Usa {ctx.seuNome}");
      Console.WriteLine("       .WithContext(contexto)        // ⭐ Dados tipados injetados!");
      Console.WriteLine("       .WithInstructions(ctx => $\"Use {ctx.IdiomaPreferido}\") // ⭐ Context dinâmico!");
      Console.ResetColor();

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
          .WithInstructions(ctx => $"Seja criativo, envolvente e mantenha o estilo jornalístico mineiro. Use emojis apropriados. Apresente-se dizendo seu nome: {ctx.seuNome}. responda sempre no IdiomaPreferido: {ctx.IdiomaPreferido}")
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
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📊 ESTATÍSTICAS TÉCNICAS:");
        Console.WriteLine($"   • Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"   • Context.RegiaoFoco: {contexto.RegiaoFoco}");
        Console.WriteLine($"   • Context.seuNome: {contexto.seuNome}");
        Console.WriteLine($"   • Context.IdiomaPreferido: {contexto.IdiomaPreferido}");
        Console.WriteLine($"   • Persona: Jornalista Mineiro (dinâmica com contexto)");
        Console.ResetColor();

        // Exemplo adicional
        Console.WriteLine("\n🔄 Testando outra pergunta com busca duckduckgo...\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🔥 Pergunta: 'Qual seria a última tendência gastronômica na Savassi?'");
        Console.ResetColor();

        //adicionando ferramentas de busca web
        jornalista.WithTools(new SearchToolPack());

        var resultado2 = await jornalista.ExecuteAsync(
            "Qual seria a última tendência gastronômica no bairro Savassi em Belo Horizonte? Pesquise na web por informações atuais, use na resposta."
        );

        Console.WriteLine("\n📻 Resposta do Jornalista:");
        Console.WriteLine(new string('-', 50));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado2.Data);
        resultado2.Tools.ForEach(tool =>
            Console.WriteLine($"🔧 Ferramenta utilizada: {tool.Name}"));
        Console.ResetColor();

        Console.WriteLine("\n🔄 Demonstrando mudança de contexto...");
        
        // Mudar o contexto para demonstrar como afeta o comportamento
        contexto.seuNome = "Ana Silva";
        contexto.IdiomaPreferido = "en-US";
        contexto.RegiaoFoco = "São Paulo";
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n⚡ CONTEXTO ALTERADO:");
        Console.WriteLine($"   Nome: {contexto.seuNome}");
        Console.WriteLine($"   Idioma: {contexto.IdiomaPreferido}");
        Console.WriteLine($"   Região: {contexto.RegiaoFoco}");
        Console.ResetColor();
        
        var resultado3 = await jornalista.ExecuteAsync(
            "Tell me about a current technology trend in São Paulo"
        );
        
        Console.WriteLine("\n📻 Resposta com Novo Contexto:");
        Console.WriteLine(new string('-', 50));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado3.Data);
        Console.ResetColor();
        
        Console.WriteLine("\n🎯 OBSERVE: O agente agora se apresenta como Ana Silva e responde em inglês!");
        Console.WriteLine("💡 PRÓXIMO PASSO: Veja o Exemplo 3 para entender como adicionar ferramentas!");
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
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("🏢 NÍVEL 3 - SISTEMA EMPRESARIAL: Caso Real Completo");
      Console.WriteLine("═══════════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
      Console.WriteLine("   • Agente especializado em domínio específico");
      Console.WriteLine("   • Contexto empresarial complexo");
      Console.WriteLine("   • Análise de dados financeiros");
      Console.WriteLine("   • Integração com sistemas reais");
      Console.WriteLine("   • Relatórios executivos\n");

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
