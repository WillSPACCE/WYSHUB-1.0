using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SystemWM.Models;

namespace SystemWM.Views
{
    public partial class MaintenanceView : UserControl
    {
        private List<ItemLimpeza> _itensLimpeza = new();
        private List<ProgramaInstalado> _programas = new();

        public MaintenanceView()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            _itensLimpeza = await Task.Run(() => AppState.Cleanup.ListarItensDisponiveis());
            ListaLimpeza.ItemsSource = _itensLimpeza;

            _programas = AppState.UltimosProgramas ?? await Task.Run(() => AppState.Programs.Listar());
            AppState.UltimosProgramas = _programas;
            TxtQtdProgramas.Text = $"{_programas.Count} programas encontrados";
            ListaProgramas.ItemsSource = _programas;
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = TxtFiltro.Text?.Trim() ?? "";
            ListaProgramas.ItemsSource = string.IsNullOrEmpty(filtro)
                ? _programas
                : _programas.Where(p => p.Nome.Contains(filtro, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private async void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            var selecionados = _itensLimpeza.Where(i => i.Selecionado).ToList();
            if (!selecionados.Any())
            {
                MessageBox.Show("Selecione ao menos um item para limpar.", "SystemWM");
                return;
            }

            var textoConfirmacao = "Os seguintes itens serão apagados permanentemente:\n\n" +
                string.Join("\n", selecionados.Select(i => $"• {i.Nome} (~{i.TamanhoEstimadoMB} MB)")) +
                "\n\nDeseja continuar?";

            var confirmar = MessageBox.Show(textoConfirmacao, "Confirmar limpeza", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmar != MessageBoxResult.Yes) return;

            BtnLimpar.IsEnabled = false;
            BtnLimpar.Content = "Limpando...";

            var resultado = await Task.Run(() => AppState.Cleanup.Limpar(selecionados));
            AppState.UltimaLimpeza = resultado;

            BtnLimpar.IsEnabled = true;
            BtnLimpar.Content = "🧹 Limpar selecionados";

            MessageBox.Show(
                "Limpeza concluída:\n\n" + string.Join("\n", resultado.Select(kv => $"• {kv.Key}: limpo")),
                "SystemWM");

            await CarregarAsync();
        }
    }
}
