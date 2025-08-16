using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSharp.Attributes;
using AgentSharp.Tools;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// ToolPack automático para captura de variáveis globais em conversas TeamChat
    /// </summary>
    public class VariableCapturingToolPack : ToolPack
    {
        private readonly GlobalVariableCollection _globalVariables;
        private readonly string _currentAgent;

        public VariableCapturingToolPack(GlobalVariableCollection globalVariables, string currentAgent)
        {
            _globalVariables = globalVariables ?? throw new ArgumentNullException(nameof(globalVariables));
            _currentAgent = currentAgent ?? throw new ArgumentNullException(nameof(currentAgent));
            
            // Definir agente executando para aplicação de propriedade
            _globalVariables.SetCurrentExecutingAgent(_currentAgent);
        }

        /// <summary>
        /// Salva o valor de uma variável global
        /// </summary>
        [FunctionCall("save_variable")]
        [FunctionCallParameter("name", "Nome da variável para salvar")]
        [FunctionCallParameter("value", "Valor da variável extraído da conversa")]  
        [FunctionCallParameter("confidence", "Nível de confiança 0.0-1.0 (padrão: 1.0)")]
        public async Task<string> SaveVariable(string name, string value, double confidence = 1.0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Erro: Nome da variável não pode ser vazio";

                if (string.IsNullOrWhiteSpace(value))
                    return "❌ Erro: Valor da variável não pode ser vazio";

                if (confidence < 0.0 || confidence > 1.0)
                    return "❌ Erro: Confiança deve estar entre 0.0 e 1.0";

                _globalVariables.SetVariable(name, value, confidence);
                
                var confidenceText = confidence < 1.0 ? $" com {confidence:P0} confiança" : "";
                return $"✅ Capturada com sucesso {name}='{value}'{confidenceText}";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"❌ Acesso negado: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                return $"❌ Variável inválida: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao salvar variável: {ex.Message}";
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Recupera o valor de uma variável global
        /// </summary>
        [FunctionCall("get_variable")]
        [FunctionCallParameter("name", "Nome da variável para recuperar")]
        public string GetVariable(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Erro: Nome da variável não pode ser vazio";

                var value = _globalVariables.GetVariableValue(name);
                if (value == null)
                    return $"📋 Variável '{name}' não foi definida ou está vazia";

                return $"📋 {name} = '{value}'";
            }
            catch (ArgumentException)
            {
                return $"❌ Variável '{name}' não existe";
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao recuperar variável: {ex.Message}";
            }
        }

        /// <summary>
        /// Lista todas as variáveis pelas quais o agente atual é responsável
        /// </summary>
        [FunctionCall("list_my_variables")]
        public string ListMyVariables()
        {
            try
            {
                var owned = _globalVariables.GetOwnedVariables(_currentAgent);
                if (!owned.Any())
                    return $"📋 Nenhuma variável atribuída ao agente '{_currentAgent}'";

                var result = new StringBuilder();
                result.AppendLine($"📋 Variáveis do agente '{_currentAgent}':");
                
                foreach (var variable in owned)
                {
                    var status = variable.IsCollected 
                        ? $"✅ '{variable.Value}'" + (variable.Confidence < 1.0 ? $" (confiança: {variable.Confidence:P0})" : "")
                        : "❌ Não coletada";
                    
                    var requiredIndicator = variable.IsRequired ? " (OBRIGATÓRIA)" : "";
                    result.AppendLine($"  • {variable.Name}: {status}{requiredIndicator}");
                    
                    if (!string.IsNullOrEmpty(variable.Description))
                        result.AppendLine($"    📄 {variable.Description}");
                }
                
                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao listar variáveis: {ex.Message}";
            }
        }

        /// <summary>
        /// Mostra o progresso geral da conversa
        /// </summary>
        [FunctionCall("get_progress")]
        public string GetProgress()
        {
            try
            {
                var progress = _globalVariables.GetProgress();
                var result = new StringBuilder();
                
                result.AppendLine("📊 PROGRESSO DA CONVERSA");
                result.AppendLine($"   Total: {progress.FilledVariables}/{progress.TotalVariables} variáveis ({progress.CompletionPercentage:P0})");
                result.AppendLine($"   Obrigatórias: {progress.RequiredFilled}/{progress.RequiredVariables} ({progress.RequiredCompletionPercentage:P0})");
                
                if (progress.IsComplete)
                {
                    result.AppendLine("🎉 Todas as variáveis obrigatórias foram coletadas!");
                }
                else
                {
                    var missing = _globalVariables.GetMissingVariables(_currentAgent);
                    var requiredMissing = missing.Where(v => v.IsRequired).ToList();
                    
                    if (requiredMissing.Any())
                    {
                        result.AppendLine("⚠️  Variáveis obrigatórias ainda em falta:");
                        foreach (var variable in requiredMissing)
                        {
                            result.AppendLine($"   • {variable.Name}: {variable.Description}");
                        }
                    }
                }

                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao obter progresso: {ex.Message}";
            }
        }

        /// <summary>
        /// Lista todas as variáveis já coletadas por outros agentes
        /// </summary>
        [FunctionCall("list_collected_variables")]
        public string ListCollectedVariables()
        {
            try
            {
                var collected = _globalVariables.GetFilledVariables();
                if (!collected.Any())
                    return "📋 Nenhuma variável foi coletada ainda";

                var result = new StringBuilder();
                result.AppendLine("📋 Variáveis já coletadas pela equipe:");
                
                foreach (var variable in collected.OrderBy(v => v.CapturedAt))
                {
                    var confidenceText = variable.Confidence < 1.0 ? $" (confiança: {variable.Confidence:P0})" : "";
                    var timeText = variable.CapturedAt?.ToString("HH:mm") ?? "";
                    result.AppendLine($"  • {variable.Name} = '{variable.Value}' por {variable.CapturedBy} às {timeText}{confidenceText}");
                }
                
                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao listar variáveis coletadas: {ex.Message}";
            }
        }

        /// <summary>
        /// Verifica se uma variável específica já foi coletada
        /// </summary>
        [FunctionCall("check_variable")]
        [FunctionCallParameter("name", "Nome da variável para verificar")]
        public string CheckVariable(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Erro: Nome da variável não pode ser vazio";

                if (!_globalVariables.HasVariable(name))
                    return $"❌ Variável '{name}' não está definida";

                var variable = _globalVariables.Variables[name];
                
                if (!variable.IsCollected)
                    return $"❌ Variável '{name}' ainda não foi coletada";

                var confidenceText = variable.Confidence < 1.0 ? $" (confiança: {variable.Confidence:P0})" : "";
                var requiredIndicator = variable.IsRequired ? " (OBRIGATÓRIA)" : "";
                
                return $"✅ {name} = '{variable.Value}' coletada por {variable.CapturedBy}{confidenceText}{requiredIndicator}";
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao verificar variável: {ex.Message}";
            }
        }

        /// <summary>
        /// Atualiza o valor de uma variável já existente (se o agente tiver permissão)
        /// </summary>
        [FunctionCall("update_variable")]
        [FunctionCallParameter("name", "Nome da variável para atualizar")]
        [FunctionCallParameter("new_value", "Novo valor para a variável")]
        [FunctionCallParameter("confidence", "Nível de confiança 0.0-1.0 (padrão: 1.0)")]
        public async Task<string> UpdateVariable(string name, string newValue, double confidence = 1.0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "❌ Erro: Nome da variável não pode ser vazio";

                if (string.IsNullOrWhiteSpace(newValue))
                    return "❌ Erro: Novo valor não pode ser vazio";

                if (!_globalVariables.HasVariable(name))
                    return $"❌ Variável '{name}' não está definida";

                var variable = _globalVariables.Variables[name];
                var oldValue = variable.Value;

                _globalVariables.SetVariable(name, newValue, confidence);
                
                var confidenceText = confidence < 1.0 ? $" com {confidence:P0} confiança" : "";
                return $"✅ Variável '{name}' atualizada de '{oldValue}' para '{newValue}'{confidenceText}";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"❌ Acesso negado para atualizar: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                return $"❌ Erro de validação: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Erro ao atualizar variável: {ex.Message}";
            }
            finally
            {
                await Task.CompletedTask;
            }
        }
    }
}