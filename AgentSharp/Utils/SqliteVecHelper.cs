using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AgentSharp.Utils
{
    /// <summary>
    /// Safe sqlite-vec extension detector and installation guide.
    /// No automatic downloads - provides clear instructions for manual installation.
    /// </summary>
    public static class SqliteVecHelper
    {
        private const string GITHUB_RELEASES_URL = "https://github.com/asg017/sqlite-vec/releases/tag/v0.1.6";

        /// <summary>
        /// Platform-specific binary information
        /// </summary>
        public static class BinaryInfo
        {
            public static readonly PlatformInfo Windows_x64 = new PlatformInfo("Windows x64", "vec0.dll", "sqlite-vec-v0.1.6-windows-x86_64.zip");
            public static readonly PlatformInfo macOS_Intel = new PlatformInfo("macOS Intel", "vec0.dylib", "sqlite-vec-v0.1.6-macos-x86_64.tar.gz");
            public static readonly PlatformInfo macOS_Silicon = new PlatformInfo("macOS Apple Silicon", "vec0.dylib", "sqlite-vec-v0.1.6-macos-aarch64.tar.gz");
            public static readonly PlatformInfo Linux_x64 = new PlatformInfo("Linux x64", "vec0.so", "sqlite-vec-v0.1.6-linux-x86_64.tar.gz");
            public static readonly PlatformInfo Linux_ARM64 = new PlatformInfo("Linux ARM64", "vec0.so", "sqlite-vec-v0.1.6-linux-aarch64.tar.gz");
        }

        /// <summary>
        /// Checks if sqlite-vec binary exists in expected locations
        /// </summary>
        /// <param name="customPath">Custom path to check (optional)</param>
        /// <returns>Path to binary if found, null otherwise</returns>
        public static string FindBinary(string customPath = null)
        {
            var expectedName = GetExpectedBinaryName();

            // Check custom path first
            if (!string.IsNullOrEmpty(customPath))
            {
                var customFullPath = Path.IsPathRooted(customPath) ? customPath : Path.Combine(Directory.GetCurrentDirectory(), customPath);
                if (File.Exists(customFullPath))
                    return customFullPath;
            }

            // Check common locations
            var locations = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), expectedName),
                Path.Combine(AppContext.BaseDirectory, expectedName),
                Path.Combine(Environment.CurrentDirectory, expectedName),
                expectedName // Relative path
            };

            foreach (var location in locations)
            {
                if (File.Exists(location))
                    return location;
            }

            return null;
        }

        /// <summary>
        /// Gets the expected binary name for the current platform
        /// </summary>
        public static string GetExpectedBinaryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "vec0.dll";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "vec0.dylib";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "vec0.so";

            return "vec0.dll"; // Default fallback
        }

        /// <summary>
        /// Gets the current platform information
        /// </summary>
        public static PlatformInfo GetCurrentPlatformInfo()
        {
            var os = RuntimeInformation.OSDescription;
            var arch = RuntimeInformation.OSArchitecture;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && arch == Architecture.X64)
                return BinaryInfo.Windows_x64;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return arch == Architecture.Arm64 ? BinaryInfo.macOS_Silicon : BinaryInfo.macOS_Intel;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return arch == Architecture.Arm64 ? BinaryInfo.Linux_ARM64 : BinaryInfo.Linux_x64;
            }

            // Fallback
            return new PlatformInfo($"{os} - {arch}", GetExpectedBinaryName(), "check-releases-page");
        }

        /// <summary>
        /// Provides detailed installation instructions for the current platform
        /// </summary>
        public static string GetInstallationInstructions()
        {
            var platform = GetCurrentPlatformInfo();
            var currentDir = Directory.GetCurrentDirectory();

            return $@"
🚀 SQLITE-VEC INSTALLATION GUIDE
================================

Current Platform: {platform.Name}
Required Binary: {platform.BinaryName}
Target Location: {Path.Combine(currentDir, platform.BinaryName)}

📋 INSTALLATION STEPS:

1️⃣  DOWNLOAD:
   Visit: {GITHUB_RELEASES_URL}
   Download: {platform.ArchiveName}

2️⃣  EXTRACT:
   {GetExtractionInstructions(platform)}

3️⃣  COPY:
   Copy {platform.BinaryName} to your application directory:
   📁 {currentDir}

4️⃣  PERMISSIONS (Linux/macOS only):
   chmod +x {platform.BinaryName}

5️⃣  VERIFY:
   Run your application - AgentSharp will detect the binary automatically.

🔍 ALTERNATIVE LOCATIONS:
   You can place the binary in any of these locations:
   • {Path.Combine(AppContext.BaseDirectory, platform.BinaryName)}
   • {Path.Combine(Environment.CurrentDirectory, platform.BinaryName)}
   • Specify custom path in SemanticSqliteStorage constructor

⚠️  SECURITY NOTES:
   • Only download from official GitHub releases
   • Verify file checksums if available
   • Scan downloaded files with antivirus software
   • Never execute files from untrusted sources

🆘 NEED HELP?
   Check the official documentation: https://github.com/asg017/sqlite-vec
";
        }

        /// <summary>
        /// Validates that a binary file appears to be a valid sqlite-vec extension
        /// </summary>
        /// <param name="filePath">Path to the binary file</param>
        /// <returns>True if the file appears valid</returns>
        public static bool ValidateBinary(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var fileInfo = new FileInfo(filePath);

                // Basic size check - sqlite-vec binaries are typically 1-10 MB
                if (fileInfo.Length < 100_000 || fileInfo.Length > 50_000_000)
                    return false;

                // Check file extension
                var expectedName = GetExpectedBinaryName();
                if (!filePath.EndsWith(expectedName, StringComparison.OrdinalIgnoreCase))
                    return false;

                // On Unix systems, check if file is executable
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // This is a basic check - full validation would require P/Invoke
                    return true; // Simplified for now
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a status report about sqlite-vec availability
        /// </summary>
        public static InstallationStatus CheckInstallationStatus(string customPath = null)
        {
            var binaryPath = FindBinary(customPath);
            var platform = GetCurrentPlatformInfo();

            if (binaryPath != null)
            {
                var isValid = ValidateBinary(binaryPath);
                return new InstallationStatus
                {
                    IsInstalled = true,
                    BinaryPath = binaryPath,
                    IsValid = isValid,
                    Platform = platform,
                    Message = isValid
                        ? $"✅ sqlite-vec found and appears valid: {binaryPath}"
                        : $"⚠️  sqlite-vec found but may be invalid: {binaryPath}"
                };
            }

            return new InstallationStatus
            {
                IsInstalled = false,
                BinaryPath = null,
                IsValid = false,
                Platform = platform,
                Message = $"❌ sqlite-vec not found. Expected: {platform.BinaryName}"
            };
        }

        private static string GetExtractionInstructions(PlatformInfo platform)
        {
            if (platform.ArchiveName.EndsWith(".zip"))
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "   Right-click → Extract All, or use: powershell Expand-Archive file.zip"
                    : "   Use: unzip " + platform.ArchiveName;
            }

            if (platform.ArchiveName.EndsWith(".tar.gz"))
            {
                return "   Use: tar -xzf " + platform.ArchiveName;
            }

            return "   Extract the downloaded archive";
        }

        public class PlatformInfo
        {
            public string Name { get; }
            public string BinaryName { get; }
            public string ArchiveName { get; }

            public PlatformInfo(string name, string binaryName, string archiveName)
            {
                Name = name;
                BinaryName = binaryName;
                ArchiveName = archiveName;
            }
        }

        public class InstallationStatus
        {
            public bool IsInstalled { get; set; }
            public string BinaryPath { get; set; }
            public bool IsValid { get; set; }
            public PlatformInfo Platform { get; set; }
            public string Message { get; set; }
        }
    }
}
