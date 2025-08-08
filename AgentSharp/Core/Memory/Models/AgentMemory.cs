using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa a memória completa de um agente, incluindo sessões e resumos
    /// </summary>
    public class AgentMemory
    {
        /// <summary>
        /// Identificador único da memória do agente
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID da sessão atual
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// ID do usuário proprietário
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Nome do agente
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Lista de execuções (runs) do agente
        /// </summary>
        public List<AgentRun> Runs { get; set; }

        /// <summary>
        /// Lista de mensagens da sessão atual
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Memórias do usuário carregadas
        /// </summary>
        public List<Memory> UserMemories { get; set; }

        /// <summary>
        /// Resumos de sessões anteriores
        /// </summary>
        public List<SessionSummary> SessionSummaries { get; set; }

        /// <summary>
        /// Contexto atual da conversa
        /// </summary>
        public string CurrentContext { get; set; }

        /// <summary>
        /// Dados específicos da sessão
        /// </summary>
        public Dictionary<string, object> SessionData { get; set; }

        /// <summary>
        /// Configurações de memória
        /// </summary>
        public MemorySettings Settings { get; set; }

        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Data do último acesso
        /// </summary>
        public DateTime LastAccessedAt { get; set; }

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public AgentMemory()
        {
            Id = Guid.NewGuid().ToString();
            Runs = new List<AgentRun>();
            Messages = new List<Message>();
            UserMemories = new List<Memory>();
            SessionSummaries = new List<SessionSummary>();
            SessionData = new Dictionary<string, object>();
            Metadata = new Dictionary<string, object>();
            Settings = new MemorySettings();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastAccessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Construtor com parâmetros essenciais
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="agentName">Nome do agente</param>
        public AgentMemory(string sessionId, string userId, string agentName = null) : this()
        {
            SessionId = sessionId;
            UserId = userId;
            AgentName = agentName;
        }

        /// <summary>
        /// Adiciona uma execução (run) à memória
        /// </summary>
        /// <param name="run">Execução a ser adicionada</param>
        public void AddRun(AgentRun run)
        {
            if (run != null)
            {
                Runs.Add(run);
                UpdatedAt = DateTime.UtcNow;

                // Limitar número de runs se configurado
                if (Settings.MaxRunsPerSession > 0 && Runs.Count > Settings.MaxRunsPerSession)
                {
                    var toRemove = Runs.Count - Settings.MaxRunsPerSession;
                    Runs.RemoveRange(0, toRemove);
                }
            }
        }

        /// <summary>
        /// Adiciona uma mensagem do sistema
        /// </summary>
        /// <param name="content">Conteúdo da mensagem</param>
        /// <param name="shouldUpdateMemory">Se deve atualizar memórias baseado nesta mensagem</param>
        public void AddSystemMessage(string content, bool shouldUpdateMemory = true)
        {
            var message = new Message
            {
                Role = AgentSharp.Core.Role.System.ToString(),
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            AddMessage(message);

            if (shouldUpdateMemory)
            {
                // Lógica para determinar se deve criar uma memória permanente
                // Isso será implementado quando tivermos o IMemoryClassifier
            }
        }

        /// <summary>
        /// Adiciona uma mensagem à lista
        /// </summary>
        /// <param name="message">Mensagem a ser adicionada</param>
        public void AddMessage(Message message)
        {
            if (message != null)
            {
                Messages.Add(message);
                UpdatedAt = DateTime.UtcNow;

                // Limitar número de mensagens se configurado
                if (Settings.MaxMessagesPerSession > 0 && Messages.Count > Settings.MaxMessagesPerSession)
                {
                    var toRemove = Messages.Count - Settings.MaxMessagesPerSession;
                    Messages.RemoveRange(0, toRemove);
                }
            }
        }

        /// <summary>
        /// Adiciona múltiplas mensagens
        /// </summary>
        /// <param name="messages">Mensagens a serem adicionadas</param>
        public void AddMessages(IEnumerable<Message> messages)
        {
            if (messages != null)
            {
                foreach (var message in messages)
                {
                    AddMessage(message);
                }
            }
        }

        /// <summary>
        /// Obtém todas as mensagens
        /// </summary>
        /// <returns>Lista de mensagens</returns>
        public List<Message> GetMessages()
        {
            return Messages.ToList();
        }

        /// <summary>
        /// Obtém mensagens dos últimos N runs
        /// </summary>
        /// <param name="runCount">Número de runs</param>
        /// <returns>Lista de mensagens</returns>
        public List<Message> GetMessagesFromLastNRuns(int runCount)
        {
            var lastRuns = Runs.Skip(Math.Max(0, Runs.Count - runCount)).ToList();
            var messages = new List<Message>();

            foreach (var run in lastRuns)
            {
                if (run.Message != null)
                    messages.Add(run.Message);

                if (run.Messages != null)
                    messages.AddRange(run.Messages);
            }

            return messages;
        }

        /// <summary>
        /// Obtém pares de mensagens (pergunta-resposta)
        /// </summary>
        /// <param name="limit">Limite de pares</param>
        /// <returns>Lista de pares de mensagens</returns>
        public List<(Message User, Message Assistant)> GetMessagePairs(int limit = 10)
        {
            var pairs = new List<(Message User, Message Assistant)>();
            Message lastUserMessage = null;

            foreach (var message in Messages.Skip(Math.Max(0, Messages.Count - limit * 2)).ToList())
            {
                if (message.Role == AgentSharp.Core.Role.User.ToString())
                {
                    lastUserMessage = message;
                }
                else if (message.Role == AgentSharp.Core.Role.Assistant.ToString() && lastUserMessage != null)
                {
                    pairs.Add((lastUserMessage, message));
                    lastUserMessage = null;
                }
            }

            return pairs.Skip(Math.Max(0, pairs.Count - limit)).ToList();
        }

        /// <summary>
        /// Obtém chamadas de ferramentas das mensagens
        /// </summary>
        /// <returns>Lista de chamadas de ferramentas</returns>
        public List<object> GetToolCalls()
        {
            var toolCalls = new List<object>();

            foreach (var message in Messages)
            {
                if (message.FunctionCalls != null)
                    toolCalls.AddRange(message.FunctionCalls);
            }

            return toolCalls;
        }

        /// <summary>
        /// Carrega memórias do usuário (será implementado quando tivermos o IMemoryService)
        /// </summary>
        /// <param name="limit">Limite de memórias</param>
        /// <returns>Task para operação assíncrona</returns>
        public async System.Threading.Tasks.Task LoadUserMemoriesAsync(int limit = 10)
        {
            // Implementação será feita quando tivermos o IMemoryService
            await System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Determina se deve atualizar memórias baseado no contexto atual
        /// </summary>
        /// <returns>True se deve atualizar</returns>
        public bool ShouldUpdateMemory()
        {
            // Lógica básica - pode ser expandida
            return Messages.Count >= Settings.MinMessagesForMemoryUpdate;
        }

        /// <summary>
        /// Atualiza as memórias do usuário (será implementado quando tivermos o IMemoryService)
        /// </summary>
        /// <returns>Task para operação assíncrona</returns>
        public async System.Threading.Tasks.Task UpdateMemoryAsync()
        {
            // Implementação será feita quando tivermos o IMemoryService
            await System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Atualiza o resumo da sessão atual
        /// </summary>
        /// <returns>Task para operação assíncrona</returns>
        public async System.Threading.Tasks.Task UpdateSummaryAsync()
        {
            // Implementação será feita quando tivermos o IMemoryService
            await System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Limpa dados temporários da sessão
        /// </summary>
        public void Clear()
        {
            Messages.Clear();
            Runs.Clear();
            SessionData.Clear();
            CurrentContext = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Converte a memória do agente para dicionário para serialização
        /// </summary>
        /// <returns>Dicionário com os dados</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["sessionId"] = SessionId,
                ["userId"] = UserId,
                ["agentName"] = AgentName,
                ["runs"] = Runs != null ? Runs.ConvertAll(r => r.ToDictionary()) : new List<Dictionary<string, object>>(),
                ["messages"] = Messages != null ? Messages.ConvertAll(m => m.ToDictionary()) : new List<Dictionary<string, object>>(),
                ["userMemories"] = UserMemories != null ? UserMemories.ConvertAll(m => m.ToDictionary()) : new List<Dictionary<string, object>>(),
                ["sessionSummaries"] = SessionSummaries != null ? SessionSummaries.ConvertAll(s => s.ToDictionary()) : new List<Dictionary<string, object>>(),
                ["currentContext"] = CurrentContext,
                ["sessionData"] = SessionData ?? new Dictionary<string, object>(),
                ["settings"] = Settings != null ? Settings.ToDictionary() : new Dictionary<string, object>(),
                ["createdAt"] = CreatedAt,
                ["updatedAt"] = UpdatedAt,
                ["lastAccessedAt"] = LastAccessedAt,
                ["metadata"] = Metadata ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Marca a memória como acessada
        /// </summary>
        public void MarkAsAccessed()
        {
            LastAccessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Representação em string da memória do agente
        /// </summary>
        /// <returns>String representando a memória</returns>
        public override string ToString()
        {
            return $"AgentMemory[{Id}]: Session={SessionId}, User={UserId}, Runs={Runs.Count}, Messages={Messages.Count}, Memories={UserMemories.Count}";
        }
    }

    /// <summary>
    /// Configurações de memória
    /// </summary>
    public class MemorySettings
    {
        /// <summary>
        /// Máximo de mensagens por sessão
        /// </summary>
        public int MaxMessagesPerSession { get; set; } = 100;

        /// <summary>
        /// Máximo de runs por sessão
        /// </summary>
        public int MaxRunsPerSession { get; set; } = 50;

        /// <summary>
        /// Mínimo de mensagens para atualizar memória
        /// </summary>
        public int MinMessagesForMemoryUpdate { get; set; } = 10;

        /// <summary>
        /// Habilitar auto-resumo
        /// </summary>
        public bool EnableAutoSummary { get; set; } = true;

        /// <summary>
        /// Limite de relevância mínima
        /// </summary>
        public double MinRelevanceThreshold { get; set; } = 0.5;

        /// <summary>
        /// Converte as configurações para dicionário
        /// </summary>
        /// <returns>Dicionário com as configurações</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["maxMessagesPerSession"] = MaxMessagesPerSession,
                ["maxRunsPerSession"] = MaxRunsPerSession,
                ["minMessagesForMemoryUpdate"] = MinMessagesForMemoryUpdate,
                ["enableAutoSummary"] = EnableAutoSummary,
                ["minRelevanceThreshold"] = MinRelevanceThreshold
            };
        }
    }
}
