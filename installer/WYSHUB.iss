[Setup]
AppName=WYSHUB
AppVersion=1.0.0
DefaultDirName={autopf}\WYSHUB
DefaultGroupName=WYSHUB
OutputBaseFilename=WYSHUB_Setup
Compression=lzma2
SolidCompression=yes
SetupIconFile=..\WYSHUB\icons\Light.ico
OutputDir=output
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianPortuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "..\WYSHUB\bin\Release\net8.0-windows\win-x64\publish\WYSHUB.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WYSHUB\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WYSHUB"; Filename: "{app}\WYSHUB.exe"; WorkingDir: "{app}"; IconFilename: "{app}\WYSHUB.exe"
Name: "{commondesktop}\WYSHUB"; Filename: "{app}\WYSHUB.exe"; WorkingDir: "{app}"; IconFilename: "{app}\WYSHUB.exe"

[Run]
Filename: "{app}\WYSHUB.exe"; Description: "Launch WYSHUB"; Flags: nowait postinstall skipifsilent
