using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace SystemWM.Views
{
    public partial class UtilitiesView : System.Windows.Controls.UserControl
    {
        public UtilitiesView()
        {
            InitializeComponent();
        }

        private void BtnSelecionarCompartilhar_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Selecione a pasta para compartilhar com a rede",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != WinForms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                return;

            TxtPastaSelecionada.Text = dialog.SelectedPath;
            var (sucesso, mensagem) = CompartilharPasta(dialog.SelectedPath);
            TxtStatusCompartilhamento.Text = mensagem;
            TxtStatusCompartilhamento.Foreground = sucesso ? FindResource("VerdeOk") as System.Windows.Media.Brush : FindResource("VermelhoErro") as System.Windows.Media.Brush;
        }

        private static (bool sucesso, string mensagem) CompartilharPasta(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !Directory.Exists(caminho))
                return (false, "A pasta selecionada é inválida ou não existe.");

            var nomeCompartilhamento = GerarNomeCompartilhamento(caminho);
            var argumentos = $"share \"{nomeCompartilhamento}\"=\"{caminho}\" /GRANT:Everyone,FULL";

            try
            {
                using var processo = Process.Start(new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = argumentos,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                if (processo == null)
                    return (false, "Falha ao iniciar o comando de compartilhamento.");

                var saida = processo.StandardOutput.ReadToEnd();
                var erro = processo.StandardError.ReadToEnd();
                processo.WaitForExit();

                if (processo.ExitCode == 0)
                    return (true, $"Pasta compartilhada como '{nomeCompartilhamento}'.");

                if (!string.IsNullOrEmpty(erro))
                    return (false, $"Erro ao compartilhar: {erro.Trim()}");

                return (false, string.IsNullOrWhiteSpace(saida) ? "Não foi possível compartilhar a pasta." : saida.Trim());
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao compartilhar: {ex.Message}");
            }
        }

        private static string GerarNomeCompartilhamento(string caminho)
        {
            var baseNome = Path.GetFileName(caminho.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrWhiteSpace(baseNome))
                baseNome = "Compartilhamento";

            baseNome = Regex.Replace(baseNome, "[^\\w\\d_-]", "_");
            if (baseNome.Length > 24)
                baseNome = baseNome[..24];

            return baseNome;
        }
    }
}
