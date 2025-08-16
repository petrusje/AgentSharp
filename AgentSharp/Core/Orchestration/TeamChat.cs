using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Orchestration;
using AgentSharp.Core;
using AgentSharp.Core.Abstractions;
using AgentSharp.Core.Configuration;
using AgentSharp.Core.Logging;
using AgentSharp.Utils;
using AgentSharp.Tools;
using AgentSharp.Models;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Intelligent multi-agent conversation workflow with LLM-driven handoffs
    /// </summary>
    public class TeamChat<TContext> : AdvancedWorkflow<TContext, string>
    {
        private readonly GlobalVariableCollection _globalVariables = new GlobalVariableCollection();
        internal readonly Dictionary<string, TeamAgent> _agents = new Dictionary<string, TeamAgent>();
        private readonly IntelligentSystemMessageBuilder _systemBuilder = new IntelligentSystemMessageBuilder();
        private readonly List<ConversationMessage> _messageHistory = new List<ConversationMessage>();
        private readonly object _conversationLock = new object();
        
        internal TeamAgent _currentAgent;
        private string _pendingTransition;
        private bool _globalMemoryEnabled;
        private bool _globalHistoryEnabled;
        private IEnhancedStorage _enhancedStorage;
        private string _conversationSessionId;
        private ConversationState _conversationState;

        /// <summary>
        /// Currently active agent in the conversation
        /// </summary>
        public string CurrentAgentName => _currentAgent?.Name;

        /// <summary>
        /// Current conversation progress based on variables
        /// </summary>
        public ConversationProgress Progress => _globalVariables.GetProgress();

        /// <summary>
        /// Conversation message history
        /// </summary>
        public IReadOnlyList<ConversationMessage> MessageHistory => _messageHistory.AsReadOnly();

        /// <summary>
        /// Available agents in this conversation
        /// </summary>
        public IReadOnlyDictionary<string, TeamAgent> AvailableAgents => _agents;

        /// <summary>
        /// Conversation session ID
        /// </summary>
        public string ConversationSessionId => _conversationSessionId;

        public TeamChat(string name = null, ILogger logger = null, IMemory memory = null,
                        WorkflowResourceConfiguration config = null, IWorkflowLogger workflowLogger = null)
            : base(name ?? "TeamChat", logger, memory, config)
        {
            _conversationSessionId = Guid.NewGuid().ToString("N");
            
            // Habilitar funcionalidades globais por padrão para TeamChat
            this.EnableGlobalMemory(enabled: memory != null, memory: memory);
            this.EnableGlobalMessageHistory(enabled: true);
        }

        #region Configuração Fluente

        /// <summary>
        /// Configura variáveis globais para a conversa
        /// </summary>
        public TeamChat<TContext> WithGlobalVariables(Action<GlobalVariableBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new GlobalVariableBuilder();
            configure(builder);
            _globalVariables.Configure(builder.Build());
            
            _logger.Log(LogLevel.Info, $"Configured {_globalVariables.Variables.Count} global variables for TeamChat");
            return this;
        }

        /// <summary>
        /// Adds an agent to the conversation with expertise description
        /// </summary>
        public TeamChat<TContext> WithAgent(string name, IAgent agent, string expertise)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Agent name cannot be empty", nameof(name));
            
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));

            if (string.IsNullOrWhiteSpace(expertise))
                throw new ArgumentException("Agent expertise description cannot be empty", nameof(expertise));

            var teamAgent = new TeamAgent
            {
                Name = name,
                Agent = agent,
                Expertise = expertise,
                IsActive = true
            };

            _agents[name] = teamAgent;
            
            _logger.Log(LogLevel.Info, $"Added agent '{name}' to TeamChat with expertise: {expertise}");
            return this;
        }

        /// <summary>
        /// Batch agent configuration using fluent builder
        /// </summary>
        public TeamChat<TContext> WithAgents(Action<IAgentBuilder<TContext>> configure)
        {
            var builder = new AgentBuilder<TContext>(this);
            configure(builder);
            return this;
        }

        /// <summary>
        /// Configura armazenamento avançado para a conversa
        /// </summary>
        public TeamChat<TContext> WithEnhancedStorage(IEnhancedStorage storage)
        {
            _enhancedStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            return this;
        }

        /// <summary>
        /// Habilita memória global para a conversa
        /// </summary>
        public TeamChat<TContext> EnableGlobalMemory(bool enabled = true, IMemory memory = null)
        {
            _globalMemoryEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Habilita histórico global de mensagens
        /// </summary>
        public TeamChat<TContext> EnableGlobalMessageHistory(bool enabled = true, int maxMessages = 1000)
        {
            _globalHistoryEnabled = enabled;
            return this;
        }



        /// <summary>
        /// Habilita modo de debug para seleção de agentes
        /// </summary>
        public TeamChat<TContext> EnableDebugMode(bool enabled = true)
        {
            base.WithDebugMode(enabled);
            return this;
        }

        #endregion

        #region Processamento de Conversa

        /// <summary>
        /// Processes user message with intelligent LLM-driven handoffs
        /// </summary>
        public async Task<string> ProcessMessageAsync(string userMessage, TContext context = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));

            lock (_conversationLock)
            {
                _messageHistory.Add(new ConversationMessage("user", userMessage, "user"));
            }

            try
            {
                // 1. Determine target agent (simple: use current or first available)
                var targetAgent = DetermineTargetAgent();
                
                if (targetAgent == null)
                {
                    var errorMsg = "No agents available. Please configure agents before processing messages.";
                    _logger.Log(LogLevel.Warning, errorMsg);
                    return errorMsg;
                }

                // 2. Perform handoff if agent was changed by previous interaction
                if (!string.IsNullOrEmpty(_pendingTransition))
                {
                    _agents.TryGetValue(_pendingTransition, out var newAgent);
                    if (newAgent != null)
                    {
                        await PerformIntelligentHandoffAsync(_currentAgent, newAgent);
                        targetAgent = newAgent;
                        _pendingTransition = null;
                    }
                }

                _currentAgent = targetAgent;

                // 3. Process message with agent
                var response = await ProcessMessageWithAgentAsync(userMessage, targetAgent, context);

                // 4. Add response to history
                lock (_conversationLock)
                {
                    _messageHistory.Add(new ConversationMessage(targetAgent.Name, response, "agent"));
                }

                // 5. Save state if storage available
                if (_enhancedStorage != null)
                {
                    await SaveConversationStateAsync();
                }

                // 6. Add to global history if enabled
                if (_globalHistoryEnabled)
                {
                    // TODO: Implement global message history
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error processing message: {ex.Message}", ex);
                var errorResponse = "I apologize, but an error occurred. Please try again.";
                
                lock (_conversationLock)
                {
                    _messageHistory.Add(new ConversationMessage("system", errorResponse, "system"));
                }

                return errorResponse;
            }
        }

        /// <summary>
        /// Simple agent determination - LLMs will handle the intelligent routing
        /// </summary>
        private TeamAgent DetermineTargetAgent()
        {
            if (!_agents.Any())
            {
                _logger.Log(LogLevel.Warning, "No agents configured for TeamChat");
                return null;
            }

            // Use current agent if available, otherwise use first agent
            return _currentAgent ?? _agents.Values.First();
        }

        /// <summary>
        /// Simplified handoff with context preservation
        /// </summary>
        private async Task PerformIntelligentHandoffAsync(TeamAgent fromAgent, TeamAgent toAgent)
        {
            var handoffMessage = fromAgent != null 
                ? $"Transferring conversation from {fromAgent.Name} to {toAgent.Name}"
                : $"Starting conversation with {toAgent.Name}";

            _logger.Log(LogLevel.Info, handoffMessage);

            // Add handoff message to history
            lock (_conversationLock)
            {
                _messageHistory.Add(new ConversationMessage("system", handoffMessage, "handoff"));
            }

            // Save handoff audit if storage available
            if (_enhancedStorage != null)
            {
                var auditEntry = new VariableAuditEntry
                {
                    SessionId = _conversationSessionId,
                    VariableName = "__handoff__",
                    OldValue = fromAgent?.Name,
                    NewValue = toAgent.Name,
                    ModifiedBy = "system",
                    ModifiedAt = DateTime.UtcNow,
                    Source = "intelligent_handoff",
                    Context = new Dictionary<string, object>
                    {
                        ["handoff_reason"] = "llm_decision",
                        ["message_count"] = _messageHistory.Count,
                        ["conversation_progress"] = Progress.CompletionPercentage
                    }
                };

                await _enhancedStorage.SaveVariableAuditAsync(_conversationSessionId, auditEntry);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Processes message with specific agent using intelligent context
        /// </summary>
        private async Task<string> ProcessMessageWithAgentAsync(string userMessage, TeamAgent agent, TContext context)
        {
            // 1. Set executing agent for ownership enforcement
            _globalVariables.SetCurrentExecutingAgent(agent.Name);

            // 2. Build intelligent system message with full context
            var systemMessage = _systemBuilder.BuildIntelligentSystemMessage(agent, _globalVariables, _agents, _messageHistory);

            // 3. Create transition context for agent decisions
            var transitionContext = new AgentTransitionContext<TContext>
            {
                Data = context,
                Variables = _globalVariables,
                History = _messageHistory.ToList(),
                AvailableAgents = _agents.Keys.ToArray(),
                CurrentAgent = agent.Name,
                OnTransition = (targetAgent) => _pendingTransition = targetAgent,
                OnComplete = (reason) => _pendingTransition = "completed"
            };

            // 4. Create enhanced toolpack with transition capabilities
            var enhancedToolPack = new IntelligentTransitionToolPack(
                _globalVariables, 
                transitionContext as AgentTransitionContext<object>, 
                _enhancedStorage, 
                _conversationSessionId);

            // 5. Execute agent with rich context
            var messages = new List<AIMessage>
            {
                AIMessage.System(systemMessage),
                AIMessage.User(userMessage)
            };

            var result = await agent.Agent.ExecuteAsync(userMessage, transitionContext, messages);
            
            var response = result?.ToString() ?? "I apologize, but I couldn't process your request.";

            // 6. Process any variable updates
            await ProcessVariableUpdatesAsync(agent, result);

            return response;
        }

        /// <summary>
        /// Processes variable updates after agent execution
        /// </summary>
        private async Task ProcessVariableUpdatesAsync(TeamAgent agent, object result)
        {
            // Log usage info if available
            if (result is AgentResult<object> agentResult && agentResult.Usage != null)
            {
                _logger.Log(LogLevel.Debug, $"Agent {agent.Name} used {agentResult.Usage.TotalTokens} tokens");
            }

            // Variables are now captured via intelligent ToolPack during execution
            // The LLM decides what variables to capture and when to transition
            await Task.CompletedTask;
        }


        #endregion

        #region Gerenciamento de Estado

        /// <summary>
        /// Salva o estado atual da conversa
        /// </summary>
        private async Task SaveConversationStateAsync()
        {
            if (_enhancedStorage == null) return;

            try
            {
                _conversationState = new ConversationState
                {
                    SessionId = _conversationSessionId,
                    UserId = base.UserId ?? "anonymous",
                    Variables = _globalVariables.Variables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    MessageHistory = _messageHistory.ToList(),
                    CurrentAgent = _currentAgent?.Name,
                    AvailableAgents = _agents.Values.Select(a => new ConversationAgent 
                    { 
                        Name = a.Name, 
                        Description = a.Expertise,
                        IsActive = a.IsActive 
                    }).ToList(),
                    LastActivity = DateTime.UtcNow,
                    Progress = Progress,
                    Status = ConversationStatus.Active,
                    SharedMemory = new Dictionary<string, object>
                    {
                        ["global_memory_enabled"] = _globalMemoryEnabled,
                        ["global_history_enabled"] = _globalHistoryEnabled,
                        ["agent_count"] = _agents.Count
                    }
                };

                await _enhancedStorage.SaveConversationStateAsync(_conversationSessionId, _conversationState);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Failed to save conversation state: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega estado de uma conversa existente
        /// </summary>
        public async Task LoadConversationStateAsync(string sessionId)
        {
            if (_enhancedStorage == null)
                throw new InvalidOperationException("Enhanced storage not configured");

            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID não pode ser vazio", nameof(sessionId));

            try
            {
                var state = await _enhancedStorage.LoadConversationStateAsync(sessionId);
                if (state == null)
                {
                    _logger.Log(LogLevel.Warning, $"No conversation state found for session {sessionId}");
                    return;
                }

                _conversationSessionId = sessionId;
                _conversationState = state;

                // Restaurar variáveis
                _globalVariables.Clear();
                foreach (var variable in state.Variables.Values)
                {
                    _globalVariables.ConfigureVariable(variable.Name, variable.OwnedBy, variable.Description, 
                        variable.IsRequired, variable.DefaultValue);
                    
                    if (variable.IsCollected)
                    {
                        _globalVariables.SetCurrentExecutingAgent(variable.CapturedBy);
                        _globalVariables.SetVariable(variable.Name, variable.Value, variable.Confidence);
                    }
                }

                // Restaurar histórico
                lock (_conversationLock)
                {
                    _messageHistory.Clear();
                    _messageHistory.AddRange(state.MessageHistory);
                }

                // Restaurar agente atual
                if (!string.IsNullOrEmpty(state.CurrentAgent) && _agents.ContainsKey(state.CurrentAgent))
                {
                    _currentAgent = _agents[state.CurrentAgent];
                }

                _logger.Log(LogLevel.Info, $"Loaded conversation state for session {sessionId} with {state.Variables.Count} variables and {state.MessageHistory.Count} messages");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Failed to load conversation state: {ex.Message}", ex);
                throw;
            }
        }

        #endregion

        #region Funcionalidades Públicas

        /// <summary>
        /// Obtém o valor de uma variável global
        /// </summary>
        public T GetVariable<T>(string variableName)
        {
            return _globalVariables.GetVariable<T>(variableName);
        }

        /// <summary>
        /// Verifica se uma variável foi coletada
        /// </summary>
        public bool IsVariableCollected(string variableName)
        {
            return _globalVariables.HasVariable(variableName) && 
                   _globalVariables.Variables[variableName].IsCollected;
        }

        /// <summary>
        /// Obtém variáveis em falta para um agente específico
        /// </summary>
        public List<GlobalVariable> GetMissingVariables(string agentName = null)
        {
            return _globalVariables.GetMissingVariables(agentName ?? _currentAgent?.Name ?? "any");
        }

        /// <summary>
        /// Força a captura de uma variável (para testes ou casos especiais)
        /// </summary>
        public void SetVariable(string variableName, object value, string capturedBy = null, double confidence = 1.0)
        {
            var originalAgent = _globalVariables.CurrentExecutingAgent;
            try
            {
                _globalVariables.SetCurrentExecutingAgent(capturedBy ?? "system");
                _globalVariables.SetVariable(variableName, value, confidence);
            }
            finally
            {
                _globalVariables.SetCurrentExecutingAgent(originalAgent);
            }
        }

        /// <summary>
        /// Marca a conversa como completa
        /// </summary>
        public async Task CompleteConversationAsync(string completionReason = null)
        {
            if (_enhancedStorage != null)
            {
                await _enhancedStorage.MarkConversationCompleteAsync(_conversationSessionId, completionReason);
            }

            _logger.Log(LogLevel.Info, $"Conversation {_conversationSessionId} marked as complete: {completionReason}");
        }

        /// <summary>
        /// Obtém resumo da conversa
        /// </summary>
        public ConversationSummary GetConversationSummary()
        {
            return new ConversationSummary
            {
                SessionId = _conversationSessionId,
                TotalMessages = _messageHistory.Count,
                CurrentAgent = _currentAgent?.Name,
                Progress = Progress,
                VariablesSummary = _globalVariables.Variables.Values.GroupBy(v => v.OwnedBy)
                    .ToDictionary(g => g.Key, g => (object)new Dictionary<string, object> 
                    { 
                        ["Total"] = g.Count(), 
                        ["Collected"] = g.Count(v => v.IsCollected) 
                    }),
                StartTime = _messageHistory.FirstOrDefault()?.Timestamp ?? DateTime.UtcNow,
                LastActivity = _messageHistory.LastOrDefault()?.Timestamp ?? DateTime.UtcNow,
                IsComplete = Progress.IsComplete
            };
        }

        #region Additional Methods for Console Compatibility
        
        /// <summary>
        /// Gets the currently active agent
        /// </summary>
        public TeamAgent GetCurrentAgent()
        {
            return _currentAgent;
        }

        /// <summary>
        /// Gets the global variables collection
        /// </summary>
        public GlobalVariableCollection GetGlobalVariables()
        {
            return _globalVariables;
        }

        /// <summary>
        /// Adds an agent using the builder pattern expected by console
        /// </summary>
        public TeamChat<TContext> AddAgent(string name, IAgent agent, string expertise, int priority = 5)
        {
            return WithAgent(name, agent, expertise);
        }

        /// <summary>
        /// Sets the initial agent for the conversation
        /// </summary>
        public TeamChat<TContext> SetInitialAgent(string agentName)
        {
            if (_agents.TryGetValue(agentName, out var agent))
            {
                _currentAgent = agent;
            }
            return this;
        }

        /// <summary>
        /// Checks if the conversation is complete
        /// </summary>
        public bool IsConversationComplete => Progress.IsComplete;

        /// <summary>
        /// Creates a new session (resets conversation state)
        /// </summary>
        public async Task CreateNewSessionAsync()
        {
            _conversationSessionId = Guid.NewGuid().ToString("N");
            _messageHistory.Clear();
            _globalVariables.Clear();
            _currentAgent = _agents.Values.FirstOrDefault();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sets user ID for the workflow
        /// </summary>
        public new TeamChat<TContext> WithUserId(string userId)
        {
            this.UserId = userId;
            return this;
        }

        #endregion

        #endregion

        #region Override ExecuteAsync (compatibilidade com AdvancedWorkflow)

        public override async Task<TContext> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
        {
            // Para TeamChat, ExecuteAsync não faz sentido da mesma forma que workflows sequenciais
            // Em vez disso, retornamos o contexto inalterado e registramos que o TeamChat está pronto
            _logger.Log(LogLevel.Info, $"TeamChat '{_name}' initialized and ready for conversation processing");
            
            // Carregar estado global se habilitado
            if (_globalMemoryEnabled || _globalHistoryEnabled)
            {
                // TODO: Load global state if needed
                await Task.CompletedTask; // Suppress warning
            }

            return context;
        }

        #endregion


        #region Modelos Auxiliares

        /// <summary>
        /// Resumo da conversa
        /// </summary>
        public class ConversationSummary
        {
            public string SessionId { get; set; }
            public int TotalMessages { get; set; }
            public string CurrentAgent { get; set; }
            public ConversationProgress Progress { get; set; }
            public Dictionary<string, object> VariablesSummary { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime LastActivity { get; set; }
            public bool IsComplete { get; set; }
        }

        #endregion
    }

}