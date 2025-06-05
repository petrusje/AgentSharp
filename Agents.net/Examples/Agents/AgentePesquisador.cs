using System;
using Agents.net.Core;
using Agents.net.Models;
using Agents.net.Attributes;

namespace Agents.net.Examples
{
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
• Participação na Global Partnership AI";
        }

        [FunctionCall("Pesquisar políticas públicas e regulamentação")]
        [FunctionCallParameter("tema", "Área específica de política pública")]
        private string PesquisarPoliticasPublicas(string tema)
        {
            return $@"
🏛️ POLÍTICAS PÚBLICAS: {tema.ToUpper()}
══════════════════════════════════

📋 MARCO LEGAL:
• Marco Regulatório Brasileiro de IA (2023)
• Lei Geral de Proteção de Dados (LGPD)
• Marco Civil da Internet
• Decreto 10.332 - Estratégia Digital BR

🔍 ÓRGÃOS REGULADORES:
• MCTI - Coordenação geral
• ANPD - Proteção de dados
• CGI.br - Governança da internet
• FAPESP/CNPq/FINEP - Financiamento

📊 PROGRAMAS ESPECÍFICOS:
• PNI (Programa Nacional de IA)
• Pro-IA (Embrapa)
• Programa Brasil Mais
• Startup Brasil

🌐 RECURSOS DISPONÍVEIS:
• Linhas de crédito BNDES - R$ 600M
• Editais de inovação - R$ 250M/ano
• Benefícios fiscais Lei do Bem
• Bolsas específicas para IA

⚖️ REGULAMENTAÇÃO EM DESENVOLVIMENTO:
• Ética em IA - Em discussão
• Responsabilidade algorítmica
• Propriedade intelectual para IA
• Princípios de transparência";
        }
    }
}
