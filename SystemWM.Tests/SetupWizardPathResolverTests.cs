using System;
using System.IO;
using SystemWM.Services;

namespace SystemWM.Tests;

public class SetupWizardPathResolverTests
{
    [Fact]
    public void ResolveScriptPath_UsesProvidedBaseDirectory_WhenScriptExists()
    {
        var root = Path.Combine(Path.GetTempPath(), "SystemWMTests", Guid.NewGuid().ToString("N"));
        var scriptsDir = Path.Combine(root, "scripts");
        Directory.CreateDirectory(scriptsDir);

        var expectedScriptPath = Path.Combine(scriptsDir, "setup-wizard.ps1");
        File.WriteAllText(expectedScriptPath, "Write-Host ok");

        try
        {
            var resolved = SetupWizardPathResolver.ResolveScriptPath(root);
            Assert.Equal(Path.GetFullPath(expectedScriptPath), resolved);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
