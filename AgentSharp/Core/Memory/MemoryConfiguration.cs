using System;

public class MemoryConfiguration
  {
      // Prompts customizáveis - null = usar padrão
      public Func<string, string, string> ExtractionPromptTemplate { get; set; }
      public Func<string, string> ClassificationPromptTemplate { get; set; }
      public Func<string, string, string> RetrievalPromptTemplate { get; set; }

      // Configurações opcionais
      public string[] CustomCategories { get; set; }
      public int MaxMemoriesPerInteraction { get; set; } = 5;
      public double MinImportanceThreshold { get; set; } = 0.5;
  }
