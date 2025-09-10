using Arcana.AgentsNet.Examples;
using Arcana.AgentsNet.Examples.DTOs;
using Arcana.AgentsNet.Examples.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Arcana.AgentsNet.Tests.Examples
{
    /// <summary>
    /// Testes para a funcionalidade de análise de commits
    /// </summary>
    public class CommitAnalysisTests
    {
        [Fact]
        public void CommitAnalysisService_AnalyzeCommits_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var service = new CommitAnalysisService();
            var emptyCommits = new List<GitCommitInfo>();

            // Act
            var result = service.AnalyzeCommits(emptyCommits, "test-repo");

            // Assert
            Assert.Equal("test-repo", result.Repository);
            Assert.Equal(0, result.Statistics.TotalCommits);
            Assert.Equal(0, result.Statistics.UniqueAuthors);
            Assert.Single(result.Insights);
            Assert.Contains("Nenhum commit encontrado", result.Insights[0]);
        }

        [Fact]
        public void CommitAnalysisService_AnalyzeCommits_WithSampleData_ReturnsCorrectStatistics()
        {
            // Arrange
            var service = new CommitAnalysisService();
            var commits = new List<GitCommitInfo>
            {
                new GitCommitInfo
                {
                    Sha = "abc123",
                    Message = "feat: Add new feature",
                    Author = "Developer1",
                    Date = new DateTime(2025, 1, 1),
                    FilesChanged = 5,
                    Additions = 100,
                    Deletions = 10
                },
                new GitCommitInfo
                {
                    Sha = "def456",
                    Message = "fix: Bug correction",
                    Author = "Developer2",
                    Date = new DateTime(2025, 1, 2),
                    FilesChanged = 2,
                    Additions = 50,
                    Deletions = 5
                },
                new GitCommitInfo
                {
                    Sha = "ghi789",
                    Message = "docs: Update documentation",
                    Author = "Developer1",
                    Date = new DateTime(2025, 1, 3),
                    FilesChanged = 1,
                    Additions = 20,
                    Deletions = 0
                }
            };

            // Act
            var result = service.AnalyzeCommits(commits, "test-repo");

            // Assert
            Assert.Equal("test-repo", result.Repository);
            Assert.Equal(3, result.Statistics.TotalCommits);
            Assert.Equal(2, result.Statistics.UniqueAuthors);
            Assert.Equal(8, result.Statistics.TotalFilesChanged);
            Assert.Equal(170, result.Statistics.TotalAdditions);
            Assert.Equal(15, result.Statistics.TotalDeletions);
            
            // Verificar tipos de commit
            Assert.Contains("feat", result.Statistics.CommitTypes.Keys);
            Assert.Contains("fix", result.Statistics.CommitTypes.Keys);
            Assert.Contains("docs", result.Statistics.CommitTypes.Keys);
            
            // Verificar contribuidores
            Assert.Equal(2, result.Statistics.TopContributors.Count);
            var topContributor = result.Statistics.TopContributors[0];
            Assert.Equal("Developer1", topContributor.Author);
            Assert.Equal(2, topContributor.CommitCount);
            
            // Verificar insights gerados
            Assert.NotEmpty(result.Insights);
            Assert.True(result.Insights.Count >= 3); // Deve ter pelo menos 3 insights
        }

        [Fact]
        public async Task GitCommitAnalysisExample_ExecutarAnaliseCommits_DoesNotThrow()
        {
            // Act & Assert - Verifica se a execução não gera exceções
            var exception = await Record.ExceptionAsync(async () => 
                await GitCommitAnalysisExample.ExecutarAnaliseCommits());

            Assert.Null(exception);
        }

        [Theory]
        [InlineData("feat: Add new feature", "feat")]
        [InlineData("fix: Bug correction", "fix")]
        [InlineData("docs: Update documentation", "docs")]
        [InlineData("chore: Update dependencies", "chore")]
        [InlineData("Add new feature without prefix", "feat")]
        [InlineData("Fix bug in login", "fix")]
        [InlineData("Update readme file", "docs")]
        [InlineData("Random commit message", "other")]
        public void CommitAnalysisService_ExtractCommitType_ReturnsCorrectType(string message, string expectedType)
        {
            // Arrange
            var service = new CommitAnalysisService();
            var commit = new GitCommitInfo { Message = message };

            // Act
            var result = service.AnalyzeCommits(new List<GitCommitInfo> { commit }, "test");

            // Assert
            Assert.Contains(expectedType, result.Statistics.CommitTypes.Keys);
        }
    }
}