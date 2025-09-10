using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Arcana.AgentsNet.Examples.DTOs
{
    /// <summary>
    /// Representa informações básicas de um commit do Git
    /// </summary>
    public class GitCommitInfo
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("commit_type")]
        public string CommitType { get; set; } = string.Empty;

        [JsonPropertyName("files_changed")]
        public int FilesChanged { get; set; }

        [JsonPropertyName("additions")]
        public int Additions { get; set; }

        [JsonPropertyName("deletions")]
        public int Deletions { get; set; }
    }

    /// <summary>
    /// Análise estatística dos commits
    /// </summary>
    public class CommitAnalysisStats
    {
        [JsonPropertyName("total_commits")]
        public int TotalCommits { get; set; }

        [JsonPropertyName("unique_authors")]
        public int UniqueAuthors { get; set; }

        [JsonPropertyName("date_range")]
        public DateRange DateRange { get; set; } = new DateRange();

        [JsonPropertyName("commit_types")]
        public Dictionary<string, int> CommitTypes { get; set; } = new Dictionary<string, int>();

        [JsonPropertyName("top_contributors")]
        public List<ContributorStats> TopContributors { get; set; } = new List<ContributorStats>();

        [JsonPropertyName("activity_by_month")]
        public Dictionary<string, int> ActivityByMonth { get; set; } = new Dictionary<string, int>();

        [JsonPropertyName("total_files_changed")]
        public int TotalFilesChanged { get; set; }

        [JsonPropertyName("total_additions")]
        public int TotalAdditions { get; set; }

        [JsonPropertyName("total_deletions")]
        public int TotalDeletions { get; set; }
    }

    /// <summary>
    /// Estatísticas de um contribuidor
    /// </summary>
    public class ContributorStats
    {
        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("commit_count")]
        public int CommitCount { get; set; }

        [JsonPropertyName("first_commit")]
        public DateTime FirstCommit { get; set; }

        [JsonPropertyName("last_commit")]
        public DateTime LastCommit { get; set; }

        [JsonPropertyName("total_additions")]
        public int TotalAdditions { get; set; }

        [JsonPropertyName("total_deletions")]
        public int TotalDeletions { get; set; }
    }

    /// <summary>
    /// Intervalo de datas
    /// </summary>
    public class DateRange
    {
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("duration_days")]
        public int DurationDays => (End - Start).Days;
    }

    /// <summary>
    /// Análise completa dos commits
    /// </summary>
    public class GitCommitAnalysisResult
    {
        [JsonPropertyName("repository")]
        public string Repository { get; set; } = string.Empty;

        [JsonPropertyName("analysis_date")]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("statistics")]
        public CommitAnalysisStats Statistics { get; set; } = new CommitAnalysisStats();

        [JsonPropertyName("recent_commits")]
        public List<GitCommitInfo> RecentCommits { get; set; } = new List<GitCommitInfo>();

        [JsonPropertyName("insights")]
        public List<string> Insights { get; set; } = new List<string>();
    }
}