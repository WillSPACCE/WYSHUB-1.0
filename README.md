# SystemWM — Ferramenta de Visitas Técnicas

App desktop (Windows) para uso em visitas técnicas: coleta diagnóstico completo da máquina do cliente
(CPU, RAM, SSD/HD, GPU, sistema), gerencia o Firewall do Windows, faz limpeza básica de arquivos,
lista programas instalados e gera/envia um relatório por e-mail via **Resend**.

## Requisitos para compilar
- Windows 10/11
- Visual Studio 2022 (com a carga de trabalho ".NET Desktop Development") **ou** .NET SDK 8.0 + linha de comando
- Conexão com a internet na primeira vez (para baixar os pacotes NuGet: `LibreHardwareMonitorLib`, `System.Management`, `System.Net.Http.Json`)

## Como abrir e rodar (modo desenvolvimento)
1. Abra `SystemWM.sln` no Visual Studio.
2. Aguarde o restore automático dos pacotes NuGet.
3. Aperte F5. O Windows vai pedir elevação (UAC) — aceite, é necessário para ler sensores de
   temperatura/fan e mexer no Firewall.

## Como gerar o executável final (.exe único, sem precisar instalar .NET no PC do cliente)
Abra o terminal na pasta do projeto (`SystemWM/SystemWM`) e rode:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

O `.exe` final fica em:
```
SystemWM\SystemWM\bin\Release\net8.0-windows\win-x64\publish\SystemWM.exe
```

Esse único arquivo (~100-150MB) já roda em qualquer Windows 10/11 x64, sem precisar instalar nada.
Basta copiar ele (por exemplo, para um pendrive) e levar nas visitas técnicas.

> Dica: dê 2 cliques nele → o Windows vai pedir permissão de Administrador automaticamente (por causa
> do `app.manifest`), que é necessária para ler os sensores de hardware e mexer no Firewall.

## Configurando o envio de e-mail (Resend)
1. Crie uma conta em https://resend.com e gere uma **API Key**.
2. Configure um domínio verificado no Resend (ou use o remetente de testes `onboarding@resend.dev`
   enquanto não configurar seu domínio).
3. Abra o SystemWM → **Configurações** → cole a API Key, defina o remetente (ex:
   `SystemWM <relatorios@seudominio.com>`) e salve.
4. A API Key fica salva localmente em
   `%AppData%\SystemWM\settings.json` — nunca é enviada para lugar nenhum além da API do Resend.

## Estrutura do projeto
```
SystemWM/
├── SystemWM.sln
└── SystemWM/
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs      → sidebar + navegação
    ├── AppState.cs                                → estado/serviços compartilhados entre telas
    ├── app.manifest                               → força elevação (UAC) automática
    ├── Models/                                    → classes de dados (CPU, RAM, Disco, GPU, Firewall...)
    ├── Services/
    │   ├── HardwareMonitorService.cs               → LibreHardwareMonitorLib (temperaturas, uso, fans)
    │   ├── FirewallService.cs                      → netsh advfirewall (ativar/desativar, regras)
    │   ├── CleanupService.cs                       → limpeza de temp/cache/lixeira
    │   ├── InstalledProgramsService.cs             → lê o Registro do Windows (Programas e Recursos)
    │   ├── ReportService.cs                        → monta o relatório em HTML
    │   ├── EmailService.cs                         → envia via API Resend
    │   ├── NetworkService.cs                       → IP público
    │   └── SettingsService.cs                      → salva configurações localmente
    ├── Views/                                      → uma tela por funcionalidade (Dashboard, Hardware,
    │                                                  Firewall, Manutenção, Relatórios, Configurações)
    └── Themes/DarkTheme.xaml                       → cores/estilos (tema dark azul/roxo)
```

## O que cada tela faz
- **Dashboard**: visão geral rápida (CPU, RAM, SSD, Sistema, rede) — igual ao protótipo.
- **Hardware**: detalhamento completo, inclusive GPU (se detectada).
- **Firewall**: liga/desliga o firewall do Windows, lista as regras existentes com checkbox
  para habilitar/desabilitar, e permite criar novas regras de porta (liberar/bloquear).
- **Manutenção**: limpeza com checkbox (temp do usuário, temp do Windows, cache do Windows
  Update, prefetch, lixeira) com confirmação antes de apagar, e lista de programas instalados
  (com filtro por nome).
- **Relatórios**: preenche dados do cliente/visita, gera o relatório em HTML (salvo também em
  Documentos\SystemWM\Relatorios) e envia por e-mail via Resend.
- **Configurações**: API Key do Resend, remetente, e-mail padrão, nome do técnico.

## Observações importantes
- O app **precisa rodar como Administrador** (já configurado para pedir isso automaticamente)
  para: ler temperatura/fans (drivers de hardware), alterar o Firewall, e limpar pastas do sistema.
- Em notebooks/desktops sem GPU dedicada (só vídeo integrado sem sensores), a seção de GPU
  simplesmente não aparece — isso é esperado.
- Antivírus podem, às vezes, alertar sobre softwares de monitoramento de hardware (o próprio
  LibreHardwareMonitor instala um driver de kernel temporário para ler sensores). Isso é normal
  e o mesmo comportamento do HWMonitor/HWiNFO.
- Para editar as cores/visual, tudo está centralizado em `Themes/DarkTheme.xaml`.
