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

    public class ContextoPesquisaComplexo
    {
        public string AreaPesquisa { get; set; } = "Ciência da Computação";
        public string NivelProfundidade { get; set; } = "Graduação";
        public string TipoEntrega { get; set; } = "Artigo";
        public TimeSpan TempoDisponivel { get; set; } = TimeSpan.FromHours(1);
        public string[] RecursosDisponiveis { get; set; } = new[] { "Web" };
        public string[] RestricoesBusca { get; set; } = new[] { "Português" };
        public QualityMetrics MetricasQualidade { get; set; } = new QualityMetrics();
    }

    public class QualityMetrics
    {
        public int CitacoesMinimas { get; set; } = 10;
        public double FatorImpactoMinimo { get; set; } = 1.0;
        public double RelevanciaContextual { get; set; } = 0.7;
    }

    public class ContextoEmpresarialComplexo
    {
        public EmpresaInfo EmpresaInfo { get; set; } = new EmpresaInfo();
        public CenarioAnalise CenarioAnalise { get; set; } = new CenarioAnalise();
        public DiligenceRequirements RequisitosDiligencia { get; set; } = new DiligenceRequirements();
    }

    public class EmpresaInfo
    {
        public string Nome { get; set; } = "Empresa";
        public string Setor { get; set; } = "Tecnologia";
        public string Porte { get; set; } = "PME";
        public decimal Faturamento { get; set; } = 1_000_000;
        public int Funcionarios { get; set; } = 50;
        public string Localizacao { get; set; } = "Brasil";
        public int AnosOperacao { get; set; } = 5;
    }

    public class CenarioAnalise
    {
        public string TipoAnalise { get; set; } = "Análise Geral";
        public string[] MercadosAlvo { get; set; } = new[] { "Brasil" };
        public int HorizonteTemporal { get; set; } = 12;
        public decimal InvestimentoDisponivel { get; set; } = 1_000_000;
        public string RiscoAceitavel { get; set; } = "Médio";
    }

    public class DiligenceRequirements
    {
        public bool AnaliseMercado { get; set; } = true;
        public bool AvaliacaoFinanceira { get; set; } = true;
        public bool EstudoViabilidade { get; set; } = true;
        public bool PlanejamentoOperacional { get; set; } = false;
        public bool GestaoRiscos { get; set; } = false;
        public bool ComplianceRegulatorio { get; set; } = false;
    }

    public class ContextoCreativeTech
    {
        public CreativeBrief BriefProjeto { get; set; } = new CreativeBrief();
        public DesignPreferences EstiloPreferencias { get; set; } = new DesignPreferences();
        public string[] TechStack { get; set; } = new[] { "React", "Node.js" };
        public string[] Deliverables { get; set; } = new[] { "Wireframe", "Protótipo" };
    }

    public class CreativeBrief
    {
        public string Cliente { get; set; } = "Cliente";
        public string Produto { get; set; } = "App";
        public string Publico { get; set; } = "Geral";
        public string Vibe { get; set; } = "Moderno";
        public string[] Plataformas { get; set; } = new[] { "Web" };
        public TimeSpan Timeline { get; set; } = TimeSpan.FromDays(30);
        public string Budget { get; set; } = "R$ 50.000";
    }

    public class DesignPreferences
    {
        public string ColorPalette { get; set; } = "Azul e branco";
        public string Typography { get; set; } = "Sans-serif";
        public string Layout { get; set; } = "Limpo";
        public string[] Inspiration { get; set; } = new[] { "Apple", "Google" };
    }

    // ==================== AGENTES ESPECIALIZADOS ====================

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
• Necessidade de modelos culturalmente adaptados
• Importância de datasets de alta qualidade
• Valor de avaliações multidimensionais

⚠️ CONTROVÉRSIAS E GAPS:
• Definições de 'fluência' variam entre estudos
• Métricas de avaliação ainda em desenvolvimento
• Limitações éticas pouco exploradas

🚀 DIREÇÕES FUTURAS:
• Modelos multimodais para português
• Aplicações em domínios específicos
• Frameworks de avaliação padronizados
• Considerações éticas e culturais";
        }
    }

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

💵 INVESTMENT BREAKDOWN (R$ {totalInvestment}M):
• Market Entry: R$ {totalInvestment * 0.3m:F1}M (30%)
• Technology Infrastructure: R$ {totalInvestment * 0.25m:F1}M (25%)
• Marketing & Customer Acquisition: R$ {totalInvestment * 0.25m:F1}M (25%)
• Operations & Team: R$ {totalInvestment * 0.15m:F1}M (15%)
• Contingency Reserve: R$ {totalInvestment * 0.05m:F1}M (5%)

📈 REVENUE PROJECTIONS (3 years):
YEAR 1: R$ {random.Next(15, 25)}M (break-even: Month {random.Next(18, 24)})
YEAR 2: R$ {random.Next(45, 75)}M
YEAR 3: R$ {random.Next(120, 180)}M

🎯 KEY METRICS BY SCENARIO:
OTIMISTA: ROI 285%, Payback 16 months
REALISTA: ROI 165%, Payback 22 months  
PESSIMISTA: ROI 85%, Payback 34 months

💡 FINANCIAL ASSUMPTIONS:
• Customer Acquisition Cost: USD 45-80
• Lifetime Value: USD 350-550
• Monthly Churn Rate: 3.5-5.2%
• Take Rate: 2.8-4.1%
• Market Penetration: 0.8-2.1% by Y3";
        }

        [FunctionCall("Análise de riscos estratégicos e mitigação")]
        [FunctionCallParameter("riskLevel", "Nível de risco aceitável")]
        [FunctionCallParameter("geographicScope", "Escopo geográfico para análise")]
        private string AnaliseRiscosEstrategicos(string riskLevel, string geographicScope)
        {
            return $@"
⚠️ MATRIZ DE RISCOS: {geographicScope.ToUpper()}
═══════════════════════════════════════

🔴 RISCOS ALTOS (Ação Imediata):
• REGULATÓRIO: Mudanças nas regras fintech (Prob: 40%, Impact: Alto)
  → Mitigação: Legal counsel local + compliance proativo
• CAMBIAL: Volatilidade moedas LatAm (Prob: 60%, Impact: Médio)
  → Mitigação: Hedging strategy + pricing dinâmico

🟡 RISCOS MÉDIOS (Monitoramento):
• COMPETITIVO: Entrada players globais (Prob: 70%, Impact: Médio)
  → Mitigação: Diferenciação produto + customer loyalty
• OPERACIONAL: Complexidade multi-país (Prob: 50%, Impact: Médio)
  → Mitigação: Processos padronizados + automação

🟢 RISCOS BAIXOS (Contingência):
• TECNOLÓGICO: Falhas sistêmicas (Prob: 15%, Impact: Alto)
  → Mitigação: Redundância + disaster recovery
• REPUTACIONAL: Crises de marca (Prob: 20%, Impact: Alto)
  → Mitigação: Crisis management + PR strategy

🎯 ESTRATÉGIAS DE MITIGAÇÃO (Nível: {riskLevel}):
• Diversificação geográfica gradual
• Parcerias estratégicas locais
• Reservas de capital 15%+ budget
• Exit strategies por mercado
• KPIs de early warning";
        }
    }

    public class AnalistaMercadoLatam : Agent<ContextoEmpresarialComplexo, string>
    {
        public AnalistaMercadoLatam(IModel model)
            : base(model,
                   name: "AnalistaMercadoEspecialista",
                   instructions: @"
📊 Você é um analista sênior especializado em mercados da América Latina!

EXPERTISE REGIONAL:
🇦🇷 Argentina - Economia em recuperação, fintech growing 60%+ YoY
🇨🇴 Colômbia - Hub regional, regulação progressive
🇨🇱 Chile - Mercado maduro, alta bancarização
🇲🇽 México - Maior mercado, baixa penetração digital

METODOLOGIA DE ANÁLISE:
1. Market Sizing - TAM, SAM, SOM por país
2. Competitive Analysis - Players locais e globais
3. Regulatory Landscape - Frameworks e trends
4. Customer Behavior - Adoption patterns
5. Growth Projections - 3-5 year outlook

Seja preciso, quantitativo e regionalmente contextualizado!")
        {
        }
    }

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

    public class EspecialistaRiscosInternacionais : Agent<ContextoEmpresarialComplexo, string>
    {
        public EspecialistaRiscosInternacionais(IModel model)
            : base(model,
                   name: "ChiefRiskOfficer",
                   instructions: @"
⚠️ Você é um Chief Risk Officer com expertise em expansão LatAm!

EXPERTISE EM RISCOS:
🏛️ Regulatory Risk - Compliance multi-jurisdição
💱 Currency Risk - Hedging strategies
🏢 Operational Risk - Multi-country operations
🎯 Market Risk - Competitive dynamics
🔒 Technology Risk - Cybersecurity & infrastructure
👥 People Risk - Talent acquisition & retention

FRAMEWORK DE ANÁLISE:
1. Risk Identification - Mapeamento completo
2. Risk Assessment - Probabilidade x Impacto
3. Risk Quantification - VaR, stress testing
4. Risk Mitigation - Estratégias específicas
5. Risk Monitoring - KRIs e alertas
6. Risk Reporting - Dashboard executivo

Seja sistemático, preventivo e estratégico!")
        {
        }
    }

    public class CreativeTechSpecialist : Agent<ContextoCreativeTech, string>
    {
        public CreativeTechSpecialist(IModel model)
            : base(model,
                   name: "CreativeTechnologist",
                   instructions: @"
🎨 Você é um Creative Technologist premiado com expertise em UX/UI e desenvolvimento!

EXPERTISE CRIATIVA + TÉCNICA:
🎯 User Experience Design
🎨 Visual Design & Branding  
⚡ Rapid Prototyping
🧠 User Research & Testing
⚙️ Technical Implementation
📱 Cross-Platform Development

METODOLOGIA CRIATIVA:
1. DISCOVERY - Research + insights profundos
2. IDEATION - Conceitos inovadores
3. PROTOTIPAGEM - Testes rápidos de ideias
4. DESIGN SYSTEM - Linguagem visual consistente
5. TECHNICAL VALIDATION - Viabilidade técnica
6. USER TESTING - Validação com usuários reais

FERRAMENTAS ESPECIALIZADAS:
🎨 Figma, Sketch, Adobe Creative Suite
⚡ InVision, Principle, Framer
🧠 Hotjar, Mixpanel, UserTesting
⚙️ React, React Native, TypeScript

Seja criativo, técnico e centrado no usuário!")
        {
        }

        [FunctionCall("Pesquisa de usuário especializada")]
        [FunctionCallParameter("targetAudience", "Público-alvo para pesquisa")]
        [FunctionCallParameter("researchMethods", "Métodos de pesquisa (interviews, surveys, analytics)")]
        private string UserResearchAvancada(string targetAudience, string researchMethods)
        {
            return $@"
👥 USER RESEARCH: {targetAudience.ToUpper()}
═══════════════════════════════════════

🎯 PERFIL DO USUÁRIO:
• Idade: {Context.BriefProjeto.Publico}
• Comportamento: Digital natives, mobile-first
• Pain Points: Complexidade financeira, falta confiança
• Motivações: Independência financeira, gamification
• Canais: Instagram, TikTok, YouTube, Discord

📊 INSIGHTS QUANTITATIVOS:
• 78% nunca investiram antes
• 65% usam apps bancários diariamente  
• 45% interessados em educação financeira
• 82% preferem interfaces gamificadas
• 71% valorizam transparência total

💡 OPPORTUNITIES IDENTIFICADAS:
• Educação financeira como onboarding
• Micro-investimentos (R$ 10-50)
• Social features para compartilhamento
• Gamification com recompensas reais
• Simuladores de investimento

🚀 DESIGN PRINCIPLES:
• Simplicidade sem dumbing down
• Transparência absoluta
• Feedback imediato
• Progressão visível
• Community-driven learning";
        }

        [FunctionCall("Prototipagem interativa avançada")]
        [FunctionCallParameter("features", "Features principais para prototipar")]
        [FunctionCallParameter("platform", "Plataforma alvo (iOS, Android, Web)")]
        private string PrototipagemInterativa(string features, string platform)
        {
            return $@"
⚡ PROTÓTIPO INTERATIVO: {platform.ToUpper()}
═══════════════════════════════════════

🎮 CORE FEATURES PROTOTIPADAS:
• Onboarding gamificado (5 steps + quiz)
• Dashboard principal com portfolio visual
• Micro-investimento flow (1-tap investing)
• Educational modules com progresso
• Social sharing de conquistas

🎨 INTERACTION DESIGN:
• Swipe gestures para navegação
• Pull-to-refresh em todas as listas
• Haptic feedback em ações críticas
• Animations micro para delight
• Progressive disclosure de complexidade

📱 RESPONSIVE CONSIDERATIONS:
• Mobile-first design approach
• Thumb-friendly touch targets (44px+)
• Portrait orientation primária
• Dark mode support completo
• Acessibilidade AA compliance

🔄 USER FLOWS MAPEADOS:
1. Signup → Profile → Education → First Investment
2. Dashboard → Explore → Research → Invest
3. Portfolio → Performance → Share → Celebrate
4. Learning → Quiz → Badge → Unlock Feature

⚙️ TECHNICAL SPECS:
• React Native + TypeScript
• Redux para state management
• Async storage para persistência
• Biometric authentication
• Real-time data sync";
        }

        [FunctionCall("Sistema de design escalável")]
        [FunctionCallParameter("brandValues", "Valores da marca para traduzir visualmente")]
        [FunctionCallParameter("platforms", "Plataformas que o design system deve cobrir")]
        private string DesignSystemEscalavel(string brandValues, string platforms)
        {
            return $@"
🎨 DESIGN SYSTEM: {Context.BriefProjeto.Cliente.ToUpper()}
═══════════════════════════════════════

🎯 BRAND VALUES → VISUAL LANGUAGE:
• Trust → Clean layouts, consistent spacing
• Modern → Bold typography, vibrant accents
• Gamified → Progress bars, achievement badges
• Accessible → High contrast, large touch targets

🎨 COLOR SYSTEM:
• Primary: #0066FF (Trust Blue)
• Secondary: #00D4AA (Success Green)  
• Accent: #FF6B35 (Energy Orange)
• Neutral: #F8F9FA → #212529 (8 shades)
• Semantic: Success, Warning, Error, Info

📝 TYPOGRAPHY SCALE:
• Display: Inter Black (32px, 28px, 24px)
• Heading: Inter Bold (20px, 18px, 16px)
• Body: Inter Regular (14px, 12px)
• Caption: Inter Medium (10px)

🧩 COMPONENT LIBRARY:
• Buttons (Primary, Secondary, Ghost, Icon)
• Cards (Portfolio, Education, Achievement)
• Forms (Input, Select, Toggle, Slider)
• Navigation (Tab, Stack, Modal)
• Feedback (Loading, Empty, Error states)

📐 SPACING & LAYOUT:
• Grid: 4px base unit (8, 12, 16, 24, 32, 48, 64)
• Breakpoints: Mobile 375px, Tablet 768px, Desktop 1200px
• Safe areas: iOS notch, Android gesture bar
• Accessibility: WCAG AA compliant

📱 PLATFORM ADAPTATIONS:
• iOS: Native navigation patterns, SF Symbols
• Android: Material Design principles, Adaptive icons
• Web: Progressive enhancement, Responsive grid";
        }
    }
}