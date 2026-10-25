; Inno Setup 6 — packs artifacts\publish into a normal Windows installer.
#ifndef AppVersion
  #define AppVersion "0.3.1"
#endif

#define MyAppName "Simplest Calendar"
#define MyAppExeName "SimplestCalendar.exe"
#define PublishDir "..\artifacts\publish"

[Setup]
AppId={{A7C3E9F1-5B2D-4E8A-9C1F-6D4B0A2E8F31}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher=Simplest Calendar
DefaultDirName={autopf}\SimplestCalendar
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=SimplestCalendar-Setup-{#AppVersion}
SetupIconFile=..\src\SimplestCalendar\Assets\AppIcon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
