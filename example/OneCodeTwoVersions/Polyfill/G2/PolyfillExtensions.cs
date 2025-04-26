#if G2
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using OneCodeTwoVersions.Polyfill.DataTypes;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;

internal static class PolyfillExtensions
{
    public static Access To2(this GH_ParamAccess access)
    {
        return access switch
        {
            GH_ParamAccess.item => Access.Item,
            GH_ParamAccess.list => Access.Twig,
            GH_ParamAccess.tree => Access.Tree,
            _ => throw new ArgumentOutOfRangeException(nameof(access), "Invalid GH_ParamAccess")
        };
    }

    public static GH_Path To1(this Grasshopper2.Data.Path path)
    {
        return new(path.ToArray());
    }

    public static object? Peel(this object? obj)
    {
        if (obj is IGH_Goo goo) return goo.ScriptVariable();
        return obj;
    }

    public static GH_Structure<IGH_Goo> To1(this ITree tree)
    {
        var tree1 = new GH_Structure<IGH_Goo>();

        for (var i = 0; i < tree.PathCount; i++)
        {
            var path = tree.Paths[i];
            var twig = tree.Twigs[i];
            var list = tree1.EnsurePath(path.To1());
            var cnt = twig.LeafCount;
            for (var j = 0; j < cnt; j++)
            {
                if (twig.NullAt(j))
                {
                    list.Add(default);
                }
                else
                {
                    list.Add(Create(twig[j].Item));
                }
            }
        }

        return tree1;
    }

    public static IGH_Goo Create<T>(T data)
    {
        return data switch
        {
            int v => new GH_Integer(v),
            bool v => new GH_Boolean(v),
            double v => new GH_Number(v),
            Vector3d v => new GH_Vector(v),
            Transform v => new GH_Transform(v),
            Plane v => new GH_Plane(v),
            Box v => new GH_Box(v),
            Line v => new GH_Line(v),
            Circle v => new GH_Circle(v),
            Rectangle3d v => new GH_Rectangle(v),
            Arc v => new GH_Arc(v),
            Curve v => new GH_Curve(v),
            Surface v => new GH_Surface(v),
            Brep v => new GH_Brep(v),
            SubD v => new GH_SubD(v),
            Mesh v => new GH_Mesh(v),
            MeshFace v => new GH_MeshFace(v),
            DateTime v => new GH_Time(v),
            System.Drawing.Color v => new GH_Colour(v),
            string v => new GH_Text(v),
            Interval v => new GH_Interval(v),
            GH_Path v => new GH_StructurePath(v),
            GeometryBase v => new GH_Geometry(v),
            _ => new GH_ObjectWrapper((object)data)
        };
    }
}
#endif