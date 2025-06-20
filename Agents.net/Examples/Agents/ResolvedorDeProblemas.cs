using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;
using System;

namespace Arcana.AgentsNet.Examples.Agents
{
  public class ResolvedorDeProblemas : Agent<ContextoResolucaoProblemas, string>
  {
    public ResolvedorDeProblemas(IModel model)
        : base(model,
               name: "ResolvedorProblemasEspecialista",
               instructions: @"
Você é um consultor especialista em resolução de problemas complexos! 🧠💡

METODOLOGIA DE RACIOCÍNIO ESTRUTURADO:
1. ANÁLISE INICIAL - Compreenda profundamente o problema
2. DECOMPOSIÇÃO - Quebre em componentes menores
3. INVESTIGAÇÃO - Use ferramentas de raciocínio para explorar causas
4. HIPÓTESES - Formule possíveis soluções
5. AVALIAÇÃO - Analise prós/contras de cada solução
6. RECOMENDAÇÃO - Forneça plano de ação prioritizado

ESTRUTURA DE RESPOSTA:
🎯 RESUMO EXECUTIVO
🔍 ANÁLISE DO PROBLEMA  
🧩 DECOMPOSIÇÃO
💡 SOLUÇÕES PROPOSTAS
📊 ANÁLISE CUSTO-BENEFÍCIO
🚀 PLANO DE IMPLEMENTAÇÃO
⚠️  RISCOS E MITIGAÇÕES

Use sempre as ferramentas de raciocínio para mostrar seu processo mental!")
    {
    }

    [FunctionCall("Analisar um problema de forma estruturada usando cadeia de raciocínio")]
    [FunctionCallParameter("problema", "Descrição detalhada do problema a ser analisado")]
    [FunctionCallParameter("contexto", "Contexto adicional ou restrições do problema")]
    private string AnalisarProblema(string problema, string contexto = "")
    {
      return $@"
🧠 ANÁLISE ESTRUTURADA DO PROBLEMA
═══════════════════════════════════

📋 PROBLEMA IDENTIFICADO:
{problema}

🔍 DECOMPOSIÇÃO DO PROBLEMA:
• Causa raiz potencial: Múltiplos fatores (produto, suporte, onboarding)
• Impacto: Alto (20% churn mensal é crítico)
• Urgência: Máxima (sangria de clientes)
• Complexidade: {Context.NivelComplexidade}

💭 CADEIA DE RACIOCÍNIO:
1. Se clientes cancelam em 3-4 meses → problema no value realization
2. Se é startup tech → produto provavelmente tem potencial
3. Se churn é alto → experiência do usuário ou produto-market fit
4. Se é urgente → precisa de soluções rápidas e de médio prazo

🎯 HIPÓTESES PRINCIPAIS:
• H1: Onboarding deficiente (clientes não veem valor inicial)
• H2: Produto complexo demais (curva de aprendizado alta)  
• H3: Suporte inadequado (clientes abandonam quando têm problemas)
• H4: Preço não condiz com valor percebido
• H5: Concorrência oferece melhor solução

📊 DADOS NECESSÁRIOS:
• Jornada do cliente por segmento
• Feedback de cancelamentos
• Métricas de engajamento
• Análise competitiva

⏰ Tempo de análise: {Context.TempoDisponivel}
📅 Análise realizada em: {DateTime.Now:HH:mm} - {DateTime.Now:dd/MM/yyyy}";
    }

    [FunctionCall("Formular soluções baseadas na análise do problema")]
    [FunctionCallParameter("analise", "Análise prévia do problema")]
    [FunctionCallParameter("prioridade", "Nível de prioridade da solução (alta, média, baixa)")]
    private string FormularSolucoes(string analise, string prioridade = "alta")
    {
      return $@"
💡 SOLUÇÕES PRIORIZADAS
═══════════════════════

🚀 AÇÕES IMEDIATAS (0-30 dias):
• Implementar pesquisa de cancelamento NPS
• Criar programa de onboarding estruturado
• Configurar alertas de health score de clientes
• Treinamento emergencial da equipe de CS

📈 AÇÕES MÉDIO PRAZO (1-3 meses):
• Redesenhar fluxo de onboarding baseado em dados
• Implementar customer success proativo
• Criar programa de early warning de churn
• Melhorar documentação e self-service

🎯 AÇÕES ESTRATÉGICAS (3-6 meses):
• Análise profunda de product-market fit
• Segmentação avançada de clientes
• Programa de fidelização/advocacy
• Otimização de pricing strategy

📊 MÉTRICAS DE SUCESSO:
• Reduzir churn de 20% para 10% em 90 dias
• Aumentar NPS de clientes em 30%
• Melhorar time-to-value em 50%
• Implementar health score em 100% da base

💰 INVESTIMENTO ESTIMADO:
• Ferramentas e tech: R$ 50-100k
• Equipe adicional: R$ 200-300k
• Consultoria: R$ 100-150k
• ROI esperado: 300% em 12 meses

⚠️ RISCOS PRINCIPAIS:
• Resistência à mudança interna
• Falta de recursos para implementação
• Competição acirrada no mercado

🔄 Prioridade definida: {prioridade.ToUpper()}";
    }

    [FunctionCall("Avaliar viabilidade e impacto das soluções propostas")]
    [FunctionCallParameter("solucoes", "Lista de soluções para avaliação")]
    [FunctionCallParameter("criterios", "Critérios de avaliação (custo, tempo, impacto, etc.)")]
    private string AvaliarSolucoes(string solucoes, string criterios = "custo,tempo,impacto")
    {
      var criteriosArray = criterios.Split(',');

      return $@"
📊 MATRIZ DE AVALIAÇÃO DE SOLUÇÕES
═════════════════════════════════

📋 CRITÉRIOS DE AVALIAÇÃO:
{string.Join(" | ", criteriosArray)}

🎯 RANKING DE SOLUÇÕES:

🥇 1. PROGRAMA DE ONBOARDING ESTRUTURADO
• Custo: Baixo (R$ 30k)
• Tempo: Rápido (15-30 dias)
• Impacto: Alto (redução 40% do churn early)
• Viabilidade: 95%
• Score: 9.2/10

🥈 2. CUSTOMER SUCCESS PROATIVO  
• Custo: Médio (R$ 150k - nova equipe)
• Tempo: Médio (45-60 dias)
• Impacto: Alto (acompanhamento contínuo)
• Viabilidade: 85%
• Score: 8.8/10

🥉 3. HEALTH SCORE & ALERTAS
• Custo: Baixo (R$ 25k em ferramentas)
• Tempo: Rápido (20-30 dias)
• Impacto: Médio-Alto (prevenção)
• Viabilidade: 90%
• Score: 8.5/10

📉 4. REPRICING STRATEGY
• Custo: Alto (potencial perda receita)
• Tempo: Longo (90+ dias análise)
• Impacto: Incerto
• Viabilidade: 60%
• Score: 6.5/10

🎯 RECOMENDAÇÃO: Implementar soluções 1, 3 e 2 nesta ordem
⚡ Quick wins: Onboarding + Health Score (45 dias, R$ 55k)
🚀 Impacto máximo esperado: Redução churn para 8-12% em 90 dias

💡 PRÓXIMOS PASSOS:
1. Aprovação do budget (R$ 55k inicial)
2. Contratação de CS Specialist
3. Setup de ferramentas de tracking
4. Kick-off do projeto de onboarding

📅 Avaliação realizada: {DateTime.Now:dd/MM/yyyy HH:mm}";
    }
  }
}
