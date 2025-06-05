// NOTA: Todas as classes deste arquivo foram movidas para seus próprios arquivos nas pastas correspondentes.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
    /// <summary>
    /// Exemplos Avançados - Demonstra funcionalidades únicas do Agents.net
    /// Casos de uso complexos com ToolPacks personalizados, Function Calling avançado,
    /// Fluent Interface sofisticado e workflows entre agentes
    /// </summary>
    public static class ExemplosAvancados
    {
        /// <summary>
        /// Agente de Research & Development com ToolPacks personalizados integrados
        /// Demonstra function calling avançado, contexto dinâmico e múltiplos ToolPacks
        /// </summary>
        public static async Task ExecutarAgentePesquisaAvancado(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🧪 EXEMPLO 7: AGENTE R&D AVANÇADO - ADVANCED TOOLPACKS & FUNCTION CALLING");
            Console.WriteLine("═════════════════════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("🔬 Demonstra ToolPacks integrados, function calling na classe e contexto dinâmico");
            Console.WriteLine("🚀 Agente com capacidades de pesquisa, análise e síntese automática\n");

            var contextoComplexo = new ContextoPesquisaComplexo
            {
                AreaPesquisa = "Large Language Models",
                NivelProfundidade = "PhD",
                TipoEntrega = "Paper Acadêmico",
                TempoDisponivel = TimeSpan.FromHours(2),
                RecursosDisponiveis = new[] { "ArXiv", "Google Scholar", "IEEE", "ACM" },
                RestricoesBusca = new[] { "Português brasileiro", "2023-2024" },
                MetricasQualidade = new QualityMetrics
                {
                    CitacoesMinimas = 50,
                    FatorImpactoMinimo = 2.5,
                    RelevanciaContextual = 0.9
                }
            };

            // Agente com múltiplos ToolPacks e auto-configuração baseada no contexto
            var agentePesquisador = new AgentePesquisaAvancado(modelo)
                .WithContext(contextoComplexo)
                .WithPersona(ctx => $@"
🧠 Você é um pesquisador PhD sênior especializado em {ctx.AreaPesquisa}! 

PERFIL DINÂMICO (baseado no contexto):
📊 Nível: {ctx.NivelProfundidade}
🎯 Entrega: {ctx.TipoEntrega}
⏰ Tempo disponível: {ctx.TempoDisponivel.TotalHours}h
🔍 Recursos: {string.Join(", ", ctx.RecursosDisponiveis)}

METODOLOGIA PERSONALIZADA:
1. ANÁLISE CONTEXTUAL - Adapte sua abordagem ao nível {ctx.NivelProfundidade}
2. BUSCA ESTRATÉGICA - Use recursos disponíveis: {string.Join(", ", ctx.RecursosDisponiveis)}
3. SÍNTESE INTELIGENTE - Foque em {ctx.TipoEntrega}
4. VALIDAÇÃO RIGOROSA - Aplique métricas de qualidade definidas

RESTRIÇÕES ATIVAS:
{string.Join("\n", ctx.RestricoesBusca.Select(r => $"• {r}"))}

Seja meticuloso, científico e adapte seu estilo ao contexto fornecido!")
                .WithInstructions(ctx => $@"
Conduza pesquisa {ctx.NivelProfundidade} em {ctx.AreaPesquisa} para produzir {ctx.TipoEntrega}.
Tempo disponível: {ctx.TempoDisponivel.TotalHours}h.
Use todas as ferramentas disponíveis de forma inteligente.")
                .WithGuardRails("Sempre cite fontes acadêmicas. Nunca invente dados ou estatísticas.")
                .WithTools(new ReasoningToolPack())
                .WithTools(new SearchToolPack());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Pesquisa: 'Estado da arte em LLMs para português brasileiro - análise abrangente'");
            Console.ResetColor();
            Console.WriteLine("\n🧪 Pesquisa R&D Avançada:");
            Console.WriteLine(new string('-', 60));

            try
            {
                var resultado = await agentePesquisador.ExecuteAsync(
                    @"Conduza uma pesquisa abrangente sobre Large Language Models para português brasileiro. 
                    Inclua: metodologias SOTA, principais modelos (BERTimbau, GPT-4, Claude), benchmarks, 
                    limitações específicas do português, datasets disponíveis e direções de pesquisa. 
                    Sintetize em formato de revisão sistemática para submissão acadêmica."
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Function calls executados: {resultado.Tools.Count}");

                if (resultado.Tools.Count > 0)
                {
                    Console.WriteLine("\n🛠️ Ferramentas utilizadas:");
                    foreach (var tool in resultado.Tools)
                    {
                        Console.WriteLine($"   🔹 {tool.Name} - {tool.Result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Consultor estratégico com análise multimodal e ToolPacks empresariais
        /// Demonstra fluent interface sofisticado e workflow entre agentes
        /// </summary>
        public static async Task ExecutarConsultorEstrategicoAvancado(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💼 EXEMPLO 8: CONSULTOR ESTRATÉGICO MULTIMODAL - ADVANCED BUSINESS WORKFLOW");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("🏢 Demonstra workflow empresarial complexo com múltiplos agentes especializados");
            Console.WriteLine("📊 Análise estratégica, financeira e operacional integrada\n");

            var contextoEmpresarial = new ContextoEmpresarialComplexo
            {
                EmpresaInfo = new EmpresaInfo
                {
                    Nome = "TechMinas Innovation",
                    Setor = "Fintech",
                    Porte = "Scale-up",
                    Faturamento = 85_000_000,
                    Funcionarios = 350,
                    Localizacao = "Belo Horizonte, MG",
                    AnosOperacao = 7
                },
                CenarioAnalise = new CenarioAnalise
                {
                    TipoAnalise = "Expansão Internacional",
                    MercadosAlvo = new[] { "Argentina", "Colômbia", "Chile", "México" },
                    HorizonteTemporal = 24, // meses
                    InvestimentoDisponivel = 25_000_000,
                    RiscoAceitavel = "Médio-Alto"
                },
                RequisitosDiligencia = new DiligenceRequirements
                {
                    AnaliseMercado = true,
                    AvaliacaoFinanceira = true,
                    EstudoViabilidade = true,
                    PlanejamentoOperacional = true,
                    GestaoRiscos = true,
                    ComplianceRegulatorio = true
                }
            };

            // Consultor principal com personalidade e instruções baseadas no contexto complexo
            var consultorPrincipal = new ConsultorEstrategicoMultimodal(modelo)
                .WithContext(contextoEmpresarial)
                .WithPersona(ctx => $@"
💼 Você é um consultor estratégico sênior da McKinsey com 20+ anos de experiência!

PERFIL DO CLIENTE:
🏢 Empresa: {ctx.EmpresaInfo.Nome} ({ctx.EmpresaInfo.Setor})
📊 Porte: {ctx.EmpresaInfo.Porte} - R$ {ctx.EmpresaInfo.Faturamento:N0}
👥 Time: {ctx.EmpresaInfo.Funcionarios} funcionários
🌍 Objetivo: {ctx.CenarioAnalise.TipoAnalise}
💰 Budget: R$ {ctx.CenarioAnalise.InvestimentoDisponivel:N0}

MERCADOS-ALVO: {string.Join(", ", ctx.CenarioAnalise.MercadosAlvo)}
HORIZONTE: {ctx.CenarioAnalise.HorizonteTemporal} meses
RISCO: {ctx.CenarioAnalise.RiscoAceitavel}

METODOLOGIA EXECUTIVA:
1. DIAGNÓSTICO 360° - Análise situacional completa
2. MARKET INTELLIGENCE - Pesquisa de mercados-alvo
3. FINANCIAL MODELING - Projeções e cenários
4. RISK ASSESSMENT - Matriz de riscos detalhada
5. STRATEGIC ROADMAP - Plano de execução
6. GOVERNANCE FRAMEWORK - Estrutura de controle

Use frameworks: Porter 5 Forces, BCG Matrix, Ansoff, Blue Ocean!")
                .WithInstructions(ctx => $@"
Conduza análise estratégica completa para {ctx.CenarioAnalise.TipoAnalise}.
Mercados: {string.Join(", ", ctx.CenarioAnalise.MercadosAlvo)}.
Budget: R$ {ctx.CenarioAnalise.InvestimentoDisponivel:N0}.
Prazo: {ctx.CenarioAnalise.HorizonteTemporal} meses.
Use todas as ferramentas para análise abrangente.")
                .WithGuardRails(@"
Sempre base recomendações em dados. Identifique claramente premissas e riscos.
Nunca prometa resultados garantidos. Mantenha ética profissional.")
                .WithTools(new ReasoningToolPack())
                .WithTools(new FinanceToolPack())
                .WithTools(new SearchToolPack());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Cenário: Expansão internacional da TechMinas para América Latina");
            Console.ResetColor();
            Console.WriteLine("\n💼 Análise Estratégica Multimodal:");
            Console.WriteLine(new string('-', 60));

            try
            {
                var resultado = await consultorPrincipal.ExecuteAsync(
                    @"A TechMinas Innovation (fintech brasileira, R$ 85M faturamento, 350 funcionários) 
                    planeja expansão para Argentina, Colômbia, Chile e México. Budget de R$ 25M, 
                    horizonte de 24 meses. Conduza análise estratégica completa incluindo:
                    1. Market sizing e oportunidades
                    2. Análise competitiva por país
                    3. Modelo financeiro com projeções
                    4. Matriz de riscos e mitigação
                    5. Roadmap de implementação
                    6. Estrutura organizacional requerida"
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Function calls executados: {resultado.Tools.Count}");

                // Demonstrar workflow entre agentes especializados
                Console.WriteLine("\n🔄 Iniciando workflow com agentes especializados...\n");
                await ExecutarWorkflowAgentesEspecializados(modelo, contextoEmpresarial);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Demonstra um workflow complexo entre múltiplos agentes especializados
        /// </summary>
        private static async Task ExecutarWorkflowAgentesEspecializados(IModel modelo, ContextoEmpresarialComplexo contexto)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔄 WORKFLOW: AGENTES ESPECIALIZADOS EM PARALELO");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.ResetColor();

            // Agente Analista de Mercado
            var analistaMercado = new AnalistaMercadoLatam(modelo)
                .WithContext(contexto)
                .WithPersona(ctx => $@"
📊 Especialista em mercados da América Latina com foco em Fintech!
Mercados-alvo: {string.Join(", ", ctx.CenarioAnalise.MercadosAlvo)}
Sua expertise: sizing, penetração, regulamentação, concorrência")
                .WithInstructions("Foque em análise quantitativa de mercado e oportunidades")
                .WithTools(new SearchToolPack());

            // Agente Analista Financeiro
            var analistaFinanceiro = new AnalistaFinanceiroEstrategico(modelo)
                .WithContext(contexto)
                .WithPersona(ctx => $@"
💰 CFO especialista em expansão internacional e modelagem financeira!
Budget: R$ {ctx.CenarioAnalise.InvestimentoDisponivel:N0}
Foco: ROI, CAPEX/OPEX, break-even, cenários de stress")
                .WithInstructions("Construa modelos financeiros robustos com cenários múltiplos")
                .WithTools(new FinanceToolPack());

            // Agente Especialista em Riscos
            var especialistaRiscos = new EspecialistaRiscosInternacionais(modelo)
                .WithContext(contexto)
                .WithPersona(ctx => $@"
⚠️ Chief Risk Officer com experiência em expansão LatAm!
Risco aceitável: {ctx.CenarioAnalise.RiscoAceitavel}
Expertise: regulatório, cambial, operacional, reputacional")
                .WithInstructions("Identifique e quantifique todos os riscos potenciais")
                .WithTools(new ReasoningToolPack());

            try
            {
                // Executar análises em paralelo (simulado)
                Console.WriteLine("🔍 Analista de Mercado executando...");
                var resultadoMercado = await analistaMercado.ExecuteAsync(
                    "Analise o mercado fintech nos países-alvo: sizing, penetração, concorrência, barreiras de entrada"
                );

                Console.WriteLine("💰 Analista Financeiro executando...");
                var resultadoFinanceiro = await analistaFinanceiro.ExecuteAsync(
                    "Construa modelo financeiro para expansão: investimento inicial, projeções de receita, break-even por país"
                );

                Console.WriteLine("⚠️ Especialista em Riscos executando...");
                var resultadoRiscos = await especialistaRiscos.ExecuteAsync(
                    "Mapeie matriz de riscos: regulatório, cambial, operacional. Propose estratégias de mitigação"
                );

                // Compilar resultados
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ WORKFLOW CONCLUÍDO - COMPILAÇÃO DOS RESULTADOS:");
                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.ResetColor();

                Console.WriteLine("\n📊 ANÁLISE DE MERCADO:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine(resultadoMercado.Data.Substring(0, Math.Min(300, resultadoMercado.Data.Length)) + "...");

                Console.WriteLine("\n💰 MODELAGEM FINANCEIRA:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine(resultadoFinanceiro.Data.Substring(0, Math.Min(300, resultadoFinanceiro.Data.Length)) + "...");

                Console.WriteLine("\n⚠️ ANÁLISE DE RISCOS:");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine(resultadoRiscos.Data.Substring(0, Math.Min(300, resultadoRiscos.Data.Length)) + "...");

                var totalTokens = resultadoMercado.Usage.TotalTokens +
                                resultadoFinanceiro.Usage.TotalTokens +
                                resultadoRiscos.Usage.TotalTokens;
                var totalFunctionCalls = resultadoMercado.Tools.Count +
                                       resultadoFinanceiro.Tools.Count +
                                       resultadoRiscos.Tools.Count;

                Console.WriteLine($"\n📊 MÉTRICAS DO WORKFLOW:");
                Console.WriteLine($"🔢 Total de tokens: {totalTokens:N0}");
                Console.WriteLine($"⚙️ Total de function calls: {totalFunctionCalls}");
                Console.WriteLine($"👥 Agentes especializados: 3");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro no workflow: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Agente de Creative Tech com ToolPacks de design e prototipagem
        /// Demonstra uso avançado de contexto visual e criativo
        /// </summary>
        public static async Task ExecutarCreativeTechAgent(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎨 EXEMPLO 9: CREATIVE TECH AGENT - DESIGN & PROTOTYPING");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("🚀 Demonstra agente criativo com ToolPacks de design, prototipagem e UX");
            Console.WriteLine("💡 Contexto adaptativo e personalidade baseada em brief criativo\n");

            var contextoCriativo = new ContextoCreativeTech
            {
                BriefProjeto = new CreativeBrief
                {
                    Cliente = "Startup FinTech BH",
                    Produto = "App de Investimentos para Jovens",
                    Publico = "18-25 anos, universitários",
                    Vibe = "Modern, trustworthy, gamified",
                    Plataformas = new[] { "iOS", "Android", "Web" },
                    Timeline = TimeSpan.FromDays(21),
                    Budget = "R$ 150.000"
                },
                EstiloPreferencias = new DesignPreferences
                {
                    ColorPalette = "Tech blues, vibrant accents",
                    Typography = "Sans-serif moderna",
                    Layout = "Minimalista com micro-interactions",
                    Inspiration = new[] { "Revolut", "Nubank", "Inter" }
                },
                TechStack = new[] { "React Native", "TypeScript", "Node.js", "PostgreSQL" },
                Deliverables = new[] { "User Journey", "Wireframes", "Protótipo", "Design System" }
            };

            var creativeTechAgent = new CreativeTechSpecialist(modelo)
                .WithContext(contextoCriativo)
                .WithPersona(ctx => $@"
🎨 Você é um Creative Technologist premiado com expertise em UX/UI e desenvolvimento!

BRIEF DO PROJETO:
🏢 Cliente: {ctx.BriefProjeto.Cliente}
📱 Produto: {ctx.BriefProjeto.Produto}
👥 Público: {ctx.BriefProjeto.Publico}
✨ Vibe: {ctx.BriefProjeto.Vibe}
📅 Timeline: {ctx.BriefProjeto.Timeline.Days} dias
💰 Budget: {ctx.BriefProjeto.Budget}

STACK TÉCNICO: {string.Join(", ", ctx.TechStack)}
INSPIRAÇÕES: {string.Join(", ", ctx.EstiloPreferencias.Inspiration)}

METODOLOGIA CRIATIVA:
1. RESEARCH & INSIGHTS - Entenda profundamente o usuário
2. CONCEITUAÇÃO - Desenvolva conceitos inovadores
3. PROTOTIPAGEM RÁPIDA - Teste ideias rapidamente
4. DESIGN SYSTEM - Crie linguagem visual consistente
5. TECHNICAL FEASIBILITY - Valide viabilidade técnica
6. USER TESTING - Teste com usuários reais

Seja criativo, técnico e sempre focado na experiência do usuário!")
                .WithInstructions(ctx => $@"
Desenvolva {string.Join(", ", ctx.Deliverables)} para {ctx.BriefProjeto.Produto}.
Público: {ctx.BriefProjeto.Publico}. Timeline: {ctx.BriefProjeto.Timeline.Days} dias.
Use stack: {string.Join(", ", ctx.TechStack)}.")
                .WithGuardRails("Sempre considere acessibilidade e usabilidade. Valide viabilidade técnica.")
                .WithTools(new ReasoningToolPack())
                .WithTools(new SearchToolPack());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Projeto: App de investimentos gamificado para universitários");
            Console.ResetColor();
            Console.WriteLine("\n🎨 Creative Tech em Ação:");
            Console.WriteLine(new string('-', 60));

            try
            {
                var resultado = await creativeTechAgent.ExecuteAsync(
                    @"Desenvolva um conceito completo para app de investimentos direcionado a universitários (18-25 anos). 
                    O app deve ser educativo, gamificado e acessível para iniciantes. Inclua:
                    1. User research insights
                    2. User journey mapping
                    3. Information architecture
                    4. Key features e gamification
                    5. Wireframes principais
                    6. Design system básico
                    7. Prototipo interativo conceitual
                    8. Roadmap técnico de implementação"
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Creative tools utilizados: {resultado.Tools.Count}");

                if (resultado.Tools.Count > 0)
                {
                    Console.WriteLine("\n🛠️ Ferramentas criativas utilizadas:");
                    foreach (var tool in resultado.Tools)
                    {
                        Console.WriteLine($"   🎨 {tool.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    // ==================== CONTEXTOS COMPLEXOS ====================

    // Todas as classes foram movidas para seus próprios arquivos:
    // - Contexts/ContextoPesquisaComplexo.cs
    // - Contexts/QualityMetrics.cs
    // - Contexts/ContextoEmpresarialComplexo.cs
    // - Contexts/EmpresaInfo.cs
    // - Contexts/CenarioAnalise.cs
    // - Contexts/DiligenceRequirements.cs
    // - Contexts/ContextoCreativeTech.cs
    // - Contexts/CreativeBrief.cs
    // - Contexts/DesignPreferences.cs
    // - Agents/AgentePesquisaAvancado.cs
    // - Agents/ConsultorEstrategicoMultimodal.cs
    // - Agents/AnalistaMercadoLatam.cs
    // - Agents/AnalistaFinanceiroEstrategico.cs
    // - Agents/EspecialistaRiscosInternacionais.cs
    // - Agents/CreativeTechSpecialist.cs
}