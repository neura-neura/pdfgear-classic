using System.IO;
using System.Diagnostics;
using Forms = System.Windows.Forms;

namespace pdfgear_classic_helper;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--stop")
        {
            StopInstalledInstances();
            return;
        }
        using var mutex = new Mutex(true, @"Local\PDFgearClassic", out bool created);
        if (!created) return;
        Forms.Application.SetHighDpiMode(Forms.HighDpiMode.PerMonitorV2);

        string? path = args.Length == 1 ? args[0] : PdfgearLocator.Find();
        if (args.Length > 1 || !PdfgearLocator.IsValid(path))
        {
            Forms.MessageBox.Show(
                "PDFgear could not be found. Install PDFgear and start this helper again, or pass the full path to pdfeditor.exe.\n\nExample:\nPDFgearClassic.exe \"C:\\Program Files\\PDFgear\\pdfeditor.exe\"",
                "PDFgear Classic", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            return;
        }
        Forms.Application.Run(new ClassicContext(path!));
    }

    static void StopInstalledInstances()
    {
        foreach (var process in Process.GetProcessesByName("PDFgearClassic"))
        {
            using (process)
            {
                if (process.Id == Environment.ProcessId) continue;
                try
                {
                    if (string.Equals(process.MainModule?.FileName, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
