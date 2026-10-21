; =====================================================================
;  INSTALADOR DE BIBLIOTECA CUBO - Sistema Bibliotecario
;  Script para Inno Setup 6.x  (self-contained: la app incluye el
;  motor .NET 10 embebido, no requiere descargas en la PC destino)
;
;  COMO COMPILARLO:
;   1) Abra "Inno Setup Compiler" (instalado desde jrsoftware.org).
;   2) Archivo > Abrir... > seleccione este archivo .iss
;   3) Presione F9 (Compilar). El instalador queda en la carpeta
;      "instalador" junto a este script.
; =====================================================================

#define AppName "Biblioteca CUBO"
#define AppVersion "1.0"
#define AppPublisher "Erick Miranda"
#define AppExeName "BibliotecaApp.exe"

; Carpeta de publicación: RUTA RELATIVA al propio .iss (que vive en la raíz del
; repo), así el instalador se compila en cualquier PC sin editar el script.
; Sale de: dotnet publish -c Release -r win-x64 --self-contained true
#define PublishDir "BibliotecaApp\bin\Release\net10.0-windows\win-x64\publish"

[Setup]
AppId={{8F2C6A1E-4D3B-4E9A-9C7D-1B5E2F8A3C4D}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
VersionInfoVersion=1.0.0.0
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Requiere administrador (la BD del usuario vive en %APPDATA%, sin problemas de permisos)
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
; Salida del instalador: carpeta "instalador" junto a este .iss
OutputDir=instalador
OutputBaseFilename=BibliotecaCUBO-Instalador-{#AppVersion}
SetupIconFile={#PublishDir}\Recursos\app.ico
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
SetupLogging=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Contenido COMPLETO de la carpeta publish: BibliotecaApp.exe, TODAS las .dll
; (incluye e_sqlite3.dll, la nativa de SQLite, y el motor .NET 10 completo),
; el CSV del catálogo junto al .exe y la carpeta Recursos\ con todas las
; imágenes y el icono. Todo a {app}.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Acceso directo del menú de inicio con el icono oficial de la app
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Recursos\app.ico"; Comment: "Sistema Bibliotecario CUBO"
; Acceso directo del escritorio (opcional, segun el usuario)
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Recursos\app.ico"; Comment: "Sistema Bibliotecario CUBO"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

; NOTA: No hay seccion [Code] a proposito. La aplicacion se publica
; self-contained (dotnet publish --self-contained true), es decir, el
; motor .NET 10 viaja embebido junto al .exe; por lo tanto el instalador
; no necesita detectar ni descargar ninguna dependencia en la PC destino.
