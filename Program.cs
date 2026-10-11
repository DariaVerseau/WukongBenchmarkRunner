using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

namespace WukongBenchmarkRunner
{
    class Program
    {
        static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"b1\Saved\Config\Windows\GameUserSettings.ini");

        static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"b1\Saved\Logs");

        static void Main(string[] args)
        {
            Console.WriteLine("======================================================");
            Console.WriteLine(" Black Myth: Wukong Benchmark Automation Tool");
            Console.WriteLine("======================================================\n");

            string gamePath = FindBenchmarkPath();
            if (string.IsNullOrEmpty(gamePath))
            {
                Console.WriteLine("Benchmark not found. Make sure it is installed via Steam.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Using benchmark: {gamePath}\n");

            PrintSystemSpecs();

            Console.WriteLine("\n--- CPU Test ---");
            string cpuSettings = GetCpuSettings();
            Console.WriteLine("Applied CPU settings:");
            Console.WriteLine(cpuSettings);
            RunBenchmark("CPU", cpuSettings, gamePath);
            string cpuResult = ParseLogs();
            Console.WriteLine($"CPU Test Result: {cpuResult}");

            Console.WriteLine("\n--- GPU Test ---");
            string gpuSettings = GetGpuSettings();
            Console.WriteLine("Applied GPU settings:");
            Console.WriteLine(gpuSettings);
            RunBenchmark("GPU", gpuSettings, gamePath);
            string gpuResult = ParseLogs();
            Console.WriteLine($"GPU Test Result: {gpuResult}");

            Console.WriteLine("\n======================================================");
            Console.WriteLine(" FINAL REPORT");
            Console.WriteLine("======================================================");
            Console.WriteLine($"CPU Test: {cpuResult}");
            Console.WriteLine($"GPU Test: {gpuResult}");
            Console.WriteLine("======================================================");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static string FindBenchmarkPath()
        {
            const string relativePath = @"steamapps\common\BlackMythWukongBenchmark\b1\Binaries\Win64\b1-Win64-Shipping.exe";

            string steamPath = GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
                return null;

            var libraries = GetSteamLibraries(steamPath);

            foreach (var lib in libraries)
            {
                string candidate = Path.Combine(lib, relativePath);
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        static string GetSteamInstallPath()
        {
            string[] keys =
            {
                @"SOFTWARE\WOW6432Node\Valve\Steam",
                @"SOFTWARE\Valve\Steam"
            };

            foreach (var keyPath in keys)
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                    var path = key?.GetValue("InstallPath") as string;
                    if (!string.IsNullOrEmpty(path))
                        return path.Replace('/', '\\');
                }
                catch { }
            }

            try
            {
                using var userKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var path = userKey?.GetValue("SteamPath") as string;
                if (!string.IsNullOrEmpty(path))
                    return path.Replace('/', '\\');
            }
            catch { }

            return null;
        }

        static List<string> GetSteamLibraries(string steamPath)
        {
            var libraries = new List<string> { steamPath };

            string vdfPath = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(vdfPath))
                return libraries;

            try
            {
                foreach (var line in File.ReadAllLines(vdfPath))
                {
                    var match = Regex.Match(line, @"""path""\s+""([^""]+)""");
                    if (match.Success)
                    {
                        string lib = match.Groups[1].Value.Replace(@"\\", @"\");
                        if (!libraries.Contains(lib) && Directory.Exists(lib))
                            libraries.Add(lib);
                    }
                }
            }
            catch { }

            return libraries;
        }

        static void PrintSystemSpecs()
        {
            Console.WriteLine("--- System Specifications ---");
            Console.WriteLine($"CPU: {GetWmiValue("Win32_Processor", "Name")}");
            Console.WriteLine($"GPU: {GetWmiValue("Win32_VideoController", "Name")}");
            Console.WriteLine($"RAM: {GetRamGb()} GB");
            Console.WriteLine($"OS:  {GetWmiValue("Win32_OperatingSystem", "Caption")}");
        }

        static string GetWmiValue(string className, string property)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {className}");
                foreach (var obj in searcher.Get())
                {
                    var value = obj[property]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value.Trim();
                }
            }
            catch { }
            return "Unknown";
        }

        static string GetRamGb()
        {
            try
            {
                var raw = GetWmiValue("Win32_ComputerSystem", "TotalPhysicalMemory");
                if (double.TryParse(raw, out double bytes))
                    return Math.Round(bytes / 1024 / 1024 / 1024, 2).ToString();
            }
            catch { }
            return "0";
        }

        static string GetCpuSettings()
        {
            return @"[/Script/Engine.GameUserSettings]
ResolutionSizeX=1280
ResolutionSizeY=720
bUseVSync=False
bUseDynamicResolution=False
FullscreenMode=2
sg.ResolutionQuality=50.000000
sg.ViewDistanceQuality=0
sg.AntiAliasingQuality=0
sg.ShadowQuality=0
sg.GlobalIlluminationQuality=0
sg.ReflectionQuality=0
sg.PostProcessQuality=0
sg.TextureQuality=0
sg.EffectsQuality=0
sg.FoliageQuality=0
sg.ShadingQuality=0
r.DLSS.Enable=0
r.FSR.Enable=0
r.XeSS.Enable=0
r.RayTracing=0
";
        }

        static string GetGpuSettings()
        {
            return @"[/Script/Engine.GameUserSettings]
ResolutionSizeX=3840
ResolutionSizeY=2160
bUseVSync=False
bUseDynamicResolution=False
FullscreenMode=1
sg.ResolutionQuality=100.000000
sg.ViewDistanceQuality=3
sg.AntiAliasingQuality=3
sg.ShadowQuality=3
sg.GlobalIlluminationQuality=3
sg.ReflectionQuality=3
sg.PostProcessQuality=3
sg.TextureQuality=3
sg.EffectsQuality=3
sg.FoliageQuality=3
sg.ShadingQuality=3
r.DLSS.Enable=0
r.FSR.Enable=0
r.XeSS.Enable=0
r.RayTracing=1
";
        }

        static void RunBenchmark(string testType, string settingsContent, string gamePath)
        {
            try
            {
                var configDir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                File.WriteAllText(ConfigPath, settingsContent);
                Console.WriteLine($"[{testType}] Settings applied to: {ConfigPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{testType}] Failed to write settings: {ex.Message}");
                return;
            }

            if (Directory.Exists(LogDir))
            {
                foreach (var file in Directory.GetFiles(LogDir, "*.log"))
                {
                    try { File.Delete(file); } catch { }
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = gamePath,
                Arguments = "-benchmark -unattended -nosplash -log",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(gamePath) ?? string.Empty,
                CreateNoWindow = false
            };

            Console.WriteLine($"[{testType}] Launching benchmark...");

            try
            {
                using var process = Process.Start(psi);
                if (process == null)
                {
                    Console.WriteLine($"[{testType}] Failed to start process.");
                    return;
                }

                Console.WriteLine($"[{testType}] Process started (PID: {process.Id}). Waiting...");

                bool exited = process.WaitForExit(15 * 60 * 1000);
                if (!exited)
                {
                    Console.WriteLine($"[{testType}] Timeout exceeded. Killing process.");
                    try { process.Kill(entireProcessTree: true); } catch { }
                }
                else
                {
                    Console.WriteLine($"[{testType}] Process finished with exit code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{testType}] Exception while running benchmark: {ex.Message}");
                return;
            }

            Thread.Sleep(5000);
        }

        static string ParseLogs()
        {
            if (!Directory.Exists(LogDir))
                return $"Log directory not found: {LogDir}";

            var latestLog = Directory.GetFiles(LogDir, "*.log")
                .OrderByDescending(File.GetLastWriteTime)
                .FirstOrDefault();

            if (latestLog == null)
                return "No log files found.";

            Console.WriteLine($"Parsing log: {Path.GetFileName(latestLog)}");

            string content;
            try
            {
                content = File.ReadAllText(latestLog);
            }
            catch (Exception ex)
            {
                return $"Failed to read log: {ex.Message}";
            }

            var avgMatch = Regex.Match(content, @"[Aa]verage\s*FPS[:\s]+([\d\.]+)");
            var minMatch = Regex.Match(content, @"[Mm]in\s*FPS[:\s]+([\d\.]+)");
            var maxMatch = Regex.Match(content, @"[Mm]ax\s*FPS[:\s]+([\d\.]+)");
            var scoreMatch = Regex.Match(content, @"[Ss]core[:\s]+([\d\.]+)");

            if (!avgMatch.Success && !scoreMatch.Success)
                return "Could not parse FPS/Score from log. Check regex patterns.";

            string result = "";
            if (avgMatch.Success) result += $"Avg FPS: {avgMatch.Groups[1].Value}";
            if (minMatch.Success) result += (result.Length > 0 ? " | " : "") + $"Min: {minMatch.Groups[1].Value}";
            if (maxMatch.Success) result += (result.Length > 0 ? " | " : "") + $"Max: {maxMatch.Groups[1].Value}";
            if (scoreMatch.Success) result += (result.Length > 0 ? " | " : "") + $"Score: {scoreMatch.Groups[1].Value}";

            return result.Length > 0 ? result : "No metrics found.";
        }
    }
}