using AgentSharp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentSharp.Tests.Models
{
  [TestClass]
  public class ModelConfigurationTests
  {
    [TestMethod]
    public void Constructor_SetsDefaultValues()
    {
      // Arrange & Act
      var config = new ModelConfiguration();

      // Assert
      Assert.AreEqual(0.7, config.Temperature);
      Assert.AreEqual(2048, config.MaxTokens);
      Assert.AreEqual(0.0, config.FrequencyPenalty);
      Assert.AreEqual(0.0, config.PresencePenalty);
      Assert.AreEqual(1.0, config.TopP);
      Assert.AreEqual(60, config.TimeoutSeconds);
    }

    [TestMethod]
    public void Merge_WithNullParameter_ReturnsSameInstance()
    {
      // Arrange
      var config = new ModelConfiguration { Temperature = 0.5 };

      // Act
      var result = config.Merge(null);

      // Assert
      Assert.AreEqual(config.Temperature, result.Temperature);
      Assert.AreEqual(config.MaxTokens, result.MaxTokens);
    }

    [TestMethod]
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
      Assert.AreEqual(config2.Temperature, result.Temperature);
      Assert.AreEqual(config1.MaxTokens, result.MaxTokens);
      Assert.AreEqual(config2.TopP, result.TopP);
    }
  }
}
