using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SystemWM.Models;

namespace SystemWM.Services
{
    /// <summary>
    /// Controla o Firewall do Windows via "netsh advfirewall".
    /// Precisa rodar como Administrador (já garantido pelo app.manifest).
    /// </summary>
    public class FirewallService
    {
        private (string output, string error, int exitCode) ExecutarNetsh(string argumentos)
        {
            var psi = new ProcessStartInfo("netsh", argumentos)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var proc = Process.Start(psi)!;
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            return (output, error, proc.ExitCode);
        }

        /// <summary>Estado atual do firewall (Domínio/Privado/Público).</summary>
        public bool EstaAtivo()
        {
            var (output, _, _) = ExecutarNetsh("advfirewall show currentprofile state");
            return output.Contains("ON", StringComparison.OrdinalIgnoreCase);
        }

        public bool AtivarFirewall()
        {
            var (_, _, code) = ExecutarNetsh("advfirewall set allprofiles state on");
            return code == 0;
        }

        public bool DesativarFirewall()
        {
            var (_, _, code) = ExecutarNetsh("advfirewall set allprofiles state off");
            return code == 0;
        }

        /// <summary>Lista as regras de firewall configuradas (entrada e saída).</summary>
        public List<FirewallRegra> ListarRegras()
        {
            var regras = new List<FirewallRegra>();
            var (output, _, _) = ExecutarNetsh("advfirewall firewall show rule name=all");

            // O netsh retorna blocos de texto separados por "----------------------------------------------------------------------"
            var blocos = output.Split(new[] { "----------------------------------------------------------------------" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var bloco in blocos)
            {
                if (!bloco.Contains("Rule Name:")) continue;

                string Pega(string chave)
                {
                    var linha = bloco.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith(chave));
                    if (linha == null) return "";
                    var idx = linha.IndexOf(':');
                    return idx >= 0 ? linha[(idx + 1)..].Trim() : "";
                }

                var regra = new FirewallRegra
                {
                    Nome = Pega("Rule Name"),
                    Direcao = Pega("Direction"),
                    Habilitada = Pega("Enabled").Equals("Yes", StringComparison.OrdinalIgnoreCase),
                    Perfil = Pega("Profiles"),
                    Protocolo = Pega("Protocol"),
                    Porta = Pega("LocalPort"),
                    Acao = Pega("Action")
                };

                if (!string.IsNullOrWhiteSpace(regra.Nome))
                    regras.Add(regra);
            }

            return regras;
        }

        public bool HabilitarRegra(string nomeRegra, bool habilitar)
        {
            string valor = habilitar ? "yes" : "no";
            var (_, _, code) = ExecutarNetsh($"advfirewall firewall set rule name=\"{nomeRegra}\" new enable={valor}");
            return code == 0;
        }

        /// <summary>Cria uma nova regra de liberação/bloqueio de porta.</summary>
        public bool CriarRegraPorta(string nome, int porta, string protocolo, bool permitir, string direcao = "in")
        {
            string acao = permitir ? "allow" : "block";
            var (_, _, code) = ExecutarNetsh(
                $"advfirewall firewall add rule name=\"{nome}\" dir={direcao} action={acao} protocol={protocolo} localport={porta}");
            return code == 0;
        }

        public bool RemoverRegra(string nome)
        {
            var (_, _, code) = ExecutarNetsh($"advfirewall firewall delete rule name=\"{nome}\"");
            return code == 0;
        }
    }
}
