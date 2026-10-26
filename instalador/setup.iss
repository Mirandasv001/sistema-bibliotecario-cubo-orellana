; ============================================================
;  Instalador - Sistema Bibliotecario CUBO
;  Compilador probado: Inno Setup 6.7.3
;
;  Pasos:
;    1) dotnet publish ..\BibliotecaApp\BibliotecaApp.csproj ^
;         -c Release -r win-x64 --self-contained true -o app
;    2) "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
;
;  Salida: bin\Instalador-BibliotecaCUBO-1.0.0.exe
;
;  Notas:
;    - La base de datos vive en %APPDATA%\BibliotecaCUBO y NO se
;      desinstala: conservarla es intencionado.
;    - Las imagenes de Recursos\ no se leen en tiempo de ejecucion
;      (los logotipos van embebidos); solo se distribuyen por si se
;      usan en documentacion.
; ============================================================

#define AppName "Sistema Bibliotecario CUBO"
#define AppVersion "1.0.0"
#define AppExe "BibliotecaApp.exe"
#define PubDir "app"

[Setup]
AppId={{5B4A0B7E-3C11-4F2E-9C6D-1A7E8D2F4C91}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Biblioteca CUBO
AppSupportURL=
DefaultDirName={autopf}\BibliotecaCUBO
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=bin
OutputBaseFilename=Instalador-BibliotecaCUBO-{#AppVersion}
SetupIconFile=..\BibliotecaApp\Recursos\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
MinVersion=10.0.14393
CloseApplications=yes
DisableReadyPage=no
VersionInfoVersion={#AppVersion}.0
VersionInfoProductName={#AppName}

[Languages]
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Publicacion self-contained: .exe + runtime de .NET + catalogo_cubo.csv + Recursos\
Source: "{#PubDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "LEEME.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
// La aplicacion abierta bloquea BibliotecaApp.exe y el .exe actualizado
// no se podria reemplazar durante una actualizacion.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  Exec('taskkill.exe', '/IM BibliotecaApp.exe /F', '', SW_HIDE,
       ewWaitUntilTerminated, ResultCode);
  Sleep(700);
end;
