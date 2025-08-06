using AgentSharp.Attributes;
using AgentSharp.Core;
using AgentSharp.Examples.Contexts;
using AgentSharp.Models;

namespace AgentSharp.Examples.Agents
{
  public class AgenteAnalista : Agent<ContextoPesquisa, string>
  {
    public AgenteAnalista(IModel model)
        : base(model,
               name: "AnalistaEstrategico",
               instructions: @"
Você é um analista estratégico sênior especializado em tecnologia! 📊🎯

METODOLOGIA DE ANÁLISE:
1. DIAGNÓSTICO - Identifique status quo e gaps
2. BENCHMARKING - Compare com cenário global
3. SWOT - Forças, fraquezas, oportunidades, ameaças
4. CENÁRIOS - Projete futuros possíveis
5. RECOMENDAÇÕES - Sugira ações estratégicas

FRAMEWORK DE ANÁLISE:
🎯 POSICIONAMENTO COMPETITIVO
📈 ANÁLISE DE TENDÊNCIAS
💡 IDENTIFICAÇÃO DE OPORTUNIDADES
⚠️ MAPEAMENTO DE RISCOS
🚀 RECOMENDAÇÕES ESTRATÉGICAS

Seja analítico, baseado em dados e projete insights acionáveis!")
    {
    }

    [FunctionCall("Analisar competitividade do setor")]
    [FunctionCallParameter("dados", "Dados do setor para análise")]
    [FunctionCallParameter("benchmark", "País/região para comparação")]
    private string AnalisarCompetitividade(string dados, string benchmark = "EUA")
    {
      return $@"
📊 ANÁLISE COMPETITIVA - BRASIL vs {benchmark.ToUpper()}
═══════════════════════════════════════════════════

🎯 POSICIONAMENTO ATUAL:
• Ranking Global IA: Brasil #13, {benchmark} #1
• Investimento per capita: BR R$140, {benchmark} R$2.300
• Patentes registradas: BR 240, {benchmark} 12.500
• Startups unicórnio: BR 2, {benchmark} 47

📈 FORÇAS IDENTIFICADAS:
• Talento técnico abundante e qualificado
• Mercado interno grande (220M habitantes)
• Agronegócio líder mundial (caso de uso único)
• Ecossistema fintech maduro
• Universidades de qualidade (USP, UNICAMP)

⚠️ FRAQUEZAS CRÍTICAS:
• Baixo investimento em P&D (1.2% PIB vs 3.4%)
• Fuga de cérebros para exterior
• Infraestrutura de dados limitada
• Regulamentação em desenvolvimento
• Acesso restrito a dados públicos

🚀 OPORTUNIDADES:
• IA para inclusão social (banking, education)
• Agricultura 4.0 e sustentabilidade
• Telemedicina e saúde digital
• Smart cities e governo digital
• Processamento de linguagem natural PT-BR

🔴 AMEAÇAS:
• Dependência tecnológica externa
• Competição global por talentos
• Mudanças regulatórias internacionais
• Concentração em poucos players
• Gap de infraestrutura digital

🎯 POSIÇÃO ESTRATÉGICA RECOMENDADA:
Foco em nichos de vantagem competitiva:
1. AgTech e sustentabilidade
2. FinTech e inclusão financeira
3. HealthTech e medicina preventiva
4. GovTech e serviços públicos digitais

📊 Score Competitivo: 6.8/10 (Potencial: 8.2/10)";
    }
  }
}
