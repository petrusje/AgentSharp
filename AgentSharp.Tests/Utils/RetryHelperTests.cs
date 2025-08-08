using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Utils
{
  [TestClass]
  public class RetryHelperTests
  {
    [TestMethod]
    public async Task ExecuteWithRetryAsync_SuccessfulOperation_ReturnsResult()
    {
      // Arrange
      Func<Task<int>> operation = () => Task.FromResult(42);

      // Act
      var result = await RetryHelper.ExecuteWithRetryAsync(operation);

      // Assert
      Assert.AreEqual(42, result);
    }

    //[Fact]
    //public async Task ExecuteWithRetryAsync_FailsFirstAttemptThenSucceeds_ReturnsResult()
    //{
    //  // Arrange
    //  int attempts = 0;
    //  Func<Task<int>> operation = () =>
    //  {
    //    attempts++;
    //    if (attempts == 1)
    //      throw new Exception("First attempt failed");

    //    return Task.FromResult(42);
    //  };

    //  // Act
    //  var result = await RetryHelper.ExecuteWithRetryAsync(operation);

    //  // Assert
    //  Assert.Equal(42, result);
    //  Assert.Equal(2, attempts);
    //}

    //[Fact]
    //public async Task ExecuteWithRetryAsync_ExceedsMaxRetries_ThrowsException()
    //{
    //  // Arrange
    //  int attempts = 0;
    //  Func<Task<int>> operation = () =>
    //  {
    //    attempts++;
    //    throw new Exception($"Attempt {attempts} failed");
    //  };

    //  // Act & Assert
    //  await Assert.ThrowsAsync<AgentSharp.Exceptions.ModelException>(() =>
    //      RetryHelper.ExecuteWithRetryAsync(operation, retryCount: 2));

    //  Assert.Equal(3, attempts); // Initial attempt + 2 retries
    //}
  }
}
