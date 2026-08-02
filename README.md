# WYSHUB

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D7?style=for-the-badge&logo=microsoft" alt="Windows" />
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/WPF-Desktop%20App-6C63FF?style=for-the-badge" alt="WPF" />
</p>

WYSHUB é um sistema desktop para suporte técnico, visitas de campo e auditoria de máquinas Windows. A versão 1.0 já reúne diagnóstico completo, controle de firewall, limpeza rápida, levantamento de programas instalados e geração de relatórios prontos para envio.

## ✅ O que a versão 1.0 oferece

- Diagnóstico completo de hardware: CPU, RAM, SSD/HD, GPU e rede.
- Leitura de temperatura, uso, carga e sensores quando disponíveis.
- Ativação e desativação do firewall do Windows com gestão de regras.
- Limpeza de arquivos temporários, cache, prefetch e lixeira.
- Listagem de programas instalados para auditoria.
- Geração de relatório em HTML com dados da visita.
- Envio de relatório por e-mail via API do Resend.
- Distribuição pronta para clientes com pasta portátil e instalador automatizado.

## 🧭 Telas principais

- Dashboard: visão geral rápida do estado da máquina.
- Hardware: informações detalhadas sobre sensores e componentes.
- Firewall: status, regras e alterações de portas.
- Manutenção: limpeza de arquivos e inventário de programas.
- Relatórios: geração do documento final da visita.
- Configurações: API Key do Resend, remetente e dados do técnico.

## ⚙️ Requisitos

- Windows 10 ou 11.
- Permissão de Administrador para algumas funções, como sensores, limpeza e firewall.
- Conta no Resend para envio de e-mail (opcional, se o usuário quiser utilizar essa função).

## 🚀 Como usar

### Opção 1: instalar no computador do cliente
1. Baixe o pacote [instaladores/setup.zip](instaladores/setup.zip).
2. Extraia a pasta.
3. Execute [instaladores/setup/install.ps1](instaladores/setup/install.ps1) como administrador.
4. O programa será instalado em C:\Program Files\WYSHUB e terá atalhos na área de trabalho e no menu Iniciar.

### Opção 2: versão portátil
1. Use a pasta [instaladores/WYSHUB_Portavel](instaladores/WYSHUB_Portavel).
2. Execute o arquivo WYSHUB.exe diretamente.
3. Se o Windows pedir elevação, aceite. Isso é necessário para partes do sistema.

## 🧰 Estrutura de distribuição

- [instaladores/WYSHUB_Portavel](instaladores/WYSHUB_Portavel): pacote portátil com o executável, DLLs e dependências.
- [instaladores/setup](instaladores/setup): scripts de instalação com atalhos para Windows.
- [installer](installer): pacote adicional de distribuição.

## 🛠️ Desenvolvimento

Para abrir o projeto no Visual Studio:
1. Abra [SystemWM.sln](SystemWM.sln).
2. Restaure os pacotes NuGet.
3. Execute o projeto no modo Debug.

Para gerar a build localmente:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

## 📝 Observações importantes

- Algumas funções dependem de permissões administrativas do Windows.
- A leitura de sensores pode variar conforme hardware, drivers e compatibilidade do equipamento.
- O projeto usa LibreHardwareMonitorLib para sensores e netsh para regras do firewall.

## 🌐 Documentação

A documentação visual está em [docs/index.html](docs/index.html).

## 📦 Versão 1.0

Esta versão foi preparada para uso real, com distribuição para clientes, instalação simplificada e materiais prontos para teste e validação.
