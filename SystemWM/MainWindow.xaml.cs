using System;
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
            InitializeComponent();
            SetConteudo(new DashboardView());
        }

        private void Menu_Checked(object sender, RoutedEventArgs e)
        {
            if (ConteudoPrincipal == null) return;

            if (sender == RbDashboard) SetConteudo(new DashboardView());
            else if (sender == RbHardware) SetConteudo(new HardwareView());
            else if (sender == RbFirewall) SetConteudo(new FirewallView());
            else if (sender == RbManutencao) SetConteudo(new MaintenanceView());
            else if (sender == RbRelatorios) SetConteudo(new ReportsView());
            else if (sender == RbConfiguracoes) SetConteudo(new SettingsView());

            if (!_menuAberto)
            {
                ToggleMenu(false);
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

        private void TitleBarHitArea_MouseEnter(object sender, MouseEventArgs e)
        {
            MostrarBotoesJanela(true);
        }

        private void TitleBarHitArea_MouseLeave(object sender, MouseEventArgs e)
        {
            MostrarBotoesJanela(false);
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
