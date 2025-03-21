using GrasshopperIO;
using GrasshopperIO.DataBase;
using System.Diagnostics;
using System.Text;

namespace G2FileViewer;

public partial class FormViewer : Form
{
    public FormViewer()
    {
        InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        using var form = new OpenFileDialog();
        form.Filter = "*.ghz|*.ghz";

        if (form.ShowDialog() != DialogResult.OK) return;
        OpenArchive(form.FileName);
    }

    private void OpenArchive(string location)
    {
        var sb = new StringBuilder();

        try
        {
            Read(sb, location);
        }catch(Exception ex)
        {
            sb.AppendLine("Unknown error:");
            sb.AppendLine(ex.ToString());
        }

        textBox1.Text = sb.ToString();
    }

    static readonly byte[] ZipHeader = { 0x50, 0x4b, 0x3, 0x4 };

    private static void Read(StringBuilder sb, string location)
    {
        Span<byte> header = stackalloc byte[4];

        using var file = File.OpenRead(location);
        if (file.Length < 4)
        {
            sb.AppendLine("File is too short.");
            return;
        }

        file.ReadExactly(header);
        if (header.SequenceEqual(ZipHeader))
        {
            // ZIP


            ReadAsArchive(sb, location);
        }
        else
        {
            // Not ZIP, try IStorable

            file.Seek(0, SeekOrigin.Begin);
            var storable = IO.ReadStorableFromStream(file, new(location));

            sb.AppendLine($"IStorable: {storable.GetType()}");
            sb.AppendLine("==================");
            sb.AppendLine(storable.ToString());
        }
    }
    private static void ReadAsArchive(StringBuilder sb, string location)
    {
        var archive = Archive.Read(location);

        EnumerateMessages(sb, archive.Errors, "ERROR");
        EnumerateMessages(sb, archive.Warnings, "WARN");
        EnumerateMessages(sb, archive.Notes, "REMARK");

        sb.AppendLine();

        var sw = new Stopwatch();

        var paths = archive.Paths.ToArray();
        foreach (var path in paths)
        {
            sb.AppendLine(path);
            sb.AppendLine("==================");

            if (IsTextFile(path))
            {
                sb.AppendLine(archive.GetText(path));
                continue;
            }

            try
            {
                sw.Restart();
                var node = archive.GetRoot(path);
                sw.Stop();

                sb.AppendLine($"Parsed in: {sw.ElapsedMilliseconds / 1000.0:0.000} s");
                sb.AppendLine();

                PrintNode(sb, node, 0);
            }
            catch
            {
                sb.AppendLine($"Fail to interpret {path}");
            }

            sb.AppendLine();
        }
    }
    private static bool IsTextFile(string path)
    {
        if (path.Equals("plugins.txt", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
    private static void PrintNode(StringBuilder sb, Node node, int indent)
    {
        var idt = GetIndent(indent);

        foreach (var it in node.Items)
        {
            var name = it.Name;
            if (name.Index < 0)
            {
                sb.AppendLine($"{idt}{name.Label} = {it?.Value ?? "<null>"}");
            }
            else
            {
                sb.AppendLine($"{idt}{name.Label}({name.Index}) = {it?.Value ?? "<null>"}");
            }
        }

        foreach (var subNode in node.Nodes)
        {
            var name = subNode.Name;
            if (name.Index < 0)
            {
                sb.AppendLine($"{idt}{name.Label} = {{");
            }
            else
            {
                sb.AppendLine($"{idt}{name.Label}({name.Index}) = {{");
            }

            PrintNode(sb, subNode, indent + 1);

            sb.AppendLine($"{idt}}}");
        }
    }

    private static string GetIndent(int cnt)
    {
        return new string('\t', cnt);
    }

    private static void EnumerateMessages(StringBuilder sb, IEnumerable<string> msgs, string type)
    {
        foreach (var it in msgs)
            sb.AppendLine($"[{type}] {it}");
    }
}
