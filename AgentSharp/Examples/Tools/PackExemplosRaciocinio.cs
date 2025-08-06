using AgentSharp.Attributes;
using AgentSharp.Tools;

namespace AgentSharp.Examples.Tools
{
  public class PackExemplosRaciocinio : Toolkit
  {
    public PackExemplosRaciocinio()
        : base("Exemplos de Raciocínio Estruturado", "Exemplos de agentes com raciocínio estruturado para resolução de problemas empresariais")
    {

    }

    [FunctionCall("Obter desenvolvedores por tecnologia")]
    [FunctionCallParameter("tecnologia", "Tecnologia para buscar desenvolvedores (ex: React, Node.js, AngularJS, Java, AWS)")]
    string getEmployeeByTecnology(string tecnologia)
    {
      switch (tecnologia.ToLower())
      {
        case "react":
          return "14 Desenvolvedores React";
        case "node.js":
          return "20 Desenvolvedores Node.js";
        case "angularjs":
          return "15 Desenvolvedores AngularJS";
        case "java":
          return "15 Desenvolvedores Java";
        case "aws":
          return "1 Especialista AWS";
        default:
          return "5 Desenvolvedor Genérico";
      }
    }
  }
}
