using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Pancake.Dataset;
using Pancake.UI;
using Pancake.Utility;

namespace PancakeNextCore.Helper;

internal class Performance
{
    internal static double GetProcessorTime(GH_Document doc, IEnumerable<IGH_ActiveObject> objs)
    {
        return objs?.Sum(x => x.ProcessorTime.TotalMilliseconds) ?? GetProcessorTime(doc, doc.ActiveObjects());
    }

    internal static double GetProcessorTimeAtom(GH_Document doc, IGH_ActiveObject objs)
    {
        return objs?.ProcessorTime.TotalMilliseconds ?? GetProcessorTime(doc, null);
    }

    internal static void ShowObjectGroupTimeReport(GH_Document doc, IEnumerable<IGH_ActiveObject> activeObjs,
        Func<GH_Document, IGH_ActiveObject, double> evaluationFunction = null,
        Func<IGH_ActiveObject, string> extendInfo = null, Func<GH_Document, string> extendDocInfo = null)
    {
        var evaluate = evaluationFunction ?? GetProcessorTimeAtom;

        var objs = activeObjs.ToList();
        if (objs.Count == 0)
            return;

        objs.Sort((x, y) => evaluate(doc, y).CompareTo(evaluate(doc, x)));

        var total = evaluate(doc, null);
        var totalSpan = SpanFromMs(total);

        var objTime = objs.Select(x => evaluate(doc, x)).ToList();

        var selected = objTime.Sum();
        var selectedSpan = SpanFromMs(selected);

        var report = string.Format(Strings.Performance_ShowObjectGroupTimeReport_Report1, objs.Count);
        report += Strings.Performance_ShowObjectGroupTimeReport_Report2;

        report += string.Format(Strings.Performance_ShowObjectGroupTimeReport_Report3, selectedSpan);
        report += string.Format(Strings.Performance_ShowObjectGroupTimeReport_Report4, totalSpan);
        report += string.Format(Strings.Performance_ShowObjectGroupTimeReport_Report5, selected / total * 100);

        if (extendDocInfo != null)
        {
            report += "\r\n";
            report += extendDocInfo(doc);
            report += "\r\n";
        }

        report += "\r\n";
        report += Strings.Performance_ShowObjectGroupTimeReport_Report6;

        for (var i = 0; i < objs.Count; i++)
        {
            var obj = objs[i];
            var extend = extendInfo == null ? "" : $" {extendInfo(obj)}";
            report +=
                $"    {objTime[i] / selected * 100:00.00}% {SpanFromMs(objTime[i]):G}{extend} {obj.NickName}\r\n";
        }

        report += "\r\n";
        report += Strings.Performance_ShowObjectGroupTimeReport_Report7;

        var sum = new Dictionary<Guid, double>();
        var names = new Dictionary<Guid, string>();

        for (var i = 0; i < objs.Count; i++)
        {
            var obj = objs[i];
            var gid = obj.ComponentGuid;
            if (sum.ContainsKey(gid))
            {
                sum[gid] += objTime[i];
            }
            else
            {
                names[gid] = obj.Name;
                sum[gid] = objTime[i];
            }
        }

        var timeList = sum.ToList();
        timeList.Sort((y, x) => x.Value.CompareTo(y.Value));

        foreach (var time in timeList)
        {
            report +=
                $"    {time.Value / selected * 100:00.00}% {SpanFromMs(time.Value):G} {names[time.Key]}\r\n";
        }

        Presenter.ShowReportWindow(report);
    }

    private static TimeSpan SpanFromMs(double ms)
    {
        return TimeSpan.FromTicks(Convert.ToInt64(ms * 10000));
    }

    public static void ProfileRepeated(GH_Document doc, IEnumerable<IGH_ActiveObject> ghObjs)
    {
        Cursor.Current = Cursors.WaitCursor;
        Application.DoEvents();

        var docTime = new List<double>();
        var times = new Dictionary<Guid, List<double>>();
        var avgTime = new Dictionary<Guid, double>();
        var extInfo = new Dictionary<Guid, string>();
        var objs = ghObjs.ToList();
        objs.ForEach(x => times[x.InstanceGuid] = new List<double>());

        var runCount = 0;
        var timeCount = new Stopwatch();

        for (; ; )
        {
            runCount++;
            timeCount.Start();
            doc.NewSolution(true, GH_SolutionMode.Silent);
            timeCount.Stop();

            if ((timeCount.ElapsedMilliseconds > 3000 || runCount > 60) && runCount > 3)
                break;
        }

        runCount = 0;
        timeCount.Reset();

        for (; ; )
        {
            runCount++;
            timeCount.Start();
            doc.NewSolution(true, GH_SolutionMode.Silent);
            timeCount.Stop();
            objs.ForEach(x => times[x.InstanceGuid].Add(x.ProcessorTime.TotalMilliseconds));
            docTime.Add(doc.ActiveObjects().Sum(x => x.ProcessorTime.TotalMilliseconds));

            if ((timeCount.ElapsedMilliseconds > 5000 || runCount > 100) && runCount > 5)
                break;
        }

        var docTotal = docTime.Sum();
        foreach (var obj in objs)
        {
            var guid = obj.InstanceGuid;
            var avg = times[guid].Average();
            avgTime[guid] = avg;

            CalculateJitter(avg, times[guid].Max(), times[guid].Min(), out var max, out var min);
            extInfo[guid] = $"(-{min * 100:00.00}% ~ +{max * 100:00.00}%)";
        }

        Cursor.Current = Cursors.Default;
        Application.DoEvents();

        ShowObjectGroupTimeReport(doc, objs,
            (_, obj) => obj == null ? docTotal / runCount : avgTime[obj.InstanceGuid],
            obj => extInfo[obj.InstanceGuid], _ => string.Format(Strings.Performance_ProfileRepeated_Repeated, runCount));
    }

    private static void CalculateJitter(double avg, double max, double min, out double maxOffset, out double minOffset)
    {
        if (Math.Abs(avg) < 1e-7)
        {
            maxOffset = 0;
            minOffset = 0;
            return;
        }
        maxOffset = (max - avg) / avg;
        minOffset = (avg - min) / avg;
    }

    internal static void ShowEntireDocument()
    {
        PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.Close();
        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null)
            return;

        var snapshot = PerformanceSnapshot.CreateFromDoc(doc);

        // if (Config.UseEtoWhenPossible)
        {
            PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.Show();
            if (PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.TryGet(out var form))
            {
                form.ShowSnapshot(snapshot);
                form.BringToFront();
            }

            return;
        }

        //{
        //    PersistentForm<FormPerformanceAnalyzer>.Show();
        //    if (PersistentForm<FormPerformanceAnalyzer>.TryGet(out var form))
        //    {
        //        form.ShowSnapshot(snapshot);
        //    }
        //}
    }

    internal static void ShowSelected()
    {
        var doc = Instances.ActiveCanvas?.Document;
        if (doc is null) return;

        var list = doc.SelectedObjects();
        if (list.Count == 0) return;

        var snapshot = PerformanceSnapshot.CreateFromDoc(doc);

        // if (Config.UseEtoWhenPossible)
        {
            PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.Close();
            PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.Show(
                () => UI.EtoForms.FormPerformanceAnalyzer.CreateFromSnapshot(
                    snapshot, true));

            return;
        }

        //PersistentForm<FormPerformanceAnalyzer>.Close();
        //PersistentForm<FormPerformanceAnalyzer>.Show(
        //    () => FormPerformanceAnalyzer.CreateFromSnapshot(
        //       snapshot, true));
    }

    internal static void BenchmarkSelected()
    {
        var doc = Instances.ActiveCanvas?.Document;
        if (doc is null) return;

        var list = doc.SelectedObjects();
        if (list.Count == 0) return;

        var sw = new Stopwatch();
        sw.Start();

        var runCnt = 0;
        var snapshot = PerformanceSnapshot.CreateFromDocAverage(() =>
        {
            ++runCnt;
            return !(runCnt > 5 && (sw.ElapsedMilliseconds > 5000 || runCnt > 100));
        }, doc);

        sw.Stop();

        if (snapshot == null) return;

        // if (Config.UseEtoWhenPossible)
        {
            PersistentEtoForm<UI.EtoForms.FormPerformanceAnalyzer>.ShowSeparated(
            () => UI.EtoForms.FormPerformanceAnalyzer.CreateFromSnapshot(
                snapshot,
                list.Select(obj => obj.InstanceGuid)));

            return;
        }

        //PersistentForm<FormPerformanceAnalyzer>.ShowSeparated(
        //() => FormPerformanceAnalyzer.CreateFromSnapshot(
        //    snapshot,
        //    list.Select(obj => obj.InstanceGuid)));
    }

    //private static void ShowTextReport(GH_Document doc, PerformanceSnapshot snapshot, IEnumerable<Guid> focused = null)
    //{
    //    var guids = focused == null ? null : new HashSet<Guid>(focused);
    //    var baseData = focused == null ? snapshot.ElapsedByComponent : snapshot.ElapsedByComponent.Where(kv => guids.Contains(kv.Key));
    //    var sb = new StringBuilder();

    //    sb.AppendLine($"Total time: {snapshot.TotalMilliseconds} ms");
    //    sb.AppendLine();

    //    var sum = 0;
    //    var maxLength = 0;

    //    foreach (var it in baseData
    //        .Where(kv => kv.Value > 5)
    //        .OrderByDescending(kv => kv.Value))
    //    {
    //        var obj = doc.FindObject(it.Key, true);
    //        if (obj is null) continue;

    //        var digitLength = it.Value.ToString().Length;
    //        if (digitLength > maxLength)
    //            maxLength = digitLength;

    //        sb.Append("    ");
    //        if (digitLength < maxLength)
    //            for (var i = 0; i < maxLength - digitLength; i++)
    //                sb.Append(" ");

    //        sb.Append($"{it.Value} ms: ");
    //        sb.AppendLine(obj.Name);

    //        sum += it.Value;
    //    }

    //    if (focused != null)
    //    {
    //        sb.AppendLine();
    //        sb.AppendLine($"Total of selected components: {sum} ms, {100.0 * sum / snapshot.TotalMilliseconds:0.00}%");
    //    }

    //    Presenter.ShowReportWindow(sb.ToString());
    //}
}
