# Changelog

## 1.1.0

- Added a Windows installer that starts the helper immediately and on sign-in.
- Bundled the .NET runtime so a separate runtime installation is unnecessary.
- Added PDFgear installation discovery through running processes, Windows installation records, and common folders.
- Enabled the traditional Windows print dialog during setup, with a saved value for restoration during uninstall.
- Added an uninstall entry and a `--stop` command for the installed helper.
- Added a reproducible installer build script.

The installer and helper build successfully. The installer has not been run as an installation/uninstallation test. No print jobs were submitted while preparing this release. Automatic switching was previously observed with the original helper on PDFgear 2.1.20 in English on Windows 11.
