using AgentSharp.Core.Memory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Lightweight service for basic message logging and conversation history.
    /// Low-cost service that stores messages without semantic processing.
    /// </summary>
    public interface IMessageHistoryService
    {
        /// <summary>
        /// Store a message in conversation history
        /// </summary>
        Task<string> AddMessageAsync(Message message);

        /// <summary>
        /// Get conversation history for a session
        /// </summary>
        Task<List<Message>> GetSessionHistoryAsync(string sessionId, int limit = 50);

        /// <summary>
        /// Get recent messages from conversation
        /// </summary>
        Task<List<Message>> GetRecentMessagesAsync(string sessionId, int count = 5);

        /// <summary>
        /// Clear history for a session
        /// </summary>
        Task ClearSessionHistoryAsync(string sessionId);

        /// <summary>
        /// Get summary of conversation (basic text summary)
        /// </summary>
        Task<string> GetConversationSummaryAsync(string sessionId, int maxMessages = 100);
    }
}