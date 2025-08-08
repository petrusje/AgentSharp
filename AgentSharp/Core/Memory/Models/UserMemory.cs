using System;
using System.Collections.Generic;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa uma memória individual do usuário
    /// </summary>
    public class UserMemory
    {
        /// <summary>
        /// ID único da memória
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ID do usuário associado à memória
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ID da sessão onde a memória foi criada
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Conteúdo textual da memória
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Tipo da memória (Fato, Preferência, Habilidade, etc.)
        /// </summary>
        public AgentMemoryType Type { get; set; }

        /// <summary>
        /// Data e hora de criação da memória
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Número de vezes que a memória foi acessada
        /// </summary>
        public int AccessCount { get; set; } = 0;

        /// <summary>
        /// Data do último acesso
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// Score de relevância da memória (0.0 a 1.0)
        /// </summary>
        public double RelevanceScore { get; set; } = 0.0;

        // Embedding removido temporariamente (postergado)

        /// <summary>
        /// Tags associadas à memória para categorização
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Metadados adicionais da memória
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Indica se a memória está ativa (não foi removida)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Prioridade da memória (1 = baixa, 5 = alta)
        /// </summary>
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Contexto onde a memória foi criada
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public UserMemory()
        {
        }

        /// <summary>
        /// Construtor com conteúdo
        /// </summary>
        /// <param name="content">Conteúdo da memória</param>
        /// <param name="type">Tipo da memória</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="sessionId">ID da sessão</param>
        public UserMemory(string content, AgentMemoryType type, string userId = null, string sessionId = null)
        {
            Content = content;
            Type = type;
            UserId = userId;
            SessionId = sessionId;
        }

        /// <summary>
        /// Atualiza o timestamp de acesso
        /// </summary>
        public void RecordAccess()
        {
            AccessCount++;
            LastAccessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Atualiza o conteúdo da memória
        /// </summary>
        /// <param name="newContent">Novo conteúdo</param>
        public void UpdateContent(string newContent)
        {
            Content = newContent;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Converte a memória para dicionário para serialização
        /// </summary>
        /// <returns>Dicionário com dados da memória</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["userId"] = UserId,
                ["sessionId"] = SessionId,
                ["content"] = Content,
                ["type"] = Type.ToString(),
                ["createdAt"] = CreatedAt,
                ["updatedAt"] = UpdatedAt,
                ["accessCount"] = AccessCount,
                ["lastAccessedAt"] = LastAccessedAt,
                ["relevanceScore"] = RelevanceScore,
                ["tags"] = Tags,
                ["metadata"] = Metadata,
                ["isActive"] = IsActive,
                ["priority"] = Priority,
                ["context"] = Context
            };
        }

        /// <summary>
        /// Cria uma memória a partir de um dicionário
        /// </summary>
        /// <param name="dict">Dicionário com dados da memória</param>
        /// <returns>Nova instância de UserMemory</returns>
        public static UserMemory FromDictionary(Dictionary<string, object> dict)
        {
            var memory = new UserMemory();

            if (dict.ContainsKey("id")) memory.Id = dict["id"]?.ToString();
            if (dict.ContainsKey("userId")) memory.UserId = dict["userId"]?.ToString();
            if (dict.ContainsKey("sessionId")) memory.SessionId = dict["sessionId"]?.ToString();
            if (dict.ContainsKey("content")) memory.Content = dict["content"]?.ToString();
            if (dict.ContainsKey("type") && Enum.TryParse<AgentMemoryType>(dict["type"]?.ToString(), out var type))
                memory.Type = type;
            if (dict.ContainsKey("createdAt") && DateTime.TryParse(dict["createdAt"]?.ToString(), out var createdAt))
                memory.CreatedAt = createdAt;
            if (dict.ContainsKey("updatedAt") && DateTime.TryParse(dict["updatedAt"]?.ToString(), out var updatedAt))
                memory.UpdatedAt = updatedAt;
            if (dict.ContainsKey("accessCount") && int.TryParse(dict["accessCount"]?.ToString(), out var accessCount))
                memory.AccessCount = accessCount;
            if (dict.ContainsKey("lastAccessedAt") && DateTime.TryParse(dict["lastAccessedAt"]?.ToString(), out var lastAccessedAt))
                memory.LastAccessedAt = lastAccessedAt;
            if (dict.ContainsKey("relevanceScore") && double.TryParse(dict["relevanceScore"]?.ToString(), out var relevanceScore))
                memory.RelevanceScore = relevanceScore;
            if (dict.ContainsKey("tags") && dict["tags"] is List<string> tags)
                memory.Tags = tags;
            if (dict.ContainsKey("metadata") && dict["metadata"] is Dictionary<string, object> metadata)
                memory.Metadata = metadata;
            if (dict.ContainsKey("isActive") && bool.TryParse(dict["isActive"]?.ToString(), out var isActive))
                memory.IsActive = isActive;
            if (dict.ContainsKey("priority") && int.TryParse(dict["priority"]?.ToString(), out var priority))
                memory.Priority = priority;
            if (dict.ContainsKey("context")) memory.Context = dict["context"]?.ToString();

            return memory;
        }

        public override string ToString()
        {
            return $"Memory[{Type}]: {Content?.Substring(0, Math.Min(Content.Length, 50))}...";
        }
    }
}
