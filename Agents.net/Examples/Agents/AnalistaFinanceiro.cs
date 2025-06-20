using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;
using System;

namespace Arcana.AgentsNet.Examples.Agents
{
  public class AnalistaFinanceiro : Agent<AnaliseFinanceiraContext, string>
  {
    public AnalistaFinanceiro(IModel model)
        : base(model,
               name: "AnalistaFinanceiroEspecialista",
               instructions: @"
Você é um analista financeiro experiente especializado no mercado brasileiro! 📊💼

DIRETRIZES PARA ANÁLISES:
1. Comece com o preço atual da ação e variação do dia
2. Apresente recomendações de analistas e preços-alvo consensuais
3. Inclua métricas-chave: P/L, valor de mercado, faixa de 52 semanas
4. Analise padrões de negociação e tendências de volume
5. Compare performance contra índices setoriais relevantes

GUIA DE ESTILO:
- Use tabelas para apresentação estruturada de dados
- Inclua cabeçalhos claros para cada seção de dados
- Adicione explicações breves para termos técnicos
- Destaque mudanças notáveis com emojis (📈 📉)
- Use pontos para insights rápidos
- Compare valores atuais com médias históricas
- Termine com uma perspectiva financeira baseada em dados

ESTRUTURA RECOMENDADA:
📈 Preço e Performance
📊 Métricas Fundamentais  
🎯 Recomendações de Analistas
📉 Análise Técnica
🏭 Contexto Setorial
💡 Perspectivas")
    {
    }

    [FunctionCall("Obter dados financeiros atualizados de uma ação mineira")]
    [FunctionCallParameter("ticker", "Código da ação (ex: CMIG4, VALE3, USIM5)")]
    [FunctionCallParameter("periodo", "Período da análise (ex: 1M, 3M, 6M, 1A)")]
    private string ObterDadosFinanceiros(string ticker, string periodo = "3M")
    {
      // Simulação de dados financeiros - em produção, integraria com API real
      var preco = new Random().Next(15, 45) + Math.Round(new Random().NextDouble(), 2);
      var variacao = Math.Round((new Random().NextDouble() - 0.5) * 10, 2);
      var volume = new Random().Next(10000000, 100000000);

      return $@"
📊 DADOS FINANCEIROS - {ticker.ToUpper()}
═════════════════════════════════════

📈 COTAÇÃO ATUAL:
• Preço: R$ {preco:F2}
• Variação Dia: {(variacao >= 0 ? "+" : "")}{variacao:F2}% {(variacao >= 0 ? "📈" : "📉")}
• Volume: {volume:N0} ações
• Faixa 52 semanas: R$ {preco * 0.7:F2} - R$ {preco * 1.4:F2}

📊 MÉTRICAS FUNDAMENTAIS:
• P/L: {new Random().Next(8, 25):F1}x
• P/VP: {Math.Round(new Random().NextDouble() * 3 + 0.5, 1):F1}x
• Dividend Yield: {Math.Round(new Random().NextDouble() * 8 + 2, 1):F1}%
• Market Cap: R$ {new Random().Next(50, 300):N0} bilhões

🎯 CONSENSO ANALISTAS:
• Recomendação: {(variacao > 0 ? "COMPRA" : "NEUTRO")}
• Preço-alvo: R$ {preco * (1 + new Random().NextDouble() * 0.3):F2}
• Número de analistas: {new Random().Next(8, 25)}

📅 Período analisado: {periodo}
🕐 Última atualização: {DateTime.Now:HH:mm} - {DateTime.Now:dd/MM/yyyy}";
    }

    [FunctionCall("Analisar contexto macroeconômico de Minas Gerais")]
    [FunctionCallParameter("setor", "Setor específico para análise contextual")]
    private string AnalisarContextoMacro(string setor)
    {
      return $@"
⛰️ CONTEXTO MACROECONÔMICO MINAS GERAIS
══════════════════════════════════════

📊 INDICADORES REGIONAIS:
• PIB MG: R$ 780 bilhões (2° maior do Brasil)
• Crescimento: +2,8% (acima da média nacional)
• Setor mineração: 18% do PIB estadual
• Empregos: 10,8 milhões (setor formal)

🏭 SETOR {setor.ToUpper()}:
• Performance YTD: +{new Random().Next(5, 25)}%
• Perspectiva: {(new Random().Next(0, 2) == 0 ? "Positiva" : "Neutra")}
• Principais drivers: Mineração, agronegócio, indústria
• Riscos: Commodities, infraestrutura

💡 FATORES RELEVANTES MG:
• Forte tradição industrial
• Hub logístico estratégico
• Polo de inovação em BH
• Diversificação econômica

📈 OUTLOOK: Otimista para economia mineira
🕐 Análise válida para: {Context.PeriodoAnalise}";
    }
  }
}
