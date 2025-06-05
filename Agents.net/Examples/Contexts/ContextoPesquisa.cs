using System;

namespace Agents.net.Examples
{
    public class ContextoPesquisa
    {
        public string TopicoPesquisa { get; set; }
        public string ProfundidadeAnalise { get; set; } = "Básica";
        public string PublicoAlvo { get; set; } = "Geral";
        public DateTime InicioWorkflow { get; set; } = DateTime.Now;
        
        // Propriedades para fluxo de dados entre etapas do workflow
        public string DadosPesquisa { get; set; }
        public string AnaliseEstrategica { get; set; }
        public string RelatorioFinal { get; set; }
        public string RelatorioRevisado { get; set; }
    }
}
