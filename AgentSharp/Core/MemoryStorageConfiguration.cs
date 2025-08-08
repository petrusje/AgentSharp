using System;
using System.Collections.Generic;

namespace AgentSharp.Core
{
    /// <summary>
    /// Configuração para o sistema de memória e storage
    /// </summary>
    public class MemoryStorageConfiguration
    {
        /// <summary>
        /// Tipo de storage a ser utilizado
        /// </summary>
        public StorageType StorageType { get; set; } = StorageType.Json;

        /// <summary>
        /// String de conexão para o storage
        /// </summary>
        public string ConnectionString { get; set; } = "./data";

        /// <summary>
        /// Configurações específicas por tipo de storage
        /// </summary>
        public Dictionary<string, object> StorageOptions { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Configurações de memória
        /// </summary>
        public MemoryConfiguration Memory { get; set; } = new MemoryConfiguration();

        /// <summary>
        /// Configurações de auditoria
        /// </summary>
        public AuditConfiguration Audit { get; set; } = new AuditConfiguration();

        /// <summary>
        /// Configurações de cache
        /// </summary>
        public CacheConfiguration Cache { get; set; } = new CacheConfiguration();
    }

    /// <summary>
    /// Tipos de storage suportados
    /// </summary>
    public enum StorageType
    {
        Json,
        Sqlite,
        PostgreSql,
        MongoDB,
        InMemory
    }

    /// <summary>
    /// Tipos de recuperação de memória
    /// </summary>
    public enum MemoryRetrieval
    {
        Recent,
        Relevant,
        All,
        Summary
    }

    /// <summary>
    /// Configurações específicas de memória
    /// Como Quantidade máxima de memórias (sessão e usuário), max execuções, relevância mínima, auto summary, numero minimo para cria resumo.
    /// </summary>
    public class MemoryConfiguration
    {
        /// <summary>
        /// Limite máximo de memórias a serem mantidas por usuário
        /// </summary>
        public int MaxMemoriesPerUser { get; set; } = 1000;

        /// <summary>
        /// Limite máximo de mensagens por sessão
        /// </summary>
        public int MaxMessagesPerSession { get; set; } = 100;

        /// <summary>
        /// Limite máximo de runs por sessão
        /// </summary>
        public int MaxRunsPerSession { get; set; } = 50;

        /// <summary>
        /// Relevância mínima para uma memória ser considerada importante
        /// </summary>
        public double MinRelevanceThreshold { get; set; } = 0.5;

        /// <summary>
        /// Habilitar criação automática de resumos
        /// </summary>
        public bool EnableAutoSummary { get; set; } = true;

        /// <summary>
        /// Número de mensagens necessárias para criar um resumo
        /// </summary>
        public int SummaryThreshold { get; set; } = 20;
    }

    /// <summary>
    /// Configurações de auditoria
    /// </summary>
    public class AuditConfiguration
    {
        /// <summary>
        /// Habilitar sistema de auditoria
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Nível de auditoria (Minimal, Normal, Verbose)
        /// </summary>
        public AuditLevel Level { get; set; } = AuditLevel.Normal;

        /// <summary>
        /// Retenção de logs de auditoria em dias
        /// </summary>
        public int RetentionDays { get; set; } = 90;

        /// <summary>
        /// Habilitar exportação de logs
        /// </summary>
        public bool EnableExport { get; set; } = false;
    }

    /// <summary>
    /// Níveis de auditoria
    /// </summary>
    public enum AuditLevel
    {
        Minimal,
        Normal,
        Verbose
    }

    /// <summary>
    /// Configurações de cache
    /// </summary>
    public class CacheConfiguration
    {
        /// <summary>
        /// Habilitar cache
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Tempo de expiração do cache em minutos
        /// </summary>
        public int ExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Tamanho máximo do cache (número de itens)
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// Habilitar cache para consultas de memória
        /// </summary>
        public bool EnableMemoryCache { get; set; } = true;

        /// <summary>
        /// Habilitar cache para sessões
        /// </summary>
        public bool EnableSessionCache { get; set; } = true;
    }
}
