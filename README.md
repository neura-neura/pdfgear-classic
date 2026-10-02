# PDFgear Classic

A small Windows background helper that automatically selects **Classic Mode** when you open PDFgear's print window.

Use it if Classic Mode works better with your printer and you want to skip selecting it manually. PDFgear's regular print window may appear briefly before the helper switches to Classic Mode. You still choose your printer, pages, and preferences, and press the final **Print** button yourself.

## Requirements

- Windows with PDFgear installed.
- .NET 10 SDK to build the project.
- .NET 10 Desktop Runtime to run a framework-dependent build.

The original helper was observed switching successfully with PDFgear 2.1.20 on Windows 11 in English. The user also confirmed that printing through Classic Mode resolved a margin problem with a Canon G3010. This is a workaround for selecting a print mode; it does not guarantee a fix for every printer or document. The configurable version in this repository has not been separately validated across installations or languages.

## Build and run

From the repository root, run these commands in PowerShell:

```powershell
dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained false -o .\publish
& .\publish\PDFgearClassic.exe 'C:\Program Files\PDFgear\pdfeditor.exe'
```

Replace the argument with the actual path to your installation's `pdfeditor.exe`. Without an argument, the helper looks in the `PDFgear` folder under Windows' Program Files directories. For a custom installation, always supply the path.

The helper has no main window. Open a PDF in PDFgear and select **Print** or press **Ctrl+P**. It switches the regular print window to Classic Mode automatically.

To produce a build that includes the .NET runtime:

```powershell
dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained true -o .\publish
```

## Start with Windows

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

Reopen PDFgear afterward. This optional Windows setting can affect other applications that use the same print dialog. The helper itself does not change it. To undo it, restore the previous value, or remove `PreferLegacyPrintDialog` if it did not exist before.

## How it works

The helper checks the foreground window and verifies that its process matches the configured PDFgear executable. It recognizes PDFgear's regular print window using its printer control, then finds **Classic Mode** through Windows UI Automation. It invokes that control when possible; otherwise, it clicks its reported position and restores the pointer.

- It only acts on the configured PDFgear installation.
- It does not press the final Print button or submit print jobs.
- It does not edit PDFs, patch PDFgear, change printer settings, or access the network.
- It runs one instance per Windows session.

Local activity entries are written to `%LOCALAPPDATA%\PDFgearClassic\activity.log`. They contain timestamps and the helper's switch action, without document names or contents. An entry records that the action was issued, not proof that a print job succeeded.

## Limitations and stopping

The normal print window can flash briefly. Changes to PDFgear's interface or translated labels may prevent detection. Printer preferences and page ranges still belong to the resulting print dialog; select them there before printing.

To stop the helper, end **PDFgearClassic.exe** in Task Manager. To disable automatic startup, remove **PDFgear Classic.lnk** from your Startup folder. After stopping it, you can remove its installed folder. This does not uninstall PDFgear or revert any separately configured Windows or printer preferences.

This project is independent of PDFgear and Canon. Source code is provided under the MIT license.
