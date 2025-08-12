using System;
using AgentSharp.Core.Orchestration;
using Xunit;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Unit tests for TeamOperationResult and TeamOperationResult<T>
    /// Tests success/error result patterns, data handling, and error management
    /// </summary>
    public class TeamOperationResultTests
    {
        #region Generic TeamOperationResult<T> Tests

        [Fact]
        public void SuccessResult_WithData_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var testData = "Test success data";

            // Act
            var result = TeamOperationResult<string>.SuccessResult(testData);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(testData, result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void SuccessResult_WithNullData_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = TeamOperationResult<string>.SuccessResult(null!);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void ErrorResult_WithMessage_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Test error message";

            // Act
            var result = TeamOperationResult<string>.ErrorResult(errorMessage);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(default(string), result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void ErrorResult_WithMessageAndException_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Test error with exception";
            var exception = new InvalidOperationException("Test exception");

            // Act
            var result = TeamOperationResult<string>.ErrorResult(errorMessage, exception);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(default(string), result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(exception, result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void ErrorResult_WithMessageExceptionAndContext_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Test error with context";
            var exception = new ArgumentException("Test argument exception");
            var context = new { UserId = "user123", Operation = "TeamHandoff" };

            // Act
            var result = TeamOperationResult<string>.ErrorResult(errorMessage, exception, context);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(default(string), result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(exception, result.Exception);
            Assert.Equal(context, result.ErrorContext);
        }

        [Fact]
        public void GetDataOrThrow_WithSuccessResult_ShouldReturnData()
        {
            // Arrange
            var testData = "Success data";
            var result = TeamOperationResult<string>.SuccessResult(testData);

            // Act
            var data = result.GetDataOrThrow();

            // Assert
            Assert.Equal(testData, data);
        }

        [Fact]
        public void GetDataOrThrow_WithErrorResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var errorMessage = "Operation failed";
            var result = TeamOperationResult<string>.ErrorResult(errorMessage);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => result.GetDataOrThrow());
            Assert.Contains(errorMessage, exception.Message);
        }

        [Fact]
        public void GetDataOrThrow_WithErrorResultAndInnerException_ShouldThrowWithInnerException()
        {
            // Arrange
            var errorMessage = "Operation failed";
            var innerException = new ArgumentException("Inner exception");
            var result = TeamOperationResult<string>.ErrorResult(errorMessage, innerException);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => result.GetDataOrThrow());
            Assert.Contains(errorMessage, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        [Fact]
        public void GetDataOrDefault_WithSuccessResult_ShouldReturnData()
        {
            // Arrange
            var testData = "Success data";
            var result = TeamOperationResult<string>.SuccessResult(testData);

            // Act
            var data = result.GetDataOrDefault();

            // Assert
            Assert.Equal(testData, data);
        }

        [Fact]
        public void GetDataOrDefault_WithErrorResult_ShouldReturnDefault()
        {
            // Arrange
            var result = TeamOperationResult<string>.ErrorResult("Error occurred");

            // Act
            var data = result.GetDataOrDefault();

            // Assert
            Assert.Equal(default(string), data);
        }

        [Fact]
        public void GetDataOrDefault_WithErrorResultAndCustomDefault_ShouldReturnCustomDefault()
        {
            // Arrange
            var customDefault = "Custom default value";
            var result = TeamOperationResult<string>.ErrorResult("Error occurred");

            // Act
            var data = result.GetDataOrDefault(customDefault);

            // Assert
            Assert.Equal(customDefault, data);
        }

        #endregion

        #region Non-Generic TeamOperationResult Tests

        [Fact]
        public void NonGenericSuccessResult_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = TeamOperationResult.SuccessResult();

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void NonGenericErrorResult_WithMessage_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Non-generic error";

            // Act
            var result = TeamOperationResult.ErrorResult(errorMessage);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void NonGenericErrorResult_WithMessageAndException_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Non-generic error with exception";
            var exception = new NotSupportedException("Test not supported");

            // Act
            var result = TeamOperationResult.ErrorResult(errorMessage, exception);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(exception, result.Exception);
            Assert.Null(result.ErrorContext);
        }

        [Fact]
        public void NonGenericErrorResult_WithMessageExceptionAndContext_ShouldCreateErrorResult()
        {
            // Arrange
            var errorMessage = "Non-generic error with context";
            var exception = new TimeoutException("Operation timed out");
            var context = new { Timeout = 30, RetryCount = 3 };

            // Act
            var result = TeamOperationResult.ErrorResult(errorMessage, exception, context);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Equal(exception, result.Exception);
            Assert.Equal(context, result.ErrorContext);
        }

        #endregion

        #region Type-Specific Tests

        [Fact]
        public void GenericResult_WithIntegerType_ShouldWork()
        {
            // Arrange
            var testValue = 42;

            // Act
            var successResult = TeamOperationResult<int>.SuccessResult(testValue);
            var errorResult = TeamOperationResult<int>.ErrorResult("Integer operation failed");

            // Assert
            Assert.True(successResult.Success);
            Assert.Equal(testValue, successResult.Data);
            
            Assert.False(errorResult.Success);
            Assert.Equal(0, errorResult.Data); // Default int value
        }

        [Fact]
        public void GenericResult_WithCustomObjectType_ShouldWork()
        {
            // Arrange
            var customObject = new { Id = 123, Name = "Test Object", Active = true };

            // Act
            var result = TeamOperationResult<object>.SuccessResult(customObject);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(customObject, result.Data);
        }

        [Fact]
        public void GenericResult_WithNullableType_ShouldWork()
        {
            // Arrange
            int? nullableValue = null;

            // Act
            var result = TeamOperationResult<int?>.SuccessResult(nullableValue);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region Error Context Tests

        [Fact]
        public void ErrorResult_WithComplexErrorContext_ShouldPreserveContext()
        {
            // Arrange
            var errorContext = new
            {
                TeamId = "team-123",
                AgentName = "TestAgent",
                OperationType = "Handoff",
                Timestamp = DateTime.UtcNow,
                AdditionalData = new { Retry = 3, LastError = "Timeout" }
            };

            // Act
            var result = TeamOperationResult<string>.ErrorResult("Complex error", null, errorContext);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(errorContext, result.ErrorContext);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void SuccessResult_WithEmptyString_ShouldWork()
        {
            // Act
            var result = TeamOperationResult<string>.SuccessResult("");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("", result.Data);
        }

        [Fact]
        public void ErrorResult_WithEmptyMessage_ShouldWork()
        {
            // Act
            var result = TeamOperationResult<string>.ErrorResult("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("", result.ErrorMessage);
        }

        [Fact]
        public void ErrorResult_WithNullMessage_ShouldWork()
        {
            // Act
            var result = TeamOperationResult<string>.ErrorResult(null);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.ErrorMessage);
        }

        #endregion
    }
}