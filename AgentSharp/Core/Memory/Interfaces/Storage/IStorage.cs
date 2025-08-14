using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface base para persistência de dados de memória do agente.
    /// Define métodos comuns para gerenciar sessões, memórias e operações básicas de armazenamento.
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Storage especializado para sessões
        /// </summary>
        ISessionStorage Sessions { get; }

        /// <summary>
        /// Storage especializado para memórias
        /// </summary>
        IMemoryStorage Memories { get; }

        /// <summary>
        /// Storage especializado para embeddings
        /// </summary>
        IEmbeddingStorage Embeddings { get; }

        /// <summary>
        /// Inicializa o storage (conexão, criação de estruturas, etc.)
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Limpa todos os dados do storage
        /// </summary>
        Task ClearAllAsync();

        #region Métodos Legados (para compatibilidade com código existente)

        /// <summary>
        /// Verifica se a conexão está ativa
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Conecta ao provedor de armazenamento
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Desconecta do provedor de armazenamento
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Salva uma mensagem no armazenamento
        /// </summary>
        Task SaveMessageAsync(Message message);

        /// <summary>
        /// Recupera mensagens de uma sessão
        /// </summary>
        Task<List<Message>> GetSessionMessagesAsync(string sessionId, int? limit = null);

        /// <summary>
        /// Salva uma memória no armazenamento
        /// </summary>
        Task SaveMemoryAsync(UserMemory memory);

        /// <summary>
        /// Recupera memórias por contexto
        /// </summary>
        Task<List<UserMemory>> GetMemoriesAsync(MemoryContext context = null, int? limit = null);

        /// <summary>
        /// Busca memórias por conteúdo
        /// </summary>
        Task<List<UserMemory>> SearchMemoriesAsync(string query, MemoryContext context, int limit = 10);

        /// <summary>
        /// Atualiza uma memória existente
        /// </summary>
        Task UpdateMemoryAsync(UserMemory memory);

        /// <summary>
        /// Remove uma memória
        /// </summary>
        Task DeleteMemoryAsync(string id);

        /// <summary>
        /// Remove todas as memórias de um contexto
        /// </summary>
        Task ClearMemoriesAsync(MemoryContext context = null);

        /// <summary>
        /// Cria ou obtém uma sessão
        /// </summary>
        Task<ISession> GetOrCreateSessionAsync(string sessionId, string userId);

        /// <summary>
        /// Lista sessões de um usuário
        /// </summary>
        Task<List<ISession>> GetUserSessionsAsync(string userId, int? limit = null);

        /// <summary>
        /// Remove uma sessão e todos os seus dados
        /// </summary>
        Task DeleteSessionAsync(string sessionId);

        /// <summary>
        /// Obtém histórico de mensagens de uma sessão para contexto conversacional (como /refs)
        /// </summary>
        Task<List<AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10);

        /// <summary>
        /// Salva uma mensagem no histórico da sessão
        /// </summary>
        Task SaveSessionMessageAsync(string userId, string sessionId, AIMessage message);

        #endregion
    }
}
