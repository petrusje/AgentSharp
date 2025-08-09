using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Exemplo demonstrando o OpenAIEmbeddingService refatorado
    /// usando o SDK oficial da OpenAI com integração ao modelo
    /// </summary>
    public static class ExemplosEmbedding
    {
        /// <summary>
        /// Demonstra o uso do OpenAIEmbeddingService refatorado
        /// usando as configurações do modelo OpenAI
        /// </summary>
        /// <param name="modelo">Instância do modelo OpenAI configurado</param>
        public static async Task ExecutarExemploEmbedding(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔗 EMBEDDINGS - OpenAI Embedding Service Refatorado");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.ResetColor();

            if (!(modelo is OpenAIModel openAIModel))
            {
                Console.WriteLine("❌ Este exemplo requer um OpenAIModel configurado");
                return;
            }

            try
            {
                // O embedding service agora é obtido diretamente do modelo OpenAI
                var embeddingService = openAIModel.GetEmbeddingService("text-embedding-3-small");

                Console.WriteLine("✅ OpenAIEmbeddingService obtido diretamente do modelo configurado\n");

                // Textos de exemplo para demonstração
                var textos = new List<string>
                {
                    "João prefere café forte pela manhã para começar bem o dia",
                    "Maria gosta de chá verde durante as tardes tranquilas",
                    "João adora todas as bebidas cafeinadas que lhe dão energia",
                    "Pedro trabalha melhor após tomar um bom café expresso",
                    "Ana prefere sucos naturais em vez de bebidas com cafeína"
                };

                Console.WriteLine("📝 Textos para análise:");
                for (int i = 0; i < textos.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {textos[i]}");
                }
                Console.WriteLine();

                // Gerar embeddings usando o SDK oficial
                Console.WriteLine("🔄 Gerando embeddings com SDK oficial da OpenAI...");
                var embeddings = await embeddingService.GenerateEmbeddingsAsync(textos);

                Console.WriteLine($"✅ {embeddings.Count} embeddings gerados com {embeddings[0].Count} dimensões cada\n");

                // Demonstrar cálculo de similaridades
                Console.WriteLine("📊 Análise de Similaridades:");
                Console.WriteLine("───────────────────────────");

                for (int i = 0; i < textos.Count; i++)
                {
                    for (int j = i + 1; j < textos.Count; j++)
                    {
                        var similaridade = embeddingService.CalculateSimilarity(embeddings[i], embeddings[j]);
                        var nivel = GetNivelSimilaridade(similaridade);

                        Console.WriteLine($"  📍 Texto {i + 1} vs Texto {j + 1}: {similaridade:F3} {nivel}");
                    }
                }

                // Busca por texto mais similar
                Console.WriteLine("\n🔍 Exemplo de Busca Semântica:");
                Console.WriteLine("─────────────────────────────");

                var consulta = "Quem gosta de bebidas com cafeína?";
                Console.WriteLine($"📋 Consulta: \"{consulta}\"");

                var embeddingConsulta = await embeddingService.GenerateEmbeddingAsync(consulta);

                var resultados = new List<(string texto, double similaridade)>();
                for (int i = 0; i < textos.Count; i++)
                {
                    var sim = embeddingService.CalculateSimilarity(embeddingConsulta, embeddings[i]);
                    resultados.Add((textos[i], sim));
                }

                // Ordenar por similaridade
                resultados.Sort((a, b) => b.similaridade.CompareTo(a.similaridade));

                Console.WriteLine("\n🎯 Resultados ordenados por relevância:");
                for (int i = 0; i < resultados.Count; i++)
                {
                    var emoji = i == 0 ? "🥇" : i == 1 ? "🥈" : i == 2 ? "🥉" : "📌";
                    Console.WriteLine($"  {emoji} {resultados[i].similaridade:F3} - {resultados[i].texto}");
                }

                Console.WriteLine($"\n✨ Exemplo concluído com sucesso usando modelo {modelo.GetType().Name}!");

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erro no exemplo de embedding: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Exemplo específico para demonstrar diferentes modelos de embedding
        /// </summary>
        public static async Task CompararModelosEmbedding(IModel modelo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚖️  COMPARAÇÃO - Diferentes Modelos de Embedding");
            Console.WriteLine("══════════════════════════════════════════════════");
            Console.ResetColor();

            if (!(modelo is OpenAIModel openAIModel))
            {
                Console.WriteLine("❌ Este exemplo requer um OpenAIModel configurado");
                return;
            }

            var modelos = new[]
            {
                "text-embedding-3-small",
                "text-embedding-3-large",
                "text-embedding-ada-002"
            };

            var textoTeste = "Inteligência artificial está revolucionando o desenvolvimento de software";

            foreach (var modeloEmbed in modelos)
            {
                try
                {
                    var service = openAIModel.GetEmbeddingService(modeloEmbed);
                    var embedding = await service.GenerateEmbeddingAsync(textoTeste);

                    Console.WriteLine($"📊 {modeloEmbed}:");
                    Console.WriteLine($"   - Dimensões: {embedding.Count}");
                    Console.WriteLine($"   - Primeiro valor: {embedding[0]:F6}");
                    Console.WriteLine($"   - Último valor: {embedding[embedding.Count - 1]:F6}");
                    Console.WriteLine();

                    service.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro com {modeloEmbed}: {ex.Message}");
                }
            }
        }

        #region Métodos Auxiliares

        private static string GetNivelSimilaridade(double similaridade)
        {
            if (similaridade >= 0.9) return "🔥 (Muito Alta)";
            if (similaridade >= 0.7) return "⭐ (Alta)";
            if (similaridade >= 0.5) return "➖ (Média)";
            if (similaridade >= 0.3) return "📉 (Baixa)";
            return "❄️ (Muito Baixa)";
        }

        #endregion
    }
}
