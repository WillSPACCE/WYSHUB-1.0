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
        public static bool FirewallAtivoCache;
        public static ClienteVisita ClienteAtual = new();
    }
}
