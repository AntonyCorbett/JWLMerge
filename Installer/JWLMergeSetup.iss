#define MyAppName "JWLMerge"
#define MyAppPublisher "Antony Corbett"
#define MyAppURL "https://github.com/AntonyCorbett/JWLMerge"
#define MyAppExeName "JWLMerge.exe"

#define MyAppVersion GetFileVersion('Staging\JWLMerge.exe');

[Setup]
AppId={{53082E90-DEA3-405D-B4C8-6495076D3D98}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf}\JWLMerge
DefaultGroupName={#MyAppName}
OutputBaseFilename=JWLMergeSetup
SetupIconFile=..\JWLMerge.ico
Compression=lzma
SolidCompression=yes
AppContact=antony@corbetts.org.uk
DisableWelcomePage=false
SetupLogging=True
RestartApplications=False
CloseApplications=False
AppMutex=JWLMergeAC

PrivilegesRequired=admin

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[InstallDelete]
Type: files; Name: "{app}\*.dll"
Type: files; Name: "{app}\*.exe"
Type: files; Name: "{app}\*.json"
Type: files; Name: "{app}\*.dat"

[Files]
Source: "Staging\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.pdb"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[ThirdParty]
UseRelativePaths=True
