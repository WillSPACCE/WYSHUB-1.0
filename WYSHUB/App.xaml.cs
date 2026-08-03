using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SystemWM
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // QuestPDF removed; PDF generation not supported in this build.

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            try
            {
                var main = new MainWindow();
                MainWindow = main;
                main.Show();
            }
            catch (Exception ex)
            {
                // Log full exception ToString() to capture XamlParseException.SourceUri and other details
                LogException(ex.ToString(), "OnStartup_LoadMainWindow");
                MessageBox.Show($"Falha ao carregar a janela principal:\n{ex.Message}\n\nDetalhes gravados em Documents/SystemWM/Logs", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Error);
                // Encerrar aplicação após log
                Shutdown(-1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var detalhe = MontarDetalheException(e.Exception, "DispatcherUnhandledException");
            LogException(detalhe, "DispatcherUnhandledException");
            MessageBox.Show($"Ocorreu um erro inesperado:\n{e.Exception.Message}\n\nDetalhes técnicos foram gravados em: Documents/SystemWM/Logs", "SystemWM", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var detalhe = MontarDetalheException(ex, "CurrentDomain_UnhandledException");
                LogException(detalhe, "CurrentDomain_UnhandledException");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            var detalhe = MontarDetalheException(e.Exception, "TaskScheduler_UnobservedTaskException");
            LogException(detalhe, "TaskScheduler_UnobservedTaskException");
            e.SetObserved();
        }

        private static string MontarDetalheException(Exception exception, string source)
        {
            var mensagem = $"Source: {source}\nType: {exception.GetType().FullName}\nMessage: {exception.Message}\nStackTrace:\n{exception.StackTrace}\n";

            Exception? inner = exception.InnerException;
            var nivel = 1;
            while (inner != null)
            {
                mensagem += $"\n--- InnerException {nivel} ---\nType: {inner.GetType().FullName}\nMessage: {inner.Message}\nStackTrace:\n{inner.StackTrace}\n";
                inner = inner.InnerException;
                nivel++;
            }

            return mensagem;
        }

        private static void LogException(string detalhe, string source)
        {
            try
            {
                var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SystemWM", "Logs");
                Directory.CreateDirectory(pasta);
                var caminho = Path.Combine(pasta, $"SystemWM_Error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                File.WriteAllText(caminho, detalhe);
            }
            catch
            {
                // Não interromper o app durante log de falha.
            }
        }
    }
}
