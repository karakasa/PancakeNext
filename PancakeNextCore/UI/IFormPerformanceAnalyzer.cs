using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI;

public interface IFormPerformanceAnalyzer
{
    void BeginVisualize();
    void EndVisualize();
    bool FocusSelected { get; }
    GroupCriteria Grouping { get; }
    int AddEntryImage(Image img);
    void AddEntry(PerformanceAnalyzerEntry entry);
    void AddSpecialEntry(SpecialEntryType type, PerformanceAnalyzerEntry? entry);
}
