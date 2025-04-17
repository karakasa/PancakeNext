using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Pancake.GH.Settings;
using Pancake.Utility;

namespace PancakeNextCore.UI.EtoForms;

internal class PancakeSettingUi : Form
{
    private static IPancakeSettingUi[] _settings;
    static PancakeSettingUi()
    {
        LocatePancakeSettings();
    }
    private static void LocatePancakeSettings()
    {
        _settings = ReflectionHelper.GetEnumerableOfType<IPancakeSettingUi>()
            .Where(ui => ui.Visible)
            .OrderBy(ui => ui.Name)
            .ToArray();
    }
    public PancakeSettingUi()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Title = "Pancake Settings";
        ClientSize = new Size(500, 700);

        var dynamicLayout = new DynamicLayout();

        dynamicLayout.BeginVertical(xscale: true);

        foreach (var setting in _settings)
        {
            var groupBox = new GroupBox
            {
                Text = setting.Name,
                Padding = new Padding(10),
                //Width = ClientSize.Width
            };

            var str = setting.Tooltip;
            if (!string.IsNullOrEmpty(str))
                groupBox.ToolTip = str;

            setting.BuildUi(groupBox);

            dynamicLayout.Add(groupBox);
        }

        dynamicLayout.AddSpace();
        dynamicLayout.EndVertical();

        Content = new Scrollable
        {
            Content = dynamicLayout,
            Padding = new Padding(10),
            ExpandContentWidth = true,
            ExpandContentHeight = false
        };
    }
}
