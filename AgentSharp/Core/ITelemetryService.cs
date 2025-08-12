using System;

namespace AgentSharp.Core
{
    /// <summary>
    /// Interface for telemetry services that track performance metrics
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Gets whether telemetry is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Starts timing an operation
        /// </summary>
        void StartOperation(string operationId);

        /// <summary>
        /// Ends timing an operation and returns elapsed time
        /// </summary>
        double EndOperation(string operationId, bool displayResult = false);

        /// <summary>
        /// Tracks LLM request timing
        /// </summary>
        void TrackLLMRequest(string operationId);

        /// <summary>
        /// Completes LLM request tracking with token count and cost in tokens
        /// </summary>
        void CompleteLLMRequest(string operationId, int tokenCount, double costInTokens = 0.0);

        /// <summary>
        /// Tracks memory operations
        /// </summary>
        void TrackMemoryOperation(string operation, double elapsedSeconds);

        /// <summary>
        /// Tracks tool execution timing
        /// </summary>
        void TrackToolExecution(string toolName, double elapsedSeconds);

        /// <summary>
        /// Gets summary of all tracked events
        /// </summary>
        TelemetrySummary GetSummary();

        /// <summary>
        /// Clears all telemetry data
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Summary of telemetry data
    /// </summary>
    public class TelemetrySummary
    {
        public int TotalEvents { get; set; }
        public double TotalElapsedSeconds { get; set; }
        public double AverageElapsedSeconds { get; set; }
        public int LLMEvents { get; set; }
        public int MemoryEvents { get; set; }
        public int ToolEvents { get; set; }
        public double TotalLLMTime { get; set; }
        public double TotalMemoryTime { get; set; }
        public double TotalToolTime { get; set; }
        public int TotalTokens { get; set; }
        public double TotalCostInTokens { get; set; }
    }
}