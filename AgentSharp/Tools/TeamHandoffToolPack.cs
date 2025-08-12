using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentSharp.Attributes;
using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Core.Orchestration;
using AgentSharp.Utils;

namespace AgentSharp.Tools
{
    /// <summary>
    /// ToolPack for handling agent handoffs and team coordination.
    /// Provides functionality for agents to hand off tasks to other team members,
    /// share context, and coordinate work within a team.
    /// </summary>
    public class TeamHandoffToolPack : ToolPack
    {
        private readonly Dictionary<string, IAgent> _availableAgents;
        private readonly IMemoryManager _memoryManager;
        private readonly string _teamId;

        /// <summary>
        /// Initializes a new instance of the TeamHandoffToolPack.
        /// </summary>
        /// <param name="teamAgents">Array of agents available for handoffs</param>
        /// <param name="memoryManager">Memory manager for context sharing</param>
        /// <param name="teamId">Unique identifier for the team</param>
        public TeamHandoffToolPack(IAgent[] teamAgents, IMemoryManager memoryManager, string teamId = null)
        {
            if (teamAgents == null || teamAgents.Length == 0)
                throw new ArgumentException("At least one team agent is required", nameof(teamAgents));

            Name = "TeamHandoffToolPack";
            Description = "Tools for agent handoffs and team coordination";
            Version = "1.0.0";

            _availableAgents = teamAgents.ToDictionary(a => a.Name, a => a);
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            _teamId = teamId ?? Guid.NewGuid().ToString("N").Substring(0, TeamConstants.DefaultTeamIdLength);
        }

        /// <summary>
        /// Hands off a task to another agent in the team with context preservation.
        /// </summary>
        /// <param name="target_agent">Name of the agent to hand off the task to</param>
        /// <param name="task_description">Description of the task to be handed off</param>
        /// <param name="reason">Reason for the handoff</param>
        /// <param name="priority">Priority level (High, Medium, Low)</param>
        /// <returns>Result of the handoff operation</returns>
        [FunctionCall("Hand off a task to another team agent")]
        [FunctionCallParameter("target_agent", "Name of the agent to hand off to")]
        [FunctionCallParameter("task_description", "Description of the task to be handed off")]
        [FunctionCallParameter("reason", "Reason for the handoff")]
        [FunctionCallParameter("priority", "Priority level: High, Medium, or Low")]
        public async Task<string> HandoffTask(string target_agent, string task_description, string reason, string priority = "Medium")
        {
            try
            {
                // Validate target agent
                if (!_availableAgents.TryGetValue(target_agent, out var targetAgent))
                {
                    var availableNames = string.Join(", ", _availableAgents.Keys);
                    return $"Agent '{target_agent}' not found in team. Available agents: {availableNames}";
                }

                // Get current context from the calling agent
                var currentContext = GetProperty("ContextVar");
                var currentAgentName = GetProperty("Name")?.ToString() ?? "Unknown";

                // Create handoff context for memory storage
                var handoffContext = new MemoryContext
                {
                    UserId = _memoryManager.UserId ?? TeamConstants.DefaultUserId,
                    SessionId = $"{TeamConstants.TeamSessionPrefix}{_teamId}{TeamConstants.HandoffSessionSuffix}_{DateTime.UtcNow:yyyyMMddHHmm}",
                };

                // Store handoff information in shared memory
                var handoffMemory = $@"TASK HANDOFF:
From: {currentAgentName}
To: {target_agent}
Priority: {priority}
Reason: {reason}
Task: {task_description}
Context: {currentContext}
Handoff Time: {DateTime.UtcNow.ToString(TeamConstants.DateTimeFormat)} UTC";

                await _memoryManager.AddMemoryAsync(handoffMemory, handoffContext);

                // Execute the task with the target agent
                var handoffPrompt = $@"You have received a task handoff from {currentAgentName}.

HANDOFF DETAILS:
Priority: {priority}
Reason for handoff: {reason}
Task Description: {task_description}

CONTEXT FROM PREVIOUS AGENT:
{currentContext}

Please execute this task using your expertise. If you need to hand off to another agent, use the handoff_task tool.";

                var result = await targetAgent.ExecuteAsync(handoffPrompt, currentContext);

                // Store handoff completion in memory
                var completionMemory = $@"HANDOFF COMPLETED:
From: {currentAgentName}
To: {target_agent}
Task: {task_description}
Result: {result}
Completion Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await _memoryManager.AddMemoryAsync(completionMemory, handoffContext);

                return $"Task successfully handed off to {target_agent}. Result: {result}";
            }
            catch (Exception ex)
            {
                return $"Handoff failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Lists all available agents in the team with their capabilities.
        /// </summary>
        /// <returns>List of available team agents and their descriptions</returns>
        [FunctionCall("List all available agents in the team")]
        public string ListTeamAgents()
        {
            var agentList = _availableAgents.Values
                .Select(a => $"- {a.Name}: {a.description}")
                .ToList();

            return $"Available team agents ({_availableAgents.Count} total):\n" + string.Join("\n", agentList);
        }

        /// <summary>
        /// Shares context or information with all team members through shared memory.
        /// </summary>
        /// <param name="information">Information to share with the team</param>
        /// <param name="category">Category of information (e.g., "insight", "data", "decision")</param>
        /// <returns>Confirmation of information sharing</returns>
        [FunctionCall("Share information with all team members")]
        [FunctionCallParameter("information", "Information to share with the team")]
        [FunctionCallParameter("category", "Category of information (insight, data, decision, etc.)")]
        public async Task<string> ShareWithTeam(string information, string category = "general")
        {
            try
            {
                var currentAgentName = GetProperty("Name")?.ToString() ?? "Unknown";

                var sharedContext = new MemoryContext
                {
                    UserId = _memoryManager.UserId ?? "team_user",
                    SessionId = $"team_{_teamId}_shared",
                };

                var sharedMemory = $@"TEAM SHARED INFORMATION:
From: {currentAgentName}
Category: {category}
Information: {information}
Shared Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await _memoryManager.AddMemoryAsync(sharedMemory, sharedContext);

                return $"Information successfully shared with team. Category: {category}";
            }
            catch (Exception ex)
            {
                return $"Failed to share information: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves recent team communications and shared context.
        /// </summary>
        /// <param name="limit">Maximum number of recent communications to retrieve</param>
        /// <returns>Recent team communications</returns>
        [FunctionCall("Retrieve recent team communications and shared context")]
        [FunctionCallParameter("limit", "Maximum number of recent communications to retrieve")]
        public async Task<string> GetTeamContext(int limit = 5)
        {
            try
            {
                var teamContext = new MemoryContext
                {
                    UserId = _memoryManager.UserId ?? "team_user",
                    SessionId = $"team_{_teamId}_shared",
                };

                var memories = await _memoryManager.GetExistingMemoriesAsync(teamContext, limit);

                if (memories == null || !memories.Any())
                {
                    return "No recent team communications found.";
                }

                var recentComms = memories
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(limit)
                    .Select(m => $"[{m.CreatedAt:yyyy-MM-dd HH:mm}] {m.Content}")
                    .ToList();

                return $"Recent team communications ({recentComms.Count} items):\n\n" + string.Join("\n\n", recentComms);
            }
            catch (Exception ex)
            {
                return $"Failed to retrieve team context: {ex.Message}";
            }
        }

        /// <summary>
        /// Requests help or consultation from a specific team member.
        /// </summary>
        /// <param name="target_agent">Name of the agent to consult</param>
        /// <param name="question">Question or area where help is needed</param>
        /// <param name="context">Additional context for the consultation</param>
        /// <returns>Response from the consulted agent</returns>
        [FunctionCall("Request help or consultation from a team member")]
        [FunctionCallParameter("target_agent", "Name of the agent to consult")]
        [FunctionCallParameter("question", "Question or area where help is needed")]
        [FunctionCallParameter("context", "Additional context for the consultation")]
        public async Task<string> ConsultAgent(string target_agent, string question, string context = "")
        {
            try
            {
                if (!_availableAgents.TryGetValue(target_agent, out var targetAgent))
                {
                    var availableNames = string.Join(", ", _availableAgents.Keys);
                    return $"Agent '{target_agent}' not found in team. Available agents: {availableNames}";
                }

                var currentAgentName = GetProperty("Name")?.ToString() ?? "Unknown";
                var currentContext = GetProperty("ContextVar");

                var consultationPrompt = $@"Your teammate {currentAgentName} is requesting your consultation.

CONSULTATION REQUEST:
Question: {question}
Additional Context: {context}

CURRENT SITUATION:
{currentContext}

Please provide your expertise and advice on this matter. Keep your response focused and helpful.";

                var result = await targetAgent.ExecuteAsync(consultationPrompt, currentContext);

                // Log the consultation in shared memory
                var consultContext = new MemoryContext
                {
                    UserId = _memoryManager.UserId ?? "team_user",
                    SessionId = $"team_{_teamId}_consultation",
                };

                var consultationMemory = $@"TEAM CONSULTATION:
Requester: {currentAgentName}
Consultant: {target_agent}
Question: {question}
Response: {result}
Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await _memoryManager.AddMemoryAsync(consultationMemory, consultContext);

                return $"Consultation response from {target_agent}: {result}";
            }
            catch (Exception ex)
            {
                return $"Consultation failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Updates the team on current progress or status.
        /// </summary>
        /// <param name="status">Current status or progress update</param>
        /// <param name="completion_percentage">Percentage of task completion (0-100)</param>
        /// <param name="next_steps">Planned next steps</param>
        /// <returns>Confirmation of status update</returns>
        [FunctionCall("Update the team on current progress or status")]
        [FunctionCallParameter("status", "Current status or progress update")]
        [FunctionCallParameter("completion_percentage", "Percentage of task completion (0-100)")]
        [FunctionCallParameter("next_steps", "Planned next steps")]
        public async Task<string> UpdateTeamStatus(string status, int completion_percentage = -1, string next_steps = "")
        {
            try
            {
                var currentAgentName = GetProperty("Name")?.ToString() ?? "Unknown";

                var statusContext = new MemoryContext
                {
                    UserId = _memoryManager.UserId ?? "team_user",
                    SessionId = $"team_{_teamId}_status",
                };

                var progressInfo = completion_percentage >= 0 ? $"\nProgress: {completion_percentage}%" : "";
                var nextStepsInfo = !string.IsNullOrEmpty(next_steps) ? $"\nNext Steps: {next_steps}" : "";

                var statusMemory = $@"TEAM STATUS UPDATE:
From: {currentAgentName}
Status: {status}{progressInfo}{nextStepsInfo}
Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                await _memoryManager.AddMemoryAsync(statusMemory, statusContext);

                return $"Team status updated successfully. Current status: {status}";
            }
            catch (Exception ex)
            {
                return $"Failed to update team status: {ex.Message}";
            }
        }

        public override async Task InitializeAsync()
        {
            // Initialize team memory context
            var initContext = new MemoryContext
            {
                UserId = _memoryManager.UserId ?? "team_user",
                SessionId = $"team_{_teamId}_init",
            };

            var teamInitMemory = $@"TEAM INITIALIZED:
Team ID: {_teamId}
Members: {string.Join(", ", _availableAgents.Keys)}
Initialized: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            await _memoryManager.AddMemoryAsync(teamInitMemory, initContext);
        }
    }
}
