using System;
using System.Globalization;
using System.Resources;

namespace AgentSharp.console.Services
{
    /// <summary>
    /// Service for managing localization and internationalization
    /// Supports pt-BR and en-US languages
    /// </summary>
    internal class LocalizationService
    {
        private readonly ResourceManager _resourceManager;
        private readonly IConsoleService _console;
        private CultureInfo _currentCulture;

        public LocalizationService(IConsoleService console)
        {
            _console = console;
            _resourceManager = new ResourceManager("Agents_console.Resources.Messages",
                typeof(LocalizationService).Assembly);
            _currentCulture = CultureInfo.InvariantCulture; // Default to English
        }

        /// <summary>
        /// Gets the current culture being used
        /// </summary>
        public CultureInfo CurrentCulture => _currentCulture;

        /// <summary>
        /// Sets the culture for localization
        /// </summary>
        /// <param name="culture">Culture to use (pt-BR or en-US)</param>
        public void SetCulture(string culture)
        {
            try
            {
                _currentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = _currentCulture;
            }
            catch (CultureNotFoundException)
            {
                _console.WriteLine($"⚠️ Culture '{culture}' not found, using default (en-US)");
                _currentCulture = CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// Gets a localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public string GetString(string key)
        {
            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                return value ?? $"[Missing: {key}]";
            }
            catch (Exception)
            {
                return $"[Error: {key}]";
            }
        }

        /// <summary>
        /// Gets a localized string with formatted parameters
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        public string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch (Exception)
            {
                return $"[Format Error: {key}]";
            }
        }

        /// <summary>
        /// Prompts user to select language and configures the service
        /// </summary>
        /// <returns>Selected culture code</returns>
        public string PromptForLanguageSelection()
        {
            _console.WriteLine();
            _console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            _console.WriteLine("║              🌍 LANGUAGE / IDIOMA SELECTION                  ║");
            _console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            _console.WriteLine();

            // Always show language selection in both languages
            _console.WriteLine("Select your language / Selecione seu idioma:");
            _console.WriteLine("1. English (US)");
            _console.WriteLine("2. Português (BR)");
            _console.Write("Choose / Escolha (1-2): ");

            var choice = _console.ReadLine();
            _console.WriteLine();

            switch (choice)
            {
                case "1":
                    SetCulture("en-US");
                    _console.WriteLine("✅ Language set to English (US)");
                    return "en-US";
                case "2":
                    SetCulture("pt-BR");
                    _console.WriteLine("✅ Idioma configurado para Português (BR)");
                    return "pt-BR";
                default:
                    _console.WriteLine("⚠️ Invalid selection, using English (US) / Seleção inválida, usando English (US)");
                    SetCulture("en-US");
                    return "en-US";
            }
        }

        /// <summary>
        /// Checks if current culture is Portuguese (Brazil)
        /// </summary>
        public bool IsPortuguese => _currentCulture.Name.StartsWith("pt");

        /// <summary>
        /// Gets culture-specific prompt for telemetry configuration
        /// </summary>
        public string GetTelemetryPromptChar()
        {
            return IsPortuguese ? "s" : "y"; // "sim" for Portuguese, "yes" for English
        }
    }
}
