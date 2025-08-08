namespace AgentSharp.Core
{
    /// <summary>
    /// Contains session information including user and session identifiers
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// The user identifier
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The session identifier  
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Indicates whether the IDs were automatically generated for anonymous session
        /// </summary>
        public bool WasGenerated { get; set; }

        /// <summary>
        /// Indicates whether this is an anonymous session
        /// </summary>
        public bool IsAnonymous => !string.IsNullOrEmpty(UserId) && UserId.StartsWith("anonymous_");
    }
}