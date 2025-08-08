using AgentSharp.Attributes;
using AgentSharp.Core;
using System;
using System.Threading.Tasks;

namespace AgentSharp.Tools
{
    /// <summary>
    /// ToolPack para manipulação e acesso ao resumo de contexto do agente.
    /// Permite que o LLM acesse ou atualize o resumo do contexto via FunctionCall.
    /// </summary>
    public class ContextSummaryToolPack : ToolPack
    {
        public ContextSummaryToolPack()
        {
            Name = "ContextSummaryToolPack";
            Description = "Ferramentas para manipulação do resumo de contexto do agente.";
            Version = "1.0.0";
        }

        /// <summary>
        /// Retorna o resumo atual do contexto do agente.
        /// </summary>
        [FunctionCall("Obter o resumo do contexto atual do agente")]
        public string GetContextSummary()
        {
            var summary = _context?.GetMemorySummary();
            return summary ?? "";
        }

        /// <summary>
        /// Atualiza o resumo do contexto do agente.
        /// </summary>
        /// <param name="summary">Novo resumo do contexto</param>
        [FunctionCall("Atualizar o resumo do contexto do agente")]
        [FunctionCallParameter("summary", "Novo resumo do contexto")]
        public void SetContextSummary(string summary)
        {
            // Se o agente implementar um setter, pode ser chamado aqui
            // Exemplo: _context.SetMemorySummary(summary);
            // Por padrão, não faz nada
        }
    }
}
