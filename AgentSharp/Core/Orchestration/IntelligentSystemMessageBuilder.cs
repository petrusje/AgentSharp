using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Builds intelligent system messages with full context for LLM-driven decision making
    /// </summary>
    public class IntelligentSystemMessageBuilder
    {
        /// <summary>
        /// Builds a comprehensive system message for an agent with full context
        /// </summary>
        public string BuildIntelligentSystemMessage(
            TeamAgent currentAgent, 
            GlobalVariableCollection variables, 
            Dictionary<string, TeamAgent> availableAgents,
            List<ConversationMessage> messageHistory)
        {
            var message = new StringBuilder();

            // Agent Identity and Expertise
            message.AppendLine($"## YOU ARE: {currentAgent.Name}");
            message.AppendLine($"EXPERTISE: {currentAgent.Expertise}");
            message.AppendLine();

            // Current Context Overview
            message.AppendLine("## CURRENT CONVERSATION CONTEXT:");
            BuildContextSection(message, variables, messageHistory);
            message.AppendLine();

            // Available Team Members
            message.AppendLine("## AVAILABLE TEAM MEMBERS:");
            BuildTeamSection(message, availableAgents, currentAgent.Name);
            message.AppendLine();

            // Variable Status and Responsibilities
            message.AppendLine("## VARIABLE STATUS & RESPONSIBILITIES:");
            BuildVariableSection(message, variables, currentAgent.Name);
            message.AppendLine();

            // Available Tools and Capabilities
            message.AppendLine("## YOUR CAPABILITIES:");
            BuildCapabilitiesSection(message);
            message.AppendLine();

            // Decision Making Guidelines
            message.AppendLine("## DECISION MAKING:");
            BuildDecisionSection(message, variables);

            return message.ToString();
        }

        private void BuildContextSection(StringBuilder message, GlobalVariableCollection variables, List<ConversationMessage> history)
        {
            var progress = variables.GetProgress();
            message.AppendLine($"📊 Progress: {progress.FilledVariables}/{progress.TotalVariables} variables collected ({progress.CompletionPercentage:P0})");
            message.AppendLine($"🎯 Required: {progress.RequiredFilled}/{progress.RequiredVariables} completed ({progress.RequiredCompletionPercentage:P0})");
            
            if (history.Count > 0)
            {
                message.AppendLine($"💬 Messages: {history.Count} total");
                var recentMessages = history.Skip(Math.Max(0, history.Count - 3)).Take(3).ToList();
                message.AppendLine("Recent conversation:");
                foreach (var msg in recentMessages)
                {
                    var truncated = msg.Content.Length > 100 ? msg.Content.Substring(0, 100) + "..." : msg.Content;
                    message.AppendLine($"   [{msg.MessageType}] {msg.AgentName}: {truncated}");
                }
            }
        }

        private void BuildTeamSection(StringBuilder message, Dictionary<string, TeamAgent> agents, string currentAgentName)
        {
            foreach (var agent in agents.Values)
            {
                var indicator = agent.Name == currentAgentName ? "👤 YOU" : "🤝";
                var status = agent.IsActive ? "Active" : "Inactive";
                message.AppendLine($"{indicator} {agent.Name}: {agent.Expertise} ({status})");
            }
        }

        private void BuildVariableSection(StringBuilder message, GlobalVariableCollection variables, string currentAgentName)
        {
            var allVariables = variables.GetAllVariables().ToList();
            var ownedByMe = allVariables.Where(v => v.OwnedBy == currentAgentName).ToList();
            var ownedByOthers = allVariables.Where(v => v.OwnedBy != currentAgentName && v.OwnedBy != "any").ToList();
            var shared = allVariables.Where(v => v.OwnedBy == "any").ToList();

            // My responsibilities
            if (ownedByMe.Any())
            {
                message.AppendLine("📋 YOUR RESPONSIBILITIES:");
                foreach (var variable in ownedByMe)
                {
                    var status = variable.IsCollected ? "✅ COLLECTED" : "❌ MISSING";
                    var required = variable.IsRequired ? " (REQUIRED)" : "";
                    var value = variable.IsCollected ? $" = '{variable.Value}'" : "";
                    message.AppendLine($"   • {variable.Name}: {variable.Description} [{status}]{required}{value}");
                }
                message.AppendLine();
            }

            // Collected by others
            var collectedByOthers = ownedByOthers.Where(v => v.IsCollected).ToList();
            if (collectedByOthers.Any())
            {
                message.AppendLine("✅ ALREADY COLLECTED BY TEAM:");
                foreach (var variable in collectedByOthers)
                {
                    message.AppendLine($"   • {variable.Name} = '{variable.Value}' (by {variable.CapturedBy})");
                }
                message.AppendLine();
            }

            // Still missing from others
            var missingFromOthers = ownedByOthers.Where(v => !v.IsCollected).ToList();
            if (missingFromOthers.Any())
            {
                message.AppendLine("⏳ PENDING FROM OTHER AGENTS:");
                foreach (var variable in missingFromOthers)
                {
                    var required = variable.IsRequired ? " (REQUIRED)" : "";
                    message.AppendLine($"   • {variable.Name}: {variable.Description} (assigned to {variable.OwnedBy}){required}");
                }
                message.AppendLine();
            }

            // Shared variables
            if (shared.Any())
            {
                message.AppendLine("🤝 SHARED VARIABLES (any agent can modify):");
                foreach (var variable in shared)
                {
                    var status = variable.IsCollected ? $"✅ '{variable.Value}'" : "❌ Not set";
                    message.AppendLine($"   • {variable.Name}: {variable.Description} [{status}]");
                }
            }
        }

        private void BuildCapabilitiesSection(StringBuilder message)
        {
            message.AppendLine("🛠️ TOOLS AVAILABLE:");
            message.AppendLine("   • SaveVariable: Capture data from conversation");
            message.AppendLine("   • GetVariable: Check any variable value");
            message.AppendLine("   • TransferToAgent: Hand off to team member");
            message.AppendLine("   • CompleteConversation: Finish when objectives met");
            message.AppendLine("   • AnalyzeContext: Get detailed context analysis");
            message.AppendLine();
            message.AppendLine("💡 CONVERSATION APPROACH:");
            message.AppendLine("   • Engage naturally with the user");
            message.AppendLine("   • Collect information through dialogue");
            message.AppendLine("   • Use tools when you identify relevant data");
            message.AppendLine("   • Transfer when another agent would serve better");
        }

        private void BuildDecisionSection(StringBuilder message, GlobalVariableCollection variables)
        {
            var progress = variables.GetProgress();
            
            message.AppendLine("🧠 INTELLIGENT DECISIONS:");
            message.AppendLine("1. **Continue Conversation**: If you can help the user with your expertise");
            message.AppendLine("2. **Collect Variables**: When you identify data you're responsible for");
            message.AppendLine("3. **Transfer to Agent**: When another specialist would be more effective");
            message.AppendLine("4. **Complete Conversation**: When all objectives are achieved");
            message.AppendLine();
            
            if (progress.IsComplete)
            {
                message.AppendLine("🎉 **CONVERSATION STATUS**: All required variables collected! You may complete the conversation.");
            }
            else
            {
                message.AppendLine($"⚠️ **CONVERSATION STATUS**: {progress.RequiredVariables - progress.RequiredFilled} required variables still missing.");
            }
            message.AppendLine();
            
            message.AppendLine("**IMPORTANT**: Always explain transfers to the user (\"I'll connect you with our specialist...\")");
            message.AppendLine("**CRITICAL**: Only you can decide when to transfer - analyze the full context and user needs.");
        }
    }
}