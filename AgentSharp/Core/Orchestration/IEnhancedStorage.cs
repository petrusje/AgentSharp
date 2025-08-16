using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Orchestration;
using AgentSharp.Core;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Interface estendida para armazenamento com suporte a funcionalidades de TeamChat
    /// Estende IStorage com capacidades de conversa, variáveis globais e auditoria
    /// </summary>
    public interface IEnhancedStorage : IStorage
    {
        #region Gerenciamento de Estado de Conversa

        /// <summary>
        /// Carrega o estado completo de uma conversa (variáveis, histórico, contexto)
        /// </summary>
        /// <param name="sessionId">ID da sessão de conversa</param>
        /// <returns>Estado da conversa ou null se não encontrada</returns>
        Task<ConversationState> LoadConversationStateAsync(string sessionId);

        /// <summary>
        /// Salva o estado completo de uma conversa
        /// </summary>
        /// <param name="sessionId">ID da sessão de conversa</param>
        /// <param name="state">Estado completo da conversa</param>
        Task SaveConversationStateAsync(string sessionId, ConversationState state);

        /// <summary>
        /// Atualiza apenas as variáveis globais de uma conversa
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="variables">Coleção de variáveis atualizadas</param>
        Task UpdateConversationVariablesAsync(string sessionId, GlobalVariableCollection variables);

        /// <summary>
        /// Verifica se existe estado de conversa para a sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        Task<bool> ConversationStateExistsAsync(string sessionId);

        #endregion

        #region Auditoria de Variáveis

        /// <summary>
        /// Obtém trilha de auditoria completa de variáveis para uma sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="variableName">Nome da variável (opcional, null = todas)</param>
        /// <returns>Lista de entradas de auditoria ordenadas por timestamp</returns>
        Task<List<VariableAuditEntry>> GetVariableAuditAsync(string sessionId, string variableName = null);

        /// <summary>
        /// Salva uma entrada de auditoria de variável
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="entry">Entrada de auditoria</param>
        Task SaveVariableAuditAsync(string sessionId, VariableAuditEntry entry);

        /// <summary>
        /// Obtém histórico de mudanças de uma variável específica
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="variableName">Nome da variável</param>
        /// <param name="limit">Número máximo de entradas (default: todas)</param>
        Task<List<VariableAuditEntry>> GetVariableHistoryAsync(string sessionId, string variableName, int? limit = null);

        #endregion

        #region Gerenciamento de Sessões de Conversa

        /// <summary>
        /// Lista todas as sessões de conversa ativas
        /// </summary>
        /// <param name="userId">ID do usuário (opcional)</param>
        /// <returns>Lista de IDs de sessões ativas</returns>
        Task<List<string>> GetActiveConversationSessionsAsync(string userId = null);

        /// <summary>
        /// Lista sessões de conversa por critérios
        /// </summary>
        /// <param name="criteria">Critérios de busca</param>
        /// <returns>Lista de metadados de sessões</returns>
        Task<List<ConversationSessionMetadata>> GetConversationSessionsAsync(ConversationSearchCriteria criteria);

        /// <summary>
        /// Marca uma sessão de conversa como inativa/completa
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="completionReason">Motivo da conclusão</param>
        Task MarkConversationCompleteAsync(string sessionId, string completionReason = null);

        /// <summary>
        /// Remove sessões expiradas com base na política de retenção
        /// </summary>
        /// <param name="maxAge">Idade máxima das sessões</param>
        /// <param name="preserveComplete">Se deve preservar conversas completas</param>
        /// <returns>Número de sessões removidas</returns>
        Task<int> CleanupExpiredSessionsAsync(TimeSpan maxAge, bool preserveComplete = true);

        #endregion

        #region Backup e Restauração

        /// <summary>
        /// Cria um backup completo de uma conversa
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>Backup serializado da conversa</returns>
        Task<ConversationBackup> CreateBackupAsync(string sessionId);

        /// <summary>
        /// Restaura uma conversa a partir de um backup
        /// </summary>
        /// <param name="sessionId">ID da sessão (pode ser novo)</param>
        /// <param name="backup">Backup a ser restaurado</param>
        Task RestoreFromBackupAsync(string sessionId, ConversationBackup backup);

        /// <summary>
        /// Exporta dados de conversa em formato estruturado
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="format">Formato de exportação (json, xml, csv)</param>
        Task<byte[]> ExportConversationDataAsync(string sessionId, string format = "json");

        #endregion

        #region Análise e Relatórios

        /// <summary>
        /// Obtém estatísticas de uso de variáveis
        /// </summary>
        /// <param name="dateFrom">Data inicial (opcional)</param>
        /// <param name="dateTo">Data final (opcional)</param>
        Task<VariableUsageStatistics> GetVariableUsageStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null);

        /// <summary>
        /// Obtém métricas de performance de conversas
        /// </summary>
        /// <param name="criteria">Critérios de análise</param>
        Task<ConversationPerformanceMetrics> GetConversationMetricsAsync(ConversationAnalysisCriteria criteria);

        /// <summary>
        /// Identifica conversas com problemas (variáveis não capturadas, etc.)
        /// </summary>
        /// <param name="criteria">Critérios de problema</param>
        Task<List<ConversationIssue>> IdentifyConversationIssuesAsync(ConversationIssueCriteria criteria);

        #endregion

        #region Configuração e Manutenção

        /// <summary>
        /// Otimiza estruturas de armazenamento para performance
        /// </summary>
        Task OptimizeStorageAsync();

        /// <summary>
        /// Obtém informações de saúde do armazenamento
        /// </summary>
        Task<StorageHealthInfo> GetStorageHealthAsync();

        /// <summary>
        /// Configura políticas de retenção de dados
        /// </summary>
        /// <param name="policy">Política de retenção</param>
        Task ConfigureRetentionPolicyAsync(RetentionPolicy policy);

        #endregion
    }

    #region Modelos de Dados para Enhanced Storage

    /// <summary>
    /// Estado completo de uma conversa TeamChat
    /// </summary>
    public class ConversationState
    {
        /// <summary>
        /// ID da sessão
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// ID do usuário
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Variáveis globais da conversa
        /// </summary>
        public Dictionary<string, GlobalVariable> Variables { get; set; } = new Dictionary<string, GlobalVariable>();

        /// <summary>
        /// Histórico de mensagens da conversa
        /// </summary>
        public List<ConversationMessage> MessageHistory { get; set; } = new List<ConversationMessage>();

        /// <summary>
        /// Memória compartilhada da conversa
        /// </summary>
        public Dictionary<string, object> SharedMemory { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Agente atualmente ativo
        /// </summary>
        public string CurrentAgent { get; set; }

        /// <summary>
        /// Agentes disponíveis nesta conversa
        /// </summary>
        public List<ConversationAgent> AvailableAgents { get; set; } = new List<ConversationAgent>();

        /// <summary>
        /// Timestamp da última atividade
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Progresso atual da conversa
        /// </summary>
        public ConversationProgress Progress { get; set; }

        /// <summary>
        /// Status da conversa
        /// </summary>
        public ConversationStatus Status { get; set; } = ConversationStatus.Active;

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Timestamp de criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp de última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Entrada de auditoria para mudanças de variáveis
    /// </summary>
    public class VariableAuditEntry
    {
        /// <summary>
        /// ID único da entrada
        /// </summary>
        public string EntryId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ID da sessão
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Nome da variável
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Valor anterior
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Novo valor
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Agente que fez a modificação
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Timestamp da modificação
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nível de confiança na modificação
        /// </summary>
        public double Confidence { get; set; } = 1.0;

        /// <summary>
        /// Origem da modificação (agent, system, migration, manual)
        /// </summary>
        public string Source { get; set; } = "agent";

        /// <summary>
        /// Contexto adicional da modificação
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Metadados de uma sessão de conversa
    /// </summary>
    public class ConversationSessionMetadata
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public ConversationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public int TotalMessages { get; set; }
        public int VariablesCollected { get; set; }
        public int TotalVariables { get; set; }
        public string CurrentAgent { get; set; }
        public double CompletionPercentage { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Agente disponível em uma conversa TeamChat
    /// </summary>
    public class ConversationAgent
    {
        /// <summary>
        /// Nome único do agente
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Instância do agente (transient - não é persistida)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IAgent Agent { get; set; }

        /// <summary>
        /// Descrição do agente
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gatilhos que ativam este agente
        /// </summary>
        public string[] Triggers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Variáveis pelas quais este agente é responsável
        /// </summary>
        public string[] OwnedVariables { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Metadados do agente
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Especializações do agente para roteamento avançado
        /// </summary>
        public string[] Specializations { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Prioridade do agente (1-10, sendo 10 a maior prioridade)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Indica se o agente está ativo e disponível
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Backup completo de uma conversa
    /// </summary>
    public class ConversationBackup
    {
        public string SessionId { get; set; }
        public DateTime BackupCreatedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
        public ConversationState State { get; set; }
        public List<VariableAuditEntry> AuditTrail { get; set; } = new List<VariableAuditEntry>();
        public Dictionary<string, object> BackupMetadata { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Critérios de Busca e Análise

    /// <summary>
    /// Critérios para busca de sessões de conversa
    /// </summary>
    public class ConversationSearchCriteria
    {
        public string UserId { get; set; }
        public ConversationStatus? Status { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string CurrentAgent { get; set; }
        public double? MinCompletionPercentage { get; set; }
        public double? MaxCompletionPercentage { get; set; }
        public int? Limit { get; set; } = 100;
        public int? Offset { get; set; } = 0;
    }

    /// <summary>
    /// Critérios para análise de conversas
    /// </summary>
    public class ConversationAnalysisCriteria
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string UserId { get; set; }
        public string AgentName { get; set; }
        public bool IncludeCompleted { get; set; } = true;
        public bool IncludeActive { get; set; } = true;
        public bool IncludeAbandoned { get; set; } = false;
    }

    /// <summary>
    /// Critérios para identificação de problemas
    /// </summary>
    public class ConversationIssueCriteria
    {
        public TimeSpan? MaxInactivityPeriod { get; set; }
        public double? MinRequiredCompletionRate { get; set; }
        public int? MaxMessagesWithoutProgress { get; set; }
        public bool IncludeStuckConversations { get; set; } = true;
        public bool IncludeAbandonedConversations { get; set; } = true;
    }

    #endregion

    #region Modelos de Estatísticas e Métricas

    /// <summary>
    /// Estatísticas de uso de variáveis
    /// </summary>
    public class VariableUsageStatistics
    {
        public int TotalConversations { get; set; }
        public Dictionary<string, int> VariableUsageCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> VariableAverageConfidence { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, string> MostActiveAgents { get; set; } = new Dictionary<string, string>();
        public TimeSpan AverageCompletionTime { get; set; }
        public double OverallCompletionRate { get; set; }
    }

    /// <summary>
    /// Métricas de performance de conversas
    /// </summary>
    public class ConversationPerformanceMetrics
    {
        public int TotalConversations { get; set; }
        public int CompletedConversations { get; set; }
        public int ActiveConversations { get; set; }
        public int AbandonedConversations { get; set; }
        public double CompletionRate { get; set; }
        public TimeSpan AverageConversationDuration { get; set; }
        public double AverageMessagesPerConversation { get; set; }
        public Dictionary<string, int> AgentUsageCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, TimeSpan> AgentAverageResponseTime { get; set; } = new Dictionary<string, TimeSpan>();
    }

    /// <summary>
    /// Problema identificado em uma conversa
    /// </summary>
    public class ConversationIssue
    {
        public string SessionId { get; set; }
        public string IssueType { get; set; }
        public string Description { get; set; }
        public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;
        public string Severity { get; set; } // Low, Medium, High, Critical
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
        public string RecommendedAction { get; set; }
    }

    /// <summary>
    /// Informações de saúde do armazenamento
    /// </summary>
    public class StorageHealthInfo
    {
        public bool IsHealthy { get; set; }
        public string Version { get; set; }
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public long TotalConversations { get; set; }
        public long TotalVariables { get; set; }
        public long TotalAuditEntries { get; set; }
        public long StorageSizeBytes { get; set; }
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new Dictionary<string, object>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Política de retenção de dados
    /// </summary>
    public class RetentionPolicy
    {
        public TimeSpan ConversationRetentionPeriod { get; set; } = TimeSpan.FromDays(90);
        public TimeSpan AuditTrailRetentionPeriod { get; set; } = TimeSpan.FromDays(365);
        public bool PreserveCompletedConversations { get; set; } = true;
        public bool CompressOldData { get; set; } = false;
        public int MaxBackupsPerConversation { get; set; } = 5;
        public Dictionary<string, object> CustomPolicies { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Enums

    /// <summary>
    /// Status possíveis de uma conversa
    /// </summary>
    public enum ConversationStatus
    {
        /// <summary>
        /// Conversa ativa em andamento
        /// </summary>
        Active,

        /// <summary>
        /// Conversa completada com sucesso
        /// </summary>
        Completed,

        /// <summary>
        /// Conversa pausada/em espera
        /// </summary>
        Paused,

        /// <summary>
        /// Conversa abandonada pelo usuário
        /// </summary>
        Abandoned,

        /// <summary>
        /// Conversa com erro/problema
        /// </summary>
        Error,

        /// <summary>
        /// Conversa arquivada
        /// </summary>
        Archived
    }

    #endregion
}