using AgentSharp.Core;
using AgentSharp.Examples.Agents;
using AgentSharp.Examples.Contexts;
using AgentSharp.Examples.Tools;
using AgentSharp.Models;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
  /// <summary>
  /// Exemplos de reasoning agents para análise de problemas empresariais
  /// Demonstra capacidades de raciocínio estruturado e resolução de problemas em BH
  /// </summary>
  public static class ExemplosRaciocinio
  {
    /// <summary>
    /// Demonstra um agente que pode "pensar" e "analisar" através de cadeia de raciocínio
    /// Contexto de resolução de problemas empresariais em Belo Horizonte
    /// </summary>
    public static async Task ExecutarResolvedorProblemas(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🧠 EXEMPLO 4: RESOLVEDOR DE PROBLEMAS BH - REASONING AGENT");
      Console.WriteLine("═══════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📄 Análise de problemas empresariais em Belo Horizonte");
      Console.WriteLine("Demonstra raciocínio estruturado para negócios locais\n");

      var contextoProblema = new ContextoResolucaoProblemas
      {
        TipoProblema = "Empresarial",
        NivelComplexidade = "Alto",
        TempoDisponivel = "30 minutos"
      };

      // Agente especializado em resolução de problemas com reasoning
      var resolvedorProblemas = new ResolvedorDeProblemas(modelo)
          .WithContext(contextoProblema)
          .WithReasoning(true) // Habilita reasoning
          .WithPersona(@"
Você é um consultor especialista em resolução de problemas complexos! 🧠💡

METODOLOGIA DE RACIOCÍNIO ESTRUTURADO:
1. Analise profundamente o problema apresentado
2. Decomponha em componentes menores
3. Desenvolva hipóteses e soluções
4. Avalie cada solução criticamente
5. Forneça recomendações estruturadas e acionáveis

ESTRUTURA DE RESPOSTA:
🎯 RESUMO EXECUTIVO
🔍 ANÁLISE DO PROBLEMA
🧩 DECOMPOSIÇÃO
💡 SOLUÇÕES PROPOSTAS
📊 ANÁLISE CUSTO-BENEFÍCIO
🚀 PLANO DE IMPLEMENTAÇÃO
⚠️  RISCOS E MITIGAÇÕES");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Problema: 'Uma startup de BH está perdendo 20% dos clientes mensalmente. Como resolver?'");
      Console.ResetColor();
      Console.WriteLine("\n🧠 Análise com Raciocínio Estruturado:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await resolvedorProblemas.ExecuteAsync(
            @"Uma startup de tecnologia de Belo Horizonte está enfrentando uma taxa de churn de 20% ao mês.
                    Os clientes estão cancelando após 3-4 meses de uso.
                    A empresa tem 800 clientes atuais e precisa de uma solução urgente.
                    Analise o problema considerando o contexto do mercado mineiro."
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        // Mostrar reasoning content se disponível
        if (!string.IsNullOrEmpty(resultado.ReasoningContent))
        {
          Console.WriteLine("\n🧠 PROCESSO DE RACIOCÍNIO (Reasoning Content):");
          Console.WriteLine(new string('=', 60));
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine(resultado.ReasoningContent);
          Console.ResetColor();
        }

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🔧 Ferramentas usadas: {resultado.Tools.Count}");
        Console.WriteLine($"🧠 Reasoning habilitado: {(!string.IsNullOrEmpty(resultado.ReasoningContent) ? "Sim" : "Não")}");

        // Exemplo adicional com problema técnico
        Console.WriteLine("\n🔄 Testando com problema técnico...\n");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🔥 Problema: 'Sistema de e-commerce com 5 segundos de carregamento. Como otimizar?'");
        Console.ResetColor();

        var resultado2 = await resolvedorProblemas.ExecuteAsync(
            @"Um sistema de e-commerce está com tempo de carregamento de 5 segundos,
                    causando 40% de abandono de carrinho. O sistema usa React no frontend,
                    Node.js no backend e PostgreSQL. Como otimizar a performance?"
        );

        Console.WriteLine("\n🧠 Solução Técnica:");
        Console.WriteLine(new string('-', 50));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado2.Data);
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
    /// Demonstra um agente avaliador de soluções empresariais
    /// </summary>
    public static async Task ExecutarAvaliadorSolucoes(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("⚖️ EXEMPLO 5: AVALIADOR DE SOLUÇÕES - REASONING AGENT");
      Console.WriteLine("═══════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📊 Análise e avaliação de soluções empresariais");
      Console.WriteLine("Demonstra raciocínio para comparar e priorizar opções\n");

      var contextoAvaliacao = new ContextoResolucaoProblemas
      {
        TipoProblema = "Avaliação de Soluções",
        NivelComplexidade = "Alto",
        TempoDisponivel = "45 minutos",
        budgetMaximo = "R$ 1.000.000",
        RestricaoMaisImportante = "Não posso contratar mais de 5 pessoas para o projeto"
      };

      var avaliadorSolucoes = new Agent<ContextoResolucaoProblemas, string>(modelo, "AvaliadorSolucoesEspecialista")
          .WithContext(contextoAvaliacao)
          .WithReasoning(true)
          .WithTools(new PackExemplosRaciocinio()) // Inclui pack de ferramentas
          .WithPersona(@"
Você é um consultor especialista em avaliação de soluções empresariais! ⚖️📊

METODOLOGIA DE AVALIAÇÃO:
1. Análise de viabilidade técnica e financeira
2. Matriz de impacto vs. esforço
3. Análise de riscos e mitigações
4. Comparação custo-benefício
5. Ranking priorizado de soluções

ESTRUTURA DE RESPOSTA:
📊 MATRIZ DE AVALIAÇÃO
⚖️ COMPARAÇÃO DETALHADA
💰 ANÁLISE FINANCEIRA
🎯 RANKING DE PRIORIDADES
🚨 RISCOS E MITIGAÇÕES
📋 RECOMENDAÇÕES FINAIS")
          .WithInstructions(
              ctx => $@" Leve em consideracao o bugget máximo de {ctx.budgetMaximo} e a restrição mais importante: {ctx.RestricaoMaisImportante}");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Problema: 'Avaliar 3 opções para migração de sistema legado'");
      Console.ResetColor();
      Console.WriteLine("\n⚖️ Avaliação Estruturada:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await avaliadorSolucoes.ExecuteAsync(
            @"Preciso avaliar 3 opções para migração do sistema legado da empresa:

                    OPÇÃO 1: Migração completa para cloud-native (React + Node.js + AWS)
                    - Tecnologia : React, Node.js, AWS
                    - Custo: R$ 800k
                    - Tempo: 18 meses
                    - Benefícios: Escalabilidade total, performance moderna

                    OPÇÃO 2: Modernização incremental (manter backend, modernizar frontend)
                     - Tecnologia : AngularJS, Java
                    - Custo: R$ 300k
                    - Tempo: 8 meses
                    - Benefícios: Menor risco, entrega mais rápida

                    OPÇÃO 3: Migração híbrida (microsserviços gradual)
                    - Tecnologia : React, Node.js, AWS + AngularJS + java
                    - Custo: R$ 500k
                    - Tempo: 16 meses
                    - Benefícios: Flexibilidade, migração por módulos

                    Avalie considerando: custo, tempo, risco, benefícios de longo prazo e disponibilidade de time por tecnologia"
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"⚖️ Avaliação concluída com sucesso");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }

    /// <summary>
    /// Demonstra um agente identificador de obstáculos
    /// </summary>
    public static async Task ExecutarIdentificadorObstaculos(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🛡️ EXEMPLO 6: IDENTIFICADOR DE OBSTÁCULOS - REASONING AGENT");
      Console.WriteLine("═══════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("🔍 Identificação proativa de obstáculos e riscos");
      Console.WriteLine("Demonstra análise preventiva de problemas potenciais\n");

      var contextoObstaculos = new ContextoResolucaoProblemas
      {
        TipoProblema = "Identificação de Riscos",
        NivelComplexidade = "Alto",
        TempoDisponivel = "30 minutos",
      };

      var identificadorObstaculos = new Agent<ContextoResolucaoProblemas, string>(modelo, "IdentificadorObstaculosEspecialista")
          .WithContext(contextoObstaculos)
          .WithReasoning(true)
          .WithPersona(@"
Você é um especialista em identificação de obstáculos e gestão de riscos! 🛡️🔍

METODOLOGIA DE ANÁLISE:
1. Mapeamento completo de stakeholders
2. Identificação de riscos técnicos, financeiros e operacionais
3. Análise de dependências críticas
4. Avaliação de probabilidade vs. impacto
5. Planos de contingência e mitigação

ESTRUTURA DE RESPOSTA:
🎯 OBSTÁCULOS IDENTIFICADOS
📊 MATRIZ DE RISCOS
⚠️ PONTOS CRÍTICOS
🛡️ ESTRATÉGIAS DE MITIGAÇÃO
📋 PLANO DE CONTINGÊNCIA
🚨 ALERTAS PRIORITÁRIOS");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("🔥 Cenário: 'Lançamento de produto em startup com prazo apertado'");
      Console.ResetColor();
      Console.WriteLine("\n🛡️ Análise de Obstáculos:");
      Console.WriteLine(new string('-', 50));

      try
      {
        var resultado = await identificadorObstaculos.ExecuteAsync(
            @"Nossa startup precisa lançar um MVP em 3 meses para apresentar aos investidores.
                    O produto é uma plataforma SaaS B2B de gestão de projetos.

                    CONTEXTO:
                    - Equipe: 5 desenvolvedores (2 seniors, 3 plenos)
                    - Budget: R$ 200k
                    - Concorrentes: Trello, Asana, Monday.com
                    - Deadline rígido para demo aos investidores

                    Identifique todos os obstáculos potenciais e riscos que podem comprometer
                    o lançamento, incluindo aspectos técnicos, de mercado, equipe e financeiros."
        );

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(resultado.Data);
        Console.ResetColor();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🛡️ Análise de obstáculos concluída");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
      }
    }
  }

  // Todas as classes foram movidas para seus próprios arquivos:
  // - Contexts/ContextoResolucaoProblemas.cs
  // - Agents/ResolvedorDeProblemas.cs
  // - Tools/PackExemplosRaciocinio.cs
}
