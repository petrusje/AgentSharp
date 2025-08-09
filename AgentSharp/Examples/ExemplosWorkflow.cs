// NOTA: Todas as classes deste arquivo foram movidas para seus próprios arquivos nas pastas correspondentes.
using AgentSharp.Core.Orchestration;
using AgentSharp.Examples.Agents;
using AgentSharp.Examples.Contexts;
using AgentSharp.Models;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
  /// <summary>
  /// Exemplos de Workflows - funcionalidade avançada do AgentSharp
  /// Demonstra o sistema de workflow sofisticado com sessões persistentes
  /// </summary>
  public static class ExemplosWorkflow
  {
    /// <summary>
    /// Exemplo de workflow completo de pesquisa, análise e relatório
    /// Demonstra o uso do AdvancedWorkflow com encadeamento de entrada e saída
    /// </summary>
    public static async Task ExecutarWorkflowCompleto(IModel modelo)
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("🔄 NÍVEL 3 - WORKFLOWS MULTI-AGENTE: Orquestração");
      Console.WriteLine("════════════════════════════════════════════════════");
      Console.ResetColor();

      Console.WriteLine("📚 CONCEITOS DEMONSTRADOS:");
      Console.WriteLine("   • AdvancedWorkflow - orquestração multi-agente");
      Console.WriteLine("   • RegisterStep() - definição de etapas");
      Console.WriteLine("   • Context chaining - passagem de dados entre etapas");
      Console.WriteLine("   • Session management - controle de sessão");
      Console.WriteLine("   • Metrics e telemetria");
      Console.WriteLine("   • Debug mode para desenvolvimento\n");

      try
      {
        // Contexto do workflow
        var contextoWorkflow = new ContextoPesquisa
        {
          TopicoPesquisa = "Inteligência Artificial no Brasil",
          ProfundidadeAnalise = "Detalhada",
          PublicoAlvo = "Executivos de tecnologia"
        };

        // Criar agentes especializados
        var agentePesquisador = new AgentePesquisador(modelo);
        var agenteAnalista = new AgenteAnalista(modelo);
        var agenteEscritor = new AgenteEscritor(modelo);
        var agenteRevisor = new AgenteRevisor(modelo);

        // Criar workflow avançado com encadeamento de entrada e saída
        var workflow = new AdvancedWorkflow<ContextoPesquisa, string>("Workflow de Pesquisa IA Brasil")
            .WithDebugMode(true)
            .WithTelemetry(true)
            .ForTask(ctx => $"Criar um relatório executivo completo sobre {ctx.TopicoPesquisa} com análise {ctx.ProfundidadeAnalise} para {ctx.PublicoAlvo}")
            .RegisterStep("Pesquisa e Coleta de Dados", agentePesquisador,
                ctx => $"Pesquise informações sobre o estado atual da Inteligência Artificial no Brasil, incluindo startups, investimentos, políticas públicas e tendências. Foque em {ctx.ProfundidadeAnalise} análise.",
                (ctx, resultado) =>
                {
                  ctx.DadosPesquisa = resultado;
                  Console.WriteLine($"✅ Pesquisa concluída - {resultado.Length} caracteres coletados");
                })
            .RegisterStep("Análise Estratégica", agenteAnalista,
                ctx => $"Analise os seguintes dados sobre IA no Brasil e identifique principais tendências, oportunidades e desafios:\n\n{ctx.DadosPesquisa}",
                (ctx, resultado) =>
                {
                  ctx.AnaliseEstrategica = resultado;
                  Console.WriteLine($"✅ Análise concluída - Insights estratégicos gerados");
                })
            .RegisterStep("Geração de Relatório", agenteEscritor,
                ctx => $@"Com base na pesquisa e análise realizadas, crie um relatório executivo sobre IA no Brasil:

DADOS DA PESQUISA:
{ctx.DadosPesquisa?.Substring(0, Math.Min(1000, ctx.DadosPesquisa?.Length ?? 0))}...

ANÁLISE ESTRATÉGICA:
{ctx.AnaliseEstrategica?.Substring(0, Math.Min(1000, ctx.AnaliseEstrategica?.Length ?? 0))}...

Público-alvo: {ctx.PublicoAlvo}",
                (ctx, resultado) =>
                {
                  ctx.RelatorioFinal = resultado;
                  Console.WriteLine($"✅ Relatório gerado");
                })
            .RegisterStep("Revisão e Finalização", agenteRevisor,
                ctx => $@"Revise e aprimore este relatório para garantir qualidade executiva:

{ctx.RelatorioFinal}

Foque em: clareza, objetividade, insights acionáveis para {ctx.PublicoAlvo}",
                (ctx, resultado) =>
                {
                  ctx.RelatorioRevisado = resultado;
                  Console.WriteLine($"✅ Revisão concluída");
                });

        Console.WriteLine($"🔧 Workflow criado com ID: {workflow.WorkflowId}");
        Console.WriteLine($"📋 Sessão criada com ID: {workflow.SessionId}");
        Console.WriteLine($"🐛 Modo debug: {workflow.DebugMode}");
        Console.WriteLine();

        // Executar o workflow
        Console.WriteLine("🚀 Iniciando execução do workflow...\n");
        var resultadoFinal = await workflow.ExecuteAsync(contextoWorkflow);

        // Exibir resultado final
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n📋 RELATÓRIO FINAL:");
        Console.WriteLine(new string('═', 60));
        Console.WriteLine(resultadoFinal.RelatorioRevisado ?? "Relatório não disponível");
        Console.WriteLine(new string('═', 60));
        Console.ResetColor();

        // Obter métricas do workflow
        var metrics = workflow.GetMetrics();
        Console.WriteLine($"\n📊 ESTATÍSTICAS DO WORKFLOW:");
        Console.WriteLine($"   🔍 Etapas executadas: {workflow.Steps.Count}");
        Console.WriteLine($"   📊 Total de execuções: {metrics.TotalRuns}");
        Console.WriteLine($"   ✅ Execuções bem-sucedidas: {metrics.SuccessfulRuns}");
        Console.WriteLine($"   ❌ Execuções falhadas: {metrics.FailedRuns}");
        Console.WriteLine($"   📈 Taxa de sucesso: {metrics.SuccessRate:P2}");
        Console.WriteLine($"   ⏱️  Tempo médio de execução: {metrics.AverageExecutionTime?.TotalSeconds:F2}s");
        Console.WriteLine($"   🎯 Tópico: {contextoWorkflow.TopicoPesquisa}");

        // Obter snapshot da sessão
        var sessionSnapshot = workflow.GetSessionSnapshot();
        Console.WriteLine($"\n🗂️  INFORMAÇÕES DA SESSÃO:");
        Console.WriteLine($"   📝 Nome da sessão: {sessionSnapshot.SessionName}");
        Console.WriteLine($"   🕐 Criada em: {sessionSnapshot.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   🔄 Última atualização: {sessionSnapshot.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   🏃 Execuções na sessão: {sessionSnapshot.Runs.Count}");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro no workflow: {ex.Message}");
        Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
        Console.ResetColor();
      }
    }
  }

  #region Classes de Contexto e Agentes do Workflow

  // Todas as classes foram movidas para seus próprios arquivos:
  // - Contexts/ContextoPesquisa.cs
  // - Agents/AgentePesquisador.cs
  // - Agents/AgenteAnalista.cs
  // - Agents/AgenteEscritor.cs
  // - Agents/AgenteRevisor.cs

  #endregion
}
