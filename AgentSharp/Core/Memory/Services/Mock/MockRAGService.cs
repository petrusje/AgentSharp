using AgentSharp.Core.Memory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Services.Mock
{
    /// <summary>
    /// Serviço RAG mockado para demonstração e testes
    /// Simula busca semântica com dataset fixo
    /// </summary>
    public class MockRAGService : IKnowledgeService
    {
        private readonly List<KnowledgeDocument> _documents;
        
        public bool IsAvailable => true;
        
        public MockRAGService()
        {
            _documents = CreateMockDataset();
        }
        
        public async Task<List<KnowledgeDocument>> SearchAsync(string query, int limit = 5)
        {
            await Task.Delay(50); // Simular latência de rede
            
            if (string.IsNullOrWhiteSpace(query))
                return new List<KnowledgeDocument>();
            
            // Busca simples por palavras-chave
            var keywords = query.ToLowerInvariant().Split(' ');
            
            var results = _documents
                .Select(doc => new
                {
                    Document = doc,
                    Relevance = CalculateRelevance(doc, keywords)
                })
                .Where(x => x.Relevance > 0)
                .OrderByDescending(x => x.Relevance)
                .Take(limit)
                .Select(x => new KnowledgeDocument
                {
                    Id = x.Document.Id,
                    Content = x.Document.Content,
                    Title = x.Document.Title,
                    Source = x.Document.Source,
                    Relevance = x.Relevance,
                    Metadata = x.Document.Metadata
                })
                .ToList();
            
            return results;
        }
        
        private double CalculateRelevance(KnowledgeDocument doc, string[] keywords)
        {
            var content = (doc.Title + " " + doc.Content).ToLowerInvariant();
            var matches = keywords.Count(keyword => content.Contains(keyword));
            return (double)matches / keywords.Length;
        }
        
        private List<KnowledgeDocument> CreateMockDataset()
        {
            return new List<KnowledgeDocument>
            {
                new KnowledgeDocument
                {
                    Id = "doc-001",
                    Title = "Configuração de Memória Semântica",
                    Content = "Para configurar memória semântica no AgentSharp, use o método WithSemanticMemory com um IStorage configurado. Você pode escolher entre SemanticMemoryStorage (HNSW in-memory) para desenvolvimento ou SemanticSqliteStorage para produção.",
                    Source = "Documentação AgentSharp",
                    Metadata = { ["category"] = "configuration", ["type"] = "tutorial" }
                },
                new KnowledgeDocument
                {
                    Id = "doc-002", 
                    Title = "Performance HNSW vs SQLite",
                    Content = "SemanticMemoryStorage (HNSW) oferece busca O(log n) com inicialização instantânea, ideal para desenvolvimento. SemanticSqliteStorage oferece persistência com sqlite-vec, melhor para produção com grandes volumes.",
                    Source = "Guia de Performance",
                    Metadata = { ["category"] = "performance", ["type"] = "comparison" }
                },
                new KnowledgeDocument
                {
                    Id = "doc-003",
                    Title = "Controles de Custo",
                    Content = "Use WithUserMemories(true) para habilitar extração de memórias pela LLM. Use WithMemorySearch(true) para permitir busca semântica. Estes controles permitem economia de tokens quando não necessários.",
                    Source = "Best Practices",
                    Metadata = { ["category"] = "cost-optimization", ["type"] = "guide" }
                },
                new KnowledgeDocument
                {
                    Id = "doc-004",
                    Title = "Configuração para Produção",
                    Content = "Em produção, recomenda-se usar SemanticSqliteStorage com sqlite-vec extension. Configure connection pooling e índices adequados para melhor performance em cenários de alta concorrência.",
                    Source = "Production Guide",
                    Metadata = { ["category"] = "production", ["type"] = "setup" }
                },
                new KnowledgeDocument
                {
                    Id = "doc-005",
                    Title = "Integração com RAG",
                    Content = "O AgentSharp permite integração com serviços RAG externos via IKnowledgeService. Use WithKnowledgeSearch(true) para habilitar busca em bases de conhecimento customizadas.",
                    Source = "Advanced Integration",
                    Metadata = { ["category"] = "integration", ["type"] = "advanced" }
                },
                new KnowledgeDocument
                {
                    Id = "doc-006",
                    Title = "Troubleshooting Comum",
                    Content = "Problemas comuns incluem: sqlite-vec não encontrado (verificar instalação), performance lenta (criar índices), uso excessivo de memória (usar CompactHNSW para desenvolvimento).",
                    Source = "Troubleshooting Guide",
                    Metadata = { ["category"] = "troubleshooting", ["type"] = "solutions" }
                }
            };
        }
    }
}