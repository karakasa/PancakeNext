using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;

internal class ReportUi : Form
{
    private TextArea _textAreaReport;
    public ReportUi()
    {
        InitializeComponents();
    }

    [MemberNotNull(nameof(_textAreaReport))]
    private void InitializeComponents()
    {
        Title = "Report";
        ClientSize = new Size(600, 400);

        _textAreaReport = new TextArea
        {
            ReadOnly = true
        };

        Content = new TableLayout
        {
            Padding = new Padding(10),
            Rows =
            {
                new TableRow(_textAreaReport)
            }
        };
    }

    public void SetContent(string content, bool wrap)
    {
        _textAreaReport.Wrap = wrap;
        _textAreaReport.Text = content;
        _textAreaReport.CaretIndex = 0;
    }
}
