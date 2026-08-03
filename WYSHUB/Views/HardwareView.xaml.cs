using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SystemWM.Models;

namespace SystemWM.Views
{
    public partial class HardwareView : UserControl
    {
        public HardwareView()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

        private void BtnGerarRelatorio_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Relatório gerado. Envio por email será configurado depois.", "Relatório", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAbrirGerenciador_Click(object sender, RoutedEventArgs e) => OpenDeviceManager();

        private void MenuItemAbrirGerenciador_Click(object sender, RoutedEventArgs e) => OpenDeviceManager();

        private static void OpenDeviceManager()
        {
            try
            {
                Process.Start(new ProcessStartInfo("devmgmt.msc") { UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show("Não foi possível abrir o Gerenciador de Dispositivos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CarregarAsync()
        {
            TxtCarregando.Visibility = Visibility.Visible;

            var diag = AppState.UltimoDiagnostico ?? await Task.Run(() => AppState.Hardware.ColetarTudoSemDispositivos());
            AppState.UltimoDiagnostico = diag;

            ListaCpu.ItemsSource = LinhasCpu(diag.Cpu);
            ListaRam.ItemsSource = LinhasRam(diag.Ram);
            ListaDiscos.ItemsSource = LinhasDiscos(diag.Discos);
            ListaDispositivos.ItemsSource = null;

            if (diag.Gpu != null)
            {
                CardGpu.Visibility = Visibility.Visible;
                ListaGpu.ItemsSource = LinhasGpu(diag.Gpu);
            }

            TxtCarregando.Visibility = Visibility.Collapsed;
        }

        private async void ExpanderDispositivos_Expanded(object sender, RoutedEventArgs e)
        {
            if (AppState.UltimoDiagnostico?.Dispositivos?.Count > 0)
            {
                ListaDispositivos.ItemsSource = AppState.UltimoDiagnostico.Dispositivos;
                return;
            }

            TxtCarregando.Visibility = Visibility.Visible;
            var dispositivos = await Task.Run(() => AppState.Hardware.ColetarDispositivos());
            TxtCarregando.Visibility = Visibility.Collapsed;

            if (AppState.UltimoDiagnostico != null)
                AppState.UltimoDiagnostico.Dispositivos = dispositivos;

            ListaDispositivos.ItemsSource = dispositivos;
        }

        private List<string> LinhasCpu(CpuInfo c) => new()
        {
            $"Modelo: {c.Nome}",
            $"Uso atual: {c.UsoAtualPercent:0}%",
            $"Frequência atual: {c.FrequenciaAtualGHz} GHz  |  Base: {c.FrequenciaBaseGHz} GHz",
            $"Temperatura: {c.TemperaturaC:0}°C",
            $"TDP: {c.TdpWatts:0} W  |  Voltagem: {c.VoltagemV} V",
            $"Núcleos físicos: {c.NucleosFisicos}  |  Threads: {c.Threads}",
            $"Fan CPU: {c.FanCpuRpm:0} RPM  |  Fan Sistema: {c.FanSistemaRpm:0} RPM",
        };

        private List<string> LinhasRam(RamInfo r) => new()
        {
            $"Total instalado: {r.TotalGB} GB",
            $"Tipo: {r.Tipo}  |  Velocidade: {r.VelocidadeMHz} MHz",
            $"Em uso: {r.UsoPercent}% ({r.UsadaGB} GB)",
            $"Disponível: {r.DisponivelGB} GB",
            $"Cache: {r.CacheGB} GB  |  Em espera: {r.EmEsperaGB} GB",
            $"Comprometido: {r.ComprometidoGB} GB",
            $"Slots usados: {r.SlotsUsados} de {r.SlotsTotais}",
        };

        private List<string> LinhasDiscos(List<DiscoInfo> discos)
        {
            var lista = new List<string>();
            foreach (var d in discos)
            {
                lista.Add($"■ {d.Modelo} ({d.Interface})");
                lista.Add($"   Capacidade: {d.CapacidadeTotalGB} GB  |  Usado: {d.EspacoUsadoGB} GB  |  Livre: {d.EspacoLivreGB} GB");
                lista.Add($"   Temperatura: {d.TemperaturaC:0}°C  |  Saúde: {d.SaudePercentTexto}");
            }
            return lista;
        }

        private List<string> LinhasGpu(GpuInfo g) => new()
        {
            $"Modelo: {g.Nome}",
            $"Uso: {g.UsoPercent:0}%  |  Temperatura: {g.TemperaturaC:0}°C",
            $"Memória: {g.MemoriaUsadaGB} / {g.MemoriaTotalGB} GB",
            $"Consumo: {g.ConsumoWatts:0} W  |  Fan: {g.FanRpm:0} RPM",
        };
    }
}
