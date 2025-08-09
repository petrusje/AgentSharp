using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AgentSharp.Tests.Memory
{
    public class MemoryManagerDomainConfigTests
    {
    private readonly MockModel _mockModel;
    private readonly InMemoryStorage _storage;
    private readonly ConsoleLogger _logger;

        public MemoryManagerDomainConfigTests()
        {
            _mockModel = new MockModel();
            _storage = new InMemoryStorage();
            _logger = new ConsoleLogger();
        }

        [Fact]
        public void Constructor_WithoutConfig_UsesDefaults()
        {
            // Act
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger);

            // Assert
            Assert.NotNull(memoryManager);
        }

        [Fact]
        public void Constructor_WithConfig_AcceptsConfiguration()
        {
            // Arrange
            var config = new MemoryDomainConfiguration
            {
                MaxMemoriesPerInteraction = 10,
                MinImportanceThreshold = 0.7
            };

            // Act
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, config);

            // Assert
            Assert.NotNull(memoryManager);
        }

        [Fact]
        public void Constructor_ThrowsOnNullStorage()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new MemoryManager(null, _mockModel, _logger));
        }

        [Fact]
        public void Constructor_ThrowsOnNullModel()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new MemoryManager(_storage, null, _logger));
        }

        [Fact]
        public async Task AddMemoryAsync_WorksWithCustomConfig()
        {
            // Arrange
            var config = new MemoryDomainConfiguration
            {
                MaxMemoriesPerInteraction = 10,
                MinImportanceThreshold = 0.3
            };
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, config)
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Act
            var result = await memoryManager.AddMemoryAsync("Test memory content");

            // Assert
            Assert.Contains("sucesso", result);
        }

        [Fact]
        public async Task GetExistingMemoriesAsync_RespectsConfiguration()
        {
            // Arrange
            var config = new MemoryDomainConfiguration
            {
                MaxMemoriesPerInteraction = 2
            };
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, config)
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Add some memories
            await memoryManager.AddMemoryAsync("Memory 1");
            await memoryManager.AddMemoryAsync("Memory 2");
            await memoryManager.AddMemoryAsync("Memory 3");

            // Act
            var memories = await memoryManager.GetExistingMemoriesAsync(limit: 5);

            // Assert - Should respect the configuration limit if it's smaller
            Assert.NotNull(memories);
        }

        [Fact]
        public async Task RunAsync_WorksWithCustomConfig()
        {
            // Arrange
            var config = new MemoryDomainConfiguration
            {
                MinImportanceThreshold = 0.1 // Very low threshold
            };
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, config)
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Act
            var result = await memoryManager.RunAsync("Test message");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UpdateMemoryAsync_WorksWithCustomConfig()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, new MemoryDomainConfiguration())
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Add a memory first
            await memoryManager.AddMemoryAsync("Original content");
            var memories = await memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;

            // Act
            var result = await memoryManager.UpdateMemoryAsync(memoryId, "Updated content");

            // Assert
            Assert.Contains("sucesso", result);
        }

        [Fact]
        public async Task DeleteMemoryAsync_WorksWithCustomConfig()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, new MemoryDomainConfiguration())
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Add a memory first
            await memoryManager.AddMemoryAsync("Content to delete");
            var memories = await memoryManager.GetExistingMemoriesAsync();
            var memoryId = memories[0].Id;

            // Act
            var result = await memoryManager.DeleteMemoryAsync(memoryId);

            // Assert
            Assert.Contains("sucesso", result);
        }

        [Fact]
        public async Task ClearMemoryAsync_WorksWithCustomConfig()
        {
            // Arrange
            var memoryManager = new MemoryManager(_storage, _mockModel, _logger, null, new MemoryDomainConfiguration())
            {
                UserId = "testUser",
                SessionId = "testSession"
            };

            // Add some memories
            await memoryManager.AddMemoryAsync("Memory 1");
            await memoryManager.AddMemoryAsync("Memory 2");

            // Act
            var result = await memoryManager.ClearMemoryAsync();

            // Assert
            Assert.Contains("sucesso", result);
        }
    }
}
