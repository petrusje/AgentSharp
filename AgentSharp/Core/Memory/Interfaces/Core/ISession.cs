using System;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para representar uma sessão de conversação
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// ID único da sessão
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// ID do usuário proprietário da sessão
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// Título da sessão
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Data e hora de criação
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data e hora da última atividade
        /// </summary>
        DateTime LastActivity { get; set; }

        /// <summary>
        /// Metadados adicionais da sessão
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Status da sessão (ativa, arquivada, etc.)
        /// </summary>
        SessionStatus Status { get; set; }
    }

    /// <summary>
    /// Status possíveis de uma sessão
    /// </summary>
    public enum SessionStatus
    {
        Active,
        Inactive,
        Completed,
        Cancelled,
        Error,
        Archived,
        Deleted
    }
}
