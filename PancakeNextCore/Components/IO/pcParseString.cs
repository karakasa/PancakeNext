using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Colour;
using GrasshopperIO;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Utility;
using Rhino.Geometry;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;

[IoId("6507d247-997f-454f-b8b1-33bf59967424")]
public sealed partial class pcParseString : PancakeComponent
{
    public pcParseString() : base(typeof(pcParseString)) { }
    public pcParseString(IReader reader) : base(reader) { }
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("string2");
        AddParam<TextParameter>("desiredtype", requirement: Grasshopper2.Parameters.Requirement.MayBeMissing);
    }
    protected override void RegisterOutputs()
    {
        AddParam("parsed");
        AddParam<TextParameter>("type");
    }

    private delegate bool TryParseDelegate<T>(string strData, [NotNullWhen(true)] out T result);
    private bool CheckType<T>(string typeName, TryParseDelegate<T> parseFunc)
    {
        if (!CheckDesire(typeName)) return false;
        if (parseFunc(_result, out var obj))
        {
            _access.SetItem(0, obj);
            _access.SetItem(1, typeName);
            return true;
        }

        return false;
    }

    private bool CheckDesire(string typeName)
    {
        return _desiredTypeTester.Contains(typeName, true);
    }

    private OptimizedConditionTester<string> _desiredTypeTester;

    private string? _result = null;
    private IDataAccess? _access = null;

    static readonly string[] DesiredTypeSeparators = [",", " "];

    private bool TryHandleFile(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        switch (extension)
        {
            case ".json":
                _result = FileIo.ReadAllText(path);
                if (CheckType<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight))
                    return true;
                _result = "";
                break;
            case ".xml":
                _result = FileIo.ReadAllText(path);
                if (CheckType<GhAssocBase>("Xml", TryParseXml))
                    return true;
                _result = "";
                break;
        }

        return false;
    }
    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out string result);

        if (!access.GetItem(1, out string desiredType) || string.IsNullOrEmpty(desiredType))
        {
            _desiredTypeTester = default;
        }
        else
        {
            desiredType.SplitLikelyOne(DesiredTypeSeparators, ref _desiredTypeTester);
        }

        _result = result.Trim();
        _access = access;

        try
        {
            if (FileIo.IsValidPath(_result) && TryHandleFile(_result))
            {
                return;
            }

            if (CheckType<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight) ||
                CheckType<GhLengthDecimal>("DecimalLength", GhLengthDecimal.TryParse) ||
                CheckType<GhLengthFeetInch>("FeetInchLength", GhLengthFeetInch.TryParse) ||
                CheckType<Point3d>("Point", TryParsePoint) ||
                CheckType<Interval>("Domain", TryParseInterval) ||
                CheckType<Colour>("Colour", TryParseColor) ||
                CheckType<GhAssocBase>("Xml", TryParseXml) ||
                CheckType<object>("Null", TryParseNull) ||
                CheckType<string>("CommaString", TryParseCommaString) ||
                CheckType<int>("Integer", int.TryParse) ||
                CheckType<double>("Number", double.TryParse) ||
                CheckType<bool>("Boolean", bool.TryParse) ||
                CheckType<DateTime>("DateTime", DateTime.TryParse) ||
                CheckType<Guid>("Guid", Guid.TryParse))
            {
                return;
            }
        }
        catch
        {

        }
        finally
        {
            _access = null;
            _result = "";
        }

        access.AddWarning("Unknown data type", Strings.CannotDetermineTheDataType);
        access.SetItem(1, "?");
    }
}