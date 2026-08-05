using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace SystemWM.Services
{
    public enum RequirementStatus
    {
        Installed,
        Missing,
        Warning,
        Optional
    }

    public sealed class RequirementChecklistItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
        public RequirementStatus Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string InstallHint { get; set; } = string.Empty;
    }

    public static class RequirementChecklistService
    {
        public static List<RequirementChecklistItem> GetChecklist(string? baseDirectory = null)
        {
            var requirementsPath = ResolveRequirementsPath(baseDirectory);
            var items = ParseRequirementsFile(requirementsPath);

            foreach (var item in items)
            {
                item.Status = EvalStatus(item.Name);
                item.Detail = BuildDetail(item.Name, item.Status);
            }

            return items;
        }

        public static string ResolveRequirementsPath(string? baseDirectory = null)
        {
            var searchRoot = string.IsNullOrWhiteSpace(baseDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : baseDirectory;

            var candidates = new[]
            {
                Path.Combine(searchRoot, "requirements.txt"),
                Path.Combine(searchRoot, "..", "..", "..", "requirements.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "requirements.txt"),
                Path.Combine(Path.GetDirectoryName(searchRoot) ?? searchRoot, "requirements.txt")
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            throw new FileNotFoundException("Arquivo requirements.txt não encontrado no diretório do projeto.");
        }

        public static List<RequirementChecklistItem> ParseRequirementsFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo requirements.txt não encontrado.", filePath);

            var items = new List<RequirementChecklistItem>();
            var inOptionalSection = false;
            var lines = File.ReadAllLines(filePath);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("Opcional", StringComparison.OrdinalIgnoreCase))
                {
                    inOptionalSection = true;
                    continue;
                }

                if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
                {
                    var name = line.Substring(2).Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (name.StartsWith("pasta do programa", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("exclusão do Defender", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("assinatura do executável", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    items.Add(new RequirementChecklistItem
                    {
                        Name = name,
                        IsOptional = inOptionalSection,
                        Status = RequirementStatus.Warning,
                        InstallHint = BuildInstallHint(name)
                    });
                    continue;
                }

                if (!line.StartsWith("- ", StringComparison.Ordinal) && !line.StartsWith("* ", StringComparison.Ordinal))
                {
                    if (line.Contains("x64") || line.Contains("runtime") || line.Contains("PowerShell") || line.Contains("WMI") || line.Contains("Visual C++") || line.Contains("firewall"))
                    {
                        items.Add(new RequirementChecklistItem
                        {
                            Name = line,
                            IsOptional = inOptionalSection,
                            Status = RequirementStatus.Warning,
                            InstallHint = BuildInstallHint(line)
                        });
                    }
                }
            }

            return items;
        }

        private static RequirementStatus EvalStatus(string name)
        {
            var normalized = name.ToLowerInvariant();

            if (normalized.Contains("windows 10/11 x64") || normalized.Contains("windows 10") || normalized.Contains("windows 11"))
                return IsWindowsSupported() ? RequirementStatus.Installed : RequirementStatus.Missing;

            if (normalized.Contains(".net 8") || normalized.Contains("runtime .net 8") || normalized.Contains("desktop runtime x64"))
                return HasDotNetRuntime8() ? RequirementStatus.Installed : RequirementStatus.Missing;

            if (normalized.Contains("visual c++ redistributable"))
                return HasVisualCppRedist() ? RequirementStatus.Installed : RequirementStatus.Missing;

            if (normalized.Contains("powershell 5.1") || normalized.Contains("windows powershell"))
                return RequirementStatus.Installed;

            if (normalized.Contains("wmi") || normalized.Contains("windows management instrumentation"))
                return IsWmiEnabled() ? RequirementStatus.Installed : RequirementStatus.Missing;

            if (normalized.Contains("firewall"))
                return IsWindowsFirewallEnabled() ? RequirementStatus.Installed : RequirementStatus.Missing;

            if (normalized.Contains("acesso de administrador") || normalized.Contains("administrador"))
                return IsAdministrator() ? RequirementStatus.Installed : RequirementStatus.Warning;

            if (normalized.Contains("defender"))
                return RequirementStatus.Optional;

            return RequirementStatus.Warning;
        }

        private static string BuildDetail(string name, RequirementStatus status)
        {
            return status switch
            {
                RequirementStatus.Installed => $"OK - {name} já está disponível no sistema.",
                RequirementStatus.Missing => $"Pendente - {name} precisa ser baixado e instalado.",
                RequirementStatus.Warning => $"Verificação manual - {name} requer atenção para confirmar o estado.",
                RequirementStatus.Optional => $"Opcional - {name} pode ser aplicado quando necessário.",
                _ => $"Estado indefinido - {name}"
            };
        }

        private static string BuildInstallHint(string name)
        {
            if (name.Contains(".NET 8", StringComparison.OrdinalIgnoreCase))
                return "Baixar e instalar o runtime .NET 8 Desktop da Microsoft (winget ou instalador oficial).";

            if (name.Contains("Visual C++", StringComparison.OrdinalIgnoreCase))
                return "Baixar e instalar o redistribuível Microsoft Visual C++ 2015-2022 (x64).";

            if (name.Contains("WMI", StringComparison.OrdinalIgnoreCase))
                return "Ativar e iniciar o serviço Winmgmt com permissões de administrador.";

            if (name.Contains("Firewall", StringComparison.OrdinalIgnoreCase))
                return "Habilitar o Firewall do Windows para os perfis necessário.";

            return "Usar o instalador oficial ou o pacote recomendado pela Microsoft.";
        }

        private static bool IsWindowsSupported()
        {
            return Environment.OSVersion.Version.Major >= 10;
        }

        private static bool HasDotNetRuntime8()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process == null)
                    return false;

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return output.Contains("Microsoft.NETCore.App 8", StringComparison.OrdinalIgnoreCase)
                    || error.Contains("Microsoft.NETCore.App 8", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool HasVisualCppRedist()
        {
            var uninstallNames = new[]
            {
                "Microsoft Visual C++ 2015-2022 Redistributable (x64)",
                "Microsoft Visual C++ 2015-2022 Redistributable (x86)",
                "Microsoft Visual C++ 2015-2022 Redistributable"
            };

            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key == null)
                return false;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                var displayName = subKey?.GetValue("DisplayName") as string;
                if (uninstallNames.Any(name => string.Equals(name, displayName, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;
        }

        private static bool IsWmiEnabled()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "query winmgmt",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return false;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase)
                    || output.Contains("START_PENDING", StringComparison.OrdinalIgnoreCase)
                    || output.Contains("AUTO_START", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWindowsFirewallEnabled()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process == null)
                    return false;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Contains("Firewall is ON", StringComparison.OrdinalIgnoreCase)
                    || output.Contains("Firewall is OFF", StringComparison.OrdinalIgnoreCase) == false;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAdministrator()
        {
            return Environment.UserName.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
                || Environment.GetEnvironmentVariable("USERNAME")?.Contains("Administrator", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
