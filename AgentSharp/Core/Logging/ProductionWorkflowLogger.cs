using AgentSharp.Core.Abstractions;
using AgentSharp.Utils;
using System;

namespace AgentSharp.Core.Logging
{
    /// <summary>
    /// Implementação de produção do IWorkflowLogger
    /// Usa ILogger estruturado para logging adequado
    /// </summary>
    public class ProductionWorkflowLogger : IWorkflowLogger
    {
        private readonly ILogger _logger;
        
        public ProductionWorkflowLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public bool IsEnabled => _logger != null;
        
        public void LogInformation(string message, params object[] args)
        {
            _logger?.Log(LogLevel.Info, $"[WORKFLOW] {string.Format(message, args)}");
        }
        
        public void LogWarning(string message, params object[] args)
        {
            _logger?.Log(LogLevel.Warning, $"[WORKFLOW] {string.Format(message, args)}");
        }
        
        public void LogError(string message, Exception exception = null, params object[] args)
        {
            var formattedMessage = $"[WORKFLOW] {string.Format(message, args)}";
            if (exception != null)
            {
                _logger?.Log(LogLevel.Error, $"{formattedMessage} - Exception: {exception.Message}", exception);
            }
            else
            {
                _logger?.Log(LogLevel.Error, formattedMessage);
            }
        }
        
        public void LogDisposeError(string resourceType, Exception exception)
        {
            LogError("Error disposing {0}: {1}", 
                exception, resourceType, exception?.Message);
        }
        
        public void LogTimeout(string operation, TimeSpan timeout)
        {
            LogWarning("Operation {0} timed out after {1}ms", 
                operation, timeout.TotalMilliseconds);
        }
        
        public void LogResourceValidationFailure(string resourceType, string reason)
        {
            LogError("Resource validation failed for {0}: {1}", 
                null, resourceType, reason);
        }
    }
    
    /// <summary>
    /// Implementação fallback que usa Console para desenvolvimento
    /// Quando ILogger não estiver disponível
    /// </summary>
    public class ConsoleWorkflowLogger : IWorkflowLogger
    {
        public bool IsEnabled => true;
        
        public void LogInformation(string message, params object[] args)
        {
            Console.WriteLine($"[INFO] [WORKFLOW] {string.Format(message, args)}");
        }
        
        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine($"[WARN] [WORKFLOW] {string.Format(message, args)}");
        }
        
        public void LogError(string message, Exception exception = null, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            if (exception != null)
            {
                Console.WriteLine($"[ERROR] [WORKFLOW] {formattedMessage} - Exception: {exception.Message}");
            }
            else
            {
                Console.WriteLine($"[ERROR] [WORKFLOW] {formattedMessage}");
            }
        }
        
        public void LogDisposeError(string resourceType, Exception exception)
        {
            LogError("Error disposing {0}: {1}", exception, resourceType, exception?.Message);
        }
        
        public void LogTimeout(string operation, TimeSpan timeout)
        {
            LogWarning("Operation {0} timed out after {1}ms", operation, timeout.TotalMilliseconds);
        }
        
        public void LogResourceValidationFailure(string resourceType, string reason)
        {
            LogError("Resource validation failed for {0}: {1}", null, resourceType, reason);
        }
    }
}