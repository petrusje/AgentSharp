using System;
using System.Collections.Generic;

namespace AgentSharp.Core.Memory.Models
{
    /// <summary>
    /// Implementação do contexto de memória do agente
    /// </summary>
    public class MemoryContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public List<UserMemory> Memories { get; set; } = new List<UserMemory>();
        public List<AIMessage> MessageHistory { get; set; } = new List<AIMessage>();
        public AgentSession Session { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }
}
