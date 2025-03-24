using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Utility;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.Io;

[IoId("5d6c7460-aedd-4ede-bd29-7730b0228c1f")]
public sealed class pcExportSTL : PancakeComponent
{
    static pcExportSTL()
    {
        CreateBinaryHeader();
    }

    private static readonly byte[] BinaryStlHeader = new byte[80];
    private static void CreateBinaryHeader()
    {
        var customHeader = Encoding.ASCII.GetBytes("PancakeForGhSTLMaker,v1");
        Buffer.BlockCopy(customHeader, 0, BinaryStlHeader, 5, customHeader.Length);
    }

    public pcExportSTL(IReader reader) : base(reader)
    {
    }

    public pcExportSTL() : base(typeof(pcExportSTL))
    {
    }

    protected override void RegisterInputs()
    {
    }

    protected override void RegisterOutputs()
    {
    }

    protected override void AddInputs(InputAdder inputs)
    {
        inputs.AddMesh("Mesh", "Ms", "Mesh to export");
        inputs.AddBoolean("Relocate", "Rl", "Relocate the mesh to the origin").Set(true);
        inputs.AddText("Destination", "Dst", "Path to save the file");
        inputs.AddBoolean("Save Format", "Fmt", "Save format of the file", ("Binary", "Save file as binary"), ("ASCII", "Save file as ASCII")).Set(true);
        inputs.AddBoolean("Save", "OK", "Save the file").Set(false);
    }

    protected override void AddOutputs(OutputAdder outputs)
    {
        outputs.AddBoolean("Saved", "OK", "Whether the file is saved.");
    }

    protected override void Process(IDataAccess access)
    {
        access.SetItem(0, false);

        access.GetItem<bool>(4, out var work);

        if (!work) return;

        access.GetItem<Mesh>(0, out var mesh);
        access.GetItem<bool>(1, out var reloc);
        access.GetItem<string>(2, out var savepath);
        access.GetItem<bool>(3, out var asbinary);

        if (Directory.Exists(savepath) || savepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            access.AddError("Wrong destination", "The path parameter shouldn't be a folder. It needs to be a file.");
            return;
        }

        if (File.Exists(savepath))
        {
            access.AddWarning("File already exists", "The file already exists. It will be overwritten.");
        }

        if (string.IsNullOrEmpty(Path.GetExtension(savepath)))
        {
            access.AddWarning("No extension", "The file extension is missing.");
        }

        if (!mesh.IsClosed)
        {
            access.AddWarning("Bad mesh", "The mesh is open. It may result in bad STL files.");
        }

        var mesh2 = mesh.DuplicateMesh();
        if (!mesh2.Faces.ConvertQuadsToTriangles() && mesh2.Faces.QuadCount != 0)
        {
            access.AddError("Bad mesh", "Failed to triangluate the mesh.");
            return;
        }

        if (reloc)
        {
            var bbox = mesh2.GetBoundingBox(true);
            bbox.MakeValid();
            Point3d minPoint = bbox.Min;
            if (!mesh2.Transform(Transform.Translation(-minPoint.X, -minPoint.Y, -minPoint.Z)))
                access.AddWarning("Bad mesh", "Failed to relocate the mesh.");
        }

        //mesh2.UnifyNormals();
        mesh2.FaceNormals.ComputeFaceNormals();

        try
        {
            if (asbinary)
            {
                WriteBinarySTL(savepath, mesh2);
            }
            else
            {
                WriteAsciiSTL(savepath, mesh2);
            }
        }
        catch (Exception e)
        {
            access.AddError("Error during export", e.Message);
            return;
        }

        access.SetItem(0, true);
    }

    private static void WriteBinarySTL(string savepath, Mesh mesh)
    {
        using var writer = new BinaryWriter(File.Open(savepath, FileMode.Create));

        writer.Write(BinaryStlHeader);
        writer.Write((uint)mesh.Faces.Count);
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            mesh.Faces.GetFaceVertices(i, out var a, out var b, out var c, out _);
            writer.Write((float)mesh.FaceNormals[i].X);
            writer.Write((float)mesh.FaceNormals[i].Y);
            writer.Write((float)mesh.FaceNormals[i].Z);

            writer.Write((float)a.X);
            writer.Write((float)a.Y);
            writer.Write((float)a.Z);

            a = b;

            writer.Write((float)a.X);
            writer.Write((float)a.Y);
            writer.Write((float)a.Z);

            a = c;

            writer.Write((float)a.X);
            writer.Write((float)a.Y);
            writer.Write((float)a.Z);

            writer.Write((ushort)0);
        }
    }

    private static void WriteAsciiSTL(string savepath, Mesh mesh)
    {
        using var writer = new StreamWriter(File.Open(savepath, FileMode.Create), Encoding.ASCII);

        writer.Write("solid PancakeGHSTLMaker,v1\n");
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            writer.Write("  facet normal {0:0.000000e+000} {1:0.000000e+000} {2:0.000000e+000}\n",
            mesh.FaceNormals[i].X, mesh.FaceNormals[i].Y, mesh.FaceNormals[i].Z);
            writer.Write("    outer loop\n");

            mesh.Faces.GetFaceVertices(i, out var a, out var b, out var c, out _);
            writer.Write("      vertex {0:0.000000e+000} {1:0.000000e+000} {2:0.000000e+000}\n",
                a.X, a.Y, a.Z);
            a = b;
            writer.Write("      vertex {0:0.000000e+000} {1:0.000000e+000} {2:0.000000e+000}\n",
                a.X, a.Y, a.Z);
            a = c;
            writer.Write("      vertex {0:0.000000e+000} {1:0.000000e+000} {2:0.000000e+000}\n",
                a.X, a.Y, a.Z);
            writer.Write("    endloop\n");
            writer.Write("  endfacet\n");
        }

        writer.Write("endsolid PancakeGHSTLMaker\n");
    }

    protected override IIcon? ActualIcon => SvgGhIcon.CreateFromSvgResource("ExportSTL", 32, 32);
}
