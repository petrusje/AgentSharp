using System;

namespace Agents_console
{
  internal class ConsoleObj
  {
    internal ConsoleObj WithColor(ConsoleColor color)
    {
      Console.ForegroundColor = color;
      return this;
    }

    internal ConsoleObj WriteLine(string message)
    {
      Console.WriteLine(message);
      return this;
    }

    internal ConsoleObj ResetColor()
    {
      Console.ResetColor();
      return this;
    }
  }
}
