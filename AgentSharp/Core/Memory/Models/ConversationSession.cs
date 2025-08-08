using System;
using System.Collections.Generic;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Implementação concreta de sessão de conversação
    /// </summary>
    public class ConversationSession : ISession
    {
        /// <summary>
        /// ID único da sessão
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ID do usuário proprietário da sessão
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Título da sessão
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Data e hora de criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora da última atividade
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Metadados adicionais da sessão
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Status da sessão (ativa, arquivada, etc.)
        /// </summary>
        public Interfaces.SessionStatus Status { get; set; } = Interfaces.SessionStatus.Active;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public ConversationSession()
        {
        }

        /// <summary>
        /// Construtor com parâmetros essenciais
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="title">Título da sessão</param>
        public ConversationSession(string userId, string title = null)
        {
            UserId = userId;
            Title = title ?? $"Session {DateTime.Now:yyyy-MM-dd HH:mm}";
        }

        /// <summary>
        /// Atualiza a última atividade da sessão
        /// </summary>
        public void UpdateLastActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        /// <summary>
        /// Converte a sessão para dicionário para serialização
        /// </summary>
        /// <returns>Dicionário com dados da sessão</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["userId"] = UserId,
                ["title"] = Title,
                ["createdAt"] = CreatedAt,
                ["lastActivity"] = LastActivity,
                ["metadata"] = Metadata,
                ["status"] = Status.ToString()
            };
        }

        /// <summary>
        /// Cria uma sessão a partir de um dicionário
        /// </summary>
        /// <param name="dict">Dicionário com dados da sessão</param>
        /// <returns>Nova instância de ConversationSession</returns>
        public static ConversationSession FromDictionary(Dictionary<string, object> dict)
        {
            var session = new ConversationSession();

            if (dict.ContainsKey("id")) session.Id = dict["id"]?.ToString();
            if (dict.ContainsKey("userId")) session.UserId = dict["userId"]?.ToString();
            if (dict.ContainsKey("title")) session.Title = dict["title"]?.ToString();
            if (dict.ContainsKey("createdAt") && DateTime.TryParse(dict["createdAt"]?.ToString(), out var createdAt))
                session.CreatedAt = createdAt;
            if (dict.ContainsKey("lastActivity") && DateTime.TryParse(dict["lastActivity"]?.ToString(), out var lastActivity))
                session.LastActivity = lastActivity;
            if (dict.ContainsKey("metadata") && dict["metadata"] is Dictionary<string, object> metadata)
                session.Metadata = metadata;
            if (dict.ContainsKey("status") && Enum.TryParse<Interfaces.SessionStatus>(dict["status"]?.ToString(), out var status))
                session.Status = status;

            return session;
        }

        public override string ToString()
        {
            return $"Session[{Id}]: {Title} - User: {UserId}, Status: {Status}";
        }
    }
}
