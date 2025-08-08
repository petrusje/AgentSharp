using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface principal para serviços de memória
    /// Responsável por operações CRUD de alto nível para memórias de agentes
    /// </summary>
    public interface IMemoryService
    {
        /// <summary>
        /// Cria uma nova memória para um agente
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="content">Conteúdo inicial da memória</param>
        /// <param name="metadata">Metadados adicionais</param>
        /// <returns>ID da memória criada</returns>
        Task<string> CreateMemoryAsync(string sessionId, string userId, string content = null, Dictionary<string, object> metadata = null);

        /// <summary>
        /// Obtém uma memória específica por ID
        /// </summary>
        /// <param name="memoryId">ID da memória</param>
        /// <returns>Dados da memória</returns>
        Task<AgentMemory> GetMemoryAsync(string memoryId);

        /// <summary>
        /// Obtém todas as memórias de um usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="retrievalType">Tipo de recuperação</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias</returns>
        Task<IEnumerable<AgentMemory>> GetMemoriesAsync(string userId, MemoryRetrieval retrievalType = MemoryRetrieval.Recent, int limit = 10);

        /// <summary>
        /// Atualiza uma memória existente
        /// </summary>
        /// <param name="memoryId">ID da memória</param>
        /// <param name="content">Novo conteúdo</param>
        /// <param name="metadata">Novos metadados</param>
        /// <returns>True se atualizada com sucesso</returns>
        Task<bool> UpdateMemoryAsync(string memoryId, string content = null, Dictionary<string, object> metadata = null);

        /// <summary>
        /// Remove uma memória
        /// </summary>
        /// <param name="memoryId">ID da memória</param>
        /// <returns>True se removida com sucesso</returns>
        Task<bool> DeleteMemoryAsync(string memoryId);

        /// <summary>
        /// Busca memórias por conteúdo ou metadados
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="query">Consulta de busca</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias relevantes</returns>
        Task<IEnumerable<AgentMemory>> SearchMemoriesAsync(string userId, string query, int limit = 10);

        /// <summary>
        /// Cria um resumo das memórias de uma sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resumo da sessão</returns>
        Task<SessionSummary> CreateSummaryAsync(string sessionId, string userId);

        /// <summary>
        /// Limpa memórias antigas baseado em critérios
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="retentionDays">Dias de retenção</param>
        /// <param name="maxMemories">Número máximo de memórias a manter</param>
        /// <returns>Número de memórias removidas</returns>
        Task<int> CleanupMemoriesAsync(string userId, int retentionDays = 30, int maxMemories = 1000);
    }
}
