using System;

namespace AgentSharp.Core.Memory.Configuration
{
    /// <summary>
    /// Configuração unificada do sistema de memória
    /// Combina configurações de comportamento, prompts e storage
    /// </summary>
    public class MemoryConfiguration
    {
        // 🎯 CONFIGURAÇÕES DE PROMPTS/LLM
        /// <summary>
        /// Template customizado para extração de memórias - null = usar padrão
        /// </summary>
        public Func<string, string, string> ExtractionPromptTemplate { get; set; }
        
        /// <summary>
        /// Template customizado para classificação de memórias - null = usar padrão
        /// </summary>
        public Func<string, string> ClassificationPromptTemplate { get; set; }
        
        /// <summary>
        /// Template customizado para recuperação de memórias - null = usar padrão
        /// </summary>
        public Func<string, string, string> RetrievalPromptTemplate { get; set; }

        /// <summary>
        /// Categorias customizadas para classificação
        /// </summary>
        public string[] CustomCategories { get; set; }

        // 🎯 CONFIGURAÇÕES DE COMPORTAMENTO POR INTERAÇÃO
        /// <summary>
        /// Máximo de memórias extraídas por interação
        /// </summary>
        public int MaxMemoriesPerInteraction { get; set; } = 5;
        
        /// <summary>
        /// Threshold mínimo de importância (0.0 a 1.0)
        /// </summary>
        public double MinImportanceThreshold { get; set; } = 0.5;

        // 🎯 CONFIGURAÇÕES DE STORAGE/QUOTAS (Consolidadas)
        /// <summary>
        /// Limite máximo de memórias por usuário
        /// </summary>
        public int MaxMemoriesPerUser { get; set; } = 1000;
        
        /// <summary>
        /// Limite máximo de mensagens por sessão
        /// </summary>
        public int MaxMessagesPerSession { get; set; } = 100;
        
        /// <summary>
        /// Limite máximo de execuções por sessão
        /// </summary>
        public int MaxRunsPerSession { get; set; } = 50;
        
        /// <summary>
        /// Auto-resumir quando sessão exceder X mensagens
        /// </summary>
        public bool EnableAutoSummary { get; set; } = true;
        
        /// <summary>
        /// Número mínimo de mensagens para criar resumo
        /// </summary>
        public int MinMessagesForSummary { get; set; } = 20;
    }
}
