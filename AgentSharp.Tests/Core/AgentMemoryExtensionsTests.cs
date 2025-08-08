using AgentSharp.Core;
using AgentSharp.Models;
using AgentSharp.Tests.Mocks;
using System;
using Xunit;

namespace AgentSharp.Tests.Core
{
    public class AgentMemoryExtensionsTests
    {
        private readonly MockModel _mockModel;
        private readonly Agent<TestContext, string> _agent;

        public AgentMemoryExtensionsTests()
        {
            _mockModel = new MockModel();
            _agent = new Agent<TestContext, string>(_mockModel, "TestAgent");
        }

        [Fact]
        public void WithMemoryDomainConfiguration_SetsConfiguration()
        {
            // Arrange
            var config = new MemoryDomainConfiguration
            {
                MaxMemoriesPerInteraction = 10,
                MinImportanceThreshold = 0.7
            };

            // Act
            var result = _agent.WithMemoryDomainConfiguration(config);

            // Assert
            Assert.Same(_agent, result);
            var retrievedConfig = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(retrievedConfig);
            Assert.Equal(10, retrievedConfig.MaxMemoriesPerInteraction);
            Assert.Equal(0.7, retrievedConfig.MinImportanceThreshold);
        }

        [Fact]
        public void WithMemoryDomainConfiguration_ThrowsOnNullAgent()
        {
            // Arrange
            Agent<TestContext, string> nullAgent = null;
            var config = new MemoryDomainConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullAgent.WithMemoryDomainConfiguration(config));
        }

        [Fact]
        public void WithMemoryDomainConfiguration_ThrowsOnNullConfig()
        {
            // Arrange
            MemoryDomainConfiguration nullConfig = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _agent.WithMemoryDomainConfiguration(nullConfig));
        }

        [Fact]
        public void WithMemoryExtraction_SetsExtractionTemplate()
        {
            // Arrange
            Func<string, string, string> template = (user, assistant) => $"Custom: {user} -> {assistant}";

            // Act
            var result = _agent.WithMemoryExtraction(template);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(template, config.ExtractionPromptTemplate);
        }

        [Fact]
        public void WithMemoryExtraction_ThrowsOnNullTemplate()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _agent.WithMemoryExtraction(null));
        }

        [Fact]
        public void WithMemoryClassification_SetsClassificationTemplate()
        {
            // Arrange
            Func<string, string> template = content => $"Classify: {content}";

            // Act
            var result = _agent.WithMemoryClassification(template);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(template, config.ClassificationPromptTemplate);
        }

        [Fact]
        public void WithMemoryRetrieval_SetsRetrievalTemplate()
        {
            // Arrange
            Func<string, string, string> template = (query, memories) => $"Retrieve: {query} from {memories}";

            // Act
            var result = _agent.WithMemoryRetrieval(template);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(template, config.RetrievalPromptTemplate);
        }

        [Fact]
        public void WithMemoryCategories_SetsCustomCategories()
        {
            // Arrange
            var categories = new[] { "Medical", "Legal", "Technical" };

            // Act
            var result = _agent.WithMemoryCategories(categories);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(categories, config.CustomCategories);
        }

        [Fact]
        public void WithMemoryCategories_ThrowsOnEmptyCategories()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _agent.WithMemoryCategories());
            Assert.Throws<ArgumentException>(() => _agent.WithMemoryCategories(null));
        }

        [Fact]
        public void WithMemoryThresholds_SetsThresholds()
        {
            // Act
            var result = _agent.WithMemoryThresholds(maxMemories: 15, minImportance: 0.8);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(15, config.MaxMemoriesPerInteraction);
            Assert.Equal(0.8, config.MinImportanceThreshold);
        }

        [Fact]
        public void WithMemoryThresholds_ThrowsOnInvalidValues()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _agent.WithMemoryThresholds(maxMemories: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _agent.WithMemoryThresholds(maxMemories: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _agent.WithMemoryThresholds(minImportance: -0.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _agent.WithMemoryThresholds(minImportance: 1.1));
        }

        [Fact]
        public void WithMemoryStrategies_SetsStrategies()
        {
            // Act
            var result = _agent.WithMemoryStrategies(prioritizeRecent: false, enableSemanticGrouping: true);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.False(config.PrioritizeRecentMemories);
            Assert.True(config.EnableSemanticGrouping);
        }

        [Fact]
        public void ChainedCalls_Work()
        {
            // Act
            var result = _agent
                .WithMemoryCategories("Medical", "Legal")
                .WithMemoryThresholds(maxMemories: 20, minImportance: 0.9)
                .WithMemoryStrategies(prioritizeRecent: false, enableSemanticGrouping: true);

            // Assert
            Assert.Same(_agent, result);
            var config = _agent.GetMemoryDomainConfiguration();
            Assert.NotNull(config);
            Assert.Equal(new[] { "Medical", "Legal" }, config.CustomCategories);
            Assert.Equal(20, config.MaxMemoriesPerInteraction);
            Assert.Equal(0.9, config.MinImportanceThreshold);
            Assert.False(config.PrioritizeRecentMemories);
            Assert.True(config.EnableSemanticGrouping);
        }

        public class TestContext
        {
            public string UserId { get; set; }
            public string SessionId { get; set; }
        }
    }
}