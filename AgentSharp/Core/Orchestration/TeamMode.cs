namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Defines the different modes of operation for agent teams
    /// </summary>
    public enum TeamMode
    {
        /// <summary>
        /// Coordinate mode: One coordinator agent manages and delegates tasks to other team members
        /// The coordinator handles task distribution and result aggregation
        /// </summary>
        Coordinate,

        /// <summary>
        /// Route mode: Tasks are intelligently routed to the most appropriate agent
        /// based on capabilities, performance, or custom routing logic
        /// </summary>
        Route,

        /// <summary>
        /// Collaborate mode: Multiple agents work together in parallel on the same task
        /// Results are synthesized from all participating agents
        /// </summary>
        Collaborate
    }
}