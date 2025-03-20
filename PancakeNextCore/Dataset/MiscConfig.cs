using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Dataset;

internal static class MiscConfig
{
    private static bool? _showSecondsInPerformanceAnalyzer = null;
    public static bool ShowSecondsInPerformanceAnalyzer
    {
        get
        {
            if (!_showSecondsInPerformanceAnalyzer.HasValue)
                _showSecondsInPerformanceAnalyzer = Config.Read(nameof(ShowSecondsInPerformanceAnalyzer), false, false);

            return _showSecondsInPerformanceAnalyzer.Value;
        }
        set
        {
            _showSecondsInPerformanceAnalyzer = value;
            Config.Write(nameof(ShowSecondsInPerformanceAnalyzer), value.ToString());
        }
    }
}
