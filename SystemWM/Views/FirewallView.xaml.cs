using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SystemWM.Models;

namespace SystemWM.Views
{
    public partial class FirewallView : UserControl
    {
        public FirewallView()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

        private async Task CarregarAsync()
        {
            TxtStatusFirewall.Text = "Verificando...";
            bool ativo = await Task.Run(() => AppState.Firewall.EstaAtivo());
            AtualizarStatusVisual(ativo);

            var regras = await Task.Run(() => AppState.Firewall.ListarRegras());
            AppState.UltimasRegrasFirewall = regras;
            AppState.FirewallAtivoCache = ativo;
            ListaRegras.ItemsSource = regras;
        }

        private void AtualizarStatusVisual(bool ativo)
        {
            TxtStatusFirewall.Text = ativo ? "🟢 Ativo" : "🔴 Desativado";
        }

        private async void BtnAtivar_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await Task.Run(() => AppState.Firewall.AtivarFirewall());
            if (ok) AtualizarStatusVisual(true);
            else MessageBox.Show("Não foi possível ativar o Firewall. Confirme se o programa está rodando como Administrador.", "SystemWM");
        }

        private async void BtnDesativar_Click(object sender, RoutedEventArgs e)
        {
            var confirmar = MessageBox.Show(
                "Tem certeza que deseja DESATIVAR o Firewall do Windows? Isso reduz a proteção da máquina.",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes) return;

            bool ok = await Task.Run(() => AppState.Firewall.DesativarFirewall());
            if (ok) AtualizarStatusVisual(false);
            else MessageBox.Show("Não foi possível desativar o Firewall.", "SystemWM");
        }

        private async void BtnCriarRegra_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNomeRegra.Text) || !int.TryParse(TxtPorta.Text, out int porta))
            {
                MessageBox.Show("Informe um nome de regra e uma porta numérica válida.", "SystemWM");
                return;
            }

            string protocolo = (CmbProtocolo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "TCP";
            bool permitir = (CmbAcao.SelectedIndex == 0);

            bool ok = await Task.Run(() => AppState.Firewall.CriarRegraPorta(TxtNomeRegra.Text, porta, protocolo, permitir));

            if (ok)
            {
                MessageBox.Show("Regra criada com sucesso.", "SystemWM");
                TxtNomeRegra.Clear();
                TxtPorta.Clear();
                await CarregarAsync();
            }
            else
            {
                MessageBox.Show("Não foi possível criar a regra.", "SystemWM");
            }
        }

        private async void RegraCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || cb.Tag is not FirewallRegra regra) return;
            await Task.Run(() => AppState.Firewall.HabilitarRegra(regra.Nome, cb.IsChecked == true));
        }
    }
}
