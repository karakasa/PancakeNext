﻿using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using Rhino.Runtime.RhinoAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components;
public abstract partial class PancakeComponent
{
    protected virtual string InputPanelCategoryName { get; } = "Adjustments";
    public override void AppendToInputPanel(InputPanel panel)
    {
        base.AppendToInputPanel(panel);

        if (SimpleOptions.Length > 0)
        {
            AppendToInputPanel(panel, InputPanelCategoryName, SimpleOptions);
        }
    }

    protected void AppendToInputPanel(InputPanel panel, string name, InputOption[][] options)
    {
        using var _ = panel.BeginCategory(name);
        foreach (var barItems in options)
        {
            var bar = panel.AddBar(true, []);
            foreach (var barItem in barItems)
            {
                barItem.AppendToBar(this, bar);
            }
        }
    }

    protected abstract class InputOption
    {
        public virtual void AppendToBar(PancakeComponent comp, Bar bar)
        {
            bar.Add(Create(comp));
        }
        public virtual BarItem Create(PancakeComponent comp)
        {
            throw new Exception();
        }
    }

    protected sealed class TextOption(string name, string desc, string intialContent, Action<string>? setter, string placeholder = "", string? chapterName = "") : InputOption
    {
        public IIcon? Icon { get; set; }
        public string Name { get; set; } = name;
        public string Description { get; set; } = desc;
        public string InitialContent { get; set; } = intialContent;
        public string Placeholder { get; set; } = placeholder;
        public Action<string>? Setter { get; set; } = setter;
        public Action<string>? TextChanged { get; set; }
        public string? ChapterName { get; set; } = chapterName;
        public override BarItem Create(PancakeComponent comp)
        {
            var text = new TextField(Icon, new Grasshopper2.UI.Nomen(Name, Description, ChapterName), InitialContent)
            {
                Placeholder = Placeholder
            };

            if (Setter is not null)
                text.EnterPressed = () =>
                {
                    Setter(text.Text.Text);
                    comp.ExpireSolution();
                };
            if (TextChanged is not null)
                text.TextChanged += (_, e) => TextChanged(e);
            return text;
        }
    }

    protected sealed class ToggleOption(string name, string desc, bool initialValue, Action<bool> setter, string onText, string offText, string? chapterName = null) : InputOption
    {
        public IIcon? Icon { get; set; }
        public string Name { get; set; } = name;
        public string Description { get; set; } = desc;
        public bool InitialValue { get; set; } = initialValue;
        public Action<bool> Setter { get; set; } = setter;
        public Color? OnColor { get; set; }
        public Color? OffColor { get; set; }
        public string OnText { get; set; } = onText;
        public string OffText { get; set; } = offText;
        public string? ChapterName { get; set; } = chapterName;
        public override BarItem Create(PancakeComponent comp)
        {
            var toggle = new RadioToggle(Icon, new Grasshopper2.UI.Nomen(Name, Description, ChapterName), InitialValue, v =>
            {
                Setter(v);
                comp.ExpireSolution();
            })
            {
                OffColour = OffColor,
                OnColour = OnColor,
                OnText = OnText,
                OffText = OffText
            };

            toggle.SetSizeLimits(SharedOptionWidth, 1000);

            return toggle;
        }
    }

    protected class Pick<T>(string name, T value, string nomenName, string nomenDesc, IIcon? icon = null)
    {
        public string Name { get; } = name;
        public T Value { get; } = value;
        public IIcon? Icon { get; } = icon;
        public string NomenName { get; } = nomenName;
        public string NomenDesc { get; } = nomenDesc;
    }

    protected sealed class PickOneOption<T>(string sectionName, T initialValue, Action<T> setter, Pick<T>[] values) : InputOption
    {
        public T InitialValue { get; set; } = initialValue;
        public Action<T> Setter { get; set; } = setter;
        public string SectionName { get; set; } = sectionName;
        public Pick<T>[] Values { get; set; } = values;
        public override void AppendToBar(PancakeComponent comp, Bar bar)
        {
            foreach(var pick in Values)
            {
                var val = pick.Value;
                var valName = pick.Name;

                bool check;
                if (val is IEquatable<T> equtable)
                {
                    check = equtable.Equals(InitialValue);
                }
                else
                {
                    check = val?.Equals(InitialValue) ?? false;
                }

                var toggle = new RadioToggle(pick.Icon, new(pick.NomenName, pick.NomenDesc, SectionName, SectionName), check, v =>
                {
                    if (v)
                    {
                        Setter(val);
                        comp.ExpireSolution();
                    }
                })
                {
                    OnText = valName,
                    OffText = valName,
                };
                bar.Add(toggle);
            }
        }
    }

    protected sealed class ButtonOption(string name, string desc, Action action) : InputOption
    {
        public IIcon? Icon { get; set; }
        public string Name { get; set; } = name;
        public string Description { get; set; } = desc;
        public Action Action { get; set; } = action;
        public Color? OnColor { get; set; } = Colors.White;
        public string? ChapterName { get; set; }
        public override BarItem Create(PancakeComponent comp)
        {
            var toggle = new PushButton(Icon, new Grasshopper2.UI.Nomen(Name, Description, ChapterName), Action);
            toggle.OnColour = toggle.OffColour = OnColor;

            return toggle;
        }
    }

    protected sealed class SpacerOption : InputOption
    {
        public override void AppendToBar(PancakeComponent comp, Bar bar)
        {
            bar.AddSpacer();
        }
    }

    protected virtual InputOption[][] SimpleOptions { get; } = [];
}
