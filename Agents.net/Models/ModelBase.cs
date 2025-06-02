using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Tools;
using Agents.net.Utils;

namespace Agents.net.Models
{
    /// <summary>
    /// Classe base para todas as implementações de modelos de IA
    /// </summary>
    public abstract class ModelBase : IModel
    {
        /// <summary>
        /// Nome ou ID do modelo
        /// </summary>
        public string ModelName { get; }

        /// <summary>
        /// Logger para registro de operações do modelo
        /// </summary>
        //protected readonly ILogger Logger;

        /// <summary>
        /// Inicializa uma nova instância da classe ModelBase
        /// </summary>
        /// <param name="modelName">ID do modelo</param>
        /// <param name="logger">Logger opcional, se não for fornecido será usado o logger padrão</param>
        protected ModelBase(string modelName, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentException("O nome do modelo não pode ser vazio", nameof(modelName));

            ModelName = modelName;
            //Logger = logger ?? new ConsoleLogger();
        }

        /// <summary>
        /// Gera uma resposta assíncrona com base na requisição
        /// </summary>
        /// <param name="request">Requisição contendo mensagens e ferramentas</param>
        /// <param name="config">Configuração para o modelo</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>A resposta do modelo</returns>
        public abstract Task<ModelResponse> GenerateResponseAsync(
            ModelRequest request,
            ModelConfiguration config = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gera uma resposta em streaming com base na requisição
        /// </summary>
        /// <param name="request">Requisição contendo mensagens e ferramentas</param>
        /// <param name="config">Configuração para o modelo</param>
        /// <param name="handler">Handler para processar chunks da resposta</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>A resposta completa do modelo após o streaming terminar</returns>
        public abstract Task<ModelResponse> GenerateStreamingResponseAsync(
            ModelRequest request,
            ModelConfiguration config,
            Action<string> handler,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna informação sobre o modelo
        /// </summary>
        /// <returns>String contendo detalhes do modelo</returns>
        public override string ToString()
        {
            return $"Model: {ModelName}";
        }
    }
}