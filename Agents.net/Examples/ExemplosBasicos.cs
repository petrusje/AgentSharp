using System;
using System.Threading.Tasks;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Tools;
using Agents.net.Attributes;
using System.Runtime.InteropServices;
using OpenAI.RealtimeConversation;

namespace Agents.net.Examples
{
    /// <summary>
    /// Exemplos básicos demonstrando personality-driven agents,
    /// agents com ferramentas e análise financeira no contexto de Belo Horizonte
    /// </summary>
    public static class ExemplosBasicos
    {
        /// <summary>
        /// Demonstra um agente com personalidade distinta (jornalista mineiro)
        /// Contexto regional de Belo Horizonte
        /// </summary>
        public static async Task ExecutarJornalistaMineiro(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⛰️  EXEMPLO 1: JORNALISTA MINEIRO - PERSONALITY-DRIVEN AGENT");
            Console.WriteLine("════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📄 Contexto regional de Belo Horizonte e Minas Gerais");
            Console.WriteLine("Demonstra como criar um agente com personalidade distinta\n");


            var contexto = new JornalistaMineiroContext
            {
                RegiaoFoco = "Belo Horizonte",
                IdiomaPreferido = "pt-BR",
                UltimaAtualizacao = DateTime.Now,
                seuNome = "Mauricio Mauro"
            };

            // Agente com personalidade de jornalista mineiro de BH
            var jornalista = new Agent<JornalistaMineiroContext, string>(modelo, "JornalistaMineiro")
                .WithPersona(ctx => @"
Você é um repórter mineiro com talento para contar histórias! ⛰️
Pense em si mesmo como uma mistura entre um contador de causos e um jornalista afiado.

SEU GUIA DE ESTILO:
- Comece com uma manchete chamativa usando emoji apropriados
- Compartilhe notícias com o jeito mineiro d/e ser: acolhedor e inteligente
- Mantenha suas respostas concisas mas envolventes (2-3 parágrafos)
- Use referências locais de BH e expressões mineiras quando apropriado
- Termine com uma assinatura marcante como 'Uai, sô!' ou 'Reportando da terra do pão de açúcar!'

Lembre-se de verificar todos os fatos enquanto mantém essa hospitalidade mineira!
Seja criativo mas responsável no jornalismo!
Seu nome é {ctx.seuNome}, um repórter apaixonado por Belo Horizonte e Minas Gerais.")
                .WithContext(contexto)
                .WithInstructions("Seja criativo, envolvente e mantenha o estilo jornalístico mineiro. Use emojis apropriados.")
                .WithGuardRails(" Nunca fale direto da terra do pão de açucar, pois Minas é terra dp Pão de Queijo!");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Pergunta: 'Me conte sobre uma história interessante que poderia estar acontecendo na Praça da Liberdade agora'");
            Console.ResetColor();
            Console.WriteLine("\n📻 Resposta do Jornalista:");
            Console.WriteLine(new string('-', 50));

            try
            {
                var resultado = await jornalista.ExecuteAsync(
                    "Me conte sobre uma história interessante que poderia estar acontecendo na Praça da Liberdade em Belo Horizonte agora"
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"⏱️  Tempo: Não disponível");



                // Exemplo adicional
                Console.WriteLine("\n🔄 Testando outra pergunta com busca duckduckgo...\n");


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🔥 Pergunta: 'Qual seria a última tendência gastronômica na Savassi?'");
                Console.ResetColor();

                //adicionando ferramentas de busca web
                jornalista.WithTools(new SearchToolPack());

                var resultado2 = await jornalista.ExecuteAsync(
                    "Qual seria a última tendência gastronômica no bairro Savassi em Belo Horizonte? Pesquise na web por informações atuais."
                );

                Console.WriteLine("\n📻 Resposta do Jornalista:");
                Console.WriteLine(new string('-', 50));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado2.Data);
                resultado2.Tools.ForEach(tool =>
                    Console.WriteLine($"🔧 Ferramenta utilizada: {tool.Name}"));
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Demonstra um agente com ferramentas de busca web
        /// Contexto regional de Belo Horizonte
        /// </summary>
        public static async Task ExecutarReporterComFerramentas(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔍 EXEMPLO 2: REPÓRTER COM WEB SEARCH CUSTOMIZADA - AGENT WITH TOOLS");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📄 Contexto de reportagem em Belo Horizonte");
            Console.WriteLine("Demonstra como adicionar ferramentas de busca web ao agente\n");

            var contexto = new JornalistaContext
            {
                RegiaoFoco = "Belo Horizonte",
                IdiomaPreferido = "pt-BR"
            };

            // Agente com ferramentas de busca
            var reporterBusca = new JornalistaComBusca(modelo)
                .WithContext(contexto)
                .WithTools(new SearchToolPack());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Pergunta: 'Quais são as últimas notícias sobre tecnologia no Brasil?'");
            Console.ResetColor();
            Console.WriteLine("\n📻 Resposta do Jornalista (com busca web):");
            Console.WriteLine(new string('-', 50));

            try
            {
                var resultado = await reporterBusca.ExecuteAsync(
                    "Quais são as últimas notícias sobre tecnologia em Belo Horizonte e Minas Gerais?"
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
                Console.WriteLine($"⏱️  Tempo: Não disponível");

                // Mostrar quais ferramentas foram usadas
                if (resultado.Tools.Count > 0)
                {
                    Console.WriteLine("\n🛠️  Ferramentas utilizadas:");
                    foreach (var tool in resultado.Tools)
                    {
                        Console.WriteLine($"   🔹 {tool.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }

            // Exemplo adicional
            Console.WriteLine("\n🔄 Testando outra pergunta...\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Pergunta: 'Quais são as últimas inovações em inteligência artificial no Brasil?'");
            Console.ResetColor();
        }

        /// <summary>
        /// Demonstra análise financeira com dados estruturados
        /// Contexto de mercado financeiro em Minas Gerais
        /// </summary>
        public static async Task ExecutarAnalistaFinanceiroBH(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📈 EXEMPLO 3: ANALISTA FINANCEIRO BH - FINANCE DATA AGENT");
            Console.WriteLine("═════════════════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("📄 Análise de mercado financeiro em Minas Gerais");
            Console.WriteLine("Demonstra análise financeira com dados estruturados\n");

            var contexto = new AnaliseFinanceiraContext
            {
                FocoMercado = "Minas Gerais",
                TipoAnalise = "Investimento",
                PeriodoAnalise = "Últimos 3 meses"
            };

            var analistaFinanceiro = new AnalistaFinanceiro(modelo)
                .WithContext(contexto);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔥 Pergunta: 'Faça uma análise das ações da Petrobras (PETR4) considerando o cenário atual'");
            Console.ResetColor();
            Console.WriteLine("\n📊 Análise do Especialista:");
            Console.WriteLine(new string('-', 50));

            try
            {
                var resultado = await analistaFinanceiro.ExecuteAsync(
                    "Faça uma análise das ações da Cemig (CMIG4) considerando o cenário atual do mercado mineiro e setor elétrico"
                );

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(resultado.Data);
                Console.ResetColor();

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"📊 Tokens utilizados: {resultado.Usage.TotalTokens}");
                Console.WriteLine($"🔧 Ferramentas chamadas: {resultado.Tools.Count}");
                Console.WriteLine($"⏱️  Tempo: Não disponível");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    #region Classes de Contexto e Agentes Especializados

    public class JornalistaContext
    {
        public string RegiaoFoco { get; set; } = "Belo Horizonte";
        public string IdiomaPreferido { get; set; } = "pt-BR";
        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;
    }

    public class AnaliseFinanceiraContext
    {
        public string FocoMercado { get; set; } = "Minas Gerais";
        public string TipoAnalise { get; set; } = "Investimento";
        public string PeriodoAnalise { get; set; } = "Últimos 30 dias";
        public DateTime DataAnalise { get; set; } = DateTime.Now;
    }

    public class JornalistaComBusca : Agent<JornalistaContext, string>
    {
        public JornalistaComBusca(IModel model)
            : base(model,
                   name: "JornalistaBuscaWeb",
                   instructions: @"
Você é um repórter entusiasta que pesquisa notícias reais na web! 🔍📰

DIRETRIZES PARA CADA REPORTAGEM:
1. Comece com uma manchete chamativa usando emojis relevantes
2. Use a ferramenta de busca para encontrar informações atuais e precisas
3. Apresente notícias com autêntico entusiasmo e sabor local
4. Estruture suas reportagens em seções claras:
   - Manchete marcante
   - Resumo breve da notícia
   - Detalhes-chave e citações
   - Impacto local ou contexto
5. Mantenha respostas concisas mas informativas (2-3 parágrafos máx)
6. Inclue comentários no estilo local e referências regionais
7. Termine com uma frase de assinatura

EXEMPLOS DE ASSINATURA:
- 'De volta para vocês no estúdio, pessoal!'
- 'Reportando ao vivo da terra que nunca para!'
- 'Aqui é [Seu Nome], direto do coração do Brasil!'

Lembre-se: Sempre verifique fatos através de buscas web e mantenha essa energia autêntica!")
        {
        }

        [FunctionCall("Busca informações atuais na web sobre um tópico específico")]
        [FunctionCallParameter("consulta", "Termos de busca para encontrar informações atuais")]
        [FunctionCallParameter("regiao", "Região ou país para focar a busca (opcional)")]
        private string BuscarNoticias(string consulta, string regiao = "")
        {
            // Simulação de busca web - em produção, integraria com API real de busca
            var resultados = new[]
            {
                "🔥 BREAKING: Startup mineira de IA recebe investimento de R$ 30M em BH",
                "📱 Nova funcionalidade desenvolvida em centro tech de Belo Horizonte",
                "🚀 Empresa de tecnologia de BH está expandindo para interior de MG",
                "💡 IA revoluciona setor bancário com solução desenvolvida na capital",
                "🏢 Centro de desenvolvimento tech será inaugurado no Savassi"
            };

            var resultadoSelecionado = resultados[new Random().Next(resultados.Length)];

            return $@"
📊 RESULTADOS DA BUSCA WEB:
Consulta: {consulta}
Região: {(string.IsNullOrEmpty(regiao) ? Context.RegiaoFoco : regiao)}

🎯 PRINCIPAIS NOTÍCIAS ENCONTRADAS:
• {resultadoSelecionado}
• Setor tech mineiro cresce 18% no último trimestre
• Investimentos em startups de BH batem recorde em 2024
• Governo de MG anuncia incentivos para inovação

📅 Última atualização: {DateTime.Now:HH:mm} - {DateTime.Now:dd/MM/yyyy}
🔍 Fonte: Agregador de notícias tecnológicas";
        }
    }

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

    #endregion

    #region Classe de contexto para agente jornalista
    class JornalistaMineiroContext
    {
        public string RegiaoFoco { get; set; } = "Belo Horizonte";
        public string IdiomaPreferido { get; set; } = "pt-BR";
        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;

        public string seuNome { get; set; } = "Mauricio Mauro";
    }
    #endregion
} 