using System;

namespace AgentSharp.Core.Abstractions
{
    /// <summary>
    /// Interface para logging de produção em workflows
    /// Substitui Debug.WriteLine por logging estruturado
    /// </summary>
    public interface IWorkflowLogger
    {
        /// <summary>
        /// Registra uma mensagem de informação
        /// </summary>
        void LogInformation(string message, params object[] args);
        
        /// <summary>
        /// Registra um aviso
        /// </summary>
        void LogWarning(string message, params object[] args);
        
        /// <summary>
        /// Registra um erro
        /// </summary>
        void LogError(string message, Exception exception = null, params object[] args);
        
        /// <summary>
        /// Registra um erro durante dispose de recursos
        /// </summary>
        void LogDisposeError(string resourceType, Exception exception);
        
        /// <summary>
        /// Registra timeout durante operações assíncronas
        /// </summary>
        void LogTimeout(string operation, TimeSpan timeout);
        
        /// <summary>
        /// Registra falha na validação de recursos
        /// </summary>
        void LogResourceValidationFailure(string resourceType, string reason);
        
        /// <summary>
        /// Indica se o logger está habilitado
        /// </summary>
        bool IsEnabled { get; }
    }
}