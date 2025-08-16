using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Representa uma variável global com propriedade baseada em agente
    /// </summary>
    public class GlobalVariable
    {
        /// <summary>
        /// Nome da variável
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Agente responsável por esta variável ("any" para acesso compartilhado)
        /// </summary>
        public string OwnedBy { get; set; }

        /// <summary>
        /// Descrição de como a variável deve ser capturada
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica se a variável é obrigatória para completar a conversa
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Valor padrão da variável
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Valor atual da variável
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Indica se a variável foi coletada
        /// </summary>
        public bool IsCollected { get; set; }

        /// <summary>
        /// Nível de confiança na captura (0.0 a 1.0)
        /// </summary>
        public double Confidence { get; set; } = 1.0;

        /// <summary>
        /// Agente que capturou a variável
        /// </summary>
        public string CapturedBy { get; set; }

        /// <summary>
        /// Timestamp de quando a variável foi capturada
        /// </summary>
        public DateTime? CapturedAt { get; set; }

        /// <summary>
        /// Histórico de mudanças da variável
        /// </summary>
        public List<VariableChange> ChangeHistory { get; set; } = new List<VariableChange>();

        public GlobalVariable()
        {
        }

        public GlobalVariable(string name, string ownedBy, string description, bool isRequired = false, object defaultValue = null)
        {
            Name = name;
            OwnedBy = ownedBy;
            Description = description;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }

        /// <summary>
        /// Atualiza o valor da variável e registra a mudança
        /// </summary>
        public void UpdateValue(object newValue, string updatedBy, double confidence = 1.0)
        {
            var oldValue = Value;
            
            // Registrar mudança no histórico
            ChangeHistory.Add(new VariableChange
            {
                OldValue = oldValue,
                NewValue = newValue,
                UpdatedBy = updatedBy,
                UpdatedAt = DateTime.UtcNow,
                Confidence = confidence
            });

            // Atualizar valores
            Value = newValue;
            Confidence = confidence;
            CapturedBy = updatedBy;
            CapturedAt = DateTime.UtcNow;
            IsCollected = true;
        }

        /// <summary>
        /// Cria uma cópia profunda da variável
        /// </summary>
        public GlobalVariable DeepCopy()
        {
            return new GlobalVariable
            {
                Name = Name,
                OwnedBy = OwnedBy,
                Description = Description,
                IsRequired = IsRequired,
                DefaultValue = DefaultValue,
                Value = Value,
                IsCollected = IsCollected,
                Confidence = Confidence,
                CapturedBy = CapturedBy,
                CapturedAt = CapturedAt,
                ChangeHistory = new List<VariableChange>(ChangeHistory.Select(c => c.DeepCopy()))
            };
        }
    }

    /// <summary>
    /// Representa uma mudança no valor de uma variável
    /// </summary>
    public class VariableChange
    {
        /// <summary>
        /// Valor anterior
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Novo valor
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Agente que fez a mudança
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Timestamp da mudança
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Nível de confiança na mudança
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Origem da mudança (agent, system, migration)
        /// </summary>
        public string Source { get; set; } = "agent";

        /// <summary>
        /// Cria uma cópia profunda da mudança
        /// </summary>
        public VariableChange DeepCopy()
        {
            return new VariableChange
            {
                OldValue = OldValue,
                NewValue = NewValue,
                UpdatedBy = UpdatedBy,
                UpdatedAt = UpdatedAt,
                Confidence = Confidence,
                Source = Source
            };
        }
    }

    /// <summary>
    /// Coleção gerenciada de variáveis globais com aplicação de propriedade
    /// </summary>
    public class GlobalVariableCollection
    {
        private readonly Dictionary<string, GlobalVariable> _variables = new Dictionary<string, GlobalVariable>();
        private string _currentExecutingAgent;

        /// <summary>
        /// Agente atualmente executando (para aplicação de propriedade)
        /// </summary>
        public string CurrentExecutingAgent => _currentExecutingAgent;

        /// <summary>
        /// Todas as variáveis na coleção
        /// </summary>
        public IReadOnlyDictionary<string, GlobalVariable> Variables => _variables;

        /// <summary>
        /// Configura uma nova variável
        /// </summary>
        public void ConfigureVariable(string name, string ownedBy, string description, bool required = false, object defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome da variável não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(ownedBy))
                throw new ArgumentException("Proprietário da variável não pode ser vazio", nameof(ownedBy));

            _variables[name] = new GlobalVariable(name, ownedBy, description, required, defaultValue);
        }

        /// <summary>
        /// Remove uma variável da coleção
        /// </summary>
        public bool RemoveVariable(string name)
        {
            return _variables.Remove(name);
        }

        /// <summary>
        /// Define o agente atualmente executando
        /// </summary>
        public void SetCurrentExecutingAgent(string agentName)
        {
            _currentExecutingAgent = agentName;
        }

        /// <summary>
        /// Define o valor de uma variável com aplicação de propriedade
        /// </summary>
        public void SetVariable(string name, object value, double confidence = 1.0)
        {
            if (!_variables.TryGetValue(name, out var variable))
                throw new ArgumentException($"Variável '{name}' não definida");

            // Aplicação de propriedade
            if (variable.OwnedBy != "any" && variable.OwnedBy != _currentExecutingAgent)
                throw new UnauthorizedAccessException(
                    $"Agente '{_currentExecutingAgent}' não pode modificar variável '{name}' pertencente a '{variable.OwnedBy}'");

            // Atualizar variável
            variable.UpdateValue(value, _currentExecutingAgent, confidence);
        }

        /// <summary>
        /// Obtém o valor de uma variável (acesso de leitura global)
        /// </summary>
        public T GetVariable<T>(string name)
        {
            if (!_variables.TryGetValue(name, out var variable))
                throw new ArgumentException($"Variável '{name}' não definida");

            if (variable.Value == null)
                return default(T);

            try
            {
                return (T)variable.Value;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Não é possível converter variável '{name}' de {variable.Value.GetType()} para {typeof(T)}");
            }
        }

        /// <summary>
        /// Obtém o valor de uma variável como objeto
        /// </summary>
        public object GetVariableValue(string name)
        {
            if (!_variables.TryGetValue(name, out var variable))
                throw new ArgumentException($"Variável '{name}' não definida");

            return variable.Value;
        }

        /// <summary>
        /// Verifica se uma variável existe
        /// </summary>
        public bool HasVariable(string name)
        {
            return _variables.ContainsKey(name);
        }

        /// <summary>
        /// Obtém todas as variáveis pertencentes a um agente
        /// </summary>
        public List<GlobalVariable> GetOwnedVariables(string agentName)
        {
            return _variables.Values
                .Where(v => v.OwnedBy == agentName || v.OwnedBy == "any")
                .ToList();
        }

        /// <summary>
        /// Obtém variáveis não coletadas de um agente
        /// </summary>
        public List<GlobalVariable> GetMissingVariables(string agentName)
        {
            return _variables.Values
                .Where(v => (v.OwnedBy == agentName || v.OwnedBy == "any") && !v.IsCollected)
                .ToList();
        }

        /// <summary>
        /// Obtém todas as variáveis coletadas
        /// </summary>
        public List<GlobalVariable> GetFilledVariables()
        {
            return _variables.Values
                .Where(v => v.IsCollected)
                .ToList();
        }

        /// <summary>
        /// Obtém todas as variáveis
        /// </summary>
        public List<GlobalVariable> GetAllVariables()
        {
            return _variables.Values.ToList();
        }

        /// <summary>
        /// Calcula o progresso da conversa
        /// </summary>
        public ConversationProgress GetProgress()
        {
            var total = _variables.Count;
            var filled = _variables.Values.Count(v => v.IsCollected);
            var required = _variables.Values.Count(v => v.IsRequired);
            var requiredFilled = _variables.Values.Count(v => v.IsRequired && v.IsCollected);

            return new ConversationProgress
            {
                TotalVariables = total,
                FilledVariables = filled,
                RequiredVariables = required,
                RequiredFilled = requiredFilled,
                IsComplete = requiredFilled == required,
                CompletionPercentage = total > 0 ? (double)filled / total : 1.0
            };
        }

        /// <summary>
        /// Limpa todas as variáveis
        /// </summary>
        public void Clear()
        {
            _variables.Clear();
        }

        /// <summary>
        /// Configura múltiplas variáveis usando um builder
        /// </summary>
        public void Configure(List<GlobalVariable> variables)
        {
            foreach (var variable in variables)
            {
                _variables[variable.Name] = variable;
            }
        }

        /// <summary>
        /// Cria uma cópia profunda da coleção
        /// </summary>
        public GlobalVariableCollection DeepCopy()
        {
            var copy = new GlobalVariableCollection();
            copy._currentExecutingAgent = _currentExecutingAgent;
            
            foreach (var kvp in _variables)
            {
                copy._variables[kvp.Key] = kvp.Value.DeepCopy();
            }

            return copy;
        }
    }

    /// <summary>
    /// Progresso de uma conversa baseado em variáveis
    /// </summary>
    public class ConversationProgress
    {
        /// <summary>
        /// Total de variáveis definidas
        /// </summary>
        public int TotalVariables { get; set; }

        /// <summary>
        /// Variáveis já preenchidas
        /// </summary>
        public int FilledVariables { get; set; }

        /// <summary>
        /// Total de variáveis obrigatórias
        /// </summary>
        public int RequiredVariables { get; set; }

        /// <summary>
        /// Variáveis obrigatórias preenchidas
        /// </summary>
        public int RequiredFilled { get; set; }

        /// <summary>
        /// Indica se todas as variáveis obrigatórias foram preenchidas
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Percentual de conclusão (0.0 a 1.0)
        /// </summary>
        public double CompletionPercentage { get; set; }

        /// <summary>
        /// Percentual de variáveis obrigatórias completadas
        /// </summary>
        public double RequiredCompletionPercentage => RequiredVariables > 0 ? (double)RequiredFilled / RequiredVariables : 1.0;

        public override string ToString()
        {
            return $"Progresso: {FilledVariables}/{TotalVariables} variáveis ({CompletionPercentage:P0}). " +
                   $"Obrigatórias: {RequiredFilled}/{RequiredVariables} ({RequiredCompletionPercentage:P0})";
        }
    }

    /// <summary>
    /// Builder fluente para configuração de variáveis globais
    /// </summary>
    public class GlobalVariableBuilder
    {
        private readonly List<GlobalVariable> _variables = new List<GlobalVariable>();

        /// <summary>
        /// Adiciona uma variável com proprietário específico
        /// </summary>
        public GlobalVariableBuilder Add(string name, string ownedBy, string description, bool required = false, object defaultValue = null)
        {
            _variables.Add(new GlobalVariable(name, ownedBy, description, required, defaultValue));
            return this;
        }

        /// <summary>
        /// Adiciona uma variável compartilhada (qualquer agente pode modificar)
        /// </summary>
        public GlobalVariableBuilder AddShared(string name, string description, bool required = false, object defaultValue = null)
        {
            _variables.Add(new GlobalVariable(name, "any", description, required, defaultValue));
            return this;
        }

        /// <summary>
        /// Constrói a lista de variáveis
        /// </summary>
        public List<GlobalVariable> Build()
        {
            return new List<GlobalVariable>(_variables);
        }
    }
}