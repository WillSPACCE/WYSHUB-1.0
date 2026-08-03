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

            using var proc = Process.Start(psi);
            if (proc == null)
                throw new InvalidOperationException("Falha ao iniciar o comando netsh. Verifique se o utilitário está disponível no sistema.");

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

        public List<PortaAtivaInfo> ListarPortasAtivas()
        {
            var portas = new List<PortaAtivaInfo>();
            var psi = new ProcessStartInfo("netstat", "-ano")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var proc = Process.Start(psi);
            if (proc == null)
                throw new InvalidOperationException("Falha ao iniciar o comando netstat. Verifique se o utilitário está disponível no sistema.");

            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            var linhas = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var linha in linhas)
            {
                var texto = linha.Trim();
                if (texto.StartsWith("Proto", StringComparison.OrdinalIgnoreCase) ||
                    texto.StartsWith("Active Connections", StringComparison.OrdinalIgnoreCase) ||
                    texto.StartsWith("TCP", StringComparison.OrdinalIgnoreCase) ||
                    texto.StartsWith("UDP", StringComparison.OrdinalIgnoreCase) == false && texto.Length == 0)
                {
                    // skip header / empty lines handled below
                }

                if (texto.StartsWith("TCP", StringComparison.OrdinalIgnoreCase) || texto.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
                {
                    var partes = texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length < 4) continue;

                    string protocolo = partes[0];
                    string local = partes[1];
                    string remoto = partes[2];
                    string estado = string.Empty;
                    string pidValue;

                    if (protocolo.Equals("TCP", StringComparison.OrdinalIgnoreCase) && partes.Length >= 5)
                    {
                        estado = partes[3];
                        pidValue = partes[4];
                    }
                    else
                    {
                        pidValue = partes[3];
                    }

                    if (!int.TryParse(pidValue, out var pid))
                        pid = 0;

                    string processo = "-";
                    try
                    {
                        var procInfo = Process.GetProcessById(pid);
                        processo = procInfo.ProcessName;
                    }
                    catch
                    {
                        if (pid > 0)
                            processo = $"PID {pid}";
                    }

                    portas.Add(new PortaAtivaInfo
                    {
                        Protocolo = protocolo,
                        LocalEndpoint = local,
                        RemoteEndpoint = remoto,
                        Estado = string.IsNullOrWhiteSpace(estado) ? "Listening" : estado,
                        Pid = pid,
                        Processo = processo
                    });
                }
            }

            return portas.OrderBy(p => p.Protocolo).ThenBy(p => p.Porta).ToList();
        }
    }
}
