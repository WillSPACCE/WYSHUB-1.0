using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SystemWM.Services;

namespace SystemWM.Views
{
    public partial class SetupWizardWindow : Window
    {
        private readonly string _scriptPath = SetupWizardPathResolver.ResolveScriptPath();
        private readonly List<RequirementChecklistItem> _requisitos = new();
        private readonly StringBuilder _wizardLog = new();
        private string _wizardLogPath = string.Empty;

        public SetupWizardWindow()
        {
            InitializeComponent();
            CarregarChecklist();
            TxtStatus.Text = "Pronto para preparar o ambiente.";
        }

        private void CarregarChecklist()
        {
            try
            {
                _requisitos.Clear();
                var checklist = RequirementChecklistService.GetChecklist(AppDomain.CurrentDomain.BaseDirectory);
                _requisitos.AddRange(checklist);

                LstRequisitos.ItemsSource = checklist
                    .Select(item => $"[{item.Status}] {item.Name}")
                    .ToList();

                TxtDetalhes.Text = "O assistente vai verificar cada item do arquivo de requisitos e instalar somente o que estiver faltando.";
            }
            catch (Exception ex)
            {
                LstRequisitos.ItemsSource = new[] { $"Falha ao ler o arquivo de requisitos: {ex.Message}" };
                TxtDetalhes.Text = ex.Message;
            }
        }

        private void RegistrarLog(string mensagem)
        {
            var linha = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensagem}";
            _wizardLog.AppendLine(linha);
        }

        private void SalvarLogArquivo()
        {
            try
            {
                var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SystemWM", "Logs");
                Directory.CreateDirectory(pasta);
                _wizardLogPath = Path.Combine(pasta, $"SetupWizard_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                File.WriteAllText(_wizardLogPath, _wizardLog.ToString());
            }
            catch
            {
                _wizardLogPath = string.Empty;
            }
        }

        private async void BtnInstalar_Click(object sender, RoutedEventArgs e)
        {
            await ExecutarAssistenteAsync();
        }

        private void BtnPular_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnElevate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "WYSHUB.exe";
                var startInfo = new ProcessStartInfo
                {
                    FileName = currentExe,
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível reabrir como administrador:\n{ex.Message}", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private async Task ExecutarAssistenteAsync()
        {
            _wizardLog.Clear();
            RegistrarLog("Iniciando assistente de instalação do WYSHUB.");
            RegistrarLog($"Script de instalação localizado em: {_scriptPath}");
            RegistrarLog($"Diretório de base da aplicação: {AppDomain.CurrentDomain.BaseDirectory}");

            BtnInstalar.IsEnabled = false;
            BtnPular.IsEnabled = false;
            ProgressBarInstall.Visibility = Visibility.Visible;
            TxtStatus.Text = "Verificando e instalando requisitos...";
            TxtDetalhes.Text = "Aguarde. O assistente vai validar cada item do requirements.txt e instalar apenas o que estiver ausente.";

            try
            {
                if (!File.Exists(_scriptPath))
                {
                    throw new FileNotFoundException("Script de instalação não encontrado", _scriptPath);
                }

                RegistrarLog("Validação do script concluída.");

                if (!IsAdministrator())
                {
                    RegistrarLog("Aplicação sem privilégios de administrador. Reabrindo com UAC.");
                    var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "WYSHUB.exe";
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = currentExe,
                        Verb = "runas",
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                    Environment.Exit(0);
                    return;
                }

                RegistrarLog("Privilégios de administrador confirmados.");

                var appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{_scriptPath}\" -SourcePath \"{appBaseDirectory}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                RegistrarLog($"Executando PowerShell com argumentos: {psi.Arguments}");
                using var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Não foi possível iniciar o PowerShell.");

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                RegistrarLog($"Saída do PowerShell:\n{output}");
                RegistrarLog($"Erro do PowerShell:\n{error}");
                RegistrarLog($"ExitCode retornado: {process.ExitCode}");

                LstRequisitos.ItemsSource = _requisitos
                    .Select(item => $"[{item.Status}] {item.Name}")
                    .ToList();

                if (process.ExitCode == 0)
                {
                    TxtStatus.Text = "Ambiente preparado com sucesso.";
                    TxtDetalhes.Text = "O programa pode ser aberto agora.";
                    RegistrarLog("Assistente concluído com sucesso.");
                    SalvarLogArquivo();
                    MessageBox.Show($"Assistente concluído com sucesso.\nLog do assistente salvo em:\n{_wizardLogPath}", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    TxtStatus.Text = "A instalação foi concluída com avisos ou falhas.";
                    TxtDetalhes.Text = "Verifique o log acima e, se necessário, execute o assistente novamente.";
                    RegistrarLog("Assistente retornou falha ou aviso no processo PowerShell.");
                    SalvarLogArquivo();
                    MessageBox.Show($"A instalação retornou um erro.\nLog do assistente salvo em:\n{_wizardLogPath}", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                RegistrarLog($"Exceção capturada no assistente: {ex}");
                SalvarLogArquivo();
                TxtStatus.Text = "Falha no assistente.";
                TxtDetalhes.Text = ex.Message;
                MessageBox.Show($"Erro ao executar o assistente:\n{ex.Message}\n\nLog detalhado salvo em:\n{_wizardLogPath}", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnInstalar.IsEnabled = true;
                BtnPular.IsEnabled = true;
                ProgressBarInstall.Visibility = Visibility.Collapsed;
            }
        }
    }
}
