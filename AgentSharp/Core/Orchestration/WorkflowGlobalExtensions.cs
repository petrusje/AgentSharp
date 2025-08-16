using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Orchestration;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Extensões para adicionar funcionalidades globais a qualquer workflow
    /// </summary>
    public static class WorkflowGlobalExtensions
    {
        private static readonly Dictionary<string, GlobalWorkflowFeatures> _workflowFeatures = 
            new Dictionary<string, GlobalWorkflowFeatures>();

        /// <summary>
        /// Adiciona suporte a memória global a um workflow
        /// </summary>
        public static T WithGlobalMemory<T>(this T workflow, bool enabled = true, IMemory memory = null) 
            where T : class, IWorkflow<object>
        {
            var features = GetOrCreateFeatures(workflow);
            features.GlobalMemoryEnabled = enabled;
            features.GlobalMemory = memory;
            return workflow;
        }

        /// <summary>
        /// Adiciona suporte a histórico global de mensagens a um workflow
        /// </summary>
        public static T WithGlobalMessageHistory<T>(this T workflow, bool enabled = true, int maxMessages = 1000) 
            where T : class, IWorkflow<object>
        {
            var features = GetOrCreateFeatures(workflow);
            features.GlobalHistoryEnabled = enabled;
            features.MaxGlobalMessages = maxMessages;
            return workflow;
        }

        /// <summary>
        /// Adiciona suporte a armazenamento avançado a um workflow
        /// </summary>
        public static T WithEnhancedStorage<T>(this T workflow, IEnhancedStorage storage) 
            where T : class, IWorkflow<object>
        {
            var features = GetOrCreateFeatures(workflow);
            features.EnhancedStorage = storage;
            return workflow;
        }

        /// <summary>
        /// Configura políticas de retenção para o workflow
        /// </summary>
        public static T WithRetentionPolicy<T>(this T workflow, RetentionPolicy policy) 
            where T : class, IWorkflow<object>
        {
            var features = GetOrCreateFeatures(workflow);
            features.RetentionPolicy = policy;
            return workflow;
        }

        /// <summary>
        /// Obtém as funcionalidades globais de um workflow
        /// </summary>
        public static GlobalWorkflowFeatures GetGlobalFeatures<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var workflowId = GetWorkflowId(workflow);
            return _workflowFeatures.TryGetValue(workflowId, out var features) ? features : null;
        }

        /// <summary>
        /// Verifica se um workflow tem memória global habilitada
        /// </summary>
        public static bool HasGlobalMemory<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            return features?.GlobalMemoryEnabled == true;
        }

        /// <summary>
        /// Verifica se um workflow tem histórico global habilitado
        /// </summary>
        public static bool HasGlobalHistory<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            return features?.GlobalHistoryEnabled == true;
        }

        /// <summary>
        /// Obtém a memória global de um workflow
        /// </summary>
        public static IMemory GetGlobalMemory<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            return features?.GlobalMemory;
        }

        /// <summary>
        /// Obtém o histórico global de mensagens de um workflow
        /// </summary>
        public static List<GlobalMessage> GetGlobalMessageHistory<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            return features?.GlobalMessageHistory ?? new List<GlobalMessage>();
        }

        /// <summary>
        /// Adiciona uma mensagem ao histórico global
        /// </summary>
        public static async Task AddGlobalMessageAsync<T>(this T workflow, string agentName, string message, string messageType = "agent") 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            if (features?.GlobalHistoryEnabled != true) return;

            var globalMessage = new GlobalMessage
            {
                AgentName = agentName,
                Content = message,
                MessageType = messageType,
                Timestamp = DateTime.UtcNow,
                WorkflowName = GetWorkflowName(workflow)
            };

            features.GlobalMessageHistory.Add(globalMessage);

            // Limitar tamanho do histórico
            if (features.GlobalMessageHistory.Count > features.MaxGlobalMessages)
            {
                features.GlobalMessageHistory.RemoveAt(0);
            }

            // Salvar no armazenamento se disponível
            if (features.EnhancedStorage != null)
            {
                await SaveGlobalMessageAsync(features.EnhancedStorage, globalMessage);
            }
        }

        /// <summary>
        /// Carrega histórico global do armazenamento
        /// </summary>
        public static async Task LoadGlobalHistoryAsync<T>(this T workflow, string sessionId) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            if (features?.EnhancedStorage == null) return;

            try
            {
                var messages = await LoadGlobalMessagesAsync(features.EnhancedStorage, sessionId);
                features.GlobalMessageHistory.Clear();
                features.GlobalMessageHistory.AddRange(messages);
            }
            catch (Exception ex)
            {
                // Log erro mas não falhe o workflow
                Console.WriteLine($"Failed to load global history: {ex.Message}");
            }
        }

        /// <summary>
        /// Salva estado global do workflow
        /// </summary>
        public static async Task SaveGlobalStateAsync<T>(this T workflow, string sessionId) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            if (features?.EnhancedStorage == null) return;

            var globalState = new GlobalWorkflowState
            {
                SessionId = sessionId,
                WorkflowName = GetWorkflowName(workflow),
                GlobalMemoryData = features.GlobalMemory != null ? await SerializeMemoryAsync(features.GlobalMemory) : null,
                MessageHistory = features.GlobalMessageHistory,
                LastActivity = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["workflow_type"] = workflow.GetType().Name,
                    ["global_memory_enabled"] = features.GlobalMemoryEnabled,
                    ["global_history_enabled"] = features.GlobalHistoryEnabled
                }
            };

            await SaveGlobalWorkflowStateAsync(features.EnhancedStorage, globalState);
        }

        /// <summary>
        /// Carrega estado global do workflow
        /// </summary>
        public static async Task LoadGlobalStateAsync<T>(this T workflow, string sessionId) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            if (features?.EnhancedStorage == null) return;

            try
            {
                var globalState = await LoadGlobalWorkflowStateAsync(features.EnhancedStorage, sessionId);
                if (globalState != null)
                {
                    if (features.GlobalHistoryEnabled && globalState.MessageHistory != null)
                    {
                        features.GlobalMessageHistory.Clear();
                        features.GlobalMessageHistory.AddRange(globalState.MessageHistory);
                    }

                    if (features.GlobalMemoryEnabled && features.GlobalMemory != null && globalState.GlobalMemoryData != null)
                    {
                        await DeserializeMemoryAsync(features.GlobalMemory, globalState.GlobalMemoryData);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log erro mas não falhe o workflow
                Console.WriteLine($"Failed to load global state: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpa dados globais de um workflow
        /// </summary>
        public static async Task ClearGlobalDataAsync<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var features = GetGlobalFeatures(workflow);
            if (features == null) return;

            features.GlobalMessageHistory.Clear();
            
            if (features.GlobalMemory != null)
            {
                try
                {
                    await features.GlobalMemory.ClearAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear global memory: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Remove funcionalidades globais de um workflow
        /// </summary>
        public static void RemoveGlobalFeatures<T>(this T workflow) 
            where T : class, IWorkflow<object>
        {
            var workflowId = GetWorkflowId(workflow);
            _workflowFeatures.Remove(workflowId);
        }

        #region Métodos privados auxiliares

        private static GlobalWorkflowFeatures GetOrCreateFeatures<T>(T workflow) 
            where T : class, IWorkflow<object>
        {
            var workflowId = GetWorkflowId(workflow);
            
            if (!_workflowFeatures.TryGetValue(workflowId, out var features))
            {
                features = new GlobalWorkflowFeatures
                {
                    WorkflowId = workflowId,
                    WorkflowName = GetWorkflowName(workflow)
                };
                _workflowFeatures[workflowId] = features;
            }

            return features;
        }

        private static string GetWorkflowId<T>(T workflow) where T : class
        {
            // Usar hash do objeto para identificar unicamente o workflow
            return workflow.GetHashCode().ToString();
        }

        private static string GetWorkflowName<T>(T workflow) where T : class
        {
            // Tentar obter nome do workflow se disponível
            if (workflow is Workflow<object> baseWorkflow)
            {
                return baseWorkflow.Name;
            }
            return workflow.GetType().Name;
        }

        private static async Task SaveGlobalMessageAsync(IEnhancedStorage storage, GlobalMessage message)
        {
            try
            {
                // Implementar salvamento de mensagem global
                // Para simplificação inicial, usar session storage
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save global message: {ex.Message}");
            }
        }

        private static async Task<List<GlobalMessage>> LoadGlobalMessagesAsync(IEnhancedStorage storage, string sessionId)
        {
            try
            {
                // Implementar carregamento de mensagens globais
                await Task.CompletedTask;
                return new List<GlobalMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load global messages: {ex.Message}");
                return new List<GlobalMessage>();
            }
        }

        private static async Task SaveGlobalWorkflowStateAsync(IEnhancedStorage storage, GlobalWorkflowState state)
        {
            try
            {
                // Salvar estado global usando o enhanced storage
                var conversationState = new ConversationState
                {
                    SessionId = state.SessionId,
                    SharedMemory = state.GlobalMemoryData ?? new Dictionary<string, object>(),
                    MessageHistory = state.MessageHistory?.ConvertAll(m => new ConversationMessage
                    {
                        AgentName = m.AgentName,
                        Content = m.Content,
                        MessageType = m.MessageType,
                        Timestamp = m.Timestamp
                    }) ?? new List<ConversationMessage>(),
                    LastActivity = state.LastActivity,
                    Metadata = state.Metadata
                };

                await storage.SaveConversationStateAsync(state.SessionId, conversationState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save global workflow state: {ex.Message}");
            }
        }

        private static async Task<GlobalWorkflowState> LoadGlobalWorkflowStateAsync(IEnhancedStorage storage, string sessionId)
        {
            try
            {
                var conversationState = await storage.LoadConversationStateAsync(sessionId);
                if (conversationState == null) return null;

                return new GlobalWorkflowState
                {
                    SessionId = sessionId,
                    GlobalMemoryData = conversationState.SharedMemory,
                    MessageHistory = conversationState.MessageHistory?.ConvertAll(m => new GlobalMessage
                    {
                        AgentName = m.AgentName,
                        Content = m.Content,
                        MessageType = m.MessageType,
                        Timestamp = m.Timestamp
                    }) ?? new List<GlobalMessage>(),
                    LastActivity = conversationState.LastActivity,
                    Metadata = conversationState.Metadata
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load global workflow state: {ex.Message}");
                return null;
            }
        }

        private static async Task<Dictionary<string, object>> SerializeMemoryAsync(IMemory memory)
        {
            try
            {
                // Implementar serialização da memória
                // Para versão inicial, retornar dados básicos
                await Task.CompletedTask;
                return new Dictionary<string, object>
                {
                    ["serialized_at"] = DateTime.UtcNow,
                    ["memory_type"] = memory.GetType().Name
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to serialize memory: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        private static async Task DeserializeMemoryAsync(IMemory memory, Dictionary<string, object> data)
        {
            try
            {
                // Implementar deserialização da memória
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize memory: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Funcionalidades globais habilitadas para um workflow
    /// </summary>
    public class GlobalWorkflowFeatures
    {
        /// <summary>
        /// ID único do workflow
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// Nome do workflow
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Se memória global está habilitada
        /// </summary>
        public bool GlobalMemoryEnabled { get; set; } = false;

        /// <summary>
        /// Se histórico global está habilitado
        /// </summary>
        public bool GlobalHistoryEnabled { get; set; } = false;

        /// <summary>
        /// Instância de memória global
        /// </summary>
        public IMemory GlobalMemory { get; set; }

        /// <summary>
        /// Histórico global de mensagens
        /// </summary>
        public List<GlobalMessage> GlobalMessageHistory { get; set; } = new List<GlobalMessage>();

        /// <summary>
        /// Número máximo de mensagens no histórico global
        /// </summary>
        public int MaxGlobalMessages { get; set; } = 1000;

        /// <summary>
        /// Armazenamento avançado se disponível
        /// </summary>
        public IEnhancedStorage EnhancedStorage { get; set; }

        /// <summary>
        /// Política de retenção de dados
        /// </summary>
        public RetentionPolicy RetentionPolicy { get; set; }

        /// <summary>
        /// Timestamp da última atividade
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Mensagem global do workflow
    /// </summary>
    public class GlobalMessage
    {
        /// <summary>
        /// ID único da mensagem
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nome do agente que enviou a mensagem
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Tipo da mensagem (user, agent, system, workflow)
        /// </summary>
        public string MessageType { get; set; } = "agent";

        /// <summary>
        /// Timestamp da mensagem
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nome do workflow de origem
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Estado global de um workflow
    /// </summary>
    public class GlobalWorkflowState
    {
        /// <summary>
        /// ID da sessão
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Nome do workflow
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Dados serializados da memória global
        /// </summary>
        public Dictionary<string, object> GlobalMemoryData { get; set; }

        /// <summary>
        /// Histórico de mensagens globais
        /// </summary>
        public List<GlobalMessage> MessageHistory { get; set; }

        /// <summary>
        /// Timestamp da última atividade
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Metadados do estado global
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}