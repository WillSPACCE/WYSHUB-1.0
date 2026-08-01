using System.Collections.Generic;

namespace SystemWM.Models
{
    public class CpuInfo
    {
        public string Nome { get; set; } = "";
        public double UsoAtualPercent { get; set; }
        public double FrequenciaAtualGHz { get; set; }
        public double FrequenciaBaseGHz { get; set; }
        public double TemperaturaC { get; set; }
        public double TdpWatts { get; set; }
        public double VoltagemV { get; set; }
        public int NucleosFisicos { get; set; }
        public int Threads { get; set; }
        public double FanCpuRpm { get; set; }
        public double FanSistemaRpm { get; set; }
        public List<double> CargaPorNucleo { get; set; } = new();
        public List<double> TemperaturaPorNucleo { get; set; } = new();
    }

    public class RamInfo
    {
        public double TotalGB { get; set; }
        public double UsadaGB { get; set; }
        public double DisponivelGB { get; set; }
        public double CacheGB { get; set; }
        public double EmEsperaGB { get; set; }
        public double ComprometidoGB { get; set; }
        public double UsoPercent { get; set; }
        public string Tipo { get; set; } = "DDR4";
        public double VelocidadeMHz { get; set; }
        public int SlotsUsados { get; set; }
        public int SlotsTotais { get; set; }
    }

    public class DiscoInfo
    {
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string Interface { get; set; } = ""; // NVMe / SATA / PCIe x4 etc
        public double CapacidadeTotalGB { get; set; }
        public double EspacoUsadoGB { get; set; }
        public double EspacoLivreGB { get; set; }
        public double TemperaturaC { get; set; }
        public string SaudePercentTexto { get; set; } = ""; // ex: "100% (Excelente)"
        public double UsoPercent { get; set; }
    }

    public class GpuInfo
    {
        public string Nome { get; set; } = "";
        public double UsoPercent { get; set; }
        public double TemperaturaC { get; set; }
        public double MemoriaTotalGB { get; set; }
        public double MemoriaUsadaGB { get; set; }
        public double ConsumoWatts { get; set; }
        public double FanRpm { get; set; }
    }

    public class SistemaInfo
    {
        public string NomeHost { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string PlacaMae { get; set; } = "";
        public string Chipset { get; set; } = "";
        public string BiosVersao { get; set; } = "";
        public string SistemaOperacional { get; set; } = "";
        public string Arquitetura { get; set; } = "";
        public string Uptime { get; set; } = "";
        public string RedeStatus { get; set; } = "";
        public string IpLocal { get; set; } = "";
        public string IpPublica { get; set; } = "";
        public bool VpnAtiva { get; set; }
    }

    public class DiagnosticoCompleto
    {
        public CpuInfo Cpu { get; set; } = new();
        public RamInfo Ram { get; set; } = new();
        public List<DiscoInfo> Discos { get; set; } = new();
        public GpuInfo? Gpu { get; set; }
        public SistemaInfo Sistema { get; set; } = new();
        public System.DateTime ColetadoEm { get; set; } = System.DateTime.Now;
    }
}
