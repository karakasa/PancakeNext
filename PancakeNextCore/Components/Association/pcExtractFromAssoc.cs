using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
[IoId("f611c939-fe6f-449b-999d-e863608753a0")]
public sealed class pcExtractFromAssoc : PancakeComponent<pcExtractFromAssoc>, IPancakeLocalizable<pcExtractFromAssoc>
{
    public pcExtractFromAssoc() { }
    public pcExtractFromAssoc(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.DeconstructAssociativeArrayByKeys;
    public static string StaticLocalizedDescription => Strings.RetrieveDataFromAnAssociativeArrayByKeyPaths;
    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("assoc");
        AddParam<TextParameter>("path");
        AddParam("delimiter2", "/");
    }
    protected override void RegisterOutputs()
    {
        AddParam("value");
    }

    static readonly string[] DefaultDelimiter = ["/"];
    static string[] GetCachedDelimiterIfPossible(string delimiter)
    {
        if (delimiter == "/") return DefaultDelimiter;
        return [delimiter];
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out object assoc);

        if (assoc is not INodeQueryReadCapable inode)
        {
            access.AddError("Wrong input", Strings.InputTypeNotSupported);
            return;
        }

        access.GetItem(1, out string txt);
        access.GetItem(2, out string delimiter);

        if (string.IsNullOrEmpty(txt))
        {
            access.AddError("Wrong input", Strings.IncorrectPathFormat);
            return;
        }

        var path = txt.Split(GetCachedDelimiterIfPossible(delimiter), StringSplitOptions.None);

        if (!NodeQuery.TryGetNodeValue(inode, path, out var obj))
        {
            access.AddError("Path not found", string.Format(Strings.Path0NotFound, txt));
            return;
        }

        access.SetPear(0, obj);
    }

}