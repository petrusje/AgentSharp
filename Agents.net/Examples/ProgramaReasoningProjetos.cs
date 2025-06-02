using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
    /// <summary>
    /// Programa de teste completo do sistema de reasoning com function calling
    /// para análise de gargalos em projetos de desenvolvimento
    /// </summary>
    public static class ProgramaReasoningProjetos
    {
        public static async Task ExecutarAnaliseGargalosProjetos(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🚀 TESTE COMPLETO: REASONING + FUNCTION CALLING - ANÁLISE DE PROJETOS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📊 Sistema avançado de análise de gargalos em projetos");
            Console.WriteLine("🧠 Demonstra reasoning estruturado com tools especializados\n");

            // Inicializar banco de dados de projetos
            var bancoDados = InicializarBancoDadosProjetos();
            var contexto = new ContextoProjetos { BancoDados = bancoDados };

            // Criar agente especialista em projetos com reasoning habilitado
            var analistaProjectos = new Agent<ContextoProjetos, RelatorioAnalise>(modelo, "AnalistaProjetos")
                .WithContext(contexto)
                .WithReasoning(true)
                .WithReasoningSteps(4, 8)
                .WithToolPacks(new ProjetosToolPack())
                .WithPersona(@"
Você é um consultor sênior especialista em análise de projetos de desenvolvimento! 🎯📊

ESPECIALIDADE:
- Identificação de gargalos em projetos
- Análise de métricas de performance
- Otimização de processos de desenvolvimento
- Diagnóstico de problemas de equipe e infraestrutura

METODOLOGIA DE ANÁLISE:
1. Colete dados dos projetos usando as ferramentas disponíveis
2. Analise métricas críticas (velocidade, bugs, deploys)
3. Identifique padrões e gargalos
4. Proponha soluções específicas e mensuráveis
5. Priorize ações por impacto x esforço

SEMPRE use as ferramentas para buscar dados reais antes de fazer análises!")
                .WithInstructions(@"
Para cada análise:
1. Use GetProjeto() para obter detalhes do projeto
2. Use GetMetricasProjeto() para dados de performance
3. Use GetEquipeProjeto() para informações da equipe
4. Use GetHistoricoDeploysDefeitos() para análise de qualidade
5. Analise os dados usando reasoning estruturado
6. Forneça recomendações acionáveis");

            // Cenário de teste conhecido: Projeto com gargalo específico
            var projetoProblema = "PROJ-003"; // Projeto com problemas de deploy

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"🔍 Analisando projeto com gargalos conhecidos: {projetoProblema}");
            Console.WriteLine("📋 Esperado: Identificar problemas de deploy e instabilidade");
            Console.ResetColor();
            Console.WriteLine("\n🧠 Iniciando análise com reasoning estruturado...\n");

            try
            {
                var resultado = await analistaProjectos.ExecuteAsync($@"
Analise completamente o projeto {projetoProblema} e identifique todos os gargalos existentes.

ESCOPO DA ANÁLISE:
- Performance geral do projeto
- Métricas de velocidade de desenvolvimento
- Qualidade de código e bugs
- Eficiência da equipe
- Problemas de deploy e infraestrutura

Use todas as ferramentas disponíveis para coletar dados e forneça:
1. Diagnóstico completo dos problemas
2. Priorização dos gargalos por impacto
3. Plano de ação específico
4. Métricas para acompanhar melhorias

Seja detalhado e baseie-se nos dados coletados.");

                // Exibir resultado principal
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ RELATÓRIO DE ANÁLISE GERADO:");
                Console.WriteLine(new string('=', 60));
                Console.ResetColor();
                
                if (resultado.Data != null)
                {
                    Console.WriteLine(resultado.Data);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Erro: Resultado da análise está vazio!");
                    Console.ResetColor();
                }

                // Exibir ferramentas utilizadas
                if (resultado.Tools.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n🔧 FERRAMENTAS UTILIZADAS:");
                    Console.WriteLine(new string('=', 40));
                    foreach (var tool in resultado.Tools)
                    {
                        Console.WriteLine($"🛠️  {tool.Name}: {tool.Result}");
                    }
                    Console.ResetColor();
                }

                // Exibir processo de reasoning
                if (!string.IsNullOrEmpty(resultado.ReasoningContent))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("\n🧠 PROCESSO DE RACIOCÍNIO:");
                    Console.WriteLine(new string('=', 50));
                    Console.WriteLine(resultado.ReasoningContent);
                    Console.ResetColor();
                }

                // Validar se identificou os gargalos esperados
                ValidarResultadoEsperado(resultado, projetoProblema);

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
                Console.WriteLine($"🧠 Steps de reasoning: {resultado.ReasoningSteps.Count}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro durante análise: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        private static void ValidarResultadoEsperado(AgentResult<RelatorioAnalise> resultado, string projetoId)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🎯 VALIDAÇÃO DOS RESULTADOS ESPERADOS:");
            Console.WriteLine(new string('=', 45));

            // Verificações esperadas para PROJ-003
            var problemas_esperados = new List<string>
            {
                "deploy", "instabilidade", "frequency", "pipeline", "infraestrutura"
            };

            var resultado_texto = resultado.Data?.ToString() ?? "";
            var encontrados = new List<string>();
            var nao_encontrados = new List<string>();

            foreach (var problema in problemas_esperados)
            {
                if (resultado_texto.ToLower().Contains(problema.ToLower()))
                {
                    encontrados.Add(problema);
                }
                else
                {
                    nao_encontrados.Add(problema);
                }
            }

            Console.WriteLine($"✅ Problemas identificados corretamente: {encontrados.Count}/{problemas_esperados.Count}");
            foreach (var item in encontrados)
            {
                Console.WriteLine($"   ✓ {item}");
            }

            if (nao_encontrados.Count > 0)
            {
                Console.WriteLine($"❌ Problemas não identificados: {nao_encontrados.Count}");
                foreach (var item in nao_encontrados)
                {
                    Console.WriteLine($"   ✗ {item}");
                }
            }

            // Validar uso de ferramentas
            var ferramentas_esperadas = new List<string> { "GetProjeto", "GetMetricas", "GetEquipe" };
            var ferramentas_usadas = resultado.Tools.Select(t => t.Name).ToList();
            
            Console.WriteLine($"\n🔧 Ferramentas utilizadas: {ferramentas_usadas.Count}");
            foreach (var ferramenta in ferramentas_esperadas)
            {
                if (ferramentas_usadas.Any(f => f.Contains(ferramenta)))
                {
                    Console.WriteLine($"   ✓ {ferramenta}");
                }
                else
                {
                    Console.WriteLine($"   ✗ {ferramenta} (não utilizada)");
                }
            }

            Console.ResetColor();
        }

        private static BancoDadosProjetos InicializarBancoDadosProjetos()
        {
            return new BancoDadosProjetos
            {
                Projetos = new Dictionary<string, Projeto>
                {
                    ["PROJ-001"] = new Projeto
                    {
                        Id = "PROJ-001",
                        Nome = "Sistema de E-commerce",
                        Status = "Em Desenvolvimento",
                        DataInicio = DateTime.Now.AddMonths(-6),
                        TamanhoEquipe = 5,
                        Tecnologias = new[] { "React", "Node.js", "MongoDB" },
                        MetricasVelocidade = new MetricasVelocidade
                        {
                            VelocidadeMedia = 8.5,
                            BurndownRate = 0.85,
                            TasksCompletadas = 45,
                            TasksPendentes = 12
                        },
                        MetricasQualidade = new MetricasQualidade
                        {
                            BugsAbertos = 3,
                            BugsFechados = 28,
                            CoberturaTestes = 85.2,
                            DeployFrequency = 12 // deploys por mês
                        }
                    },
                    ["PROJ-002"] = new Projeto
                    {
                        Id = "PROJ-002", 
                        Nome = "API de Pagamentos",
                        Status = "Homologação",
                        DataInicio = DateTime.Now.AddMonths(-4),
                        TamanhoEquipe = 3,
                        Tecnologias = new[] { ".NET Core", "PostgreSQL", "Redis" },
                        MetricasVelocidade = new MetricasVelocidade
                        {
                            VelocidadeMedia = 12.0,
                            BurndownRate = 0.95,
                            TasksCompletadas = 67,
                            TasksPendentes = 3
                        },
                        MetricasQualidade = new MetricasQualidade
                        {
                            BugsAbertos = 1,
                            BugsFechados = 15,
                            CoberturaTestes = 92.8,
                            DeployFrequency = 8
                        }
                    },
                    ["PROJ-003"] = new Projeto
                    {
                        Id = "PROJ-003",
                        Nome = "Dashboard Analytics", 
                        Status = "Em Desenvolvimento",
                        DataInicio = DateTime.Now.AddMonths(-8),
                        TamanhoEquipe = 7,
                        Tecnologias = new[] { "Vue.js", "Python", "ClickHouse" },
                        MetricasVelocidade = new MetricasVelocidade
                        {
                            VelocidadeMedia = 4.2, // BAIXA - GARGALO!
                            BurndownRate = 0.65,   // BAIXA - GARGALO!
                            TasksCompletadas = 23,
                            TasksPendentes = 34    // ALTO - GARGALO!
                        },
                        MetricasQualidade = new MetricasQualidade
                        {
                            BugsAbertos = 18,      // ALTO - GARGALO!
                            BugsFechados = 22,
                            CoberturaTestes = 45.3, // BAIXA - GARGALO!
                            DeployFrequency = 2     // MUITO BAIXA - GARGALO CRÍTICO!
                        }
                    }
                },
                Equipes = new Dictionary<string, EquipeProjeto>
                {
                    ["PROJ-001"] = new EquipeProjeto
                    {
                        ProjetoId = "PROJ-001",
                        TechLead = "Carlos Silva",
                        Desenvolvedores = new[] { "Ana Costa", "João Pereira", "Maria Santos" },
                        QA = "Pedro Oliveira",
                        SatisfacaoEquipe = 8.5,
                        RotatividadeUltimos6Meses = 1
                    },
                    ["PROJ-002"] = new EquipeProjeto
                    {
                        ProjetoId = "PROJ-002",
                        TechLead = "Fernanda Lima",
                        Desenvolvedores = new[] { "Roberto Nunes", "Julia Martins" },
                        QA = "Diego Rocha",
                        SatisfacaoEquipe = 9.2,
                        RotatividadeUltimos6Meses = 0
                    },
                    ["PROJ-003"] = new EquipeProjeto
                    {
                        ProjetoId = "PROJ-003",
                        TechLead = "Rafael Mendes",
                        Desenvolvedores = new[] { "Lucia Ferreira", "Marcos Andrade", "Camila Torres", "Bruno Machado" },
                        QA = "Tatiana Gomes",
                        SatisfacaoEquipe = 5.8, // BAIXA - GARGALO!
                        RotatividadeUltimos6Meses = 3 // ALTA - GARGALO!
                    }
                },
                HistoricoDeploysDefeitos = new Dictionary<string, List<EventoDeploy>>
                {
                    ["PROJ-001"] = new List<EventoDeploy>
                    {
                        new EventoDeploy { Data = DateTime.Now.AddDays(-7), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(5) },
                        new EventoDeploy { Data = DateTime.Now.AddDays(-14), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(3) },
                        new EventoDeploy { Data = DateTime.Now.AddDays(-21), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(8) }
                    },
                    ["PROJ-002"] = new List<EventoDeploy>
                    {
                        new EventoDeploy { Data = DateTime.Now.AddDays(-4), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(2) },
                        new EventoDeploy { Data = DateTime.Now.AddDays(-11), Sucesso = true, TempoInatividade = TimeSpan.FromMinutes(1) }
                    },
                    ["PROJ-003"] = new List<EventoDeploy>
                    {
                        new EventoDeploy { Data = DateTime.Now.AddDays(-45), Sucesso = false, TempoInatividade = TimeSpan.FromHours(4) }, // GARGALO CRÍTICO!
                        new EventoDeploy { Data = DateTime.Now.AddDays(-90), Sucesso = false, TempoInatividade = TimeSpan.FromHours(6) }, // GARGALO CRÍTICO!
                        new EventoDeploy { Data = DateTime.Now.AddDays(-120), Sucesso = true, TempoInatividade = TimeSpan.FromHours(2) }
                    }
                }
            };
        }
    }

    #region Classes de Dados e Contexto

    public class ContextoProjetos
    {
        public BancoDadosProjetos BancoDados { get; set; }
    }

    public class BancoDadosProjetos
    {
        public Dictionary<string, Projeto> Projetos { get; set; } = new Dictionary<string, Projeto>();
        public Dictionary<string, EquipeProjeto> Equipes { get; set; } = new Dictionary<string, EquipeProjeto>();
        public Dictionary<string, List<EventoDeploy>> HistoricoDeploysDefeitos { get; set; } = new Dictionary<string, List<EventoDeploy>>();
    }

    public class Projeto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Status { get; set; }
        public DateTime DataInicio { get; set; }
        public int TamanhoEquipe { get; set; }
        public string[] Tecnologias { get; set; }
        public MetricasVelocidade MetricasVelocidade { get; set; }
        public MetricasQualidade MetricasQualidade { get; set; }
    }

    public class MetricasVelocidade
    {
        public double VelocidadeMedia { get; set; } // story points por sprint
        public double BurndownRate { get; set; } // 0-1
        public int TasksCompletadas { get; set; }
        public int TasksPendentes { get; set; }
    }

    public class MetricasQualidade
    {
        public int BugsAbertos { get; set; }
        public int BugsFechados { get; set; }
        public double CoberturaTestes { get; set; } // %
        public int DeployFrequency { get; set; } // deploys por mês
    }

    public class EquipeProjeto
    {
        public string ProjetoId { get; set; }
        public string TechLead { get; set; }
        public string[] Desenvolvedores { get; set; }
        public string QA { get; set; }
        public double SatisfacaoEquipe { get; set; } // 0-10
        public int RotatividadeUltimos6Meses { get; set; }
    }

    public class EventoDeploy
    {
        public DateTime Data { get; set; }
        public bool Sucesso { get; set; }
        public TimeSpan TempoInatividade { get; set; }
    }

    public class RelatorioAnalise
    {
        public string ProjetoId { get; set; }
        public List<string> GargalosIdentificados { get; set; } = new List<string>();
        public List<string> Recomendacoes { get; set; } = new List<string>();
        public string PrioridadeAcoes { get; set; }
        public Dictionary<string, double> MetricasChave { get; set; } = new Dictionary<string, double>();
    }

    #endregion

    #region ToolPack Especializado

    public class ProjetosToolPack : ToolPack
    {
        [FunctionCall("Obter informações detalhadas de um projeto")]
        [FunctionCallParameter("projetoId", "ID do projeto a ser consultado")]
        public string GetProjeto(string projetoId)
        {
            var banco = GetProperty("BancoDados") as BancoDadosProjetos;
            
            if (banco?.Projetos.TryGetValue(projetoId, out var projeto) == true)
            {
                return $@"
📊 PROJETO: {projeto.Nome} ({projeto.Id})
Status: {projeto.Status}
Início: {projeto.DataInicio:dd/MM/yyyy}
Equipe: {projeto.TamanhoEquipe} pessoas
Tecnologias: {string.Join(", ", projeto.Tecnologias)}
Duração: {(DateTime.Now - projeto.DataInicio).Days} dias";
            }
            
            return $"❌ Projeto {projetoId} não encontrado";
        }

        [FunctionCall("Obter métricas de performance do projeto")]
        [FunctionCallParameter("projetoId", "ID do projeto")]
        public string GetMetricasProjeto(string projetoId)
        {
            var banco = GetProperty("BancoDados") as BancoDadosProjetos;
            
            if (banco?.Projetos.TryGetValue(projetoId, out var projeto) == true)
            {
                var velocidade = projeto.MetricasVelocidade;
                var qualidade = projeto.MetricasQualidade;
                
                return $@"
📈 MÉTRICAS DE PERFORMANCE - {projetoId}:

VELOCIDADE:
• Velocidade média: {velocidade.VelocidadeMedia:F1} pts/sprint
• Burndown rate: {velocidade.BurndownRate:P0}
• Tasks completadas: {velocidade.TasksCompletadas}
• Tasks pendentes: {velocidade.TasksPendentes}

QUALIDADE:
• Bugs abertos: {qualidade.BugsAbertos}
• Bugs fechados: {qualidade.BugsFechados}  
• Cobertura testes: {qualidade.CoberturaTestes:F1}%
• Deploy frequency: {qualidade.DeployFrequency}/mês

ALERTAS:
{(velocidade.VelocidadeMedia < 6 ? "⚠️ Velocidade baixa!" : "")}
{(velocidade.BurndownRate < 0.8 ? "⚠️ Burndown rate baixo!" : "")}
{(qualidade.BugsAbertos > 10 ? "⚠️ Muitos bugs abertos!" : "")}
{(qualidade.CoberturaTestes < 70 ? "⚠️ Cobertura de testes baixa!" : "")}
{(qualidade.DeployFrequency < 4 ? "⚠️ Deploy frequency muito baixa!" : "")}";
            }
            
            return $"❌ Métricas não encontradas para {projetoId}";
        }

        [FunctionCall("Obter informações da equipe do projeto")]
        [FunctionCallParameter("projetoId", "ID do projeto")]
        public string GetEquipeProjeto(string projetoId)
        {
            var banco = GetProperty("BancoDados") as BancoDadosProjetos;
            
            if (banco?.Equipes.TryGetValue(projetoId, out var equipe) == true)
            {
                return $@"
👥 EQUIPE DO PROJETO {projetoId}:

ESTRUTURA:
• Tech Lead: {equipe.TechLead}
• Desenvolvedores: {string.Join(", ", equipe.Desenvolvedores)}
• QA: {equipe.QA}

MÉTRICAS:
• Satisfação da equipe: {equipe.SatisfacaoEquipe:F1}/10
• Rotatividade (6 meses): {equipe.RotatividadeUltimos6Meses} pessoas

ALERTAS:
{(equipe.SatisfacaoEquipe < 7 ? "⚠️ Satisfação da equipe baixa!" : "")}
{(equipe.RotatividadeUltimos6Meses > 1 ? "⚠️ Alta rotatividade de pessoal!" : "")}";
            }
            
            return $"❌ Equipe não encontrada para {projetoId}";
        }

        [FunctionCall("Obter histórico de deploys e defeitos")]
        [FunctionCallParameter("projetoId", "ID do projeto")]
        public string GetHistoricoDeploysDefeitos(string projetoId)
        {
            var banco = GetProperty("BancoDados") as BancoDadosProjetos;
            
            if (banco?.HistoricoDeploysDefeitos.TryGetValue(projetoId, out var historico) == true)
            {
                var deploysSucesso = historico.Where(d => d.Sucesso).Count();
                var deploysFalha = historico.Where(d => !d.Sucesso).Count();
                var tempoMedioInatividade = historico.Average(d => d.TempoInatividade.TotalMinutes);
                
                var resultado = $@"
🚀 HISTÓRICO DE DEPLOYS - {projetoId}:

ESTATÍSTICAS:
• Total de deploys: {historico.Count}
• Deploys com sucesso: {deploysSucesso}
• Deploys com falha: {deploysFalha}
• Taxa de sucesso: {(deploysSucesso * 100.0 / historico.Count):F1}%
• Tempo médio de inatividade: {tempoMedioInatividade:F1} minutos

ÚLTIMOS EVENTOS:";

                foreach (var deploy in historico.OrderByDescending(d => d.Data).Take(3))
                {
                    resultado += $"\n• {deploy.Data:dd/MM/yyyy}: {(deploy.Sucesso ? "✅ Sucesso" : "❌ Falha")} - Inatividade: {deploy.TempoInatividade.TotalMinutes:F0}min";
                }

                if (deploysFalha > deploysSucesso)
                {
                    resultado += "\n\n⚠️ ALERTA CRÍTICO: Mais falhas que sucessos nos deploys!";
                }
                
                if (tempoMedioInatividade > 60)
                {
                    resultado += "\n⚠️ Tempo de inatividade elevado nos deploys!";
                }

                return resultado;
            }
            
            return $"❌ Histórico não encontrado para {projetoId}";
        }

        [FunctionCall("Comparar projeto com benchmarks da indústria")]
        [FunctionCallParameter("projetoId", "ID do projeto a comparar")]
        public string CompararComBenchmarks(string projetoId)
        {
            var banco = GetProperty("BancoDados") as BancoDadosProjetos;
            
            if (banco?.Projetos.TryGetValue(projetoId, out var projeto) == true)
            {
                var velocidade = projeto.MetricasVelocidade;
                var qualidade = projeto.MetricasQualidade;
                
                // Benchmarks da indústria (valores típicos)
                const double velocidadeBenchmark = 8.0;
                const double burndownBenchmark = 0.85;
                const double coberturaBenchmark = 80.0;
                const int deployBenchmark = 8;
                
                return $@"
📊 COMPARAÇÃO COM BENCHMARKS - {projetoId}:

VELOCIDADE:
• Atual: {velocidade.VelocidadeMedia:F1} | Benchmark: {velocidadeBenchmark:F1} | {(velocidade.VelocidadeMedia >= velocidadeBenchmark ? "✅" : "❌")}

BURNDOWN RATE:
• Atual: {velocidade.BurndownRate:P0} | Benchmark: {burndownBenchmark:P0} | {(velocidade.BurndownRate >= burndownBenchmark ? "✅" : "❌")}

COBERTURA DE TESTES:
• Atual: {qualidade.CoberturaTestes:F1}% | Benchmark: {coberturaBenchmark:F1}% | {(qualidade.CoberturaTestes >= coberturaBenchmark ? "✅" : "❌")}

DEPLOY FREQUENCY:
• Atual: {qualidade.DeployFrequency}/mês | Benchmark: {deployBenchmark}/mês | {(qualidade.DeployFrequency >= deployBenchmark ? "✅" : "❌")}

CLASSIFICAÇÃO GERAL: {ClassificarProjeto(velocidade, qualidade)}";
            }
            
            return $"❌ Projeto {projetoId} não encontrado para comparação";
        }

        private string ClassificarProjeto(MetricasVelocidade velocidade, MetricasQualidade qualidade)
        {
            var pontos = 0;
            if (velocidade.VelocidadeMedia >= 8.0) pontos++;
            if (velocidade.BurndownRate >= 0.85) pontos++;
            if (qualidade.CoberturaTestes >= 80.0) pontos++;
            if (qualidade.DeployFrequency >= 8) pontos++;
            
            switch (pontos)
            {
                case 4: return "🟢 EXCELENTE";
                case 3: return "🟡 BOM";
                case 2: return "🟠 REGULAR";
                case 1: return "🔴 PROBLEMÁTICO";
                default: return "🚨 CRÍTICO";
            }
        }
    }

    #endregion
} 