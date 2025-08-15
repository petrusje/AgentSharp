using System;

namespace AgentSharp.Core.Configuration
{
    /// <summary>
    /// Configurações para gerenciamento de recursos em workflows
    /// Permite customização de timeouts e limites
    /// </summary>
    public class WorkflowResourceConfiguration
    {
        /// <summary>
        /// Timeout padrão para dispose assíncrono em contexto síncrono (5 segundos)
        /// </summary>
        public static readonly TimeSpan DefaultAsyncDisposeTimeout = TimeSpan.FromSeconds(5);
        
        /// <summary>
        /// Timeout padrão para operações de workflow (30 segundos)
        /// </summary>
        public static readonly TimeSpan DefaultWorkflowTimeout = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Timeout para dispose assíncrono em contexto síncrono
        /// </summary>
        public TimeSpan AsyncDisposeTimeout { get; set; } = DefaultAsyncDisposeTimeout;
        
        /// <summary>
        /// Timeout geral para operações de workflow
        /// </summary>
        public TimeSpan WorkflowTimeout { get; set; } = DefaultWorkflowTimeout;
        
        /// <summary>
        /// Número máximo de tentativas de dispose em caso de falha
        /// </summary>
        public int MaxDisposeRetries { get; set; } = 3;
        
        /// <summary>
        /// Habilita logging detalhado de operações de recursos
        /// </summary>
        public bool EnableResourceLogging { get; set; } = true;
        
        /// <summary>
        /// Habilita validação rigorosa de recursos
        /// </summary>
        public bool EnableStrictResourceValidation { get; set; } = true;
        
        /// <summary>
        /// Configuração padrão para desenvolvimento
        /// </summary>
        public static WorkflowResourceConfiguration Development => new WorkflowResourceConfiguration
        {
            AsyncDisposeTimeout = TimeSpan.FromSeconds(2),
            WorkflowTimeout = TimeSpan.FromSeconds(10),
            MaxDisposeRetries = 1,
            EnableResourceLogging = true,
            EnableStrictResourceValidation = false
        };
        
        /// <summary>
        /// Configuração padrão para produção
        /// </summary>
        public static WorkflowResourceConfiguration Production => new WorkflowResourceConfiguration
        {
            AsyncDisposeTimeout = TimeSpan.FromSeconds(10),
            WorkflowTimeout = TimeSpan.FromMinutes(5),
            MaxDisposeRetries = 3,
            EnableResourceLogging = true,
            EnableStrictResourceValidation = true
        };
        
        /// <summary>
        /// Cria configuração a partir de variáveis de ambiente ou appsettings
        /// </summary>
        /// <param name="asyncDisposeTimeoutMs">Timeout para dispose assíncrono em ms</param>
        /// <param name="workflowTimeoutMs">Timeout para workflow em ms</param>
        /// <param name="maxRetries">Máximo de tentativas</param>
        /// <param name="enableLogging">Habilita logging</param>
        /// <param name="enableValidation">Habilita validação</param>
        /// <returns>Configuração customizada</returns>
        public static WorkflowResourceConfiguration FromSettings(
            int? asyncDisposeTimeoutMs = null,
            int? workflowTimeoutMs = null,
            int? maxRetries = null,
            bool? enableLogging = null,
            bool? enableValidation = null)
        {
            return new WorkflowResourceConfiguration
            {
                AsyncDisposeTimeout = asyncDisposeTimeoutMs.HasValue 
                    ? TimeSpan.FromMilliseconds(asyncDisposeTimeoutMs.Value) 
                    : DefaultAsyncDisposeTimeout,
                WorkflowTimeout = workflowTimeoutMs.HasValue 
                    ? TimeSpan.FromMilliseconds(workflowTimeoutMs.Value) 
                    : DefaultWorkflowTimeout,
                MaxDisposeRetries = maxRetries ?? 3,
                EnableResourceLogging = enableLogging ?? true,
                EnableStrictResourceValidation = enableValidation ?? true
            };
        }
    }
}