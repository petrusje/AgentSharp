using System.Collections.Generic;
using System.Threading.Tasks;
using AgentSharp.Core.Memory.Models;

namespace AgentSharp.Core.Memory.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de sessões.
    /// </summary>
    public interface ISessionStorage
    {
        Task<string> CreateSessionAsync(AgentSession session);
        Task<AgentSession> GetSessionAsync(string sessionId);
        Task<List<AgentSession>> GetUserSessionsAsync(string userId);
        Task UpdateSessionAsync(AgentSession session);
        Task DeleteSessionAsync(string sessionId);
        void Clear();
    }
}
