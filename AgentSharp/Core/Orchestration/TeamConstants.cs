namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Constants for team orchestration functionality.
    /// Centralizes magic numbers and strings for better maintainability.
    /// </summary>
    public static class TeamConstants
    {
        /// <summary>
        /// Default length for generated team IDs
        /// </summary>
        public const int DefaultTeamIdLength = 8;

        /// <summary>
        /// Default user ID when none is provided
        /// </summary>
        public const string DefaultUserId = "team_user";

        /// <summary>
        /// Session prefix for team operations
        /// </summary>
        public const string TeamSessionPrefix = "team_";

        /// <summary>
        /// Session suffix for shared team memory
        /// </summary>
        public const string SharedSessionSuffix = "_shared";

        /// <summary>
        /// Session suffix for team communications
        /// </summary>
        public const string CommunicationsSessionSuffix = "_communications";

        /// <summary>
        /// Session suffix for team handoffs
        /// </summary>
        public const string HandoffSessionSuffix = "_handoff";

        /// <summary>
        /// Session suffix for team consultations
        /// </summary>
        public const string ConsultationSessionSuffix = "_consultation";

        /// <summary>
        /// Session suffix for team status updates
        /// </summary>
        public const string StatusSessionSuffix = "_status";

        /// <summary>
        /// Session suffix for team snapshots
        /// </summary>
        public const string SnapshotSessionSuffix = "_snapshots";

        /// <summary>
        /// Session suffix for team initialization
        /// </summary>
        public const string InitSessionSuffix = "_init";

        /// <summary>
        /// Maximum number of recent communications to retrieve by default
        /// </summary>
        public const int DefaultRecentCommunicationsLimit = 50;

        /// <summary>
        /// Default batch size for memory operations
        /// </summary>
        public const int DefaultMemoryBatchSize = 100;

        /// <summary>
        /// Default retention period in days for team communications
        /// </summary>
        public const int DefaultRetentionDays = 30;

        /// <summary>
        /// Date time format for logging
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    }
}