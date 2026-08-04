# Guia de execução do WYSHUB em Windows Sandbox / máquina limpa

## 1. Requisitos mínimos

- Windows 10 ou 11 x64
- 4 GB de RAM recomendados
- acesso de administrador
- rede habilitada
- Microsoft .NET 8 Desktop Runtime x64 se a build não estiver self-contained

## 2. O que o programa precisa para funcionar

O projeto usa:

- WPF .NET 8
- LibreHardwareMonitorLib para sensores de CPU, RAM, SSD, GPU e ventiladores
- System.Management para leitura de WMI
- netsh advfirewall para regras de firewall
- acesso administrativo para alguns sensores e para manipular o firewall

## 3. Instalação rápida em um Windows Sandbox

1. Copie a pasta publicada do app para uma pasta estável, por exemplo:
   - C:\Program Files\WYSHUB

2. Abra um terminal como administrador.

3. Garanta que o WMI esteja ativo:

```powershell
sc config winmgmt start= auto
net start winmgmt
```

4. Se o app for executado a partir de uma build que não seja self-contained, instale o runtime .NET 8:
   - Microsoft .NET 8 Desktop Runtime x64

5. Execute o app como administrador.

## 4. Sensores e bibliotecas

- LibreHardwareMonitorLib já está declarado no projeto e é incluído automaticamente ao publicar o app.
- Alguns sensores podem não aparecer em ambientes virtualizados como Sandbox, pois dependem do hardware real do host.
- Mesmo assim, o app consegue abrir e exibir dados básicos do sistema sem depender totalmente desses sensores.

## 5. Como evitar que o Microsoft Defender delete ou bloqueie o programa

O Defender pode marcar o executável como suspeito em ambientes limpos ou de teste. A forma mais simples e segura é adicionar exclusões para a pasta e para o processo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\defender-exclusions.ps1
```

Esse script adiciona:
- exclusão da pasta do programa
- exclusão do processo WYSHUB.exe

Se o Defender continuar bloqueando, a opção mais forte é mover o app para:
- C:\Program Files\WYSHUB

e manter o executável dentro dessa pasta, que costuma reduzir falsos positivos.

## 6. Recomendação final

Para Sandbox, o melhor fluxo é:

- publicar o app em modo self-contained
- copiar para C:\Program Files\WYSHUB
- executar como administrador
- adicionar as exclusões do Defender
- testar primeiro sem abrir portas externas adicionais
