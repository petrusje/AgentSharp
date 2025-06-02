using System;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Core.Orchestration;
using Agents.net.Models;
using Agents.net.Utils;

namespace Agents.net.Examples
{
    /// <summary>
    /// Exemplo demonstrando o uso do AdvancedWorkflow com todas as melhorias implementadas
    /// </summary>
    public class AdvancedWorkflowExample
    {
        public class ResearchContext
        {
            public string Topic { get; set; }
            public string ResearchData { get; set; }
            public string Analysis { get; set; }
            public string FinalReport { get; set; }
            public string UserLanguage { get; set; } = "pt-BR";
        }

        public static async Task RunExample()
        {
            Console.WriteLine("=== Exemplo de AdvancedWorkflow ===\n");

            try
            {
                // Configurar modelo
                var model = new OpenAIModel("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
                var logger = new ConsoleLogger();

                // Criar agentes especializados
                var researchAgent = new Agent<ResearchContext, string>(model, "Pesquisador", logger: logger)
                    .WithInstructions("Você é um pesquisador especializado. Colete informações detalhadas sobre o tópico fornecido.")
                    .WithPersona("Pesquisador acadêmico meticuloso e detalhista");

                var analysisAgent = new Agent<ResearchContext, string>(model, "Analista", logger: logger)
                    .WithInstructions("Você é um analista de dados. Analise as informações coletadas e identifique padrões e insights.")
                    .WithPersona("Analista experiente com foco em insights práticos");

                var reportAgent = new Agent<ResearchContext, string>(model, "Redator", logger: logger)
                    .WithInstructions("Você é um redator técnico. Crie um relatório final bem estruturado.")
                    .WithPersona("Redator técnico com excelente capacidade de síntese");

                // Criar workflow avançado
                var workflow = new AdvancedWorkflow<ResearchContext, string>("Workflow de Pesquisa e Análise", logger)
                    .WithUserId("user123")
                    .WithDebugMode(true)
                    .WithTelemetry(true)
                    .ForTask(ctx => $"Realizar pesquisa completa sobre '{ctx.Topic}' e gerar relatório final em {ctx.UserLanguage}")
                    .RegisterStep(
                        "Pesquisa",
                        researchAgent,
                        ctx => $"Pesquise informações detalhadas sobre: {ctx.Topic}",
                        (ctx, output) => ctx.ResearchData = output
                    )
                    .RegisterStep(
                        "Análise",
                        analysisAgent,
                        ctx => $"Analise os seguintes dados de pesquisa e identifique insights importantes:\n{ctx.ResearchData}",
                        (ctx, output) => ctx.Analysis = output
                    )
                    .RegisterStep(
                        "Relatório",
                        reportAgent,
                        ctx => $"Crie um relatório final bem estruturado baseado na pesquisa e análise:\n\nPesquisa:\n{ctx.ResearchData}\n\nAnálise:\n{ctx.Analysis}",
                        (ctx, output) => ctx.FinalReport = output
                    );

                Console.WriteLine($"Workflow criado com ID: {workflow.WorkflowId}");
                Console.WriteLine($"Sessão criada com ID: {workflow.SessionId}");
                Console.WriteLine($"Modo debug: {workflow.DebugMode}");
                Console.WriteLine();

                // Criar contexto de pesquisa
                var context = new ResearchContext
                {
                    Topic = "Inteligência Artificial Generativa",
                    UserLanguage = "pt-BR"
                };

                Console.WriteLine("=== Executando Workflow ===\n");

                // Executar workflow
                var result = await workflow.ExecuteAsync(context);

                Console.WriteLine("\n=== Resultado Final ===");
                Console.WriteLine($"Tópico: {result.Topic}");
                Console.WriteLine($"Relatório Final:\n{result.FinalReport}");

                // Obter métricas
                var metrics = workflow.GetMetrics();
                Console.WriteLine("\n=== Métricas do Workflow ===");
                Console.WriteLine($"Total de execuções: {metrics.TotalRuns}");
                Console.WriteLine($"Execuções bem-sucedidas: {metrics.SuccessfulRuns}");
                Console.WriteLine($"Execuções falhadas: {metrics.FailedRuns}");
                Console.WriteLine($"Taxa de sucesso: {metrics.SuccessRate:P2}");
                Console.WriteLine($"Tempo médio de execução: {metrics.AverageExecutionTime?.TotalSeconds:F2}s");

                // Obter snapshot da sessão
                var sessionSnapshot = workflow.GetSessionSnapshot();
                Console.WriteLine("\n=== Informações da Sessão ===");
                Console.WriteLine($"ID da Sessão: {sessionSnapshot.SessionId}");
                Console.WriteLine($"Nome da Sessão: {sessionSnapshot.SessionName}");
                Console.WriteLine($"Criada em: {sessionSnapshot.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Última atualização: {sessionSnapshot.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Total de execuções na sessão: {sessionSnapshot.Runs.Count}");

                // Demonstrar reutilização de sessão
                Console.WriteLine("\n=== Executando Segunda Pesquisa na Mesma Sessão ===");
                
                var context2 = new ResearchContext
                {
                    Topic = "Blockchain e Criptomoedas",
                    UserLanguage = "pt-BR"
                };

                var result2 = await workflow.ExecuteAsync(context2);
                
                // Métricas atualizadas
                var updatedMetrics = workflow.GetMetrics();
                Console.WriteLine($"\nMétricas atualizadas - Total de execuções: {updatedMetrics.TotalRuns}");
                Console.WriteLine($"Taxa de sucesso: {updatedMetrics.SuccessRate:P2}");

                // Demonstrar criação de nova sessão
                Console.WriteLine("\n=== Criando Nova Sessão ===");
                workflow.CreateNewSession("Sessão de Testes");
                Console.WriteLine($"Nova sessão criada: {workflow.SessionId}");

                var context3 = new ResearchContext
                {
                    Topic = "Computação Quântica",
                    UserLanguage = "en-US"
                };

                await workflow.ExecuteAsync(context3);
                
                var newSessionMetrics = workflow.GetMetrics();
                Console.WriteLine($"Métricas da nova sessão - Total de execuções: {newSessionMetrics.TotalRuns}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante execução: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Exemplo demonstrando recuperação de falhas e resiliência
        /// </summary>
        public static async Task RunResilienceExample()
        {
            Console.WriteLine("\n=== Exemplo de Resiliência ===\n");

            var model = new OpenAIModel("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            var logger = new ConsoleLogger();

            // Agente que pode falhar
            var unreliableAgent = new Agent<ResearchContext, string>(model, "Agente Instável", logger: logger)
                .WithInstructions("Você pode falhar ocasionalmente para demonstrar resiliência");

            var workflow = new AdvancedWorkflow<ResearchContext, string>("Workflow de Teste de Resiliência", logger)
                .WithDebugMode(true)
                .RegisterStep(
                    "Step Instável",
                    unreliableAgent,
                    ctx => "Execute uma tarefa que pode falhar",
                    (ctx, output) => ctx.ResearchData = output
                );

            try
            {
                var context = new ResearchContext { Topic = "Teste de Falha" };
                await workflow.ExecuteAsync(context);
                
                Console.WriteLine("Execução bem-sucedida!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha capturada: {ex.Message}");
                
                // Verificar métricas após falha
                var metrics = workflow.GetMetrics();
                Console.WriteLine($"Execuções falhadas: {metrics.FailedRuns}");
                Console.WriteLine($"Taxa de sucesso: {metrics.SuccessRate:P2}");
            }
        }
    }
} 