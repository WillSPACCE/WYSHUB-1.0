using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using SystemWM.Services;

namespace SystemWM.Views
{
    public partial class SetupWizardWindow : Window
    {
        private readonly string _scriptPath = SetupWizardPathResolver.ResolveScriptPath();

        public SetupWizardWindow()
        {
            InitializeComponent();
            TxtStatus.Text = "Pronto para preparar o ambiente.";
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
            BtnInstalar.IsEnabled = false;
            BtnPular.IsEnabled = false;
            ProgressBarInstall.Visibility = Visibility.Visible;
            TxtStatus.Text = "Instalando componentes e preparando o ambiente...";
            TxtDetalhes.Text = "Aguarde. O assistente pode solicitar permissão de administrador.";

            try
            {
                if (!File.Exists(_scriptPath))
                {
                    throw new FileNotFoundException("Script de instalação não encontrado", _scriptPath);
                }

                if (!IsAdministrator())
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
                    return;
                }

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

                using var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Não foi possível iniciar o PowerShell.");

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                TxtLog.Text = output + Environment.NewLine + error;

                if (process.ExitCode == 0)
                {
                    TxtStatus.Text = "Ambiente preparado com sucesso.";
                    TxtDetalhes.Text = "O programa pode ser aberto agora.";
                    MessageBox.Show("Assistente concluído com sucesso.", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    TxtStatus.Text = "A instalação foi concluída com avisos ou falhas.";
                    TxtDetalhes.Text = "Verifique o log acima e, se necessário, execute o assistente novamente.";
                    MessageBox.Show("A instalação retornou um erro. Verifique o log do assistente.", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Falha no assistente.";
                TxtDetalhes.Text = ex.Message;
                MessageBox.Show($"Erro ao executar o assistente:\n{ex.Message}", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Error);
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
