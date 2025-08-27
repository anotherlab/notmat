using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;
using System.Management;
using System.Globalization;
using System.Runtime.Versioning;

namespace SharpFetch;

class Program
{
    static void Main(string[] args)
    {
        // Parse command line arguments
        var options = ParseArguments(args);
        
        // Set the culture if specified (do this early so error messages can be localized)
        if (!string.IsNullOrEmpty(options.Locale))
        {
            try
            {
                var culture = new CultureInfo(options.Locale);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine(string.Format(Resources.ErrorInvalidLocale, options.Locale));
            }
        }

        if (options.ShowHelp)
        {
            ShowHelp();
            return;
        }

        var fetcher = new SystemInfoFetcher();
        fetcher.DisplaySystemInfo();
    }

    private static CommandLineOptions ParseArguments(string[] args)
    {
        var options = new CommandLineOptions();

        // First pass: look for locale to set it early
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i].ToLowerInvariant() == "-l" || args[i].ToLowerInvariant() == "--locale") && i + 1 < args.Length)
            {
                options.Locale = args[i + 1];
                try
                {
                    var culture = new CultureInfo(options.Locale);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
                catch (CultureNotFoundException)
                {
                    // Will be handled in Main
                }
                break;
            }
        }

        // Second pass: parse all arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-l":
                case "--locale":
                    if (i + 1 < args.Length)
                    {
                        options.Locale = args[++i]; // Already handled in first pass
                    }
                    else
                    {
                        Console.WriteLine(Resources.ErrorMissingLocaleValue);
                        options.ShowHelp = true;
                    }
                    break;
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;
                default:
                    Console.WriteLine(string.Format(Resources.ErrorUnknownOption, args[i]));
                    options.ShowHelp = true;
                    break;
            }
        }

        return options;
    }

    private static void ShowHelp()
    {
        Console.WriteLine(Resources.HelpDescription);
        Console.WriteLine();
        Console.WriteLine(Resources.HelpUsage);
        Console.WriteLine();
        Console.WriteLine(Resources.HelpOptions);
        Console.WriteLine(Resources.HelpLocaleOption);
        Console.WriteLine(Resources.HelpHelpOption);
        Console.WriteLine();
        Console.WriteLine(Resources.HelpExamples);
        Console.WriteLine(Resources.HelpExampleDefault);
        Console.WriteLine(Resources.HelpExampleSpanish);
    }
}

public class CommandLineOptions
{
    public string? Locale { get; set; }
    public bool ShowHelp { get; set; }
}

public class SystemInfoFetcher
{
    private const string Reset = "\u001b[0m";
    private const string Bold = "\u001b[1m";
    private const string Red = "\u001b[31m";
    private const string Green = "\u001b[32m";
    private const string Yellow = "\u001b[33m";
    private const string Blue = "\u001b[34m";
    private const string Magenta = "\u001b[35m";
    private const string Cyan = "\u001b[36m";
    private const string White = "\u001b[37m";

    public void DisplaySystemInfo()
    {
        var ascii = GetOSLogo();
        var info = GetSystemInfo();
        
        DisplayWithAscii(ascii, info);
    }

    private string[] GetOSLogo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[]
            {
                $"{Blue}                                ..,",
                $"{Blue}                    ....,,:;+ccllll",
                $"{Blue}      ...,,+:;  cllllllllllllllllll",
                $"{Blue},cclllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}                                   ",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}llllllllllllll  lllllllllllllllllll",
                $"{Blue}`'ccllllllllll  lllllllllllllllllll",
                $"{Blue}       `' \\*::  :ccllllllllllllllll",
                $"{Blue}                       ````''*::cll",
                $"{Blue}                                 ``{Reset}"
            };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new[]
            {
                $"{Green}                    'c.",
                $"{Green}                 ,xNMM.",
                $"{Green}               .OMMMMo",
                $"{Green}               OMMM0,",
                $"{Green}     .;loddo:' loolloddol;.",
                $"{Green}   cKMMMMMMMMMMNWMMMMMMMMMM0:",
                $"{Yellow} .KMMMMMMMMMMMMMMMMMMMMMMMWd.",
                $"{Yellow} XMMMMMMMMMMMMMMMMMMMMMMMX.",
                $"{Red};MMMMMMMMMMMMMMMMMMMMMMMM:",
                $"{Red}:MMMMMMMMMMMMMMMMMMMMMMMM:",
                $"{Red}.MMMMMMMMMMMMMMMMMMMMMMMMX.",
                $"{Red} kMMMMMMMMMMMMMMMMMMMMMMMMWd.",
                $"{Magenta} .XMMMMMMMMMMMMMMMMMMMMMMMMMMk",
                $"{Magenta}  .XMMMMMMMMMMMMMMMMMMMMMMMMK.",
                $"{Blue}    kMMMMMMMMMMMMMMMMMMMMMMd",
                $"{Blue}     ;KMMMMMMMWXXWMMMMMMMk.",
                $"{Blue}       .cooc,.    .,coo:.{Reset}"
            };
        }
        else // Linux and other Unix-like systems
        {
            return new[]
            {
                $"{Yellow}        #####",
                $"{Yellow}       #######",
                $"{Yellow}       ##O#O##",
                $"{Yellow}       #VVVVV#",
                $"{Yellow}     ##  VVV  ##",
                $"{Yellow}    #          ##",
                $"{Yellow}   #            ##",
                $"{Yellow}  #            ###",
                $"{Yellow} #              ##",
                $"{Yellow}#              ##",
                $"{Yellow}#             ###",
                $"{Yellow}#             ####",
                $"{Yellow}#              ###",
                $"{Yellow}#               ##",
                $"{Yellow}#                #",
                $"{Yellow}#                #{Reset}"
            };
        }
    }

    private string[] GetSystemInfo()
    {
        var info = new List<string>();
        
        // Header
        var username = Environment.UserName;
        var hostname = Environment.MachineName;
        info.Add($"{Bold}{Green}{username}@{hostname}{Reset}");
        info.Add($"{new string('-', $"{username}@{hostname}".Length)}");
        info.Add("");

        // OS
        var osInfo = GetOSInfo();
        info.Add($"{Bold}{Yellow}{Resources.LabelOS}{Reset} {osInfo}");

        // Kernel
        var kernelInfo = GetKernelInfo();
        if (!string.IsNullOrEmpty(kernelInfo))
            info.Add($"{Bold}{Yellow}{Resources.LabelKernel}{Reset} {kernelInfo}");

        // Uptime
        var uptime = GetUptime();
        if (!string.IsNullOrEmpty(uptime))
            info.Add($"{Bold}{Yellow}{Resources.LabelUptime}{Reset} {uptime}");

        // CPU
        var cpu = GetCPUInfo();
        if (!string.IsNullOrEmpty(cpu))
            info.Add($"{Bold}{Yellow}{Resources.LabelCPU}{Reset} {cpu}");

        // Memory
        // var memory = GetMemoryInfo();
        // if (!string.IsNullOrEmpty(memory))
        //     info.Add($"{Bold}{Yellow}{Resources.LabelMemory}{Reset} {memory}");

        // Shell
        var shell = GetShellInfo();
        if (!string.IsNullOrEmpty(shell))
            info.Add($"{Bold}{Yellow}{Resources.LabelShell}{Reset} {shell}");

        // Terminal
        var terminal = GetTerminalInfo();
        if (!string.IsNullOrEmpty(terminal))
            info.Add($"{Bold}{Yellow}{Resources.LabelTerminal}{Reset} {terminal}");

        // .NET Runtime
        var dotnetVersion = GetDotNetVersion();
        if (!string.IsNullOrEmpty(dotnetVersion))
            info.Add($"{Bold}{Yellow}{Resources.LabelDotNet}{Reset} {dotnetVersion}");

        // Memory
        var TotalRam = GetTotalRam();
        var UsedRam = GetUsedRam();

        info.Add($"{Bold}{Yellow}{Resources.LabelMemory}{Reset} {TotalRam} / {UsedRam}");

        // Network
        var NetworkInterfaces = GetNetworkInfo();

        if (NetworkInterfaces.Any())
        {
            info.Add($"{Bold}{Yellow}{Resources.LabelNetwork}{Reset}");
            foreach (var network in NetworkInterfaces.Take(2)) // Limit to first 2 interfaces for space
            {
                var interfaceType = GetNetworkTypeShort(network.Type);
                info.Add($"{Reset}  {network.Name} ({interfaceType})");

                if (network.IPv4Addresses.Any())
                {
                    info.Add($"{Reset}    IPv4: {network.IPv4Addresses.First()}");
                }
                
                if (network.IPv6Addresses.Any())
                {
                    var ipv6 = network.IPv6Addresses.First();
                    if (ipv6.Length > 25) ipv6 = ipv6.Substring(0, 22) + "...";
                    info.Add($"{Reset}    IPv6: {ipv6}");
                }
            }
            info.Add("");
        }



        info.Add("");
        
        // Color palette
        info.Add($"{Bold}{Resources.LabelColors}{Reset}");
        info.Add($"{Red}███{Green}███{Yellow}███{Blue}███{Magenta}███{Cyan}███{White}███{Reset}");

        return info.ToArray();
    }

    private string GetOSInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var version = Environment.OSVersion;
                var build = GetWindowsBuild();
                var WinNum = version.Version.Major;

                // if ((WinNum < 11) && (Convert.ToInt32(build) >= 22000))
                // {
                //     WinNum = 11;
                // }

                return string.Format(Resources.WindowsOSFormat, WinNum, version.Version.Minor, build);
            }
            catch
            {
                return Resources.WindowsDefault;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var version = GetMacOSVersion();
                return string.Format(Resources.MacOSFormat, version);
            }
            catch
            {
                return Resources.MacOSDefault;
            }
        }
        else
        {
            try
            {
                var osRelease = File.ReadAllText("/etc/os-release");
                var prettyName = osRelease.Split('\n')
                    .FirstOrDefault(line => line.StartsWith("PRETTY_NAME="))?
                    .Split('=')[1].Trim('"');
                return prettyName ?? RuntimeInformation.OSDescription;
            }
            catch
            {
                return RuntimeInformation.OSDescription;
            }
        }
    }

    private string GetKernelInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.OSVersion.VersionString;
            }
            else
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "uname",
                        Arguments = "-r",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return result;
            }
        }
        catch
        {
            return "";
        }
    }

    private string GetUptime()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            var days = uptime.Days;
            var hours = uptime.Hours;
            var minutes = uptime.Minutes;

            if (days > 0)
                return string.Format(Resources.UptimeDaysHoursMinutes, days, hours, minutes);
            else if (hours > 0)
                return string.Format(Resources.UptimeHoursMinutes, hours, minutes);
            else
                return string.Format(Resources.UptimeMinutes, minutes);
        }
        catch
        {
            return "";
        }
    }

    private string GetCPUInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
            }
            else
            {
                var cpuInfo = File.ReadAllText("/proc/cpuinfo");
                var modelName = cpuInfo.Split('\n')
                    .FirstOrDefault(line => line.StartsWith("model name"))?
                    .Split(':')[1].Trim();
                if (!string.IsNullOrEmpty(modelName))
                    return modelName;
            }
        }
        catch { }

        return string.Format(Resources.CPUCoresFormat, Environment.ProcessorCount);
    }

    // private string GetMemoryInfo()
    // {
    //     try
    //     {
    //         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //         {
    //             using var searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
    //             foreach (ManagementObject obj in searcher.Get())
    //             {
    //                 var totalMem = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
    //                 var availMem = Convert.ToUInt64(obj["AvailablePhysicalMemory"]) * 1024;
    //                 var usedMem = totalMem - availMem;
                    
    //                 return string.Format(Resources.MemoryUsageFormat, FormatBytes(usedMem), FormatBytes(totalMem));
    //             }
    //         }
    //         else
    //         {
    //             var memInfo = File.ReadAllText("/proc/meminfo");
    //             var memTotal = GetMemInfoValue(memInfo, "MemTotal") * 1024;
    //             var memAvailable = GetMemInfoValue(memInfo, "MemAvailable") * 1024;
    //             var memUsed = memTotal - memAvailable;
                
    //             return string.Format(Resources.MemoryUsageFormat, FormatBytes(memUsed), FormatBytes(memTotal));
    //         }
    //     }
    //     catch { }

    //     return "";
    // }

    private string GetShellInfo()
    {
        try
        {
            var shell = Environment.GetEnvironmentVariable("SHELL");
            if (!string.IsNullOrEmpty(shell))
            {
                return Path.GetFileName(shell);
            }

            // On Windows, try to detect PowerShell or Command Prompt
            var parent = GetParentProcess();
            if (parent != null)
            {
                var name = parent.ProcessName.ToLowerInvariant();
                if (name.Contains("powershell") || name.Contains("pwsh"))
                    return Resources.PowerShell;
                if (name.Contains("cmd"))
                    return Resources.CommandPrompt;
                if (name.Contains("windowsterminal"))
                    return Resources.WindowsTerminal;
            }
        }
        catch { }

        return "";
    }

    private string GetTerminalInfo()
    {
        try
        {
            var term = Environment.GetEnvironmentVariable("TERM");
            var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
            
            if (!string.IsNullOrEmpty(termProgram))
                return termProgram;
            if (!string.IsNullOrEmpty(term))
                return term;

            // Try to detect Windows Terminal
            var parent = GetParentProcess();
            if (parent?.ProcessName.ToLowerInvariant().Contains("windowsterminal") == true)
                return Resources.WindowsTerminal;
        }
        catch { }

        return "";
    }

    private string GetDotNetVersion()
    {
        return RuntimeInformation.FrameworkDescription;
    }

    [SupportedOSPlatform("windows")]
    private string GetWindowsBuild()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["BuildNumber"]?.ToString() ?? "";
            }
        }
        catch { }
        return "";
    }

    private string GetMacOSVersion()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sw_vers",
                    Arguments = "-productVersion",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }
        catch
        {
            return "";
        }
    }

    private ulong GetMemInfoValue(string memInfo, string key)
    {
        var line = memInfo.Split('\n').FirstOrDefault(l => l.StartsWith(key + ":"));
        if (line != null)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && ulong.TryParse(parts[1], out var value))
                return value;
        }
        return 0;
    }

    private string FormatBytes(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private Process? GetParentProcess()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var parentPid = GetParentProcessId(currentProcess.Id);
            if (parentPid > 0)
                return Process.GetProcessById(parentPid);
        }
        catch { }
        return null;
    }

    private int GetParentProcessId(int processId)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}");
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["ParentProcessId"] != null)
                        return Convert.ToInt32(obj["ParentProcessId"]);
                }
            }
        }
        catch { }
        return 0;
    }

    private string GetTotalRam()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsTotalRam();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxTotalRam();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacTotalRam();
            }
        }
        catch
        {
            // Ignore errors
        }
        return "Unknown";
    }

    [SupportedOSPlatform("windows")]
    private string GetWindowsTotalRam()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory"))
            {
                ulong totalBytes = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    //totalBytes += Convert.ToInt64(obj["Capacity"]);
                    totalBytes += Convert.ToUInt64(obj["Capacity"]);
                }
                return FormatBytes(totalBytes);
            }
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetLinuxTotalRam()
    {
        try
        {
            var memInfo = File.ReadAllText("/proc/meminfo");
            var lines = memInfo.Split('\n');
            var totalLine = lines.FirstOrDefault(l => l.StartsWith("MemTotal:"));
            if (totalLine != null)
            {
                var parts = totalLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && ulong.TryParse(parts[1], out ulong kb))
                {
                    return FormatBytes(kb * 1024);
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        return "Unknown";
    }

    private string GetMacTotalRam()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sysctl",
                    Arguments = "-n hw.memsize",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            if (ulong.TryParse(result, out ulong bytes))
            {
                return FormatBytes(bytes);
            }
        }
        catch
        {
            // Ignore errors
        }
        return "Unknown";
    }

    private string GetUsedRam()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            return FormatBytes((ulong)workingSet);
        }
        catch
        {
            return "Unknown";
        }
    }

        private List<NetworkInfo> GetNetworkInfo()
        {
            var networks = new List<NetworkInfo>();
            
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (var iface in interfaces)
                {
                    try
                    {
                        // Skip loopback and non-operational interfaces for cleaner output
                        if (iface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                            iface.OperationalStatus != OperationalStatus.Up)
                            continue;

                        var networkInfo = new NetworkInfo
                        {
                            Name = iface.Name,
                            Description = iface.Description,
                            Type = iface.NetworkInterfaceType,
                            Status = iface.OperationalStatus,
                            Speed = iface.Speed
                        };

                        // Get MAC address
                        var physicalAddress = iface.GetPhysicalAddress();
                        if (physicalAddress != null && physicalAddress.GetAddressBytes().Length > 0)
                        {
                            networkInfo.MacAddress = string.Join(":", 
                                physicalAddress.GetAddressBytes().Select(b => b.ToString("X2")));
                        }

                        // Get IP addresses
                        var ipProperties = iface.GetIPProperties();
                        foreach (var unicastAddress in ipProperties.UnicastAddresses)
                        {
                            var address = unicastAddress.Address;
                            if (address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                networkInfo.IPv4Addresses.Add(address.ToString());
                            }
                            else if (address.AddressFamily == AddressFamily.InterNetworkV6 &&
                                     !address.IsIPv6LinkLocal && !address.IsIPv6SiteLocal)
                            {
                                networkInfo.IPv6Addresses.Add(address.ToString());
                            }
                        }

                        // Only add interfaces that have IP addresses
                        if (networkInfo.IPv4Addresses.Any() || networkInfo.IPv6Addresses.Any())
                        {
                            networks.Add(networkInfo);
                        }
                    }
                    catch (Exception)
                    {
                        // Skip problematic interfaces
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // If we can't get network info, add a placeholder
                networks.Add(new NetworkInfo
                {
                    Name = "Unknown",
                    Description = "Unable to retrieve network information",
                    Status = OperationalStatus.Unknown
                });
            }
            
            return networks;
        }


    private void DisplayWithAscii(string[] ascii, string[] info)
    {
        var maxLines = Math.Max(ascii.Length, info.Length);
        var asciiWidth = ascii.Max(line => StripAnsiCodes(line).Length);

        for (int i = 0; i < maxLines; i++)
        {
            var asciiLine = i < ascii.Length ? ascii[i] : new string(' ', asciiWidth);
            var infoLine = i < info.Length ? info[i] : "";
            
            // Pad ascii line to consistent width
            var paddedAscii = asciiLine + new string(' ', Math.Max(0, asciiWidth - StripAnsiCodes(asciiLine).Length));
            
            Console.WriteLine($"{paddedAscii}  {infoLine}");
        }
    }

    private string StripAnsiCodes(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, @"\u001b\[[0-9;]*m", "");
    }

    private string GetNetworkTypeShort(NetworkInterfaceType type)
    {
        return type switch
        {
            NetworkInterfaceType.Ethernet => "Ethernet",
            NetworkInterfaceType.Wireless80211 => "WiFi",
            NetworkInterfaceType.Ppp => "PPP",
            NetworkInterfaceType.Loopback => "Loopback",
            NetworkInterfaceType.Tunnel => "Tunnel",
            _ => type.ToString()
        };
    }

}

/// <summary>
/// Contains network interface information
/// </summary>
public class NetworkInfo
{
    public string Name { get; set; } = "Unknown";
    public string Description { get; set; } = "Unknown";
    public NetworkInterfaceType Type { get; set; }
    public OperationalStatus Status { get; set; }
    public List<string> IPv4Addresses { get; set; } = new List<string>();
    public List<string> IPv6Addresses { get; set; } = new List<string>();
    public string MacAddress { get; set; } = "Unknown";
    public long Speed { get; set; }
}
