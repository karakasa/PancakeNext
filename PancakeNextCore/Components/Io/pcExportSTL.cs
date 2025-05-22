using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;

[IoId("5d6c7460-aedd-4ede-bd29-7730b0228c1f")]
[ComponentCategory("io", 1)]
public sealed class pcExportSTL : PancakeComponent<pcExportSTL>, IPancakeLocalizable<pcExportSTL>
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

    public pcExportSTL()
    {
    }

    protected override void RegisterInputs()
    {
        AddParam<MeshParameter>("mesh");
        AddParam("relocate", true);
        AddParam<TextParameter>("savepath");
        AddParam("saveasbinaryfile", true);
        AddParam("savenow", false);
    }

    protected override void RegisterOutputs()
    {
        AddParam<BooleanParameter>("ok");
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
            access.AddError("Wrong destination", Strings.ThePathParameterShouldnTBeAFolder);
            return;
        }

        if (File.Exists(savepath))
        {
            access.AddWarning("File already exists", Strings.TargetFileAlreadyExistsSTL);
        }

        if (string.IsNullOrEmpty(Path.GetExtension(savepath)))
        {
            access.AddWarning("No extension", Strings.NoExtensionIsIncludedInThePath);
        }

        if (!mesh.IsClosed)
        {
            access.AddWarning("Bad mesh", Strings.TheMeshIsNotClosed);
        }

        var mesh2 = mesh.DuplicateMesh();
        if (!mesh2.Faces.ConvertQuadsToTriangles() && mesh2.Faces.QuadCount != 0)
        {
            access.AddError("Bad mesh", Strings.FailToTriangulateTheMesh);
            return;
        }

        if (reloc)
        {
            var bbox = mesh2.GetBoundingBox(true);
            bbox.MakeValid();
            Point3d minPoint = bbox.Min;
            if (!mesh2.Transform(Transform.Translation(-minPoint.X, -minPoint.Y, -minPoint.Z)))
                access.AddWarning("Bad mesh", Strings.FailToRelocateTheMesh);
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

    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("ExportSTL");

    public static string StaticLocalizedName => Strings.ExportSTL;

    public static string StaticLocalizedDescription => Strings.ExportSTLDesc;
}
