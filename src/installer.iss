[Setup]
AppName=TsRemux
AppVerName=TsRemux 0.23.1
DefaultDirName={pf}\TsRemux
DefaultGroupName=TsRemux
UninstallDisplayIcon={app}\TsRemux.exe
Compression=lzma
SolidCompression=yes
OutputDir=install

[Files]
Source: "bin\Release\TsRemux.exe"; DestDir: "{app}"
Source: "bin\Release\mplayer.exe"; DestDir: "{app}"
Source: "bin\Release\TsRemux.exe.config"; DestDir: "{app}"
Source: "TsRemux_0.23.1_src.7z"; DestDir: "{app}"
Source: "license.txt"; DestDir: "{app}"

[Icons]
Name: "{group}\TsRemux"; Filename: "{app}\TsRemux.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall TsRemux"; Filename: "{uninstallexe}"



