[Setup]
AppName=GitHubView
AppVersion=1.0
DefaultDirName={pf}\GitHubView
VersionInfoCopyright=Copyright (c) 鸭鸭「カモ」
AppPublisher=鸭鸭「カモ」
AppPublisherURL=https://duckduckstudio.github.io/yazicbs.github.io/
OutputDir=.
OutputBaseFilename=GitHubView-installer

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Icons]
Name: "{commonstartmenu}\Programs\GitHubView"; Filename: "{app}\ghv.exe"; WorkingDir: "{app}"

[Files]
Source: "..\output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "{sys}\cmd.exe"; Parameters: "/C setx PATH ""{app};%PATH%"" /M"; Flags: runhidden

[UninstallRun]
Filename: "{sys}\cmd.exe"; Parameters: "/C setx PATH ""%PATH:{app};=%"" /M"; Flags: runhidden; RunOnceId: UninstallSetPath
