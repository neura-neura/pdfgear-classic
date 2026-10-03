# PDFgear Classic

A Windows background helper that selects **Classic Mode** in PDFgear. Version 1.1.4 also applies a reversible local repair to one supported PDFgear 2.1.20 executable to address its classic printing failures.

## Install

Close PDFgear, download **PDFgearClassic-Setup-1.1.4-win-x64.exe** from [Releases](https://github.com/neura-neura/pdfgear-classic/releases/latest), and run the installer. Setup includes the .NET runtime and starts the helper immediately and when you sign in to Windows.

PDFgear must already be installed. The helper detects running processes, Windows installation records, and common installation folders. The installer supports Windows 10/11 on x64-compatible systems and installs the helper for the current user.

The local repair requires write access to PDFgear's installation folder. For a protected folder such as Program Files, run the installer with administrator rights under the same Windows account. If PDFgear is open, close it and leave it closed for at least two seconds so the helper can apply the pending repair. Repair failures are recorded in the local activity log.

The installer is unsigned. The repaired PDFgear executable also loses its original vendor signature because its code changes. An untouched copy is retained next to it.

## What changed in 1.1.4

Version 1.1.3 enabled a page-range control in Windows' extended print dialog for a one-page PDF. Windows still allowed zero page ranges, so pressing Print produced a validation error. That control workaround has been removed.

For the supported executable, version 1.1.4 makes two changes:

- PDFgear's Classic Mode uses the older Windows print dialog, which does not use the extended dialog's array of page ranges.
- Before printing, **All** is converted internally to the document's full page range, satisfying PDFgear's print code. **Pages** ranges are left intact.

You still choose the printer, copies, and pages and press the final **OK** button yourself. The helper never submits a print job. On a one-page document, **All** is available; the page-range fields may be disabled normally.

The supported original executable has SHA-256:

```text
EF1843A316478BD313DCD8630E3F019D14BFFB1B92675E62E820E9DA84E3850E
```

Other builds are left unchanged. A PDFgear update may replace the repaired executable or require a different repair. This project does not distribute PDFgear binaries.

## Observations and limits

The repaired application opened through PDFgear's normal file launcher. Its Classic Mode displayed the older dialog with **All** enabled for a one-page document. The repaired code was inspected to confirm the dialog choice and full-range conversion. No print jobs were submitted during this repair, so physical printing remains unconfirmed.

Previous observations that a dialog opened successfully did not establish a complete fix. The user reported subsequent failures in versions 1.1.2 and 1.1.3. Release notes retain those limitations.

## Backup and restore

The repair creates these files next to PDFgear's executable:

- `pdfeditor.exe.PDFgearClassic-original`: untouched original executable.
- `pdfeditor.exe.PDFgearClassic-repair.json`: checksums used to identify the repaired file and its backup.

Close PDFgear before restoring. Stop the helper and run:

```powershell
& "$env:LOCALAPPDATA\PDFgearClassic\PDFgearClassic.exe" --stop
& "$env:LOCALAPPDATA\PDFgearClassic\PDFgearClassic.exe" --restore-pdfgear
```

Restoration only proceeds when both checksums match the saved record. It does not overwrite a different PDFgear update. The original backup is retained.

Uninstalling PDFgear Classic stops the helper, removes its startup registration, and attempts to restore the original executable. Close PDFgear before uninstalling. If the file is open or permissions prevent restoration, the backup remains and the uninstaller reports the problem. You can restore the backup manually after closing PDFgear.

## Background behavior

The helper checks the foreground process against the detected PDFgear executable and selects Classic Mode using Windows UI Automation. PDFgear's own print window may appear briefly. Only the configured PDFgear installation is controlled. The helper does not edit PDF documents or change printer preferences.

It also retains the previous versions' Windows legacy print preferences, including an application list entry and a user-wide preference. The latter can affect other applications using Windows' unified print dialog. Setup saves the previous value and restores it on uninstall if it has not changed; application entries added by the helper are removed individually.

One helper instance runs per session. Local activity entries are written to `%LOCALAPPDATA%\PDFgearClassic\activity.log` without document names or contents. They record actions and repair status, not successful printing. The helper does not access the network.

## Build

Requirements: Windows, .NET 10 SDK, and Inno Setup 6. Restore uses NuGet for Mono.Cecil, which is included with its MIT license.

```powershell
.\build-installer.ps1
```

The script publishes a self-contained x64 build and creates the installer in `dist`. Pass `-InnoCompiler 'C:\path\to\ISCC.exe'` for a custom compiler location.

For a manual build:

```powershell
dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained true -o .\publish
& .\publish\PDFgearClassic.exe 'C:\Program Files\PDFgear\pdfeditor.exe'
```

Replace the argument with the actual installation path. Without an argument, the helper detects PDFgear. Keep the published files together. Manual builds do not register automatic startup.

This project is independent of PDFgear and printer manufacturers. Its own source code is available under the MIT license; PDFgear retains its own license.
