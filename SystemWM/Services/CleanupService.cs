using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemWM.Models;

namespace SystemWM.Services
{
    /// <summary>
    /// Faz a limpeza básica do Windows: temp do usuário, temp do Windows,
    /// cache do Windows Update, prefetch e Lixeira.
    /// </summary>
    public class CleanupService
    {
        public List<ItemLimpeza> ListarItensDisponiveis()
        {
            var itens = new List<ItemLimpeza>
            {
                new() { Nome = "Arquivos temporários do usuário", CaminhoTipo = "TempUser",
                        Descricao = "%TEMP% - arquivos temporários gerados por programas do usuário atual" },
                new() { Nome = "Arquivos temporários do Windows", CaminhoTipo = "TempWindows",
                        Descricao = "C:\\Windows\\Temp - arquivos temporários do sistema" },
                new() { Nome = "Cache do Windows Update", CaminhoTipo = "CacheWindowsUpdate",
                        Descricao = "C:\\Windows\\SoftwareDistribution\\Download - pacotes de atualização já instalados" },
                new() { Nome = "Cache de Prefetch", CaminhoTipo = "PrefetchCache",
                        Descricao = "C:\\Windows\\Prefetch - cache de inicialização de programas" },
                new() { Nome = "Lixeira", CaminhoTipo = "Lixeira",
                        Descricao = "Esvaziar a Lixeira do Windows (todos os discos)" },
            };

            foreach (var item in itens)
                item.TamanhoEstimadoMB = Math.Round(CalcularTamanhoMB(item.CaminhoTipo), 1);

            return itens;
        }

        private IEnumerable<string> CaminhosParaTipo(string tipo) => tipo switch
        {
            "TempUser" => new[] { Path.GetTempPath() },
            "TempWindows" => new[] { @"C:\Windows\Temp" },
            "CacheWindowsUpdate" => new[] { @"C:\Windows\SoftwareDistribution\Download" },
            "PrefetchCache" => new[] { @"C:\Windows\Prefetch" },
            _ => Array.Empty<string>()
        };

        private double CalcularTamanhoMB(string tipo)
        {
            if (tipo == "Lixeira")
                return 0; // calculado de forma diferente, via Shell; deixado como 0 para não travar a tela

            double totalBytes = 0;
            foreach (var caminho in CaminhosParaTipo(tipo))
            {
                if (!Directory.Exists(caminho)) continue;
                try
                {
                    totalBytes += new DirectoryInfo(caminho)
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Sum(f => { try { return f.Length; } catch { return 0; } });
                }
                catch { /* pastas protegidas do sistema podem negar acesso a alguns arquivos */ }
            }
            return totalBytes / (1024.0 * 1024);
        }

        /// <summary>Executa a limpeza dos itens marcados. Retorna um resumo (item -> MB liberados).</summary>
        public Dictionary<string, double> Limpar(List<ItemLimpeza> itensSelecionados)
        {
            var resultado = new Dictionary<string, double>();

            foreach (var item in itensSelecionados.Where(i => i.Selecionado))
            {
                double liberadoMB = 0;

                if (item.CaminhoTipo == "Lixeira")
                {
                    liberadoMB = EsvaziarLixeira();
                }
                else
                {
                    foreach (var caminho in CaminhosParaTipo(item.CaminhoTipo))
                        liberadoMB += LimparPasta(caminho);
                }

                resultado[item.Nome] = Math.Round(liberadoMB, 1);
            }

            return resultado;
        }

        private double LimparPasta(string caminho)
        {
            if (!Directory.Exists(caminho)) return 0;
            double liberadoBytes = 0;

            foreach (var arquivo in Directory.EnumerateFiles(caminho, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(arquivo);
                    long tamanho = fi.Length;
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                    liberadoBytes += tamanho;
                }
                catch { /* arquivo em uso ou protegido: ignora e segue */ }
            }

            // Remove subpastas vazias
            foreach (var dir in Directory.EnumerateDirectories(caminho, "*", SearchOption.AllDirectories).Reverse())
            {
                try { if (!Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir); }
                catch { }
            }

            return liberadoBytes / (1024.0 * 1024);
        }

        private double EsvaziarLixeira()
        {
            try
            {
                // SHEmptyRecycleBin via API nativa do Shell do Windows
                Shell32Interop.SHEmptyRecycleBin(IntPtr.Zero, null, Shell32Interop.SHERB_NOCONFIRMATION | Shell32Interop.SHERB_NOPROGRESSUI | Shell32Interop.SHERB_NOSOUND);
                return 0; // Windows não retorna o tamanho liberado diretamente
            }
            catch
            {
                return 0;
            }
        }
    }

    internal static class Shell32Interop
    {
        public const uint SHERB_NOCONFIRMATION = 0x00000001;
        public const uint SHERB_NOPROGRESSUI = 0x00000002;
        public const uint SHERB_NOSOUND = 0x00000004;

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);
    }
}
