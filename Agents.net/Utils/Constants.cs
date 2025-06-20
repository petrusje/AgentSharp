using System;

namespace Arcana.AgentsNet.Utils
{
  /// <summary>
  /// Provides global constant values used throughout the Agents.net library.
  /// </summary>
  /// <remarks>
  /// This static class centralizes important constant values that define behavior limits,
  /// version information, and operational parameters for the library.
  /// By keeping these values in one place, it's easier to maintain consistency
  /// and modify global behavior without changing multiple files.
  /// </remarks>
  public static class Constants
  {
    /// <summary>
    /// The current version of the Agents.net library.
    /// </summary>
    /// <remarks>
    /// This version number should be updated according to semantic versioning principles
    /// when making releases of the library.
    /// </remarks>
    public const string Version = "1.0.0";

    /// <summary>
    /// Maximum allowed length for model responses in characters.
    /// </summary>
    /// <remarks>
    /// This limit helps prevent excessive memory usage and potential issues
    /// when handling unusually large responses from AI models.
    /// </remarks>
    public const int MaxResponseLength = 100000;

    /// <summary>
    /// Default timeout duration for operations in seconds.
    /// </summary>
    /// <remarks>
    /// This value is used as the default timeout for network requests and other
    /// time-sensitive operations when a specific timeout isn't provided.
    /// </remarks>
    public const int DefaultTimeoutSeconds = 60;

    /// <summary>
    /// Maximum number of retry attempts for operations that fail temporarily.
    /// </summary>
    /// <remarks>
    /// This value defines the upper limit for automatic retry attempts when
    /// encountering transient failures, such as network issues or rate limiting.
    /// </remarks>
    public const int MaxRetryAttempts = 3;

    /// <summary>
    /// Maximum number of tool calls allowed in a single execution cycle.
    /// </summary>
    /// <remarks>
    /// This limit helps prevent infinite loops or excessive resource usage
    /// that could occur if an AI model continuously calls tools without reaching
    /// a conclusion or final response.
    /// </remarks>
    public const int MaxToolCalls = 10;
  }

  /// <summary>
  /// Manages global configuration settings for the Agents.net library.
  /// </summary>
  /// <remarks>
  /// This static class provides runtime-configurable settings that affect
  /// the behavior of the entire library. These settings can be modified
  /// programmatically or through environment variables, allowing for flexible
  /// configuration without recompilation.
  /// </remarks>
  public static class GlobalConfig
  {
    /// <summary>
    /// Gets or sets whether detailed debug logging is enabled.
    /// </summary>
    /// <remarks>
    /// When set to true, the library will output more detailed log messages
    /// including debug-level information. This is useful for troubleshooting
    /// but may impact performance and create larger log files.
    /// </remarks>
    public static bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets whether result caching is globally enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, the library will cache results of AI model calls and other
    /// operations to improve performance by avoiding redundant processing.
    /// This setting can be overridden by individual components if needed.
    /// </remarks>
    public static bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets whether automatic retry logic is enabled for operations.
    /// </summary>
    /// <remarks>
    /// When enabled, failed operations that are eligible for retry (such as
    /// network requests) will be automatically retried according to the
    /// retry policy defined by the library or component.
    /// </remarks>
    public static bool EnableAutoRetry { get; set; } = true;

    /// <summary>
    /// Static constructor that initializes global configuration settings
    /// from environment variables when available.
    /// </summary>
    /// <remarks>
    /// This constructor is automatically called when the class is first accessed.
    /// It reads environment variables to configure settings, allowing for
    /// environment-specific configuration without code changes.
    /// </remarks>
    static GlobalConfig()
    {
      // Check environment variables
      var verboseEnv = Environment.GetEnvironmentVariable("AGENTS_VERBOSE_LOGGING");
      if (!string.IsNullOrEmpty(verboseEnv) && bool.TryParse(verboseEnv, out var verbose))
      {
        VerboseLogging = verbose;
      }

      // Configure logger based on settings
      if (VerboseLogging && Logger.Instance is ConsoleLogger consoleLogger)
      {
        consoleLogger.MinimumLevel = LogLevel.Debug;
      }
    }
  }
}
