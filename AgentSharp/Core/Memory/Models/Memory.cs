using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa uma memória individual no sistema
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// Identificador único da memória
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID do usuário proprietário da memória
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ID da sessão onde a memória foi criada (opcional)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Conteúdo da memória
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Tags para categorização
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Score de relevância (0.0 a 1.0)
        /// </summary>
        public double Relevance { get; set; }

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

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
        /// Número de vezes que foi acessada
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// Tipo da memória (Fato, Preferência, etc.)
        /// </summary>
        public AgentMemoryType Type { get; set; } = AgentMemoryType.Fact;

        /// <summary>
        /// Embedding vetorial para busca semântica
        /// </summary>
        public List<float> Embedding { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public Memory()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new List<string>();
            Metadata = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastAccessedAt = DateTime.UtcNow;
            Relevance = 1.0;
            AccessCount = 0;
        }

        /// <summary>
        /// Construtor com parâmetros essenciais
        /// </summary>
        /// <param name="content">Conteúdo da memória</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="sessionId">ID da sessão (opcional)</param>
        public Memory(string content, string userId, string sessionId = null) : this()
        {
            Content = content;
            UserId = userId;
            SessionId = sessionId;
        }

        /// <summary>
        /// Converte a memória para dicionário para serialização
        /// </summary>
        /// <returns>Dicionário com os dados da memória</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["userId"] = UserId,
                ["sessionId"] = SessionId,
                ["content"] = Content,
                ["tags"] = Tags?.ToList() ?? new List<string>(),
                ["relevance"] = Relevance,
                ["metadata"] = Metadata ?? new Dictionary<string, object>(),
                ["createdAt"] = CreatedAt,
                ["updatedAt"] = UpdatedAt,
                ["lastAccessedAt"] = LastAccessedAt,
                ["accessCount"] = AccessCount
            };
        }

        /// <summary>
        /// Cria uma memória a partir de um dicionário
        /// </summary>
        /// <param name="dict">Dicionário com os dados</param>
        /// <returns>Instância de Memory</returns>
        public static Memory FromDictionary(Dictionary<string, object> dict)
        {
            var memory = new Memory();

            if (dict.TryGetValue("id", out var id))
                memory.Id = id?.ToString();

            if (dict.TryGetValue("userId", out var userId))
                memory.UserId = userId?.ToString();

            if (dict.TryGetValue("sessionId", out var sessionId))
                memory.SessionId = sessionId?.ToString();

            if (dict.TryGetValue("content", out var content))
                memory.Content = content?.ToString();

            if (dict.TryGetValue("tags", out var tags) && tags is IEnumerable<object> tagList)
                memory.Tags = tagList.Select(t => t?.ToString()).Where(t => !string.IsNullOrEmpty(t)).ToList();

            if (dict.TryGetValue("relevance", out var relevance) && double.TryParse(relevance?.ToString(), out var relevanceValue))
                memory.Relevance = relevanceValue;

            if (dict.TryGetValue("metadata", out var metadata) && metadata is Dictionary<string, object> metaDict)
                memory.Metadata = metaDict;

            if (dict.TryGetValue("createdAt", out var createdAt) && DateTime.TryParse(createdAt?.ToString(), out var createdAtValue))
                memory.CreatedAt = createdAtValue;

            if (dict.TryGetValue("updatedAt", out var updatedAt) && DateTime.TryParse(updatedAt?.ToString(), out var updatedAtValue))
                memory.UpdatedAt = updatedAtValue;

            if (dict.TryGetValue("lastAccessedAt", out var lastAccessedAt) && DateTime.TryParse(lastAccessedAt?.ToString(), out var lastAccessedAtValue))
                memory.LastAccessedAt = lastAccessedAtValue;

            if (dict.TryGetValue("accessCount", out var accessCount) && int.TryParse(accessCount?.ToString(), out var accessCountValue))
                memory.AccessCount = accessCountValue;

            return memory;
        }

        /// <summary>
        /// Marca a memória como acessada
        /// </summary>
        public void MarkAsAccessed()
        {
            LastAccessedAt = DateTime.UtcNow;
            AccessCount++;
        }

        /// <summary>
        /// Atualiza o conteúdo da memória
        /// </summary>
        /// <param name="newContent">Novo conteúdo</param>
        /// <param name="newTags">Novas tags (opcional)</param>
        /// <param name="newRelevance">Nova relevância (opcional)</param>
        public void Update(string newContent, IEnumerable<string> newTags = null, double? newRelevance = null)
        {
            if (!string.IsNullOrEmpty(newContent))
                Content = newContent;

            if (newTags != null)
                Tags = newTags.ToList();

            if (newRelevance.HasValue)
                Relevance = Math.Max(0.0, Math.Min(1.0, newRelevance.Value));

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adiciona uma tag à memória
        /// </summary>
        /// <param name="tag">Tag a ser adicionada</param>
        public void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                Tags.Add(tag);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Remove uma tag da memória
        /// </summary>
        /// <param name="tag">Tag a ser removida</param>
        public bool RemoveTag(string tag)
        {
            var removed = Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
                UpdatedAt = DateTime.UtcNow;
            return removed;
        }

        /// <summary>
        /// Adiciona ou atualiza um metadado
        /// </summary>
        /// <param name="key">Chave do metadado</param>
        /// <param name="value">Valor do metadado</param>
        public void SetMetadata(string key, object value)
        {
            Metadata[key] = value;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Obtém um metadado
        /// </summary>
        /// <typeparam name="T">Tipo do metadado</typeparam>
        /// <param name="key">Chave do metadado</param>
        /// <param name="defaultValue">Valor padrão se não encontrado</param>
        /// <returns>Valor do metadado</returns>
        public T GetMetadata<T>(string key, T defaultValue = default(T))
        {
            if (Metadata.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Verifica se a memória contém uma tag específica
        /// </summary>
        /// <param name="tag">Tag a ser verificada</param>
        /// <returns>True se contém a tag</returns>
        public bool HasTag(string tag)
        {
            return Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Calcula a idade da memória em dias
        /// </summary>
        /// <returns>Idade em dias</returns>
        public double GetAgeInDays()
        {
            return (DateTime.UtcNow - CreatedAt).TotalDays;
        }

        /// <summary>
        /// Representação em string da memória
        /// </summary>
        /// <returns>String representando a memória</returns>
        public override string ToString()
        {
            var preview = Content?.Length > 50 ? Content.Substring(0, 50) + "..." : Content;
            return $"Memory[{Id}]: {preview} (Relevance: {Relevance:F2}, Tags: {string.Join(", ", Tags)})";
        }
    }
}
