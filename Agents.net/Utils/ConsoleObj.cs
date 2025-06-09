using System;

namespace Agents.net.Utils
{
  /// <summary>
  /// Classe utilitária para manipulação de saída no console.
  /// </summary>
  public class ConsoleObj
  {
    public ConsoleObj WithColor(ConsoleColor color)
    {
      Console.ForegroundColor = color;
      return this;
    }

    public ConsoleObj WriteLine(string message)
    {
      Console.WriteLine(message);
      return this;
    }

    public ConsoleObj ResetColor()
    {
      Console.ResetColor();
      return this;
    }

    public ConsoleObj WriteSeparator(char caracter = '=', int count = 50)
    {
      Console.WriteLine(new string(caracter, count));
      return this;
    }
  }
}
