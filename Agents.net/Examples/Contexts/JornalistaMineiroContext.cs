using System;

namespace Agents.net.Examples
{
    public class JornalistaMineiroContext
    {
        public string RegiaoFoco { get; set; } = "Belo Horizonte";
        public string IdiomaPreferido { get; set; } = "pt-BR";
        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;
        public string seuNome { get; set; } = "Mauricio Mauro";
    }
}
