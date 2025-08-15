using AgentSharp.Core.Abstractions;
using System;

namespace AgentSharp.Core.Validation
{
    /// <summary>
    /// Implementação de produção do validador de recursos
    /// Realiza verificações rigorosas para garantir recursos válidos
    /// </summary>
    public class ProductionResourceValidator : IResourceValidator
    {
        private readonly IWorkflowLogger _logger;
        private readonly bool _strictValidation;
        
        public ProductionResourceValidator(IWorkflowLogger logger = null, bool strictValidation = true)
        {
            _logger = logger;
            _strictValidation = strictValidation;
        }
        
        public ResourceValidationResult ValidateDisposable(IDisposable disposable)
        {
            if (disposable == null)
            {
                return ResourceValidationResult.Failure("IDisposable", "Resource is null");
            }
            
            var resourceType = disposable.GetType().Name;
            
            // Verificar se o recurso já foi disposed (se possível)
            if (IsAlreadyDisposed(disposable))
            {
                var reason = "Resource appears to be already disposed";
                _logger?.LogResourceValidationFailure(resourceType, reason);
                return ResourceValidationResult.Failure(resourceType, reason, false);
            }
            
            // Verificar se é um tipo conhecido problemático
            if (_strictValidation && IsProblematicType(disposable))
            {
                var reason = "Resource type is known to have disposal issues";
                _logger?.LogResourceValidationFailure(resourceType, reason);
                return ResourceValidationResult.Failure(resourceType, reason, true); // Still safe to try
            }
            
            return ResourceValidationResult.Success(resourceType);
        }
        
        public ResourceValidationResult ValidateAsyncDisposable(IAsyncDisposable asyncDisposable)
        {
            if (asyncDisposable == null)
            {
                return ResourceValidationResult.Failure("IAsyncDisposable", "Resource is null");
            }
            
            var resourceType = asyncDisposable.GetType().Name;
            
            // Verificar se o recurso já foi disposed (se possível)
            if (IsAlreadyDisposed(asyncDisposable))
            {
                var reason = "Async resource appears to be already disposed";
                _logger?.LogResourceValidationFailure(resourceType, reason);
                return ResourceValidationResult.Failure(resourceType, reason, false);
            }
            
            // Verificar se implementa tanto IDisposable quanto IAsyncDisposable
            if (asyncDisposable is IDisposable)
            {
                _logger?.LogInformation("Resource {0} implements both IDisposable and IAsyncDisposable", resourceType);
            }
            
            return ResourceValidationResult.Success(resourceType);
        }
        
        public ResourceValidationResult ValidateResource(object resource)
        {
            if (resource == null)
            {
                return ResourceValidationResult.Failure("Unknown", "Resource is null");
            }
            
            var resourceType = resource.GetType().Name;
            
            // Verificar se implementa pelo menos uma interface de dispose
            var isDisposable = resource is IDisposable;
            var isAsyncDisposable = resource is IAsyncDisposable;
            
            if (!isDisposable && !isAsyncDisposable)
            {
                var reason = "Resource does not implement IDisposable or IAsyncDisposable";
                _logger?.LogResourceValidationFailure(resourceType, reason);
                return ResourceValidationResult.Failure(resourceType, reason, false);
            }
            
            // Validar baseado no tipo específico
            if (isAsyncDisposable)
            {
                return ValidateAsyncDisposable((IAsyncDisposable)resource);
            }
            else
            {
                return ValidateDisposable((IDisposable)resource);
            }
        }
        
        /// <summary>
        /// Verifica se um recurso já foi disposed (heurística)
        /// </summary>
        private bool IsAlreadyDisposed(object resource)
        {
            if (resource == null) return true;
            
            try
            {
                // Verificar propriedades comuns que indicam dispose
                var type = resource.GetType();
                
                // Procurar por propriedade IsDisposed comum
                var isDisposedProperty = type.GetProperty("IsDisposed");
                if (isDisposedProperty != null && isDisposedProperty.CanRead)
                {
                    var isDisposed = (bool)isDisposedProperty.GetValue(resource);
                    return isDisposed;
                }
                
                // Para CancellationTokenSource
                if (resource is System.Threading.CancellationTokenSource cts)
                {
                    // Tentar acessar Token - se lançar ObjectDisposedException, foi disposed
                    var _ = cts.Token;
                    return false;
                }
                
                return false; // Assumir que não foi disposed se não conseguir determinar
            }
            catch (ObjectDisposedException)
            {
                return true; // Definitivamente foi disposed
            }
            catch
            {
                return false; // Em caso de erro, assumir que não foi disposed
            }
        }
        
        /// <summary>
        /// Verifica se é um tipo conhecido por ter problemas de dispose
        /// </summary>
        private bool IsProblematicType(object resource)
        {
            if (resource == null) return false;
            
            var type = resource.GetType();
            
            // Lista de tipos conhecidos por serem problemáticos
            // Pode ser expandida baseada na experiência
            var problematicTypes = new[]
            {
                "HttpClient", // Conhecido por problemas se disposed incorretamente
                "FileStream", // Pode ter problemas com dispose duplo
                "NetworkStream" // Problemas de rede podem complicar dispose
            };
            
            var typeName = type.Name;
            foreach (var problematicType in problematicTypes)
            {
                if (typeName.Contains(problematicType))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Validador mais permissivo para desenvolvimento
    /// </summary>
    public class DevelopmentResourceValidator : IResourceValidator
    {
        public ResourceValidationResult ValidateDisposable(IDisposable disposable)
        {
            return disposable == null 
                ? ResourceValidationResult.Failure("IDisposable", "Resource is null") 
                : ResourceValidationResult.Success(disposable.GetType().Name);
        }
        
        public ResourceValidationResult ValidateAsyncDisposable(IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable == null 
                ? ResourceValidationResult.Failure("IAsyncDisposable", "Resource is null") 
                : ResourceValidationResult.Success(asyncDisposable.GetType().Name);
        }
        
        public ResourceValidationResult ValidateResource(object resource)
        {
            if (resource == null)
                return ResourceValidationResult.Failure("Unknown", "Resource is null");
                
            if (resource is IAsyncDisposable asyncDisposable)
                return ValidateAsyncDisposable(asyncDisposable);
                
            if (resource is IDisposable disposable)
                return ValidateDisposable(disposable);
                
            return ResourceValidationResult.Failure(resource.GetType().Name, 
                "Resource does not implement disposal interfaces", false);
        }
    }
}