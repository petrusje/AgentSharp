using System;

namespace Agents.net.Examples
{
    public class ContextoResolucaoProblemas
    {
        public string RestricaoMaisImportante { get; set; } = "Nenhuma restrição específica";
        public string budgetMaximo { get; set; } = "R$ 100.000";
        public string TipoProblema { get; set; }
        public string NivelComplexidade { get; set; }
        public string TempoDisponivel { get; set; }
    }
}
