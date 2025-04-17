using Pancake.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI;

public interface IFormPerformanceAnalyzer<TImage>
{
    void BeginVisualize();
    void EndVisualize();
    bool FocusSelected { get; }
    GroupCriteria Grouping { get; }
    int AddEntryImage(TImage img);
    void AddEntry(PerformanceAnalyzerEntry entry);
    void AddSpecialEntry(SpecialEntryType type, PerformanceAnalyzerEntry entry);
}
