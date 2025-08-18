using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory
{
    /// <summary>
    /// Interface de baixo nível para cache simples de memória.
    /// 
    /// Esta interface é destinada ao armazenamento básico de itens genéricos em memória,
    /// sendo usada principalmente pelo ExecutionEngine para logging e cache temporário.
    /// 
    /// Para gestão inteligente de memória com classificação automática, contexto de usuário
    /// e integração com LLM, use IMemoryManager em vez desta interface.
    /// 
    /// Casos de uso típicos:
    /// - Cache de resultados de ferramentas
    /// - Log de respostas do modelo
    /// - Armazenamento temporário durante execução
    /// 
    /// Não usar para:
    /// - Memória persistente de usuários
    /// - Classificação inteligente de memórias
    /// - Integração com contexto de conversas
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Adiciona um item à memória
        /// </summary>
        Task AddItemAsync(MemoryItem item);

        /// <summary>
        /// Obtém itens da memória
        /// </summary>
        Task<List<MemoryItem>> GetItemsAsync(string type = null, int limit = 10);

        /// <summary>
        /// Remove um item da memória
        /// </summary>
        Task RemoveItemAsync(string id);

        /// <summary>
        /// Limpa a memória
        /// </summary>
        Task ClearAsync();
    }
}
