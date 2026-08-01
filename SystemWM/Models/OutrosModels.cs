using System;

namespace SystemWM.Models
{
    public class FirewallRegra
    {
        public string Nome { get; set; } = "";
        public string Direcao { get; set; } = ""; // Entrada / Saída
        public string Protocolo { get; set; } = "";
        public string Porta { get; set; } = "";
        public string Acao { get; set; } = ""; // Permitir / Bloquear
        public bool Habilitada { get; set; }
        public string Perfil { get; set; } = ""; // Domínio/Privado/Público
    }

    public class ItemLimpeza
    {
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string CaminhoTipo { get; set; } = ""; // "TempUser", "TempWindows", "Lixeira", "CacheWindowsUpdate", "PrefetchCache"
        public bool Selecionado { get; set; } = true;
        public double TamanhoEstimadoMB { get; set; }
    }

    public class ProgramaInstalado
    {
        public string Nome { get; set; } = "";
        public string Versao { get; set; } = "";
        public string Fabricante { get; set; } = "";
        public DateTime? DataInstalacao { get; set; }
        public double TamanhoMB { get; set; }
    }

    public class ClienteVisita
    {
        public string NomeCliente { get; set; } = "";
        public string EmpresaCliente { get; set; } = "";
        public string EmailDestino { get; set; } = "";
        public string Observacoes { get; set; } = "";
        public DateTime DataVisita { get; set; } = DateTime.Now;
    }
}
