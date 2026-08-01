# WYSHUB

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D7?style=for-the-badge&logo=microsoft" alt="Windows" />
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/WPF-Desktop%20App-6C63FF?style=for-the-badge" alt="WPF" />
</p>

Sistema desktop para suporte técnico e visitas de campo. Ele coleta diagnóstico completo do cliente,
controla o firewall do Windows, executa limpezas rápidas, lista programas instalados e gera relatórios
prontos para envio por e-mail usando o serviço do Resend.

## ✨ O que ele faz

- Diagnóstico de hardware com foco em CPU, RAM, SSD/HD, GPU e rede.
- Leitura de temperatura, uso, carga e sensores de hardware.
- Ativação/desativação do Firewall do Windows e gerenciamento de regras.
- Limpeza de arquivos temporários, cache do Windows, prefetch e lixeira.
- Listagem de programas instalados para auditoria de máquina.
- Geração de relatório em HTML com dados da visita.
- Envio do relatório por e-mail via API do Resend.

## 🧭 Telas do sistema

- Dashboard: visão geral rápida da máquina e do diagnóstico.
- Hardware: informações detalhadas de hardware e sensores.
- Firewall: status do firewall, regras e alteração de portas.
- Manutenção: limpeza de arquivos e inventário de programas.
- Relatórios: geração do arquivo final com layout de visita.
- Configurações: API key do Resend e dados do técnico.

## ⚙️ Requisitos para rodar

- Windows 10 ou 11
- Visual Studio 2022 com a carga de trabalho .NET Desktop Development
- Ou .NET SDK 8.0 + terminal
- Permissão de Administrador para algumas funções do sistema
- Conta no Resend para envio de e-mail

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

## 📝 Observações importantes

- Algumas funções exigem privilégios administrativos do Windows.
- Em máquinas sem GPU dedicada, a seção de GPU pode aparecer vazia ou não existir.
- O projeto usa `LibreHardwareMonitorLib` para leitura de sensores e o `netsh` para regras do firewall.

## 🌐 DOCUMENTAÇÃO 

A documentação visual está em `docs/index.html` e pode ser publicada com GitHub Pages.

## 🚀 Próximos passos

- Versionar o projeto em GitHub.
- Criar releases com builds estáveis.
- Adicionar screenshots e uma demo de uso.
