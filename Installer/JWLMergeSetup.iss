; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "JWLMerge"
#define MyAppPublisher "Antony Corbett"
#define MyAppURL "https://github.com/AntonyCorbett/JWLMerge"
#define MyAppExeName "JWLMerge.exe"

#define MyAppVersion GetFileVersion('..\JWLMerge\bin\Release\net5.0-windows\publish\JWLMerge.exe');

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{53082E90-DEA3-405D-B4C8-6495076D3D98}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\JWLMerge
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

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[InstallDelete]
; files from pre-net-5 edition
Type: files; Name: "{app}\JWLMerge.exe.config"
Type: files; Name: "{app}\JWLMergeCLI.exe.config"
Type: files; Name: "{app}\Serilog.Settings.AppSettings.dll"
Type: files; Name: "{app}\SQLite.Interop.dll"
Type: files; Name: "{app}\System.Data.SQLite.dll"
Type: files; Name: "{app}\System.Windows.Interactivity.dll"
Type: files; Name: "{app}\GalaSoft.MvvmLight.dll"
Type: files; Name: "{app}\GalaSoft.MvvmLight.Extras.dll"
Type: files; Name: "{app}\GalaSoft.MvvmLight.Platform.dll"
Type: files; Name: "{app}\CommonServiceLocator.dll"

; from abortive net-5 pre-release
Type: filesandordirs; Name: "{app}\ref"
Type: filesandordirs; Name: "{app}\runtimes"

[Files]
Source: "..\JWLMerge\bin\Release\net5.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.pdb"
Source: "..\JWLMergeCLI\bin\Release\net5.0\publish\JWLMergeCLI.*"; DestDir: "{app}"; Flags: ignoreversion; Excludes: "*.pdb"
Source: "..\JWLMergeCLI\bin\Release\net5.0\publish\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}"; Flags: ignoreversion;

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[ThirdParty]
UseRelativePaths=True

