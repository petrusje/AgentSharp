using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Basic in-memory implementation of message history service.
    /// Lightweight service for conversation logging without semantic processing.
    /// </summary>
    public class BasicMessageHistoryService : IMessageHistoryService
    {
        private readonly ConcurrentDictionary<string, List<Message>> _sessionHistory;

        public BasicMessageHistoryService()
        {
            _sessionHistory = new ConcurrentDictionary<string, List<Message>>();
        }

        public async Task<string> AddMessageAsync(Message message)
        {
            var sessionId = message.SessionId ?? "default";

            _sessionHistory.AddOrUpdate(sessionId,
                new List<Message> { message },
                (key, existing) =>
                {
                    existing.Add(message);
                    return existing;
                });

            return await Task.FromResult(message.Id);
        }

        public async Task<List<Message>> GetSessionHistoryAsync(string sessionId, int limit = 50)
        {
            if (string.IsNullOrEmpty(sessionId))
                return new List<Message>();

            if (_sessionHistory.TryGetValue(sessionId, out var messages))
            {
                // TakeLast não existe em .NET Standard 2.0, usar LINQ compatível
                return await Task.FromResult(messages.Skip(Math.Max(0, messages.Count - limit)).ToList());
            }

            return new List<Message>();
        }

        public async Task<List<Message>> GetRecentMessagesAsync(string sessionId, int count = 5)
        {
            return await GetSessionHistoryAsync(sessionId, count);
        }

        public async Task ClearSessionHistoryAsync(string sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionHistory.TryRemove(sessionId, out _);
            }
            await Task.CompletedTask;
        }

        public async Task<string> GetConversationSummaryAsync(string sessionId, int maxMessages = 100)
        {
            var messages = await GetSessionHistoryAsync(sessionId, maxMessages);

            if (!messages.Any())
                return "No conversation history.";

            // Basic text summary (no LLM processing)
            var messageCount = messages.Count;
            var userMessages = messages.Count(m => m.Role == "user");
            var assistantMessages = messages.Count(m => m.Role == "assistant");

            return $"Conversation with {messageCount} messages ({userMessages} user, {assistantMessages} assistant). "
        + $"Started: {messages[0].Timestamp:yyyy-MM-dd HH:mm}, "
        + $"Last: {messages[messages.Count - 1].Timestamp:yyyy-MM-dd HH:mm}";
        }
    }
}
