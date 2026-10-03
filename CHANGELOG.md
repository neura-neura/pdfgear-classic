# Changelog

## 1.1.4

- Remove the control workaround from 1.1.3. Windows' extended dialog still allowed zero page ranges for the reported one-page document, producing a validation error when Print was pressed.
- Add a reversible local code repair for a specific PDFgear 2.1.20 executable, verified by SHA-256. Other builds are left unchanged.
- Make Classic Mode use the older Windows print dialog and normalize AllPages to a full SomePages range before PDFgear's existing print-range check. Explicit page ranges are preserved.
- Save the untouched original executable and a checksum record alongside it. Add manual restoration and an uninstall restoration attempt. A running application or insufficient permissions can prevent restoration; the backup remains.
- The modified executable loses its vendor signature. PDF files are not modified, and no PDFgear binaries are distributed in this release.
- The repaired application opened through its normal launcher and displayed the older dialog with All enabled for a one-page PDF. Its patched code was inspected. No print jobs were submitted, so physical printing remains unconfirmed.

## 1.1.3

- Work around PDFgear 2.1.20's classic printing crash when the Windows dialog returns AllPages or CurrentPage. The application throws an unhandled ArgumentNullException because its print code requires SomePages.
- Keep Pages selected in the native classic dialog. For a one-page document, enable the otherwise disabled Pages choice and populate the range with 1. For multiple pages, preserve the range supplied by PDFgear. All and Current Page are disabled while this workaround is active; enter the desired range in Pages.
- Limit the range workaround to PDFgear 2.1.20. Other versions retain their page selection behavior.
- Register the exact path reported by a running PDFgear process in addition to the detected installation path, preserving distinct path spellings. This is a defensive change; path spelling has not been established as the cause of the modern dialog returning.
- A one-page PDF showed Pages: 1 and an eight-page PDF showed Pages: 1-8. No print jobs were submitted, so successful physical printing remains unconfirmed.
- The previous observation that the classic dialog appeared after reopening did not establish that version 1.1.2 fixed every launch or the printing crash. The user subsequently reported both failures.

## 1.1.2

- Register the detected PDFgear executable in Windows' application list for the traditional print dialog. The global preference used in 1.1.1 was insufficient after reopening PDFgear on the reported setup.
- Preserve other application entries and track entries added by the helper so uninstall can remove only those entries.
- The traditional dialog was observed after two complete PDFgear close/reopen cycles with the application entry present. No print jobs were submitted.
- Restart PDFgear once after updating if it is already open. This behavior was observed with PDFgear 2.1.20 in English on the original Windows 11 setup; other installations and languages have not been checked.

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
