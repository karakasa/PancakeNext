using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;

[ComponentCategory("output", 2)]
[IoId("feac04da-23c1-46ec-b2c5-c71d7a74bbdb")]
public sealed class pcFileSeries : PancakeComponent<pcFileSeries>, IPancakeLocalizable<pcFileSeries>
{
    public pcFileSeries() { }
    public pcFileSeries(IReader reader) : base(reader) { }

    private int counter = -1;
    private bool counterSet = false;

    private static int SeekNextFile(int id, string filepath)
    {

        for (int i = 0; i <= 10000; i++)
        {
            if (!File.Exists(string.Format(filepath, id + i)))
                return id + i;
        }

        return -1;
    }

    public static string StaticLocalizedName => Strings.FilepathSeries;
    public static string StaticLocalizedDescription => Strings.CalculateTheNextFilenameInAFilepathSeries;
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("patternoffilepath");
        AddParam("startingindex", 1);
        AddParam("quantityoffilenames", -1);
        AddParam("generatenext", false);
        AddParam("reset", false);
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("filepath");
        AddParam<TextParameter>("listoffilepath", Access.Twig);
        AddParam<IntegerParameter>("counteroffirstfile");
    }

    protected override void Process(IDataAccess access)
    {
        FailInUntrusted();

        if (!counterSet)
        {
            if (!access.GetItem(1, out counter))
            {
                access.AddError("Wrong index", Strings.InvalidStartingIndex);
                return;
            }
            counterSet = true;
        }

        if (!access.GetItem(0, out string pattern) || pattern is null)
        {
            access.AddError("Wrong format", Strings.InvalidPattern);
            return;
        }

        try
        {
            _ = string.Format(pattern, counter);
        }
        catch (Exception)
        {
            access.AddError("Wrong format", Strings.IllegalPatternFormat);
            return;
        }

        access.GetItem(2, out int multiple);

        if (multiple <= 0)
            multiple = 1;

        access.GetItem(3, out bool ok);
        access.GetItem(4, out bool reset);

        access.SetItem(0, "");

        if (reset)
        {
            if (!access.GetItem(1, out counter))
            {
                access.AddError("Wrong index", Strings.InvalidStartingIndex);
                return;
            }

            counterSet = true;
        }


        var result = SeekNextFile(counter, pattern);
        if (result == -1)
        {
            access.AddError("IO failure", Strings.CannotDetermineTheNextIndexAfter10000SearchEs);
        }
        else
        {
            counter = result;
            var filepath = string.Format(pattern, counter);
            var directory = Path.GetDirectoryName(filepath);

            if (directory is null)
            {
                access.AddError("Wrong format", $"{filepath} isn't in a valid directory.");
                return;
            }

            Directory.CreateDirectory(directory);
            access.SetItem(0, filepath);
            access.SetItem(2, counter);

            if (multiple >= 1)
            {
                var fnList = new List<string> { Capacity = multiple };
                for (int i = 0; i < multiple; i++)
                    fnList.Add(string.Format(pattern, counter + i));

                access.SetTwig(1, Garden.TwigFromList(fnList));
            }
        }

    }

    protected override InputOption[][] SimpleOptions => [[
            new ButtonOption("Reset counter", Strings.ResetInternalCounter, () => counterSet = false, "Reset")
            ]];
}