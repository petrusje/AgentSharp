namespace AgentSharp.Examples.Contexts
{
  public class EmpresaInfo
  {
    public string Nome { get; set; } = "Empresa";
    public string Setor { get; set; } = "Tecnologia";
    public string Porte { get; set; } = "PME";
    public decimal Faturamento { get; set; } = 1_000_000;
    public int Funcionarios { get; set; } = 50;
    public string Localizacao { get; set; } = "Brasil";
    public int AnosOperacao { get; set; } = 5;
  }
}
