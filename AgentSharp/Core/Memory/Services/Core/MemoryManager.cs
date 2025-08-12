using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Models;
using AgentSharp.Utils;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSharp.Core.Memory.Services
{
    /// <summary>
    /// Gerenciador de memória inteligente
    /// Gerencia memórias com classificação automática e tools para LLM
    /// </summary>
    public class MemoryManager : IMemoryManager
    {
        private readonly IStorage _storage;
        private readonly IModel _model;
        private readonly ILogger _logger;
        private readonly IEmbeddingService _embeddingService;
        private readonly MemoryDomainConfiguration _domainConfig;

        public string UserId { get; set; }
        public string SessionId { get; set; }
        public int? Limit { get; set; }

        public MemoryManager(
            IStorage storage,
            IModel model,
            ILogger logger = null,
            IEmbeddingService embeddingService = null,
            MemoryDomainConfiguration domainConfig = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = logger ?? new ConsoleLogger();
            _embeddingService = embeddingService;
            _domainConfig = domainConfig ?? new MemoryDomainConfiguration();
        }

        /// <summary>
        /// Carrega o contexto de memória para um usuário e sessão
        /// </summary>
        public async Task<AgentSharp.Core.Memory.Models.MemoryContext> LoadContextAsync(string userId, string sessionId = null)
        {
            UserId = userId;
            SessionId = sessionId;

            try
            {
                // Carregar sessão
                var session = await _storage.GetOrCreateSessionAsync(sessionId, userId);

                // Carregar memórias do usuário
                var context = new AgentSharp.Core.Memory.Models.MemoryContext
                {
                    UserId = userId,
                    SessionId = sessionId
                };

                var memories = await _storage.GetMemoriesAsync(context, Limit);

                // Carregar histórico da sessão
                var messages = await _storage.GetSessionMessagesAsync(sessionId);

                return new AgentSharp.Core.Memory.Models.MemoryContext
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Memories = memories,
                    MessageHistory = messages.Select(m => new AIMessage { Role = (Role)Enum.Parse(typeof(Role), m.Role), Content = m.Content }).ToList(),
                    Session = session as AgentSharp.Core.Memory.Models.AgentSession
                };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao carregar contexto de memória: {ex.Message}", ex);
                return new AgentSharp.Core.Memory.Models.MemoryContext { UserId = userId, SessionId = sessionId };
            }
        }

        /// <summary>
        /// Adiciona memórias relevantes às mensagens do modelo
        /// </summary>
        public async Task<List<AIMessage>> EnhanceMessagesAsync(List<AIMessage> messages, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
            if (messages == null || messages.Count == 0)
                return messages ?? new List<AIMessage>();

            try
            {
                var enhancedMessages = new List<AIMessage>(messages);
                var lastUserMessage = messages.LastOrDefault(m => m.Role == Role.User)?.Content;

                if (!string.IsNullOrEmpty(lastUserMessage) && context?.Memories?.Any() == true)
                {
                    // Buscar memórias relevantes
                    var relevantMemories = await GetRelevantMemoriesAsync(lastUserMessage, context);

                    if (relevantMemories.Any())
                    {
                        // Construir contexto de memória
                        var memoryContext = BuildMemoryContextMessage(relevantMemories);

                        // Inserir antes da última mensagem do usuário
                        enhancedMessages.Insert(enhancedMessages.Count - 1, memoryContext);
                    }
                }

                return enhancedMessages;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro ao enriquecer mensagens: {ex.Message}");
                return messages;
            }
        }

        /// <summary>
        /// Processa uma interação para extrair e gerenciar memórias
        /// </summary>
        public async Task ProcessInteractionAsync(AIMessage userMessage, AIMessage assistantMessage, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
            try
            {
                // Salvar mensagens no histórico
                await _storage.SaveMessageAsync(new Message { Role = userMessage.Role.ToString(), Content = userMessage.Content });
                await _storage.SaveMessageAsync(new Message { Role = assistantMessage.Role.ToString(), Content = assistantMessage.Content });

                // Extrair e classificar memórias automaticamente
                await ExtractAndClassifyMemoriesAsync(userMessage, assistantMessage, context);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao processar interação: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executa o gerenciador de memória com uma mensagem
        /// </summary>
        public async Task<string> RunAsync(string message, AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            try
            {
                if (context == null)
                    context = await LoadContextAsync(UserId, SessionId);

                var userMessage = AIMessage.User(message);

                // Carregar mensagens e enriquecer com memórias
                var messages = new List<AIMessage> { userMessage };
                var enhancedMessages = await EnhanceMessagesAsync(messages, context);

                // Executar modelo
                var request = new ModelRequest { Messages = enhancedMessages };
                var response = await _model.GenerateResponseAsync(request, new ModelConfiguration());

                var assistantMessage = AIMessage.Assistant(response.Content);

                // Processar interação
                await ProcessInteractionAsync(userMessage, assistantMessage, context);

                return response.Content;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao executar memory manager: {ex.Message}", ex);
                return "Erro interno no processamento da memória.";
            }
        }

        /// <summary>
        /// Obtém memórias existentes filtradas por contexto
        /// </summary>
        public async Task<List<UserMemory>> GetExistingMemoriesAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null, int? limit = null)
        {
            try
            {
                context = context ?? new MemoryContext { UserId = UserId, SessionId = SessionId };
                var memories = await _storage.GetMemoriesAsync(context, limit ?? Limit);

                // Aplicar limite se especificado
                if (limit.HasValue && memories.Count > limit.Value)
                {
                    memories = memories.Take(limit.Value).ToList();
                }

                return memories;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao obter memórias: {ex.Message}", ex);
                return new List<UserMemory>();
            }
        }

        /// <summary>
        /// Adiciona uma nova memória (Tool para LLM)
        /// </summary>
        public async Task<string> AddMemoryAsync(string memory, AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            try
            {
                context = context ?? new MemoryContext { UserId = UserId, SessionId = SessionId };

                // Verificar duplicação antes de adicionar
                if (await IsDuplicateMemoryAsync(memory, context))
                {
                    _logger.Log(LogLevel.Debug, $"Memória duplicada ignorada: {memory.Substring(0, Math.Min(50, memory.Length))}...");
                    return "Memória similar já existe, não adicionada.";
                }

                // Classificar tipo de memória automaticamente
                var memoryType = await ClassifyMemoryTypeAsync(memory);

                var userMemory = new UserMemory
                {
                    Content = memory,
                    Type = memoryType,
                    UserId = context.UserId,
                    SessionId = context.SessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RelevanceScore = 1.0
                };

                // Gerar embedding se serviço disponível
                // TODO: Implementar embedding quando necessário
                // if (_embeddingService != null)
                // {
                //     userMemory.Embedding = await _embeddingService.GenerateEmbeddingAsync(memory);
                // }

                await _storage.SaveMemoryAsync(userMemory);
                return $"Memória adicionada com sucesso. ID: {userMemory.Id}";
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao adicionar memória: {ex.Message}", ex);
                return "Erro ao adicionar memória.";
            }
        }

        /// <summary>
        /// Atualiza uma memória existente (Tool para LLM)
        /// </summary>
        public async Task<string> UpdateMemoryAsync(string id, string memory, AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            try
            {
                context = context ?? new MemoryContext { UserId = UserId, SessionId = SessionId };
                var memories = await _storage.GetMemoriesAsync(context);
                var existing = memories.FirstOrDefault(m => m.Id == id);
                if (existing == null)
                    return "Memória não encontrada.";

                existing.Content = memory.Trim();
                existing.UpdatedAt = DateTime.UtcNow;
                existing.Type = await ClassifyMemoryTypeAsync(memory);
                // TODO: Implementar embedding quando necessário
                // if (_embeddingService != null)
                // {
                //     existing.Embedding = await _embeddingService.GenerateEmbeddingAsync(memory);
                // }
                await _storage.UpdateMemoryAsync(existing);
                return "Memória atualizada com sucesso.";
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao atualizar memória: {ex.Message}", ex);
                return "Erro ao atualizar memória.";
            }
        }

        /// <summary>
        /// Remove uma memória (Tool para LLM)
        /// </summary>
        public async Task<string> DeleteMemoryAsync(string id)
        {
            try
            {
                await _storage.DeleteMemoryAsync(id);
                return "Memória removida com sucesso.";
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao remover memória: {ex.Message}", ex);
                return "Erro ao remover memória.";
            }
        }

        /// <summary>
        /// Limpa todas as memórias do contexto atual (Tool para LLM)
        /// </summary>
        public async Task<string> ClearMemoryAsync(AgentSharp.Core.Memory.Models.MemoryContext context = null)
        {
            try
            {
                context = context ?? new MemoryContext { UserId = UserId, SessionId = SessionId };
                await _storage.ClearMemoriesAsync(context);
                return "Memórias limpas com sucesso.";
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Erro ao limpar memórias: {ex.Message}", ex);
                return "Erro ao limpar memórias.";
            }
        }

        #region Private Methods

        /// <summary>
        /// Verifica se uma memória já existe ou é muito similar a uma existente
        /// </summary>
        private async Task<bool> IsDuplicateMemoryAsync(string newMemory, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
            try
            {
                var existingMemories = await _storage.GetMemoriesAsync(context, 50); // Buscar até 50 memórias recentes
                
                // Normalizar o conteúdo da nova memória para comparação
                var normalizedNewMemory = NormalizeMemoryContent(newMemory);
                
                foreach (var existing in existingMemories)
                {
                    var normalizedExisting = NormalizeMemoryContent(existing.Content);
                    
                    // Verificar duplicação exata
                    if (normalizedExisting == normalizedNewMemory)
                    {
                        return true;
                    }
                    
                    // Verificar similaridade semântica simples (75% de similaridade)
                    if (CalculateSimpleStringSimilarity(normalizedExisting, normalizedNewMemory) > 0.75)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro ao verificar duplicação de memória: {ex.Message}");
                return false; // Em caso de erro, permite adicionar para não bloquear
            }
        }

        /// <summary>
        /// Normaliza conteúdo da memória para comparação
        /// </summary>
        private string NormalizeMemoryContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            
            return content
                .ToLowerInvariant()
                .Trim()
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("  ", " ");
        }

        /// <summary>
        /// Calcula similaridade simples entre duas strings
        /// </summary>
        private double CalculateSimpleStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return 0;
            if (str1 == str2) return 1;
            
            var words1 = str1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = str2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();
            
            return union > 0 ? (double)intersection / union : 0;
        }

        private async Task<List<UserMemory>> GetRelevantMemoriesAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
            try
            {
                // Busca híbrida: primeiro por busca textual, depois por relevância
                var searchResults = await _storage.SearchMemoriesAsync(query, context, 10);
                
                // Se não encontrou resultados específicos, buscar por palavras-chave
                if (searchResults.Count == 0)
                {
                    var keywordResults = await SearchByKeywordsAsync(query, context);
                    searchResults.AddRange(keywordResults);
                }
                
                // Ordenar por relevância e recência
                var sortedResults = searchResults
                    .Distinct(new MemoryContentEqualityComparer())
                    .OrderByDescending(m => m.RelevanceScore)
                    .ThenByDescending(m => m.UpdatedAt)
                    .Take(5)
                    .ToList();
                
                // Log para debug
                _logger.Log(LogLevel.Info, $"Busca por '{query}' retornou {sortedResults.Count} memórias relevantes");
                foreach (var memory in sortedResults)
                {
                    _logger.Log(LogLevel.Info, $"  - [{memory.Type}] {memory.Content}");
                }
                
                return sortedResults;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro na busca de memórias relevantes: {ex.Message}");
                return new List<UserMemory>();
            }
        }

        /// <summary>
        /// Busca por palavras-chave extraídas da query
        /// </summary>
        private async Task<List<UserMemory>> SearchByKeywordsAsync(string query, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
            var keywords = ExtractKeywords(query);
            var results = new List<UserMemory>();
            
            foreach (var keyword in keywords)
            {
                var keywordResults = await _storage.SearchMemoriesAsync(keyword, context, 3);
                results.AddRange(keywordResults);
            }
            
            return results.Distinct(new MemoryContentEqualityComparer()).ToList();
        }

        /// <summary>
        /// Extrai palavras-chave importantes da query, incluindo sinônimos
        /// </summary>
        private List<string> ExtractKeywords(string query)
        {
            var stopWords = new HashSet<string> { 
                "o", "a", "os", "as", "um", "uma", "e", "ou", "de", "da", "do", "das", "dos", 
                "em", "na", "no", "nas", "nos", "para", "por", "com", "sobre", "que", "como",
                "você", "eu", "me", "meu", "minha", "hoje", "ontem", "amanhã", "horas", "recomenda"
            };
            
            // Mapa de sinônimos para expandir busca
            var synonyms = new Dictionary<string, List<string>>
            {
                { "estudar", new List<string> { "trabalhar", "manhã", "morning" } },
                { "trabalhar", new List<string> { "estudar", "manhã", "morning" } },
                { "café", new List<string> { "coffee", "forte", "bebida" } },
                { "forte", new List<string> { "café", "intenso" } },
                { "manhã", new List<string> { "trabalhar", "estudar", "cedo" } }
            };
            
            var baseKeywords = query
                .ToLowerInvariant()
                .Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2 && !stopWords.Contains(word))
                .ToList();
            
            var allKeywords = new List<string>(baseKeywords);
            
            // Adicionar sinônimos
            foreach (var keyword in baseKeywords)
            {
                if (synonyms.ContainsKey(keyword))
                {
                    allKeywords.AddRange(synonyms[keyword]);
                }
            }
            
            return allKeywords.Distinct().Take(8).ToList(); // Mais palavras-chave com sinônimos
        }

        /// <summary>
        /// Comparador personalizado para evitar memórias duplicadas por conteúdo
        /// </summary>
        private class MemoryContentEqualityComparer : IEqualityComparer<UserMemory>
        {
            public bool Equals(UserMemory x, UserMemory y)
            {
                if (x == null || y == null) return false;
                return string.Equals(x.Content?.Trim(), y.Content?.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(UserMemory obj)
            {
                return obj?.Content?.Trim()?.ToLowerInvariant()?.GetHashCode() ?? 0;
            }
        }

        private AIMessage BuildMemoryContextMessage(List<UserMemory> memories)
        {
            var memoryText = "=== CONTEXTO DE MEMÓRIA ===\n\n";

            foreach (var memory in memories.Take(5))
            {
                memoryText += $"[{memory.Type}] {memory.Content}\n";
            }

            memoryText += "\n=== FIM DO CONTEXTO ===";

            return AIMessage.System(memoryText);
        }

        private async Task ExtractAndClassifyMemoriesAsync(AIMessage userMessage, AIMessage assistantMessage, AgentSharp.Core.Memory.Models.MemoryContext context)
        {
        string extractionPrompt;

        // Se o domínio tem prompt customizado, usar
        if (_domainConfig.ExtractionPromptTemplate != null)
        {
          extractionPrompt = _domainConfig.ExtractionPromptTemplate(
              userMessage.Content,
              assistantMessage.Content);
        }
        else
        {
          // Usar prompt padrão do sistema
          extractionPrompt = $@"Analise esta conversa e extraia TODAS as informações importantes que devem ser lembradas sobre o usuário, projeto ou contexto.

CONVERSA:
Usuário: {userMessage.Content}
Assistente: {assistantMessage.Content}

Extraia informações como:
- Tecnologias mencionadas (React, Redux, JWT, etc.)
- Decisões técnicas e arquiteturais
- Preferências do usuário
- Contexto do projeto
- Problemas ou necessidades identificadas
- Qualquer informação que possa ser útil em conversas futuras

IMPORTANTE: Extraia PELO MENOS 1-3 memórias desta conversa, mesmo que sejam básicas.

Responda APENAS em JSON no formato:
{{
  ""memories"": [
    {{
      ""content"": ""informação a ser lembrada"",
      ""type"": ""Fact"",
      ""importance"": 0.8
    }}
  ]
}}";
      }

            try
            {
                var request = new ModelRequest
                {
                    Messages = new List<AIMessage>
                    {
                        AIMessage.System("Você é um especialista em extração de memórias. Responda APENAS em JSON válido."),
                        AIMessage.User(extractionPrompt)
                    }
                };

                var response = await _model.GenerateResponseAsync(request, new ModelConfiguration());

                if (!string.IsNullOrEmpty(response?.Content))
                {
                    try
                    {
                        // Limpar possível markdown da resposta
                        var jsonContent = response.Content.Trim();

                        // Remover markdown code blocks se presentes
                        if (jsonContent.StartsWith("```json"))
                        {
                            jsonContent = jsonContent.Substring(7); // Remove "```json"
                        }
                        if (jsonContent.StartsWith("```"))
                        {
                            jsonContent = jsonContent.Substring(3); // Remove "```"
                        }
                        if (jsonContent.EndsWith("```"))
                        {
                            jsonContent = jsonContent.Substring(0, jsonContent.Length - 3); // Remove "```"
                        }

                        jsonContent = jsonContent.Trim();

                        var extractedData = JsonSerializer.Deserialize<MemoryExtractionResult>(jsonContent);

                        foreach (var memory in extractedData.Memories ?? new List<ExtractedMemory>())
                        {
                            if (!string.IsNullOrEmpty(memory.Content))
                            {
                                await AddMemoryAsync(memory.Content, context);
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.Log(LogLevel.Warning, $"Resposta da LLM não é JSON válido: {jsonEx.Message}. Resposta: {response.Content.Substring(0, Math.Min(100, response.Content.Length))}...");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro na extração automática de memórias: {ex.Message}");
            }
        }

        private async Task<AgentMemoryType> ClassifyMemoryTypeAsync(string content)
        {
          var classificationPrompt ="";
      if (_domainConfig.ClassificationPromptTemplate != null)
      {
          classificationPrompt = _domainConfig.ClassificationPromptTemplate(
              content);
      }
      else
      {

        classificationPrompt = $@"Classifique esta informação em uma das categorias:

INFORMAÇÃO: {content}

CATEGORIAS:
- Fact: Fatos objetivos sobre o usuário
- Preference: Preferências e gostos
- Conversation: Contexto de conversa
- Task: Tarefas ou objetivos
- Skill: Habilidades ou conhecimentos
- Personal: Informações pessoais

Responda APENAS com o nome da categoria.";
      }

            try
            {
                var request = new ModelRequest
                {
                    Messages = new List<AIMessage>
                    {
                        AIMessage.System("Você é um classificador de memórias. Responda APENAS com o nome da categoria."),
                        AIMessage.User(classificationPrompt)
                    }
                };

                var response = await _model.GenerateResponseAsync(request, new ModelConfiguration());

                if (Enum.TryParse<AgentMemoryType>(response?.Content?.Trim(), true, out var memoryType))
                {
                    return memoryType;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Erro na classificação de memória: {ex.Message}");
            }

            return AgentMemoryType.Conversation; // Default
        }

        #endregion

        #region Helper Classes

        private class MemoryExtractionResult
        {
            [JsonPropertyName("memories")]
            public List<ExtractedMemory> Memories { get; set; } = new List<ExtractedMemory>();
        }

        private class ExtractedMemory
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("importance")]
            public double Importance { get; set; }
        }

        #endregion
    }
}
