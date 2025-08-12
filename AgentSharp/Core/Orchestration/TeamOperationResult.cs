using System;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Represents the result of a team operation with success/error information.
    /// Provides structured error handling instead of returning strings.
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class TeamOperationResult<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The data returned by the operation (if successful)
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Error message (if operation failed)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The exception that caused the failure (if any)
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Additional error context or metadata
        /// </summary>
        public object ErrorContext { get; set; }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        /// <param name="data">The success data</param>
        /// <returns>A successful TeamOperationResult</returns>
        public static TeamOperationResult<T> SuccessResult(T data)
        {
            return new TeamOperationResult<T>
            {
                Success = true,
                Data = data,
                ErrorMessage = null,
                Exception = null
            };
        }

        /// <summary>
        /// Creates a failed result with error information
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Optional exception</param>
        /// <param name="context">Optional error context</param>
        /// <returns>A failed TeamOperationResult</returns>
        public static TeamOperationResult<T> ErrorResult(string message, Exception ex = null, object context = null)
        {
            return new TeamOperationResult<T>
            {
                Success = false,
                Data = default(T),
                ErrorMessage = message,
                Exception = ex,
                ErrorContext = context
            };
        }

        /// <summary>
        /// Returns the data if successful, otherwise throws an exception
        /// </summary>
        /// <returns>The operation data</returns>
        /// <exception cref="InvalidOperationException">Thrown when the operation failed</exception>
        public T GetDataOrThrow()
        {
            if (!Success)
            {
                var message = $"Team operation failed: {ErrorMessage}";
                throw Exception != null 
                    ? new InvalidOperationException(message, Exception) 
                    : new InvalidOperationException(message);
            }
            return Data;
        }

        /// <summary>
        /// Returns the data if successful, otherwise returns the default value
        /// </summary>
        /// <param name="defaultValue">Default value to return on failure</param>
        /// <returns>The operation data or default value</returns>
        public T GetDataOrDefault(T defaultValue = default(T))
        {
            return Success ? Data : defaultValue;
        }
    }

    /// <summary>
    /// Non-generic version for operations that don't return data
    /// </summary>
    public class TeamOperationResult : TeamOperationResult<object>
    {
        /// <summary>
        /// Creates a successful result without data
        /// </summary>
        /// <returns>A successful TeamOperationResult</returns>
        public static TeamOperationResult SuccessResult()
        {
            return new TeamOperationResult
            {
                Success = true,
                Data = null,
                ErrorMessage = null,
                Exception = null
            };
        }

        /// <summary>
        /// Creates a failed result with error information
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Optional exception</param>
        /// <param name="context">Optional error context</param>
        /// <returns>A failed TeamOperationResult</returns>
        public static new TeamOperationResult ErrorResult(string message, Exception ex = null, object context = null)
        {
            return new TeamOperationResult
            {
                Success = false,
                Data = null,
                ErrorMessage = message,
                Exception = ex,
                ErrorContext = context
            };
        }
    }
}