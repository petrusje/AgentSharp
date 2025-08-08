using AgentSharp.Attributes;
using AgentSharp.Core.Memory.Interfaces;
using AgentSharp.Core.Memory.Models;
using AgentSharp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentSharp.Tools
{
    /// <summary>
    /// ToolPack para gestão de memórias
    /// Permite que o LLM gerencie memórias de forma inteligente
    /// </summary>
public class SmartMemoryToolPack : ToolPack
    {
        private IMemoryManager _memoryManager;

        public SmartMemoryToolPack()
        {
            Name = "SmartMemoryToolPack";
            Description = "Ferramentas avançadas para gestão inteligente de memórias.";
            Version = "1.0.0";
        }

        public override Task InitializeAsync()
        {
            // Obter o memory manager do contexto
            if (_context != null)
            {
                _memoryManager = _context.GetMemoryManager();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adiciona uma nova memória ao contexto do usuário
        /// </summary>
        /// <param name="content">Conteúdo da memória a ser armazenada</param>
        /// <param name="type">Tipo da memória (Fact, Preference, Task, etc.)</param>
        /// <param name="importance">Nível de importância de 0.0 a 1.0</param>
        [FunctionCall("Adicionar uma nova memória importante sobre o usuário")]
        [FunctionCallParameter("content", "Conteúdo da informação a ser lembrada")]
        [FunctionCallParameter("type", "Tipo de memória: Fact, Preference, Conversation, Task, Skill, Personal")]
        [FunctionCallParameter("importance", "Importância de 0.0 a 1.0 (opcional, padrão 0.8)")]
        public async Task<string> AddMemory(string content, string type = "Conversation", double importance = 0.8)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            if (string.IsNullOrWhiteSpace(content))
                return "Conteúdo da memória não pode estar vazio.";

            try
            {
                var result = await _memoryManager.AddMemoryAsync(content);
                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao adicionar memória: {ex.Message}";
            }
        }

        /// <summary>
        /// Busca memórias existentes usando consulta textual ou semântica
        /// </summary>
        /// <param name="query">Consulta para buscar memórias relacionadas</param>
        /// <param name="limit">Número máximo de resultados (padrão 5)</param>
        [FunctionCall("Buscar memórias existentes relacionadas a um tópico, quando for responder veja se há memórias relevantes")]
        [FunctionCallParameter("query", "topico para buscar nas memórias")]
        [FunctionCallParameter("limit", "Número máximo de resultados (opcional, padrão 5)")]
        public async Task<string> SearchMemories(string query, int limit = 5)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            if (string.IsNullOrWhiteSpace(query))
                return "Consulta não pode estar vazia.";

            try
            {
                var memories = await _memoryManager.GetExistingMemoriesAsync(limit: limit);
                var filteredMemories = memories
                    .Where(m => m.Content != null && m.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Take(limit)
                    .ToList();

                if (!filteredMemories.Any())
                    return "Nenhuma memória encontrada para esta consulta.";

                var result = "=== MEMÓRIAS ENCONTRADAS ===\n";
                foreach (var memory in filteredMemories)
                {
                    result += $"[{memory.Type}] {memory.Content} (ID: {memory.Id})\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao buscar memórias: {ex.Message}";
            }
        }

        /// <summary>
        /// Atualiza uma memória existente
        /// </summary>
        /// <param name="memoryId">ID da memória a ser atualizada</param>
        /// <param name="newContent">Novo conteúdo da memória</param>
        [FunctionCall("Atualizar uma memória existente")]
        [FunctionCallParameter("memoryId", "ID da memória a ser atualizada")]
        [FunctionCallParameter("newContent", "Novo conteúdo para a memória")]
        public async Task<string> UpdateMemory(string memoryId, string newContent)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            if (string.IsNullOrWhiteSpace(memoryId) || string.IsNullOrWhiteSpace(newContent))
                return "ID da memória e novo conteúdo são obrigatórios.";

            try
            {
                var result = await _memoryManager.UpdateMemoryAsync(memoryId, newContent);
                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao atualizar memória: {ex.Message}";
            }
        }

        /// <summary>
        /// Remove uma memória específica
        /// </summary>
        /// <param name="memoryId">ID da memória a ser removida</param>
        [FunctionCall("Remover uma memória específica")]
        [FunctionCallParameter("memoryId", "ID da memória a ser removida")]
        public async Task<string> DeleteMemory(string memoryId)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            if (string.IsNullOrWhiteSpace(memoryId))
                return "ID da memória é obrigatório.";

            try
            {
                var result = await _memoryManager.DeleteMemoryAsync(memoryId);
                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao remover memória: {ex.Message}";
            }
        }

        /// <summary>
        /// Lista todas as memórias do usuário atual
        /// </summary>
        /// <param name="type">Filtrar por tipo específico (opcional)</param>
        /// <param name="limit">Número máximo de resultados (padrão 10)</param>
        [FunctionCall("Listar todas as memórias do usuário")]
        [FunctionCallParameter("type", "Tipo de memória para filtrar (opcional)")]
        [FunctionCallParameter("limit", "Número máximo de resultados (opcional, padrão 10)")]
        public async Task<string> ListMemories(string type = null, int limit = 10)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            try
            {
                var memories = await _memoryManager.GetExistingMemoriesAsync(limit: limit);

                if (!string.IsNullOrEmpty(type))
                {
                    if (Enum.TryParse<AgentMemoryType>(type, true, out var memoryType))
                    {
                        memories = memories.Where(m => m.Type == memoryType).ToList();
                    }
                }

                if (!memories.Any())
                    return "Nenhuma memória encontrada.";

                var result = "=== MEMÓRIAS DO USUÁRIO ===\n";
                foreach (var memory in memories.OrderByDescending(m => m.CreatedAt))
                {
                    result += $"[{memory.Type}] {memory.Content}\n";
                    result += $"   ID: {memory.Id} | Criado: {memory.CreatedAt:yyyy-MM-dd HH:mm}\n\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao listar memórias: {ex.Message}";
            }
        }

        /// <summary>
        /// Limpa todas as memórias do contexto atual
        /// </summary>
        /// <param name="confirmationCode">Código de confirmação (deve ser "CLEAR_ALL")</param>
        [FunctionCall("Limpar todas as memórias (CUIDADO: ação irreversível)")]
        [FunctionCallParameter("confirmationCode", "Digite 'CLEAR_ALL' para confirmar a limpeza")]
        public async Task<string> ClearAllMemories(string confirmationCode)
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            if (confirmationCode != "CLEAR_ALL")
                return "Código de confirmação incorreto. Digite 'CLEAR_ALL' para confirmar.";

            try
            {
                var result = await _memoryManager.ClearMemoryAsync();
                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao limpar memórias: {ex.Message}";
            }
        }

        /// <summary>
        /// Obtém estatísticas das memórias
        /// </summary>
        [FunctionCall("Obter estatísticas das memórias armazenadas")]
        public async Task<string> GetMemoryStats()
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            try
            {
                var memories = await _memoryManager.GetExistingMemoriesAsync();

                if (!memories.Any())
                    return "Nenhuma memória armazenada.";

                var stats = memories
                    .GroupBy(m => m.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var result = "=== ESTATÍSTICAS DE MEMÓRIA ===\n";
                result += $"Total de memórias: {memories.Count}\n\n";

                result += "Por tipo:\n";
                foreach (var stat in stats)
                {
                    result += $"  {stat.Type}: {stat.Count} memórias\n";
                }

                var oldestMemory = memories.OrderBy(m => m.CreatedAt).FirstOrDefault();
                var newestMemory = memories.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

                result += $"\nMemória mais antiga: {oldestMemory?.CreatedAt:yyyy-MM-dd HH:mm}\n";
                result += $"Memória mais recente: {newestMemory?.CreatedAt:yyyy-MM-dd HH:mm}\n";

                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao obter estatísticas: {ex.Message}";
            }
        }

        /// <summary>
        /// Consolida memórias similares automaticamente
        /// </summary>
        [FunctionCall("Consolidar memórias similares para otimizar o armazenamento")]
        public async Task<string> ConsolidateMemories()
        {
            if (_memoryManager == null)
                return "Memory Manager não disponível.";

            try
            {
                var memories = await _memoryManager.GetExistingMemoriesAsync();

                if (memories.Count < 2)
                    return "Não há memórias suficientes para consolidação.";

                // Agrupar memórias similares por tipo e conteúdo similar
                var consolidated = 0;
                var groups = memories
                    .GroupBy(m => m.Type)
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in groups)
                {
                    var similar = group
                        .GroupBy(m => GetSimpleHash(m.Content))
                        .Where(g => g.Count() > 1)
                        .ToList();

                    foreach (var similarGroup in similar)
                    {
                        var toKeep = similarGroup.OrderByDescending(m => m.UpdatedAt).First();
                        var toRemove = similarGroup.Skip(1).ToList();

                        foreach (var memory in toRemove)
                        {
                            await _memoryManager.DeleteMemoryAsync(memory.Id);
                            consolidated++;
                        }
                    }
                }

                return consolidated > 0
                    ? $"Consolidação concluída. {consolidated} memórias duplicadas foram removidas."
                    : "Nenhuma memória duplicada encontrada.";
            }
            catch (Exception ex)
            {
                return $"Erro na consolidação: {ex.Message}";
            }
        }

        private string GetSimpleHash(string content)
        {
            // Hash simples para detectar conteúdo similar
            return content?.Trim().ToLowerInvariant().Replace(" ", "").Substring(0, Math.Min(50, content.Length));
        }
    }
}
