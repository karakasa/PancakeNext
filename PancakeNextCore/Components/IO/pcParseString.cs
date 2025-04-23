using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Colour;
using Grasshopper2.Types.Conversion;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH;
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
        return side == Side.Output && index == 1 && Parameters.OutputCount == 1;
    }

    public override bool CanRemoveParameter(Side side, int index)
    {
        return side == Side.Output && index == 1 && Parameters.OutputCount == 2;
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
    private bool _noFilterMode = false;
    protected override void Process(IDataAccess access)
    {
        var hasTypeOutput = Parameters.OutputCount > 1;

        access.GetTree<string>(0, out var tree);

        _noFilterMode = false;
        if (!access.GetItem(1, out string desiredType) || string.IsNullOrEmpty(desiredType))
        {
            _desiredTypeTester = default;
            _noFilterMode = true;
        }
        else
        {
            desiredType.ToLowerInvariant().SplitLikelyOne(DesiredTypeSeparators, ref _desiredTypeTester);
        }

        // var solutionCancellationToken = access.Solution.Token;
        ITree? result;
        Tree<string>? resultTypes = null;

        if (HasOnlyOneDesiredType)
        {
            var type = _desiredTypeTester.SingleValue;

            if (TryParseStringTreeToOneType(tree, type, out var to))
            {
                result = to;
                if (hasTypeOutput)
                {
                    resultTypes = GhExtensions.MimicTreeWithOneValue(tree, type, false);
                }
            }
            else
            {
                access.AddError("Wrong type", $"{type} is not a known type.");
                return;
            }
        }
        else
        {
            result = null;
            var type = "";

            if (GuessType(tree) is { } parser && parser.TryParseStringTreeAs(tree, ref result, ref type, true))
            {
                resultTypes = GhExtensions.MimicTreeWithOneValue(tree, type, false);
            }
            else
            {
                ParseStringTreeGeneric(tree, out result, hasTypeOutput, out resultTypes);
            }
        }

        access.SetTree(0, result);
        if (resultTypes is not null && hasTypeOutput)
        {
            access.SetTree(1, resultTypes);
        }
    }

    private static Parser? GuessType(Tree<string> tree)
    {
        EducatedGuess lastGuess = EducatedGuess.Unknown;

        foreach (var it in tree.AllPears)
        {
            if (it?.Item is not { } str) return null;

            var guess = GuessString(str);

            if (guess is EducatedGuess.Unknown)
                return null;

            if (guess == lastGuess)
                continue;

            if (lastGuess is EducatedGuess.Unknown)
            {
                lastGuess = guess;
                continue;
            }

            switch (guess)
            {
                case EducatedGuess.Bool:
                    return null;

                case EducatedGuess.Integer:
                    if (lastGuess is not EducatedGuess.Number)
                        return null;
                    break;

                case EducatedGuess.Number:
                    if (lastGuess is EducatedGuess.Integer)
                    {
                        lastGuess = EducatedGuess.Number;
                    }
                    else
                    {
                        return null;
                    }

                    break;

                default:
                    return null;
            }
        }

        return lastGuess switch
        {
            EducatedGuess.Bool => BoolParser,
            EducatedGuess.Integer => IntParser,
            EducatedGuess.Number => DoubleParser,
            EducatedGuess.TupleOf3Numbers => Point3dParser,
            _ => null,
        };
    }

    private bool TryParseStringTreeToOneType(Tree<string> from, string desiredType, [NotNullWhen(true)] out ITree? to)
    {
        var typeName = "";
        to = null;

        foreach (var it in Parsers)
        {
            if (!CheckDesire(it.Name))
                continue;

            return it.TryParseStringTreeAs(from, ref to, ref typeName);
        }

        return false;
    }
    private bool ParseString(string from, [NotNullWhen(true)] out object? to, [NotNullWhen(true)] out string? typeName)
    {
        typeName = null;
        to = null;

        if (TryAsFilePath && FileIo.IsValidPath(from) && TryHandleFile(from, ref to, ref typeName))
        {
            return true;
        }

        foreach (var it in Parsers)
        {
            if (!CheckDesire(it.Name))
                continue;

            if (_noFilterMode && it.ExcludeInAllMode)
                continue;

            if (it.TryConvertGeneric(from, ref to, ref typeName))
                return true;
        }

        return false;
    }

    private void ParseStringTreeGeneric(Tree<string> strings, out ITree outputs, bool populateTypeNames,
        out Tree<string>? outTypeName)
    {
        var pathCount = strings.PathCount;
        var twigs = new ITwig[pathCount];

        var outputTypeNames = populateTypeNames ? new Twig<string>[pathCount] : null;

        for (var i = 0; i < pathCount; i++)
        {
            var strs = strings.Twigs[i];
            var j = 0;

            var cnt = strs.LeafCount;

            var twig = new IPear?[cnt];

            string[]? names = null;

            if (populateTypeNames)
            {
                names = new string[cnt];
            }

            foreach (var leaf in strs.Pears)
            {
                if (leaf is null || !ParseString(leaf.Item, out var to, out var typeName))
                {
                    to = null;
                    typeName = "?";
                }

                if (populateTypeNames)
                {
                    names[j] = typeName;
                }

                var pear = to.AsPear();
                if (leaf?.Meta is not null)
                {
                    pear = pear.WithMeta(leaf.Meta);
                }
                twig[j] = pear;

                ++j;
            }

            twigs[i] = Garden.ITwigFromPears(twig);
            if (populateTypeNames)
            {
                outputTypeNames[i] = Garden.TwigFromList(names);
            }
        }

        outputs = Garden.ITreeFromITwigs(twigs);

        if (populateTypeNames)
        {
            outTypeName = Garden.TreeFromTwigs(outputTypeNames);
        }
        else
        {
            outTypeName = null;
        }
    }

    const string ConfigTryAsFilePath = "TryAsFilePath";
    bool _tryInterpretAsFilePath = false;
    public bool TryAsFilePath
    {
        get => _tryInterpretAsFilePath;
        set => SetValue(ConfigTryAsFilePath, _tryInterpretAsFilePath = value);
    }

    protected override void ReadConfig()
    {
        _tryInterpretAsFilePath = GetValue(ConfigTryAsFilePath, false);
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption("Try as file path", "Try to interpret input as a file path", TryAsFilePath, x => TryAsFilePath = x, "Try as file", "Don't try as file")
            ]];
}