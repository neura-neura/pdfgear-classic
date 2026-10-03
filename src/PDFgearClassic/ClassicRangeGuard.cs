using System.Runtime.InteropServices;
using System.Text;

namespace pdfgear_classic_helper;

// PDFgear 2.1.20's OnBeginPrint rejects every PrintRange except SomePages.
// The Windows dialog disables that choice for a one-page document and returns AllPages.
static class ClassicRangeGuard
{
    public static bool Apply(nint dialog)
    {
        var pages = FindControl(dialog, 1059);
        var range = FindControl(dialog, 1152);
        var all = FindControl(dialog, 1056);
        var current = FindControl(dialog, 1058);
        // Identify the native print dialog through its page range and printer controls.
        if (pages == 0 || range == 0 || all == 0 || FindControl(dialog, 1005) == 0
            || FindControl(dialog, 1010) == 0) return false;
        bool selected = SendMessage(pages, 0x00F0, 0, 0) == 1; // BM_GETCHECK
        if (!selected)
        {
            var text = new StringBuilder(256);
            SendMessage(range, 0x000D, text.Capacity, text); // WM_GETTEXT across processes
            bool singlePage = !IsWindowEnabled(pages);
            // Do not invent an all-document range for a multipage document.
            if (text.Length == 0 && !singlePage) return false;
            EnableWindow(pages, true);
            EnableWindow(range, true);
            SendMessage(pages, 0x00F5, 0, 0); // BM_CLICK: select Pages and notify the dialog
            selected = SendMessage(pages, 0x00F0, 0, 0) == 1;
            // Selecting Pages clears the previously disabled edit on Windows 11.
            if (selected && singlePage) SendMessage(range, 0x000C, 0, text.Length == 0 ? "1" : text.ToString());
        }
        if (selected)
        {
            EnableWindow(all, false);
            if (current != 0) EnableWindow(current, false);
        }
        return selected;
    }

    static nint FindControl(nint dialog, int id)
    {
        nint found = 0;
        EnumChildWindows(dialog, (child, _) =>
        {
            if (GetDlgCtrlID(child) != id) return true;
            found = child;
            return false;
        }, 0);
        return found;
    }
    delegate bool EnumChildCallback(nint window, nint parameter);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(nint window, EnumChildCallback callback, nint parameter);
    [DllImport("user32.dll")] static extern int GetDlgCtrlID(nint window);
    [DllImport("user32.dll")] static extern bool IsWindowEnabled(nint window);
    [DllImport("user32.dll")] static extern bool EnableWindow(nint window, bool enabled);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern nint SendMessage(nint window, uint message, nint wparam, StringBuilder text);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern nint SendMessage(nint window, uint message, nint wparam, string text);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern nint SendMessage(nint window, uint message, nint wparam, nint lparam);
}

