using AgentSharp.Core;
using AgentSharp.Models;
using AgentSharp.Utils;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgentSharp.Examples
{
  /// <summary>
  /// Exemplo prático de uso de Structured Outputs com Agent
  /// </summary>
  public class AgentStructuredOutputExample
  {
    /// <summary>
    /// Resposta estruturada para análise de código
    /// </summary>
    public class CodeAnalysis
    {
      [JsonPropertyName("language")]
      public string Language { get; set; }

      [JsonPropertyName("complexity")]
      public string Complexity { get; set; }

      [JsonPropertyName("issues")]
      public Issue[] Issues { get; set; }

      [JsonPropertyName("suggestions")]
      public string[] Suggestions { get; set; }

      [JsonPropertyName("score")]
      public int Score { get; set; }
    }

    public class Issue
    {
      [JsonPropertyName("type")]
      public string Type { get; set; }

      [JsonPropertyName("severity")]
      public string Severity { get; set; }

      [JsonPropertyName("description")]
      public string Description { get; set; }

      [JsonPropertyName("line")]
      public int? Line { get; set; }
    }

    /// <summary>
    /// Resposta estruturada para extração de tarefas
    /// </summary>
    public class TaskExtraction
    {
      [JsonPropertyName("tasks")]
      public ProjectTask[] Tasks { get; set; }

      [JsonPropertyName("total_estimated_hours")]
      public double TotalEstimatedHours { get; set; }

      [JsonPropertyName("priority_summary")]
      public PrioritySummary PrioritySummary { get; set; }
    }

    public class ProjectTask
    {
      [JsonPropertyName("title")]
      public string Title { get; set; }

      [JsonPropertyName("description")]
      public string Description { get; set; }

      [JsonPropertyName("estimated_hours")]
      public double EstimatedHours { get; set; }

      [JsonPropertyName("priority")]
      public string Priority { get; set; }

      [JsonPropertyName("category")]
      public string Category { get; set; }
    }

    public class PrioritySummary
    {
      [JsonPropertyName("high_priority_count")]
      public int HighPriorityCount { get; set; }

      [JsonPropertyName("medium_priority_count")]
      public int MediumPriorityCount { get; set; }

      [JsonPropertyName("low_priority_count")]
      public int LowPriorityCount { get; set; }
    }

    /// <summary>
    /// Demonstra o uso de structured output com Agent
    /// </summary>
    public static async Task RunExample()
    {
      Console.WriteLine("🤖 Agent with Structured Output Example");
      Console.WriteLine("=======================================\n");

      try
      {
        // Criar modelo
        var model = new OpenAIModel("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

        await CodeAnalysisExample(model);
        await TaskExtractionExample(model);
        await FluentAPIExample(model);

      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Erro: {ex.Message}");
      }
    }

    /// <summary>
    /// Exemplo de análise de código estruturada
    /// </summary>
    private static async Task CodeAnalysisExample(IModel model)
    {
      Console.WriteLine("🔍 Exemplo: Análise de Código Estruturada");
      Console.WriteLine("----------------------------------------");

      // Criar agent especializado em análise de código
      var codeAnalysisAgent = new Agent<string, CodeAnalysis>(
          model,
          "Analista de Código",
          "Você é um especialista em análise de código. Analise o código fornecido e retorne uma análise estruturada.")
          .WithConfig(new ModelConfiguration().WithStructuredExtraction<CodeAnalysis>());

      var codeToAnalyze = @"
                public class Calculator {
                    public int add(int a, int b) {
                        return a + b;
                    }
                    public int divide(int a, int b) {
                        return a / b; // Problema: divisão por zero
                    }
                }";

      var agentResult = await codeAnalysisAgent.ExecuteAsync(codeToAnalyze);
      var analysis = agentResult.Data;

      if (analysis != null)
      {
        Console.WriteLine($"✅ Linguagem: {analysis.Language}");
        Console.WriteLine($"✅ Complexidade: {analysis.Complexity}");
        Console.WriteLine($"✅ Score: {analysis.Score}/100");
        Console.WriteLine("✅ Issues encontradas:");
        foreach (var issue in analysis.Issues)
        {
          Console.WriteLine($"   - {issue.Type} ({issue.Severity}): {issue.Description}");
        }
        Console.WriteLine("✅ Sugestões:");
        foreach (var suggestion in analysis.Suggestions)
        {
          Console.WriteLine($"   - {suggestion}");
        }
      }

      Console.WriteLine($"📊 Tokens: {agentResult.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Exemplo de extração de tarefas estruturada
    /// </summary>
    private static async Task TaskExtractionExample(IModel model)
    {
      Console.WriteLine("📋 Exemplo: Extração de Tarefas");
      Console.WriteLine("-------------------------------");

      // Criar agent especializado em extração de tarefas
      var taskAgent = new Agent<string, TaskExtraction>(
          model,
          "Extrator de Tarefas",
          "Você é um especialista em gestão de projetos. Extraia e organize tarefas do texto fornecido.")
          .WithConfig(new ModelConfiguration().WithStructuredExtraction<TaskExtraction>());

      var projectDescription = @"
                Precisamos implementar um sistema de autenticação completo.
                Isso inclui: criar o login com email/senha (3 horas),
                implementar OAuth2 com Google (5 horas - alta prioridade),
                adicionar verificação de email (2 horas),
                criar tela de recuperação de senha (2 horas),
                implementar logout seguro (1 hora),
                e adicionar testes unitários para tudo (4 horas - média prioridade).
                Também precisamos documentar a API (2 horas - baixa prioridade).";

      var agentResult = await taskAgent.ExecuteAsync(projectDescription);
      var extraction = agentResult.Data;

      if (extraction != null)
      {
        Console.WriteLine($"✅ Total de horas estimadas: {extraction.TotalEstimatedHours}h");
        Console.WriteLine($"✅ Resumo de prioridades:");
        Console.WriteLine($"   - Alta: {extraction.PrioritySummary.HighPriorityCount} tarefas");
        Console.WriteLine($"   - Média: {extraction.PrioritySummary.MediumPriorityCount} tarefas");
        Console.WriteLine($"   - Baixa: {extraction.PrioritySummary.LowPriorityCount} tarefas");
        Console.WriteLine("✅ Tarefas extraídas:");
        foreach (var task in extraction.Tasks)
        {
          Console.WriteLine($"   📌 {task.Title} ({task.Priority})");
          Console.WriteLine($"      ⏱️  {task.EstimatedHours}h | 🏷️  {task.Category}");
          Console.WriteLine($"      📝 {task.Description}");
        }
      }

      Console.WriteLine($"📊 Tokens: {agentResult.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Demonstra o uso da API fluente
    /// </summary>
    private static async Task FluentAPIExample(IModel model)
    {
      Console.WriteLine("✨ Exemplo: API Fluente");
      Console.WriteLine("----------------------");

      // Criar agent com configuração fluente
      var agent = new Agent<string, SentimentInsight>(
          model,
          "Analisador de Sentimento",
          "Analise o sentimento e extraia insights estruturados.")
          .WithConfig(new ModelConfiguration()
              .WithStructuredGeneration<SentimentInsight>(temperature: 0.4));

      var feedback = "O produto é incrível! A interface é muito intuitiva, mas a documentação poderia ser melhor. No geral, estou muito satisfeito.";

      var agentResult = await agent.ExecuteAsync(feedback);
      var insights = agentResult.Data;

      if (insights != null)
      {
        Console.WriteLine($"✅ Sentimento geral: {insights.OverallSentiment}");
        Console.WriteLine($"✅ Confiança: {insights.Confidence:P1}");
        Console.WriteLine("✅ Aspectos analisados:");
        foreach (var aspect in insights.Aspects)
        {
          Console.WriteLine($"   - {aspect.Category}: {aspect.Sentiment} (score: {aspect.Score})");
        }
      }

      Console.WriteLine($"📊 Tokens: {agentResult.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Classe para insights de sentimento
    /// </summary>
    public class SentimentInsight
    {
      [JsonPropertyName("overall_sentiment")]
      public string OverallSentiment { get; set; }

      [JsonPropertyName("confidence")]
      public double Confidence { get; set; }

      [JsonPropertyName("aspects")]
      public AspectAnalysis[] Aspects { get; set; }

      [JsonPropertyName("summary")]
      public string Summary { get; set; }
    }

    public class AspectAnalysis
    {
      [JsonPropertyName("category")]
      public string Category { get; set; }

      [JsonPropertyName("sentiment")]
      public string Sentiment { get; set; }

      [JsonPropertyName("score")]
      public double Score { get; set; }

      [JsonPropertyName("keywords")]
      public string[] Keywords { get; set; }
    }
  }
}
