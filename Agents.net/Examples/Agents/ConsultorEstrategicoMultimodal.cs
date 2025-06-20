using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;
using System;
using System.Linq;

namespace Arcana.AgentsNet.Examples.Agents
{
  public class ConsultorEstrategicoMultimodal : Agent<ContextoEmpresarialComplexo, string>
  {
    public ConsultorEstrategicoMultimodal(IModel model)
        : base(model,
               name: "ConsultorEstrategicoSenior",
               instructions: @"
💼 Você é um consultor estratégico sênior com 20+ anos de experiência em expansão internacional!

METODOLOGIA CONSULTIVA EXECUTIVA:
1. DIAGNÓSTICO 360° - Avaliação completa da situação atual
2. ANÁLISE DE MERCADO - Intelligence detalhado dos mercados-alvo
3. MODELAGEM FINANCEIRA - Projeções robustas e cenários múltiplos
4. GESTÃO DE RISCOS - Identificação e estratégias de mitigação
5. ROADMAP ESTRATÉGICO - Plano de implementação detalhado
6. GOVERNANCE - Framework de controle e monitoramento

FRAMEWORKS UTILIZADOS:
🎯 Porter's Five Forces
📊 BCG Growth-Share Matrix  
🔄 Ansoff Matrix
🌊 Blue Ocean Strategy
📈 Financial Modeling
⚖️ Risk Assessment Matrix

DELIVERABLES EXECUTIVOS:
📋 Executive Summary
📊 Market Analysis
💰 Financial Projections
⚠️ Risk Matrix
🗺️ Implementation Roadmap
📈 Success Metrics

Seja analítico, baseado em dados e estrategicamente orientado!")
    {
    }

    [FunctionCall("Análise de mercado internacional especializada")]
    [FunctionCallParameter("markets", "Mercados-alvo para análise")]
    [FunctionCallParameter("sector", "Setor específico para análise")]
    [FunctionCallParameter("timeframe", "Horizonte temporal da análise")]
    private string AnaliseMarketIntelligence(string markets, string sector, string timeframe)
    {
      var random = new Random();
      var marketSizes = markets.Split(',').Select(m => $"{m.Trim()}: USD {random.Next(50, 500)}B").ToArray();

      return $@"
🌎 MARKET INTELLIGENCE: {sector.ToUpper()}
═══════════════════════════════════════

📊 MARKET SIZING ({timeframe}):
{string.Join("\n", marketSizes.Select(m => $"• {m}"))}

🎯 PENETRAÇÃO FINTECH:
• Argentina: {random.Next(25, 45)}% (mature market)
• Colômbia: {random.Next(15, 35)}% (growing rapidly)
• Chile: {random.Next(30, 50)}% (high adoption)
• México: {random.Next(20, 40)}% (massive potential)

🏆 COMPETITIVE LANDSCAPE:
• Neobanks dominando: Nubank, Rappi, Ualá
• Fintechs locais emergindo rapidamente
• Bancos tradicionais se digitalizando
• Regulamentação favorável crescente

💡 OPPORTUNITIES IDENTIFIED:
• Cross-border payments: USD 50B+ opportunity
• Investment products: Underserved segment
• SME lending: High demand, low supply
• Digital wallets: Growing 40%+ YoY

⚠️ BARRIERS TO ENTRY:
• Regulatory complexity varies by country
• Local partnership requirements
• Customer acquisition costs rising
• Currency volatility risks";
    }

    [FunctionCall("Modelagem financeira avançada para expansão")]
    [FunctionCallParameter("investment", "Valor do investimento disponível")]
    [FunctionCallParameter("markets", "Mercados para modelagem")]
    [FunctionCallParameter("scenarios", "Cenários para análise (otimista, realista, pessimista)")]
    private string ModelagemFinanceiraAvancada(string investment, string markets, string scenarios)
    {
      var random = new Random();
      var totalInvestment = decimal.Parse(investment.Replace("R$", "").Replace(".", "").Replace(",", "").Trim()) / 1_000_000m;

      return $@"
💰 MODELAGEM FINANCEIRA: EXPANSÃO LatAm
═══════════════════════════════════════

📊 INVESTMENT ALLOCATION:
• Capex total: R$ {totalInvestment:N1}M
• Argentina: {random.Next(15, 30)}%
• Colômbia: {random.Next(20, 35)}%
• Chile: {random.Next(15, 25)}%
• México: {random.Next(25, 40)}%
• Reserva estratégica: 10%

📈 CENÁRIO REALISTA:
• Break-even: {random.Next(18, 30)} meses
• ROI 3 anos: {random.Next(25, 45)}%
• IRR: {random.Next(15, 25)}%
• EBITDA Y3: R$ {random.Next(15, 35)}M

🚀 CENÁRIO OTIMISTA:
• Break-even: {random.Next(12, 18)} meses
• ROI 3 anos: {random.Next(45, 75)}%
• IRR: {random.Next(25, 40)}%
• EBITDA Y3: R$ {random.Next(35, 70)}M

⚠️ CENÁRIO PESSIMISTA:
• Break-even: {random.Next(30, 48)} meses
• ROI 3 anos: {random.Next(5, 15)}%
• IRR: {random.Next(5, 15)}%
• EBITDA Y3: R$ {random.Next(5, 15)}M

💡 KEY FINANCIAL DRIVERS:
• Customer acquisition cost: Principal variável
• Regulatory compliance costs: Subestimados
• Revenue per user: Benchmark Brasil + 15%
• Churn rate: Estimado em 3-5% monthly";
    }

    [FunctionCall("Análise de riscos estratégicos e mitigação")]
    [FunctionCallParameter("riskLevel", "Nível de risco aceitável")]
    [FunctionCallParameter("markets", "Mercados para análise de risco")]
    private string AnaliseRiscosEstrategicos(string riskLevel, string markets)
    {
      return $@"
⚠️ MATRIZ DE RISCOS: EXPANSÃO LatAm
═══════════════════════════════════

🔴 RISCOS ALTOS:
• Volatilidade cambial (Argentina, impact: alto)
• Mudanças regulatórias (México, impact: alto)
• Competição agressiva (Chile, impact: médio-alto)
• Customer acquisition cost elevado (todos)

🟠 RISCOS MÉDIOS:
• Integração de equipes remotas
• Adaptação cultural do produto
• Prazos regulatórios estendidos
• Escassez de talentos locais

🟡 RISCOS BAIXOS:
• Aceitação de marca brasileira
• Infraestrutura tecnológica
• Parcerias estratégicas locais
• Escalabilidade da plataforma

📋 ESTRATÉGIAS DE MITIGAÇÃO:
• Hedge cambial estruturado
• Equipes regulatórias dedicadas por país
• Parcerias estratégicas locais
• Go-to-market sequencial (não simultâneo)
• MVP adaptado a cada mercado
• Buffer financeiro de 15% por mercado

⚖️ RISCO RESIDUAL ESTIMADO:
Alinhado com apetite '{riskLevel}' do cliente";
    }

    [FunctionCall("Roadmap de implementação estratégica")]
    [FunctionCallParameter("timeframe", "Horizonte temporal total")]
    [FunctionCallParameter("firstMarket", "Primeiro mercado para entrada")]
    private string RoadmapImplementacao(string timeframe, string firstMarket)
    {
      return $@"
🗺️ ROADMAP DE IMPLEMENTAÇÃO: {timeframe}
════════════════════════════════════

📅 FASE 1: PREPARAÇÃO (Meses 1-3)
• Due diligence regulatória em todos mercados
• Estruturação legal e fiscal
• Definição de MVPs por mercado
• Recrutamento lideranças locais
• Parcerias estratégicas iniciais

📅 FASE 2: ENTRADA {firstMarket.ToUpper()} (Meses 4-6)
• Lançamento MVP adaptado
• Equipe local inicial (15-20 pessoas)
• Primeiras campanhas de aquisição
• Ajustes produto baseados em feedback
• Processos regulatórios demais mercados

📅 FASE 3: EXPANSÃO REGIONAL (Meses 7-18)
• Lançamento sequencial demais mercados
• Escala em {firstMarket} (50+ pessoas)
• Cross-selling novos produtos
• Hub regional de operações
• Otimização CAC e LTV

📅 FASE 4: CONSOLIDAÇÃO (Meses 19-24)
• Integração completa de operações
• Plataforma regional unificada
• Eficiências operacionais
• Novos verticais de produtos
• Preparação para Série C

🚩 MILESTONES CRÍTICOS:
• {firstMarket} 10k users: Mês 6
• Segundo mercado entry: Mês 9
• Break-even primeiro mercado: Mês 18
• 100k usuários regional: Mês 24";
    }
  }
}
