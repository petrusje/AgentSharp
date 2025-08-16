using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AgentSharp.Core.Orchestration;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Services;
using AgentSharp.Core;
using Microsoft.Extensions.Logging;

namespace AgentSharp.Tests.Core.Orchestration
{
    /// <summary>
    /// Testes de integração para funcionalidades globais de workflows
    /// </summary>
    public class GlobalFeaturesIntegrationTests
    {
        [Fact]
        public void GlobalMemory_WithGlobalMemory_EnablesFeature()
        {
            // Arrange
            var workflow = new TestWorkflow();
            var memory = new BasicMemory();

            // Act
            workflow.WithGlobalMemory(enabled: true, memory: memory);

            // Assert
            var features = workflow.GetGlobalFeatures();
            Assert.NotNull(features);
            Assert.True(features.GlobalMemoryEnabled);
            Assert.Equal(memory, features.GlobalMemory);
        }

        [Fact]
        public void GlobalMessageHistory_WithGlobalMessageHistory_EnablesFeature()
        {
            // Arrange
            var workflow = new TestWorkflow();

            // Act
            workflow.WithGlobalMessageHistory(enabled: true, maxMessages: 500);

            // Assert
            var features = workflow.GetGlobalFeatures();
            Assert.NotNull(features);
            Assert.True(features.GlobalHistoryEnabled);
            Assert.Equal(500, features.MaxGlobalMessages);
        }

        [Fact]
        public void EnhancedStorage_WithEnhancedStorage_ConfiguresStorage()
        {
            // Arrange
            var workflow = new TestWorkflow();
            var storage = new TestEnhancedStorage();

            // Act
            workflow.WithEnhancedStorage(storage);

            // Assert
            var features = workflow.GetGlobalFeatures();
            Assert.NotNull(features);
            Assert.Equal(storage, features.EnhancedStorage);
        }

        [Fact]
        public async Task AddGlobalMessage_WhenHistoryEnabled_AddsMessage()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMessageHistory(enabled: true);

            // Act
            await workflow.AddGlobalMessageAsync("TestAgent", "Test message", "agent");

            // Assert
            var history = workflow.GetGlobalMessageHistory();
            Assert.Single(history);
            Assert.Equal("TestAgent", history[0].AgentName);
            Assert.Equal("Test message", history[0].Content);
            Assert.Equal("agent", history[0].MessageType);
        }

        [Fact]
        public async Task AddGlobalMessage_WhenHistoryDisabled_DoesNotAddMessage()
        {
            // Arrange
            var workflow = new TestWorkflow();
            // Não habilitar histórico global

            // Act
            await workflow.AddGlobalMessageAsync("TestAgent", "Test message", "agent");

            // Assert
            var history = workflow.GetGlobalMessageHistory();
            Assert.Empty(history);
        }

        [Fact]
        public async Task AddGlobalMessage_WithMaxLimit_LimitsHistorySize()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMessageHistory(enabled: true, maxMessages: 2);

            // Act
            await workflow.AddGlobalMessageAsync("Agent1", "Message 1");
            await workflow.AddGlobalMessageAsync("Agent2", "Message 2");
            await workflow.AddGlobalMessageAsync("Agent3", "Message 3"); // Should remove first

            // Assert
            var history = workflow.GetGlobalMessageHistory();
            Assert.Equal(2, history.Count);
            Assert.Equal("Message 2", history[0].Content);
            Assert.Equal("Message 3", history[1].Content);
        }

        [Fact]
        public void HasGlobalMemory_WhenEnabled_ReturnsTrue()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMemory(enabled: true);

            // Act & Assert
            Assert.True(workflow.HasGlobalMemory());
        }

        [Fact]
        public void HasGlobalMemory_WhenDisabled_ReturnsFalse()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMemory(enabled: false);

            // Act & Assert
            Assert.False(workflow.HasGlobalMemory());
        }

        [Fact]
        public void HasGlobalHistory_WhenEnabled_ReturnsTrue()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMessageHistory(enabled: true);

            // Act & Assert
            Assert.True(workflow.HasGlobalHistory());
        }

        [Fact]
        public void HasGlobalHistory_WhenDisabled_ReturnsFalse()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMessageHistory(enabled: false);

            // Act & Assert
            Assert.False(workflow.HasGlobalHistory());
        }

        [Fact]
        public void GetGlobalMemory_WhenConfigured_ReturnsMemory()
        {
            // Arrange
            var workflow = new TestWorkflow();
            var memory = new BasicMemory();
            workflow.WithGlobalMemory(enabled: true, memory: memory);

            // Act
            var retrievedMemory = workflow.GetGlobalMemory();

            // Assert
            Assert.Equal(memory, retrievedMemory);
        }

        [Fact]
        public void GetGlobalMemory_WhenNotConfigured_ReturnsNull()
        {
            // Arrange
            var workflow = new TestWorkflow();

            // Act
            var retrievedMemory = workflow.GetGlobalMemory();

            // Assert
            Assert.Null(retrievedMemory);
        }

        [Fact]
        public async Task ClearGlobalData_ClearsHistoryAndMemory()
        {
            // Arrange
            var workflow = new TestWorkflow();
            var memory = new TestMemory();
            workflow.WithGlobalMemory(enabled: true, memory: memory);
            workflow.WithGlobalMessageHistory(enabled: true);
            
            await workflow.AddGlobalMessageAsync("Agent", "Test message");

            // Act
            await workflow.ClearGlobalDataAsync();

            // Assert
            var history = workflow.GetGlobalMessageHistory();
            Assert.Empty(history);
            Assert.True(memory.WasCleared);
        }

        [Fact]
        public void RemoveGlobalFeatures_RemovesFeaturesFromWorkflow()
        {
            // Arrange
            var workflow = new TestWorkflow();
            workflow.WithGlobalMemory(enabled: true);

            // Act
            workflow.RemoveGlobalFeatures();

            // Assert
            var features = workflow.GetGlobalFeatures();
            Assert.Null(features);
        }

        [Fact]
        public async Task SaveAndLoadGlobalState_WithEnhancedStorage_PreservesState()
        {
            // Arrange
            var workflow = new TestWorkflow();
            var storage = new TestEnhancedStorage();
            workflow.WithEnhancedStorage(storage);
            workflow.WithGlobalMessageHistory(enabled: true);
            
            await workflow.AddGlobalMessageAsync("Agent", "Test message");
            string sessionId = "test-session";

            // Act
            await workflow.SaveGlobalStateAsync(sessionId);
            await workflow.ClearGlobalDataAsync(); // Clear current state
            await workflow.LoadGlobalStateAsync(sessionId);

            // Assert - Verify state was restored
            var history = workflow.GetGlobalMessageHistory();
            Assert.Single(history);
            Assert.Equal("Test message", history[0].Content);
        }

        [Fact]
        public void MultipleWorkflows_IndependentGlobalFeatures()
        {
            // Arrange
            var workflow1 = new TestWorkflow();
            var workflow2 = new TestWorkflow();

            // Act
            workflow1.WithGlobalMemory(enabled: true);
            workflow2.WithGlobalMessageHistory(enabled: true);

            // Assert
            Assert.True(workflow1.HasGlobalMemory());
            Assert.False(workflow1.HasGlobalHistory());
            
            Assert.False(workflow2.HasGlobalMemory());
            Assert.True(workflow2.HasGlobalHistory());
        }

        [Fact]
        public void FluentConfiguration_ChainsCorrectly()
        {
            // Arrange & Act
            var workflow = new TestWorkflow()
                .WithGlobalMemory(enabled: true, memory: new BasicMemory())
                .WithGlobalMessageHistory(enabled: true, maxMessages: 1000)
                .WithEnhancedStorage(new TestEnhancedStorage())
                .WithRetentionPolicy(new RetentionPolicy());

            // Assert
            var features = workflow.GetGlobalFeatures();
            Assert.NotNull(features);
            Assert.True(features.GlobalMemoryEnabled);
            Assert.True(features.GlobalHistoryEnabled);
            Assert.Equal(1000, features.MaxGlobalMessages);
            Assert.NotNull(features.EnhancedStorage);
            Assert.NotNull(features.RetentionPolicy);
        }
    }

    #region Test Classes

    /// <summary>
    /// Workflow de teste para validar funcionalidades globais
    /// </summary>
    public class TestWorkflow : IWorkflow<object>
    {
        public string Name => "TestWorkflow";

        public IWorkflow<object> AddStep(string name, IAgent agent, Func<object, string> getInput, Action<object, string> processOutput)
        {
            return this;
        }

        public Task<object> ExecuteAsync(object context, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>("Test Result");
        }

        public void Dispose()
        {
            // Test implementation
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Implementação de memória para testes
    /// </summary>
    public class TestMemory : IMemory
    {
        public bool WasCleared { get; private set; }

        public Task AddItemAsync(AgentSharp.Core.Memory.Models.MemoryItem item)
        {
            return Task.CompletedTask;
        }

        public Task<List<AgentSharp.Core.Memory.Models.MemoryItem>> SearchAsync(string query, int limit = 10)
        {
            return Task.FromResult(new List<AgentSharp.Core.Memory.Models.MemoryItem>());
        }

        public Task ClearAsync()
        {
            WasCleared = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Test implementation
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Implementação de Enhanced Storage para testes
    /// </summary>
    public class TestEnhancedStorage : IEnhancedStorage
    {
        private readonly Dictionary<string, ConversationState> _conversationStates = new();
        private readonly Dictionary<string, List<VariableAuditEntry>> _auditEntries = new();

        // IStorage base properties
        public ISessionStorage Sessions => throw new NotImplementedException();
        public IMemoryStorage Memories => throw new NotImplementedException();
        public IEmbeddingStorage Embeddings => throw new NotImplementedException();
        public bool IsConnected => true;

        // Enhanced Storage methods
        public Task<ConversationState> LoadConversationStateAsync(string sessionId)
        {
            _conversationStates.TryGetValue(sessionId, out var state);
            return Task.FromResult(state);
        }

        public Task SaveConversationStateAsync(string sessionId, ConversationState state)
        {
            _conversationStates[sessionId] = state;
            return Task.CompletedTask;
        }

        public Task UpdateConversationVariablesAsync(string sessionId, GlobalVariableCollection variables)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ConversationStateExistsAsync(string sessionId)
        {
            return Task.FromResult(_conversationStates.ContainsKey(sessionId));
        }

        public Task<List<VariableAuditEntry>> GetVariableAuditAsync(string sessionId, string variableName = null)
        {
            _auditEntries.TryGetValue(sessionId, out var entries);
            return Task.FromResult(entries ?? new List<VariableAuditEntry>());
        }

        public Task SaveVariableAuditAsync(string sessionId, VariableAuditEntry entry)
        {
            if (!_auditEntries.ContainsKey(sessionId))
                _auditEntries[sessionId] = new List<VariableAuditEntry>();
            
            _auditEntries[sessionId].Add(entry);
            return Task.CompletedTask;
        }

        public Task<List<VariableAuditEntry>> GetVariableHistoryAsync(string sessionId, string variableName, int? limit = null)
        {
            return Task.FromResult(new List<VariableAuditEntry>());
        }

        // Stub implementations para outros métodos
        public Task<List<string>> GetActiveConversationSessionsAsync(string userId = null) => 
            Task.FromResult(new List<string>());

        public Task<List<ConversationSessionMetadata>> GetConversationSessionsAsync(ConversationSearchCriteria criteria) => 
            Task.FromResult(new List<ConversationSessionMetadata>());

        public Task MarkConversationCompleteAsync(string sessionId, string completionReason = null) => 
            Task.CompletedTask;

        public Task<int> CleanupExpiredSessionsAsync(TimeSpan maxAge, bool preserveComplete = true) => 
            Task.FromResult(0);

        public Task<ConversationBackup> CreateBackupAsync(string sessionId) => 
            Task.FromResult<ConversationBackup>(null);

        public Task RestoreFromBackupAsync(string sessionId, ConversationBackup backup) => 
            Task.CompletedTask;

        public Task<byte[]> ExportConversationDataAsync(string sessionId, string format = "json") => 
            Task.FromResult(new byte[0]);

        public Task<VariableUsageStatistics> GetVariableUsageStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null) => 
            Task.FromResult(new VariableUsageStatistics());

        public Task<ConversationPerformanceMetrics> GetConversationMetricsAsync(ConversationAnalysisCriteria criteria) => 
            Task.FromResult(new ConversationPerformanceMetrics());

        public Task<List<ConversationIssue>> IdentifyConversationIssuesAsync(ConversationIssueCriteria criteria) => 
            Task.FromResult(new List<ConversationIssue>());

        public Task OptimizeStorageAsync() => Task.CompletedTask;

        public Task<StorageHealthInfo> GetStorageHealthAsync() => 
            Task.FromResult(new StorageHealthInfo { IsHealthy = true });

        public Task ConfigureRetentionPolicyAsync(RetentionPolicy policy) => Task.CompletedTask;

        // IStorage delegação
        public Task InitializeAsync() => Task.CompletedTask;
        public Task ClearAllAsync() => Task.CompletedTask;
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;
        public Task SaveMessageAsync(AgentSharp.Models.Message message) => Task.CompletedTask;
        public Task<List<AgentSharp.Models.Message>> GetSessionMessagesAsync(string sessionId, int? limit = null) => 
            Task.FromResult(new List<AgentSharp.Models.Message>());
        public Task SaveMemoryAsync(AgentSharp.Core.Memory.Models.UserMemory memory) => Task.CompletedTask;
        public Task<List<AgentSharp.Core.Memory.Models.UserMemory>> GetMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null, int? limit = null) => 
            Task.FromResult(new List<AgentSharp.Core.Memory.Models.UserMemory>());
        public Task<List<AgentSharp.Core.Memory.Models.UserMemory>> SearchMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context, int limit = 10) => 
            Task.FromResult(new List<AgentSharp.Core.Memory.Models.UserMemory>());
        public Task UpdateMemoryAsync(AgentSharp.Core.Memory.Models.UserMemory memory) => Task.CompletedTask;
        public Task DeleteMemoryAsync(string id) => Task.CompletedTask;
        public Task ClearMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null) => Task.CompletedTask;
        public Task<AgentSharp.Core.Memory.Interfaces.ISession> GetOrCreateSessionAsync(string sessionId, string userId) => 
            Task.FromResult<AgentSharp.Core.Memory.Interfaces.ISession>(null);
        public Task<List<AgentSharp.Core.Memory.Interfaces.ISession>> GetUserSessionsAsync(string userId, int? limit = null) => 
            Task.FromResult(new List<AgentSharp.Core.Memory.Interfaces.ISession>());
        public Task DeleteSessionAsync(string sessionId) => Task.CompletedTask;
        public Task<List<AgentSharp.Models.AIMessage>> GetSessionHistoryAsync(string userId, string sessionId, int limit = 10) => 
            Task.FromResult(new List<AgentSharp.Models.AIMessage>());
        public Task SaveSessionMessageAsync(string userId, string sessionId, AgentSharp.Models.AIMessage message) => Task.CompletedTask;
    }

    #endregion
}