using Grasshopper2.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Dataset;

internal static class IssueTracker
{
    public static void ReportInPlace(string desc)
    {
        Logger.Add(Level.Error, "[Pancake] " + desc);
    }
    public static void ReportAndThrow(string desc)
    {
        ReportInPlace(desc);
        throw new Exception(desc);
    }
}
