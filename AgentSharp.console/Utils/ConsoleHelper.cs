using System;

namespace AgentSharp.console.Utils
{
    /// <summary>
    /// Helper class for safe console operations that work in both interactive and non-interactive modes
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Safe console input that handles non-interactive mode (e.g., when input is redirected)
        /// </summary>
        public static void SafeReadKey()
        {
            try
            {
                Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                // In non-interactive mode (redirected input), just read a line
                Console.ReadLine();
            }
        }
    }
}