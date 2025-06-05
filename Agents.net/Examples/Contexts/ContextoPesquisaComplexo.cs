using System;

namespace Agents.net.Examples
{
    public class ContextoPesquisaComplexo
    {
        public string AreaPesquisa { get; set; } = "Ciência da Computação";
        public string NivelProfundidade { get; set; } = "Graduação";
        public string TipoEntrega { get; set; } = "Artigo";
        public TimeSpan TempoDisponivel { get; set; } = TimeSpan.FromHours(1);
        public string[] RecursosDisponiveis { get; set; } = new[] { "Web" };
        public string[] RestricoesBusca { get; set; } = new[] { "Português" };
        public QualityMetrics MetricasQualidade { get; set; } = new QualityMetrics();
    }
}
