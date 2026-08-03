using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SystemWM.Views;

namespace SystemWM
{
    public partial class MainWindow : Window
    {
        private bool _menuAberto = true;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                try
                {
                    var pasta = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SystemWM", "Logs");
                    System.IO.Directory.CreateDirectory(pasta);
                    var caminho = System.IO.Path.Combine(pasta, $"MainWindow_Failed_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    System.IO.File.WriteAllText(caminho, ex.ToString());
                }
                catch { }
                throw;
            }

            SetConteudo(new DashboardView());
        }

        private void Menu_Checked(object sender, RoutedEventArgs e)
        {
            if (ConteudoPrincipal == null) return;

            if (sender == RbDashboard) SetConteudo(new DashboardView());
            else if (sender == RbHardware) SetConteudo(new HardwareView());
            else if (sender == RbFirewall) SetConteudo(new FirewallView());
            else if (sender == RbManutencao) SetConteudo(new MaintenanceView());
            else if (sender == RbProgramas) SetConteudo(new ProgramsView());
            else if (sender == RbUtilidades) SetConteudo(new UtilitiesView());
            else if (sender == RbRelatorios) SetConteudo(new ReportsView());
            else if (sender == RbEmail) SetConteudo(new SettingsView());
            else if (sender == RbConfiguracoes) SetConteudo(new ConfigView());

            if (!_menuAberto)
            {
                ToggleMenu(false);
            }
        }

        public void AbrirAbaEmailComRelatorio()
        {
            if (RbEmail != null)
            {
                RbEmail.IsChecked = true;
            }
        }

        private void BtnMenuToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu(!_menuAberto);
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnLogo_Click(object sender, RoutedEventArgs e)
        {
            const string url = "https://willspacce.netlify.app";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível abrir o site.\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TitleBarHitArea_MouseEnter(object sender, MouseEventArgs e)
        {
            MostrarBotoesJanela(true);
        }

        private void TitleBarHitArea_MouseLeave(object sender, MouseEventArgs e)
        {
            MostrarBotoesJanela(false);
        }

        private void TitleBarHitArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                    // Ignora se o movimento não puder ser iniciado.
                }
            }
        }

        private void MostrarBotoesJanela(bool mostrar)
        {
            var animacao = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            animacao.To = mostrar ? 1 : 1;
            WindowButtons.BeginAnimation(OpacityProperty, animacao);
        }

        private void ToggleMenu(bool abrir)
        {
            _menuAberto = abrir;

            var colunaSidebar = LayoutRoot.ColumnDefinitions[0];
            colunaSidebar.Width = abrir ? new GridLength(240) : new GridLength(0);

            var animacao = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            animacao.To = abrir ? 0 : -220;
            SidebarTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animacao);

            BtnMenuHamburger.Visibility = abrir ? Visibility.Collapsed : Visibility.Visible;
            BtnMenuToggle.Content = abrir ? "✕" : "☰";
            BtnMenuHamburger.Content = abrir ? "☰" : "☰";

            var fade = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            fade.To = abrir ? 1 : 0.95;
            SidebarPanel.BeginAnimation(OpacityProperty, fade);
        }

        private void SetConteudo(UserControl controle)
        {
            ConteudoPrincipal.Content = controle;

            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var scale = new DoubleAnimation
            {
                From = 0.97,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ConteudoPrincipal.BeginAnimation(OpacityProperty, fade);
            ContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scale);
            ContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scale);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            AppState.Hardware.Dispose();
        }
    }
}
