using System.Collections.Generic;
using System.IO;
using SystemWM.Models;
using SystemWM.Services;

namespace SystemWM.Tests;

public class ReportServiceTests
{
    [Fact]
    public void GerarTxt_DeveRetornarMensagemQuandoNenhumaSecaoSelecionada()
    {
        var service = new ReportService();
        var cliente = new ClienteVisita { NomeCliente = "Cliente", DataVisita = System.DateTime.Now };
        var diagnostico = new DiagnosticoCompleto();

        var resultado = service.GerarTxt(
            cliente,
            diagnostico,
            new List<ProgramaInstalado>(),
            new Dictionary<string, double>(),
            new List<FirewallRegra>(),
            false,
            new List<PortaAtivaInfo>(),
            false,
            false,
            false,
            false);

        Assert.Contains("Nenhuma seção foi selecionada", resultado);
        Assert.DoesNotContain("Resumo do Dashboard", resultado);
        Assert.DoesNotContain("Hardware completo", resultado);
    }

    [Fact]
    public void GerarHtml_DeveNaoInserirSecoesNaoSelecionadas()
    {
        var service = new ReportService();
        var cliente = new ClienteVisita { NomeCliente = "Cliente", DataVisita = System.DateTime.Now };
        var diagnostico = new DiagnosticoCompleto();

        var resultado = service.GerarHtml(
            cliente,
            diagnostico,
            new List<ProgramaInstalado>(),
            new Dictionary<string, double>(),
            new List<FirewallRegra>(),
            false,
            new List<PortaAtivaInfo>(),
            false,
            false,
            false,
            false);

        Assert.Contains("Nenhuma seção foi selecionada", resultado);
        Assert.DoesNotContain("Resumo do Dashboard", resultado);
        Assert.DoesNotContain("Hardware completo", resultado);
    }

    [Fact]
    public void GerarTxt_DeveIncluirApenasProgramasSelecionadosNoRelatorio()
    {
        var service = new ReportService();
        var cliente = new ClienteVisita { NomeCliente = "Cliente", DataVisita = System.DateTime.Now };
        var diagnostico = new DiagnosticoCompleto();
        var programas = new List<ProgramaInstalado>
        {
            new() { Nome = "Chrome", IncluirNoRelatorio = true },
            new() { Nome = "Edge", IncluirNoRelatorio = false }
        };

        var resultado = service.GerarTxt(
            cliente,
            diagnostico,
            programas,
            new Dictionary<string, double>(),
            new List<FirewallRegra>(),
            false,
            new List<PortaAtivaInfo>(),
            true,
            false,
            false,
            false);

        Assert.Contains("Chrome", resultado);
        Assert.DoesNotContain("Edge", resultado);
    }

    [Fact]
    public void GerarTxt_DeveIncluirLimpezasNoRelatorioQuandoHouverResultadoMesmoSemSecaoMarcada()
    {
        var service = new ReportService();
        var cliente = new ClienteVisita { NomeCliente = "Cliente", DataVisita = System.DateTime.Now };
        var diagnostico = new DiagnosticoCompleto();
        var limpezaResultado = new Dictionary<string, double>
        {
            ["Pasta temporária"] = 2.5
        };

        var resultado = service.GerarTxt(
            cliente,
            diagnostico,
            new List<ProgramaInstalado>(),
            limpezaResultado,
            new List<FirewallRegra>(),
            false,
            new List<PortaAtivaInfo>(),
            false,
            false,
            false,
            false);

        Assert.Contains("Limpeza realizada", resultado);
        Assert.Contains("Pasta temporária", resultado);
    }

    [Fact]
    public void CleanupService_DeveIncluirCaminhoDaPastaPersonalizadaNoResultado()
    {
        var service = new CleanupService();
        var pastaPersonalizada = "C:\\Temp\\TestePersonalizado";

        var itens = new List<ItemLimpeza>
        {
            new ItemLimpeza
            {
                Nome = "Pasta personalizada",
                CaminhoTipo = "Personalizada",
                CaminhoPersonalizado = pastaPersonalizada,
                Selecionado = true
            }
        };

        var resultado = service.Limpar(itens);

        Assert.Contains($"Pasta personalizada: {pastaPersonalizada}", resultado.Keys);
    }

    [Fact]
    public void AplicarTemplateEmail_DeveSubstituirPlaceholders()
    {
        var service = new EmailService();
        var resultado = service.AplicarTemplateEmail(
            "Olá {cliente} da {empresa}.\nAtenciosamente, {tecnico}.\n\n{relatorio}",
            "Maria",
            "Acme",
            "José",
            "01/08/2026",
            "Resumo do relatório");

        Assert.Contains("Olá Maria da Acme.", resultado);
        Assert.Contains("José", resultado);
        Assert.Contains("Resumo do relatório", resultado);
    }

    [Fact]
    public void SettingsService_DeveSalvarApiKeyEmArquivoDentroDaPastaDoPrograma()
    {
        var service = new SettingsService();
        var caminho = service.ObterCaminhoArquivoApi();

        if (File.Exists(caminho))
            File.Delete(caminho);

        service.SalvarApiKey("api-teste");

        Assert.True(File.Exists(caminho));
        Assert.Equal("api-teste", File.ReadAllText(caminho).Trim());
        Assert.Equal("api-teste", service.CarregarApiKey());

        if (File.Exists(caminho))
            File.Delete(caminho);
    }

    [Fact]
    public void ProgramaInstalado_DeveComecarNaoSelecionadoPorPadrao()
    {
        var programa = new ProgramaInstalado();

        Assert.False(programa.IncluirNoRelatorio);
    }

    [Fact]
    public void DashboardCardsSelecionados_DevePersistirNoEstadoGlobal()
    {
        AppState.DashboardCardsSelecionados.Clear();
        AppState.DashboardCardsSelecionados.Add("CpuCard");
        AppState.DashboardCardsSelecionados.Add("RamCard");

        Assert.Contains("CpuCard", AppState.DashboardCardsSelecionados);
        Assert.Contains("RamCard", AppState.DashboardCardsSelecionados);
    }
}
