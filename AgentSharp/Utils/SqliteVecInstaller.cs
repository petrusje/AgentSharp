using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AgentSharp.Utils
{
    /// <summary>
    /// Automatic installer for sqlite-vec extension binaries.
    /// Downloads the appropriate binary for the current platform from GitHub releases.
    /// </summary>
    public static class SqliteVecInstaller
    {
        private const string GITHUB_RELEASES_URL = "https://github.com/asg017/sqlite-vec/releases/download";
        private const string CURRENT_VERSION = "v0.1.6";

        /// <summary>
        /// Platform-specific binary information
        /// </summary>
        private static readonly PlatformInfo[] SupportedPlatforms =
        {
            // Windows
            new PlatformInfo("windows", "x64", "sqlite-vec-v0.1.6-windows-x86_64.zip", "vec0.dll"),

            // macOS
            new PlatformInfo("osx", "x64", "sqlite-vec-v0.1.6-macos-x86_64.tar.gz", "vec0.dylib"),
            new PlatformInfo("osx", "arm64", "sqlite-vec-v0.1.6-macos-aarch64.tar.gz", "vec0.dylib"),

            // Linux
            new PlatformInfo("linux", "x64", "sqlite-vec-v0.1.6-linux-x86_64.tar.gz", "vec0.so"),
            new PlatformInfo("linux", "arm64", "sqlite-vec-v0.1.6-linux-aarch64.tar.gz", "vec0.so")
        };

        /// <summary>
        /// Installs sqlite-vec binary for the current platform
        /// </summary>
        /// <param name="targetDirectory">Directory to install the binary (defaults to current directory)</param>
        /// <param name="forceReinstall">Force reinstallation even if binary exists</param>
        /// <returns>Path to the installed binary</returns>
        public static async Task<string> InstallAsync(string targetDirectory = null, bool forceReinstall = false)
        {
            // Fix C# 7.3 compatibility: replace null coalescing assignment
            if (targetDirectory == null)
                targetDirectory = Directory.GetCurrentDirectory();

            var platformInfo = GetCurrentPlatformInfo();
            if (platformInfo == null)
            {
                throw new PlatformNotSupportedException(
                    $"Platform not supported: {RuntimeInformation.OSDescription} - {RuntimeInformation.OSArchitecture}");
            }

            var binaryPath = Path.Combine(targetDirectory, platformInfo.BinaryName);

            // Check if already installed
            if (File.Exists(binaryPath) && !forceReinstall)
            {
                Console.WriteLine($"✅ sqlite-vec binary already exists: {binaryPath}");
                return binaryPath;
            }

            Console.WriteLine($"🔽 Downloading sqlite-vec {CURRENT_VERSION} for {platformInfo.OS}-{platformInfo.Architecture}...");

            try
            {
                await DownloadAndExtractAsync(platformInfo, targetDirectory);
                Console.WriteLine($"✅ sqlite-vec installed successfully: {binaryPath}");
                return binaryPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to install sqlite-vec: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if sqlite-vec binary is available for the current platform
        /// </summary>
        public static bool IsPlatformSupported()
        {
            return GetCurrentPlatformInfo() != null;
        }

        /// <summary>
        /// Gets the expected binary name for the current platform
        /// </summary>
        public static string GetExpectedBinaryName()
        {
            var platformInfo = GetCurrentPlatformInfo();
            return platformInfo?.BinaryName ?? "vec0.dll"; // Default fallback
        }

        /// <summary>
        /// Gets installation instructions for manual installation
        /// </summary>
        public static string GetManualInstallationInstructions()
        {
            var platformInfo = GetCurrentPlatformInfo();
            if (platformInfo == null)
            {
                return "Platform not supported for automatic installation. Please visit: https://github.com/asg017/sqlite-vec/releases";
            }

            var downloadUrl = $"{GITHUB_RELEASES_URL}/{CURRENT_VERSION}/{platformInfo.ArchiveName}";

            return $@"Manual Installation Instructions:

1. Download: {downloadUrl}
2. Extract the archive
3. Copy {platformInfo.BinaryName} to your application directory
4. Ensure the binary has execute permissions (Linux/macOS)

Alternative: Use SqliteVecInstaller.InstallAsync() for automatic installation.";
        }

        private static async Task DownloadAndExtractAsync(PlatformInfo platformInfo, string targetDirectory)
        {
            var downloadUrl = $"{GITHUB_RELEASES_URL}/{CURRENT_VERSION}/{platformInfo.ArchiveName}";
            var tempDir = Path.Combine(Path.GetTempPath(), "sqlite-vec-install");
            var tempArchive = Path.Combine(tempDir, platformInfo.ArchiveName);

            try
            {
                // Create temp directory
                Directory.CreateDirectory(tempDir);

                // Download archive
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout

                    Console.WriteLine($"   Downloading from: {downloadUrl}");
                    var response = await httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = File.Create(tempArchive))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                Console.WriteLine($"   Downloaded {new FileInfo(tempArchive).Length / 1024 / 1024:F1} MB");

                // Extract archive
                Console.WriteLine("   Extracting archive...");
                if (platformInfo.ArchiveName.EndsWith(".zip"))
                {
                    ExtractZip(tempArchive, tempDir, platformInfo.BinaryName, targetDirectory);
                }
                else if (platformInfo.ArchiveName.EndsWith(".tar.gz"))
                {
                    ExtractTarGzAsync(tempArchive, tempDir, platformInfo.BinaryName, targetDirectory);
                }

                // Set execute permissions on Unix systems
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var binaryPath = Path.Combine(targetDirectory, platformInfo.BinaryName);
                    SetExecutePermission(binaryPath);
                }
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, recursive: true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private static void ExtractZip(string archivePath, string tempDir, string binaryName, string targetDirectory)
        {
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Equals(binaryName, StringComparison.OrdinalIgnoreCase))
                    {
                        var targetPath = Path.Combine(targetDirectory, binaryName);
                        entry.ExtractToFile(targetPath, overwrite: true);
                        Console.WriteLine($"   Extracted: {binaryName}");
                        return;
                    }
                }
            }

            throw new FileNotFoundException($"Binary {binaryName} not found in archive");
        }

        private static void ExtractTarGzAsync(string archivePath, string tempDir, string binaryName, string targetDirectory)
        {
            // For .tar.gz files, we need to use external tools or implement tar extraction
            // For now, provide instructions for manual extraction
            var extractDir = Path.Combine(tempDir, "extracted");
            Directory.CreateDirectory(extractDir);

            // Try to use system tar command if available
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tar",
                    Arguments = $"-xzf \"{archivePath}\" -C \"{extractDir}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(processInfo))
                {
                    process.WaitForExit(); // Replace WaitForExitAsync with sync version for C# 7.3

                    if (process.ExitCode == 0)
                    {
                        // Find and copy the binary
                        var files = Directory.GetFiles(extractDir, binaryName, SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            var targetPath = Path.Combine(targetDirectory, binaryName);
                            File.Copy(files[0], targetPath, overwrite: true);
                            Console.WriteLine($"   Extracted: {binaryName}");
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Fall back to manual instructions
            }

            throw new NotSupportedException($"Automatic extraction of .tar.gz not supported on this system. Please extract manually:\n{GetManualInstallationInstructions()}");
        }

        private static void SetExecutePermission(string filePath)
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(processInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch
            {
                Console.WriteLine($"⚠️  Could not set execute permission for {filePath}. You may need to run: chmod +x {filePath}");
            }
        }

        private static PlatformInfo GetCurrentPlatformInfo()
        {
            var os = GetOSIdentifier();
            var arch = GetArchitectureIdentifier();

            foreach (var platform in SupportedPlatforms)
            {
                if (platform.OS == os && platform.Architecture == arch)
                {
                    return platform;
                }
            }

            return null;
        }

        private static string GetOSIdentifier()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "osx";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";

            return "unknown";
        }

        private static string GetArchitectureIdentifier()
        {
            // Replace switch expression with traditional switch statement for C# 7.3
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    return "x64";
                case Architecture.Arm64:
                    return "arm64";
                case Architecture.X86:
                    return "x86";
                case Architecture.Arm:
                    return "arm";
                default:
                    return "unknown";
            }
        }

        private class PlatformInfo
        {
            public string OS { get; }
            public string Architecture { get; }
            public string ArchiveName { get; }
            public string BinaryName { get; }

            public PlatformInfo(string os, string architecture, string archiveName, string binaryName)
            {
                OS = os;
                Architecture = architecture;
                ArchiveName = archiveName;
                BinaryName = binaryName;
            }
        }
    }
}
