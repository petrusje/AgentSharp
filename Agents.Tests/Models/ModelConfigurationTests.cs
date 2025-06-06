using Agents.net.Models;

namespace Agents.Tests.Models
{
  public class ModelConfigurationTests
  {
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
      // Arrange & Act
      var config = new ModelConfiguration();

      // Assert
      Assert.Equal(0.7, config.Temperature);
      Assert.Equal(2048, config.MaxTokens);
      Assert.Equal(0.0, config.FrequencyPenalty);
      Assert.Equal(0.0, config.PresencePenalty);
      Assert.Equal(1.0, config.TopP);
      Assert.Equal(60, config.TimeoutSeconds);
    }

    [Fact]
    public void Merge_WithNullParameter_ReturnsSameInstance()
    {
      // Arrange
      var config = new ModelConfiguration { Temperature = 0.5 };

      // Act
      var result = config.Merge(null);

      // Assert
      Assert.Equal(config.Temperature, result.Temperature);
      Assert.Equal(config.MaxTokens, result.MaxTokens);
    }

    [Fact]
    public void Merge_WithValidParameter_ReturnsCorrectlyMergedInstance()
    {
      // Arrange
      var config1 = new ModelConfiguration
      {
        Temperature = 0.5,
        MaxTokens = 2048
      };

      var config2 = new ModelConfiguration
      {
        Temperature = 0.8,
        TopP = 0.9
      };

      // Act
      var result = config1.Merge(config2);

      // Assert
      Assert.Equal(config2.Temperature, result.Temperature);
      Assert.Equal(config1.MaxTokens, result.MaxTokens);
      Assert.Equal(config2.TopP, result.TopP);
    }
  }
}
