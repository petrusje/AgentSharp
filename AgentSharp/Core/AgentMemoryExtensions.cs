using System;

namespace AgentSharp.Core
{
    /// <summary>
    /// Extensões fluentes para configuração customizada do sistema de memória de agentes
    /// </summary>
    public static class AgentMemoryExtensions
    {
        /// <summary>
        /// Configura o sistema de memória com uma configuração de domínio customizada
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="config">Configuração de domínio customizada</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryDomainConfiguration<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            MemoryDomainConfiguration config)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            return agent.SetMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura um template customizado para extração de memórias
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="promptTemplate">Template que recebe (userMessage, assistantMessage) e retorna o prompt customizado</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryExtraction<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            Func<string, string, string> promptTemplate)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (promptTemplate == null) throw new ArgumentNullException(nameof(promptTemplate));

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.ExtractionPromptTemplate = promptTemplate;
            return agent.WithMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura um template customizado para classificação de memórias
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="promptTemplate">Template que recebe o conteúdo da memória e retorna o prompt de classificação</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryClassification<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            Func<string, string> promptTemplate)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (promptTemplate == null) throw new ArgumentNullException(nameof(promptTemplate));

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.ClassificationPromptTemplate = promptTemplate;
            return agent.WithMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura um template customizado para recuperação de memórias
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="promptTemplate">Template que recebe (query, existingMemories) e retorna o prompt de recuperação</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryRetrieval<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            Func<string, string, string> promptTemplate)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (promptTemplate == null) throw new ArgumentNullException(nameof(promptTemplate));

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.RetrievalPromptTemplate = promptTemplate;
            return agent.WithMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura categorias customizadas para classificação de memórias
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="categories">Array de categorias customizadas</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryCategories<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            params string[] categories)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (categories == null || categories.Length == 0) 
                throw new ArgumentException("At least one category must be provided", nameof(categories));

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.CustomCategories = categories;
            return agent.WithMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura thresholds e limites para o sistema de memória
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="maxMemories">Número máximo de memórias por interação</param>
        /// <param name="minImportance">Threshold mínimo de importância (0.0 a 1.0)</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryThresholds<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            int maxMemories = 5, 
            double minImportance = 0.5)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (maxMemories <= 0) throw new ArgumentOutOfRangeException(nameof(maxMemories), "Must be greater than 0");
            if (minImportance < 0.0 || minImportance > 1.0) 
                throw new ArgumentOutOfRangeException(nameof(minImportance), "Must be between 0.0 and 1.0");

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.MaxMemoriesPerInteraction = maxMemories;
            config.MinImportanceThreshold = minImportance;
            return agent.WithMemoryDomainConfiguration(config);
        }

        /// <summary>
        /// Configura estratégias avançadas de memória
        /// </summary>
        /// <param name="agent">O agente a ser configurado</param>
        /// <param name="prioritizeRecent">Se deve priorizar memórias recentes</param>
        /// <param name="enableSemanticGrouping">Se deve habilitar agrupamento semântico</param>
        /// <returns>O agente configurado</returns>
        public static Agent<TContext, TResult> WithMemoryStrategies<TContext, TResult>(
            this Agent<TContext, TResult> agent, 
            bool prioritizeRecent = true, 
            bool enableSemanticGrouping = false)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            var config = agent.GetMemoryDomainConfiguration()?.Clone() ?? new MemoryDomainConfiguration();
            config.PrioritizeRecentMemories = prioritizeRecent;
            config.EnableSemanticGrouping = enableSemanticGrouping;
            return agent.WithMemoryDomainConfiguration(config);
        }
    }
}