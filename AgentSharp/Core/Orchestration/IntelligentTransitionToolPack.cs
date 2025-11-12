using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSharp.Attributes;
using AgentSharp.Tools;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Enhanced ToolPack that enables intelligent agent transitions and variable management
    /// </summary>
    public class IntelligentTransitionToolPack : ToolPack
    {
    private readonly GlobalVariableCollection _globalVariables;
    private readonly new IAgentTransitionContext _context;
    private readonly IEnhancedStorage _storage;
    private readonly string _sessionId;

        public IntelligentTransitionToolPack(
            GlobalVariableCollection globalVariables,
            IAgentTransitionContext context,
            IEnhancedStorage storage = null,
            string sessionId = null)
        {
            _globalVariables = globalVariables ?? throw new ArgumentNullException(nameof(globalVariables));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _storage = storage;
            _sessionId = sessionId;
        }

        #region Variable Management

        /// <summary>
        /// Captures a variable value from the conversation
        /// </summary>
        [FunctionCall("save_variable")]
        [FunctionCallParameter("name", "Variable name to save")]
        [FunctionCallParameter("value", "Variable value extracted from conversation")]
        [FunctionCallParameter("confidence", "Confidence level 0.0-1.0 (default: 1.0)")]
        public async Task<string> SaveVariable(string name, string value, double confidence = 1.0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Error: Variable name cannot be empty";

                if (string.IsNullOrWhiteSpace(value))
                    return "❌ Error: Variable value cannot be empty";

                if (confidence < 0.0 || confidence > 1.0)
                    return "❌ Error: Confidence must be between 0.0 and 1.0";

                _globalVariables.SetVariable(name, value, confidence);

                var confidenceText = confidence < 1.0 ? $" with {confidence:P0} confidence" : "";
                return $"✅ Successfully captured {name} = '{value}'{confidenceText}";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"❌ Access denied: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                return $"❌ Invalid variable: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Error saving variable: {ex.Message}";
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Retrieves a variable value
        /// </summary>
        [FunctionCall("get_variable")]
        [FunctionCallParameter("name", "Variable name to retrieve")]
        public string GetVariable(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Error: Variable name cannot be empty";

                var value = _globalVariables.GetVariableValue(name);
                if (value == null)
                    return $"📋 Variable '{name}' is not defined or empty";

                return $"📋 {name} = '{value}'";
            }
            catch (ArgumentException)
            {
                return $"❌ Variable '{name}' does not exist";
            }
            catch (Exception ex)
            {
                return $"❌ Error retrieving variable: {ex.Message}";
            }
        }

        #endregion

        #region Intelligent Transitions

        /// <summary>
        /// Transfers conversation to another specialist agent
        /// </summary>
        [FunctionCall("transfer_to_agent")]
        [FunctionCallParameter("agent_name", "Name of the specialist agent")]
        [FunctionCallParameter("reason", "Reason for the transfer")]
        [FunctionCallParameter("message_to_user", "Explanation message for the user")]
        public async Task<string> TransferToAgent(string agentName, string reason, string messageToUser)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(agentName))
                    return "❌ Error: Agent name cannot be empty";

                if (!_context.AvailableAgents.Contains(agentName))
                {
                    var available = string.Join(", ", _context.AvailableAgents);
                    return $"❌ Agent '{agentName}' not found. Available: {available}";
                }

                if (agentName == _context.CurrentAgent)
                    return "❌ You cannot transfer to yourself";

                // Save transfer audit
                await SaveTransferAuditAsync(agentName, reason);

                // Execute transition
                _context.TransitionTo(agentName);

                return messageToUser ?? $"I'm connecting you with {agentName}, our specialist who can better assist you.";
            }
            catch (Exception ex)
            {
                return $"❌ Error during transfer: {ex.Message}";
            }
        }

        /// <summary>
        /// Completes the conversation when objectives are met
        /// </summary>
        [FunctionCall("complete_conversation")]
        [FunctionCallParameter("reason", "Reason for completion")]
        [FunctionCallParameter("message_to_user", "Final message to the user")]
        public async Task<string> CompleteConversation(string reason, string messageToUser)
        {
            try
            {
                // Save completion audit
                await SaveCompletionAuditAsync(reason);

                // Mark conversation as complete
                _context.CompleteConversation(reason);

                return messageToUser ?? "Thank you! I've completed assisting you. Is there anything else you need?";
            }
            catch (Exception ex)
            {
                return $"❌ Error completing conversation: {ex.Message}";
            }
        }

        #endregion

        #region Context Analysis

        /// <summary>
        /// Provides detailed analysis of current conversation context
        /// </summary>
        [FunctionCall("analyze_context")]
        public string AnalyzeContext()
        {
            var analysis = new StringBuilder();

            analysis.AppendLine("📊 CURRENT CONTEXT ANALYSIS:");

            var progress = _context.GetProgress();
            analysis.AppendLine($"   Progress: {progress.FilledVariables}/{progress.TotalVariables} variables ({progress.CompletionPercentage:P0})");
            analysis.AppendLine($"   Required: {progress.RequiredFilled}/{progress.RequiredVariables} completed ({progress.RequiredCompletionPercentage:P0})");

            var missing = _context.GetMissingVariables();
            if (missing.Any())
            {
                analysis.AppendLine("\n❌ MISSING INFORMATION:");
                foreach (var m in missing.Where(v => v.IsRequired))
                {
                    analysis.AppendLine($"   • {m.Name} (REQUIRED): {m.Description}");
                }
                foreach (var m in missing.Where(v => !v.IsRequired))
                {
                    analysis.AppendLine($"   • {m.Name} (optional): {m.Description}");
                }
            }

            var collected = _context.GetCollectedVariables();
            if (collected.Any())
            {
                analysis.AppendLine("\n✅ COLLECTED INFORMATION:");
                foreach (var c in collected)
                {
                    var confidence = c.Confidence < 1.0 ? $" ({c.Confidence:P0} confidence)" : "";
                    analysis.AppendLine($"   • {c.Name} = '{c.Value}' (by {c.CapturedBy}){confidence}");
                }
            }

            analysis.AppendLine($"\n🤝 AVAILABLE AGENTS: {string.Join(", ", _context.AvailableAgents)}");
            analysis.AppendLine($"💬 CONVERSATION LENGTH: {_context.History.Count} messages");

            analysis.AppendLine("\n🎯 RECOMMENDATION:");
            if (progress.IsComplete)
                analysis.AppendLine("   All required information collected. Consider completing the conversation.");
            else if (missing.Any(v => v.IsRequired && v.OwnedBy != _context.CurrentAgent))
                analysis.AppendLine("   Consider transferring to agent responsible for missing required variables.");
            else
                analysis.AppendLine("   Continue conversation to collect remaining information.");

            return analysis.ToString();
        }

        /// <summary>
        /// Lists variables that the current agent is responsible for
        /// </summary>
        [FunctionCall("list_my_variables")]
        public string ListMyVariables()
        {
            try
            {
                var owned = _globalVariables.GetOwnedVariables(_context.CurrentAgent);
                if (!owned.Any())
                    return $"📋 No variables assigned to agent '{_context.CurrentAgent}'";

                var result = new StringBuilder();
                result.AppendLine($"📋 Variables for agent '{_context.CurrentAgent}':");

                foreach (var variable in owned)
                {
                    var status = variable.IsCollected
                        ? $"✅ '{variable.Value}'" + (variable.Confidence < 1.0 ? $" ({variable.Confidence:P0} confidence)" : "")
                        : "❌ Not collected";

                    var requiredIndicator = variable.IsRequired ? " (REQUIRED)" : "";
                    result.AppendLine($"  • {variable.Name}: {status}{requiredIndicator}");

                    if (!string.IsNullOrEmpty(variable.Description))
                        result.AppendLine($"    📄 {variable.Description}");
                }

                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"❌ Error listing variables: {ex.Message}";
            }
        }

        /// <summary>
        /// Shows overall conversation progress
        /// </summary>
        [FunctionCall("get_progress")]
        public string GetProgress()
        {
            try
            {
                var progress = _context.GetProgress();
                var result = new StringBuilder();

                result.AppendLine("📊 CONVERSATION PROGRESS");
                result.AppendLine($"   Total: {progress.FilledVariables}/{progress.TotalVariables} variables ({progress.CompletionPercentage:P0})");
                result.AppendLine($"   Required: {progress.RequiredFilled}/{progress.RequiredVariables} ({progress.RequiredCompletionPercentage:P0})");

                if (progress.IsComplete)
                {
                    result.AppendLine("🎉 All required variables collected!");
                }
                else
                {
                    var missing = _context.GetMissingVariables();
                    var requiredMissing = missing.Where(v => v.IsRequired).ToList();

                    if (requiredMissing.Any())
                    {
                        result.AppendLine("⚠️ Required variables still missing:");
                        foreach (var variable in requiredMissing)
                        {
                            result.AppendLine($"   • {variable.Name}: {variable.Description} (assigned to {variable.OwnedBy})");
                        }
                    }
                }

                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"❌ Error getting progress: {ex.Message}";
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task SaveTransferAuditAsync(string targetAgent, string reason)
        {
            if (_storage == null || string.IsNullOrEmpty(_sessionId)) return;

            try
            {
                var auditEntry = new VariableAuditEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    SessionId = _sessionId,
                    VariableName = "__agent_transfer__",
                    OldValue = _context.CurrentAgent,
                    NewValue = targetAgent,
                    ModifiedBy = _context.CurrentAgent,
                    ModifiedAt = DateTime.UtcNow,
                    Source = "intelligent_decision",
                    Context = new Dictionary<string, object>
                    {
                        ["transfer_reason"] = reason,
                        ["conversation_progress"] = _context.GetProgress().CompletionPercentage,
                        ["message_count"] = _context.History.Count
                    }
                };

                await _storage.SaveVariableAuditAsync(_sessionId, auditEntry);
            }
            catch (Exception ex)
            {
                // Log but don't fail the transfer
                Console.WriteLine($"Failed to save transfer audit: {ex.Message}");
            }
        }

        private async Task SaveCompletionAuditAsync(string reason)
        {
            if (_storage == null || string.IsNullOrEmpty(_sessionId)) return;

            try
            {
                var auditEntry = new VariableAuditEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    SessionId = _sessionId,
                    VariableName = "__conversation_completion__",
                    OldValue = "active",
                    NewValue = "completed",
                    ModifiedBy = _context.CurrentAgent,
                    ModifiedAt = DateTime.UtcNow,
                    Source = "intelligent_completion",
                    Context = new Dictionary<string, object>
                    {
                        ["completion_reason"] = reason,
                        ["final_progress"] = _context.GetProgress().CompletionPercentage,
                        ["total_messages"] = _context.History.Count
                    }
                };

                await _storage.SaveVariableAuditAsync(_sessionId, auditEntry);
            }
            catch (Exception ex)
            {
                // Log but don't fail the completion
                Console.WriteLine($"Failed to save completion audit: {ex.Message}");
            }
        }

        #endregion
    }
}
