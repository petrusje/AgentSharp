using System;

namespace AgentSharp.Core.Validation
{
    /// <summary>
    /// Interface para validação de recursos antes do registro
    /// Garante que recursos sejam válidos antes de adicionar ao CompositeDisposable
    /// </summary>
    public interface IResourceValidator
    {
        /// <summary>
        /// Valida um recurso IDisposable
        /// </summary>
        /// <param name="disposable">Recurso a ser validado</param>
        /// <returns>Resultado da validação</returns>
        ResourceValidationResult ValidateDisposable(IDisposable disposable);
        
        /// <summary>
        /// Valida um recurso IAsyncDisposable
        /// </summary>
        /// <param name="asyncDisposable">Recurso async a ser validado</param>
        /// <returns>Resultado da validação</returns>
        ResourceValidationResult ValidateAsyncDisposable(IAsyncDisposable asyncDisposable);
        
        /// <summary>
        /// Valida um recurso genérico
        /// </summary>
        /// <param name="resource">Recurso a ser validado</param>
        /// <returns>Resultado da validação</returns>
        ResourceValidationResult ValidateResource(object resource);
    }
    
    /// <summary>
    /// Resultado da validação de recursos
    /// </summary>
    public class ResourceValidationResult
    {
        /// <summary>
        /// Indica se a validação foi bem-sucedida
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Razão da falha na validação (se houver)
        /// </summary>
        public string FailureReason { get; set; }
        
        /// <summary>
        /// Tipo do recurso validado
        /// </summary>
        public string ResourceType { get; set; }
        
        /// <summary>
        /// Indica se o recurso pode ser usado com segurança
        /// </summary>
        public bool IsSafeToUse { get; set; }
        
        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static ResourceValidationResult Success(string resourceType)
        {
            return new ResourceValidationResult
            {
                IsValid = true,
                ResourceType = resourceType,
                IsSafeToUse = true
            };
        }
        
        /// <summary>
        /// Cria um resultado de falha
        /// </summary>
        public static ResourceValidationResult Failure(string resourceType, string reason, bool isSafeToUse = false)
        {
            return new ResourceValidationResult
            {
                IsValid = false,
                ResourceType = resourceType,
                FailureReason = reason,
                IsSafeToUse = isSafeToUse
            };
        }
    }
}