using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Orchestration;

namespace AgentSharp.Core.Memory
{
    /// <summary>
    /// Extension methods for IMemoryManager to support team-based memory sharing and workflow integration.
    /// Provides functionality for shared context management, team memory coordination, and workflow-specific memory operations.
    /// </summary>
    public static class MemoryManagerExtensions
    {
        /// <summary>
        /// Gets shared context for a team, allowing multiple agents to access the same memory space.
        /// </summary>
        /// <typeparam name="TContext">Type of context to retrieve</typeparam>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Unique identifier for the team</param>
        /// <param name="userId">User identifier</param>
        /// <param name="contextKey">Key for the specific context data</param>
        /// <returns>The shared context object, or default if not found</returns>
        public static async Task<TContext> GetSharedContext<TContext>(
            this IMemoryManager memoryManager,
            string teamId,
            string userId = null,
            string contextKey = "shared_context")
        {
            if (string.IsNullOrEmpty(teamId))
                throw new ArgumentException("Team ID cannot be null or empty", nameof(teamId));

            try
            {
                var sessionId = $"team_{teamId}_shared";
                var context = await memoryManager.LoadContextAsync(userId ?? "team_user", sessionId);

                // Try to get the context data from the memory context settings
                if (context?.Settings != null && context.Settings.TryGetValue(contextKey, out var contextData))
                {
                    if (contextData is TContext typedContext)
                        return typedContext;

                    // Try to convert if it's not the exact type
                    try
                    {
                        return (TContext)Convert.ChangeType(contextData, typeof(TContext));
                    }
                    catch
                    {
                        // If conversion fails, return default
                        return default(TContext);
                    }
                }

                return default(TContext);
            }
            catch
            {
                return default(TContext);
            }
        }

        /// <summary>
        /// Updates shared context for a team, making it accessible to all team members.
        /// </summary>
        /// <typeparam name="TContext">Type of context to update</typeparam>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Unique identifier for the team</param>
        /// <param name="sharedContext">The context object to share</param>
        /// <param name="userId">User identifier</param>
        /// <param name="contextKey">Key for the specific context data</param>
        /// <returns>Success message or error description</returns>
        public static async Task<string> UpdateSharedContext<TContext>(
            this IMemoryManager memoryManager,
            string teamId,
            TContext sharedContext,
            string userId = null,
            string contextKey = "shared_context")
        {
            if (string.IsNullOrEmpty(teamId))
            {
              throw new ArgumentException("Team ID cannot be null or empty", nameof(teamId));
            }

            try
            {
                var sessionId = $"team_{teamId}_shared";
                var context = await memoryManager.LoadContextAsync(userId ?? "team_user", sessionId);

                // Update the context with the new shared data
                if (context.Settings == null)
                {
                  context.Settings = new Dictionary<string, object>();
                }

            context.Settings[contextKey] = sharedContext;
                context.Settings["last_updated"] = DateTime.UtcNow;
                context.Settings["last_updated_by"] = userId ?? "team_user";

                // Store as a memory item for searchability
                var memoryContent = $"Shared context updated for team {teamId}: {sharedContext}";
                await memoryManager.AddMemoryAsync(memoryContent, context);

                return $"Shared context updated successfully for team {teamId}";
            }
            catch (Exception ex)
            {
                return $"Failed to update shared context: {ex.Message}";
            }
        }

        /// <summary>
        /// Configures a workflow to use shared memory with automatic context synchronization.
        /// </summary>
        /// <typeparam name="TContext">Context type for the workflow</typeparam>
        /// <typeparam name="TResult">Result type for the workflow</typeparam>
        /// <param name="workflow">The workflow to configure</param>
        /// <param name="memoryManager">Memory manager for shared operations</param>
        /// <param name="teamId">Team identifier for shared memory</param>
        /// <returns>The configured workflow</returns>
        public static AdvancedWorkflow<TContext, TResult> WithSharedMemory<TContext, TResult>(
            this AdvancedWorkflow<TContext, TResult> workflow,
            IMemoryManager memoryManager,
            string teamId = null)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));
            if (memoryManager == null)
                throw new ArgumentNullException(nameof(memoryManager));

            if (teamId == null)
                teamId = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Store the memory manager and team ID in the workflow session
            workflow.Session?.UpdateState("shared_memory_manager", memoryManager);
            workflow.Session?.UpdateState("team_id", teamId);
            workflow.Session?.UpdateState("shared_memory_enabled", true);

            return workflow;
        }

        /// <summary>
        /// Adds team communication log to shared memory.
        /// </summary>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="fromAgent">Agent sending the communication</param>
        /// <param name="toAgent">Agent receiving the communication (null for broadcast)</param>
        /// <param name="message">Communication message</param>
        /// <param name="communicationType">Type of communication (info, handoff, request, etc.)</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Confirmation message</returns>
        public static async Task<string> LogTeamCommunication(
            this IMemoryManager memoryManager,
            string teamId,
            string fromAgent,
            string toAgent,
            string message,
            string communicationType = "info",
            string userId = null)
        {
            try
            {
                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_communications",
                };

                var recipient = !string.IsNullOrEmpty(toAgent) ? $" to {toAgent}" : " (broadcast)";
                var communicationLog = $@"TEAM COMMUNICATION:
Type: {communicationType}
From: {fromAgent}{recipient}
Message: {message}
Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await memoryManager.AddMemoryAsync(communicationLog, context);
                return $"Team communication logged successfully";
            }
            catch (Exception ex)
            {
                return $"Failed to log team communication: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves team communication history for a specific team.
        /// </summary>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="limit">Maximum number of communications to retrieve</param>
        /// <param name="agentFilter">Optional filter for specific agent communications</param>
        /// <param name="userId">User identifier</param>
        /// <returns>List of recent team communications</returns>
        public static async Task<List<string>> GetTeamCommunicationHistory(
            this IMemoryManager memoryManager,
            string teamId,
            int limit = 20,
            string agentFilter = null,
            string userId = null)
        {
            try
            {
                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_communications",
                };

                var memories = await memoryManager.GetExistingMemoriesAsync(context, limit * 2); // Get more to filter

                if (memories == null || !memories.Any())
                    return new List<string>();

                var communications = memories
                    .Where(m => m.Content.Contains("TEAM COMMUNICATION:"))
                    .Where(m => string.IsNullOrEmpty(agentFilter) || m.Content.Contains($"From: {agentFilter}"))
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(limit)
                    .Select(m => m.Content)
                    .ToList();

                return communications;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Stores workflow execution context in team memory for persistence and sharing.
        /// </summary>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="workflowId">Workflow identifier</param>
        /// <param name="executionContext">Execution context to store</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Confirmation message</returns>
        public static async Task<string> StoreWorkflowContext(
            this IMemoryManager memoryManager,
            string teamId,
            string workflowId,
            object executionContext,
            string userId = null)
        {
            try
            {
                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_workflow_{workflowId}",
                };

                var contextMemory = $@"WORKFLOW CONTEXT:
Team: {teamId}
Workflow: {workflowId}
Context: {executionContext}
Stored: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await memoryManager.AddMemoryAsync(contextMemory, context);
                return "Workflow context stored successfully";
            }
            catch (Exception ex)
            {
                return $"Failed to store workflow context: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves stored workflow context from team memory.
        /// </summary>
        /// <typeparam name="T">Expected type of the workflow context</typeparam>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="workflowId">Workflow identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>The retrieved workflow context or default value</returns>
        public static async Task<T> RetrieveWorkflowContext<T>(
            this IMemoryManager memoryManager,
            string teamId,
            string workflowId,
            string userId = null)
        {
            try
            {
                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_workflow_{workflowId}",
                };

                var memories = await memoryManager.GetExistingMemoriesAsync(context, 1);
                if (memories?.Any() != true)
                    return default(T);

                var latestMemory = memories.OrderByDescending(m => m.CreatedAt).First();
                var memoryText = latestMemory.Content;

                // Extract context from the memory text
                var contextStart = memoryText.IndexOf("Context: ");
                if (contextStart == -1) return default(T);

                var contextEnd = memoryText.IndexOf("\nStored:");
                if (contextEnd == -1) contextEnd = memoryText.Length;

                var contextText = memoryText.Substring(contextStart + 9, contextEnd - contextStart - 9).Trim();

                // Try to deserialize or convert the context
                try
                {
                    if (typeof(T) == typeof(string))
                        return (T)(object)contextText;

                    return (T)Convert.ChangeType(contextText, typeof(T));
                }
                catch
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Creates a memory snapshot for team state at a specific point in time.
        /// </summary>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="snapshotName">Name for the snapshot</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Snapshot identifier</returns>
        public static async Task<string> CreateTeamMemorySnapshot(
            this IMemoryManager memoryManager,
            string teamId,
            string snapshotName = null,
            string userId = null)
        {
            try
            {
                snapshotName = snapshotName ?? $"snapshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
                var snapshotId = Guid.NewGuid().ToString("N").Substring(0, 12);

                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_snapshots",
                };

                // Get current team communications
                var communications = await GetTeamCommunicationHistory(memoryManager, teamId, 50, null, userId);

                // Get shared context
                var sharedContext = await GetSharedContext<object>(memoryManager, teamId, userId);

                var snapshotMemory = $@"TEAM MEMORY SNAPSHOT:
Snapshot ID: {snapshotId}
Snapshot Name: {snapshotName}
Team: {teamId}
Created: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Communications Count: {communications.Count}
Shared Context: {sharedContext}";

                await memoryManager.AddMemoryAsync(snapshotMemory, context);
                return snapshotId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create team memory snapshot: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Purges old team communications while preserving important memories.
        /// </summary>
        /// <param name="memoryManager">The memory manager instance</param>
        /// <param name="teamId">Team identifier</param>
        /// <param name="retentionDays">Number of days to retain communications</param>
        /// <param name="preserveHandoffs">Whether to preserve handoff communications regardless of age</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Number of communications purged</returns>
        public static async Task<int> PurgeOldTeamCommunications(
            this IMemoryManager memoryManager,
            string teamId,
            int retentionDays = 30,
            bool preserveHandoffs = true,
            string userId = null)
        {
            try
            {
                var context = new MemoryContext
                {
                    UserId = userId ?? "team_user",
                    SessionId = $"team_{teamId}_communications",
                };

                var allMemories = await memoryManager.GetExistingMemoriesAsync(context, 1000);
                if (allMemories == null || !allMemories.Any())
                    return 0;

                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                var purgeCount = 0;

                foreach (var memory in allMemories)
                {
                    // Skip if newer than cutoff date
                    if (memory.CreatedAt > cutoffDate)
                        continue;

                    // Skip if it's a handoff and we're preserving them
                    if (preserveHandoffs && memory.Content.Contains("TASK HANDOFF"))
                        continue;

                    // Skip if it's a snapshot
                    if (memory.Content.Contains("TEAM MEMORY SNAPSHOT"))
                        continue;

                    try
                    {
                        await memoryManager.DeleteMemoryAsync(memory.Id);
                        purgeCount++;
                    }
                    catch
                    {
                        // Continue if individual deletion fails
                        continue;
                    }
                }

                return purgeCount;
            }
            catch
            {
                return 0;
            }
        }
    }
}
