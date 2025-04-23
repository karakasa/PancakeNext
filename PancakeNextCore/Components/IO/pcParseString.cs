using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Colour;
using Grasshopper2.Types.Conversion;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using Rhino.Commands;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;

[IoId("6507d247-997f-454f-b8b1-33bf59967424")]
[ComponentCategory("io", 0)]
public sealed partial class pcParseString : PancakeComponent<pcParseString>, IPancakeLocalizable<pcParseString>
{
    public pcParseString() { }
    public pcParseString(IReader reader) : base(reader) { }
    internal bool CreateTypeOutput { get; set; } = false;
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("string2", Access.Tree);
        AddParam<TextParameter>("desiredtype", requirement: Requirement.MayBeMissing);
    }
    protected override void RegisterOutputs()
    {
        AddParam("parsed", access: Access.Tree);
        if (CreateTypeOutput)
        {
            AddParam<TextParameter>("type", Access.Tree);
            CreateTypeOutput = false;
        }
    }

    public override bool CanCreateParameter(Side side, int index)
    {
        return side == Side.Output && index == 1;
    }

    public override bool CanRemoveParameter(Side side, int index)
    {
        return side == Side.Output && index == 1;
    }

    public override void DoCreateParameter(Side side, int index)
    {
        var p = CreateLocalizedParameter<TextParameter>("type", Access.Tree);
        Parameters.AddOutput(p, index);
    }
    private bool CheckDesire(string typeName)
    {
        return _desiredTypeTester.Contains(typeName, true);
    }

    private OptimizedConditionTester<string> _desiredTypeTester;
    private bool HasOnlyOneDesiredType => _desiredTypeTester.Count == 1;

    static readonly string[] DesiredTypeSeparators = [",", " "];

    public static string StaticLocalizedName => Strings.ParseString;

    public static string StaticLocalizedDescription => Strings.ParseFormattedStringToItsCorrectTypeCurrentlyThisComponentSupportsIntegerNumberBooleanGuidLengthQuantityDatetimePointDomain12DColourAndJsonSeeExamplesOrManualForMoreInformation;
    protected override void Process(IDataAccess access)
    {
        var hasTypeOutput = Parameters.OutputCount > 1;

        access.GetTree<string>(0, out var tree);

        if (!access.GetItem(1, out string desiredType) || string.IsNullOrEmpty(desiredType))
        {
            _desiredTypeTester = default;
        }
        else
        {
            desiredType.SplitLikelyOne(DesiredTypeSeparators, ref _desiredTypeTester);
        }

        var solutionCancellationToken = access.Solution.Token;
        ITree result;
        Tree<string>? resultTypes = null;

        if (HasOnlyOneDesiredType)
        {
            if (TryParseStringTree(tree, out var to, out var typeName))
            {
                result = to;
                if (hasTypeOutput)
                {
                    resultTypes = GhExtensions.MimicTreeWithOneValue(tree, typeName, false);
                }
            }
            else
            {
                result = GhExtensions.MimicTreeWithOneValue(tree, default(object?), true);
                if (hasTypeOutput)
                {
                    resultTypes = GhExtensions.MimicTreeWithOneValue(tree, "?", false);
                }
            }
        }
        else
        {
            ParseStringTreeGeneric(tree, out result, hasTypeOutput, out resultTypes);
        }

        access.SetTree(0, result);
        if (resultTypes is not null && hasTypeOutput)
        {
            access.SetTree(1, resultTypes);
        }
    }
}