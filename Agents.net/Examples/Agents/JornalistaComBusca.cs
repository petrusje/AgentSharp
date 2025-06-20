using Arcana.AgentsNet.Attributes;
using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Examples.Contexts;
using Arcana.AgentsNet.Models;
using System;

namespace Arcana.AgentsNet.Examples.Agents
{
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
}
