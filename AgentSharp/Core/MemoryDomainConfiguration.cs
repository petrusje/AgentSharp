using System;

namespace AgentSharp.Core
{
    /// <summary>
    /// Configuração para customização do sistema de memória por domínio específico.
    /// Permite que desenvolvedores adaptem a extração, classificação e recuperação de memórias
    /// para contextos específicos como medicina, direito, tecnologia, etc.
    /// </summary>
    public class MemoryDomainConfiguration
    {
        /// <summary>
        /// Template customizado para extração de memórias a partir de conversas.
        /// Se null, usa o prompt padrão do sistema.
        /// Parâmetros: (userMessage, assistantMessage) => customPrompt
        /// </summary>
        public Func<string, string, string> ExtractionPromptTemplate { get; set; }

        /// <summary>
        /// Template customizado para classificação de memórias em categorias.
        /// Se null, usa a classificação padrão do sistema.
        /// Parâmetro: (memoryContent) => customPrompt
        /// </summary>
        public Func<string, string> ClassificationPromptTemplate { get; set; }

        /// <summary>
        /// Template customizado para recuperação de memórias relevantes.
        /// Se null, usa a estratégia padrão do sistema.
        /// Parâmetros: (query, existingMemories) => customPrompt
        /// </summary>
        public Func<string, string, string> RetrievalPromptTemplate { get; set; }

        /// <summary>
        /// Categorias customizadas para classificação de memórias.
        /// Se null ou vazia, usa as categorias padrão do sistema.
        /// </summary>
        public string[] CustomCategories { get; set; }

        /// <summary>
        /// Número máximo de memórias a serem extraídas por interação.
        /// Padrão: 5
        /// </summary>
        public int MaxMemoriesPerInteraction { get; set; } = 5;

        /// <summary>
        /// Threshold mínimo de importância para uma memória ser armazenada.
        /// Valores de 0.0 a 1.0. Padrão: 0.5
        /// </summary>
        public double MinImportanceThreshold { get; set; } = 0.5;

        /// <summary>
        /// Se deve priorizar memórias mais recentes na recuperação.
        /// Padrão: true
        /// </summary>
        public bool PrioritizeRecentMemories { get; set; } = true;

        /// <summary>
        /// Se deve habilitar agrupamento semântico de memórias relacionadas.
        /// Padrão: false (funcionalidade experimental)
        /// </summary>
        public bool EnableSemanticGrouping { get; set; } = false;

        /// <summary>
        /// Cria uma nova instância com configurações padrão
        /// </summary>
        public MemoryDomainConfiguration() { }

        /// <summary>
        /// Cria uma cópia desta configuração
        /// </summary>
        /// <returns>Nova instância com os mesmos valores</returns>
        public MemoryDomainConfiguration Clone()
        {
            return new MemoryDomainConfiguration
            {
                ExtractionPromptTemplate = this.ExtractionPromptTemplate,
                ClassificationPromptTemplate = this.ClassificationPromptTemplate,
                RetrievalPromptTemplate = this.RetrievalPromptTemplate,
                CustomCategories = this.CustomCategories?.Clone() as string[],
                MaxMemoriesPerInteraction = this.MaxMemoriesPerInteraction,
                MinImportanceThreshold = this.MinImportanceThreshold,
                PrioritizeRecentMemories = this.PrioritizeRecentMemories,
                EnableSemanticGrouping = this.EnableSemanticGrouping
            };
        }
    }
}