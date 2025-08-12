using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Models;

namespace AgentSharp.Examples
{
    /// <summary>
    /// Demonstra como usar SemanticSqliteStorage para busca vetorial de alta performance.
    /// Utiliza a extensão sqlite-vec - sucessor moderno e mais simples do sqlite-vss.
    /// </summary>
    public static class ExemplosVectorSQLiteVec
    {
    private const string EmbeddingModel = "text-embedding-3-small";
    private const string DistanceMetric = "cosine";
    private static string OpenAIModelName = "gpt-4";
        private static string OpenAIKey = "seu-api-key-aqui";

    public static async Task ExecutarExemploBasico()
    {
      Console.WriteLine("=== Exemplo VectorSqliteVec - Performance com sqlite-vec ===\n");

      // Configurar modelo OpenAI
      var openAIModel = new OpenAIModel(OpenAIModelName, OpenAIKey);
      var embeddingService = openAIModel.GetEmbeddingService();

      // Configurar storage com sqlite-vec (muito mais simples que sqlite-vss)
      var vecStorage = new SemanticSqliteStorage(
          connectionString: "Data Source=exemplo_vec.db",
          embeddingModel: EmbeddingModel,
          dimensions: 1536,
          distanceMetric: DistanceMetric  // cosine, l2, ou inner_product
      );

      // Dados de exemplo
      var documentos = new[]
      {
        "Machine learning é uma área da inteligência artificial",
        "Deep learning usa redes neurais profundas",
        "Natural language processing analisa texto",
        "Computer vision processa imagens",
        "Reinforcement learning aprende através de recompensas"
      };

      Console.WriteLine("1. Gerando embeddings...");
      var embeddings = new List<SemanticSqliteStorage.EmbeddingVector>();

      foreach (var doc in documentos)
      {
        var embedding = await embeddingService.GenerateEmbeddingAsync(doc);
        embeddings.Add(new SemanticSqliteStorage.EmbeddingVector
        {
          Content = doc,
          Vector = embedding.ToArray(),
          Metadata = new Dictionary<string, object>
          {
            ["categoria"] = "IA",
            ["fonte"] = "exemplo",
            ["timestamp"] = DateTime.UtcNow
          }
        });
      }

      Console.WriteLine($"Armazenando {embeddings.Count} vetores...");
      vecStorage.StoreEmbeddings(embeddings);

      Console.WriteLine($"\n2. Informações do índice:");
      Console.WriteLine(vecStorage.GetIndexInfo());

      // Buscar documentos similares
      Console.WriteLine("\n3. Buscando documentos similares...");
      var queryEmbedding = await embeddingService.GenerateEmbeddingAsync("aprendizado de máquina");

      var startTime = DateTime.UtcNow;
      var resultados = vecStorage.SearchSimilar(queryEmbedding.ToArray(), topK: 3);
      var elapsed = DateTime.UtcNow - startTime;

      Console.WriteLine($"\nResultados encontrados em {elapsed.TotalMilliseconds:F2}ms:");
      foreach (var resultado in resultados)
      {
        Console.WriteLine($"- Similaridade: {resultado.Similarity:F4}");
        Console.WriteLine($"  Conteúdo: {resultado.Content}");
        Console.WriteLine($"  Categoria: {resultado.Metadata?["categoria"]}");
        Console.WriteLine();
      }

      Console.WriteLine($"Total de vetores: {vecStorage.GetEmbeddingCount()}");
    }
        public static async Task ExecutarExemploComparacaoMetricas()
    {
      Console.WriteLine("=== Exemplo VectorSqliteVec - Comparação de Métricas ===\n");

      var openAIModel = new OpenAIModel(OpenAIModelName, "seu-api-key-aqui");
      var embeddingService = openAIModel.GetEmbeddingService();

      // Testar diferentes métricas de distância
      var metricas = new[] { DistanceMetric, "l2", "inner_product" };

      var documentos = new[]
      {
                "Inteligência artificial transforma indústrias",
                "Machine learning otimiza processos de negócio",
                "Deep learning revoluciona reconhecimento de padrões",
                "Redes neurais simulam o funcionamento do cérebro",
                "Algoritmos de IA automatizam tarefas complexas"
            };

      Console.WriteLine("1. Preparando embeddings...");
      var embeddings = new List<SemanticSqliteStorage.EmbeddingVector>();

      foreach (var doc in documentos)
      {
        var embedding = await embeddingService.GenerateEmbeddingAsync(doc);
        embeddings.Add(new SemanticSqliteStorage.EmbeddingVector
        {
          Content = doc,
          Vector = embedding.ToArray(),
          Metadata = new Dictionary<string, object>
          {
            ["categoria"] = "IA"
          }
        });
      }

      var query = await embeddingService.GenerateEmbeddingAsync("artificial intelligence");

      foreach (var metrica in metricas)
      {
        Console.WriteLine($"\n2. Testando métrica: {metrica.ToUpper()}");

        var vecStorage = new SemanticSqliteStorage(
            $"Data Source=exemplo_vec_{metrica}.db",
            EmbeddingModel,
            1536,
            metrica
        );

        vecStorage.StoreEmbeddings(embeddings);

        var inicio = DateTime.UtcNow;
        var resultados = vecStorage.SearchSimilar(query.ToArray(), topK: 3);
        var tempo = (DateTime.UtcNow - inicio).TotalMilliseconds;

        Console.WriteLine($"   Tempo de busca: {tempo:F2}ms");
        Console.WriteLine($"   Resultados:");

        for (int i = 0; i < Math.Min(3, resultados.Count); i++)
        {
          Console.WriteLine($"     {i + 1}. Similaridade: {resultados[i].Similarity:F4}");
          Console.WriteLine($"        Conteúdo: {resultados[i].Content}");
        }
      }

      Console.WriteLine("\n=== Recomendações de Métricas ===");
      Console.WriteLine("• cosine: Melhor para texto e embeddings normalizados (RECOMENDADO)");
      Console.WriteLine("• l2: Distância euclidiana, boa para vetores não-normalizados");
      Console.WriteLine("• inner_product: Para vetores já normalizados e otimizados");
    }

        public static async Task ExecutarExemploPerformanceEscala()
        {
            Console.WriteLine("=== Exemplo VectorSqliteVec - Teste de Performance e Escala ===\n");

            var openAIModel = new OpenAIModel(OpenAIModelName, OpenAIKey);
            var embeddingService = openAIModel.GetEmbeddingService();

            // Teste com diferentes tamanhos de dataset
            var tamanhos = new[] { 50, 200, 500 };

            foreach (var tamanho in tamanhos)
            {
                await TestarPerformanceParaTamanho(tamanho, embeddingService);
            }

            Console.WriteLine("\n=== CONCLUSÃO ===");
            Console.WriteLine("sqlite-vec oferece:");
            Console.WriteLine("• Performance consistente independente do tamanho");
            Console.WriteLine("• API muito mais simples que sqlite-vss");
            Console.WriteLine("• Sem necessidade de treinamento de índices");
            Console.WriteLine("• Suporte nativo a múltiplas métricas de distância");
            Console.WriteLine("• Gerenciamento automático de índices");
        }

        private static async Task TestarPerformanceParaTamanho(int tamanho, OpenAIEmbeddingService embeddingService)
        {
            Console.WriteLine($"\n=== Testando com {tamanho} vetores ===");

            var vecStorage = new SemanticSqliteStorage(
                $"Data Source=teste_escala_{tamanho}.db",
                EmbeddingModel,
                1536,
                DistanceMetric
            );

            // Gerar dados de teste
            var embeddings = await GerarEmbeddingsTeste(tamanho, embeddingService);

            // Teste de inserção
            Console.WriteLine("2. Testando inserção...");
            var inicioInsercao = DateTime.UtcNow;
            vecStorage.StoreEmbeddings(embeddings);
            var tempoInsercao = (DateTime.UtcNow - inicioInsercao).TotalMilliseconds;

            // Teste de busca
            var tempoMedioBusca = await TestarBuscaPerformance(embeddingService, vecStorage);

            // Rebuild index test
            Console.WriteLine("4. Testando rebuild de índice...");
            var inicioRebuild = DateTime.UtcNow;
            vecStorage.RebuildIndex();
            var tempoRebuild = (DateTime.UtcNow - inicioRebuild).TotalMilliseconds;

            // Resultados
            ExibirResultadosPerformance(tamanho, tempoInsercao, tempoMedioBusca, tempoRebuild, vecStorage);
        }

        private static async Task<List<SemanticSqliteStorage.EmbeddingVector>> GerarEmbeddingsTeste(int tamanho, OpenAIEmbeddingService embeddingService)
        {
            var embeddings = new List<SemanticSqliteStorage.EmbeddingVector>();
            Console.WriteLine($"1. Gerando {tamanho} embeddings...");

            for (int i = 0; i < tamanho; i++)
            {
                var texto = $"Documento de teste número {i} sobre inteligência artificial, " +
                          $"machine learning, deep learning e processamento de linguagem natural. " +
                          $"Este é um exemplo de conteúdo variado para teste de performance.";

                var embedding = await embeddingService.GenerateEmbeddingAsync(texto);
                embeddings.Add(new SemanticSqliteStorage.EmbeddingVector
                {
                    Content = texto,
                    Vector = embedding.ToArray(),
                    Metadata = new Dictionary<string, object>
                    {
                        ["indice"] = i,
                        ["categoria"] = i % 5 == 0 ? "especial" : "normal"
                    }
                });

                // Progresso a cada 50 items
                if ((i + 1) % 50 == 0)
                {
                    Console.WriteLine($"   Gerados {i + 1}/{tamanho} embeddings...");
                }
            }

            return embeddings;
        }

        private static async Task<double> TestarBuscaPerformance(OpenAIEmbeddingService embeddingService, SemanticSqliteStorage vecStorage)
        {
            Console.WriteLine("3. Testando busca...");
            var query = await embeddingService.GenerateEmbeddingAsync("machine learning artificial intelligence");

            var temposBusca = new List<double>();
            for (int i = 0; i < 10; i++) // 10 buscas para média
            {
                var inicioBusca = DateTime.UtcNow;
                vecStorage.SearchSimilar(query.ToArray(), topK: 10);
                var tempoBusca = (DateTime.UtcNow - inicioBusca).TotalMilliseconds;
                temposBusca.Add(tempoBusca);
            }

            return temposBusca.Sum() / temposBusca.Count;
        }

        private static void ExibirResultadosPerformance(int tamanho, double tempoInsercao, double tempoMedioBusca, double tempoRebuild, SemanticSqliteStorage vecStorage)
        {
            Console.WriteLine($"\n=== RESULTADOS PARA {tamanho} VETORES ===");
            Console.WriteLine($"Inserção: {tempoInsercao:F0}ms ({tempoInsercao/tamanho:F2}ms por vetor)");
            Console.WriteLine($"Busca (média): {tempoMedioBusca:F2}ms");
            Console.WriteLine($"Throughput: {1000/tempoMedioBusca:F0} consultas/segundo");
            Console.WriteLine($"Rebuild: {tempoRebuild:F0}ms");
            Console.WriteLine($"Total de vetores confirmados: {vecStorage.GetEmbeddingCount()}");

            // Info detalhada
            Console.WriteLine("\nInformações do índice:");
            Console.WriteLine(vecStorage.GetIndexInfo());
        }
        public static async Task ExecutarExemploCompleto()
        {
            Console.WriteLine("=== Exemplo Completo - sqlite-vec ===\n");

            var openAIModel = new OpenAIModel(OpenAIModelName, "seu-api-key-aqui");
            var embeddingService = openAIModel.GetEmbeddingService();

            // Configuração moderna com sqlite-vec
            var vecStorage = new SemanticSqliteStorage(
                "Data Source=exemplo_completo.db;Cache Size=50000;Journal Mode=WAL",
                EmbeddingModel,
                1536,
                DistanceMetric
            );

            // Dados de exemplo variados
            var documentos = new[]
            {
                "Machine learning é uma área da inteligência artificial que permite sistemas aprenderem automaticamente",
                "Deep learning utiliza redes neurais profundas para resolver problemas complexos de reconhecimento",
                "Natural language processing analisa e compreende texto em linguagem humana",
                "Computer vision processa e interpreta informações visuais de imagens e vídeos",
                "Reinforcement learning aprende através de interação com ambiente e sistema de recompensas",
                "Algoritmos genéticos usam princípios de evolução para otimização de soluções",
                "Redes neurais convolucionais são especializadas em processamento de imagens",
                "Transformers revolucionaram o processamento de linguagem natural com mecanismos de atenção",
                "Transfer learning reutiliza modelos pré-treinados para novas tarefas específicas",
                "Federated learning permite treinamento distribuído mantendo privacidade dos dados"
            };

            Console.WriteLine($"Processando {documentos.Length} documentos...");
            var embeddings = new List<SemanticSqliteStorage.EmbeddingVector>();

            foreach (var (doc, idx) in documentos.Select((d, i) => (d, i)))
            {
                var embedding = await embeddingService.GenerateEmbeddingAsync(doc);
                embeddings.Add(new SemanticSqliteStorage.EmbeddingVector
                {
                    Content = doc,
                    Vector = embedding.ToArray(),
                    Metadata = new Dictionary<string, object>
                    {
                        ["categoria"] = GetCategoria(doc),
                        ["indice"] = idx,
                        ["timestamp"] = DateTime.UtcNow,
                        ["tokens"] = doc.Split(' ').Length
                    }
                });
            }

            // Armazenar todos os vetores
            vecStorage.StoreEmbeddings(embeddings);

            // Várias consultas de teste
            var consultas = new[]
            {
                "aprendizado de máquina",
                "processamento de imagens",
                "redes neurais",
                "otimização algoritmos"
            };

            foreach (var consulta in consultas)
            {
                Console.WriteLine($"\n🔍 Buscando: '{consulta}'");
                var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(consulta);

                var inicio = DateTime.UtcNow;
                var resultados = vecStorage.SearchSimilar(queryEmbedding.ToArray(), 3);
                var tempo = (DateTime.UtcNow - inicio).TotalMilliseconds;

                Console.WriteLine($"   Encontrado em {tempo:F2}ms:");

                foreach (var resultado in resultados)
                {
                    Console.WriteLine($"   • Similaridade: {resultado.Similarity:F3}");
                    Console.WriteLine($"     Categoria: {resultado.Metadata?["categoria"]}");
                    Console.WriteLine($"     Conteúdo: {resultado.Content.Substring(0, Math.Min(80, resultado.Content.Length))}...");
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"\n📊 Estatísticas:");
            Console.WriteLine($"   Total de vetores: {vecStorage.GetEmbeddingCount()}");
            Console.WriteLine($"   Informações do índice:");
            Console.WriteLine($"   {vecStorage.GetIndexInfo()}");

            Console.WriteLine("\n✅ Exemplo completo executado com sucesso!");
        }

        private static string GetCategoria(string documento)
        {
            if (documento.Contains("learning") || documento.Contains("aprendizado"))
                return "Aprendizado";
            if (documento.Contains("neural") || documento.Contains("redes"))
                return "Redes Neurais";
            if (documento.Contains("vision") || documento.Contains("imagem"))
                return "Visão Computacional";
            if (documento.Contains("language") || documento.Contains("linguagem"))
                return "NLP";
            return "IA Geral";
        }
    }
}
