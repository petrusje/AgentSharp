using System.Threading;
using System.Threading.Tasks;

namespace Agents.net.Models
{
    /// <summary>
    /// Interface para fábrica de modelos
    /// </summary>
    public interface IModelFactory
    {
        /// <summary>
        /// Cria uma instância de um modelo
        /// </summary>
        /// <param name="modelType">Tipo de modelo</param>
        /// <param name="options">Opções de configuração</param>
        /// <returns>Instância do modelo</returns>
        IModel CreateModel(string modelType, ModelOptions options);
    }
}