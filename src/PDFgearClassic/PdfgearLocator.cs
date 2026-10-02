using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace pdfgear_classic_helper;

static class PdfgearLocator
{
    public static string? Find()
    {
        foreach (var process in Process.GetProcessesByName("pdfeditor"))
        {
            using (process)
            {
                try
                {
                    string? path = process.MainModule?.FileName;
                    if (IsValid(path)) return path;
                }
                catch (Exception) { }
            }
        }
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var root = RegistryKey.OpenBaseKey(hive, view);
                using var appPath = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\pdfeditor.exe");
                string? path = (appPath?.GetValue(null) as string)?.Trim('"');
                if (IsValid(path)) return path;
                using var uninstall = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstall is null) continue;
                foreach (var name in uninstall.GetSubKeyNames())
                {
                    using var entry = uninstall.OpenSubKey(name);
                    if (entry is null || (entry.GetValue("DisplayName") as string)?.StartsWith("PDFgear", StringComparison.OrdinalIgnoreCase) != true) continue;
                    if (entry.GetValue("InstallLocation") is not string directory || string.IsNullOrWhiteSpace(directory)) continue;
                    path = Path.Combine(directory.Trim('"'), "pdfeditor.exe");
                    if (IsValid(path)) return path;
                }
            }
            catch (Exception) { }
        }
        foreach (var directory in new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
        })
        {
            string path = Path.Combine(directory, "PDFgear", "pdfeditor.exe");
            if (IsValid(path)) return path;
        }
        return null;
    }

    public static bool IsValid(string? path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path)
        && Path.GetFileName(path).Equals("pdfeditor.exe", StringComparison.OrdinalIgnoreCase);
}
