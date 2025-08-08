using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa um registro de memória do agente (compatibilidade backward)
    /// </summary>
    public class AgentMemoryRecord
    {
        /// <summary>
        /// ID único do registro
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Conteúdo da memória
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// ID do usuário
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ID da sessão
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Metadados
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public AgentMemoryRecord() { }

        public AgentMemoryRecord(string content, string userId, string sessionId = null)
        {
            Content = content;
            UserId = userId;
            SessionId = sessionId;
        }
    }
}
