using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface de abstração para persistência de dados de memória
    /// Fornece operações CRUD de baixo nível para diferentes tipos de storage
    /// </summary>
    public interface IMemoryRepository
    {
        /// <summary>
        /// Inicializa o repositório de memória
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        /// <param name="options">Opções de configuração</param>
        Task InitializeAsync(string connectionString, Dictionary<string, object> options = null);

        /// <summary>
        /// Verifica se o repositório está inicializado
        /// </summary>
        bool IsInitialized { get; }

        // === OPERAÇÕES DE MEMÓRIA INDIVIDUAL ===

        /// <summary>
        /// Cria uma nova memória
        /// </summary>
        /// <param name="memory">Dados da memória</param>
        /// <returns>ID da memória criada</returns>
        Task<string> CreateMemoryAsync(AgentMemoryRecord memory);

        /// <summary>
        /// Obtém uma memória por ID
        /// </summary>
        /// <param name="memoryId">ID da memória</param>
        /// <returns>Dados da memória ou null se não encontrada</returns>
        Task<AgentMemoryRecord> GetMemoryAsync(string memoryId);

        /// <summary>
        /// Atualiza uma memória existente
        /// </summary>
        /// <param name="memory">Dados atualizados da memória</param>
        /// <returns>True se atualizada com sucesso</returns>
        Task<bool> UpdateMemoryAsync(AgentMemoryRecord memory);

        /// <summary>
        /// Remove uma memória
        /// </summary>
        /// <param name="memoryId">ID da memória</param>
        /// <returns>True se removida com sucesso</returns>
        Task<bool> DeleteMemoryAsync(string memoryId);

        // === OPERAÇÕES DE SESSÃO ===

        /// <summary>
        /// Cria uma nova sessão de agente
        /// </summary>
        /// <param name="session">Dados da sessão</param>
        /// <returns>ID da sessão criada</returns>
        Task<string> CreateSessionAsync(AgentSession session);

        /// <summary>
        /// Obtém uma sessão por ID
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>Dados da sessão ou null se não encontrada</returns>
        Task<AgentSession> GetSessionAsync(string sessionId);

        /// <summary>
        /// Atualiza uma sessão existente
        /// </summary>
        /// <param name="session">Dados atualizados da sessão</param>
        /// <returns>True se atualizada com sucesso</returns>
        Task<bool> UpdateSessionAsync(AgentSession session);

        /// <summary>
        /// Remove uma sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>True se removida com sucesso</returns>
        Task<bool> DeleteSessionAsync(string sessionId);

        // === OPERAÇÕES DE CONSULTA ===

        /// <summary>
        /// Obtém todas as memórias de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="limit">Limite de resultados</param>
        /// <param name="skip">Número de registros a pular</param>
        /// <returns>Lista de memórias</returns>
        Task<IEnumerable<AgentMemoryRecord>> GetMemoriesByUserAsync(string userId, int limit = 100, int skip = 0);

        /// <summary>
        /// Obtém memórias de uma sessão específica
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias da sessão</returns>
        Task<IEnumerable<AgentMemoryRecord>> GetMemoriesBySessionAsync(string sessionId, int limit = 100);

        /// <summary>
        /// Busca memórias por conteúdo
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="query">Consulta de busca</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias relevantes</returns>
        Task<IEnumerable<AgentMemoryRecord>> SearchMemoriesAsync(string userId, string query, int limit = 10);

        /// <summary>
        /// Obtém memórias por tags
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="tags">Tags a buscar</param>
        /// <param name="matchAllTags">Se deve coincidir com todas as tags ou apenas uma</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias com as tags</returns>
        Task<IEnumerable<AgentMemoryRecord>> GetMemoriesByTagsAsync(string userId, IEnumerable<string> tags, bool matchAllTags = false, int limit = 100);

        /// <summary>
        /// Obtém memórias por faixa de relevância
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="minRelevance">Relevância mínima</param>
        /// <param name="maxRelevance">Relevância máxima</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias na faixa de relevância</returns>
        Task<IEnumerable<AgentMemoryRecord>> GetMemoriesByRelevanceAsync(string userId, double minRelevance, double maxRelevance = 1.0, int limit = 100);

        /// <summary>
        /// Obtém memórias mais recentes
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias mais recentes</returns>
        Task<IEnumerable<AgentMemoryRecord>> GetRecentMemoriesAsync(string userId, int limit = 10);

        /// <summary>
        /// Obtém sessões de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="limit">Limite de resultados</param>
        /// <param name="skip">Número de registros a pular</param>
        /// <returns>Lista de sessões</returns>
        Task<IEnumerable<AgentSession>> GetSessionsByUserAsync(string userId, int limit = 50, int skip = 0);

        /// <summary>
        /// Obtém sessões mais recentes de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de sessões mais recentes</returns>
        Task<IEnumerable<AgentSession>> GetRecentSessionsAsync(string userId, int limit = 10);

        // === OPERAÇÕES DE AGREGAÇÃO ===

        /// <summary>
        /// Conta o número total de memórias de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Número total de memórias</returns>
        Task<long> CountMemoriesAsync(string userId);

        /// <summary>
        /// Conta o número total de sessões de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Número total de sessões</returns>
        Task<long> CountSessionsAsync(string userId);

        /// <summary>
        /// Obtém estatísticas de memórias de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Estatísticas detalhadas</returns>
        Task<RepositoryStatistics> GetStatisticsAsync(string userId);

        // === OPERAÇÕES DE MANUTENÇÃO ===

        /// <summary>
        /// Remove memórias antigas baseado em critérios
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="olderThan">Data limite</param>
        /// <param name="maxToKeep">Máximo de memórias a manter</param>
        /// <returns>Número de memórias removidas</returns>
        Task<int> CleanupMemoriesAsync(string userId, DateTime olderThan, int maxToKeep = 1000);

        /// <summary>
        /// Otimiza o armazenamento (compactação, índices, etc.)
        /// </summary>
        /// <returns>Relatório da otimização</returns>
        Task<OptimizationReport> OptimizeStorageAsync();

        /// <summary>
        /// Cria backup dos dados
        /// </summary>
        /// <param name="backupPath">Caminho do backup</param>
        /// <param name="includeOptions">Opções do backup</param>
        /// <returns>Caminho do arquivo de backup criado</returns>
        Task<string> CreateBackupAsync(string backupPath, BackupOptions includeOptions = null);

        /// <summary>
        /// Restaura dados de um backup
        /// </summary>
        /// <param name="backupPath">Caminho do backup</param>
        /// <param name="restoreOptions">Opções de restauração</param>
        /// <returns>True se restaurado com sucesso</returns>
        Task<bool> RestoreFromBackupAsync(string backupPath, RestoreOptions restoreOptions = null);

        // === OPERAÇÕES TRANSACIONAIS ===

        /// <summary>
        /// Executa múltiplas operações em uma transação
        /// </summary>
        /// <param name="operations">Lista de operações</param>
        /// <returns>True se todas as operações foram executadas com sucesso</returns>
        Task<bool> ExecuteTransactionAsync(IEnumerable<RepositoryOperation> operations);
    }

    /// <summary>
    /// Estatísticas do repositório
    /// </summary>
    public class RepositoryStatistics
    {
        public long TotalMemories { get; set; }
        public long TotalSessions { get; set; }
        public DateTime? OldestMemory { get; set; }
        public DateTime? NewestMemory { get; set; }
        public double AverageRelevance { get; set; }
        public Dictionary<string, long> TagCounts { get; set; }
        public long TotalStorageSize { get; set; }
        public Dictionary<string, object> AdditionalStats { get; set; }
    }

    /// <summary>
    /// Relatório de otimização do storage
    /// </summary>
    public class OptimizationReport
    {
        public DateTime OptimizedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public long StorageSizeBefore { get; set; }
        public long StorageSizeAfter { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsOptimized { get; set; }
        public IEnumerable<string> ActionsPerformed { get; set; }
    }

    /// <summary>
    /// Opções de backup
    /// </summary>
    public class BackupOptions
    {
        public bool IncludeMemories { get; set; } = true;
        public bool IncludeSessions { get; set; } = true;
        public bool IncludeMetadata { get; set; } = true;
        public bool CompressBackup { get; set; } = true;
        public string SpecificUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// Opções de restauração
    /// </summary>
    public class RestoreOptions
    {
        public bool OverwriteExisting { get; set; } = false;
        public bool RestoreOnlyMissing { get; set; } = true;
        public string TargetUserId { get; set; }
        public bool ValidateIntegrity { get; set; } = true;
    }

    /// <summary>
    /// Operação do repositório para transações
    /// </summary>
    public class RepositoryOperation
    {
        public OperationType Type { get; set; }
        public object Data { get; set; }
        public string Id { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// Tipos de operação do repositório
    /// </summary>
    public enum OperationType
    {
        CreateMemory,
        UpdateMemory,
        DeleteMemory,
        CreateSession,
        UpdateSession,
        DeleteSession
    }
}
