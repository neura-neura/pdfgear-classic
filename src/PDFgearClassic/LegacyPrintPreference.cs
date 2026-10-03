using Microsoft.Win32;

namespace pdfgear_classic_helper;

static class LegacyPrintPreference
{
    public static void Apply(string pdfgearPath)
    {
        using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        using var preference = root.CreateSubKey(@"Software\Microsoft\Print\UnifiedPrintDialog");
        using var backup = root.CreateSubKey(@"Software\PDFgearClassic\Installer");
        if (backup.GetValue("BackupCreated") is null)
        {
            bool existed = preference.GetValue("PreferLegacyPrintDialog") is int;
            if (existed) backup.SetValue("PreviousValue", preference.GetValue("PreferLegacyPrintDialog")!, RegistryValueKind.DWord);
            backup.SetValue("PreviousValueExisted", existed ? 1 : 0, RegistryValueKind.DWord);
            backup.SetValue("BackupCreated", 1, RegistryValueKind.DWord);
        }
        // Reapply even when already set: the value alone was insufficient in the reported session.
        preference.SetValue("PreferLegacyPrintDialog", 1, RegistryValueKind.DWord);
        var applications = (preference.GetValue("PreferLegacyAppList") as string[] ?? Array.Empty<string>()).ToList();
        if (!applications.Contains(pdfgearPath, StringComparer.Ordinal))
        {
            applications.Add(pdfgearPath);
            preference.SetValue("PreferLegacyAppList", applications.ToArray(), RegistryValueKind.MultiString);
            var added = (backup.GetValue("AddedLegacyApplications") as string[] ?? Array.Empty<string>()).ToList();
            if (!added.Contains(pdfgearPath, StringComparer.Ordinal)) added.Add(pdfgearPath);
            backup.SetValue("AddedLegacyApplications", added.ToArray(), RegistryValueKind.MultiString);
        }
        preference.Flush();
    }
}
