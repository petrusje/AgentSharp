using Arcana.AgentsNet.Examples.DTOs;
using Arcana.AgentsNet.Examples.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Examples
{
    /// <summary>
    /// Exemplo de análise de commits do repositório AgentSharp/Agents.net
    /// Demonstra como analisar padrões de desenvolvimento, contribuidores e atividade do projeto
    /// </summary>
    public static class GitCommitAnalysisExample
    {
        /// <summary>
        /// Executa análise dos commits do repositório atual
        /// </summary>
        public static async Task ExecutarAnaliseCommits()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🔍 ANÁLISE DE COMMITS - REPOSITÓRIO AGENTS.NET");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📊 Analisando histórico de commits do repositório...\n");

            try
            {
                // Simular dados de commits do repositório real (baseado no histórico do GitHub)
                var commits = ObterCommitsSimulados();
                
                var commitAnalysisService = new CommitAnalysisService();
                var analise = commitAnalysisService.AnalyzeCommits(commits, "petrusje/AgentSharp");

                ExibirResultadosAnalise(analise);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro durante análise: {ex.Message}");
                Console.ResetColor();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Obtém commits simulados baseados no histórico real do repositório
        /// </summary>
        private static List<GitCommitInfo> ObterCommitsSimulados()
        {
            // Dados baseados no histórico real obtido via GitHub API
            return new List<GitCommitInfo>
            {
                new GitCommitInfo
                {
                    Sha = "c6ba3ae2bec2b42a184fb0a7ce2aa28e57ac6ede",
                    Message = "Att Project (#41)\n\n* feat: Add additional NuGet metadata and support for symbol packages in project file\n\n* fix: Update target framework from net46 to netstandard2.0 in HNSW.Net.Tests project",
                    Author = "Yurinabaia",
                    Date = new DateTime(2025, 8, 18, 18, 24, 44, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/c6ba3ae2bec2b42a184fb0a7ce2aa28e57ac6ede",
                    FilesChanged = 12,
                    Additions = 89,
                    Deletions = 23
                },
                new GitCommitInfo
                {
                    Sha = "4167b8df9cd7dab257111be68633d79f3df7734a",
                    Message = "Delete AgentSharp.console/TestHNSW.zip",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 18, 13, 49, 26, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/4167b8df9cd7dab257111be68633d79f3df7734a",
                    FilesChanged = 1,
                    Additions = 0,
                    Deletions = 1
                },
                new GitCommitInfo
                {
                    Sha = "a6272ced8b4fbb6397d1c876483ceddb64a4a64d",
                    Message = "fix: Update icon for AgentSharp to improve visual consistency",
                    Author = "YuriDuarte",
                    Date = new DateTime(2025, 8, 18, 13, 37, 59, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/a6272ced8b4fbb6397d1c876483ceddb64a4a64d",
                    FilesChanged = 2,
                    Additions = 15,
                    Deletions = 3
                },
                new GitCommitInfo
                {
                    Sha = "d546386d31ad4d59c5819e8942ff134d83277142",
                    Message = "fix: Enhance percentage assertion in TeamChatComponentsTests for culture flexibility",
                    Author = "YuriDuarte",
                    Date = new DateTime(2025, 8, 17, 21, 56, 2, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/d546386d31ad4d59c5819e8942ff134d83277142",
                    FilesChanged = 1,
                    Additions = 8,
                    Deletions = 2
                },
                new GitCommitInfo
                {
                    Sha = "c45de522920f25b8af114ca36fd9650537b66af9",
                    Message = "feat: Downgrade target framework from .NET 9.0 to 8.0 in project file",
                    Author = "YuriDuarte",
                    Date = new DateTime(2025, 8, 17, 21, 49, 57, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/c45de522920f25b8af114ca36fd9650537b66af9",
                    FilesChanged = 3,
                    Additions = 12,
                    Deletions = 8
                },
                new GitCommitInfo
                {
                    Sha = "b7f4227acd3684c8d126cd390b24c3f35a961feb",
                    Message = "feat:\nCreation of TeamChat and\nImplement advanced resource management in workflows",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 16, 16, 51, 7, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/b7f4227acd3684c8d126cd390b24c3f35a961feb",
                    FilesChanged = 45,
                    Additions = 1250,
                    Deletions = 156
                },
                new GitCommitInfo
                {
                    Sha = "646fa4bb2f6d6a4beb33a8cfa1dd1a4143d41aca",
                    Message = "Add comprehensive documentation for AgentSharp tools and installation guide",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 14, 19, 43, 24, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/646fa4bb2f6d6a4beb33a8cfa1dd1a4143d41aca",
                    FilesChanged = 8,
                    Additions = 389,
                    Deletions = 12
                },
                new GitCommitInfo
                {
                    Sha = "f44d7a5faf1fefed7adf3fe12f804a56d8b9a5dd",
                    Message = "Memory and Storage simplified, samples and Storage refactored. tiny telemetry.",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 12, 19, 37, 59, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/f44d7a5faf1fefed7adf3fe12f804a56d8b9a5dd",
                    FilesChanged = 28,
                    Additions = 542,
                    Deletions = 298
                },
                new GitCommitInfo
                {
                    Sha = "901108e695b933139f93e4927006c0b7b2ee9390",
                    Message = "feat: Add examples for OpenAI embedding service and vector storage with SQLite",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 9, 3, 49, 57, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/901108e695b933139f93e4927006c0b7b2ee9390",
                    FilesChanged = 12,
                    Additions = 678,
                    Deletions = 25
                },
                new GitCommitInfo
                {
                    Sha = "5b846bcac55ccfee191f789e5c4eba3fc7f0b77e",
                    Message = "Change Library name - Refactor code structure for improved readability and maintainability",
                    Author = "Petrus",
                    Date = new DateTime(2025, 8, 6, 20, 21, 16, DateTimeKind.Utc),
                    HtmlUrl = "https://github.com/petrusje/AgentSharp/commit/5b846bcac55ccfee191f789e5c4eba3fc7f0b77e",
                    FilesChanged = 67,
                    Additions = 1890,
                    Deletions = 843
                }
            };
        }

        /// <summary>
        /// Exibe os resultados da análise de forma formatada
        /// </summary>
        private static void ExibirResultadosAnalise(GitCommitAnalysisResult analise)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ ANÁLISE CONCLUÍDA");
            Console.WriteLine("═══════════════════");
            Console.ResetColor();

            // Informações básicas
            Console.WriteLine($"📦 Repositório: {analise.Repository}");
            Console.WriteLine($"📅 Data da análise: {analise.AnalysisDate:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine();

            // Estatísticas gerais
            var stats = analise.Statistics;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📊 ESTATÍSTICAS GERAIS");
            Console.WriteLine("═════════════════════");
            Console.ResetColor();

            Console.WriteLine($"• Total de commits: {stats.TotalCommits}");
            Console.WriteLine($"• Contribuidores únicos: {stats.UniqueAuthors}");
            Console.WriteLine($"• Período: {stats.DateRange.Start:yyyy-MM-dd} até {stats.DateRange.End:yyyy-MM-dd} ({stats.DateRange.DurationDays} dias)");
            Console.WriteLine($"• Arquivos alterados: {stats.TotalFilesChanged}");
            Console.WriteLine($"• Linhas adicionadas: +{stats.TotalAdditions}");
            Console.WriteLine($"• Linhas removidas: -{stats.TotalDeletions}");
            Console.WriteLine();

            // Tipos de commit
            if (stats.CommitTypes.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🏷️ TIPOS DE COMMIT");
                Console.WriteLine("══════════════════");
                Console.ResetColor();

                foreach (var tipo in stats.CommitTypes.Take(5))
                {
                    var porcentagem = (double)tipo.Value / stats.TotalCommits * 100;
                    Console.WriteLine($"• {tipo.Key}: {tipo.Value} commits ({porcentagem:F1}%)");
                }
                Console.WriteLine();
            }

            // Principais contribuidores
            if (stats.TopContributors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("👥 PRINCIPAIS CONTRIBUIDORES");
                Console.WriteLine("═══════════════════════════");
                Console.ResetColor();

                foreach (var contribuidor in stats.TopContributors.Take(5))
                {
                    var porcentagem = (double)contribuidor.CommitCount / stats.TotalCommits * 100;
                    Console.WriteLine($"• {contribuidor.Author}: {contribuidor.CommitCount} commits ({porcentagem:F1}%)");
                    Console.WriteLine($"  Período: {contribuidor.FirstCommit:yyyy-MM-dd} até {contribuidor.LastCommit:yyyy-MM-dd}");
                    Console.WriteLine($"  Contribuição: +{contribuidor.TotalAdditions}/-{contribuidor.TotalDeletions} linhas");
                    Console.WriteLine();
                }
            }

            // Atividade por mês
            if (stats.ActivityByMonth.Any())
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("📈 ATIVIDADE POR MÊS");
                Console.WriteLine("═══════════════════");
                Console.ResetColor();

                foreach (var mes in stats.ActivityByMonth.Skip(Math.Max(0, stats.ActivityByMonth.Count - 6)))
                {
                    Console.WriteLine($"• {mes.Key}: {mes.Value} commits");
                }
                Console.WriteLine();
            }

            // Commits recentes
            if (analise.RecentCommits.Any())
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("🕒 COMMITS RECENTES");
                Console.WriteLine("══════════════════");
                Console.ResetColor();

                foreach (var commit in analise.RecentCommits.Take(5))
                {
                    Console.WriteLine($"• {commit.Date:yyyy-MM-dd} - {commit.Author}");
                    Console.WriteLine($"  {TruncateString(commit.Message.Split('\n')[0], 80)}");
                    Console.WriteLine($"  📁 {commit.FilesChanged} arquivos | +{commit.Additions}/-{commit.Deletions} linhas");
                    Console.WriteLine();
                }
            }

            // Insights
            if (analise.Insights.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("💡 INSIGHTS DA ANÁLISE");
                Console.WriteLine("══════════════════════");
                Console.ResetColor();

                foreach (var insight in analise.Insights)
                {
                    Console.WriteLine($"• {insight}");
                }
                Console.WriteLine();
            }

            // JSON para debug (opcional)
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("🔧 JSON da análise disponível para integração:");
            Console.WriteLine(JsonSerializer.Serialize(analise, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }).Substring(0, Math.Min(300, JsonSerializer.Serialize(analise).Length)) + "...");
            Console.ResetColor();
        }

        /// <summary>
        /// Trunca uma string para um tamanho máximo
        /// </summary>
        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }
    }
}