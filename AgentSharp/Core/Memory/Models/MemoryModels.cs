using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa uma sessão de agente
    /// </summary>
public class AgentSession : AgentSharp.Core.Memory.Interfaces.ISession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Title { get; set; } // ISession
        public string Name { get => Title; set => Title = value; } // Compatibilidade
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime? LastAccessedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        public List<string> Tags { get; set; } = new List<string>();
        public AgentSharp.Core.Memory.Interfaces.SessionStatus Status { get; set; } = AgentSharp.Core.Memory.Interfaces.SessionStatus.Active;
    }

    /// <summary>
    /// Representa uma mensagem armazenada
    /// </summary>
    public class Message // Removido herança para evitar conflito, ajuste conforme necessário
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; } // Usar string para evitar conflito, ajuste para enum se necessário
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public bool IsProcessed { get; set; } = false;
        public List<float> Embedding { get; set; }

        public List<String> FunctionCalls { get; set; } = new List<String>();


        public Message() { }

        public Message(string role, string content, string sessionId, string userId)
        {
            SessionId = sessionId;
            UserId = userId;
            Role = role;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Converte a mensagem para dicionário
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["sessionId"] = SessionId,
                ["userId"] = UserId,
                ["role"] = Role,
                ["content"] = Content,
                ["timestamp"] = Timestamp,
                ["metadata"] = Metadata,
                ["isProcessed"] = IsProcessed,
                ["embedding"] = Embedding,
                ["functionCalls"] = FunctionCalls
            };
        }
  }


}
