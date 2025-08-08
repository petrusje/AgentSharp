using AgentSharp.Core;
using System;
using Xunit;

namespace AgentSharp.Tests.Core
{
    public class MemoryDomainConfigurationTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Act
            var config = new MemoryDomainConfiguration();

            // Assert
            Assert.Null(config.ExtractionPromptTemplate);
            Assert.Null(config.ClassificationPromptTemplate);
            Assert.Null(config.RetrievalPromptTemplate);
            Assert.Null(config.CustomCategories);
            Assert.Equal(5, config.MaxMemoriesPerInteraction);
            Assert.Equal(0.5, config.MinImportanceThreshold);
            Assert.True(config.PrioritizeRecentMemories);
            Assert.False(config.EnableSemanticGrouping);
        }

        [Fact]
        public void Clone_CreatesExactCopy()
        {
            // Arrange
            var original = new MemoryDomainConfiguration
            {
                ExtractionPromptTemplate = (user, assistant) => "Custom extraction",
                ClassificationPromptTemplate = content => "Custom classification",
                RetrievalPromptTemplate = (query, memories) => "Custom retrieval",
                CustomCategories = new[] { "Category1", "Category2" },
                MaxMemoriesPerInteraction = 10,
                MinImportanceThreshold = 0.7,
                PrioritizeRecentMemories = false,
                EnableSemanticGrouping = true
            };

            // Act
            var clone = original.Clone();

            // Assert
            Assert.NotSame(original, clone);
            Assert.Equal(original.ExtractionPromptTemplate, clone.ExtractionPromptTemplate);
            Assert.Equal(original.ClassificationPromptTemplate, clone.ClassificationPromptTemplate);
            Assert.Equal(original.RetrievalPromptTemplate, clone.RetrievalPromptTemplate);
            Assert.Equal(original.CustomCategories, clone.CustomCategories);
            Assert.NotSame(original.CustomCategories, clone.CustomCategories);
            Assert.Equal(original.MaxMemoriesPerInteraction, clone.MaxMemoriesPerInteraction);
            Assert.Equal(original.MinImportanceThreshold, clone.MinImportanceThreshold);
            Assert.Equal(original.PrioritizeRecentMemories, clone.PrioritizeRecentMemories);
            Assert.Equal(original.EnableSemanticGrouping, clone.EnableSemanticGrouping);
        }

        [Fact]
        public void Clone_HandlesNullCategories()
        {
            // Arrange
            var original = new MemoryDomainConfiguration
            {
                CustomCategories = null
            };

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Null(clone.CustomCategories);
        }

        [Fact]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new MemoryDomainConfiguration();
            Func<string, string, string> extractionTemplate = (user, assistant) => "test";
            Func<string, string> classificationTemplate = content => "test";
            Func<string, string, string> retrievalTemplate = (query, memories) => "test";
            var categories = new[] { "Test1", "Test2" };

            // Act
            config.ExtractionPromptTemplate = extractionTemplate;
            config.ClassificationPromptTemplate = classificationTemplate;
            config.RetrievalPromptTemplate = retrievalTemplate;
            config.CustomCategories = categories;
            config.MaxMemoriesPerInteraction = 15;
            config.MinImportanceThreshold = 0.8;
            config.PrioritizeRecentMemories = false;
            config.EnableSemanticGrouping = true;

            // Assert
            Assert.Equal(extractionTemplate, config.ExtractionPromptTemplate);
            Assert.Equal(classificationTemplate, config.ClassificationPromptTemplate);
            Assert.Equal(retrievalTemplate, config.RetrievalPromptTemplate);
            Assert.Equal(categories, config.CustomCategories);
            Assert.Equal(15, config.MaxMemoriesPerInteraction);
            Assert.Equal(0.8, config.MinImportanceThreshold);
            Assert.False(config.PrioritizeRecentMemories);
            Assert.True(config.EnableSemanticGrouping);
        }
    }
}