namespace AgentSharp.Core.Memory.Models
{
    // Removido: enum MessageRole. Usar AgentSharp.Core.Role

    /// <summary>
    /// Tipos de recuperação de memória
    /// </summary>
    public enum MemoryRetrieval
    {
        /// <summary>
        /// Últimas N memórias
        /// </summary>
        LastN,
        /// <summary>
        /// Primeiras N memórias
        /// </summary>
        FirstN,
        /// <summary>
        /// Busca semântica
        /// </summary>
        Semantic,
        /// <summary>
        /// Todas as memórias
        /// </summary>
        All,
        /// <summary>
        /// Apenas resumo
        /// </summary>
        Summary
    }

    /// <summary>
    /// Tipos de memória do agente
    /// </summary>
    public enum AgentMemoryType
    {
        /// <summary>
        /// Fato ou informação
        /// </summary>
        Fact,
        /// <summary>
        /// Preferência do usuário
        /// </summary>
        Preference,
        /// <summary>
        /// Contexto da conversa
        /// </summary>
        Conversation,
        /// <summary>
        /// Tarefa ou objetivo
        /// </summary>
        Task,
        /// <summary>
        /// Contexto geral
        /// </summary>
        Context,
        /// <summary>
        /// Instrução
        /// </summary>
        Instruction,
        /// <summary>
        /// Feedback
        /// </summary>
        Feedback,
        /// <summary>
        /// Pergunta
        /// </summary>
        Question,
        /// <summary>
        /// Resposta
        /// </summary>
        Answer,
        /// <summary>
        /// Outros tipos
        /// </summary>
        Other
    }
}
