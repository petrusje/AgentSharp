using System;

namespace AgentSharp.console.Services
{
    /// <summary>
    /// Interface for console operations to enable testing and better separation of concerns
    /// </summary>
    internal interface IConsoleService
    {
        void Write(string value);
        void WriteLine();
        void WriteLine(string value);
        string ReadLine();
        ConsoleKeyInfo ReadKey(bool intercept);
    }

    /// <summary>
    /// Implementation of console service that wraps System.Console
    /// </summary>
    internal class ConsoleService : IConsoleService
    {
        public void Write(string value) => Console.Write(value);
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(string value) => Console.WriteLine(value);
        public string ReadLine() => Console.ReadLine() ?? string.Empty;
        public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    }
}
