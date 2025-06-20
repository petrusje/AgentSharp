using Agents.net.Examples;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Examples.Models;
using Arcana.AgentsNet.Examples.StructuredOutputs;
using Arcana.AgentsNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Examples
{
  /// <summary>
  /// Programa de teste completo do sistema de reasoning com function calling
  /// para análise de gargalos em projetos de desenvolvimento
  /// </summary>
  public static class ProgramaReasoningProjetos
  {
    public static async Task ExecutarAnaliseGargalosProjetos(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("🚀 TESTE COMPLETO: REASONING + FUNCTION CALLING - ANÁLISE DE PROJETOS");
      Console.WriteLine("═══════════════════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📊 Sistema avançado de análise de gargalos em projetos");
      Console.WriteLine("🧠 Demonstra reasoning estruturado com tools especializados\n");

      // Inicializar banco de dados de projetos
      var bancoDados = InicializarBancoDadosProjetos();
      var contexto = new ContextoProjetos { BancoDados = bancoDados };

      // Criar agente especialista em projetos com reasoning habilitado
      var analistaProjectos = new Agent<ContextoProjetos, RelatorioAnalise>(modelo, "AnalistaProjetos")
          .WithContext(contexto)
          .WithReasoning(true)
          .WithReasoningSteps(4, 8)
          .WithToolPacks(new ProjetosToolPack())
          .WithPersona(@"
Você é um consultor sênior especialista em análise de projetos de desenvolvimento! 🎯📊

ESPECIALIDADE:
- Identificação de gargalos em projetos
- Análise de métricas de performance
- Otimização de processos de desenvolvimento
- Diagnóstico de problemas de equipe e infraestrutura

METODOLOGIA DE ANÁLISE:
1. Colete dados dos projetos usando as ferramentas disponíveis
2. Analise métricas críticas (velocidade, bugs, deploys)
3. Identifique padrões e gargalos
4. Proponha soluções específicas e mensuráveis
5. Priorize ações por impacto x esforço

SEMPRE use as ferramentas para buscar dados reais antes de fazer análises!")
          .WithInstructions(@"
Para cada análise:
1. Use GetProjeto() para obter detalhes do projeto
2. Use GetMetricasProjeto() para dados de performance
3. Use GetEquipeProjeto() para informações da equipe
4. Use GetHistoricoDeploysDefeitos() para análise de qualidade
5. Analise os dados usando reasoning estruturado
6. Forneça recomendações acionáveis");

      // Cenário de teste conhecido: Projeto com gargalo específico
      var projetoProblema = "PROJ-003"; // Projeto com problemas de deploy

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"🔍 Analisando projeto com gargalos conhecidos: {projetoProblema}");
      Console.WriteLine("📋 Esperado: Identificar problemas de deploy e instabilidade");
      Console.ResetColor();
      Console.WriteLine("\n🧠 Iniciando análise com reasoning estruturado...\n");

      try
      {
        var resultado = await analistaProjectos.ExecuteAsync($@"
Analise completamente o projeto {projetoProblema} e identifique todos os gargalos existentes.

ESCOPO DA ANÁLISE:
- Performance geral do projeto
- Métricas de velocidade de desenvolvimento
- Qualidade de código e bugs
- Eficiência da equipe
- Problemas de deploy e infraestrutura

Use todas as ferramentas disponíveis para coletar dados e forneça:
1. Diagnóstico completo dos problemas
2. Priorização dos gargalos por impacto
3. Plano de ação específico
4. Métricas para acompanhar melhorias

Seja detalhado e baseie-se nos dados coletados.");

        // Exibir resultado principal
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ RELATÓRIO DE ANÁLISE GERADO:");
        Console.WriteLine(new string('=', 60));
        Console.ResetColor();

        if (resultado.Data != null)
        {
          Console.WriteLine(resultado.Data);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("❌ Erro: Resultado da análise está vazio!");
          Console.ResetColor();
        }

        // Exibir ferramentas utilizadas
        if (resultado.Tools.Count > 0)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.WriteLine("\n🔧 FERRAMENTAS UTILIZADAS:");
          Console.WriteLine(new string('=', 40));
          foreach (var tool in resultado.Tools)
          {
            Console.WriteLine($"🛠️  {tool.Name}: {tool.Result}");
          }
          Console.ResetColor();
        }

        // Exibir processo de reasoning
        if (!string.IsNullOrEmpty(resultado.ReasoningContent))
        {
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.WriteLine("\n🧠 PROCESSO DE RACIOCÍNIO:");
          Console.WriteLine(new string('=', 50));
          Console.WriteLine(resultado.ReasoningContent);
          Console.ResetColor();
        }

        // Validar se identificou os gargalos esperados
        ValidarResultadoEsperado(resultado, projetoProblema);

        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
        Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
        Console.WriteLine($"🧠 Steps de reasoning: {resultado.ReasoningSteps.Count}");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro durante análise: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Console.ResetColor();
      }
    }

    private static void ValidarResultadoEsperado(AgentResult<RelatorioAnalise> resultado, string projetoId)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("\n🎯 VALIDAÇÃO DOS RESULTADOS ESPERADOS:");
      Console.WriteLine(new string('=', 45));

      // Verificações esperadas para PROJ-003
      var problemas_esperados = new List<string>
            {
                "deploy", "instabilidade", "frequency", "pipeline", "infraestrutura"
            };

      var resultado_texto = resultado.Data?.ToString() ?? "";
      var encontrados = new List<string>();
      var nao_encontrados = new List<string>();

      foreach (var problema in problemas_esperados)
      {
        if (resultado_texto.ToLower().Contains(problema.ToLower()))
        {
          encontrados.Add(problema);
        }
        else
        {
          nao_encontrados.Add(problema);
        }
      }

      Console.WriteLine($"✅ Problemas identificados corretamente: {encontrados.Count}/{problemas_esperados.Count}");
      foreach (var item in encontrados)
      {
        Console.WriteLine($"   ✓ {item}");
      }

      if (nao_encontrados.Count > 0)
      {
        Console.WriteLine($"❌ Problemas não identificados: {nao_encontrados.Count}");
        foreach (var item in nao_encontrados)
        {
          Console.WriteLine($"   ✗ {item}");
        }
      }

      // Validar uso de ferramentas
      var ferramentas_esperadas = new List<string> { "GetProjeto", "GetMetricas", "GetEquipe" };
      var ferramentas_usadas = resultado.Tools.Select(t => t.Name).ToList();

      Console.WriteLine($"\n🔧 Ferramentas utilizadas: {ferramentas_usadas.Count}");
      foreach (var ferramenta in ferramentas_esperadas)
      {
        if (ferramentas_usadas.Any(f => f.Contains(ferramenta)))
        {
          Console.WriteLine($"   ✓ {ferramenta}");
        }
        else
        {
          Console.WriteLine($"   ✗ {ferramenta} (não utilizada)");
        }
      }

      Console.ResetColor();
    }

    private static BancoDadosProjetos InicializarBancoDadosProjetos()
    {
      // Método que inicializa o banco de dados de projetos com dados para teste
      // Implementação movida para Models/BancoDadosProjetos.cs

      // Criação de projetos de teste com diferentes características
      var db = new BancoDadosProjetos();

      // PROJ-001: Projeto saudável (E-commerce)
      db.Projetos["PROJ-001"] = new Projeto
      {
        Id = "PROJ-001",
        Nome = "Sistema de E-commerce",
        Status = "Em Desenvolvimento",
        DataInicio = DateTime.Now.AddMonths(-6),
        TamanhoEquipe = 5,
        Tecnologias = new[] { "React", "Node.js", "MongoDB" },
        MetricasVelocidade = new MetricasVelocidade
        {
          VelocidadeMedia = 8.5,
          BurndownRate = 0.85,
          TasksCompletadas = 45,
          TasksPendentes = 12
        },
        MetricasQualidade = new MetricasQualidade
        {
          BugsAbertos = 3,
          BugsFechados = 28,
          CoberturaTestes = 85.2,
          DeployFrequency = 12
        }
      };

      // PROJ-002: Projeto excelente (API Pagamentos)
      db.Projetos["PROJ-002"] = new Projeto
      {
        Id = "PROJ-002",
        Nome = "API de Pagamentos",
        Status = "Homologação",
        DataInicio = DateTime.Now.AddMonths(-4),
        TamanhoEquipe = 3,
        Tecnologias = new[] { ".NET Core", "PostgreSQL", "Redis" },
        MetricasVelocidade = new MetricasVelocidade
        {
          VelocidadeMedia = 12.0,
          BurndownRate = 0.95,
          TasksCompletadas = 67,
          TasksPendentes = 3
        },
        MetricasQualidade = new MetricasQualidade
        {
          BugsAbertos = 1,
          BugsFechados = 15,
          CoberturaTestes = 92.8,
          DeployFrequency = 8
        }
      };

      // PROJ-003: Projeto com PROBLEMAS (Dashboard)
      db.Projetos["PROJ-003"] = new Projeto
      {
        Id = "PROJ-003",
        Nome = "Dashboard Analytics",
        Status = "Em Desenvolvimento",
        DataInicio = DateTime.Now.AddMonths(-8),
        TamanhoEquipe = 7,
        Tecnologias = new[] { "Vue.js", "Python", "ClickHouse" },
        MetricasVelocidade = new MetricasVelocidade
        {
          VelocidadeMedia = 4.2, // BAIXA - GARGALO!
          BurndownRate = 0.65,   // BAIXA - GARGALO!
          TasksCompletadas = 23,
          TasksPendentes = 34    // ALTO - GARGALO!
        },
        MetricasQualidade = new MetricasQualidade
        {
          BugsAbertos = 18,      // ALTO - GARGALO!
          BugsFechados = 22,
          CoberturaTestes = 45.3, // BAIXA - GARGALO!
          DeployFrequency = 2     // MUITO BAIXA - GARGALO CRÍTICO!
        }
      };

      // Configuração das equipes
      db.Equipes["PROJ-001"] = new EquipeProjeto
      {
        ProjetoId = "PROJ-001",
        TechLead = "Carlos Silva",
        Desenvolvedores = new[] { "Ana Costa", "João Pereira", "Maria Santos" },
        QA = "Pedro Oliveira",
        SatisfacaoEquipe = 8.5,
        RotatividadeUltimos6Meses = 1
      };

      db.Equipes["PROJ-002"] = new EquipeProjeto
      {
        ProjetoId = "PROJ-002",
        TechLead = "Fernanda Lima",
        Desenvolvedores = new[] { "Roberto Nunes", "Julia Martins" },
        QA = "Diego Rocha",
        SatisfacaoEquipe = 9.2,
        RotatividadeUltimos6Meses = 0
      };

      db.Equipes["PROJ-003"] = new EquipeProjeto
      {
        ProjetoId = "PROJ-003",
        TechLead = "Rafael Mendes",
        Desenvolvedores = new[] { "Lucia Ferreira", "Marcos Andrade", "Camila Torres", "Bruno Machado" },
        QA = "Tatiana Gomes",
        SatisfacaoEquipe = 5.8, // BAIXA - GARGALO!
        RotatividadeUltimos6Meses = 3 // ALTA - GARGALO!
      };

      // Histórico de deploys
      db.HistoricoDeploysDefeitos["PROJ-001"] = new List<EventoDeploy>
            {
                new EventoDeploy { Data = DateTime.Now.AddDays(-7), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(5) },
                new EventoDeploy { Data = DateTime.Now.AddDays(-14), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(3) },
                new EventoDeploy { Data = DateTime.Now.AddDays(-21), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(8) }
            };

      db.HistoricoDeploysDefeitos["PROJ-002"] = new List<EventoDeploy>
            {
                new EventoDeploy { Data = DateTime.Now.AddDays(-4), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(2) },
                new EventoDeploy { Data = DateTime.Now.AddDays(-11), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(1) }
            };

      db.HistoricoDeploysDefeitos["PROJ-003"] = new List<EventoDeploy>
            {
                new EventoDeploy { Data = DateTime.Now.AddDays(-45), Sucesso = false, TempoInatividade = TimeSpan.FromHours(4) }, // GARGALO CRÍTICO!
                new EventoDeploy { Data = DateTime.Now.AddDays(-90), Sucesso = false, TempoInatividade = TimeSpan.FromHours(6) }, // GARGALO CRÍTICO!
                new EventoDeploy { Data = DateTime.Now.AddDays(-120), Sucesso = true, TempoInatividade = TimeSpan.FromHours(2) }
            };

      return db;
    }
  }
}

// Todas as classes foram movidas para seus próprios arquivos:
// - Contexts/ContextoProjetos.cs
// - Models/BancoDadosProjetos.cs
// - Models/Projeto.cs
// - Models/MetricasVelocidade.cs
// - Models/MetricasQualidade.cs
// - Models/EquipeProjeto.cs
// - Models/EventoDeploy.cs
// - StructuredOutputs/RelatorioAnalise.cs
// - Tools/ProjetosToolPack.cs
