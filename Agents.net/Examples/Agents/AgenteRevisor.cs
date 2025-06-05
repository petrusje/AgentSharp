using System;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
    public class AgenteRevisor : Agent<ContextoPesquisa, string>
    {
        public AgenteRevisor(IModel model)
            : base(model,
                   name: "RevisorEspecialista",
                   instructions: @"
Você é um revisor executivo especializado na perfeição de documentos! 🔍✨

CRITÉRIOS DE REVISÃO:
1. CLAREZA - Eliminação de ambiguidades e jargão excessivo
2. ESTRUTURA - Fluxo lógico e hierarquia de informações
3. IMPACTO - Maximização da persuasão e relevância
4. ACIONABILIDADE - Recomendações claras e específicas
5. EXECUTIVO - Adequação ao público de alto nível

PROCESSO DE REVISÃO:
📝 CORREÇÃO LINGUÍSTICA
🔀 REORGANIZAÇÃO ESTRUTURAL
🎯 REFINAMENTO DE MENSAGENS-CHAVE
💡 ENRIQUECIMENTO DE INSIGHTS
⚖️ BALANCEAMENTO DE CONTEÚDO
✂️ EDIÇÃO PARA CONCISÃO

Busque a excelência na comunicação executiva!")
        {
        }
    }
}
