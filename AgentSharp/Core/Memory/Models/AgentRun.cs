using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Status de uma execução
    /// </summary>
    public enum RunStatus
    {
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Representa uma execução (run) do agente
    /// </summary>
    public class AgentRun
    {
        /// <summary>
        /// ID único da execução
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ID da sessão
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// ID do usuário
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Mensagem principal da execução
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Lista de mensagens da execução
        /// </summary>
        public List<Message> Messages { get; set; } = new List<Message>();

        /// <summary>
        /// Data de início
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data de conclusão
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Status da execução
        /// </summary>
        public string Status { get; set; } = "Running";

        /// <summary>
        /// Resultado da execução
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Metadados da execução
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Tokens usados na execução
        /// </summary>
        public int TokensUsed { get; set; } = 0;

        /// <summary>
        /// Custo da execução
        /// </summary>
        public decimal Cost { get; set; } = 0;

        /// <summary>
        /// Duração da execução
        /// </summary>
        public TimeSpan Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : TimeSpan.Zero;

        public AgentRun() { }

        public AgentRun(string sessionId, string userId)
        {
            SessionId = sessionId;
            UserId = userId;
        }

        /// <summary>
        /// Converte para dicionário
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["sessionId"] = SessionId,
                ["userId"] = UserId,
                ["message"] = Message?.ToDictionary(),
                ["messages"] = Messages?.ConvertAll(m => m.ToDictionary()),
                ["startedAt"] = StartedAt,
                ["completedAt"] = CompletedAt,
                ["status"] = Status,
                ["result"] = Result,
                ["metadata"] = Metadata
            };
        }
    }
}
