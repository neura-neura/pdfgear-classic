# Changelog

## 1.1.1

- Reapply the traditional Windows print dialog preference when the helper starts and before selecting Classic Mode.
- Preserve the previous preference using the installer's existing backup for uninstall restoration.
- A reported session opened the modern dialog despite the saved value being `1`. Reapplying the value and reopening PDFgear restored the traditional dialog on that setup. The underlying Windows caching behavior has not been isolated.
- Restart PDFgear once after updating if it is already open. No print jobs were submitted during the repair.

## 1.1.0

- Added a Windows installer that starts the helper immediately and on sign-in.
- Bundled the .NET runtime so a separate runtime installation is unnecessary.
- Added PDFgear installation discovery through running processes, Windows installation records, and common folders.
- Enabled the traditional Windows print dialog during setup, with a saved value for restoration during uninstall.
- Added an uninstall entry and a `--stop` command for the installed helper.
- Added a reproducible installer build script.

The installer and helper build successfully. The installer has not been run as an installation/uninstallation test. No print jobs were submitted while preparing this release. Automatic switching was previously observed with the original helper on PDFgear 2.1.20 in English on Windows 11.
