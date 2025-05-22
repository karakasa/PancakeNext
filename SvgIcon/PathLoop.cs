using Eto.Drawing;
using System.Collections.Generic;
using System.IO;

namespace SvgIcon;

public sealed class PathLoop
{
    public List<PathCommand> Commands { get; } = [];
#if DEBUG
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Commands.Count + 1);

        foreach (var command in Commands)
        {
            command.WriteTo(writer);
        }

        EofCommand.Instance.WriteTo(writer);
    }
#endif

    public void ReadFrom(BinaryReader reader)
    {
        Commands.Clear();

        var n = reader.ReadInt32();
        var read = 0;
        for (var i = 0; i < n; i++)
        {
            var inst = PathCommand.ReadNext(reader);
            if (inst is EofCommand) break;

            Commands.Add(inst);

            ++read;
        }

        if (read != n - 1)
        {
            throw new InvalidDataException($"Expected {n - 1} commands, but read {read}");
        }
    }
    public void AppendToGraphicsPath(IGraphicsPath path, bool keepOpen)
    {
        path.StartFigure();
        foreach (var it in Commands)
        {
            it.AppendToGraphicsPath(path);
        }
        if (!keepOpen)
            path.CloseFigure();
    }
}
