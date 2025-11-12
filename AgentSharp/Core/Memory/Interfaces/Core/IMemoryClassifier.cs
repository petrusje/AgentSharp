using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para classificação inteligente de conteúdo de memória
    /// Responsável por determinar quando e como criar/atualizar memórias
    /// </summary>
    public interface IMemoryClassifier
    {
        /// <summary>
        /// Determina se um conteúdo deve gerar uma nova memória
        /// </summary>
        /// <param name="content">Conteúdo a ser analisado</param>
        /// <param name="context">Contexto da conversa/sessão</param>
        /// <param name="existingMemories">Memórias existentes relacionadas</param>
        /// <returns>True se deve criar uma memória</returns>
        Task<bool> ShouldUpdateAsync(string content, MemoryClassificationContext context, IEnumerable<AgentMemoryRecord> existingMemories = null);

        /// <summary>
        /// Classifica o tipo e importância de um conteúdo
        /// </summary>
        /// <param name="content">Conteúdo a ser classificado</param>
        /// <param name="context">Contexto adicional</param>
        /// <returns>Resultado da classificação</returns>
        Task<MemoryClassification> ClassifyAsync(string content, MemoryClassificationContext context = null);

        /// <summary>
        /// Calcula a relevância de um conteúdo
        /// </summary>
        /// <param name="content">Conteúdo a ser analisado</param>
        /// <param name="context">Contexto da análise</param>
        /// <param name="userProfile">Perfil do usuário</param>
        /// <returns>Score de relevância (0.0 a 1.0)</returns>
        Task<double> CalculateRelevanceAsync(string content, MemoryClassificationContext context = null, UserProfile userProfile = null);

        /// <summary>
        /// Extrai tags automáticas do conteúdo
        /// </summary>
        /// <param name="content">Conteúdo para extração</param>
        /// <param name="maxTags">Número máximo de tags</param>
        /// <returns>Lista de tags relevantes</returns>
        Task<IEnumerable<string>> ExtractTagsAsync(string content, int maxTags = 10);

        /// <summary>
        /// Identifica entidades mencionadas no conteúdo
        /// </summary>
        /// <param name="content">Conteúdo para análise</param>
        /// <returns>Entidades identificadas</returns>
        Task<IEnumerable<NamedEntity>> ExtractEntitiesAsync(string content);

        /// <summary>
        /// Analisa o sentimento do conteúdo
        /// </summary>
        /// <param name="content">Conteúdo para análise</param>
        /// <returns>Análise de sentimento</returns>
        Task<SentimentAnalysis> AnalyzeSentimentAsync(string content);

        /// <summary>
        /// Sugere quando consolidar/resumir memórias
        /// </summary>
        /// <param name="memories">Memórias a serem analisadas</param>
        /// <param name="criteria">Critérios de consolidação</param>
        /// <returns>Sugestões de consolidação</returns>
        Task<IEnumerable<ConsolidationSuggestion>> SuggestConsolidationAsync(IEnumerable<AgentMemoryRecord> memories, ConsolidationCriteria criteria = null);

        /// <summary>
        /// Detecta duplicatas ou conteúdo muito similar
        /// </summary>
        /// <param name="content">Conteúdo novo</param>
        /// <param name="existingMemories">Memórias existentes</param>
        /// <param name="similarityThreshold">Limite de similaridade</param>
        /// <returns>Memórias similares encontradas</returns>
        Task<IEnumerable<SimilarMemory>> DetectSimilarContentAsync(string content, IEnumerable<AgentMemoryRecord> existingMemories, double similarityThreshold = 0.8);
    }

    /// <summary>
    /// Contexto para classificação de memória
    /// </summary>
    public class MemoryClassificationContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime Timestamp { get; set; }
        public string ConversationTopic { get; set; }
        public AgentSharp.Core.Role Role { get; set; }
        public Dictionary<string, object> AdditionalContext { get; set; }
        public IEnumerable<string> PreviousMessages { get; set; }
    }

    /// <summary>
    /// Resultado da classificação de memória
    /// </summary>
    public class MemoryClassification
    {
        public MemoryType Type { get; set; }
        public MemoryImportance Importance { get; set; }
        public double Relevance { get; set; }
        public IEnumerable<string> SuggestedTags { get; set; }
        public IEnumerable<NamedEntity> Entities { get; set; }
        public SentimentAnalysis Sentiment { get; set; }
        public string Topic { get; set; }
        public Dictionary<string, object> AdditionalMetadata { get; set; }
    }

    /// <summary>
    /// Perfil do usuário para classificação personalizada
    /// </summary>
    public class UserProfile
    {
        public string UserId { get; set; }
        public IEnumerable<string> Interests { get; set; }
        public Dictionary<string, double> TopicPreferences { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> CustomAttributes { get; set; }
    }

    /// <summary>
    /// Entidade nomeada identificada no texto
    /// </summary>
    public class NamedEntity
    {
        public string Text { get; set; }
        public EntityType Type { get; set; }
        public double Confidence { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }

    /// <summary>
    /// Análise de sentimento
    /// </summary>
    public class SentimentAnalysis
    {
        public SentimentType Sentiment { get; set; }
        public double Confidence { get; set; }
        public double PositiveScore { get; set; }
        public double NegativeScore { get; set; }
        public double NeutralScore { get; set; }
    }

    /// <summary>
    /// Sugestão de consolidação de memórias
    /// </summary>
    public class ConsolidationSuggestion
    {
        public IEnumerable<string> MemoryIds { get; set; }
        public ConsolidationType Type { get; set; }
        public string Reason { get; set; }
        public double Confidence { get; set; }
        public string SuggestedTitle { get; set; }
    }

    /// <summary>
    /// Critérios para consolidação de memórias
    /// </summary>
    public class ConsolidationCriteria
    {
        public int MinMemoriesForConsolidation { get; set; } = 5;
        public TimeSpan MaxTimeSpan { get; set; } = TimeSpan.FromDays(7);
        public double SimilarityThreshold { get; set; } = 0.7;
        public int MaxConsolidatedMemories { get; set; } = 3;
    }

    /// <summary>
    /// Memória similar detectada
    /// </summary>
    public class SimilarMemory
    {
        public AgentMemoryRecord Memory { get; set; }
        public double SimilarityScore { get; set; }
        public SimilarityType SimilarityType { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Tipos de memória
    /// </summary>
    public enum MemoryType
    {
        Fact,
        Preference,
        Conversation,
        Task,
        Context,
        Instruction,
        Feedback,
        Question,
        Answer,
        Other
    }

    /// <summary>
    /// Níveis de importância da memória
    /// </summary>
    public enum MemoryImportance
    {
        VeryLow,
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    // Removido: enum MessageRole. Usar AgentSharp.Core.Role

    /// <summary>
    /// Tipos de entidades
    /// </summary>
    public enum EntityType
    {
        Person,
        Organization,
        Location,
        Date,
        Time,
        Money,
        Number,
        Email,
        Phone,
        Url,
        Product,
        Event,
        Other
    }

    /// <summary>
    /// Tipos de sentimento
    /// </summary>
    public enum SentimentType
    {
        Positive,
        Negative,
        Neutral,
        Mixed
    }

    /// <summary>
    /// Tipos de consolidação
    /// </summary>
    public enum ConsolidationType
    {
        Merge,
        Summarize,
        Archive,
        Delete,
        Split
    }

    /// <summary>
    /// Tipos de similaridade
    /// </summary>
    public enum SimilarityType
    {
        ContentSimilarity,
        TopicSimilarity,
        EntitySimilarity,
        SemanticSimilarity,
        ExactDuplicate
    }
}
