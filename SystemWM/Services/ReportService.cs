using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemWM.Models;

namespace SystemWM.Services
{
    /// <summary>Monta o relatório final (HTML) com todos os dados coletados na visita técnica.</summary>
    public class ReportService
    {
        public string GerarHtml(
            ClienteVisita cliente,
            DiagnosticoCompleto diagnostico,
            List<ProgramaInstalado> programas,
            Dictionary<string, double>? limpezaResultado,
            List<FirewallRegra>? regrasFirewall,
            bool firewallAtivo)
        {
            var sb = new StringBuilder();
            sb.Append($@"
<html>
<head><meta charset='utf-8'>
<style>
  body {{ font-family: Segoe UI, Arial, sans-serif; background:#0b0f1a; color:#e5e9f0; padding:24px; }}
  h1 {{ color:#5b8def; }}
  h2 {{ color:#5b8def; border-bottom:1px solid #2a3350; padding-bottom:6px; margin-top:28px;}}
  table {{ width:100%; border-collapse:collapse; margin-top:8px; }}
  td, th {{ padding:6px 10px; text-align:left; border-bottom:1px solid #232a42; font-size:14px;}}
  th {{ color:#8ea0c9; }}
  .card {{ background:#131a2b; border:1px solid #232a42; border-radius:10px; padding:16px; margin-bottom:16px;}}
  .ok {{ color:#3ddc84; }}
  .warn {{ color:#f5a623; }}
  .bad {{ color:#f25555; }}
  .header {{ display:flex; justify-content:space-between; }}
</style>
</head>
<body>
  <h1>Relatório de Visita Técnica — SystemWM</h1>
  <div class='card'>
    <div class='header'>
      <div>
        <b>Cliente:</b> {cliente.NomeCliente} {(string.IsNullOrEmpty(cliente.EmpresaCliente) ? "" : $"({cliente.EmpresaCliente})")}<br>
        <b>Data da visita:</b> {cliente.DataVisita:dd/MM/yyyy HH:mm}<br>
        <b>Máquina:</b> {diagnostico.Sistema.NomeHost} — Usuário: {diagnostico.Sistema.Usuario}
      </div>
    </div>
    {(string.IsNullOrEmpty(cliente.Observacoes) ? "" : $"<p><b>Observações:</b> {cliente.Observacoes}</p>")}
  </div>

  <h2>Sistema</h2>
  <div class='card'>
    <table>
      <tr><td>Sistema Operacional</td><td>{diagnostico.Sistema.SistemaOperacional} ({diagnostico.Sistema.Arquitetura})</td></tr>
      <tr><td>Placa-mãe</td><td>{diagnostico.Sistema.PlacaMae}</td></tr>
      <tr><td>Chipset</td><td>{diagnostico.Sistema.Chipset}</td></tr>
      <tr><td>BIOS</td><td>{diagnostico.Sistema.BiosVersao}</td></tr>
      <tr><td>Uptime</td><td>{diagnostico.Sistema.Uptime}</td></tr>
      <tr><td>Rede</td><td>{diagnostico.Sistema.RedeStatus} — IP Local: {diagnostico.Sistema.IpLocal} — IP Pública: {diagnostico.Sistema.IpPublica}</td></tr>
      <tr><td>VPN</td><td>{(diagnostico.Sistema.VpnAtiva ? "Ativa" : "Inativa")}</td></tr>
    </table>
  </div>

  <h2>CPU</h2>
  <div class='card'>
    <table>
      <tr><td>Modelo</td><td>{diagnostico.Cpu.Nome}</td></tr>
      <tr><td>Uso atual</td><td>{diagnostico.Cpu.UsoAtualPercent:0}%</td></tr>
      <tr><td>Frequência atual / base</td><td>{diagnostico.Cpu.FrequenciaAtualGHz} GHz / {diagnostico.Cpu.FrequenciaBaseGHz} GHz</td></tr>
      <tr><td>Temperatura</td><td class='{ClasseTemperaturaCpu(diagnostico.Cpu.TemperaturaC)}'>{diagnostico.Cpu.TemperaturaC:0}°C</td></tr>
      <tr><td>TDP</td><td>{diagnostico.Cpu.TdpWatts:0} W</td></tr>
      <tr><td>Voltagem</td><td>{diagnostico.Cpu.VoltagemV} V</td></tr>
      <tr><td>Núcleos / Threads</td><td>{diagnostico.Cpu.NucleosFisicos} / {diagnostico.Cpu.Threads}</td></tr>
      <tr><td>Fan CPU / Sistema</td><td>{diagnostico.Cpu.FanCpuRpm:0} RPM / {diagnostico.Cpu.FanSistemaRpm:0} RPM</td></tr>
    </table>
  </div>

  <h2>Memória RAM</h2>
  <div class='card'>
    <table>
      <tr><td>Total instalada</td><td>{diagnostico.Ram.TotalGB} GB ({diagnostico.Ram.Tipo} @ {diagnostico.Ram.VelocidadeMHz} MHz)</td></tr>
      <tr><td>Em uso</td><td>{diagnostico.Ram.UsoPercent}% ({diagnostico.Ram.UsadaGB} GB)</td></tr>
      <tr><td>Disponível</td><td>{diagnostico.Ram.DisponivelGB} GB</td></tr>
      <tr><td>Slots usados</td><td>{diagnostico.Ram.SlotsUsados} de {diagnostico.Ram.SlotsTotais}</td></tr>
    </table>
  </div>

  <h2>Armazenamento</h2>
  <div class='card'>
    <table>
      <tr><th>Modelo</th><th>Interface</th><th>Capacidade</th><th>Usado</th><th>Livre</th><th>Temp.</th><th>Saúde</th></tr>
      {string.Join("", diagnostico.Discos.Select(d => $@"
      <tr>
        <td>{d.Modelo}</td><td>{d.Interface}</td><td>{d.CapacidadeTotalGB} GB</td>
        <td>{d.EspacoUsadoGB} GB</td><td>{d.EspacoLivreGB} GB</td>
        <td class='{ClasseTemperaturaDisco(d.TemperaturaC)}'>{d.TemperaturaC:0}°C</td><td>{d.SaudePercentTexto}</td>
      </tr>"))}
    </table>
  </div>
");

            if (diagnostico.Gpu != null)
            {
                sb.Append($@"
  <h2>GPU</h2>
  <div class='card'>
    <table>
      <tr><td>Modelo</td><td>{diagnostico.Gpu.Nome}</td></tr>
      <tr><td>Uso</td><td>{diagnostico.Gpu.UsoPercent:0}%</td></tr>
      <tr><td>Temperatura</td><td>{diagnostico.Gpu.TemperaturaC:0}°C</td></tr>
      <tr><td>Memória</td><td>{diagnostico.Gpu.MemoriaUsadaGB} / {diagnostico.Gpu.MemoriaTotalGB} GB</td></tr>
      <tr><td>Consumo</td><td>{diagnostico.Gpu.ConsumoWatts:0} W</td></tr>
      <tr><td>Fan</td><td>{diagnostico.Gpu.FanRpm:0} RPM</td></tr>
    </table>
  </div>");
            }

            sb.Append($@"
  <h2>Firewall</h2>
  <div class='card'>
    <p>Status: <b class='{(firewallAtivo ? "ok" : "bad")}'>{(firewallAtivo ? "Ativo" : "Desativado")}</b></p>
    {(regrasFirewall != null ? $"<p>{regrasFirewall.Count} regras configuradas.</p>" : "")}
  </div>");

            if (limpezaResultado != null && limpezaResultado.Any())
            {
                sb.Append(@"<h2>Limpeza realizada</h2><div class='card'><table><tr><th>Item</th><th>Resultado</th></tr>");
                foreach (var (item, mb) in limpezaResultado)
                    sb.Append($"<tr><td>{item}</td><td>Limpo</td></tr>");
                sb.Append("</table></div>");
            }

            sb.Append($@"
  <h2>Programas instalados ({programas.Count})</h2>
  <div class='card'>
    <table>
      <tr><th>Nome</th><th>Versão</th><th>Fabricante</th></tr>
      {string.Join("", programas.Take(200).Select(p => $"<tr><td>{p.Nome}</td><td>{p.Versao}</td><td>{p.Fabricante}</td></tr>"))}
    </table>
    {(programas.Count > 200 ? $"<p>... e mais {programas.Count - 200} programas.</p>" : "")}
  </div>

  <p style='color:#5a6685; margin-top:32px;'>Relatório gerado automaticamente pelo SystemWM em {diagnostico.ColetadoEm:dd/MM/yyyy HH:mm:ss}.</p>
</body>
</html>");

            return sb.ToString();
        }

        private string ClasseTemperaturaCpu(double t) => t >= 85 ? "bad" : t >= 70 ? "warn" : "ok";
        private string ClasseTemperaturaDisco(double t) => t >= 60 ? "bad" : t >= 45 ? "warn" : "ok";
    }
}
