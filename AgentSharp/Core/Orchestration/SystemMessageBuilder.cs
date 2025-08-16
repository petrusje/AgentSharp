using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentSharp.Core.Orchestration
{
    /// <summary>
    /// Construtor inteligente de mensagens de sistema para agentes em TeamChat
    /// </summary>
    public class SystemMessageBuilder
    {
        private readonly Dictionary<string, string> _customTemplates = new Dictionary<string, string>();
        private SystemMessageTemplate _defaultTemplate = new SystemMessageTemplate();

        /// <summary>
        /// Constrói uma mensagem de sistema para um agente específico
        /// </summary>
        /// <param name="agentName">Nome do agente</param>
        /// <param name="variables">Coleção de variáveis globais</param>
        /// <param name="messageHistory">Histórico de mensagens da conversa (opcional)</param>
        /// <param name="additionalContext">Contexto adicional (opcional)</param>
        public string BuildForAgent(string agentName, GlobalVariableCollection variables, 
            List<ConversationMessage> messageHistory = null, Dictionary<string, object> additionalContext = null)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new ArgumentException("Nome do agente não pode ser vazio", nameof(agentName));

            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            var ownedVariables = variables.GetOwnedVariables(agentName);
            var missingVariables = variables.GetMissingVariables(agentName);
            var allFilledVariables = variables.GetFilledVariables();
            var progress = variables.GetProgress();

            var template = _customTemplates.ContainsKey(agentName) 
                ? ParseTemplate(_customTemplates[agentName]) 
                : _defaultTemplate;

            var context = new MessageBuildContext
            {
                AgentName = agentName,
                OwnedVariables = ownedVariables,
                MissingVariables = missingVariables,
                FilledVariables = allFilledVariables,
                Progress = progress,
                MessageHistory = messageHistory ?? new List<ConversationMessage>(),
                AdditionalContext = additionalContext ?? new Dictionary<string, object>()
            };

            return BuildMessage(template, context);
        }

        /// <summary>
        /// Define um template personalizado para um agente específico
        /// </summary>
        public SystemMessageBuilder WithCustomTemplate(string agentName, string template)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new ArgumentException("Nome do agente não pode ser vazio", nameof(agentName));

            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Template não pode ser vazio", nameof(template));

            _customTemplates[agentName] = template;
            return this;
        }

        /// <summary>
        /// Define o template padrão para todos os agentes
        /// </summary>
        public SystemMessageBuilder WithDefaultTemplate(SystemMessageTemplate template)
        {
            _defaultTemplate = template ?? throw new ArgumentNullException(nameof(template));
            return this;
        }

        /// <summary>
        /// Remove template personalizado de um agente
        /// </summary>
        public SystemMessageBuilder RemoveCustomTemplate(string agentName)
        {
            _customTemplates.Remove(agentName);
            return this;
        }

        private string BuildMessage(SystemMessageTemplate template, MessageBuildContext context)
        {
            var message = new StringBuilder();

            // Seção de missão
            if (template.IncludeMissionSection)
            {
                message.AppendLine(BuildMissionSection(context, template));
            }

            // Seção de contexto atual
            if (template.IncludeContextSection && context.FilledVariables.Any())
            {
                message.AppendLine(BuildContextSection(context, template));
            }

            // Seção de foco imediato
            if (template.IncludeFocusSection)
            {
                message.AppendLine(BuildFocusSection(context, template));
            }

            // Seção de ferramentas
            if (template.IncludeToolsSection)
            {
                message.AppendLine(BuildToolsSection(context, template));
            }

            // Seção de progresso
            if (template.IncludeProgressSection)
            {
                message.AppendLine(BuildProgressSection(context, template));
            }

            // Seção de histórico
            if (template.IncludeHistorySection && context.MessageHistory.Count > 0)
            {
                message.AppendLine(BuildHistorySection(context, template));
            }

            // Contexto adicional
            if (template.IncludeAdditionalContext && context.AdditionalContext.Any())
            {
                message.AppendLine(BuildAdditionalContextSection(context, template));
            }

            return message.ToString().Trim();
        }

        private string BuildMissionSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"## {template.MissionSectionTitle}");
            section.AppendLine($"Você é o agente '{context.AgentName}' responsável por capturar estas variáveis:");

            foreach (var variable in context.OwnedVariables)
            {
                var status = variable.IsCollected ? "✅ COLETADA" : "❌ FALTANDO";
                var requiredIndicator = variable.IsRequired ? " (OBRIGATÓRIA)" : "";
                section.AppendLine($"- **{variable.Name}**: {variable.Description} [{status}]{requiredIndicator}");
            }

            return section.ToString();
        }

        private string BuildContextSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"\n## {template.ContextSectionTitle}");
            section.AppendLine("Variáveis já capturadas pela equipe:");

            foreach (var filled in context.FilledVariables)
            {
                var confidenceIndicator = filled.Confidence < 1.0 ? $" (confiança: {filled.Confidence:P0})" : "";
                section.AppendLine($"- **{filled.Name}**: '{filled.Value}' (por {filled.CapturedBy}){confidenceIndicator}");
            }

            return section.ToString();
        }

        private string BuildFocusSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            
            if (context.MissingVariables.Any())
            {
                section.AppendLine($"\n## {template.FocusSectionTitle}");
                section.AppendLine("Variáveis prioritárias que você precisa capturar:");

                // Variáveis obrigatórias primeiro
                var requiredMissing = context.MissingVariables.Where(v => v.IsRequired).ToList();
                foreach (var missing in requiredMissing)
                {
                    section.AppendLine($"- **{missing.Name}** (OBRIGATÓRIA): {missing.Description}");
                }

                // Variáveis opcionais depois
                var optionalMissing = context.MissingVariables.Where(v => !v.IsRequired).ToList();
                foreach (var missing in optionalMissing)
                {
                    section.AppendLine($"- **{missing.Name}** (opcional): {missing.Description}");
                }
            }
            else
            {
                section.AppendLine($"\n## STATUS: Todas as suas variáveis foram capturadas! Foque em ajudar o usuário.");
            }

            return section.ToString();
        }

        private string BuildToolsSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"\n## {template.ToolsSectionTitle}");
            section.AppendLine("- **SaveVariable**: Use quando identificar valores de variáveis na conversa");
            section.AppendLine("- **GetVariable**: Use para verificar valores capturados por outros agentes");
            section.AppendLine("- **ListMyVariables**: Use para ver suas variáveis e status");
            section.AppendLine("- **GetProgress**: Use para verificar progresso geral da conversa");
            section.AppendLine("\nCapture variáveis naturalmente durante a conversa. Use a ferramenta SaveVariable quando identificar os valores.");

            return section.ToString();
        }

        private string BuildProgressSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"\n## {template.ProgressSectionTitle}");
            section.AppendLine($"Progresso geral: {context.Progress.FilledVariables}/{context.Progress.TotalVariables} variáveis ({context.Progress.CompletionPercentage:P0})");
            section.AppendLine($"Obrigatórias: {context.Progress.RequiredFilled}/{context.Progress.RequiredVariables} ({context.Progress.RequiredCompletionPercentage:P0})");
            
            if (context.Progress.IsComplete)
            {
                section.AppendLine("🎉 **Todas as variáveis obrigatórias foram coletadas!**");
            }

            return section.ToString();
        }

        private string BuildHistorySection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"\n## {template.HistorySectionTitle}");
            
            var recentMessages = context.MessageHistory
                .Skip(Math.Max(0, context.MessageHistory.Count - template.MaxHistoryMessages))
                .ToList();

            foreach (var message in recentMessages)
            {
                var timestamp = message.Timestamp.ToString("HH:mm");
                section.AppendLine($"[{timestamp}] **{message.AgentName}**: {TruncateMessage(message.Content, template.MaxMessageLength)}");
            }

            return section.ToString();
        }

        private string BuildAdditionalContextSection(MessageBuildContext context, SystemMessageTemplate template)
        {
            var section = new StringBuilder();
            section.AppendLine($"\n## {template.AdditionalContextSectionTitle}");

            foreach (var kvp in context.AdditionalContext)
            {
                section.AppendLine($"- **{kvp.Key}**: {kvp.Value}");
            }

            return section.ToString();
        }

        private string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length <= maxLength)
                return message;

            return message.Substring(0, maxLength - 3) + "...";
        }

        private SystemMessageTemplate ParseTemplate(string templateString)
        {
            // Para versão inicial, retorna template padrão
            // Em versões futuras, implementar parsing de template customizado
            return _defaultTemplate;
        }
    }

    /// <summary>
    /// Template configurável para mensagens de sistema
    /// </summary>
    public class SystemMessageTemplate
    {
        /// <summary>
        /// Incluir seção de missão
        /// </summary>
        public bool IncludeMissionSection { get; set; } = true;

        /// <summary>
        /// Incluir seção de contexto atual
        /// </summary>
        public bool IncludeContextSection { get; set; } = true;

        /// <summary>
        /// Incluir seção de foco imediato
        /// </summary>
        public bool IncludeFocusSection { get; set; } = true;

        /// <summary>
        /// Incluir seção de ferramentas
        /// </summary>
        public bool IncludeToolsSection { get; set; } = true;

        /// <summary>
        /// Incluir seção de progresso
        /// </summary>
        public bool IncludeProgressSection { get; set; } = true;

        /// <summary>
        /// Incluir seção de histórico
        /// </summary>
        public bool IncludeHistorySection { get; set; } = false;

        /// <summary>
        /// Incluir contexto adicional
        /// </summary>
        public bool IncludeAdditionalContext { get; set; } = false;

        /// <summary>
        /// Título da seção de missão
        /// </summary>
        public string MissionSectionTitle { get; set; } = "SUA MISSÃO";

        /// <summary>
        /// Título da seção de contexto
        /// </summary>
        public string ContextSectionTitle { get; set; } = "CONTEXTO ATUAL";

        /// <summary>
        /// Título da seção de foco
        /// </summary>
        public string FocusSectionTitle { get; set; } = "FOCO IMEDIATO";

        /// <summary>
        /// Título da seção de ferramentas
        /// </summary>
        public string ToolsSectionTitle { get; set; } = "FERRAMENTAS DISPONÍVEIS";

        /// <summary>
        /// Título da seção de progresso
        /// </summary>
        public string ProgressSectionTitle { get; set; } = "PROGRESSO DA CONVERSA";

        /// <summary>
        /// Título da seção de histórico
        /// </summary>
        public string HistorySectionTitle { get; set; } = "HISTÓRICO RECENTE";

        /// <summary>
        /// Título da seção de contexto adicional
        /// </summary>
        public string AdditionalContextSectionTitle { get; set; } = "CONTEXTO ADICIONAL";

        /// <summary>
        /// Número máximo de mensagens no histórico
        /// </summary>
        public int MaxHistoryMessages { get; set; } = 5;

        /// <summary>
        /// Comprimento máximo de cada mensagem no histórico
        /// </summary>
        public int MaxMessageLength { get; set; } = 100;
    }

    /// <summary>
    /// Contexto para construção de mensagem de sistema
    /// </summary>
    internal class MessageBuildContext
    {
        public string AgentName { get; set; }
        public List<GlobalVariable> OwnedVariables { get; set; } = new List<GlobalVariable>();
        public List<GlobalVariable> MissingVariables { get; set; } = new List<GlobalVariable>();
        public List<GlobalVariable> FilledVariables { get; set; } = new List<GlobalVariable>();
        public ConversationProgress Progress { get; set; }
        public List<ConversationMessage> MessageHistory { get; set; } = new List<ConversationMessage>();
        public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Representa uma mensagem na conversa
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// Nome do agente que enviou a mensagem
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Timestamp da mensagem
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tipo da mensagem (user, agent, system)
        /// </summary>
        public string MessageType { get; set; } = "agent";

        /// <summary>
        /// Metadados adicionais
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public ConversationMessage()
        {
        }

        public ConversationMessage(string agentName, string content, string messageType = "agent")
        {
            AgentName = agentName;
            Content = content;
            MessageType = messageType;
        }
    }

    /// <summary>
    /// Builder fluente para configuração de templates
    /// </summary>
    public class SystemMessageTemplateBuilder
    {
        private readonly SystemMessageTemplate _template = new SystemMessageTemplate();

        public SystemMessageTemplateBuilder IncludeMission(bool include = true)
        {
            _template.IncludeMissionSection = include;
            return this;
        }

        public SystemMessageTemplateBuilder IncludeContext(bool include = true)
        {
            _template.IncludeContextSection = include;
            return this;
        }

        public SystemMessageTemplateBuilder IncludeFocus(bool include = true)
        {
            _template.IncludeFocusSection = include;
            return this;
        }

        public SystemMessageTemplateBuilder IncludeTools(bool include = true)
        {
            _template.IncludeToolsSection = include;
            return this;
        }

        public SystemMessageTemplateBuilder IncludeProgress(bool include = true)
        {
            _template.IncludeProgressSection = include;
            return this;
        }

        public SystemMessageTemplateBuilder IncludeHistory(bool include = true, int maxMessages = 5, int maxMessageLength = 100)
        {
            _template.IncludeHistorySection = include;
            _template.MaxHistoryMessages = maxMessages;
            _template.MaxMessageLength = maxMessageLength;
            return this;
        }

        public SystemMessageTemplateBuilder WithSectionTitle(string section, string title)
        {
            switch (section.ToLowerInvariant())
            {
                case "mission":
                    _template.MissionSectionTitle = title;
                    break;
                case "context":
                    _template.ContextSectionTitle = title;
                    break;
                case "focus":
                    _template.FocusSectionTitle = title;
                    break;
                case "tools":
                    _template.ToolsSectionTitle = title;
                    break;
                case "progress":
                    _template.ProgressSectionTitle = title;
                    break;
                case "history":
                    _template.HistorySectionTitle = title;
                    break;
                case "additional":
                    _template.AdditionalContextSectionTitle = title;
                    break;
            }
            return this;
        }

        public SystemMessageTemplate Build()
        {
            return _template;
        }
    }
}