using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AgentSharp.Core;
using AgentSharp.console.Services;

namespace AgentSharp.console.Services
{
    /// <summary>
    /// Service for tracking and reporting detailed telemetry data
    /// Measures LLM response times, memory operations, tool executions, etc.
    /// </summary>
    internal class TelemetryService : ITelemetryService
    {
        private readonly LocalizationService _localization;
        private readonly IConsoleService _console;
        private readonly ConcurrentDictionary<string, Stopwatch> _activeTimers;
        private readonly ConcurrentBag<TelemetryEvent> _events;
        private volatile bool _isEnabled;
        private const int MAX_EVENTS = 1000; // Limite para evitar vazamento de memória

        public TelemetryService(LocalizationService localization, IConsoleService console)
        {
            _localization = localization;
            _console = console;
            _activeTimers = new ConcurrentDictionary<string, Stopwatch>();
            _events = new ConcurrentBag<TelemetryEvent>();
            _isEnabled = false;
        }

        /// <summary>
        /// Gets or sets whether telemetry is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Prompts user to enable/disable telemetry
        /// </summary>
        public void PromptForTelemetryConfiguration()
        {
            _console.Write(_localization.GetString("TelemetryPrompt"));
            var response = _console.ReadLine().Trim().ToLower();

            var enableChar = _localization.GetTelemetryPromptChar().ToLower();
            _isEnabled = response == enableChar || response == "yes" || response == "y" || response == "sim" || response == "s";

            _console.WriteLine(_isEnabled
                ? _localization.GetString("TelemetryEnabled")
                : _localization.GetString("TelemetryDisabled"));
            _console.WriteLine();
        }

        /// <summary>
        /// Starts timing an operation
        /// </summary>
        /// <param name="operationId">Unique identifier for the operation</param>
        public void StartOperation(string operationId)
        {
            if (!_isEnabled) return;

            var stopwatch = _activeTimers.AddOrUpdate(operationId,
                _ => Stopwatch.StartNew(),
                (_, existing) => { existing.Restart(); return existing; });
        }

        /// <summary>
        /// Ends timing an operation and optionally displays results
        /// </summary>
        /// <param name="operationId">Operation identifier</param>
        /// <param name="displayResult">Whether to immediately display the result</param>
        /// <returns>Elapsed time in milliseconds</returns>
        public double EndOperation(string operationId, bool displayResult = false)
        {
            if (!_isEnabled) return 0;

            if (!_activeTimers.TryRemove(operationId, out var stopwatch))
                return 0;

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed.TotalSeconds;

            // Implementar limpeza automática para evitar vazamento de memória
            if (_events.Count >= MAX_EVENTS)
            {
                CleanupOldEvents();
            }

            var telemetryEvent = new TelemetryEvent
            {
                OperationId = operationId,
                ElapsedSeconds = elapsed,
                Timestamp = DateTime.Now
            };

            _events.Add(telemetryEvent);

            if (displayResult)
            {
                DisplayOperationResult(operationId, elapsed);
            }

            return elapsed;
        }

        /// <summary>
        /// Tracks and displays LLM request timing
        /// </summary>
        /// <param name="operationId">Operation identifier</param>
        public void TrackLLMRequest(string operationId)
        {
            if (!_isEnabled) return;

            _console.WriteLine(_localization.GetString("TelemetryLLMStart"));
            StartOperation(operationId);
        }

        /// <summary>
        /// Completes LLM request tracking and displays detailed results
        /// </summary>
        /// <param name="operationId">Operation identifier</param>
        /// <param name="tokenCount">Number of tokens used</param>
        /// <param name="costInTokens">Cost expressed in token count for consistency</param>
        public void CompleteLLMRequest(string operationId, int tokenCount, double costInTokens = 0.0)
        {
            if (!_isEnabled) return;

            var elapsed = EndOperation(operationId);

            // Encontrar e atualizar o evento com dados de tokens
            var eventsList = _events.ToList();
            var evt = eventsList.LastOrDefault(e => e.OperationId == operationId);
            if (evt != null)
            {
                evt.TokenCount = tokenCount;
                evt.CostInTokens = costInTokens;
                evt.OperationType = "llm";
            }

            // Usar costInTokens como referência de "custo" em tokens, não USD
            _console.WriteLine(_localization.GetString("TelemetryLLMComplete", elapsed, tokenCount));
        }

        /// <summary>
        /// Tracks memory operations (save, load, search)
        /// </summary>
        /// <param name="operation">Type of memory operation</param>
        /// <param name="elapsedSeconds">Time taken for the operation</param>
        public void TrackMemoryOperation(string operation, double elapsedSeconds)
        {
            TrackMemoryOperation(operation, elapsedSeconds, 0);
        }

        /// <summary>
        /// Tracks memory operations with token usage
        /// </summary>
        /// <param name="operation">Type of memory operation</param>
        /// <param name="elapsedSeconds">Time taken for the operation</param>
        /// <param name="tokenCount">Number of tokens used</param>
        public void TrackMemoryOperation(string operation, double elapsedSeconds, int tokenCount)
        {
            if (!_isEnabled) return;

            var evt = new TelemetryEvent
            {
                OperationId = $"memory_{operation}",
                ElapsedSeconds = elapsedSeconds,
                Timestamp = DateTime.Now,
                TokenCount = tokenCount,
                OperationType = "memory"
            };
            _events.Add(evt);

            if (tokenCount > 0)
                _console.WriteLine(_localization.GetString("TelemetryMemoryOperationWithTokens", operation, elapsedSeconds, tokenCount));
            else
                _console.WriteLine(_localization.GetString("TelemetryMemoryOperation", operation, elapsedSeconds));
        }

        /// <summary>
        /// Tracks embedding operations with token usage
        /// </summary>
        /// <param name="operation">Type of embedding operation</param>
        /// <param name="elapsedSeconds">Time taken for the operation</param>
        /// <param name="tokenCount">Number of tokens used</param>
        public void TrackEmbeddingOperation(string operation, double elapsedSeconds, int tokenCount)
        {
            if (!_isEnabled) return;

            var evt = new TelemetryEvent
            {
                OperationId = $"embedding_{operation}",
                ElapsedSeconds = elapsedSeconds,
                Timestamp = DateTime.Now,
                TokenCount = tokenCount,
                OperationType = "embedding"
            };
            _events.Add(evt);

            _console.WriteLine(_localization.GetString("TelemetryEmbeddingOperation", operation, elapsedSeconds, tokenCount));
        }

        /// <summary>
        /// Tracks tool execution timing
        /// </summary>
        /// <param name="toolName">Name of the tool executed</param>
        /// <param name="elapsedSeconds">Time taken for execution</param>
        public void TrackToolExecution(string toolName, double elapsedSeconds)
        {
            TrackToolExecution(toolName, elapsedSeconds, 0);
        }

        /// <summary>
        /// Tracks tool execution timing with token usage
        /// </summary>
        /// <param name="toolName">Name of the tool executed</param>
        /// <param name="elapsedSeconds">Time taken for execution</param>
        /// <param name="tokenCount">Number of tokens used</param>
        public void TrackToolExecution(string toolName, double elapsedSeconds, int tokenCount)
        {
            if (!_isEnabled) return;

            var evt = new TelemetryEvent
            {
                OperationId = $"tool_{toolName}",
                ElapsedSeconds = elapsedSeconds,
                Timestamp = DateTime.Now,
                TokenCount = tokenCount,
                OperationType = "tool"
            };
            _events.Add(evt);

            if (tokenCount > 0)
                _console.WriteLine(_localization.GetString("TelemetryToolExecutionWithTokens", toolName, elapsedSeconds, tokenCount));
            else
                _console.WriteLine(_localization.GetString("TelemetryToolExecution", toolName, elapsedSeconds));
        }

        /// <summary>
        /// Displays timing result for a specific operation
        /// </summary>
        private void DisplayOperationResult(string operationId, double elapsedSeconds)
        {
            if (!_isEnabled) return;

            switch (operationId.ToLower())
            {
                case var id when id.Contains("llm"):
                    // LLM operations are handled separately with more detail
                    break;
                case var id when id.Contains("memory"):
                    TrackMemoryOperation(operationId, elapsedSeconds);
                    break;
                case var id when id.Contains("tool"):
                    TrackToolExecution(operationId, elapsedSeconds);
                    break;
                default:
                    _console.WriteLine($"⏱️ {operationId}: {elapsedSeconds:F2}s");
                    break;
            }
        }

        /// <summary>
        /// Gets summary of all tracked events
        /// </summary>
        /// <returns>Summary of telemetry data</returns>
        public AgentSharp.Core.TelemetrySummary GetSummary()
        {
            if (!_isEnabled) return new AgentSharp.Core.TelemetrySummary();

            var eventsList = _events.ToList(); // Snapshot thread-safe da coleção

            var summary = new AgentSharp.Core.TelemetrySummary
            {
                TotalEvents = eventsList.Count,
                TotalElapsedSeconds = 0,
                AverageElapsedSeconds = 0,
                LLMEvents = 0,
                MemoryEvents = 0,
                ToolEvents = 0,
                TotalLLMTime = 0,
                TotalMemoryTime = 0,
                TotalToolTime = 0,
                LLMTokens = 0,
                MemoryTokens = 0,
                EmbeddingTokens = 0,
                ToolTokens = 0
            };

            foreach (var evt in eventsList)
            {
                summary.TotalElapsedSeconds += evt.ElapsedSeconds;
                summary.TotalTokens += evt.TokenCount;
                summary.TotalCostInTokens += evt.CostInTokens;

                // Use OperationType for more accurate categorization
                switch (evt.OperationType.ToLower())
                {
                    case "llm":
                        summary.LLMEvents++;
                        summary.TotalLLMTime += evt.ElapsedSeconds;
                        summary.LLMTokens += evt.TokenCount;
                        break;
                    case "memory":
                        summary.MemoryEvents++;
                        summary.TotalMemoryTime += evt.ElapsedSeconds;
                        summary.MemoryTokens += evt.TokenCount;
                        break;
                    case "embedding":
                        summary.MemoryEvents++; // Embeddings count as memory operations
                        summary.TotalMemoryTime += evt.ElapsedSeconds;
                        summary.EmbeddingTokens += evt.TokenCount;
                        break;
                    case "tool":
                        summary.ToolEvents++;
                        summary.TotalToolTime += evt.ElapsedSeconds;
                        summary.ToolTokens += evt.TokenCount;
                        break;
                    default:
                        // Fallback to old string-based detection
                        if (evt.OperationId.ToLower().Contains("llm"))
                        {
                            summary.LLMEvents++;
                            summary.TotalLLMTime += evt.ElapsedSeconds;
                            summary.LLMTokens += evt.TokenCount;
                        }
                        else if (evt.OperationId.ToLower().Contains("memory"))
                        {
                            summary.MemoryEvents++;
                            summary.TotalMemoryTime += evt.ElapsedSeconds;
                            summary.MemoryTokens += evt.TokenCount;
                        }
                        else if (evt.OperationId.ToLower().Contains("tool"))
                        {
                            summary.ToolEvents++;
                            summary.TotalToolTime += evt.ElapsedSeconds;
                            summary.ToolTokens += evt.TokenCount;
                        }
                        break;
                }
            }

            if (eventsList.Count > 0)
                summary.AverageElapsedSeconds = summary.TotalElapsedSeconds / eventsList.Count;

            return summary;
        }

        /// <summary>
        /// Clears all telemetry data
        /// </summary>
        public void Clear()
        {
            // Para ConcurrentBag, precisamos recriar a coleção para fazer clear
            while (_events.TryTake(out _)) { }
            _activeTimers.Clear();
        }

        /// <summary>
        /// Remove eventos antigos para evitar vazamento de memória
        /// </summary>
        private void CleanupOldEvents()
        {
            var cutoffTime = DateTime.Now.AddMinutes(-10); // Manter apenas últimos 10 minutos
            var tempEvents = new List<TelemetryEvent>();

            // Extrair todos os eventos atuais
            while (_events.TryTake(out var evt))
            {
                if (evt.Timestamp > cutoffTime)
                {
                    tempEvents.Add(evt);
                }
            }

            // Re-adicionar apenas os eventos recentes
            foreach (var evt in tempEvents)
            {
                _events.Add(evt);
            }
        }
    }

    /// <summary>
    /// Represents a single telemetry event
    /// </summary>
    internal class TelemetryEvent
    {
        public string OperationId { get; set; } = string.Empty;
        public double ElapsedSeconds { get; set; }
        public DateTime Timestamp { get; set; }
        public int TokenCount { get; set; }
        public double CostInTokens { get; set; }
        public string OperationType { get; set; } = string.Empty;
    }
}
