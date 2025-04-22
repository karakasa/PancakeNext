using System;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Grasshopper2.UI;
using PancakeNextCore.Dataset;
using System.Diagnostics.CodeAnalysis;

namespace PancakeNextCore.Utility;

public static class PersistentEtoForm
{
    public static void ShowSeparated<T>(Func<T>? customNew = null, Action<T>? customInit = null) where T : Form, new()
    {
        PersistentEtoForm<T>.ShowSeparated(customNew, customInit);
    }

    public static void Show<T>(Func<T>? customNew = null, Action<T>? customInit = null) where T : Form, new()
    {
        PersistentEtoForm<T>.Show(customNew, customInit);
    }

    public static void Close<T>() where T : Form, new()
    {
        PersistentEtoForm<T>.Close();
    }
}
public static class PersistentEtoForm<T> where T : Form, new()
{
    private static readonly WeakReference<T?> _form = new(null);

    public static void Close()
    {
        if (_form.TryGetTarget(out var form) && form != null)
        {
            try
            {
                form.Close();
            }
            catch
            {
            }
            try
            {
                form.Dispose();
            }
            catch
            {
            }

            _form.SetTarget(null);
        }
    }

    public static void Show(Func<T>? customNew = null, Action<T>? customInit = null)
    {
        if (_form.TryGetTarget(out var form) && form != null)
        {
            try
            {
                form.Show();
                form.BringToFront();
                return;
            }
            catch
            {
                try
                {
                    form.Dispose(); // explicitly releasing resources
                }
                catch
                {
                }
                _form.SetTarget(null);
            }
        }

        var editor = Editor.Instance;
        if (editor == null)
            return;

        form = customNew is null ? new T() : customNew();
        _form.SetTarget(form);

        //CenterEtoFormOnCursor(form);
        customInit?.Invoke(form);
        RegisterClosedEvent(form);
        form.Show();
        form.BringToFront();
        //RegisterEtoFormIntoShepard(editor, form);
    }

    public static void ShowSeparated(Func<T>? customNew = null, Action<T>? customInit = null)
    {
        var editor = Editor.Instance;
        if (editor == null)
            return;

        var form = customNew is null ? new T() : customNew();


        //CenterEtoFormOnCursor(form);
        customInit?.Invoke(form);
        RegisterClosedEvent(form);
        form.Show();
        form.BringToFront();
        //RegisterEtoFormIntoShepard(editor, form);
        // editor.FormShepard.RegisterForm(form);
    }

    private static void RegisterClosedEvent(Form form)
    {
        form.Owner = EtoExtensions.GetGrasshopperWindowAsEto();

        form.Closed -= OnFormClosed;
        form.Closed += OnFormClosed;
    }

    private static void OnFormClosed(object? sender, EventArgs e)
    {
        _form.SetTarget(null);
    }

    public static bool TryGet([NotNullWhen(true)] out T? form)
    {
        var result = _form.TryGetTarget(out form);
        if (!result) return false;
        return form is not null;
    }

    public static bool TryShow()
    {
        if (TryGet(out var form))
        {
            try
            {
                form.Show();
                form.BringToFront();
                return true;
            }
            catch
            {
                _form.SetTarget(null);
                return false;
            }
        }

        return false;
    }

    /*
    private static void CenterEtoFormOnCursor(Form form)
    {
        if (Config.IsRunningOnMac)
        {
            CenterEtoFormOnCursorMac(form);
        }
        else
        {
            CenterEtoFormOnCursorWin(form);
        }
    }

    private static void CenterEtoFormOnCursorWin(Form form)
    {
        form.SizeChanged += OnFirstLayout;

        return;

        throw new NotImplementedException();

        // ((Form.IHandler)form.Handler).
    }

    private static void OnFirstLayout(object? sender, EventArgs e)
    {
        if (sender is not Form form)
            return;

        form.SizeChanged -= OnFirstLayout;
        CenterEtoFormOnCursorMac(form);
    }

    private static void CenterEtoFormOnCursorMac(Form form)
    {
        if (form is null || !EnsureReflection2())
            return;

        try
        {
            _centerFormOnCursorMethod.Invoke(null, new object[] { form, true });
        }
        catch
        {
        }
    }

    private static bool RegisterEtoFormIntoShepard(Editor editor, Form form)
    {
        var convertForm = ToSwf(form);
        if (convertForm is null)
            return false;

        editor.FormShepard.RegisterForm(convertForm);
        return true;
    }
    private static MethodInfo _centerFormOnCursorMethod;
    private static bool EnsureReflection2()
    {
        if (_centerFormOnCursorMethod is not null) return true;

        var formUtils = typeof(GH_WindowsFormUtil);
        _centerFormOnCursorMethod = formUtils.GetMethod("CenterFormOnCursor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new Type[] { typeof(Window), typeof(bool) }, Array.Empty<ParameterModifier>());

        return _centerFormOnCursorMethod is not null;
    }

    */
}
