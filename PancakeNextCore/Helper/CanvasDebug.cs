using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
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
    private static readonly List<double> DrawTime = [];
    private static readonly List<double> IterationTime = [];
    private static int DrawCount = 0;
    private static int DrawThreshold = 300;
    private static readonly Stopwatch BaseWatch = new();
    private static double TimeThreshold = 5 * 1000;
    internal static void BenchmarkFps()
    {
        // TODO

        DrawCount = 0;
        BaseWatch.Reset();
        DrawTime.Clear();
        IterationTime.Clear();

        var canvas = Editor.Instance.Canvas;
        var originalProjection = canvas.Projection;

        var step = 1.0 / DrawThreshold;
        var sw = new Stopwatch();

        const double TickMsConversion = 1 / 10000.0;

        for(; ; )
        {
            if (DrawCount == 0)
            {
                ++DrawCount;
                canvas.Invalidate(true);
                BaseWatch.Restart();
                Application.Instance.RunIteration();
                continue;
            }

            var flex = (IFlexControl)canvas;
            var lastMs = (int)(flex.DrawEndTime - flex.DrawStartTime).Ticks * TickMsConversion;
            DrawTime.Add(lastMs);
            ++DrawCount;

            if (DrawCount > DrawThreshold || BaseWatch.ElapsedMilliseconds > TimeThreshold)
            {
                BaseWatch.Stop();
                UiHelper.InvokeUi(DisplayResult);
                break;
            }

            sw.Restart();
            canvas.Projection = canvas.Projection.PerformPan(GetVector(DrawCount, step));
            canvas.Invalidate(true);
            Application.Instance.RunIteration();
            sw.Stop();

            IterationTime.Add(sw.ElapsedTicks * TickMsConversion);
        }

        canvas.Projection = originalProjection;
    }

    private static SizeF GetCircularVector(double percentage)
    {
        var angle = percentage * Math.PI * 2;
        return new SizeF((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    private static SizeF GetVector(int cnt, double step)
    {
        var size = 100.0f;

        var i = cnt % 4;
        SizeF v = i switch
        {
            0 => new(1, 0),
            1 => new(0, 1),
            2 => new(-1, 0),
            _ => new(0, -1),
        };

        // var b = GetCircularVector(cnt * step + step);
        // var a = GetCircularVector(cnt * step);

        // var v = new SizeF((b.Width - a.Width) * size, (b.Height - a.Height) * size);
        v *= size;
        return v;
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
            var sum = DrawTime.Sum();
            var sumIteration = IterationTime.Sum();
            var avgIteration = sumIteration / frame;
            var low1 = CalculateLow1(IterationTime);

            RhinoApp.WriteLine("Pancake GH2 Canvas Benchmark Result:");
            RhinoApp.WriteLine($"Total time: {frame} frames in {ms / 1000.0:0.000} s. {1.0*ms/frame: 0.000} ms/f = {1000.0 / ms * frame:0.0} fps.");
            RhinoApp.WriteLine($"Message pump time: {avgIteration: 0.000} (+/- {GetSD(sumIteration, IterationTime):0.000}) ms/f, {sumIteration / ms * 100.0:0.0}% of total. 1% lowest {low1:0.000} ms/f = {1000.0 / low1:0.0} fps.");
            RhinoApp.WriteLine($"Drawing pass time: {sum / frame:0.000} (+/- {GetSD(sum, DrawTime):0.000}) ms/f, {sum / ms * 100.0:0.0}% of total. 1% lowest {CalculateLow1(DrawTime):0.000} ms/f.");
        }
    }

    private static double CalculateLow1(List<double> list)
    {
        return list.OrderByDescending(x => x).Take(Math.Max(list.Count / 10, 1)).Average(x => (double)x);
    }
    private static double GetSD(double sum, List<double> list)
    {
        var cnt = list.Count;
        var avg = sum / cnt;

        var sdSum = 0.0;
        foreach (var it in list)
        {
            sdSum += (it - avg) * (it - avg);
        }
        return Math.Sqrt(sdSum / cnt);
    }
}
