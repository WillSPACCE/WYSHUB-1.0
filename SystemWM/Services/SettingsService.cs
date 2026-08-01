using System;
using System.IO;
using System.Text.Json;

namespace SystemWM.Services
{
    public class AppSettings
    {
        public string ResendApiKey { get; set; } = "";
        public string EmailRemetente { get; set; } = "SystemWM <onboarding@resend.dev>";
        public string EmailDestinoPadrao { get; set; } = "";
        public string NomeTecnico { get; set; } = "";
    }

    /// <summary>
    /// Salva/lê as configurações do app em um json local, na pasta AppData do usuário.
    /// Assim a API Key do Resend não fica exposta no código-fonte nem versionada.
    /// </summary>
    public class SettingsService
    {
        private readonly string _caminhoArquivo;

        public SettingsService()
        {
            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SystemWM");
            Directory.CreateDirectory(pasta);
            _caminhoArquivo = Path.Combine(pasta, "settings.json");
        }

        public AppSettings Carregar()
        {
            if (!File.Exists(_caminhoArquivo)) return new AppSettings();
            try
            {
                var json = File.ReadAllText(_caminhoArquivo);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }

        public void Salvar(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_caminhoArquivo, json);
        }
    }
}
