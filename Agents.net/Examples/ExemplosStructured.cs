using System;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Utils;

namespace Agents.net.Examples
{
    /// <summary>
    /// Exemplos de Structured Outputs - funcionalidade avançada do Agents.net
    /// Demonstra saídas estruturadas tipadas para análise de documentos
    /// </summary>
    public static class ExemplosStructured
    {
        /// <summary>
        /// Exemplo que demonstra structured outputs avançados do Agents.net
        /// Funcionalidade para análise estruturada de documentos empresariais
        /// </summary>
        public static async Task ExecutarAnaliseDocumento(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📋 EXEMPLO 5: ANÁLISE DE DOCUMENTOS - STRUCTURED OUTPUTS");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📄 Funcionalidade exclusiva do Agents.net");
            Console.WriteLine("Demonstra structured outputs tipados para análise de documentos\n");

            // Configuração para structured output
            var config = new ModelConfiguration()
                .WithStructuredExtraction<AnaliseDocumento>();

            var analisadorDocumento = new Agent<object, AnaliseDocumento>(modelo, 
                "AnalisadorDocumentos", 
                @"Você é um especialista em análise de documentos empresariais. 
                Analise o documento fornecido e extraia informações estruturadas conforme solicitado.
                Seja preciso e detalhado em sua análise. 
                IMPORTANTE: Retorne apenas dados válidos e certifique-se de que todos os campos de string sejam preenchidos corretamente.",
                config);

            var documentoExemplo = @"
RELATÓRIO TRIMESTRAL - Q3 2024
EMPRESA: TechStart Solutions Ltda.
CNPJ: 12.345.678/0001-90

RESUMO FINANCEIRO:
- Receita Total: R$ 2.450.000
- Custos Operacionais: R$ 1.890.000  
- Lucro Líquido: R$ 560.000
- Margem de Lucro: 22.8%

PRINCIPAIS CONQUISTAS:
- Lançamento de 2 novos produtos
- Aquisição de 150 novos clientes
- Expansão para 3 novas cidades
- Contratação de 25 funcionários

DESAFIOS IDENTIFICADOS:
- Competição acirrada no mercado
- Dificuldades de retenção de talentos
- Necessidade de investimento em marketing

PRÓXIMOS PASSOS:
- Implementar programa de retenção
- Aumentar investimento em P&D (15%)
- Expandir equipe de vendas
- Lançar campanha de marketing digital

CEO: Maria Silva
Data: 15/11/2024";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Analisando documento empresarial com extração estruturada...");
            Console.ResetColor();
            Console.WriteLine("\n📊 Resultado Estruturado:");
            Console.WriteLine(new string('-', 50));

            try
            {
                var resultado = await analisadorDocumento.ExecuteAsync(
                    $"Analise este documento empresarial e extraia todas as informações relevantes de forma estruturada:\n\n{documentoExemplo}"
                );

                // Verificar se o resultado não é nulo
                if (resultado?.Data != null)
                {
                    var analise = resultado.Data;
                    
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("🏢 INFORMAÇÕES DA EMPRESA:");
                    Console.ResetColor();
                    Console.WriteLine($"   Nome: {analise.InformacoesEmpresa?.Nome ?? "N/A"}");
                    Console.WriteLine($"   CNPJ: {analise.InformacoesEmpresa?.CNPJ ?? "N/A"}");
                    Console.WriteLine($"   CEO: {analise.InformacoesEmpresa?.CEO ?? "N/A"}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n💰 DADOS FINANCEIROS:");
                    Console.ResetColor();
                    if (analise.DadosFinanceiros != null)
                    {
                        Console.WriteLine($"   Receita: R$ {analise.DadosFinanceiros.ReceitaTotal:N2}");
                        Console.WriteLine($"   Custos: R$ {analise.DadosFinanceiros.CustosOperacionais:N2}");
                        Console.WriteLine($"   Lucro: R$ {analise.DadosFinanceiros.LucroLiquido:N2}");
                        Console.WriteLine($"   Margem: {analise.DadosFinanceiros.MargemLucro:P2}");
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n🎯 CONQUISTAS:");
                    Console.ResetColor();
                    if (analise.Conquistas != null)
                    {
                        foreach (var conquista in analise.Conquistas)
                        {
                            Console.WriteLine($"   ✅ {conquista}");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n⚠️  DESAFIOS:");
                    Console.ResetColor();
                    if (analise.Desafios != null)
                    {
                        if (!string.IsNullOrEmpty(analise.Desafios.Competicao))
                            Console.WriteLine($"   🔴 Competição: {analise.Desafios.Competicao}");
                        if (!string.IsNullOrEmpty(analise.Desafios.RetencaoTalentos))
                            Console.WriteLine($"   🔴 Retenção de Talentos: {analise.Desafios.RetencaoTalentos}");
                        if (!string.IsNullOrEmpty(analise.Desafios.InvestimentoMarketing))
                            Console.WriteLine($"   🔴 Marketing: {analise.Desafios.InvestimentoMarketing}");
                        
                        // Display additional extension data if any
                        foreach (var kvp in analise.Desafios.ExtensionData)
                        {
                            Console.WriteLine($"   🔴 {kvp.Key}: {kvp.Value}");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("\n🚀 PRÓXIMOS PASSOS:");
                    Console.ResetColor();
                    if (analise.ProximosPassos != null)
                    {
                        if (!string.IsNullOrEmpty(analise.ProximosPassos.ProgramaRetencao))
                            Console.WriteLine($"   📋 Programa Retenção: {analise.ProximosPassos.ProgramaRetencao}");
                        if (!string.IsNullOrEmpty(analise.ProximosPassos.InvestimentoPD))
                            Console.WriteLine($"   📋 Investimento P&D: {analise.ProximosPassos.InvestimentoPD}");
                        if (!string.IsNullOrEmpty(analise.ProximosPassos.ExpandirVendas))
                            Console.WriteLine($"   📋 Expandir Vendas: {analise.ProximosPassos.ExpandirVendas}");
                        if (!string.IsNullOrEmpty(analise.ProximosPassos.CampanhaMarketing))
                            Console.WriteLine($"   📋 Marketing: {analise.ProximosPassos.CampanhaMarketing}");
                        
                        // Display additional extension data if any
                        foreach (var kvp in analise.ProximosPassos.ExtensionData)
                        {
                            Console.WriteLine($"   📋 {kvp.Key}: {kvp.Value}");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\n📊 CLASSIFICAÇÃO: {analise.ClassificacaoGeral}");
                    Console.WriteLine($"📈 SCORE FINANCEIRO: {analise.ScoreFinanceiro}/10");
                    Console.WriteLine($"📅 PERÍODO: {analise.Periodo ?? "N/A"}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Resultado estruturado está nulo, verifique a configuração do modelo");
                    Console.ResetColor();
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"📊 Tokens utilizados: {resultado?.Usage?.TotalTokens ?? 0}");
                Console.WriteLine($"⚡ Structured Output: {(resultado?.Data != null ? "✅ Sucesso" : "❌ Falhou")}");

                // Exemplo adicional com currículo
                Console.WriteLine("\n🔄 Testando com análise de currículo...\n");
                await ExemplarAnaliseCurriculo(modelo);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Exemplo específico para análise de currículos com structured outputs
        /// </summary>
        public static async Task ExecutarAnaliseCurriculo(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("👤 EXEMPLO 8: ANÁLISE DE CURRÍCULOS - STRUCTURED OUTPUTS");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📋 Análise estruturada de currículos para RH");
            Console.WriteLine("Demonstra extração tipada de dados profissionais\n");

            await ExemplarAnaliseCurriculo(modelo);
        }

        private static async Task ExemplarAnaliseCurriculo(IModel modelo)
        {
            var configCurriculo = new ModelConfiguration()
                .WithStructuredExtraction<AnaliseCurriculo>();

            var analisadorCV = new Agent<object, AnaliseCurriculo>(modelo,
                "AnalisadorCurriculo",
                @"Você é um especialista em RH e análise de currículos. 
                Analise o currículo fornecido e extraia todas as informações relevantes.",
                configCurriculo);

            var curriculoExemplo = @"
JOÃO SANTOS SILVA
Desenvolvedor Full Stack Senior
Email: joao.santos@email.com | Telefone: (11) 99999-9999
LinkedIn: linkedin.com/in/joaosantos | GitHub: github.com/joaosantos

RESUMO PROFISSIONAL:
Desenvolvedor Full Stack com 8 anos de experiência em tecnologias web modernas.
Especialista em React, Node.js e cloud computing. Liderou equipes de até 12 desenvolvedores.

EXPERIÊNCIA:
• Tech Leader - InnovaTech (2022-2024)
  - Liderou migração para arquitetura de microsserviços
  - Implementou CI/CD reduzindo deploy time em 80%
  - Mentoria de 8 desenvolvedores júnior

• Senior Developer - StartupXYZ (2019-2022)  
  - Desenvolveu plataforma de e-commerce (React/Node.js)
  - Implementou sistema de pagamentos
  - Otimizou performance em 300%

• Developer - CodeCorp (2016-2019)
  - Desenvolvimento de APIs REST
  - Integração com serviços terceiros
  - Manutenção de aplicações legacy

HABILIDADES TÉCNICAS:
• Frontend: React, Vue.js, TypeScript, CSS3
• Backend: Node.js, Python, Java, .NET
• Database: PostgreSQL, MongoDB, Redis
• Cloud: AWS, Docker, Kubernetes
• Tools: Git, Jenkins, JIRA

FORMAÇÃO:
• Ciência da Computação - USP (2012-2016)
• MBA em Gestão de Projetos - FGV (2020-2021)

CERTIFICAÇÕES:
• AWS Solutions Architect
• Scrum Master Certified
• Google Cloud Professional";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Analisando currículo com extração estruturada...");
            Console.ResetColor();

            var resultado = await analisadorCV.ExecuteAsync(
                $"Analise este currículo e extraia todas as informações de forma estruturada:\n\n{curriculoExemplo}"
            );

            // Exibir resultado como JSON
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(resultado.Data);
            Console.ResetColor();
            
            // Tentar fazer parse para análise estruturada
            try
            {
                var cv = resultado.Data;

                if (cv != null)
                {
                    Console.WriteLine("\n👤 DADOS PESSOAIS:");
                    Console.WriteLine($"   Nome: {cv.DadosPessoais?.Nome ?? "N/A"}");
                    Console.WriteLine($"   Cargo: {cv.DadosPessoais?.CargoAtual ?? "N/A"}");
                    Console.WriteLine($"   Email: {cv.DadosPessoais?.Email ?? "N/A"}");
                    Console.WriteLine($"   Experiência: {cv.AnosExperiencia} anos");

                    Console.WriteLine("\n💼 EXPERIÊNCIAS:");
                    if (cv.Experiencias != null)
                    {
                        foreach (var exp in cv.Experiencias)
                        {
                            Console.WriteLine($"   🏢 {exp?.Cargo ?? "N/A"} - {exp?.Empresa ?? "N/A"} ({exp?.Periodo ?? "N/A"})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("   Nenhuma experiência encontrada");
                    }

                    Console.WriteLine("\n🛠️  HABILIDADES PRINCIPAIS:");
                    if (cv.HabilidadesPrincipais != null)
                    {
                        foreach (var skill in cv.HabilidadesPrincipais)
                        {
                            Console.WriteLine($"   ⚡ {skill ?? "N/A"}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("   Nenhuma habilidade encontrada");
                    }

                    Console.WriteLine($"\n📊 NÍVEL SENIORIDADE: {cv.NivelSenioridade}");
                    Console.WriteLine($"🎯 SCORE GERAL: {cv.ScoreGeral}/100");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Dados estruturados são nulos");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ Erro ao fazer parse do JSON: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    #region Classes para Structured Output

    public class AnaliseDocumento
    {
        [JsonPropertyName("informacoes_empresa")]
        public InformacoesEmpresa InformacoesEmpresa { get; set; }

        [JsonPropertyName("dados_financeiros")]
        public DadosFinanceiros DadosFinanceiros { get; set; }

        [JsonPropertyName("conquistas")]
        public string[] Conquistas { get; set; }

        [JsonPropertyName("desafios")]
        public DesafiosEmpresa Desafios { get; set; }

        [JsonPropertyName("proximos_passos")]
        public ProximosPassosEmpresa ProximosPassos { get; set; }

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        [JsonPropertyName("classificacao_geral")]
        public ClassificacaoEmpresa ClassificacaoGeral { get; set; }

        [JsonPropertyName("score_financeiro")]
        public int ScoreFinanceiro { get; set; }

        public AnaliseDocumento()
        {
            InformacoesEmpresa = new InformacoesEmpresa();
            DadosFinanceiros = new DadosFinanceiros();
            Conquistas = new string[0]; // Compatível com .NET Standard 2.0
            Desafios = new DesafiosEmpresa();
            ProximosPassos = new ProximosPassosEmpresa();
            Periodo = string.Empty;
        }
    }

    public class InformacoesEmpresa
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("cnpj")]
        public string CNPJ { get; set; }

        [JsonPropertyName("ceo")]
        public string CEO { get; set; }

        public InformacoesEmpresa()
        {
            Nome = string.Empty;
            CNPJ = string.Empty;
            CEO = string.Empty;
        }
    }

    public class DadosFinanceiros
    {
        [JsonPropertyName("receita_total")]
        public decimal ReceitaTotal { get; set; }

        [JsonPropertyName("custos_operacionais")]
        public decimal CustosOperacionais { get; set; }

        [JsonPropertyName("lucro_liquido")]
        public decimal LucroLiquido { get; set; }

        [JsonPropertyName("margem_lucro")]
        public decimal MargemLucro { get; set; }
    }

    public enum ClassificacaoEmpresa
    {
        Startup,
        PME,
        Corporate,
        Enterprise
    }

    // Classes para análise de currículo
    public class AnaliseCurriculo
    {
        [JsonPropertyName("dados_pessoais")]
        public DadosPessoais DadosPessoais { get; set; }

        [JsonPropertyName("anos_experiencia")]
        public int AnosExperiencia { get; set; }

        [JsonPropertyName("experiencias")]
        public ExperienciaProfissional[] Experiencias { get; set; }

        [JsonPropertyName("habilidades_principais")]
        public string[] HabilidadesPrincipais { get; set; }

        [JsonPropertyName("nivel_senioridade")]
        public NivelSenioridade NivelSenioridade { get; set; }

        [JsonPropertyName("score_geral")]
        public int ScoreGeral { get; set; }

        public AnaliseCurriculo()
        {
            DadosPessoais = new DadosPessoais();
            Experiencias = new ExperienciaProfissional[0];
            HabilidadesPrincipais = new string[0];
        }
    }

    public class DadosPessoais
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("cargo_atual")]
        public string CargoAtual { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        public DadosPessoais()
        {
            Nome = string.Empty;
            CargoAtual = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
        }
    }

    public class ExperienciaProfissional
    {
        [JsonPropertyName("cargo")]
        public string Cargo { get; set; }

        [JsonPropertyName("empresa")]
        public string Empresa { get; set; }

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        public ExperienciaProfissional()
        {
            Cargo = string.Empty;
            Empresa = string.Empty;
            Periodo = string.Empty;
        }
    }

    public class DesafiosEmpresa
    {
        [JsonPropertyName("competicao")]
        public string Competicao { get; set; } = string.Empty;

        [JsonPropertyName("retencao_talentos")]
        public string RetencaoTalentos { get; set; } = string.Empty;

        [JsonPropertyName("investimento_marketing")]
        public string InvestimentoMarketing { get; set; } = string.Empty;

        // Propriedades adicionais para flexibilidade
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public class ProximosPassosEmpresa
    {
        [JsonPropertyName("programa_retencao")]
        public string ProgramaRetencao { get; set; } = string.Empty;

        [JsonPropertyName("investimento_pd")]
        public string InvestimentoPD { get; set; } = string.Empty;

        [JsonPropertyName("expandir_vendas")]
        public string ExpandirVendas { get; set; } = string.Empty;

        [JsonPropertyName("campanha_marketing")]
        public string CampanhaMarketing { get; set; } = string.Empty;

        // Propriedades adicionais para flexibilidade
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
    }

    public enum NivelSenioridade
    {
        Junior,
        Pleno,
        Senior,
        Especialista,
        Arquiteto
    }

    #endregion
}