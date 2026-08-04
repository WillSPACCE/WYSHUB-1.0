using System;
using System.Collections.Generic;
using System.Linq;
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

        public string AplicarTemplateEmail(string template, string cliente, string empresa, string tecnico, string data, string? relatorio = null)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var texto = template
                .Replace("{cliente}", cliente, StringComparison.OrdinalIgnoreCase)
                .Replace("{empresa}", empresa, StringComparison.OrdinalIgnoreCase)
                .Replace("{tecnico}", tecnico, StringComparison.OrdinalIgnoreCase)
                .Replace("{data}", data, StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(relatorio))
                texto = texto.Replace("{relatorio}", relatorio, StringComparison.OrdinalIgnoreCase);

            return texto;
        }

        public async Task<(bool sucesso, string mensagem)> EnviarRelatorioAsync(
            string apiKey, string remetente, string destinatario, string assunto,
            string? corpoHtml = null, string? corpoTexto = null, string? cc = null,
            IEnumerable<(string Nome, byte[] Conteudo)>? anexos = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return (false, "API Key do Resend não configurada. Vá em Configurações.");

            if (string.IsNullOrWhiteSpace(corpoHtml) && string.IsNullOrWhiteSpace(corpoTexto))
                return (false, "Nenhum conteúdo de relatório para enviar.");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new Dictionary<string, object?>
                {
                    ["from"] = remetente,
                    ["to"] = new[] { destinatario },
                    ["subject"] = assunto
                };

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    payload["cc"] = new[] { cc };
                }

                if (!string.IsNullOrWhiteSpace(corpoHtml))
                    payload["html"] = corpoHtml;
                if (!string.IsNullOrWhiteSpace(corpoTexto))
                    payload["text"] = corpoTexto;

                if (anexos != null)
                {
                    payload["attachments"] = anexos.Select(anexo => new Dictionary<string, object?>
                    {
                        ["filename"] = anexo.Nome,
                        ["content"] = Convert.ToBase64String(anexo.Conteudo)
                    }).ToArray();
                }

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
