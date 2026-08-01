using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SystemWM.Models;

namespace SystemWM.Views
{
    public partial class ReportsView : UserControl
    {
        private string? _ultimoHtmlGerado;

        public ReportsView()
        {
            InitializeComponent();
            var settings = AppState.Settings.Carregar();
            TxtEmailDestino.Text = settings.EmailDestinoPadrao;
        }

        private ClienteVisita ColetarDadosCliente() => new()
        {
            NomeCliente = string.IsNullOrWhiteSpace(TxtNomeCliente.Text) ? "Cliente" : TxtNomeCliente.Text,
            EmpresaCliente = TxtEmpresa.Text,
            EmailDestino = TxtEmailDestino.Text,
            Observacoes = TxtObservacoes.Text,
            DataVisita = DateTime.Now
        };

        private async Task<string?> GarantirDiagnosticoAsync()
        {
            if (AppState.UltimoDiagnostico == null)
            {
                TxtStatus.Text = "Coletando dados do hardware antes de gerar o relatório...";
                AppState.UltimoDiagnostico = await Task.Run(() => AppState.Hardware.ColetarTudo());
            }
            AppState.UltimosProgramas ??= await Task.Run(() => AppState.Programs.Listar());
            return null;
        }

        private async void BtnGerar_Click(object sender, RoutedEventArgs e)
        {
            await GarantirDiagnosticoAsync();
            AppState.ClienteAtual = ColetarDadosCliente();

            _ultimoHtmlGerado = AppState.Reports.GerarHtml(
                AppState.ClienteAtual,
                AppState.UltimoDiagnostico!,
                AppState.UltimosProgramas!,
                AppState.UltimaLimpeza,
                AppState.UltimasRegrasFirewall,
                AppState.FirewallAtivoCache);

            var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SystemWM", "Relatorios");
            Directory.CreateDirectory(pasta);
            var caminho = Path.Combine(pasta, $"Relatorio_{AppState.ClienteAtual.NomeCliente}_{DateTime.Now:yyyyMMdd_HHmm}.html");
            File.WriteAllText(caminho, _ultimoHtmlGerado);

            TxtStatus.Text = $"Relatório gerado e salvo em: {caminho}";
        }

        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmailDestino.Text))
            {
                MessageBox.Show("Informe o e-mail de destino.", "SystemWM");
                return;
            }

            if (_ultimoHtmlGerado == null)
            {
                await GarantirDiagnosticoAsync();
                AppState.ClienteAtual = ColetarDadosCliente();
                _ultimoHtmlGerado = AppState.Reports.GerarHtml(
                    AppState.ClienteAtual,
                    AppState.UltimoDiagnostico!,
                    AppState.UltimosProgramas!,
                    AppState.UltimaLimpeza,
                    AppState.UltimasRegrasFirewall,
                    AppState.FirewallAtivoCache);
            }

            var settings = AppState.Settings.Carregar();

            BtnEnviar.IsEnabled = false;
            TxtStatus.Text = "Enviando e-mail...";

            var (sucesso, mensagem) = await AppState.Email.EnviarRelatorioAsync(
                settings.ResendApiKey,
                settings.EmailRemetente,
                TxtEmailDestino.Text,
                $"Relatório de Visita Técnica - {ColetarDadosCliente().NomeCliente}",
                _ultimoHtmlGerado);

            BtnEnviar.IsEnabled = true;
            TxtStatus.Text = mensagem;

            if (sucesso)
                MessageBox.Show("Relatório enviado com sucesso!", "SystemWM");
        }
    }
}
