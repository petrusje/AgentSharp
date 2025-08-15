using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Configuration;
using AgentSharp.Core.Validation;
using AgentSharp.Core.Logging;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Composite Disposable para gerenciar múltiplos recursos
    /// Implementa tanto IDisposable quanto IAsyncDisposable para cleanup adequado
    /// Versão aprimorada com logging, configuração e validação
    /// </summary>
    public class CompositeDisposable : IAsyncDisposable, IDisposable
    {
        private readonly List<object> _disposables = new List<object>();
        private readonly object _lock = new object();
        private volatile bool _disposed = false;
        private readonly IWorkflowLogger _logger;
        private readonly WorkflowResourceConfiguration _config;
        private readonly IResourceValidator _validator;

        /// <summary>
        /// Construtor com configuração personalizada
        /// </summary>
        public CompositeDisposable(
            IWorkflowLogger logger = null, 
            WorkflowResourceConfiguration config = null,
            IResourceValidator validator = null)
        {
            _logger = logger ?? new ConsoleWorkflowLogger();
            _config = config ?? WorkflowResourceConfiguration.Production;
            _validator = validator ?? new ProductionResourceValidator(_logger, _config.EnableStrictResourceValidation);
        }

        /// <summary>
        /// Adiciona um recurso IDisposable para gerenciamento
        /// </summary>
        /// <param name="disposable">Recurso a ser gerenciado</param>
        public void Add(IDisposable disposable)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(CompositeDisposable));
            
            if (disposable == null) 
                return;

            // Validar recurso se habilitado
            if (_config.EnableStrictResourceValidation)
            {
                var validation = _validator.ValidateDisposable(disposable);
                if (!validation.IsValid)
                {
                    if (!validation.IsSafeToUse)
                    {
                        throw new InvalidOperationException(
                            $"Resource validation failed for {validation.ResourceType}: {validation.FailureReason}");
                    }
                    _logger.LogResourceValidationFailure(validation.ResourceType, validation.FailureReason);
                }
            }

            lock (_lock) 
            {
                _disposables.Add(disposable);
                if (_config.EnableResourceLogging)
                {
                    _logger.LogInformation("Added IDisposable resource: {0}", disposable.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Adiciona um recurso IAsyncDisposable para gerenciamento
        /// </summary>
        /// <param name="asyncDisposable">Recurso async a ser gerenciado</param>
        public void Add(IAsyncDisposable asyncDisposable)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(CompositeDisposable));
            
            if (asyncDisposable == null) 
                return;

            // Validar recurso se habilitado
            if (_config.EnableStrictResourceValidation)
            {
                var validation = _validator.ValidateAsyncDisposable(asyncDisposable);
                if (!validation.IsValid)
                {
                    if (!validation.IsSafeToUse)
                    {
                        throw new InvalidOperationException(
                            $"Async resource validation failed for {validation.ResourceType}: {validation.FailureReason}");
                    }
                    _logger.LogResourceValidationFailure(validation.ResourceType, validation.FailureReason);
                }
            }

            lock (_lock) 
            {
                _disposables.Add(asyncDisposable);
                if (_config.EnableResourceLogging)
                {
                    _logger.LogInformation("Added IAsyncDisposable resource: {0}", asyncDisposable.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Dispose síncrono - executa cleanup de todos os recursos
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;

            List<object> itemsToDispose;
            lock (_lock)
            {
                itemsToDispose = _disposables.ToList();
                _disposables.Clear();
            }

            // Dispose em ordem reversa (LIFO)
            for (int i = itemsToDispose.Count - 1; i >= 0; i--)
            {
                var item = itemsToDispose[i];
                try
                {
                    switch (item)
                    {
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            // Para async disposables em contexto sync, aguarda com timeout configurável
                            var timeoutMs = (int)_config.AsyncDisposeTimeout.TotalMilliseconds;
                            asyncDisposable.DisposeAsync().AsTask().Wait(timeoutMs);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't throw - disposal should be best effort
                    _logger?.LogDisposeError(item?.GetType()?.Name ?? "Unknown", ex);
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose assíncrono - executa cleanup async de todos os recursos
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            
            _disposed = true;

            List<object> itemsToDispose;
            lock (_lock)
            {
                itemsToDispose = _disposables.ToList();
                _disposables.Clear();
            }

            // Dispose em ordem reversa (LIFO)
            for (int i = itemsToDispose.Count - 1; i >= 0; i--)
            {
                var item = itemsToDispose[i];
                try
                {
                    switch (item)
                    {
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't throw - disposal should be best effort
                    _logger?.LogDisposeError(item?.GetType()?.Name ?? "Unknown", ex);
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Indica se o CompositeDisposable foi disposed
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Número atual de recursos gerenciados
        /// </summary>
        public int Count 
        { 
            get 
            { 
                lock (_lock) 
                { 
                    return _disposed ? 0 : _disposables.Count; 
                } 
            } 
        }

        /// <summary>
        /// Finalizer para cleanup se Dispose não foi chamado
        /// </summary>
        ~CompositeDisposable()
        {
            Dispose();
        }
    }
}