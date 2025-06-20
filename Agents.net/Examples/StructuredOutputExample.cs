using Arcana.AgentsNet.Core;
using Arcana.AgentsNet.Models;
using Arcana.AgentsNet.Utils;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcana.AgentsNet.Examples
{
  /// <summary>
  /// Exemplo demonstrando o uso de OpenAI Structured Outputs.
  /// </summary>
  /// <remarks>
  /// Este exemplo mostra como configurar e usar structured outputs
  /// para obter respostas em formato estruturado e tipado.
  /// </remarks>
  public class StructuredOutputExample
  {
    /// <summary>
    /// Classe para demonstrar structured output - análise de sentimento
    /// </summary>
    public class SentimentAnalysis
    {
      [JsonPropertyName("sentiment")]
      public string Sentiment { get; set; }

      [JsonPropertyName("confidence")]
      public double Confidence { get; set; }

      [JsonPropertyName("reasoning")]
      public string Reasoning { get; set; }

      [JsonPropertyName("keywords")]
      public string[] Keywords { get; set; }
    }

    /// <summary>
    /// Classe para demonstrar structured output - extração de informações pessoais
    /// </summary>
    public class PersonInfo
    {
      [JsonPropertyName("name")]
      public string Name { get; set; }

      [JsonPropertyName("age")]
      public int? Age { get; set; }

      [JsonPropertyName("occupation")]
      public string Occupation { get; set; }

      [JsonPropertyName("location")]
      public string Location { get; set; }

      [JsonPropertyName("skills")]
      public string[] Skills { get; set; }

      [JsonPropertyName("contact")]
      public ContactInfo Contact { get; set; }
    }

    public class ContactInfo
    {
      [JsonPropertyName("email")]
      public string Email { get; set; }

      [JsonPropertyName("phone")]
      public string Phone { get; set; }
    }

    /// <summary>
    /// Enum para demonstrar structured output com enums
    /// </summary>
    public enum Priority
    {
      Low,
      Medium,
      High,
      Critical
    }

    /// <summary>
    /// Classe para demonstrar structured output com tarefas
    /// </summary>
    public class TaskAnalysis
    {
      [JsonPropertyName("title")]
      public string Title { get; set; }

      [JsonPropertyName("description")]
      public string Description { get; set; }

      [JsonPropertyName("priority")]
      public Priority Priority { get; set; }

      [JsonPropertyName("estimated_hours")]
      public double EstimatedHours { get; set; }

      [JsonPropertyName("tags")]
      public string[] Tags { get; set; }

      [JsonPropertyName("dependencies")]
      public string[] Dependencies { get; set; }
    }

    /// <summary>
    /// Executa exemplos de structured outputs
    /// </summary>
    public static async Task RunExamples()
    {
      Console.WriteLine("🏗️ OpenAI Structured Outputs Examples");
      Console.WriteLine("=====================================\n");

      try
      {
        // Configurar o modelo
        var model = new OpenAIModel("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

        await ExampleSentimentAnalysis(model);
        await ExamplePersonExtraction(model);
        await ExampleTaskAnalysis(model);
        await ExampleWithManualSchema(model);

      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Erro: {ex.Message}");
      }
    }

    /// <summary>
    /// Exemplo 1: Análise de sentimento estruturada
    /// </summary>
    private static async Task ExampleSentimentAnalysis(IModel model)
    {
      Console.WriteLine("📊 Exemplo 1: Análise de Sentimento Estruturada");
      Console.WriteLine("-----------------------------------------------");

      var config = new ModelConfiguration
      {
        EnableStructuredOutput = true,
        ResponseType = typeof(SentimentAnalysis),
        Temperature = 0.3
      };

      var request = new ModelRequest();
      request.Messages.Add(new AIMessage
      {
        Role = Role.User,
        Content = "Analyze the sentiment of this text: 'I absolutely love this new product! It has exceeded all my expectations and made my work so much easier. Highly recommend!'"
      });

      var response = await model.GenerateResponseAsync(request, config);

      if (response.IsStructuredResponse)
      {
        var sentiment = response.GetStructuredData<SentimentAnalysis>();

        Console.WriteLine($"✅ Sentimento: {sentiment.Sentiment}");
        Console.WriteLine($"✅ Confiança: {sentiment.Confidence:P1}");
        Console.WriteLine($"✅ Justificativa: {sentiment.Reasoning}");
        Console.WriteLine($"✅ Palavras-chave: {string.Join(", ", sentiment.Keywords)}");
      }
      else
      {
        Console.WriteLine($"❌ Resposta não estruturada: {response.Content}");
      }

      Console.WriteLine($"📊 Tokens utilizados: {response.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Exemplo 2: Extração de informações pessoais
    /// </summary>
    private static async Task ExamplePersonExtraction(IModel model)
    {
      Console.WriteLine("👤 Exemplo 2: Extração de Informações Pessoais");
      Console.WriteLine("----------------------------------------------");

      var config = new ModelConfiguration
      {
        EnableStructuredOutput = true,
        ResponseType = typeof(PersonInfo),
        Temperature = 0.1
      };

      var request = new ModelRequest();
      request.Messages.Add(new AIMessage
      {
        Role = Role.User,
        Content = @"Extract information from this text: 'Hi, I'm Maria Silva, a 32-year-old software engineer from São Paulo. 
                          I specialize in C#, Python, and cloud architecture. You can reach me at maria.silva@email.com or +55 11 99999-9999.'"
      });

      var response = await model.GenerateResponseAsync(request, config);

      if (response.IsStructuredResponse)
      {
        var person = response.GetStructuredData<PersonInfo>();

        Console.WriteLine($"✅ Nome: {person.Name}");
        Console.WriteLine($"✅ Idade: {person.Age}");
        Console.WriteLine($"✅ Profissão: {person.Occupation}");
        Console.WriteLine($"✅ Localização: {person.Location}");
        Console.WriteLine($"✅ Habilidades: {string.Join(", ", person.Skills)}");
        Console.WriteLine($"✅ Email: {person.Contact?.Email}");
        Console.WriteLine($"✅ Telefone: {person.Contact?.Phone}");
      }

      Console.WriteLine($"📊 Tokens utilizados: {response.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Exemplo 3: Análise de tarefa com enum
    /// </summary>
    private static async Task ExampleTaskAnalysis(IModel model)
    {
      Console.WriteLine("📋 Exemplo 3: Análise de Tarefa");
      Console.WriteLine("-------------------------------");

      var config = new ModelConfiguration
      {
        EnableStructuredOutput = true,
        ResponseType = typeof(TaskAnalysis),
        Temperature = 0.2
      };

      var request = new ModelRequest();
      request.Messages.Add(new AIMessage
      {
        Role = Role.User,
        Content = @"Analyze this task: 'Implement user authentication system with OAuth2, 
                          including password reset functionality and email verification. 
                          This is blocking other features and needs to be done ASAP.'"
      });

      var response = await model.GenerateResponseAsync(request, config);

      if (response.IsStructuredResponse)
      {
        var task = response.GetStructuredData<TaskAnalysis>();

        Console.WriteLine($"✅ Título: {task.Title}");
        Console.WriteLine($"✅ Descrição: {task.Description}");
        Console.WriteLine($"✅ Prioridade: {task.Priority}");
        Console.WriteLine($"✅ Estimativa: {task.EstimatedHours} horas");
        Console.WriteLine($"✅ Tags: {string.Join(", ", task.Tags)}");
        Console.WriteLine($"✅ Dependências: {string.Join(", ", task.Dependencies)}");
      }

      Console.WriteLine($"📊 Tokens utilizados: {response.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Exemplo 4: Usando schema manual
    /// </summary>
    private static async Task ExampleWithManualSchema(IModel model)
    {
      Console.WriteLine("⚙️ Exemplo 4: Schema Manual");
      Console.WriteLine("---------------------------");

      var manualSchema = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""summary"": {
                        ""type"": ""string"",
                        ""description"": ""Brief summary of the text""
                    },
                    ""word_count"": {
                        ""type"": ""integer"",
                        ""description"": ""Approximate word count""
                    },
                    ""main_topics"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string""
                        },
                        ""description"": ""Main topics discussed""
                    },
                    ""language"": {
                        ""type"": ""string"",
                        ""description"": ""Detected language""
                    }
                },
                ""required"": [""summary"", ""word_count"", ""main_topics"", ""language""]
            }";

      var config = new ModelConfiguration
      {
        EnableStructuredOutput = true,
        ResponseSchema = manualSchema,
        Temperature = 0.3
      };

      var request = new ModelRequest();
      request.Messages.Add(new AIMessage
      {
        Role = Role.User,
        Content = @"Analyze this text: 'Artificial Intelligence is revolutionizing various industries. 
                          From healthcare to finance, AI applications are becoming increasingly sophisticated. 
                          Machine learning algorithms can now process vast amounts of data to identify patterns 
                          and make predictions with remarkable accuracy.'"
      });

      var response = await model.GenerateResponseAsync(request, config);

      if (response.IsStructuredResponse)
      {
        Console.WriteLine($"✅ JSON Response: {response.Content}");
        Console.WriteLine("✅ Schema manual aplicado com sucesso!");
      }

      Console.WriteLine($"📊 Tokens utilizados: {response.Usage?.TotalTokens}\n");
    }

    /// <summary>
    /// Demonstra geração automática de schema
    /// </summary>
    public static void DemonstrateSchemaGeneration()
    {
      Console.WriteLine("🔧 Demonstração: Geração Automática de Schema");
      Console.WriteLine("=============================================\n");

      // Gerar schema para SentimentAnalysis
      var sentimentSchema = JsonSchemaGenerator.GenerateSchema<SentimentAnalysis>();
      Console.WriteLine("Schema para SentimentAnalysis:");
      Console.WriteLine(sentimentSchema);
      Console.WriteLine();

      // Gerar schema para PersonInfo
      var personSchema = JsonSchemaGenerator.GenerateSchema<PersonInfo>();
      Console.WriteLine("Schema para PersonInfo:");
      Console.WriteLine(personSchema);
      Console.WriteLine();

      // Gerar schema para TaskAnalysis
      var taskSchema = JsonSchemaGenerator.GenerateSchema<TaskAnalysis>();
      Console.WriteLine("Schema para TaskAnalysis:");
      Console.WriteLine(taskSchema);
      Console.WriteLine();
    }
  }
}
