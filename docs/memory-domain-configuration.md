# Configuração de Domínio de Memória (MemoryDomainConfiguration)

O AgentSharp oferece um sistema poderoso para customizar como os agentes extraem, classificam e recuperam memórias através da `MemoryDomainConfiguration`. Esta funcionalidade permite adaptar o comportamento do sistema de memória para contextos específicos como medicina, direito, tecnologia, etc.

## Visão Geral

Por padrão, o AgentSharp usa prompts genéricos para gerenciar memórias. Com `MemoryDomainConfiguration`, você pode:

- **Customizar prompts de extração** para focar em informações específicas do seu domínio
- **Definir categorias customizadas** relevantes para sua área de negócio
- **Ajustar thresholds e limites** baseados nas necessidades do contexto
- **Configurar estratégias de recuperação** otimizadas para seu caso de uso

## Configuração Básica

```csharp
using AgentSharp.Core;

var config = new MemoryDomainConfiguration
{
    CustomCategories = new[] { "Symptom", "Diagnosis", "Treatment" },
    MaxMemoriesPerInteraction = 8,
    MinImportanceThreshold = 0.7
};

var agent = new Agent<Context, string>(model, storage: storage)
    .WithPersona("Você é um assistente médico")
    .WithMemoryDomainConfiguration(config);
```

## API Fluente

### Extração Customizada

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryExtraction((userMsg, assistantMsg) => $@"
        Como especialista médico, extraia APENAS informações clinicamente relevantes:
        
        Paciente: {userMsg}
        Médico: {assistantMsg}
        
        Foque em: sintomas, diagnósticos, medicamentos, alergias, tratamentos.
        Ignore: cumprimentos, conversas casuais.
        
        JSON: {{""memories"": [{{""content"": ""..."", ""type"": ""Symptom"", ""importance"": 0.9}}]}}");
```

### Classificação Customizada

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryClassification(content => $@"
        Classifique esta informação médica:
        {content}
        
        Categorias: Symptom, Diagnosis, Medication, Treatment, Allergy, TestResult
        Responda APENAS o nome da categoria.");
```

### Categorias Customizadas

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryCategories("Symptom", "Diagnosis", "Medication", "Treatment", "Allergy");
```

### Thresholds e Limites

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryThresholds(
        maxMemories: 10,      // Mais memórias para contexto médico
        minImportance: 0.8    // Threshold alto para informações críticas
    );
```

### Estratégias Avançadas

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryStrategies(
        prioritizeRecent: true,
        enableSemanticGrouping: false
    );
```

## Exemplos por Domínio

### 🏥 Assistente Médico

```csharp
var medicalAgent = new Agent<MedicalContext, string>(model, storage: storage)
    .WithPersona("Você é um assistente médico especializado")
    .WithMemoryCategories("Symptom", "Diagnosis", "Medication", "Treatment", "Allergy", "TestResult")
    .WithMemoryExtraction((userMsg, assistantMsg) => $@"
        Como especialista médico, extraia informações clinicamente relevantes:
        
        Paciente/Médico: {userMsg}
        Resposta: {assistantMsg}
        
        EXTRAIR:
        - Sintomas específicos mencionados
        - Medicamentos prescritos ou mencionados  
        - Diagnósticos ou hipóteses diagnósticas
        - Exames solicitados ou resultados
        - Alergias ou contraindicações
        - Tratamentos recomendados
        
        IGNORAR: cumprimentos, conversas casuais, informações não-médicas
        
        JSON: {{""memories"": [{{""content"": ""..."", ""type"": ""Symptom"", ""importance"": 0.9}}]}}")
    .WithMemoryThresholds(maxMemories: 8, minImportance: 0.7);
```

### ⚖️ Consultor Jurídico

```csharp
var legalAgent = new Agent<LegalContext, string>(model, storage: storage)
    .WithPersona("Você é um advogado especializado em direito empresarial")
    .WithMemoryCategories("CaseDetail", "LegalPrecedent", "ClientInfo", "Deadline", "Contract", "Evidence")
    .WithMemoryExtraction((userMsg, assistantMsg) => $@"
        Extraia informações jurídicas relevantes desta consulta:
        
        Cliente: {userMsg}
        Advogado: {assistantMsg}
        
        PRIORIZAR:
        - Detalhes específicos do caso
        - Prazos legais mencionados
        - Precedentes ou jurisprudência citada
        - Informações contratuais
        - Evidências ou documentos
        
        JSON: {{""memories"": [{{""content"": ""..."", ""type"": ""CaseDetail"", ""importance"": 0.8}}]}}")
    .WithMemoryThresholds(maxMemories: 12, minImportance: 0.6);
```

### 💻 Consultor Técnico

```csharp
var techAgent = new Agent<TechContext, string>(model, storage: storage)
    .WithPersona("Você é um consultor técnico sênior")
    .WithMemoryCategories("Architecture", "Technology", "Requirement", "Issue", "Solution", "Performance")
    .WithMemoryExtraction((userMsg, assistantMsg) => $@"
        Extraia informações técnicas desta conversa:
        
        Usuário: {userMsg}
        Consultor: {assistantMsg}
        
        FOCAR EM:
        - Tecnologias mencionadas (React, .NET, etc.)
        - Decisões arquiteturais
        - Problemas identificados
        - Soluções propostas
        - Requisitos técnicos
        
        JSON: {{""memories"": [{{""content"": ""..."", ""type"": ""Technology"", ""importance"": 0.7}}]}}")
    .WithMemoryThresholds(maxMemories: 15, minImportance: 0.4);
```

## Configuração Avançada

### Recuperação Customizada

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithMemoryRetrieval((query, existingMemories) => $@"
        Baseado na consulta: {query}
        
        Memórias disponíveis: {existingMemories}
        
        Priorize memórias médicas recentes sobre o mesmo paciente.
        Foque em informações que possam afetar o diagnóstico ou tratamento.");
```

### Configuração Completa

```csharp
var complexConfig = new MemoryDomainConfiguration
{
    // Prompts customizados
    ExtractionPromptTemplate = (user, assistant) => BuildMedicalExtractionPrompt(user, assistant),
    ClassificationPromptTemplate = content => BuildMedicalClassificationPrompt(content),
    RetrievalPromptTemplate = (query, memories) => BuildMedicalRetrievalPrompt(query, memories),
    
    // Categorias específicas
    CustomCategories = new[] { "Symptom", "Diagnosis", "Medication", "Treatment", "Allergy", "TestResult" },
    
    // Configurações de comportamento
    MaxMemoriesPerInteraction = 10,
    MinImportanceThreshold = 0.8,
    PrioritizeRecentMemories = true,
    EnableSemanticGrouping = false
};

var agent = new Agent<MedicalContext, string>(model, storage: storage)
    .WithMemoryDomainConfiguration(complexConfig);
```

## Encadeamento de Configurações

```csharp
var agent = new Agent<Context, string>(model, storage: storage)
    .WithPersona("Assistente especializado")
    .WithMemoryCategories("Category1", "Category2", "Category3")
    .WithMemoryExtraction((user, assistant) => BuildCustomPrompt(user, assistant))
    .WithMemoryClassification(content => ClassifyContent(content))
    .WithMemoryThresholds(maxMemories: 20, minImportance: 0.6)
    .WithMemoryStrategies(prioritizeRecent: true, enableSemanticGrouping: false);
```

## Compatibilidade

- ✅ **100% compatível** com código existente
- ✅ **Fallback automático** para configuração padrão quando não especificada
- ✅ **Zero breaking changes** - funciona com ou sem configuração customizada

## Casos de Uso Recomendados

| Domínio | Categorias Sugeridas | Threshold | Max Memories |
|---------|---------------------|-----------|--------------|
| **Médico** | Symptom, Diagnosis, Medication, Treatment, Allergy | 0.7-0.9 | 8-12 |
| **Jurídico** | CaseDetail, LegalPrecedent, ClientInfo, Deadline, Contract | 0.6-0.8 | 10-15 |
| **Técnico** | Architecture, Technology, Requirement, Issue, Solution | 0.4-0.7 | 12-20 |
| **Educacional** | Concept, Example, Exercise, Assessment, Progress | 0.5-0.7 | 15-25 |
| **Comercial** | Product, Customer, Opportunity, Objection, NextStep | 0.6-0.8 | 8-15 |

## Melhores Práticas

### ✅ Boas Práticas

- **Seja específico** nos prompts de extração
- **Use categorias relevantes** para seu domínio
- **Teste diferentes thresholds** para encontrar o equilíbrio ideal
- **Mantenha prompts concisos** mas informativos
- **Documente configurações** para facilitar manutenção

### ❌ Evite

- Prompts muito genéricos que não agregam valor
- Thresholds muito baixos que capturam informações irrelevantes
- Muitas categorias que confundem a classificação
- Prompts extremamente longos que custam tokens desnecessários
- Configurações que não são testadas em cenários reais

## Integração com Testes

```csharp
[Test]
public async Task Agent_WithCustomMemoryConfig_WorksCorrectly()
{
    // Arrange
    var config = new MemoryDomainConfiguration
    {
        CustomCategories = new[] { "TestCategory" },
        MaxMemoriesPerInteraction = 5,
        MinImportanceThreshold = 0.6
    };
    
    var agent = new Agent<TestContext, string>(mockModel, storage: mockStorage)
        .WithMemoryDomainConfiguration(config);
    
    // Act
    var result = await agent.ExecuteAsync("Test message");
    
    // Assert
    Assert.NotNull(result);
    var memoryConfig = agent.GetMemoryDomainConfiguration();
    Assert.Equal(5, memoryConfig.MaxMemoriesPerInteraction);
}
```

Esta funcionalidade transforma o AgentSharp em uma ferramenta ainda mais poderosa e versátil, permitindo criar assistentes especializados que verdadeiramente entendem e se adaptam ao contexto específico do seu domínio de negócio.