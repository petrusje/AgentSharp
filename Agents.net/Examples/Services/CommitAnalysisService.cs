using Arcana.AgentsNet.Examples.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Examples.Services
{
    /// <summary>
    /// Serviço para análise de commits de repositórios Git
    /// </summary>
    public class CommitAnalysisService
    {
        private readonly Regex _commitTypeRegex = new Regex(@"^(feat|fix|docs|style|refactor|test|chore|perf|ci|build|revert)(\(.+\))?:\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Analisa uma lista de commits e gera estatísticas detalhadas
        /// </summary>
        /// <param name="commits">Lista de commits a serem analisados</param>
        /// <param name="repositoryName">Nome do repositório</param>
        /// <returns>Resultado da análise dos commits</returns>
        public GitCommitAnalysisResult AnalyzeCommits(List<GitCommitInfo> commits, string repositoryName = "")
        {
            if (commits == null || !commits.Any())
            {
                return new GitCommitAnalysisResult
                {
                    Repository = repositoryName,
                    Statistics = new CommitAnalysisStats(),
                    Insights = new List<string> { "⚠️ Nenhum commit encontrado para análise" }
                };
            }

            var result = new GitCommitAnalysisResult
            {
                Repository = repositoryName,
                AnalysisDate = DateTime.UtcNow,
                RecentCommits = commits.OrderByDescending(c => c.Date).Take(10).ToList()
            };

            // Calcular estatísticas básicas
            result.Statistics.TotalCommits = commits.Count;
            result.Statistics.UniqueAuthors = commits.Select(c => c.Author).Distinct().Count();

            if (commits.Any())
            {
                result.Statistics.DateRange.Start = commits.Min(c => c.Date);
                result.Statistics.DateRange.End = commits.Max(c => c.Date);
            }

            // Analisar tipos de commit
            result.Statistics.CommitTypes = AnalyzeCommitTypes(commits);

            // Analisar contribuidores
            result.Statistics.TopContributors = AnalyzeContributors(commits)
                .OrderByDescending(c => c.CommitCount)
                .Take(10)
                .ToList();

            // Analisar atividade por mês
            result.Statistics.ActivityByMonth = AnalyzeActivityByMonth(commits);

            // Calcular estatísticas de arquivos
            result.Statistics.TotalFilesChanged = commits.Sum(c => c.FilesChanged);
            result.Statistics.TotalAdditions = commits.Sum(c => c.Additions);
            result.Statistics.TotalDeletions = commits.Sum(c => c.Deletions);

            // Gerar insights
            result.Insights = GenerateInsights(result.Statistics, commits);

            return result;
        }

        /// <summary>
        /// Analisa os tipos de commit baseado em conventional commits
        /// </summary>
        private Dictionary<string, int> AnalyzeCommitTypes(List<GitCommitInfo> commits)
        {
            var commitTypes = new Dictionary<string, int>();

            foreach (var commit in commits)
            {
                var commitType = ExtractCommitType(commit.Message);
                if (!commitTypes.ContainsKey(commitType))
                    commitTypes[commitType] = 0;
                commitTypes[commitType]++;
            }

            return commitTypes.OrderByDescending(kvp => kvp.Value)
                             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Extrai o tipo de commit da mensagem
        /// </summary>
        private string ExtractCommitType(string commitMessage)
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
                return "other";

            var match = _commitTypeRegex.Match(commitMessage);
            if (match.Success)
            {
                return match.Groups[1].Value.ToLowerInvariant();
            }

            // Identificar padrões comuns não convencionais
            var lowerMessage = commitMessage.ToLowerInvariant();
            
            if (lowerMessage.Contains("add") || lowerMessage.Contains("new"))
                return "feat";
            if (lowerMessage.Contains("fix") || lowerMessage.Contains("bug") || lowerMessage.Contains("erro"))
                return "fix";
            if (lowerMessage.Contains("feat") || lowerMessage.Contains("feature"))
                return "feat";
            if (lowerMessage.Contains("doc") || lowerMessage.Contains("readme"))
                return "docs";
            if (lowerMessage.Contains("test") || lowerMessage.Contains("spec"))
                return "test";
            if (lowerMessage.Contains("refactor") || lowerMessage.Contains("cleanup"))
                return "refactor";
            if (lowerMessage.Contains("merge"))
                return "merge";
            if (lowerMessage.Contains("update") || lowerMessage.Contains("upgrade"))
                return "update";

            return "other";
        }

        /// <summary>
        /// Analisa estatísticas dos contribuidores
        /// </summary>
        private List<ContributorStats> AnalyzeContributors(List<GitCommitInfo> commits)
        {
            return commits.GroupBy(c => c.Author)
                         .Select(g => new ContributorStats
                         {
                             Author = g.Key,
                             CommitCount = g.Count(),
                             FirstCommit = g.Min(c => c.Date),
                             LastCommit = g.Max(c => c.Date),
                             TotalAdditions = g.Sum(c => c.Additions),
                             TotalDeletions = g.Sum(c => c.Deletions)
                         })
                         .ToList();
        }

        /// <summary>
        /// Analisa atividade por mês
        /// </summary>
        private Dictionary<string, int> AnalyzeActivityByMonth(List<GitCommitInfo> commits)
        {
            return commits.GroupBy(c => c.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture))
                         .OrderBy(g => g.Key)
                         .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Gera insights baseados nas estatísticas
        /// </summary>
        private List<string> GenerateInsights(CommitAnalysisStats stats, List<GitCommitInfo> commits)
        {
            var insights = new List<string>();

            // Insight sobre período de desenvolvimento
            if (stats.DateRange.DurationDays > 0)
            {
                var avgCommitsPerDay = (double)stats.TotalCommits / stats.DateRange.DurationDays;
                insights.Add($"📅 Período de desenvolvimento: {stats.DateRange.DurationDays} dias com média de {avgCommitsPerDay:F2} commits por dia");
            }

            // Insight sobre tipos de commit mais comuns
            if (stats.CommitTypes.Any())
            {
                var topCommitType = stats.CommitTypes.First();
                var percentage = (double)topCommitType.Value / stats.TotalCommits * 100;
                insights.Add($"🏷️ Tipo de commit mais comum: '{topCommitType.Key}' ({percentage:F1}% dos commits)");
            }

            // Insight sobre colaboração
            if (stats.UniqueAuthors > 1)
            {
                insights.Add($"👥 Projeto colaborativo com {stats.UniqueAuthors} contribuidores únicos");
                
                if (stats.TopContributors.Any())
                {
                    var topContributor = stats.TopContributors.First();
                    var contributorPercentage = (double)topContributor.CommitCount / stats.TotalCommits * 100;
                    insights.Add($"👑 Principal contribuidor: {topContributor.Author} ({contributorPercentage:F1}% dos commits)");
                }
            }

            // Insight sobre atividade
            if (stats.ActivityByMonth.Any())
            {
                var mostActiveMonth = stats.ActivityByMonth.OrderByDescending(kvp => kvp.Value).First();
                insights.Add($"📈 Mês mais ativo: {mostActiveMonth.Key} com {mostActiveMonth.Value} commits");
            }

            // Insight sobre mudanças no código
            if (stats.TotalAdditions > 0 || stats.TotalDeletions > 0)
            {
                var netChanges = stats.TotalAdditions - stats.TotalDeletions;
                var changeRatio = stats.TotalDeletions > 0 ? (double)stats.TotalAdditions / stats.TotalDeletions : double.PositiveInfinity;
                
                if (netChanges > 0)
                {
                    insights.Add($"📊 Crescimento do código: +{stats.TotalAdditions} linhas adicionadas, -{stats.TotalDeletions} removidas (crescimento líquido de +{netChanges} linhas)");
                }
                else if (netChanges < 0)
                {
                    insights.Add($"📊 Redução do código: +{stats.TotalAdditions} linhas adicionadas, -{stats.TotalDeletions} removidas (redução líquida de {Math.Abs(netChanges)} linhas)");
                }

                if (changeRatio < 0.5)
                {
                    insights.Add("🔧 Projeto em fase de refatoração/limpeza (mais linhas removidas que adicionadas)");
                }
                else if (changeRatio > 2)
                {
                    insights.Add("🚀 Projeto em fase de expansão ativa (muito mais linhas adicionadas que removidas)");
                }
            }

            // Insight sobre frequência de commits recentes
            var recentCommits = commits.Where(c => c.Date > DateTime.UtcNow.AddDays(-30)).Count();
            if (recentCommits > 0)
            {
                insights.Add($"⚡ Atividade recente: {recentCommits} commits nos últimos 30 dias");
            }
            else
            {
                insights.Add("😴 Nenhuma atividade nos últimos 30 dias");
            }

            return insights;
        }

        /// <summary>
        /// Converte dados do GitHub API para GitCommitInfo
        /// </summary>
        public List<GitCommitInfo> ConvertFromGitHubData(string jsonData)
        {
            try
            {
                var gitHubCommits = JsonSerializer.Deserialize<List<GitHubCommitData>>(jsonData);
                return gitHubCommits?.Select(ConvertToGitCommitInfo).ToList() ?? new List<GitCommitInfo>();
            }
            catch (JsonException)
            {
                return new List<GitCommitInfo>();
            }
        }

        /// <summary>
        /// Converte um item do GitHub API para GitCommitInfo
        /// </summary>
        private GitCommitInfo ConvertToGitCommitInfo(GitHubCommitData githubCommit)
        {
            return new GitCommitInfo
            {
                Sha = githubCommit.Sha ?? string.Empty,
                Message = githubCommit.Commit?.Message ?? string.Empty,
                Author = githubCommit.Commit?.Author?.Name ?? githubCommit.Author?.Login ?? "Unknown",
                Date = githubCommit.Commit?.Author?.Date ?? DateTime.MinValue,
                HtmlUrl = githubCommit.HtmlUrl ?? string.Empty,
                CommitType = ExtractCommitType(githubCommit.Commit?.Message ?? string.Empty),
                FilesChanged = githubCommit.Stats?.Total ?? 0,
                Additions = githubCommit.Stats?.Additions ?? 0,
                Deletions = githubCommit.Stats?.Deletions ?? 0
            };
        }

        // Classes auxiliares para deserialização do GitHub API
        private class GitHubCommitData
        {
            [JsonPropertyName("sha")]
            public string Sha { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; }

            [JsonPropertyName("commit")]
            public GitHubCommit Commit { get; set; }

            [JsonPropertyName("author")]
            public GitHubUser Author { get; set; }

            [JsonPropertyName("stats")]
            public GitHubStats Stats { get; set; }
        }

        private class GitHubCommit
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("author")]
            public GitHubCommitAuthor Author { get; set; }
        }

        private class GitHubCommitAuthor
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("date")]
            public DateTime Date { get; set; }
        }

        private class GitHubUser
        {
            [JsonPropertyName("login")]
            public string Login { get; set; }
        }

        private class GitHubStats
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("additions")]
            public int Additions { get; set; }

            [JsonPropertyName("deletions")]
            public int Deletions { get; set; }
        }
    }
}