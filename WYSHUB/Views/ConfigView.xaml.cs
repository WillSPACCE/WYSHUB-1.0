using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using SystemWM.Services;

namespace SystemWM.Views
{
    public partial class ConfigView : UserControl
    {
        private bool _apiValida;

        public ConfigView()
        {
            InitializeComponent();
            var s = AppState.Settings.Carregar();
            TxtApiKey.Password = s.ResendApiKey;
            TxtApiKeyVisivel.Text = s.ResendApiKey;
            AtualizarStatusApi(null, "Sem validação");
        }

        private void AtualizarStatusApi(bool? valido, string mensagem)
        {
            _apiValida = valido == true;

            if (valido == true)
            {
                StatusApiBadge.Background = new SolidColorBrush(Colors.LimeGreen);
                TxtStatusApi.Text = "Válida";
            }
            else if (valido == false)
            {
                StatusApiBadge.Background = new SolidColorBrush(Colors.IndianRed);
                TxtStatusApi.Text = "Inválida";
            }
            else
            {
                StatusApiBadge.Background = new SolidColorBrush(Colors.LightGray);
                TxtStatusApi.Text = "Sem validação";
            }

            TxtStatus.Text = mensagem;
        }

        private static bool ValorEhInvalido(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return true;

            var texto = valor.Trim();
            return texto.Contains("Adrian Gurvitz", StringComparison.OrdinalIgnoreCase)
                || texto.Contains("Classic", StringComparison.OrdinalIgnoreCase)
                || texto.Contains("placeholder", StringComparison.OrdinalIgnoreCase)
                || texto.Contains("example", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> ValidarApiAsync(string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || ValorEhInvalido(apiKey))
                return false;

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

                var payload = new
                {
                    from = "SystemWM <onboarding@resend.dev>",
                    to = new[] { "delivered@resend.dev" },
                    subject = "Teste de API",
                    text = "Teste de API do SystemWM"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.resend.com/emails", content);
                var resposta = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void BtnMostrarApi_Click(object sender, RoutedEventArgs e)
        {
            if (TxtApiKeyVisivel.Visibility == Visibility.Visible)
                return;

            if (TxtApiKey.Password == "")
            {
                TxtApiKey.Password = AppState.Settings.ObterApiKeyVigente();
            }

            SenhaApiContainer.Visibility = Visibility.Visible;
            TxtSenhaApi.Clear();
            TxtSenhaApiErro.Visibility = Visibility.Collapsed;
            TxtSenhaApi.Focus();
        }

        private void BtnConfirmarSenhaApi_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSenhaApi.Password == "2020")
            {
                var apiAtual = AppState.Settings.ObterApiKeyVigente();

                if (string.IsNullOrWhiteSpace(apiAtual))
                {
                    TxtSenhaApiErro.Text = "Nenhuma API salva.";
                    TxtSenhaApiErro.Visibility = Visibility.Visible;
                    return;
                }

                TxtApiKey.Password = "";
                TxtApiKey.Visibility = Visibility.Collapsed;
                TxtApiKeyVisivel.Text = apiAtual;
                TxtApiKeyVisivel.Visibility = Visibility.Visible;
                SenhaApiContainer.Visibility = Visibility.Collapsed;
                return;
            }

            TxtSenhaApiErro.Text = "Senha incorreta.";
            TxtSenhaApiErro.Visibility = Visibility.Visible;
            TxtSenhaApi.Clear();
            TxtSenhaApi.Focus();
        }

        private async void BtnSalvarApiArquivo_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = TxtApiKey.Password?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey) || ValorEhInvalido(apiKey))
            {
                AtualizarStatusApi(false, "Chave inválida ou vazia. Faça o teste antes de salvar.");
                return;
            }

            if (!await ValidarApiAsync(apiKey))
            {
                AtualizarStatusApi(false, "A chave precisa passar no teste antes de ser salva.");
                return;
            }

            AppState.Settings.SalvarApiKey(apiKey);
            TxtApiKey.Password = AppState.Settings.ObterApiKeyVigente();
            AtualizarStatusApi(true, $"API salva no arquivo: {AppState.Settings.ObterCaminhoArquivoApi()}");
        }

        private async void BtnSalvarConfig_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = TxtApiKey.Password?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey) || ValorEhInvalido(apiKey))
            {
                AtualizarStatusApi(false, "Chave inválida ou vazia. Faça o teste antes de salvar.");
                return;
            }

            if (!await ValidarApiAsync(apiKey))
            {
                AtualizarStatusApi(false, "A chave precisa passar no teste antes de ser salva na configuração.");
                return;
            }

            var s = AppState.Settings.Carregar();
            s.ResendApiKey = apiKey;
            AppState.Settings.Salvar(s);
            TxtApiKey.Password = AppState.Settings.ObterApiKeyVigente();
            AtualizarStatusApi(true, "API salva nas configurações e pronta para o envio.");
        }

        private async void BtnTestarApi_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = TxtApiKey.Password?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AtualizarStatusApi(false, "Informe a API key antes de testar.");
                return;
            }

            if (ValorEhInvalido(apiKey))
            {
                AtualizarStatusApi(false, "A chave parece ser inválida ou antiga. Insira uma chave válida do Resend.");
                return;
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    from = "SystemWM <onboarding@resend.dev>",
                    to = new[] { "delivered@resend.dev" },
                    subject = "Teste de API",
                    text = "Teste de API do SystemWM"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.resend.com/emails", content);
                var resposta = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    AtualizarStatusApi(true, "API válida e pronta para envio.");
                    return;
                }

                AtualizarStatusApi(false, $"API inválida: {(int)response.StatusCode} - {resposta}");
            }
            catch (Exception ex)
            {
                AtualizarStatusApi(false, $"Erro ao testar API: {ex.Message}");
            }
        }
    }
}
