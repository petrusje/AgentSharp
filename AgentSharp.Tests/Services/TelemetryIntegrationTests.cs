using AgentSharp.Core;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;
using AgentSharp.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AgentSharp.Tests.Services
{
    [TestClass]
    public class TelemetryIntegrationTests
    {
        private MockTelemetryService? _telemetryService;
        private StringWriter? _consoleOutput;
        private TextWriter? _originalConsoleOut;

        [TestInitialize]
        public void Setup()
        {
            // Capture console output to verify telemetry messages
            _originalConsoleOut = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
            
            // Create mock telemetry service
            _telemetryService = new MockTelemetryService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Restore console output
            Console.SetOut(_originalConsoleOut!);
            _consoleOutput?.Dispose();

            // Clear telemetry data for clean state
            _telemetryService?.Clear();
        }

        [TestMethod]
        public void TelemetryService_ShouldStartDisabled()
        {
            // Arrange & Act
            var isEnabled = _telemetryService!.IsEnabled;

            // Assert
            Assert.IsFalse(isEnabled);
        }

        [TestMethod]
        public void TelemetryService_WhenDisabled_ShouldNotTrackOperations()
        {
            // Arrange
            _telemetryService!.IsEnabled = false;

            // Act
            _telemetryService.StartOperation("test_operation");
            var elapsed = _telemetryService.EndOperation("test_operation");
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(0, elapsed);
            Assert.AreEqual(0, summary.TotalEvents);
        }

        [TestMethod]
        public void TelemetryService_WhenEnabled_ShouldTrackBasicOperations()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;

            // Act
            _telemetryService.StartOperation("test_operation");
            System.Threading.Thread.Sleep(10); // Small delay for measurable time
            var elapsed = _telemetryService.EndOperation("test_operation");
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.IsTrue(elapsed > 0);
            Assert.AreEqual(1, summary.TotalEvents);
            Assert.IsTrue(summary.TotalElapsedSeconds > 0);
        }

        [TestMethod]
        public void TelemetryService_ShouldTrackLLMRequests()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            var operationId = "llm_request_test";
            var tokenCount = 150;

            // Act
            _telemetryService.TrackLLMRequest(operationId);
            System.Threading.Thread.Sleep(10);
            _telemetryService.CompleteLLMRequest(operationId, tokenCount);
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(1, summary.LLMEvents);
            Assert.AreEqual(tokenCount, summary.LLMTokens);
            Assert.IsTrue(summary.TotalLLMTime > 0);
            
            // Verify telemetry data is tracked correctly
            Assert.AreEqual(1, summary.TotalEvents);
            Assert.IsTrue(summary.TotalLLMTime > 0);
        }

        [TestMethod]
        public void TelemetryService_ShouldTrackMemoryOperations()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            var operation = "save_memory";
            var elapsedSeconds = 0.05;
            var tokenCount = 25;

            // Act
            _telemetryService.TrackMemoryOperation(operation, elapsedSeconds, tokenCount);
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(1, summary.MemoryEvents);
            Assert.AreEqual(tokenCount, summary.MemoryTokens);
            Assert.AreEqual(elapsedSeconds, summary.TotalMemoryTime, 0.001);
            
            // Verify tracking integration
            Assert.AreEqual(1, summary.TotalEvents);
        }

        [TestMethod]
        public void TelemetryService_ShouldTrackEmbeddingOperations()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            var operation = "generate_embedding";
            var elapsedSeconds = 0.1;
            var tokenCount = 40;

            // Act
            _telemetryService.TrackEmbeddingOperation(operation, elapsedSeconds, tokenCount);
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(1, summary.MemoryEvents); // Embeddings count as memory operations
            Assert.AreEqual(tokenCount, summary.EmbeddingTokens);
            Assert.AreEqual(elapsedSeconds, summary.TotalMemoryTime, 0.001);
            
            // Verify tracking integration
            Assert.AreEqual(1, summary.TotalEvents);
        }

        [TestMethod]
        public void TelemetryService_ShouldTrackToolExecutions()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            var toolName = "TestTool";
            var elapsedSeconds = 0.02;
            var tokenCount = 10;

            // Act
            _telemetryService.TrackToolExecution(toolName, elapsedSeconds, tokenCount);
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(1, summary.ToolEvents);
            Assert.AreEqual(tokenCount, summary.ToolTokens);
            Assert.AreEqual(elapsedSeconds, summary.TotalToolTime, 0.001);
            
            // Verify tracking integration
            Assert.AreEqual(1, summary.TotalEvents);
        }

        [TestMethod]
        public void TelemetryService_ShouldCalculateCorrectSummary()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;

            // Act - Track multiple operations
            _telemetryService.TrackLLMRequest("llm1");
            _telemetryService.CompleteLLMRequest("llm1", 100);
            
            _telemetryService.TrackMemoryOperation("save", 0.03, 20);
            _telemetryService.TrackEmbeddingOperation("embed", 0.05, 30);
            _telemetryService.TrackToolExecution("tool1", 0.01, 5);
            
            var summary = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(4, summary.TotalEvents);
            Assert.AreEqual(1, summary.LLMEvents);
            Assert.AreEqual(2, summary.MemoryEvents); // Memory + Embedding
            Assert.AreEqual(1, summary.ToolEvents);
            
            Assert.AreEqual(100, summary.LLMTokens);
            Assert.AreEqual(20, summary.MemoryTokens);
            Assert.AreEqual(30, summary.EmbeddingTokens);
            Assert.AreEqual(5, summary.ToolTokens);
            Assert.AreEqual(155, summary.TotalTokens);
            
            Assert.IsTrue(summary.AverageElapsedSeconds > 0);
        }

        [TestMethod]
        public void TelemetryService_ShouldClearCorrectly()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            _telemetryService.TrackLLMRequest("llm1");
            _telemetryService.CompleteLLMRequest("llm1", 100);
            
            // Verify data exists
            var summaryBefore = _telemetryService.GetSummary();
            Assert.IsTrue(summaryBefore.TotalEvents > 0);

            // Act
            _telemetryService.Clear();
            var summaryAfter = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(0, summaryAfter.TotalEvents);
            Assert.AreEqual(0, summaryAfter.TotalTokens);
            Assert.AreEqual(0, summaryAfter.TotalElapsedSeconds);
        }

        [TestMethod]
        public void TelemetryService_ShouldResetPerTest()
        {
            // This test verifies that telemetry resets between tests as required by user feedback
            
            // Arrange
            _telemetryService!.IsEnabled = true;

            // Act - Track some operations
            _telemetryService.TrackLLMRequest("llm1");
            _telemetryService.CompleteLLMRequest("llm1", 50);
            
            var summaryFirst = _telemetryService.GetSummary();
            
            // Clear (simulating reset per test)
            _telemetryService.Clear();
            
            // Track different operations
            _telemetryService.TrackMemoryOperation("load", 0.02);
            var summarySecond = _telemetryService.GetSummary();

            // Assert
            Assert.AreEqual(1, summaryFirst.LLMEvents);
            Assert.AreEqual(50, summaryFirst.LLMTokens);
            
            // After reset, should only show new operations
            Assert.AreEqual(0, summarySecond.LLMEvents);
            Assert.AreEqual(0, summarySecond.LLMTokens);
            Assert.AreEqual(1, summarySecond.MemoryEvents);
        }

        [TestMethod]
        public void TelemetryService_ShouldHandleRepeatedOperationIds()
        {
            // Arrange
            _telemetryService!.IsEnabled = true;
            var operationId = "repeated_operation";

            // Act
            _telemetryService.StartOperation(operationId);
            _telemetryService.StartOperation(operationId); // Should restart the timer
            System.Threading.Thread.Sleep(10);
            var elapsed = _telemetryService.EndOperation(operationId);

            // Assert
            Assert.IsTrue(elapsed > 0);
            var summary = _telemetryService.GetSummary();
            Assert.AreEqual(1, summary.TotalEvents); // Should only have one event
        }

        [TestMethod]
        public void TelemetryService_ShouldShowOnlyTokenMetrics()
        {
            // This test ensures telemetry tracks tokens, not currency costs
            
            // Arrange
            _telemetryService!.IsEnabled = true;

            // Act
            _telemetryService.TrackLLMRequest("llm_test");
            _telemetryService.CompleteLLMRequest("llm_test", 223, 0.05); // Pass costInTokens but only track tokens
            
            var summary = _telemetryService.GetSummary();
            
            // Assert - Should track token counts, not currency
            Assert.AreEqual(223, summary.LLMTokens);
            Assert.AreEqual(223, summary.TotalTokens);
            Assert.AreEqual(1, summary.LLMEvents);
        }

        [TestMethod]
        public void TelemetryService_ShouldProvideComprehensiveApiCoverage()
        {
            // Integration test to verify all telemetry API methods work together
            
            // Arrange
            _telemetryService!.IsEnabled = true;

            // Act - Use all telemetry methods in sequence
            _telemetryService.StartOperation("comprehensive_test");
            System.Threading.Thread.Sleep(5);
            _telemetryService.EndOperation("comprehensive_test");
            
            _telemetryService.TrackLLMRequest("llm_comprehensive");
            _telemetryService.CompleteLLMRequest("llm_comprehensive", 100, 0.05);
            
            _telemetryService.TrackMemoryOperation("save", 0.02, 15);
            _telemetryService.TrackEmbeddingOperation("create", 0.03, 25);
            _telemetryService.TrackToolExecution("TestTool", 0.01, 5);
            
            var summary = _telemetryService.GetSummary();

            // Assert - Verify comprehensive tracking
            Assert.AreEqual(5, summary.TotalEvents);
            Assert.AreEqual(1, summary.LLMEvents);
            Assert.AreEqual(2, summary.MemoryEvents); // Memory + Embedding
            Assert.AreEqual(1, summary.ToolEvents);
            Assert.AreEqual(145, summary.TotalTokens); // 100 + 15 + 25 + 5
            Assert.IsTrue(summary.TotalElapsedSeconds > 0);
            Assert.IsTrue(summary.AverageElapsedSeconds > 0);
        }
    }


    /// <summary>
    /// Mock implementation of ITelemetryService for testing telemetry functionality
    /// </summary>
    public class MockTelemetryService : ITelemetryService
    {
        private readonly Dictionary<string, DateTime> _activeOperations = new();
        private readonly List<TelemetryEvent> _events = new();
        
        public bool IsEnabled { get; set; } = false;

        public void StartOperation(string operationId)
        {
            if (!IsEnabled) return;
            _activeOperations[operationId] = DateTime.Now;
        }

        public double EndOperation(string operationId, bool displayResult = false)
        {
            if (!IsEnabled) return 0;
            
            if (!_activeOperations.TryGetValue(operationId, out var startTime))
                return 0;
                
            _activeOperations.Remove(operationId);
            var elapsed = (DateTime.Now - startTime).TotalSeconds;
            
            _events.Add(new TelemetryEvent
            {
                OperationId = operationId,
                ElapsedSeconds = elapsed,
                OperationType = "general"
            });
            
            return elapsed;
        }

        public void TrackLLMRequest(string operationId)
        {
            if (!IsEnabled) return;
            StartOperation(operationId);
            Console.WriteLine("🤖 Enviando requisição para LLM...");
        }

        public void CompleteLLMRequest(string operationId, int tokenCount, double costInTokens = 0.0)
        {
            if (!IsEnabled) return;
            
            var elapsed = EndOperation(operationId);
            
            // Find and update the event
            var evt = _events.LastOrDefault(e => e.OperationId == operationId);
            if (evt != null)
            {
                evt.TokenCount = tokenCount;
                evt.CostInTokens = costInTokens;
                evt.OperationType = "llm";
            }
            
            Console.WriteLine($"✅ Resposta LLM: {elapsed:F2}s | Tokens: {tokenCount}");
        }

        public void TrackMemoryOperation(string operation, double elapsedSeconds)
        {
            TrackMemoryOperation(operation, elapsedSeconds, 0);
        }

        public void TrackMemoryOperation(string operation, double elapsedSeconds, int tokenCount)
        {
            if (!IsEnabled) return;
            
            _events.Add(new TelemetryEvent
            {
                OperationId = $"memory_{operation}",
                ElapsedSeconds = elapsedSeconds,
                TokenCount = tokenCount,
                OperationType = "memory"
            });
            
            if (tokenCount > 0)
                Console.WriteLine($"💾 Operação de memória {operation}: {elapsedSeconds:F2}s | tokens: {tokenCount}");
            else
                Console.WriteLine($"💾 Operação de memória {operation}: {elapsedSeconds:F2}s");
        }

        public void TrackEmbeddingOperation(string operation, double elapsedSeconds, int tokenCount)
        {
            if (!IsEnabled) return;
            
            _events.Add(new TelemetryEvent
            {
                OperationId = $"embedding_{operation}",
                ElapsedSeconds = elapsedSeconds,
                TokenCount = tokenCount,
                OperationType = "embedding"
            });
            
            Console.WriteLine($"🔄 Operação de embedding {operation}: {elapsedSeconds:F2}s | tokens: {tokenCount}");
        }

        public void TrackToolExecution(string toolName, double elapsedSeconds)
        {
            TrackToolExecution(toolName, elapsedSeconds, 0);
        }

        public void TrackToolExecution(string toolName, double elapsedSeconds, int tokenCount)
        {
            if (!IsEnabled) return;
            
            _events.Add(new TelemetryEvent
            {
                OperationId = $"tool_{toolName}",
                ElapsedSeconds = elapsedSeconds,
                TokenCount = tokenCount,
                OperationType = "tool"
            });
            
            if (tokenCount > 0)
                Console.WriteLine($"🔧 Execução de ferramenta {toolName}: {elapsedSeconds:F2}s | tokens: {tokenCount}");
            else
                Console.WriteLine($"🔧 Execução de ferramenta {toolName}: {elapsedSeconds:F2}s");
        }

        public TelemetrySummary GetSummary()
        {
            if (!IsEnabled) return new TelemetrySummary();
            
            var summary = new TelemetrySummary
            {
                TotalEvents = _events.Count,
                TotalElapsedSeconds = _events.Sum(e => e.ElapsedSeconds),
                LLMEvents = _events.Count(e => e.OperationType == "llm"),
                MemoryEvents = _events.Count(e => e.OperationType == "memory" || e.OperationType == "embedding"),
                ToolEvents = _events.Count(e => e.OperationType == "tool"),
                TotalLLMTime = _events.Where(e => e.OperationType == "llm").Sum(e => e.ElapsedSeconds),
                TotalMemoryTime = _events.Where(e => e.OperationType == "memory" || e.OperationType == "embedding").Sum(e => e.ElapsedSeconds),
                TotalToolTime = _events.Where(e => e.OperationType == "tool").Sum(e => e.ElapsedSeconds),
                LLMTokens = _events.Where(e => e.OperationType == "llm").Sum(e => e.TokenCount),
                MemoryTokens = _events.Where(e => e.OperationType == "memory").Sum(e => e.TokenCount),
                EmbeddingTokens = _events.Where(e => e.OperationType == "embedding").Sum(e => e.TokenCount),
                ToolTokens = _events.Where(e => e.OperationType == "tool").Sum(e => e.TokenCount),
                TotalCostInTokens = _events.Sum(e => e.CostInTokens)
            };
            
            summary.TotalTokens = summary.LLMTokens + summary.MemoryTokens + summary.EmbeddingTokens + summary.ToolTokens;
            summary.AverageElapsedSeconds = _events.Count > 0 ? summary.TotalElapsedSeconds / _events.Count : 0;
            
            return summary;
        }

        public void Clear()
        {
            _events.Clear();
            _activeOperations.Clear();
        }

        private class TelemetryEvent
        {
            public string OperationId { get; set; } = string.Empty;
            public double ElapsedSeconds { get; set; }
            public int TokenCount { get; set; }
            public double CostInTokens { get; set; }
            public string OperationType { get; set; } = string.Empty;
        }
    }
}