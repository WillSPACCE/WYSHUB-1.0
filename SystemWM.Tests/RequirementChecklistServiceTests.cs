using System;
using System.IO;
using System.Linq;
using SystemWM.Services;

namespace SystemWM.Tests;

public class RequirementChecklistServiceTests
{
    [Fact]
    public void ParseRequirementsFile_UsesOnlyActionableItems()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SystemWMTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var file = Path.Combine(tempDir, "requirements.txt");
        File.WriteAllText(file, "# Requisitos para rodar o WYSHUB\r\n" +
                                "Windows 10/11 x64\r\n" +
                                "Microsoft .NET 8 Desktop Runtime x64\r\n" +
                                "Microsoft Visual C++ Redistributable x64 (2015-2022)\r\n" +
                                "\r\n" +
                                "Opcional / recomendado:\r\n" +
                                "- pasta do programa em um local estável\r\n");

        try
        {
            var requirements = RequirementChecklistService.ParseRequirementsFile(file);

            Assert.Equal(3, requirements.Count);
            Assert.Contains(requirements, x => x.Name.Contains("Windows 10/11 x64"));
            Assert.Contains(requirements, x => x.Name.Contains("Microsoft .NET 8 Desktop Runtime x64"));
            Assert.Contains(requirements, x => x.Name.Contains("Microsoft Visual C++ Redistributable x64 (2015-2022)"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
