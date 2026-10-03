using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Forms = System.Windows.Forms;

namespace pdfgear_classic_helper;

sealed class ClassicContext : Forms.ApplicationContext
{
    readonly Forms.Timer timer = new() { Interval = 250 };
    readonly string expectedPath;
    readonly bool needsRangeGuard;
    readonly Dictionary<int, bool> processes = new();
    nint lastAttempt;
    DateTime lastAttemptTime;
    DateTime lastProcessRefresh;
    readonly string logPath;

    public ClassicContext(string pdfgearPath)
    {
        expectedPath = Path.GetFullPath(pdfgearPath);
        var version = FileVersionInfo.GetVersionInfo(expectedPath);
        needsRangeGuard = version.FileMajorPart == 2 && version.FileMinorPart == 1 && version.FileBuildPart == 20;
        var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDFgearClassic");
        Directory.CreateDirectory(logDirectory);
        logPath = Path.Combine(logDirectory, "activity.log");
        LegacyPrintPreference.Apply(expectedPath);
        timer.Tick += (_, _) => SwitchIfNeeded();
        timer.Start();
    }

    void SwitchIfNeeded()
    {
        try
        {
            nint handle = GetForegroundWindow();
            if (handle == 0) return;
            GetWindowThreadProcessId(handle, out uint rawId);
            int id = (int)rawId;
            if ((DateTime.UtcNow - lastProcessRefresh).TotalSeconds > 5)
            {
                processes.Clear();
                lastProcessRefresh = DateTime.UtcNow;
            }
            if (!processes.TryGetValue(id, out bool allowed))
            {
                using var process = Process.GetProcessById(id);
                string? runningPath = process.MainModule?.FileName;
                allowed = process.ProcessName.Equals("pdfeditor", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(runningPath, expectedPath, StringComparison.OrdinalIgnoreCase);
                if (allowed) LegacyPrintPreference.Apply(runningPath!);
                processes[id] = allowed;
            }
            if (!allowed) return;
            if (needsRangeGuard && ClassicRangeGuard.Apply(handle)) return;
            if (handle == lastAttempt && (DateTime.UtcNow-lastAttemptTime).TotalSeconds < 5) return;
            var root = AutomationElement.FromHandle(handle);
            if (root.Current.Name != "PDFgear" || !root.Current.IsEnabled) return;
            var printer = root.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "cboxPrinterList"));
            if (printer == null) return;
            var classic = root.FindFirst(TreeScope.Descendants,
                new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "ClassicModeBtn"),
                    new PropertyCondition(AutomationElement.NameProperty, "Classic Mode")));
            if (classic == null || classic.Current.IsOffscreen || !classic.Current.IsEnabled) return;
            var bounds = classic.Current.BoundingRectangle;
            if (bounds.IsEmpty || bounds.Width < 1 || bounds.Height < 1 || GetForegroundWindow()!=handle) return;
            LegacyPrintPreference.Apply(expectedPath);
            lastAttempt = handle;
            lastAttemptTime = DateTime.UtcNow;
            if (classic.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
                ((InvokePattern)pattern).Invoke();
            else
            {
                if (!GetCursorPos(out var oldPoint)) return;
                if (!SetCursorPos((int)(bounds.Left+bounds.Width/2), (int)(bounds.Top+bounds.Height/2))) return;
                var input = new INPUT[2];
                input[0].Type=0; input[0].Data.Mouse.Flags=0x0002;
                input[1].Type=0; input[1].Data.Mouse.Flags=0x0004;
                bool clicked = GetForegroundWindow()==handle && SendInput(2, input, Marshal.SizeOf<INPUT>()) == 2;
                SetCursorPos(oldPoint.X, oldPoint.Y);
                if (!clicked) return;
            }
            if (File.Exists(logPath) && new FileInfo(logPath).Length>32768) File.WriteAllText(logPath, "");
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Classic Mode selected\n");
        }
        catch (Exception error)
        {
            Debug.WriteLine(error.Message);
        }
    }

    [StructLayout(LayoutKind.Sequential)] struct POINT { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint Type; public INPUTUNION Data; }
    [StructLayout(LayoutKind.Explicit)] struct INPUTUNION
    {
        [FieldOffset(0)] public MOUSEINPUT Mouse;
    }
    [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT
    {
        public int X,Y; public uint MouseData,Flags,Time; public nuint ExtraInfo;
    }
    [DllImport("user32.dll")] static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(nint hwnd,out uint processId);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT point);
    [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
    [DllImport("user32.dll")] static extern uint SendInput(uint count,INPUT[] input,int size);
}
