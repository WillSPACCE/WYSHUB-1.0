using System.Windows;

namespace SystemWM.Views
{
    public partial class RestoreConfirmDialog : Window
    {
        public RestoreConfirmDialog()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
