#define AppVersion "1.1.0"

[Setup]
AppId={{56C66D10-E1EA-4A1C-B069-812EF5A67425}
AppName=PDFgear Classic
AppVersion={#AppVersion}
AppPublisher=neura-neura
AppPublisherURL=https://github.com/neura-neura/pdfgear-classic
AppSupportURL=https://github.com/neura-neura/pdfgear-classic/issues
DefaultDirName={localappdata}\PDFgearClassic
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=yes
LicenseFile=..\LICENSE
InfoBeforeFile=install-info.txt
OutputDir=..\dist
OutputBaseFilename=PDFgearClassic-Setup-{#AppVersion}-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=force
CloseApplicationsFilter=PDFgearClassic.exe,PDFgearClassic.dll
RestartApplications=no
UninstallDisplayIcon={app}\PDFgearClassic.exe

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PDFgearClassic"; ValueData: """{app}\PDFgearClassic.exe"""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Microsoft\Print\UnifiedPrintDialog"; ValueType: dword; ValueName: "PreferLegacyPrintDialog"; ValueData: "1"

[Icons]
Name: "{userprograms}\PDFgear Classic"; Filename: "{app}\PDFgearClassic.exe"
Name: "{userprograms}\Uninstall PDFgear Classic"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\PDFgearClassic.exe"; Description: "Start PDFgear Classic"; Flags: nowait runhidden

[UninstallRun]
Filename: "{app}\PDFgearClassic.exe"; Parameters: "--stop"; Flags: runhidden waituntilterminated; RunOnceId: "StopPDFgearClassic"

[Code]
const
  BackupKey = 'Software\PDFgearClassic\Installer';
  PrintKey = 'Software\Microsoft\Print\UnifiedPrintDialog';

procedure CurStepChanged(CurStep: TSetupStep);
var
  Value: Cardinal;
  Exists: Boolean;
begin
  if (CurStep = ssInstall) and not RegValueExists(HKCU, BackupKey, 'BackupCreated') then
  begin
    Exists := RegQueryDWordValue(HKCU, PrintKey, 'PreferLegacyPrintDialog', Value);
    if Exists then
    begin
      RegWriteDWordValue(HKCU, BackupKey, 'PreviousValue', Value);
      RegWriteDWordValue(HKCU, BackupKey, 'PreviousValueExisted', 1);
    end
    else
      RegWriteDWordValue(HKCU, BackupKey, 'PreviousValueExisted', 0);
    RegWriteDWordValue(HKCU, BackupKey, 'BackupCreated', 1);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Value, Previous, Existed: Cardinal;
begin
  if CurUninstallStep = usUninstall then
  begin
    if RegQueryDWordValue(HKCU, PrintKey, 'PreferLegacyPrintDialog', Value) and (Value = 1) then
    begin
      if RegQueryDWordValue(HKCU, BackupKey, 'PreviousValueExisted', Existed) then
      begin
        if Existed = 1 then
        begin
          if RegQueryDWordValue(HKCU, BackupKey, 'PreviousValue', Previous) then
            RegWriteDWordValue(HKCU, PrintKey, 'PreferLegacyPrintDialog', Previous);
        end
        else
          RegDeleteValue(HKCU, PrintKey, 'PreferLegacyPrintDialog');
      end;
    end;
    RegDeleteKeyIncludingSubkeys(HKCU, BackupKey);
  end;
end;
