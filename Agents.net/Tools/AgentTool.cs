using Agents.net.Attributes;
using Agents.net.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Agents.net.Tools
{
    /// <summary>
    /// Adapta um Agent para ser exposto como Tool para orquestração.
    /// </summary> s
    public class AgentTool : Tool
    {
        private readonly IAgent _agent;

        public AgentTool(IAgent agent, string toolName, string description)
            : base(toolName, description, null, null, false, ToolKind.Agent)
        {

            _agent = agent;
            MethodInfo = this.GetType().GetMethod("RunAgent");
            ParametersSchema = base.GenerateParametersSchema();

        }

        public object Context { get; set; }
        public System.Collections.Generic.List<Core.AIMessage> MessageHistory { get; set; }

        [FunctionCallParameter("Prompt", "Prompt com a taref para o Agente")]
        public string RunAgent( string prompt )
        {
            // Executa o agente com o prompt fornecido
            var result = _agent.ExecuteAsync(prompt, Context, MessageHistory);
            return result.Result != null ? result.Result.ToString() : string.Empty;
        }
    }
}