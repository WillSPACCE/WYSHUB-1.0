using System;
using System.IO;
using System.Reflection;

namespace SystemWM.Services
{
    public static class SetupWizardPathResolver
    {
        public static string ResolveScriptPath(string? baseDirectory = null)
        {
            var searchRoot = string.IsNullOrWhiteSpace(baseDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : baseDirectory;

            var candidates = new[]
            {
                Path.Combine(searchRoot, "scripts", "setup-wizard.ps1"),
                Path.Combine(searchRoot, "setup-wizard.ps1"),
                Path.Combine(searchRoot, "..", "..", "..", "scripts", "setup-wizard.ps1"),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "setup-wizard.ps1"),
                Path.Combine(Directory.GetCurrentDirectory(), "setup-wizard.ps1")
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return ExtractScriptFromResource();
        }

        private static string ExtractScriptFromResource()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "SystemWM", "scripts");
            Directory.CreateDirectory(tempFolder);
            var tempFile = Path.Combine(tempFolder, "setup-wizard.ps1");

            if (File.Exists(tempFile))
            {
                return tempFile;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = Array.Find(assembly.GetManifestResourceNames(), name => name.EndsWith("setup-wizard.ps1", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                throw new FileNotFoundException("O script incorporado não foi encontrado no assembly.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException("O script incorporado não foi encontrado no assembly.", resourceName);
            }

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            File.WriteAllText(tempFile, content);
            return tempFile;
        }
    }
}
