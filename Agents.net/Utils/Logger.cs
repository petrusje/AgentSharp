using System;

namespace Agents.net.Utils
{
    /// <summary>
    /// Log level
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        None
    }
    
    /// <summary>
    /// Logger interface
    /// </summary>
    public interface ILogger
    {
        void Log(LogLevel level, string message, Exception exception = null);
    }
    
    /// <summary>
    /// Default logger that writes to the console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;
        
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            if (level < MinimumLevel)
                return;
                
            var color = Console.ForegroundColor;
            
            switch (level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] [{level}] {message}");
            
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.GetType().Name}");
                Console.WriteLine($"Message: {exception.Message}");
                Console.WriteLine($"StackTrace: {exception.StackTrace}");
            }
            
            Console.ForegroundColor = color;
        }
    }
    
    /// <summary>
    /// Static class for managing logging
    /// </summary>
    public static class Logger
    {
        private static ILogger _instance = new ConsoleLogger();
        
        public static ILogger Instance
        {
            get => _instance;
            set => _instance = value ?? new ConsoleLogger();
        }
        
        public static void Debug(string message, Exception exception = null) =>
            Instance.Log(LogLevel.Debug, message, exception);
            
        public static void Info(string message, Exception exception = null) =>
            Instance.Log(LogLevel.Info, message, exception);
            
        public static void Warning(string message, Exception exception = null) =>
            Instance.Log(LogLevel.Warning, message, exception);
            
        public static void Error(string message, Exception exception = null) =>
            Instance.Log(LogLevel.Error, message, exception);
    }
}