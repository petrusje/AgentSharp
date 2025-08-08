using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Representa um resumo de sessão de agente
    /// </summary>
    public class SessionSummary
    {
        /// <summary>
        /// Identificador único do resumo
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID da sessão que foi resumida
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// ID do usuário proprietário da sessão
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Conteúdo do resumo
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Tópicos principais identificados na sessão
        /// </summary>
        public List<string> Topics { get; set; }

        /// <summary>
        /// Pontos-chave da sessão
        /// </summary>
        public List<string> KeyPoints { get; set; }

        /// <summary>
        /// Entidades mencionadas na sessão
        /// </summary>
        public List<string> Entities { get; set; }

        /// <summary>
        /// Sentimento geral da sessão
        /// </summary>
        public string Sentiment { get; set; }

        /// <summary>
        /// Score de importância do resumo (0.0 a 1.0)
        /// </summary>
        public double Importance { get; set; }

        /// <summary>
        /// Número de mensagens resumidas
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// Duração da sessão
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Data de início da sessão
        /// </summary>
        public DateTime SessionStartDate { get; set; }

        /// <summary>
        /// Data de fim da sessão
        /// </summary>
        public DateTime SessionEndDate { get; set; }

        /// <summary>
        /// Data de criação do resumo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public SessionSummary()
        {
            Id = Guid.NewGuid().ToString();
            Topics = new List<string>();
            KeyPoints = new List<string>();
            Entities = new List<string>();
            Metadata = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            Importance = 0.5;
        }

        /// <summary>
        /// Construtor com parâmetros essenciais
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="content">Conteúdo do resumo</param>
        public SessionSummary(string sessionId, string userId, string content) : this()
        {
            SessionId = sessionId;
            UserId = userId;
            Content = content;
        }

        /// <summary>
        /// Converte o resumo para dicionário para serialização
        /// </summary>
        /// <returns>Dicionário com os dados do resumo</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["sessionId"] = SessionId,
                ["userId"] = UserId,
                ["content"] = Content,
                ["topics"] = Topics ?? new List<string>(),
                ["keyPoints"] = KeyPoints ?? new List<string>(),
                ["entities"] = Entities ?? new List<string>(),
                ["sentiment"] = Sentiment,
                ["importance"] = Importance,
                ["messageCount"] = MessageCount,
                ["duration"] = Duration.TotalMilliseconds,
                ["sessionStartDate"] = SessionStartDate,
                ["sessionEndDate"] = SessionEndDate,
                ["createdAt"] = CreatedAt,
                ["metadata"] = Metadata ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Cria um resumo a partir de um dicionário
        /// </summary>
        /// <param name="dict">Dicionário com os dados</param>
        /// <returns>Instância de SessionSummary</returns>
        public static SessionSummary FromDictionary(Dictionary<string, object> dict)
        {
            var summary = new SessionSummary();

            if (dict.TryGetValue("id", out var id))
                summary.Id = id?.ToString();

            if (dict.TryGetValue("sessionId", out var sessionId))
                summary.SessionId = sessionId?.ToString();

            if (dict.TryGetValue("userId", out var userId))
                summary.UserId = userId?.ToString();

            if (dict.TryGetValue("content", out var content))
                summary.Content = content?.ToString();

            if (dict.TryGetValue("topics", out var topics) && topics is IEnumerable<object> topicList)
                summary.Topics = new List<string>();
                // Implementar conversão de lista

            if (dict.TryGetValue("keyPoints", out var keyPoints) && keyPoints is IEnumerable<object> keyPointList)
                summary.KeyPoints = new List<string>();
                // Implementar conversão de lista

            if (dict.TryGetValue("entities", out var entities) && entities is IEnumerable<object> entityList)
                summary.Entities = new List<string>();
                // Implementar conversão de lista

            if (dict.TryGetValue("sentiment", out var sentiment))
                summary.Sentiment = sentiment?.ToString();

            if (dict.TryGetValue("importance", out var importance) && double.TryParse(importance?.ToString(), out var importanceValue))
                summary.Importance = importanceValue;

            if (dict.TryGetValue("messageCount", out var messageCount) && int.TryParse(messageCount?.ToString(), out var messageCountValue))
                summary.MessageCount = messageCountValue;

            if (dict.TryGetValue("duration", out var duration) && double.TryParse(duration?.ToString(), out var durationValue))
                summary.Duration = TimeSpan.FromMilliseconds(durationValue);

            if (dict.TryGetValue("sessionStartDate", out var sessionStartDate) && DateTime.TryParse(sessionStartDate?.ToString(), out var sessionStartDateValue))
                summary.SessionStartDate = sessionStartDateValue;

            if (dict.TryGetValue("sessionEndDate", out var sessionEndDate) && DateTime.TryParse(sessionEndDate?.ToString(), out var sessionEndDateValue))
                summary.SessionEndDate = sessionEndDateValue;

            if (dict.TryGetValue("createdAt", out var createdAt) && DateTime.TryParse(createdAt?.ToString(), out var createdAtValue))
                summary.CreatedAt = createdAtValue;

            if (dict.TryGetValue("metadata", out var metadata) && metadata is Dictionary<string, object> metaDict)
                summary.Metadata = metaDict;

            return summary;
        }

        /// <summary>
        /// Adiciona um tópico ao resumo
        /// </summary>
        /// <param name="topic">Tópico a ser adicionado</param>
        public void AddTopic(string topic)
        {
            if (!string.IsNullOrEmpty(topic) && !Topics.Contains(topic))
                Topics.Add(topic);
        }

        /// <summary>
        /// Adiciona um ponto-chave ao resumo
        /// </summary>
        /// <param name="keyPoint">Ponto-chave a ser adicionado</param>
        public void AddKeyPoint(string keyPoint)
        {
            if (!string.IsNullOrEmpty(keyPoint) && !KeyPoints.Contains(keyPoint))
                KeyPoints.Add(keyPoint);
        }

        /// <summary>
        /// Adiciona uma entidade ao resumo
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        public void AddEntity(string entity)
        {
            if (!string.IsNullOrEmpty(entity) && !Entities.Contains(entity))
                Entities.Add(entity);
        }

        /// <summary>
        /// Representação em string do resumo
        /// </summary>
        /// <returns>String representando o resumo</returns>
        public override string ToString()
        {
            var preview = Content?.Length > 100 ? Content.Substring(0, 100) + "..." : Content;
            return $"SessionSummary[{Id}]: {preview} (Importance: {Importance:F2}, Topics: {Topics.Count})";
        }
    }
}
