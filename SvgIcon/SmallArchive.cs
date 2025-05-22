using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SvgIcon;
public abstract class SmallArchive<T>(string id)
{
    public SmallArchive() : this("") { }

    [Flags]
    protected enum StorageFlag
    {
    }
    const int MagicNumber = 1665229648;
    const int Version = 1;
    protected abstract T CreateFrom(BinaryReader reader, string name, int maxSize);
#if DEBUG
    protected abstract void WriteTo(BinaryWriter writer, string name, T item);
    protected virtual byte[] GetMetadata() { return []; }
    public byte[] GetBytes()
    {
        using var ms = new MemoryStream();
        WriteTo(ms);
        return ms.ToArray();
    }
#endif
    public List<(string Name, T Item)> Entries { get; } = [];
    public string Identifier { get; } = id; protected StorageFlag Flags { get; set; }
    protected virtual void ProcessMetadata(byte[] metadata) { }
    public void ReadFrom(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        ReadFrom(ms);
    }
    public void ReadFrom(Stream stream)
    {
        if (!stream.CanRead) throw new InvalidOperationException("Stream is not readable.");

        BeforeRead();

        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            if (reader.ReadInt32() != MagicNumber)
                throw new InvalidDataException("Invalid file header.");

            if (reader.ReadInt32() > Version)
                throw new InvalidDataException("Unsupported version.");

            Flags = (StorageFlag)reader.ReadInt32();

            if (reader.ReadInt32() != 0)
                throw new InvalidDataException("Invalid data.");
            if (reader.ReadInt32() != 0)
                throw new InvalidDataException("Invalid data.");
        }

        BeforeReadTOC();

        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            var archiveId = reader.ReadString();
            if (archiveId != Identifier)
                throw new InvalidDataException($"Unknown identifier. expected {Identifier}, got {archiveId}.");

            var metadataSize = reader.ReadInt32();
            ProcessMetadata(reader.ReadBytes(metadataSize));

            var tocSize = reader.ReadInt32();

            var toc = reader.ReadBytes(tocSize);
            using var msToc = new MemoryStream(toc, false);
            using var readerToc = new BinaryReader(msToc, Encoding.UTF8);

            var n = readerToc.ReadInt32();
            Entries.Clear();
            if (Entries.Capacity < n) Entries.Capacity = Math.Max(n, 4);

            for (var i = 0; i < n; i++)
            {
                var name = readerToc.ReadString();
                var pos = readerToc.ReadInt32();
                var size = readerToc.ReadInt32();

                var readerPosition = stream.Position;
                T item;
                try
                {
                    item = CreateFrom(reader, name, size);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to read {name}: {ex.Message}", ex);
                }
                var readSize = stream.Position - readerPosition;

                if (readSize > size)
                {
                    throw new InvalidDataException($"Invalid size for {name}: expected {size}, got {readSize}.");
                }

                Entries.Add((name, item));
            }
        }

        AfterRead();
    }
#if DEBUG
    public void WriteTo(Stream stream)
    {
        if (!stream.CanWrite) throw new InvalidOperationException("Stream is not writable.");

        BeforeWrite();

        using var msToc = new MemoryStream();
        using var msContent = new MemoryStream();
        using (var wrToc = new BinaryWriter(msToc, Encoding.UTF8, true))
        using (var wrContent = new BinaryWriter(msContent, Encoding.UTF8, true))
        {
            wrToc.Write(Entries.Count);

            foreach (var (name, item) in Entries)
            {
                wrToc.Write(name);
                wrToc.Write((int)msContent.Position);

                var position = msContent.Position;
                try
                {
                    WriteTo(wrContent, name, item);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to write {name}: {ex.Message}", ex);
                }
                var size = msContent.Position - position;

                wrToc.Write((int)size);
            }
        }

        var tocSize = (int)msToc.Position;

        using (var streamWriter = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            streamWriter.Write(MagicNumber);
            streamWriter.Write(Version);
            streamWriter.Write((int)Flags);
            streamWriter.Write(0);
            streamWriter.Write(0);
        }

        using (var streamWriter = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            streamWriter.Write(Identifier);
            var metadata = GetMetadata() ?? [];
            streamWriter.Write(metadata.Length);
            if (metadata.Length > 0)
                streamWriter.Write(metadata);
            streamWriter.Write(tocSize);
        } 

        msToc.Position = 0;
        msContent.Position = 0;
        msToc.CopyTo(stream);
        msContent.CopyTo(stream);

        stream.Flush();

        AfterWrite();
    }
    public void Add(string name, T item) => Entries.Add((name, item));
    protected virtual void BeforeWrite() { }
    protected virtual void AfterWrite() { }
#endif
    public bool TryGet(string name, out T value)
    {
        foreach (var (Name, Item) in Entries)
        {
            if (Name == name)
            {
                value = Item;
                return true;
            }
        }
        value = default!;
        return false;
    }

    protected virtual void BeforeReadTOC() { }
    protected virtual void BeforeRead() { }
    protected virtual void AfterRead() { }

}
