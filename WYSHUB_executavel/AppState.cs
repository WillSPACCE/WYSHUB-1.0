using System;
using System.Collections.Generic;
using SystemWM.Models;
using SystemWM.Services;

namespace SystemWM
{
    /// <summary>
    /// Guarda em memória os dados coletados durante a sessão/visita atual,
    /// para que Dashboard, Manutenção, Firewall e Relatórios compartilhem a mesma informação
    /// sem precisar recoletar tudo de novo.
    /// </summary>
    public static class AppState
    {
        private static HardwareMonitorService? _hardware;
        public static HardwareMonitorService Hardware => _hardware ??= new HardwareMonitorService();

        public static readonly FirewallService Firewall = new();
        public static readonly CleanupService Cleanup = new();
        public static readonly InstalledProgramsService Programs = new();
        public static readonly ReportService Reports = new();
        public static readonly EmailService Email = new();
        public static readonly SettingsService Settings = new();
        public static readonly NetworkService Network = new();

        public static DiagnosticoCompleto? UltimoDiagnostico;
        public static List<ProgramaInstalado>? UltimosProgramas;
        public static Dictionary<string, double>? UltimaLimpeza;
        public static List<FirewallRegra>? UltimasRegrasFirewall;
        public static List<PortaAtivaInfo> PortasAtivasRelatorio { get; set; } = new();
        public static HashSet<string> DashboardCardsSelecionados { get; } = new(StringComparer.OrdinalIgnoreCase);
        public static string? UltimoRelatorioTxtGerado { get; set; }
        public static string? UltimoRelatorioHtmlGerado { get; set; }
        public static string? UltimoRelatorioNomeAnexo { get; set; }
        public static string? UltimoRelatorioCaminho { get; set; }
        public static string? UltimoRelatorioTipo { get; set; }
        public static bool RelatorioDisponivelParaEmail { get; set; }
        public static bool FirewallAtivoCache;

        public static bool RelatorioIncluirResumoDashboard { get; set; } = true;
        public static bool RelatorioIncluirHardware { get; set; } = false;
        public static bool RelatorioIncluirFirewall { get; set; } = false;
        public static bool RelatorioIncluirLimpeza { get; set; } = false;

        public static ClienteVisita ClienteAtual = new();
    }
}
