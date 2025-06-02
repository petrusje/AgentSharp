using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agents.net.Attributes;
using Agents.net.Core;

namespace Agents.net.Tools
{
    /// <summary>
    /// ToolPack providing reasoning and problem-solving tools
    /// </summary>
    public class ReasoningToolPack : Toolkit
    {
        /// <summary>
        /// Creates a new ReasoningToolPack
        /// </summary>
        /// <param name="name">Name of the tool pack</param>
        /// <param name="cacheResults">Whether to cache tool results (default: true)</param>
        /// <param name="cacheTtl">Cache time-to-live in seconds (default: 300)</param>
        public ReasoningToolPack(
            string name = "reasoning", 
            bool cacheResults = true, 
            int cacheTtl = 300) 
            : base(
                name,
                "Use these tools for reasoning, problem decomposition, and solution evaluation.",
                addInstructions: true,
                cacheResults: cacheResults,
                cacheTtl: cacheTtl)
        {
        }

        /// <summary>
        /// Decompõe um problema complexo em sub-problemas menores e mais gerenciáveis
        /// </summary>
        /// <param name="problema">O problema complexo a ser decomposto</param>
        /// <param name="maxSubproblemas">Número máximo de sub-problemas a criar (padrão: 5)</param>
        [FunctionCall("Decompõe um problema complexo em sub-problemas menores e mais gerenciáveis")]
        [FunctionCallParameter("problema", "O problema complexo a ser decomposto")]
        [FunctionCallParameter("maxSubproblemas", "Número máximo de sub-problemas a criar (padrão: 5)")]
        private async Task<string> DecomporProblemaAsync(string problema, int maxSubproblemas = 5)
        {
            
            var subproblemas = new List<string>();
            var partes = problema.Split(new[] { " e ", " ou ", " então ", " para ", " com " }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < Math.Min(partes.Length, maxSubproblemas); i++)
            {
                subproblemas.Add($"Sub-problema {i + 1}: {partes[i].Trim()}");
            }
            
            if (subproblemas.Count == 0)
            {
                subproblemas.Add($"Sub-problema 1: Analisar '{problema}'");
                subproblemas.Add($"Sub-problema 2: Identificar fatores-chave");
                subproblemas.Add($"Sub-problema 3: Desenvolver estratégia de solução");
            }
            
            return $"Problema decomposto em {subproblemas.Count} partes:\n" + 
                   string.Join("\n", subproblemas);
        }

        /// <summary>
        /// Executa uma cadeia de raciocínio passo-a-passo para resolver um problema
        /// </summary>
        /// <param name="questao">O problema ou questão a ser analisada</param>
        /// <param name="contexto">Contexto adicional ou informações relevantes</param>
        [FunctionCall("Executa uma cadeia de raciocínio passo-a-passo para resolver um problema")]
        [FunctionCallParameter("questao", "O problema ou questão a ser analisada")]
        [FunctionCallParameter("contexto", "Contexto adicional ou informações relevantes")]
        private async Task<string> CadeiaRaciocinioAsync(string questao, string contexto = "")
        {
            
            var passos = new List<string>
            {
                $"🎯 **Objetivo**: {questao}",
                $"📋 **Contexto**: {(string.IsNullOrEmpty(contexto) ? "Informações limitadas" : contexto)}",
                "🔍 **Análise**: Identificando fatores-chave e variáveis relevantes",
                "💭 **Hipóteses**: Considerando diferentes abordagens possíveis",
                "⚖️ **Avaliação**: Pesando prós e contras de cada opção",
                "🎯 **Conclusão**: Direcionando para a melhor solução baseada na análise"
            };
            
            return "**Cadeia de Raciocínio:**\n\n" + string.Join("\n\n", passos);
        }

        /// <summary>
        /// Avalia a qualidade e viabilidade de uma solução proposta
        /// </summary>
        /// <param name="solucao">A solução proposta a ser avaliada</param>
        /// <param name="criterios">Critérios específicos de avaliação (ex: custo, tempo, eficácia)</param>
        [FunctionCall("Avalia a qualidade e viabilidade de uma solução proposta")]
        [FunctionCallParameter("solucao", "A solução proposta a ser avaliada")]
        [FunctionCallParameter("criterios", "Critérios específicos de avaliação (ex: custo, tempo, eficácia)")]
        private async Task<string> AvaliarSolucaoAsync(string solucao, string criterios = "")
        {
            
            var pontuacao = CalcularPontuacao(solucao);
            string nivel;
            if (pontuacao >= 8)
                nivel = "Excelente ⭐⭐⭐⭐⭐";
            else if (pontuacao >= 6)
                nivel = "Boa ⭐⭐⭐⭐";
            else if (pontuacao >= 4)
                nivel = "Razoável ⭐⭐⭐";
            else if (pontuacao >= 2)
                nivel = "Fraca ⭐⭐";
            else
                nivel = "Inadequada ⭐";
            
            var avaliacaoDetalhada = new List<string>
            {
                $"**Pontuação Geral**: {pontuacao}/10 - {nivel}",
                "",
                "**Análise por Dimensões**:",
                $"• Viabilidade Técnica: {Math.Min(pontuacao + 1, 10)}/10",
                $"• Custo-Benefício: {Math.Max(pontuacao - 1, 1)}/10", 
                $"• Facilidade de Implementação: {pontuacao}/10",
                $"• Impacto Esperado: {Math.Min(pontuacao + 2, 10)}/10",
                "",
                "**Recomendações**:",
                pontuacao >= 7 ? "✅ Solução recomendada para implementação" :
                pontuacao >= 5 ? "⚠️ Solução viável com algumas melhorias necessárias" :
                "❌ Solução precisa ser reformulada"
            };
            
            if (!string.IsNullOrEmpty(criterios))
            {
                avaliacaoDetalhada.Add("");
                avaliacaoDetalhada.Add($"**Critérios Específicos**: {criterios}");
            }
            
            return string.Join("\n", avaliacaoDetalhada);
        }

        /// <summary>
        /// Identifica possíveis obstáculos e riscos em uma abordagem
        /// </summary>
        /// <param name="abordagem">A abordagem ou plano a ser analisado</param>
        /// <param name="dominio">Área ou domínio específico (ex: tecnologia, negócios, legal)</param>
        [FunctionCall("Identifica possíveis obstáculos e riscos em uma abordagem")]
        [FunctionCallParameter("abordagem", "A abordagem ou plano a ser analisado")]
        [FunctionCallParameter("dominio", "Área ou domínio específico (ex: tecnologia, negócios, legal)")]
        private async Task<string> IdentificarObstaculosAsync(string abordagem, string dominio = "geral")
        {
            
            var obstaculos = new List<string>
            {
                "**Obstáculos Potenciais Identificados:**",
                "",
                "🚧 **Técnicos**:",
                "• Complexidade de implementação maior que esperada",
                "• Dependências externas não consideradas",
                "• Limitações de recursos ou tecnologia",
                "",
                "💰 **Financeiros**:",
                "• Custos ocultos não previstos",
                "• ROI inferior ao esperado",
                "• Necessidade de investimentos adicionais",
                "",
                "⏰ **Temporais**:",
                "• Prazos muito otimistas",
                "• Dependências que podem causar atrasos",
                "• Falta de recursos em momentos críticos",
                "",
                "👥 **Humanos**:",
                "• Resistência à mudança",
                "• Falta de competências específicas",
                "• Sobrecarga da equipe",
                "",
                $"**Específico para {dominio}**:",
                "• Regulamentações específicas do setor",
                "• Concorrência e fatores de mercado",
                "• Mudanças tecnológicas rápidas"
            };
            
            return string.Join("\n", obstaculos);
        }

        /// <summary>
        /// Gera alternativas criativas para abordar um problema
        /// </summary>
        /// <param name="problema">O problema que precisa de soluções alternativas</param>
        /// <param name="numeroAlternativas">Número de alternativas a gerar (padrão: 3)</param>
        [FunctionCall("Gera alternativas criativas para abordar um problema")]
        [FunctionCallParameter("problema", "O problema que precisa de soluções alternativas")]
        [FunctionCallParameter("numeroAlternativas", "Número de alternativas a gerar (padrão: 3)")]
        private async Task<string> GerarAlternativasAsync(string problema, int numeroAlternativas = 3)
        {
            
            var abordagens = new List<string>
            {
                "Abordagem Incremental", "Abordagem Revolucionária", "Abordagem Colaborativa",
                "Abordagem Minimalista", "Abordagem Tecnológica", "Abordagem Tradicional",
                "Abordagem Híbrida", "Abordagem Outsourcing", "Abordagem DIY"
            };
            
            var alternativas = new List<string> { "**Alternativas Geradas:**", "" };
            
            var random = new Random();
            var abordagensSelecionadas = abordagens.OrderBy(x => random.Next()).Take(numeroAlternativas);
            
            int contador = 1;
            foreach (var abordagem in abordagensSelecionadas)
            {
                alternativas.Add($"**{contador}. {abordagem}**");
                alternativas.Add($"   📝 Descrição: Solução baseada em princípios de {abordagem.Split(' ')[1].ToLower()}");
                alternativas.Add($"   ⚡ Vantagem: Oferece uma perspectiva única para '{problema}'");
                alternativas.Add($"   ⚠️ Consideração: Requer análise específica de viabilidade");
                alternativas.Add("");
                contador++;
            }
            
            return string.Join("\n", alternativas);
        }

        private static int CalcularPontuacao(string solucao)
        {
            var pontuacao = 5; // Base
            
            // Fatores que aumentam a pontuação
            if (solucao.Contains("específic") || solucao.Contains("detalh")) pontuacao += 1;
            if (solucao.Contains("viável") || solucao.Contains("prático")) pontuacao += 1;
            if (solucao.Contains("custo") || solucao.Contains("econômic")) pontuacao += 1;
            if (solucao.Length > 100) pontuacao += 1; // Soluções mais elaboradas
            if (solucao.Contains("etap") || solucao.Contains("passo")) pontuacao += 1;
            
            // Fatores que diminuem a pontuação
            if (solucao.Contains("impossível") || solucao.Contains("difícil")) pontuacao -= 2;
            if (solucao.Length < 50) pontuacao -= 1; // Soluções muito vagas
            
            return Math.Max(1, Math.Min(10, pontuacao));
        }
    }
} 