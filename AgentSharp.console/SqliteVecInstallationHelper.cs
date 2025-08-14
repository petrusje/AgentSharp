using System;
using System.Threading.Tasks;
using AgentSharp.Utils;
using AgentSharp.console.Utils;

namespace AgentSharp.console
{
    /// <summary>
    /// Utility for installing and managing sqlite-vec binaries safely
    /// </summary>
    public static class SqliteVecInstallationHelper
    {
        public static void CheckAndGuideInstallation()
        {
            Console.Clear();
            Console.WriteLine("=== SQLITE-VEC INSTALLATION CHECKER ===\n");

            // Check current status
            var status = SqliteVecHelper.CheckInstallationStatus();

            Console.WriteLine($"🔍 Platform Detection:");
            Console.WriteLine($"   Current Platform: {status.Platform.Name}");
            Console.WriteLine($"   Required Binary: {status.Platform.BinaryName}");
            Console.WriteLine();

            Console.WriteLine($"📋 Installation Status:");
            Console.WriteLine($"   {status.Message}");
            Console.WriteLine();

            if (!status.IsInstalled)
            {
                Console.WriteLine("❌ sqlite-vec is not installed.");
                Console.WriteLine();
                Console.WriteLine("Would you like to see installation instructions? (y/N): ");
                var response = Console.ReadLine()?.ToLower();

                if (response == "y" || response == "yes")
                {
                    ShowInstallationInstructions();
                }
            }
            else if (!status.IsValid)
            {
                Console.WriteLine("⚠️  sqlite-vec binary found but appears invalid.");
                Console.WriteLine($"   Location: {status.BinaryPath}");
                Console.WriteLine();
                Console.WriteLine("Possible issues:");
                Console.WriteLine("• Binary is corrupted");
                Console.WriteLine("• Wrong architecture/platform");
                Console.WriteLine("• File permissions (Linux/macOS)");
                Console.WriteLine();
                Console.WriteLine("Would you like to see installation instructions? (y/N): ");
                var response = Console.ReadLine()?.ToLower();

                if (response == "y" || response == "yes")
                {
                    ShowInstallationInstructions();
                }
            }
            else
            {
                Console.WriteLine("✅ sqlite-vec is properly installed and ready to use!");
                Console.WriteLine($"   Location: {status.BinaryPath}");
                Console.WriteLine();
                TestBinary();
            }
        }

        private static void ShowInstallationInstructions()
        {
            Console.Clear();
            Console.WriteLine(SqliteVecHelper.GetInstallationInstructions());

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            ConsoleHelper.SafeReadKey();
        }

        public static void ShowInstallationGuide()
        {
            ShowInstallationInstructions();
        }

        private static void TestBinary()
        {
            Console.WriteLine("🧪 Would you like to test the binary with a database connection? (y/N): ");
            var response = Console.ReadLine()?.ToLower();

            if (response == "y" || response == "yes")
            {
                try
                {
                    Console.WriteLine("Testing sqlite-vec connection...");

                    // This would normally create a SemanticSqliteStorage instance
                    // For now, just show that it would work
                    Console.WriteLine("✅ Binary test would be performed here");
                    Console.WriteLine("   (This would create a test SemanticSqliteStorage instance)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Test failed: {ex.Message}");
                    Console.WriteLine();
                    Console.WriteLine("This might indicate:");
                    Console.WriteLine("• Binary is corrupted or incompatible");
                    Console.WriteLine("• Missing dependencies");
                    Console.WriteLine("• Permission issues");
                }
            }
        }

        public static void ShowPlatformInfo()
        {
            Console.Clear();
            Console.WriteLine("=== PLATFORM INFORMATION ===\n");

            var platform = SqliteVecHelper.GetCurrentPlatformInfo();

            Console.WriteLine($"Detected Platform: {platform.Name}");
            Console.WriteLine($"Required Binary: {platform.BinaryName}");
            Console.WriteLine($"Archive Name: {platform.ArchiveName}");
            Console.WriteLine();

            Console.WriteLine("Supported Platforms:");
            Console.WriteLine("• Windows x64 - vec0.dll");
            Console.WriteLine("• macOS Intel - vec0.dylib");
            Console.WriteLine("• macOS Apple Silicon - vec0.dylib");
            Console.WriteLine("• Linux x64 - vec0.so");
            Console.WriteLine("• Linux ARM64 - vec0.so");
            Console.WriteLine();

            var binaryPath = SqliteVecHelper.FindBinary();
            if (binaryPath != null)
            {
                Console.WriteLine($"✅ Binary found: {binaryPath}");
            }
            else
            {
                Console.WriteLine($"❌ Binary not found. Looking for: {SqliteVecHelper.GetExpectedBinaryName()}");
            }
        }

        public static void ShowSecurityNotes()
        {
            Console.Clear();
            Console.WriteLine("=== SECURITY CONSIDERATIONS ===\n");

            Console.WriteLine("🔒 WHY NO AUTOMATIC DOWNLOADS?");
            Console.WriteLine("Automatic binary downloads pose security risks:");
            Console.WriteLine("• Man-in-the-middle attacks");
            Console.WriteLine("• Compromised release infrastructure");
            Console.WriteLine("• Supply chain attacks");
            Console.WriteLine("• Malicious binary injection");
            Console.WriteLine();

            Console.WriteLine("🛡️  SAFE INSTALLATION PRACTICES:");
            Console.WriteLine("1. Only download from official GitHub releases");
            Console.WriteLine("2. Verify checksums when available");
            Console.WriteLine("3. Scan files with antivirus software");
            Console.WriteLine("4. Use https:// connections only");
            Console.WriteLine("5. Inspect file permissions and ownership");
            Console.WriteLine();

            Console.WriteLine("✅ THIS APPROACH IS SAFER:");
            Console.WriteLine("• Manual verification required");
            Console.WriteLine("• Full user control over process");
            Console.WriteLine("• Clear audit trail");
            Console.WriteLine("• No network dependencies in production");
            Console.WriteLine("• Explicit consent for each step");
            Console.WriteLine();

            Console.WriteLine("📋 VERIFICATION CHECKLIST:");
            Console.WriteLine("□ Downloaded from official GitHub repository");
            Console.WriteLine("□ HTTPS connection used");
            Console.WriteLine("□ File size reasonable (1-10 MB)");
            Console.WriteLine("□ Correct file extension for platform");
            Console.WriteLine("□ Execute permissions set (Linux/macOS)");
            Console.WriteLine("□ Antivirus scan completed");
        }
    }
}
