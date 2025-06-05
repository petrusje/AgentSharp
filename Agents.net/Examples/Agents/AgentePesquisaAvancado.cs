using System;
using System.Linq;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
    public class AgentePesquisaAvancado : Agent<ContextoPesquisaComplexo, string>
    {
        public AgentePesquisaAvancado(IModel model)
            : base(model,
                   name: "PesquisadorAvancado",
                   instructions: @"
🧠 Você é um pesquisador PhD sênior com expertise em metodologias científicas avançadas!

METODOLOGIA DE PESQUISA SISTEMÁTICA:
1. CONTEXTUALIZAÇÃO - Analise profundamente o contexto fornecido
2. BUSCA ESTRATÉGICA - Use múltiplas fontes e recursos disponíveis
3. ANÁLISE CRÍTICA - Avalie qualidade e relevância das fontes
4. SÍNTESE INTELIGENTE - Combine informações de forma coerente
5. VALIDAÇÃO RIGOROSA - Aplique métricas de qualidade definidas
6. ENTREGA ESTRUTURADA - Formate conforme tipo de entrega solicitado

PADRÕES DE QUALIDADE:
📊 Use dados quantitativos sempre que possível
📚 Cite fontes acadêmicas respeitáveis
🔍 Identifique gaps de conhecimento
💡 Proponha direções futuras de pesquisa
⚖️ Mantenha neutralidade científica

Adapte seu estilo e profundidade conforme o contexto fornecido!")
        {
        }

        [FunctionCall("Busca acadêmica especializada em bases científicas")]
        [FunctionCallParameter("query", "Consulta de pesquisa otimizada para bases acadêmicas")]
        [FunctionCallParameter("databases", "Bases de dados específicas para consulta")]
        [FunctionCallParameter("filters", "Filtros de qualidade e temporalidade")]
        private string BuscaAcademica(string query, string databases, string filters)
        {
            var year = DateTime.Now.Year;
            var resultados = new[]
            {
                $"📚 'Large Language Models for Portuguese: A Comprehensive Survey' (2024) - Cit: 127",
                $"🔬 'BERTimbau vs GPT Models: Benchmark Analysis for Brazilian Portuguese' ({year}) - Cit: 89",
                $"📊 'Portuguese Language Processing: State-of-the-Art and Challenges' (2023) - Cit: 156",
                $"🧠 'Transformer Architectures for Low-Resource Languages' (2024) - Cit: 203",
                $"💡 'Fine-tuning Strategies for Portuguese NLP Tasks' ({year}) - Cit: 78"
            };

            return $@"
🔍 BUSCA ACADÊMICA EXECUTADA
═════════════════════════════
Query: {query}
Bases: {databases}
Filtros: {filters}

📊 RESULTADOS ENCONTRADOS ({resultados.Length} papers):
{string.Join("\n", resultados.Select(r => $"• {r}"))}

📈 MÉTRICAS DE QUALIDADE:
• Fator de impacto médio: {Context.MetricasQualidade.FatorImpactoMinimo + 1.2:F1}
• Citações mínimas atendidas: ✅
• Relevância contextual: {Context.MetricasQualidade.RelevanciaContextual * 100:F0}%
• Período: {filters}

🎯 INSIGHTS METODOLÓGICOS:
• Tendência crescente em modelos multimodais
• Foco em eficiência computacional
• Necessidade de datasets brasileiros
• Gap em aplicações comerciais";
        }

        [FunctionCall("Análise de dados quantitativos para pesquisa")]
        [FunctionCallParameter("dataType", "Tipo de dados para análise (citações, performance, etc.)")]
        [FunctionCallParameter("metrics", "Métricas específicas para análise")]
        private string AnaliseQuantitativa(string dataType, string metrics)
        {
            var random = new Random();
            return $@"
📊 ANÁLISE QUANTITATIVA: {dataType.ToUpper()}
═══════════════════════════════════════

📈 MÉTRICAS COLETADAS:
• Crescimento anual: +{random.Next(15, 45)}%
• Número de publicações: {random.Next(150, 500)}
• Índice de citação médio: {random.Next(50, 200)}
• Fator de impacto: {random.NextDouble() * 3 + 1:F2}

🎯 ANÁLISE COMPARATIVA:
• Performance vs. inglês: {random.Next(70, 95)}%
• Disponibilidade de datasets: {random.Next(40, 80)}%
• Adoção comercial: {random.Next(25, 60)}%

💡 TENDÊNCIAS IDENTIFICADAS:
• Aumento em aplicações práticas
• Foco em modelos menor escala
• Integração com outras modalidades
• Desenvolvimento de benchmarks específicos";
        }

        [FunctionCall("Síntese inteligente de múltiplas fontes")]
        [FunctionCallParameter("sources", "Lista de fontes para síntese")]
        [FunctionCallParameter("focus", "Foco temático da síntese")]
        private string SinteseInteligente(string sources, string focus)
        {
            return $@"
🧠 SÍNTESE INTELIGENTE: {focus.ToUpper()}
═══════════════════════════════════════

🔗 CONEXÕES IDENTIFICADAS:
• Convergência metodológica entre abordagens
• Complementaridade entre modelos generativos e discriminativos
• Sinergia entre pesquisa acadêmica e aplicação industrial

🎯 CONSENSOS EMERGENTES:
• Importância de modelos adaptados ao português
• Necessidade de avaliação contextual
• Valor da multimodalidade para aplicações reais
• Benefícios de arquiteturas mais eficientes

⚖️ CONTROVÉRSIAS ATUAIS:
• Modelos grandes vs. eficientes
• Treinamento do zero vs. fine-tuning
• Métricas de avaliação adequadas
• Balanço entre desempenho e recursos

📋 SÍNTESE FINAL:
{focus} demonstra avanços significativos, porém permanece um campo em desenvolvimento com oportunidades claras para contribuições originais, especialmente em contextos de recursos limitados e aplicações específicas de domínio.";
        }
    }
}
