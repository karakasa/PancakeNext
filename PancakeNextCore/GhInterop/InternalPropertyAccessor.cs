using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.ErrorFeedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GhInterop;

internal static class InternalPropertyAccessor
{
    private static bool _failed = false;
    private static PropertyInfo _nomen = null;
    private static PropertyInfo _access = null;
    private static void EnsureReflection()
    {
        if (_failed) return;

        _nomen = typeof(DocumentObject)
            .GetProperty("Nomen", BindingFlags.Instance | BindingFlags.Public);

        _access = typeof(AbstractParameter)
            .GetProperty("Access", BindingFlags.Instance | BindingFlags.Public);

        if (_nomen is null || _access is null)
            _failed = true;

        if (_failed)
        {
            PopupErrorMessage();
        }
    }

    private static void PopupErrorMessage()
    {
        var err = new Error("Pancake fail to load.")
        {
            Explanation = "[Reflection error] Nomen"
        };
        err.ShowModal();
    }

    public static bool SetNomenByReflection(this DocumentObject docObj, Nomen newName)
    {
        EnsureReflection();
        if (_failed) return false;

        _nomen.SetValue(docObj, newName);

        return true;
    }
    public static bool SetAccessByReflection(this AbstractParameter docObj, Access newAccess)
    {
        EnsureReflection();
        if (_failed) return false;

        _access.SetValue(docObj, newAccess);

        return true;
    }
}
