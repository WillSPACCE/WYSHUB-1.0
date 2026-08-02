using System;
using System.IO;
using System.Text.Json;
using SystemWM.Services;

namespace SystemWM.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void AppSettings_UsaEmailPadraoDeTesteDoUsuario()
    {
        var settings = new AppSettings();

        Assert.Equal("martins.willyan20@gmail.com", settings.EmailDestinoPadrao);
    }

    [Fact]
    public void AppSettings_ArmazenaConfiguracaoDeLimpezaAutomaticaDaPastaBackup()
    {
        var settings = new AppSettings();

        Assert.False(settings.LimparAutomaticamentePastaBackup);
        Assert.Equal(30, settings.DiasRetencaoPastaBackup);
    }

    [Fact]
    public void CleanupService_RestaurarBackup_RestauraArquivosParaOrigem()
    {
        var root = Path.Combine(Path.GetTempPath(), "SystemWMTests", Guid.NewGuid().ToString("N"));
        var backupDir = Path.Combine(root, "backup");
        var origemDir = Path.Combine(root, "origem");
        Directory.CreateDirectory(backupDir);
        Directory.CreateDirectory(origemDir);

        var arquivoOrigem = Path.Combine(origemDir, "arquivo.txt");
        var arquivoBackup = Path.Combine(backupDir, "arquivo.txt");
        File.WriteAllText(arquivoOrigem, "conteudo");
        File.WriteAllText(arquivoBackup, "conteudo");

        try
        {
            var service = new CleanupService();
            var restaurado = service.RestaurarBackup(backupDir, origemDir);

            Assert.True(restaurado);
            Assert.True(File.Exists(arquivoOrigem));
            Assert.False(File.Exists(arquivoBackup));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ObterApiKeyVigente_IgnoraChaveAntigaPlaceholder()
    {
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SystemWM");
        var settingsPath = Path.Combine(appDataDir, "settings.json");
        var apiKeyPath = Path.Combine(AppContext.BaseDirectory, "config", "resend_api.key");

        Directory.CreateDirectory(appDataDir);
        Directory.CreateDirectory(Path.GetDirectoryName(apiKeyPath)!);

        var originalSettings = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : null;
        var originalApiKey = File.Exists(apiKeyPath) ? File.ReadAllText(apiKeyPath) : null;

        try
        {
            var settings = new AppSettings { ResendApiKey = "Adrian Gurvitz - Classic " };
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings));
            File.WriteAllText(apiKeyPath, "re_valid_key_1234567890");

            var service = new SettingsService();
            var apiKey = service.ObterApiKeyVigente();

            Assert.Equal("re_valid_key_1234567890", apiKey);
        }
        finally
        {
            if (originalSettings is null)
                File.Delete(settingsPath);
            else
                File.WriteAllText(settingsPath, originalSettings);

            if (originalApiKey is null)
                File.Delete(apiKeyPath);
            else
                File.WriteAllText(apiKeyPath, originalApiKey);
        }
    }

    [Fact]
    public void ObterApiKeyVigente_PrefereApiKeyArquivoQuandoConfigTemValorAntigo()
    {
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SystemWM");
        var settingsPath = Path.Combine(appDataDir, "settings.json");
        var apiKeyPath = Path.Combine(AppContext.BaseDirectory, "config", "resend_api.key");

        Directory.CreateDirectory(appDataDir);
        Directory.CreateDirectory(Path.GetDirectoryName(apiKeyPath)!);

        var originalSettings = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : null;
        var originalApiKey = File.Exists(apiKeyPath) ? File.ReadAllText(apiKeyPath) : null;

        try
        {
            var settings = new AppSettings { ResendApiKey = "chave-antiga" };
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings));
            File.WriteAllText(apiKeyPath, "chave-nova");

            var service = new SettingsService();
            var apiKey = service.ObterApiKeyVigente();

            Assert.Equal("chave-nova", apiKey);
        }
        finally
        {
            if (originalSettings is null)
                File.Delete(settingsPath);
            else
                File.WriteAllText(settingsPath, originalSettings);

            if (originalApiKey is null)
                File.Delete(apiKeyPath);
            else
                File.WriteAllText(apiKeyPath, originalApiKey);
        }
    }
}
