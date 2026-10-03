# PDFgear Classic

A small Windows background helper that automatically selects **Classic Mode** when you open PDFgear's print window.

Use it if Classic Mode works better with your printer and you want to skip selecting it manually. PDFgear's regular print window may appear briefly before the helper switches to Classic Mode. You still choose your printer, pages, and preferences, and press the final **Print** button yourself.

## Install

Download **PDFgearClassic-Setup-1.1.3-win-x64.exe** from [Releases](https://github.com/neura-neura/pdfgear-classic/releases/latest), then run the installer. PDFgear must already be installed. The installer is for Windows 10/11 on x64-compatible systems and installs for your current user.

Setup starts the helper immediately and adds automatic startup when you sign in to Windows. It includes the .NET runtime. The helper detects PDFgear from its running process, Windows installation records, or common installation folders, including installations on another drive registered by PDFgear's installer.

The helper registers the detected PDFgear executable in Windows' application list for the traditional print dialog. It preserves other entries and removes only entries it added when you uninstall. Setup also enables the traditional Windows print dialog for your user. This may affect other applications using the same Windows dialog. It saves the previous value and restores it on uninstall if that value is still the one set by Setup. Your printer preferences remain under your control.

The installer is unsigned. This repository does not include a code-signing certificate.

## PDFgear 2.1.20: page range workaround

PDFgear 2.1.20 crashes in its classic print path if Windows returns AllPages or CurrentPage. Version 1.1.3 keeps **Pages** selected instead. For a one-page document it enables that choice and fills in **1**; for multiple pages it preserves the range supplied by PDFgear. **All** and **Current Page** are disabled while this workaround is active. Enter a page or range such as **1**, **2-4**, or **1-8** in Pages before printing.

The corrected dialog was observed with one-page and eight-page documents. No print jobs were submitted during this repair, so physical printing has not been confirmed. The user reported that the Windows modern dialog still returned after updating to 1.1.2. Version 1.1.3 also records the exact running executable path, but that change has not been established as a fix for every launch.

## Build requirements

- Windows with PDFgear installed.
- .NET 10 SDK to build the project.
- .NET 10 Desktop Runtime to run a framework-dependent build; the release installer includes the runtime.
- Inno Setup 6 to build the installer.

The original helper was observed switching successfully with PDFgear 2.1.20 on Windows 11 in English. The user also confirmed that printing through Classic Mode resolved a margin problem with a Canon G3010. This is a workaround for selecting a print mode; it does not guarantee a fix for every printer or document. The release installer and automatic installation discovery have not been validated across installations or languages.

## Build and run

From the repository root, run these commands in PowerShell:

```powershell
dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained false -o .\publish
& .\publish\PDFgearClassic.exe 'C:\Program Files\PDFgear\pdfeditor.exe'
```

Replace the argument with the actual path to your installation's `pdfeditor.exe`. Without an argument, the helper detects PDFgear through its running process, Windows installation records, or common installation folders. You can supply the path to override detection.

The helper has no main window. Open a PDF in PDFgear and select **Print** or press **Ctrl+P**. It switches the regular print window to Classic Mode automatically.

To produce a build that includes the .NET runtime:

```powershell
dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained true -o .\publish
```

## Start with Windows

The release installer handles startup automatically. The following instructions are for manual builds.

Keep the published files together in a permanent folder. The following example copies them to your user profile and adds a startup shortcut:

```powershell
$installDir = Join-Path $env:LOCALAPPDATA 'PDFgearClassic'
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Copy-Item -Path .\publish\* -Destination $installDir -Force

$startupDir = [Environment]::GetFolderPath('Startup')
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut((Join-Path $startupDir 'PDFgear Classic.lnk'))
$shortcut.TargetPath = Join-Path $installDir 'PDFgearClassic.exe'
$shortcut.Arguments = '"C:\Program Files\PDFgear\pdfeditor.exe"'
$shortcut.WorkingDirectory = $installDir
$shortcut.Save()

Start-Process -FilePath $shortcut.TargetPath -ArgumentList $shortcut.Arguments
```

Update `Arguments` if PDFgear is installed elsewhere. Run the helper at the same privilege level as PDFgear; administrator privileges are normally unnecessary.

## Windows 11: traditional print dialog

Classic Mode may open Windows 11's modern print dialog. On the original setup, that dialog caused PDFgear to close with a `PrinterSettings.PrintRange` error before a job reached the printer queue. Choosing the traditional Windows dialog resolved that particular failure.

If you encounter the same problem, first record the existing value, then opt into the traditional dialog for your Windows user:

```powershell
$key = 'HKCU:\Software\Microsoft\Print\UnifiedPrintDialog'
Get-ItemProperty -Path $key -Name PreferLegacyPrintDialog -ErrorAction SilentlyContinue
New-Item -Path $key -Force | Out-Null
New-ItemProperty -Path $key -Name PreferLegacyPrintDialog -PropertyType DWord -Value 1 -Force | Out-Null
```

Reopen PDFgear afterward. This Windows setting can affect other applications that use the same print dialog. The installer and helper set it automatically. The helper reapplies it at startup and before selecting Classic Mode. Starting with 1.1.2, it also adds PDFgear's full executable path to the REG_MULTI_SZ value PreferLegacyAppList in the same registry key. The earlier global preference alone did not survive reopening PDFgear on the reported setup; the application entry produced the traditional dialog after two complete close/reopen cycles. Windows supports this application list as documented by [Appeon](https://docs.appeon.com/pb2025r2/troubleshooting_guide/tr_1455.html). Restart PDFgear once after updating if it is already open. To undo a manual change, restore the previous value, or remove `PreferLegacyPrintDialog` if it did not exist before.

## Build the installer

With the .NET 10 SDK and Inno Setup 6 installed, run:

```powershell
.\build-installer.ps1
```

If the compiler is installed in a custom location, pass `-InnoCompiler 'C:\path\to\ISCC.exe'`. The script publishes a self-contained x64 build and creates the installer in `dist`.

## How it works

The helper checks the foreground window and verifies that its process matches the configured PDFgear executable. It recognizes PDFgear's regular print window using its printer control, then finds **Classic Mode** through Windows UI Automation. It invokes that control when possible; otherwise, it clicks its reported position and restores the pointer.

- It only acts on the configured PDFgear installation.
- It does not press the final Print button or submit print jobs. With PDFgear 2.1.20 it also selects a compatible page range in the native classic dialog.
- The helper does not edit PDFs, patch PDFgear, change printer settings, or access the network. It reapplies the traditional Windows print dialog preference for your user. Setup also configures automatic startup.
- It runs one instance per Windows session.

Local activity entries are written to `%LOCALAPPDATA%\PDFgearClassic\activity.log`. They contain timestamps and the helper's switch action, without document names or contents. An entry records that the action was issued, not proof that a print job succeeded.

## Limitations and stopping

The normal print window can flash briefly. Changes to PDFgear's interface or translated labels may prevent detection. Printer preferences and page ranges still belong to the resulting print dialog; select them there before printing.

To stop the helper, end **PDFgearClassic.exe** in Task Manager or run `PDFgearClassic.exe --stop` from its installed folder. For an installation made with the release installer, uninstall **PDFgear Classic** from Windows' installed apps list. This stops the helper, removes its startup registration, and restores the Windows print dialog setting it saved, provided the value has not changed since installation. It does not uninstall PDFgear or alter printer preferences.

For a manual installation, remove **PDFgear Classic.lnk** from your Startup folder and remove the installed folder after stopping the helper. Revert any Windows print dialog change separately.

This project is independent of PDFgear and Canon. Source code is provided under the MIT license.

