using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace pdfgear_classic_helper;

static class PdfgearRepair
{
    const string SupportedHash = "EF1843A316478BD313DCD8630E3F019D14BFFB1B92675E62E820E9DA84E3850E";
    record RepairRecord(string OriginalHash, string PatchedHash);

    public static string Apply(string path)
    {
        string current = Hash(path);
        string backup = path + ".PDFgearClassic-original";
        string manifest = path + ".PDFgearClassic-repair.json";
        if (File.Exists(manifest))
        {
            var record = JsonSerializer.Deserialize<RepairRecord>(File.ReadAllText(manifest));
            if (record?.PatchedHash == current) return "Repair already installed";
        }
        if (current != SupportedHash) return "Executable is not the supported PDFgear 2.1.20 build; left unchanged";
        if (IsRunning(path)) return "Close PDFgear to apply the repair";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(path)!);
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        resolver.AddSearchDirectory(Path.Combine(windows, "Microsoft.NET", "Framework", "v4.0.30319"));
        resolver.AddSearchDirectory(Path.Combine(windows, "Microsoft.NET", "Framework64", "v4.0.30319"));
        using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { InMemory = true, AssemblyResolver = resolver });
        var dialog = assembly.MainModule.GetType("pdfeditor.Controls.Printer.WinPrinterDialog");
        var print = assembly.MainModule.GetType("pdfeditor.Utils.Print.PdfPrintDocument");
        if (dialog is null || print is null) throw new InvalidDataException("Expected printing classes were not found.");
        var click = dialog.Methods.Single(m => m.Name == "ClassicModeBtn_Click");
        var useEx = click.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "set_UseEXDialog");
        if (useEx.Previous.OpCode != OpCodes.Ldc_I4_1) throw new InvalidDataException("Unexpected print dialog initialization.");
        useEx.Previous.OpCode = OpCodes.Ldc_I4_0;

        var constructor = print.Methods.Single(m => m.IsConstructor && m.Parameters.Count == 3);
        var begin = print.Methods.Single(m => m.Name == "OnBeginPrint");
        MethodReference Reference(string name) => constructor.Body.Instructions
            .Select(i => i.Operand).OfType<MethodReference>().First(m => m.Name == name);
        var settings = Reference("get_PrinterSettings");
        var setFrom = Reference("set_FromPage");
        var setTo = Reference("set_ToPage");
        var max = Reference("get_MaximumPage");
        var setRange = Reference("set_PrintRange");
        var getRange = begin.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().First(m => m.Name == "get_PrintRange");
        var first = begin.Body.Instructions[0];
        var il = begin.Body.GetILProcessor();
        var normalize = new[]
        {
            il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Call, settings), il.Create(OpCodes.Callvirt, getRange),
            il.Create(OpCodes.Ldc_I4_0), il.Create(OpCodes.Bne_Un, first),
            il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Call, settings), il.Create(OpCodes.Ldc_I4_1), il.Create(OpCodes.Callvirt, setFrom),
            il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Call, settings),
            il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Call, settings), il.Create(OpCodes.Callvirt, max), il.Create(OpCodes.Callvirt, setTo),
            il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Call, settings), il.Create(OpCodes.Ldc_I4_2), il.Create(OpCodes.Callvirt, setRange)
        };
        foreach (var instruction in normalize) il.InsertBefore(first, instruction);
        // The inserted branch is long. Existing relative offsets within the original body remain unchanged.
        begin.Body.MaxStackSize = Math.Max(begin.Body.MaxStackSize, 3);
        string temporary = path + ".PDFgearClassic-new";
        assembly.Write(temporary);
        if (File.Exists(backup))
        {
            if (Hash(backup) != SupportedHash) throw new IOException("An existing original backup has a different checksum.");
        }
        else File.Copy(path, backup);
        if (Hash(path) != current || IsRunning(path)) throw new IOException("PDFgear changed or started during repair; replacement cancelled.");
        string patched = Hash(temporary);
        File.WriteAllText(manifest, JsonSerializer.Serialize(new RepairRecord(current, patched)));
        File.Move(temporary, path, true);
        return "PDFgear classic print repair installed; original executable backed up";
    }

    public static string Restore(string path)
    {
        string manifest = path + ".PDFgearClassic-repair.json";
        string backup = path + ".PDFgearClassic-original";
        if (!File.Exists(manifest)) return "No repair record; executable left unchanged";
        var record = JsonSerializer.Deserialize<RepairRecord>(File.ReadAllText(manifest));
        if (record is null || !File.Exists(backup) || Hash(backup) != record.OriginalHash
            || Hash(path) != record.PatchedHash) return "Executable or backup changed; restoration skipped";
        if (IsRunning(path)) return "Close PDFgear before restoring its original executable";
        File.Copy(backup, path, true);
        return "Original PDFgear executable restored; backup retained";
    }

    static string Hash(string path)
    {
        using var file = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(file));
    }

    static bool IsRunning(string path)
    {
        foreach (var process in Process.GetProcessesByName("pdfeditor"))
        {
            using (process)
            {
                try { if (string.Equals(process.MainModule?.FileName, path, StringComparison.OrdinalIgnoreCase)) return true; }
                catch { return true; }
            }
        }
        return false;
    }
}
