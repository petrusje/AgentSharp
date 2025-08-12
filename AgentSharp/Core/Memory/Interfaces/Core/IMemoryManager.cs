using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface de alto nível para gestão inteligente de memória do agente.
    /// 
    /// Esta é a interface principal para Agents que precisam de gestão avançada de memória,
    /// incluindo classificação automática, contexto de usuário/sessão e integração com LLM.
    /// 
    /// Funcionalidades principais:
    /// - Carregamento e gestão de contexto de memória
    /// - Classificação automática de tipos de memória via IA
    /// - Enriquecimento de mensagens com memórias relevantes
    /// - Extração automática de memórias de interações
    /// - Tools para LLM gerenciar memórias (CRUD completo)
    /// - Busca textual e semântica (quando embeddings disponíveis)
    /// 
    /// Diferenças do IMemory:
    /// - IMemory: Cache simples de baixo nível
    /// - IMemoryManager: Gestão inteligente de alto nível
    /// 
    /// Casos de uso:
    /// - Agents que precisam lembrar de preferências do usuário
    /// - Contexto de conversações longas
    /// - Personalização baseada em histórico
    /// - Gestão de conhecimento do usuário
    /// </summary>
    public interface IMemoryManager
    {
        /// <summary>
        /// ID do usuário para contextualização da memória
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// ID da sessão atual
        /// </summary>
        string SessionId { get; set; }

        /// <summary>
        /// Limite de memórias a serem recuperadas
        /// </summary>
        int? Limit { get; set; }

        /// <summary>
        /// Carrega o contexto de memória para um usuário e sessão
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>Contexto de memória carregado</returns>
        Task<AgentSharp.Core.Memory.Models.MemoryContext> LoadContextAsync(string userId, string sessionId = null);
        // ...existing code...

        /// <summary>
        /// Adiciona memórias relevantes às mensagens do modelo
        /// </summary>
        /// <param name="messages">Mensagens originais</param>
        /// <param name="context">Contexto de memória</param>
        /// <returns>Mensagens enriquecidas com memórias</returns>
        Task<List<AIMessage>> EnhanceMessagesAsync(List<AIMessage> messages, AgentSharp.Core.Memory.Models.MemoryContext context);
        // ...existing code...

        /// <summary>
        /// Processa uma interação (mensagem + resposta) para extrair e gerenciar memórias
        /// </summary>
        /// <param name="userMessage">Mensagem do usuário</param>
        /// <param name="assistantMessage">Resposta do assistente</param>
        /// <param name="context">Contexto de memória</param>
        /// <returns>Task de processamento</returns>
        Task ProcessInteractionAsync(AIMessage userMessage, AIMessage assistantMessage, AgentSharp.Core.Memory.Models.MemoryContext context);
        // ...existing code...

        /// <summary>
        /// Executa o gerenciador de memória com uma mensagem
        /// </summary>
        /// <param name="message">Mensagem para processar</param>
        /// <param name="context">Contexto de memória</param>
        /// <returns>Resposta do processamento</returns>
        Task<string> RunAsync(string message, AgentSharp.Core.Memory.Models.MemoryContext context = null);
        // ...existing code...

        /// <summary>
        /// Obtém memórias existentes filtradas por contexto
        /// </summary>
        /// <param name="context">Contexto para filtrar</param>
        /// <param name="limit">Limite de resultados</param>
        /// <returns>Lista de memórias</returns>
        Task<List<UserMemory>> GetExistingMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null, int? limit = null);
        // ...existing code...

        /// <summary>
        /// Adiciona uma nova memória
        /// </summary>
        /// <param name="memory">Conteúdo da memória</param>
        /// <param name="context">Contexto da memória</param>
        /// <returns>Mensagem de status</returns>
        Task<string> AddMemoryAsync(string memory, AgentSharp.Core.Memory.Models.MemoryContext context = null);
        // ...existing code...

        /// <summary>
        /// Atualiza uma memória existente
        /// </summary>
        /// <param name="id">ID da memória</param>
        /// <param name="memory">Novo conteúdo</param>
        /// <param name="context">Contexto da memória</param>
        /// <returns>Mensagem de status</returns>
        Task<string> UpdateMemoryAsync(string id, string memory, AgentSharp.Core.Memory.Models.MemoryContext context = null);
        // ...existing code...

        /// <summary>
        /// Remove uma memória
        /// </summary>
        /// <param name="id">ID da memória</param>
        /// <returns>Mensagem de status</returns>
        Task<string> DeleteMemoryAsync(string id);
        // ...existing code...

        /// <summary>
        /// Limpa todas as memórias do contexto atual
        /// </summary>
        /// <param name="context">Contexto para limpar (ou null para limpar tudo)</param>
        /// <returns>Mensagem de status</returns>
        Task<string> ClearMemoryAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null);
        // ...existing code...
    }
}
