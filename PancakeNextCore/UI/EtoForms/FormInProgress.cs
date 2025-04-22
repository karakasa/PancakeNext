using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;
internal sealed class FormInProgress : Form
{
    public FormInProgress()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        ClientSize = new Size(300, 100);
        Title = "Please wait...";
        Maximizable = false;
        Closeable = false;

        var progressBar = new ProgressBar
        {
            Indeterminate = true
        };
        var label = new Label
        {
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "Pancake is collecting data...\r\nSolution is running in background..."
        };

        Content = new TableLayout
        {
            Spacing = new Size(5, 5),
            Padding = new Padding(10),
            Rows =
            {
                progressBar,
                label
            }
        };
    }
}
