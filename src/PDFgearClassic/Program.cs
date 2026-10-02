using System.IO;
using Forms = System.Windows.Forms;

namespace pdfgear_classic_helper;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, @"Local\PDFgearClassic", out bool created);
        if (!created) return;
        Forms.Application.SetHighDpiMode(Forms.HighDpiMode.PerMonitorV2);

        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PDFgear", "pdfeditor.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PDFgear", "pdfeditor.exe")
        };
        string? path = args.Length == 1 ? args[0] : candidates.FirstOrDefault(File.Exists);
        if (args.Length > 1 || string.IsNullOrWhiteSpace(path) || !File.Exists(path)
            || !Path.GetFileName(path).Equals("pdfeditor.exe", StringComparison.OrdinalIgnoreCase))
        {
            Forms.MessageBox.Show(
                "Pass the full path to PDFgear's pdfeditor.exe as the only argument.\n\nExample:\nPDFgearClassic.exe \"C:\\Program Files\\PDFgear\\pdfeditor.exe\"",
                "PDFgear Classic", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            return;
        }
        Forms.Application.Run(new ClassicContext(path));
    }
}
