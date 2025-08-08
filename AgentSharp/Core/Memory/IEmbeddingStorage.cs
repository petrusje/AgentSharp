using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de embeddings (busca semântica).
    /// </summary>
    public interface IEmbeddingStorage
    {
        Task<string> StoreEmbeddingAsync(string content, List<float> embedding, Dictionary<string, object> metadata = null);
        Task<List<(string content, List<float> embedding, double similarity)>> SearchSimilarAsync(List<float> queryEmbedding, int limit = 10, double threshold = 0.7);
        Task DeleteEmbeddingAsync(string id);
        Task ClearEmbeddingsAsync();
        void Clear();
    }
}
