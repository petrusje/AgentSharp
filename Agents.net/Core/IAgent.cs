using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Agents.net.Core
{
    /// <summary>
    /// Interface polimórfica para agentes AI, para uso em orquestração.
    /// </summary>
    public interface IAgent
    {
        string Name { get; }
        //Descrição do agente, para utilização as Tool
        string description { get; }

        void setContext(object context);

        Task<object> ExecuteAsync(string prompt, object context = null, List<AIMessage> messageHistory = null, CancellationToken cancellationToken = default);

        string GetSystemPrompt();
        List<AIMessage> GetMessageHistory();
    }
}