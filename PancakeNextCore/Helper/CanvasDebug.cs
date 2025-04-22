using Grasshopper2.UI;
using Grasshopper2.UI.Flex;
using PancakeNextCore.UI;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Helper;
internal static class CanvasDebug
{
    private static readonly List<int> DrawTime = [];
    private static int DrawCount = 0;
    private static int DrawThreshold = 500;
    private static readonly Stopwatch BaseWatch = new();
    private static double TimeThreshold = 10 * 1000;
    internal static void BenchmarkFps()
    {
        // TODO

        DrawCount = 0;
        BaseWatch.Reset();
        DrawTime.Clear();

        var canvas = Editor.Instance.Canvas;
        canvas.Draw += DrawEvent;
        canvas.Invalidate(true);
    }
    private static void DrawEvent(object? sender, ControlDrawEventArgs e)
    {
        if (DrawCount == 0)
        {
            ++DrawCount;
            BaseWatch.Restart();
            return;
        }

        var ctrl = e.Control;
        var lastMs = (int)(ctrl.DrawEndTime - ctrl.DrawStartTime).TotalMilliseconds;
        DrawTime.Add(lastMs);
        ++DrawCount;

        var canvas = Editor.Instance.Canvas;

        if (DrawCount > DrawThreshold || BaseWatch.ElapsedMilliseconds > TimeThreshold)
        {
            BaseWatch.Stop();
            canvas.Draw -= DrawEvent;
            UiHelper.InvokeUi(DisplayResult);
            return;
        }

        canvas.Invalidate(true);
    }

    private static void DisplayResult()
    {
        if (DrawTime.Count == 0)
        {
            RhinoApp.WriteLine("No data is collected.");
        }
        else
        {
            var ms = BaseWatch.ElapsedMilliseconds;
            var frame = DrawTime.Count;
            var sum = (double)DrawTime.Sum();
            RhinoApp.WriteLine($"{frame} frames in {ms / 1000.0:0.000} s, avg. {(double)ms / frame:0.000} ms/frame, avg. {(double)frame / ms:0.0} fps.");
            RhinoApp.WriteLine($"Draw time alone: {sum / frame:0.000} ms/frame, accounting for {sum / ms:0.0}% of total time. SD = {GetSD(sum)} ms");
        }
    }

    private static double GetSD(double sum)
    {
        var cnt = DrawTime.Count;
        var avg = sum / cnt;

        var sdSum = 0.0;
        foreach (var it in DrawTime)
        {
            sdSum += (it - avg) * (it - avg);
        }
        return Math.Sqrt(sdSum / cnt);
    }
}
