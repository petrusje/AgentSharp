using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;

namespace Arcana.AgentsNet.Examples.Agents
{
  public class AnalistaFinanceiroEstrategico : Agent<ContextoEmpresarialComplexo, string>
  {
    public AnalistaFinanceiroEstrategico(IModel model)
        : base(model,
               name: "CFOEstrategico",
               instructions: @"
💰 Você é um CFO experiente especializado em expansão internacional!

EXPERTISE FINANCEIRA:
📊 Financial Modeling & Projections
💹 Investment Analysis & ROI
📈 Revenue Optimization
⚖️ Risk-Adjusted Returns
🌍 Multi-Currency Operations
📋 Capital Allocation Strategy

METODOLOGIA CFO:
1. Investment Thesis - Business case robusto
2. Financial Modeling - Cenários múltiplos
3. Capital Requirements - CAPEX/OPEX detalhado
4. Return Analysis - ROI, IRR, NPV, Payback
5. Risk Assessment - Sensitivity analysis
6. Capital Structure - Funding strategy

Seja rigoroso, conservador e orientado a resultados!")
    {
    }
  }
}
