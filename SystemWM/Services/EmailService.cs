using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystemWM.Services
{
    /// <summary>
    /// Envia o relatório por e-mail usando a API do Resend (https://resend.com/docs/api-reference/emails/send-email).
    /// A API Key deve ser configurada em Configurações e fica salva localmente (ver SettingsService).
    /// </summary>
    public class EmailService
    {
        private const string ResendEndpoint = "https://api.resend.com/emails";

        public async Task<(bool sucesso, string mensagem)> EnviarRelatorioAsync(
            string apiKey, string remetente, string destinatario, string assunto, string corpoHtml)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return (false, "API Key do Resend não configurada. Vá em Configurações.");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    from = remetente,        // ex: "SystemWM <relatorios@seudominio.com>"
                    to = new[] { destinatario },
                    subject = assunto,
                    html = corpoHtml
                };

                var json = JsonSerializer.Serialize(payload);
                var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

                var resposta = await client.PostAsync(ResendEndpoint, conteudo);
                var respostaTexto = await resposta.Content.ReadAsStringAsync();

                if (resposta.IsSuccessStatusCode)
                    return (true, "E-mail enviado com sucesso.");

                return (false, $"Falha ao enviar ({(int)resposta.StatusCode}): {respostaTexto}");
            }
            catch (System.Exception ex)
            {
                return (false, $"Erro ao enviar e-mail: {ex.Message}");
            }
        }
    }
}
