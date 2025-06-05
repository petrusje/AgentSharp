using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agents.net.Models; // Add this using for UsageInfo

namespace Agents.net.Core.Orchestration
{
    /// <summary>
    /// Representa uma sessão de workflow com estado persistente
    /// Sistema de sessões para workflows multi-etapas
    /// </summary>
    public class WorkflowSession
    {
        /// <summary>
        /// ID único da sessão
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Nome da sessão (opcional)
        /// </summary>
        public string SessionName { get; set; }
        
        /// <summary>
        /// ID do workflow associado
        /// </summary>
        public string WorkflowId { get; set; }
        
        /// <summary>
        /// ID do usuário (opcional)
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Estado da sessão armazenado como dicionário
        /// </summary>
        public Dictionary<string, object> SessionState { get; set; }
        
        /// <summary>
        /// Metadados adicionais da sessão
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }
        
        /// <summary>
        /// Timestamp de criação da sessão
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Timestamp da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Histórico de execuções nesta sessão
        /// </summary>
        public List<WorkflowRun> Runs { get; set; }
        
        /// <summary>
        /// Indica se a sessão está ativa
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Uso total de tokens em toda a sessão
        /// </summary>
        public UsageInfo SessionTokenUsage { get; private set; }
        
        /// <summary>
        /// Total de tokens utilizados em toda a sessão
        /// </summary>
        public int TotalSessionTokens => SessionTokenUsage?.TotalTokens ?? 0;
        
        /// <summary>
        /// Custo estimado total da sessão
        /// </summary>
        public decimal TotalSessionCost => SessionTokenUsage?.EstimatedCost ?? 0m;

        public WorkflowSession()
        {
            SessionId = Guid.NewGuid().ToString("N");
            SessionState = new Dictionary<string, object>();
            Metadata = new Dictionary<string, object>();
            Runs = new List<WorkflowRun>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
            SessionTokenUsage = new UsageInfo { PromptTokens = 0, CompletionTokens = 0, EstimatedCost = 0m };
        }
        
        public WorkflowSession(string sessionId, string workflowId, string userId = null) : this()
        {
            SessionId = sessionId;
            WorkflowId = workflowId;
            UserId = userId;
        }
        
        /// <summary>
        /// Adiciona uma execução ao histórico da sessão
        /// </summary>
        public void AddRun(WorkflowRun run)
        {
            run.SessionId = SessionId;
            Runs.Add(run);
            UpdatedAt = DateTime.UtcNow;
            
            // Atualizar tokens da sessão
            UpdateSessionTokenUsage();
        }
        
        /// <summary>
        /// Atualiza o uso total de tokens da sessão baseado em todas as execuções
        /// </summary>
        private void UpdateSessionTokenUsage()
        {
            int totalPromptTokens = 0;
            int totalCompletionTokens = 0;
            decimal totalCost = 0m;
            
            foreach (var run in Runs)
            {
                if (run.TokenUsage != null)
                {
                    totalPromptTokens += run.TokenUsage.PromptTokens;
                    totalCompletionTokens += run.TokenUsage.CompletionTokens;
                    totalCost += run.TokenUsage.EstimatedCost;
                }
            }
            
            SessionTokenUsage = new UsageInfo
            {
                PromptTokens = totalPromptTokens,
                CompletionTokens = totalCompletionTokens,
                EstimatedCost = totalCost
            };
        }
        
        /// <summary>
        /// Atualiza o estado da sessão
        /// </summary>
        public void UpdateState(string key, object value)
        {
            SessionState[key] = value;
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Obtém um valor do estado da sessão
        /// </summary>
        public T GetState<T>(string key, T defaultValue = default)
        {
            if (SessionState.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Cria uma cópia profunda da sessão
        /// </summary>
        public WorkflowSession DeepCopy()
        {
            var copy = new WorkflowSession
            {
                SessionId = this.SessionId,
                SessionName = this.SessionName,
                WorkflowId = this.WorkflowId,
                UserId = this.UserId,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                IsActive = this.IsActive,
                SessionState = new Dictionary<string, object>(this.SessionState),
                Metadata = new Dictionary<string, object>(this.Metadata),
                Runs = new List<WorkflowRun>(this.Runs)
            };
            
            return copy;
        }
    }
    
    /// <summary>
    /// Representa uma execução individual de workflow
    /// </summary>
    public class WorkflowRun
    {
        /// <summary>
        /// ID único da execução
        /// </summary>
        public string RunId { get; set; }
        
        /// <summary>
        /// ID da sessão associada
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Parâmetros de entrada da execução
        /// </summary>
        public Dictionary<string, object> Input { get; set; }
        
        /// <summary>
        /// Resultado da execução
        /// </summary>
        public object Result { get; set; }
        
        /// <summary>
        /// Status da execução
        /// </summary>
        public WorkflowRunStatus Status { get; set; }
        
        /// <summary>
        /// Mensagem de erro (se houver)
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Timestamp de início da execução
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Timestamp de fim da execução
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Duração da execução
        /// </summary>
        public TimeSpan? Duration => EndTime?.Subtract(StartTime);
        
        /// <summary>
        /// Metadados da execução
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }
        
        /// <summary>
        /// Informações de uso de tokens para esta execução
        /// </summary>
        public UsageInfo TokenUsage { get; set; }
        
        /// <summary>
        /// Uso de tokens por step (step_index -> UsageInfo)
        /// </summary>
        public Dictionary<int, UsageInfo> StepTokenUsage { get; set; }
        
        /// <summary>
        /// Total de tokens utilizados em todos os steps desta execução
        /// </summary>
        public int TotalTokens => TokenUsage?.TotalTokens ?? 0;
        
        /// <summary>
        /// Custo estimado total desta execução
        /// </summary>
        public decimal EstimatedCost => TokenUsage?.EstimatedCost ?? 0m;

        public WorkflowRun()
        {
            RunId = Guid.NewGuid().ToString("N");
            Input = new Dictionary<string, object>();
            Metadata = new Dictionary<string, object>();
            StepTokenUsage = new Dictionary<int, UsageInfo>();
            Status = WorkflowRunStatus.Running;
            StartTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Marca a execução como concluída com sucesso
        /// </summary>
        public void Complete(object result)
        {
            Result = result;
            Status = WorkflowRunStatus.Completed;
            EndTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Marca a execução como falhada
        /// </summary>
        public void Fail(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Status = WorkflowRunStatus.Failed;
            EndTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Adiciona uso de tokens de um step específico
        /// </summary>
        /// <param name="stepIndex">Índice do step</param>
        /// <param name="usage">Informações de uso de tokens</param>
        public void AddStepTokenUsage(int stepIndex, UsageInfo usage)
        {
            if (usage == null) return;
            
            StepTokenUsage[stepIndex] = usage;
            
            // Atualizar o total de tokens da execução
            UpdateTotalTokenUsage();
        }
        
        /// <summary>
        /// Atualiza o total de tokens desta execução baseado nos steps
        /// </summary>
        private void UpdateTotalTokenUsage()
        {
            if (StepTokenUsage.Count == 0)
            {
                TokenUsage = null;
                return;
            }
            
            int totalPromptTokens = 0;
            int totalCompletionTokens = 0;
            decimal totalCost = 0m;
            
            foreach (var usage in StepTokenUsage.Values)
            {
                totalPromptTokens += usage.PromptTokens;
                totalCompletionTokens += usage.CompletionTokens;
                totalCost += usage.EstimatedCost;
            }
            
            TokenUsage = new UsageInfo
            {
                PromptTokens = totalPromptTokens,
                CompletionTokens = totalCompletionTokens,
                EstimatedCost = totalCost
            };
        }
    }
    
    /// <summary>
    /// Status possíveis de uma execução de workflow
    /// </summary>
    public enum WorkflowRunStatus
    {
        Running,
        Completed,
        Failed,
        Cancelled
    }
}