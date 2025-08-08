using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Mensagem armazenada para compatibilidade
    /// </summary>
    public class StoredMessage
    {
        /// <summary>
        /// ID único da mensagem
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
        /// Role da mensagem (User, Assistant, System)
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Data e hora da mensagem
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Metadados da mensagem
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Indica se a mensagem foi processada
        /// </summary>
        public bool IsProcessed { get; set; } = false;

        /// <summary>
        /// Embedding da mensagem para busca semântica
        /// </summary>
        public List<float> Embedding { get; set; }

        public StoredMessage() { }

        public StoredMessage(AIMessage aiMessage, string sessionId, string userId)
        {
            SessionId = sessionId;
            UserId = userId;
            Role = aiMessage.Role;
            Content = aiMessage.Content;
            Timestamp = DateTime.UtcNow;
        }
    }
}
