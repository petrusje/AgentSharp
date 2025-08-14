using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Abstractions;

namespace AgentSharp.Core.Storage
{
    /// <summary>
    /// Interface para fábrica de storage
    /// Responsável por criar instâncias de diferentes tipos de armazenamento
    /// </summary>
    public interface IStorageFactory
    {
        /// <summary>
        /// Cria uma instância de storage baseado no tipo
        /// </summary>
        /// <param name="storageType">Tipo de storage (sqlite, memory, etc.)</param>
        /// <param name="options">Opções de configuração do storage</param>
        /// <returns>Instância do storage</returns>
        IStorage CreateStorage(string storageType, StorageOptions options);
    }

    /// <summary>
    /// Opções de configuração para storage
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// String de conexão ou caminho do arquivo
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Serviço de embedding para storage semântico
        /// </summary>
        public object EmbeddingService { get; set; }

        /// <summary>
        /// Dimensões dos vetores (padrão: 1536)
        /// </summary>
        public int Dimensions { get; set; } = 1536;

        /// <summary>
        /// Configurações adicionais
        /// </summary>
        public StorageConfiguration Configuration { get; set; }

        /// <summary>
        /// Validação das opções
        /// </summary>
        public void Validate()
        {
            // Para storage em memória, connectionString pode ser null
            // Para outros tipos, connectionString é necessária
        }
    }

}