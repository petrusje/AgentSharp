using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Agents.net.Core
{
  public enum NextAction
  {
    Continue,
    Validate,
    FinalAnswer,
    Reset
  }

  public class ReasoningStep
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; }

    [JsonPropertyName("next_action")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NextAction? NextAction { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }
  }

  public class ReasoningSteps
  {
    [JsonPropertyName("reasoning_steps")]
    public List<ReasoningStep> Steps { get; set; } = new List<ReasoningStep>();
  }

  /// <summary>
  /// Resultado do processamento de reasoning com conteúdo formatado e steps estruturados
  /// </summary>
  public class ReasoningResult
  {
    public string Content { get; set; }
    public List<ReasoningStep> Steps { get; set; }

    public ReasoningResult(string content, List<ReasoningStep> steps)
    {
      Content = content;
      Steps = steps ?? new List<ReasoningStep>();
    }
  }
}
