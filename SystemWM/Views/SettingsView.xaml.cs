using System.Windows;
using System.Windows.Controls;
using SystemWM.Services;

namespace SystemWM.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            var s = AppState.Settings.Carregar();
            TxtApiKey.Password = s.ResendApiKey;
            TxtRemetente.Text = s.EmailRemetente;
            TxtDestinoPadrao.Text = s.EmailDestinoPadrao;
            TxtNomeTecnico.Text = s.NomeTecnico;
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            var s = new AppSettings
            {
                ResendApiKey = TxtApiKey.Password,
                EmailRemetente = string.IsNullOrWhiteSpace(TxtRemetente.Text) ? "SystemWM <onboarding@resend.dev>" : TxtRemetente.Text,
                EmailDestinoPadrao = TxtDestinoPadrao.Text,
                NomeTecnico = TxtNomeTecnico.Text
            };

            AppState.Settings.Salvar(s);
            TxtStatus.Text = "Configurações salvas com sucesso.";
        }
    }
}
