using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para serviços de RAG/Knowledge externos
    /// Permite integração com sistemas de busca semântica customizados
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Busca documentos/conhecimento relacionado à query
        /// </summary>
        /// <param name="query">Consulta de busca</param>
        /// <param name="limit">Número máximo de resultados</param>
        /// <returns>Lista de documentos relevantes</returns>
        Task<List<KnowledgeDocument>> SearchAsync(string query, int limit = 5);
        
        /// <summary>
        /// Verifica se o serviço está disponível/configurado
        /// </summary>
        bool IsAvailable { get; }
    }
    
    /// <summary>
    /// Documento de conhecimento retornado pela busca
    /// </summary>
    public class KnowledgeDocument
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public double Relevance { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}