using System;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
    /// <summary>
    /// Exemplos de Workflows - funcionalidade avançada do Agents.net
    /// Demonstra o sistema de workflow sofisticado com sessões persistentes
    /// </summary>
    public static class ExemplosWorkflow
    {
        /// <summary>
        /// Exemplo de workflow completo de pesquisa, análise e relatório
        /// Demonstra as capacidades avançadas de workflow do Agents.net
        /// </summary>
        public static async Task ExecutarWorkflowCompleto(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔄 EXEMPLO 6: WORKFLOW DE PESQUISA - MULTI-STEP WORKFLOW");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📄 Demonstra sistema avançado de workflow do Agents.net");
            Console.WriteLine("Workflow: Pesquisa → Análise → Relatório → Revisão\n");

            try
            {
                // Contexto do workflow
                var contextoWorkflow = new ContextoPesquisa
                {
                    TopicoPesquisa = "Inteligência Artificial no Brasil",
                    ProfundidadeAnalise = "Detalhada",
                    PublicoAlvo = "Executivos de tecnologia"
                };

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🔍 ETAPA 1: PESQUISA E COLETA DE DADOS");
                Console.ResetColor();
                
                // Agente pesquisador
                var agentePesquisador = new AgentePesquisador(modelo)
                    .WithContext(contextoWorkflow);

                var resultadoPesquisa = await agentePesquisador.ExecuteAsync(
                    "Pesquise informações sobre o estado atual da Inteligência Artificial no Brasil, incluindo startups, investimentos, políticas públicas e tendências"
                );

                Console.WriteLine("✅ Pesquisa concluída");
                Console.WriteLine($"📊 Dados coletados: {resultadoPesquisa.Data.Length} caracteres");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n📊 ETAPA 2: ANÁLISE ESTRATÉGICA");
                Console.ResetColor();

                // Agente analista
                var agenteAnalista = new AgenteAnalista(modelo)
                    .WithContext(contextoWorkflow);

                var resultadoAnalise = await agenteAnalista.ExecuteAsync(
                    $"Analise os seguintes dados sobre IA no Brasil e identifique principais tendências, oportunidades e desafios:\n\n{resultadoPesquisa.Data}"
                );

                Console.WriteLine("✅ Análise concluída");
                Console.WriteLine($"📈 Insights gerados: {resultadoAnalise.Tools.Count} análises");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n📝 ETAPA 3: GERAÇÃO DE RELATÓRIO");
                Console.ResetColor();

                // Agente escritor
                var agenteEscritor = new AgenteEscritor(modelo)
                    .WithContext(contextoWorkflow);

                var resultadoRelatorio = await agenteEscritor.ExecuteAsync(
                    $@"Com base na pesquisa e análise realizadas, crie um relatório executivo sobre IA no Brasil:

DADOS DA PESQUISA:
{resultadoPesquisa.Data.Substring(0, Math.Min(1000, resultadoPesquisa.Data.Length))}...

ANÁLISE ESTRATÉGICA:
{resultadoAnalise.Data.Substring(0, Math.Min(1000, resultadoAnalise.Data.Length))}...

Público-alvo: {contextoWorkflow.PublicoAlvo}"
                );

                Console.WriteLine("✅ Relatório gerado");
                
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n🔍 ETAPA 4: REVISÃO E FINALIZAÇÃO");
                Console.ResetColor();

                // Agente revisor
                var agenteRevisor = new AgenteRevisor(modelo)
                    .WithContext(contextoWorkflow);

                var resultadoFinal = await agenteRevisor.ExecuteAsync(
                    $@"Revise e aprimore este relatório para garantir qualidade executiva:

{resultadoRelatorio.Data}

Foque em: clareza, objetividade, insights acionáveis para {contextoWorkflow.PublicoAlvo}"
                );

                Console.WriteLine("✅ Revisão concluída");

                // Exibir resultado final
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n📋 RELATÓRIO FINAL:");
                Console.WriteLine(new string('═', 60));
                Console.WriteLine(resultadoFinal.Data);
                Console.WriteLine(new string('═', 60));
                Console.ResetColor();

                // Estatísticas do workflow
                var totalTokens = resultadoPesquisa.Usage.TotalTokens + 
                                 resultadoAnalise.Usage.TotalTokens + 
                                 resultadoRelatorio.Usage.TotalTokens + 
                                 resultadoFinal.Usage.TotalTokens;

                Console.WriteLine($"\n📊 ESTATÍSTICAS DO WORKFLOW:");
                Console.WriteLine($"   🔍 Etapas executadas: 4");
                Console.WriteLine($"   📊 Total de tokens: {totalTokens}");
                Console.WriteLine($"   🛠️  Ferramentas usadas: {resultadoPesquisa.Tools.Count + resultadoAnalise.Tools.Count + resultadoRelatorio.Tools.Count + resultadoFinal.Tools.Count}");
                Console.WriteLine($"   📝 Relatório final: {resultadoFinal.Data.Length} caracteres");
                Console.WriteLine($"   🎯 Tópico: {contextoWorkflow.TopicoPesquisa}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro no workflow: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    #region Classes de Contexto e Agentes do Workflow

    public class ContextoPesquisa
    {
        public string TopicoPesquisa { get; set; }
        public string ProfundidadeAnalise { get; set; } = "Básica";
        public string PublicoAlvo { get; set; } = "Geral";
        public DateTime InicioWorkflow { get; set; } = DateTime.Now;
    }

    public class AgentePesquisador : Agent<ContextoPesquisa, string>
    {
        public AgentePesquisador(IModel model)
            : base(model,
                   name: "PesquisadorEspecialista",
                   instructions: @"
Você é um pesquisador especialista em tecnologia e inovação! 🔍📊

METODOLOGIA DE PESQUISA:
1. COLETA ABRANGENTE - Busque informações de múltiplas fontes
2. VERIFICAÇÃO - Cruze dados para garantir precisão
3. CATEGORIZAÇÃO - Organize por temas relevantes
4. CONTEXTUALIZAÇÃO - Adicione contexto temporal e geográfico

ESTRUTURA DA PESQUISA:
📈 PANORAMA GERAL
🏢 PRINCIPAIS PLAYERS
💰 INVESTIMENTOS E FUNDING
🏛️  POLÍTICAS PÚBLICAS
🎯 TENDÊNCIAS EMERGENTES
📊 DADOS E ESTATÍSTICAS

Use sempre as ferramentas de busca para obter informações atualizadas!")
        {
        }

        [FunctionCall("Buscar informações sobre startups e empresas de IA")]
        [FunctionCallParameter("setor", "Setor específico de IA para pesquisar")]
        [FunctionCallParameter("regiao", "Região/estado para focar a busca")]
        private string PesquisarEcossistemaIA(string setor, string regiao = "Brasil")
        {
            return $@"
🏢 ECOSSISTEMA DE IA - {setor.ToUpper()} - {regiao}
═══════════════════════════════════════════

🚀 PRINCIPAIS STARTUPS:
• Semantix - Plataforma de big data e analytics
• Kunumi - Soluções de IA para e-commerce
• Aquarela Analytics - Analytics avançado
• Isaac - IA para agronegócio
• Olist - Machine learning para marketplace

💰 INVESTIMENTOS 2024:
• Total investido: R$ 2.8 bilhões
• Número de rounds: 156 investimentos
• Ticket médio: R$ 18M
• Principais VCs: Monashees, Kaszek, Valor

🏛️ INICIATIVAS GOVERNAMENTAIS:
• Estratégia Brasileira de IA (EBIA)
• Centro de Excelência em IA (CEIA)
• Lei Marco da IA em discussão
• Programas de fomento via FINEP/CNPq

📊 MERCADO:
• Tamanho: R$ 14.2 bilhões (2024)
• Crescimento anual: 28%
• Empregos gerados: 180k+ profissionais
• Setores líderes: Fintech, Agro, Saúde

🎯 TENDÊNCIAS:
• IA Generativa em ascensão
• AutoML democratizando acesso
• Edge Computing + IA
• IA Responsável e Ética

📅 Última atualização: {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        [FunctionCall("Pesquisar políticas públicas e regulamentação")]
        [FunctionCallParameter("tema", "Área específica de política pública")]
        private string PesquisarPoliticasPublicas(string tema)
        {
            return $@"
🏛️ POLÍTICAS PÚBLICAS DE IA - {tema.ToUpper()}
════════════════════════════════════════

📋 MARCO REGULATÓRIO:
• PL 2338/2023 - Lei de IA em tramitação
• Regulamentação de algoritmos
• Proteção de dados e LGPD
• Diretrizes éticas para IA

🎯 ESTRATÉGIA NACIONAL:
• EBIA 2021-2030 aprovada
• Eixos: Pesquisa, Educação, Aplicação
• Orçamento: R$ 1.2 bilhão
• Meta: Top 20 mundial em IA

💼 PROGRAMAS GOVERNAMENTAIS:
• Centro de Pesquisa IA - R$ 400M
• Qualificação profissional - 100k vagas
• Sandbox regulatório para fintechs
• Parcerias público-privadas

🌍 COOPERAÇÃO INTERNACIONAL:
• Parceria com Reino Unido
• Acordo bilateral EUA-Brasil
• Participação na Global Partnership AI
• Colaboração com Singapura

📊 RESULTADOS:
• 45 centros de pesquisa credenciados
• 15k bolsas de estudo concedidas
• 230 projetos de P&D aprovados
• 12 sandboxes regulatórios ativos

⚖️ DESAFIOS REGULATÓRIOS:
• Definição de responsabilidade civil
• Transparência algorítmica
• Proteção trabalhista
• Propriedade intelectual

📅 Status: {DateTime.Now:dd/MM/yyyy} - Em desenvolvimento";
        }
    }

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

    public class AgenteEscritor : Agent<ContextoPesquisa, string>
    {
        public AgenteEscritor(IModel model)
            : base(model,
                   name: "RedatorExecutivo",
                   instructions: @"
Você é um redator executivo especializado em relatórios estratégicos! ✍️📋

PRINCÍPIOS DE ESCRITA EXECUTIVA:
1. CLAREZA - Linguagem direta e objetiva
2. ESTRUTURA - Organização lógica e hierárquica
3. INSIGHTS - Destaque pontos-chave e acionáveis
4. EVIDÊNCIAS - Base em dados e fatos
5. EXECUTIVO - Foco em decisões e estratégia

ESTRUTURA PADRÃO:
📊 RESUMO EXECUTIVO
🔍 CONTEXTO E METODOLOGIA
📈 PRINCIPAIS DESCOBERTAS
💡 INSIGHTS ESTRATÉGICOS
🎯 RECOMENDAÇÕES
📋 PRÓXIMOS PASSOS

Use bullet points, tabelas e formatação visual para facilitar leitura!")
        {
        }
    }

    public class AgenteRevisor : Agent<ContextoPesquisa, string>
    {
        public AgenteRevisor(IModel model)
            : base(model,
                   name: "RevisorExecutivo",
                   instructions: @"
Você é um revisor executivo sênior com 15+ anos de experiência! 🔍✨

CRITÉRIOS DE REVISÃO:
1. CLAREZA - Linguagem acessível ao público-alvo
2. PRECISÃO - Verificação de dados e consistência
3. RELEVÂNCIA - Foco em insights acionáveis
4. ESTRUTURA - Organização lógica e fluída
5. IMPACTO - Mensagem clara e convincente

CHECKLIST DE QUALIDADE:
✅ Resumo executivo impactante
✅ Dados verificados e atualizados
✅ Insights únicos e valiosos
✅ Recomendações específicas
✅ Call-to-action claro
✅ Formatação profissional

Eleve o conteúdo ao padrão C-level!")
        {
        }
    }

    #endregion
} 