using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agents.net.Utils;
using Agents.net.Core.Memory;
using Agents.net.Models;

namespace Agents.net.Core.Orchestration
{
    /// <summary>
    /// Workflow avançado com gerenciamento de sessão, observabilidade e resiliência
    /// Sistema avançado de workflow para coordenação de múltiplos agentes
    /// </summary>
    public class AdvancedWorkflow<TContext, TResult> : Workflow<TContext>
    {
        private readonly PromptManager<TContext> _promptManager;
        private readonly object _sessionLock = new object();
        private readonly object _executionLock = new object();
        
        // Configurações do workflow
        private bool _hasInstructions = false;
        private volatile bool _isExecuting = false;
        private bool _debugMode = false;
        private bool _enableTelemetry = true;
        
        // Sessão e estado
        private WorkflowSession _session;
        private IMemory _memory;
        
        // Propriedades públicas
        public string WorkflowId { get; private set; }
        public string SessionId => _session != null ? _session.SessionId : null;
        public string UserId { get; set; }
        public bool DebugMode 
        { 
            get => _debugMode; 
            set 
            { 
                _debugMode = value;
                if (value)
                    _logger.Log(LogLevel.Debug, "Debug mode enabled for workflow");
            } 
        }
        
        public WorkflowSession Session
        {
            get
            {
                lock (_sessionLock)
                {
                    return _session;
                }
            }
            private set
            {
                lock (_sessionLock)
                {
                    _session = value;
                }
            }
        }

        public string TaskGoal
        {
            get 
            {
                lock (_sessionLock)
                {
                    if (_session != null)
                    {
                        var context = _session.GetState<TContext>("current_context");
                        return context != null ? _promptManager.BuildSystemPrompt(context) : String.Empty;
                    }
                    return String.Empty;
                }
            }
        }

        public AdvancedWorkflow(string name, ILogger logger = null, IMemory memory = null)
            : base(name, logger)
        {
            WorkflowId = Guid.NewGuid().ToString("N");
            _promptManager = new PromptManager<TContext>();
            _memory = memory ?? new InMemoryStore();
            
            // Criar sessão inicial
            CreateNewSession();
            
            _logger.Log(LogLevel.Info, $"Advanced workflow '{name}' created with ID: {WorkflowId}");
        }

        #region Configuração Fluente

        public AdvancedWorkflow<TContext, TResult> ForTask(Func<TContext, string> generator)
        {
            _promptManager.AddInstructions(generator);
            _hasInstructions = true;
            return this;
        }
        
        public AdvancedWorkflow<TContext, TResult> WithUserId(string userId)
        {
            UserId = userId;
            if (_session != null)
                _session.UserId = userId;
            return this;
        }
        
        public AdvancedWorkflow<TContext, TResult> WithDebugMode(bool enabled = true)
        {
            DebugMode = enabled;
            return this;
        }
        
        public AdvancedWorkflow<TContext, TResult> WithTelemetry(bool enabled = true)
        {
            _enableTelemetry = enabled;
            return this;
        }
        
        public AdvancedWorkflow<TContext, TResult> WithMemory(IMemory memory)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
            return this;
        }
        
        public AdvancedWorkflow<TContext, TResult> RegisterStep(string name, IAgent agent, Func<TContext, string> getInput, Action<TContext, string> processOutput)
        {
            _steps.Add(new WorkflowStep(name, agent, getInput, processOutput));
            _logger.Log(LogLevel.Debug, $"Registered step '{name}' in workflow '{_name}'");
            return this;
        }

        #endregion

        #region Gerenciamento de Sessão

        /// <summary>
        /// Cria uma nova sessão para o workflow
        /// </summary>
        public void CreateNewSession(string sessionName = null)
        {
            lock (_sessionLock)
            {
                _session = new WorkflowSession
                {
                    WorkflowId = WorkflowId,
                    UserId = UserId,
                    SessionName = sessionName ?? $"Session-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
                };
                
                _logger.Log(LogLevel.Info, $"Created new session: {_session.SessionId}");
            }
        }
        
        /// <summary>
        /// Carrega uma sessão existente
        /// </summary>
        public void LoadSession(WorkflowSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
                
            lock (_sessionLock)
            {
                _session = session.DeepCopy();
                _logger.Log(LogLevel.Info, $"Loaded session: {_session.SessionId}");
            }
        }
        
        /// <summary>
        /// Obtém snapshot da sessão atual
        /// </summary>
        public WorkflowSession GetSessionSnapshot()
        {
            lock (_sessionLock)
            {
                return _session != null ? _session.DeepCopy() : null;
            }
        }

        #endregion

        #region Execução Principal

        public override async Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
        {
            // Verificar se já está executando
            lock (_executionLock)
            {
                if (_isExecuting)
                {
                    throw new InvalidOperationException("Workflow já está em execução. Execução paralela não é suportada.");
                }
                _isExecuting = true;
            }

            // Criar contexto de execução
            var executionContext = ExecutionContext.CreateNew();
            var workflowRun = new WorkflowRun();
            
            try
            {
                // Preparar execução
                await PrepareExecutionAsync(context, workflowRun, executionContext);
                
                // Executar steps
                var result = await ExecuteStepsAsync(context, workflowRun, executionContext, cancellationToken);
                
                // Finalizar execução
                await FinalizeExecutionAsync(result, workflowRun, executionContext);
                
                return result;
            }
            catch (Exception ex)
            {
                await HandleExecutionErrorAsync(ex, workflowRun, executionContext);
                throw;
            }
            finally
            {
                lock (_executionLock)
                {
                    _isExecuting = false;
                }
                
                ExecutionContext.ClearCurrent();
            }
        }

        private async Task PrepareExecutionAsync(TContext context, WorkflowRun run, ExecutionContext executionContext)
        {
            _logger.Log(LogLevel.Info, $"Starting workflow '{_name}' execution with {_steps.Count} steps");
            
            if (_debugMode)
            {
                _logger.Log(LogLevel.Debug, $"Execution Context: {executionContext}");
                _logger.Log(LogLevel.Debug, $"Session: {_session.SessionId}");
            }
            
            // Armazenar contexto na sessão
            lock (_sessionLock)
            {
                _session.UpdateState("current_context", context);
                _session.UpdateState("execution_start", DateTime.UtcNow);
                _session.AddRun(run);
            }
            
            // Salvar na memória
            await _memory.AddItemAsync(new MemoryItem(
                $"Workflow execution started: {_name}",
                "workflow_start",
                1.0,
                new Dictionary<string, object>
                {
                    ["workflow_id"] = WorkflowId,
                    ["session_id"] = SessionId,
                    ["execution_id"] = executionContext.ExecutionId
                }
            ));
        }

        private async Task<TContext> ExecuteStepsAsync(TContext context, WorkflowRun run, ExecutionContext executionContext, CancellationToken cancellationToken)
        {
            var messages = new List<AIMessage>();
            
            // Adicionar prompt do workflow se configurado
            if (_hasInstructions)
            {
                var systemPrompt = AIMessage.System(
                    "<meta> Você é um Agente dentro de um workflow sequencial de Agentes, a meta final é:" + 
                    _promptManager.BuildSystemPrompt(context) + "</meta>");
                messages.Add(systemPrompt);
            }

            // Executar cada step
            for (int i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                
                try
                {
                    await ExecuteStepAsync(step, context, messages, i, executionContext, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Error in step {i + 1}/{_steps.Count} '{step.Name}': {ex.Message}");
                    
                    // Salvar erro na sessão
                    lock (_sessionLock)
                    {
                        _session.UpdateState($"step_{i}_error", ex.Message);
                    }
                    
                    throw;
                }
            }

            return context;
        }

        private async Task ExecuteStepAsync(WorkflowStep step, TContext context, List<AIMessage> messages, int stepIndex, ExecutionContext executionContext, CancellationToken cancellationToken)
        {
            var stepStartTime = DateTime.UtcNow;
            
            _logger.Log(LogLevel.Debug, $"Executing step {stepIndex + 1}/{_steps.Count}: {step.Name}");
            
            // Obter input do step
            string input = step.GetInput(context);
            
            if (_debugMode)
            {
                _logger.Log(LogLevel.Debug, $"Step input: {input}");
            }
            
            // Executar agente
            var result = await step.Agent.ExecuteAsync(input, context, messages, cancellationToken);
            string output = result?.ToString() ?? string.Empty;
            
            // Capturar informações de uso de tokens
            UsageInfo stepTokenUsage = null;
            if (result is AgentResult<object> agentResult && agentResult.Usage != null)
            {
                stepTokenUsage = agentResult.Usage;
                
                if (_debugMode)
                {
                    _logger.Log(LogLevel.Debug, $"Step {stepIndex + 1} token usage - Prompt: {stepTokenUsage.PromptTokens}, Completion: {stepTokenUsage.CompletionTokens}, Total: {stepTokenUsage.TotalTokens}, Cost: ${stepTokenUsage.EstimatedCost:F4}");
                }
            }
            
            // Processar output
            step.ProcessOutput(context, output);
            
            var stepDuration = DateTime.UtcNow - stepStartTime;
            
            _logger.Log(LogLevel.Info, $"Completed step {stepIndex + 1}/{_steps.Count}: {step.Name} (Duration: {stepDuration.TotalMilliseconds:F0}ms)");
            
            // Salvar informações do step na sessão
            lock (_sessionLock)
            {
                _session.UpdateState($"step_{stepIndex}_output", output);
                _session.UpdateState($"step_{stepIndex}_duration", stepDuration);
                
                // Salvar informações de tokens se disponível
                if (stepTokenUsage != null)
                {
                    _session.UpdateState($"step_{stepIndex}_tokens", stepTokenUsage.TotalTokens);
                    _session.UpdateState($"step_{stepIndex}_prompt_tokens", stepTokenUsage.PromptTokens);
                    _session.UpdateState($"step_{stepIndex}_completion_tokens", stepTokenUsage.CompletionTokens);
                    _session.UpdateState($"step_{stepIndex}_cost", stepTokenUsage.EstimatedCost);
                    
                    // Adicionar ao run atual se existir
                    if (_session.Runs.Count > 0)
                    {
                        var currentRun = _session.Runs[_session.Runs.Count - 1];
                        currentRun.AddStepTokenUsage(stepIndex, stepTokenUsage);
                    }
                }
            }
            
            // Salvar na memória
            var memoryMetadata = new Dictionary<string, object>
            {
                ["step_name"] = step.Name,
                ["step_index"] = stepIndex,
                ["duration_ms"] = stepDuration.TotalMilliseconds,
                ["output_length"] = output.Length
            };
            
            // Adicionar informações de tokens à memória se disponível
            if (stepTokenUsage != null)
            {
                memoryMetadata["total_tokens"] = stepTokenUsage.TotalTokens;
                memoryMetadata["prompt_tokens"] = stepTokenUsage.PromptTokens;
                memoryMetadata["completion_tokens"] = stepTokenUsage.CompletionTokens;
                memoryMetadata["estimated_cost"] = stepTokenUsage.EstimatedCost;
            }
            
            await _memory.AddItemAsync(new MemoryItem(
                $"Step completed: {step.Name}",
                "step_completion",
                1.0,
                memoryMetadata
            ));
        }

        private async Task FinalizeExecutionAsync(TContext result, WorkflowRun run, ExecutionContext executionContext)
        {
            run.Complete(result);
            
            _logger.Log(LogLevel.Info, $"Workflow '{_name}' completed successfully");
            
            if (_debugMode)
            {
                _logger.Log(LogLevel.Debug, $"Final execution context: {executionContext}");
                _logger.Log(LogLevel.Debug, $"Total execution time: {executionContext.ElapsedTime}");
            }
            
            // Atualizar sessão
            lock (_sessionLock)
            {
                _session.UpdateState("execution_end", DateTime.UtcNow);
                _session.UpdateState("execution_duration", executionContext.ElapsedTime);
                _session.UpdateState("final_result", result);
            }
            
            // Salvar na memória
            await _memory.AddItemAsync(new MemoryItem(
                $"Workflow execution completed: {_name}",
                "workflow_completion",
                1.0,
                new Dictionary<string, object>
                {
                    ["workflow_id"] = WorkflowId,
                    ["session_id"] = SessionId,
                    ["execution_id"] = executionContext.ExecutionId,
                    ["duration_ms"] = executionContext.ElapsedTime.TotalMilliseconds,
                    ["total_calls"] = executionContext.CallCount,
                    ["total_executions"] = executionContext.ExecutionCount
                }
            ));
        }

        private async Task HandleExecutionErrorAsync(Exception ex, WorkflowRun run, ExecutionContext executionContext)
        {
            run.Fail(ex.Message);
            
            _logger.Log(LogLevel.Error, $"Workflow '{_name}' failed: {ex.Message}", ex);
            
            // Atualizar sessão com erro
            lock (_sessionLock)
            {
                _session.UpdateState("execution_error", ex.Message);
                _session.UpdateState("execution_end", DateTime.UtcNow);
            }
            
            // Salvar erro na memória
            await _memory.AddItemAsync(new MemoryItem(
                $"Workflow execution failed: {_name} - {ex.Message}",
                "workflow_error",
                1.0,
                new Dictionary<string, object>
                {
                    ["workflow_id"] = WorkflowId,
                    ["session_id"] = SessionId,
                    ["execution_id"] = executionContext.ExecutionId,
                    ["error_type"] = ex.GetType().Name,
                    ["error_message"] = ex.Message
                }
            ));
        }

        #endregion

        #region Observabilidade

        /// <summary>
        /// Obtém métricas da execução atual
        /// </summary>
        public WorkflowMetrics GetMetrics()
        {
            lock (_sessionLock)
            {
                return new WorkflowMetrics
                {
                    WorkflowId = WorkflowId,
                    SessionId = SessionId,
                    TotalRuns = _session.Runs.Count,
                    SuccessfulRuns = _session.Runs.FindAll(r => r.Status == WorkflowRunStatus.Completed).Count,
                    FailedRuns = _session.Runs.FindAll(r => r.Status == WorkflowRunStatus.Failed).Count,
                    AverageExecutionTime = CalculateAverageExecutionTime(),
                    LastExecutionTime = _session.Runs.Count > 0 ? _session.Runs[_session.Runs.Count - 1].StartTime : (DateTime?)null,
                    TotalTokens = _session.TotalSessionTokens,
                    TotalPromptTokens = _session.SessionTokenUsage?.PromptTokens ?? 0,
                    TotalCompletionTokens = _session.SessionTokenUsage?.CompletionTokens ?? 0,
                    TotalEstimatedCost = _session.TotalSessionCost
                };
            }
        }

        private TimeSpan? CalculateAverageExecutionTime()
        {
            var completedRuns = _session.Runs.FindAll(r => r.Status == WorkflowRunStatus.Completed && r.Duration.HasValue);
            if (completedRuns.Count == 0) return null;
            
            var totalTicks = completedRuns.Sum(r => r.Duration.Value.Ticks);
            return new TimeSpan(totalTicks / completedRuns.Count);
        }

        #endregion
    }

    /// <summary>
    /// Métricas de execução do workflow
    /// </summary>
    public class WorkflowMetrics
    {
        public string WorkflowId { get; set; }
        public string SessionId { get; set; }
        public int TotalRuns { get; set; }
        public int SuccessfulRuns { get; set; }
        public int FailedRuns { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        
        /// <summary>
        /// Total de tokens utilizados em toda a sessão
        /// </summary>
        public int TotalTokens { get; set; }
        
        /// <summary>
        /// Total de tokens de prompt utilizados
        /// </summary>
        public int TotalPromptTokens { get; set; }
        
        /// <summary>
        /// Total de tokens de completion utilizados
        /// </summary>
        public int TotalCompletionTokens { get; set; }
        
        /// <summary>
        /// Custo estimado total da sessão
        /// </summary>
        public decimal TotalEstimatedCost { get; set; }
        
        /// <summary>
        /// Média de tokens por execução bem-sucedida
        /// </summary>
        public double AverageTokensPerRun => SuccessfulRuns > 0 ? (double)TotalTokens / SuccessfulRuns : 0.0;
        
        /// <summary>
        /// Custo médio por execução bem-sucedida
        /// </summary>
        public decimal AverageCostPerRun => SuccessfulRuns > 0 ? TotalEstimatedCost / SuccessfulRuns : 0m;
        
        public double SuccessRate => TotalRuns > 0 ? (double)SuccessfulRuns / TotalRuns : 0.0;
    }
}