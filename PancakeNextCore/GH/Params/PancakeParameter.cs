using Grasshopper2.Components;
using Grasshopper2.Interop;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Dataset;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public abstract class PancakeParameter<TContent, TParameter> : Parameter<TContent>
    where TParameter : IPancakeLocalizable<TParameter>
{
    public PancakeParameter(IReader reader) : base(reader)
    {
        // ReadVersion(reader);

        ProcessRuntimeModifier();
        HandleLocalizationForRestored();

        ReadConfig();
    }

    public PancakeParameter() : this(Access.Tree)
    {
    }

    public PancakeParameter(Access access) : base(CreateNomen(), access)
    {
        ProcessRuntimeModifier();
        HandleLocalizationForNewlyCreated();

        ReadConfig();
    }

    public PancakeParameter(Nomen nomen, Access access) : base(nomen, access)
    {
        ProcessRuntimeModifier();
        HandleLocalizationForNewlyCreated();

        ReadConfig();
    }

#if NET
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<TParameter>();
    }
#else
    private static string? CachedLocalizedName;
    private static string? CachedLocalizedDescription;
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<TParameter>(ref CachedLocalizedName, ref CachedLocalizedDescription);
    }
#endif

    public virtual string LocalizedName { get; private set; }

    public virtual string LocalizedDescription { get; private set; }

    private const string SettingLastSaveLocal = "LastSaveLocalization";
    public string LastSaveLocalization => CustomValues.Get(SettingLastSaveLocal, null);

    private void SaveCurrentLangIfNot()
    {
        if (string.IsNullOrEmpty(LastSaveLocalization))
        {
            CustomValues.Set(SettingLastSaveLocal, GlobalizationResolver.CurrentLanguageTraits);
        }
    }
    protected void FailInUntrusted(IDataAccess? access = null)
    {
        if (!Config.IsInUntrustedMode) return;

        Document?.Solution?.Stop();
        access?.AddError("Access violation", "This feature is disabled in the untrusted mode.");
        throw new InvalidOperationException();
    }

    protected virtual void ReadConfig() { }

    protected void RefreshLocalizationAppearance(Nomen expected)
    {
        ModifyNameAndInfo(expected.Name, expected.Info);
    }

    public void RefreshLocalizationAppearance()
    {
        ModifyNameAndInfo(LocalizedName, LocalizedDescription);
    }

    private bool _isSupported = true;
    private string? _versionRequirementString;

    private void ProcessRuntimeModifier()
    {
        var type = GetType();
        var depth = 0;

        while (type != typeof(AbstractParameter) && type is not null && depth < 16)
        {
            ReadAttributes(type);
            type = type.BaseType;
            ++depth;
        }
    }

    [MemberNotNull(nameof(LocalizedDescription), nameof(LocalizedName))]
    private void HandleLocalizationForNewlyCreated()
    {
        SaveCurrentLangIfNot();

        LocalizedName = Nomen.Name;
        LocalizedDescription = Nomen.Info;
    }

    [MemberNotNull(nameof(LocalizedDescription), nameof(LocalizedName))]
    private void HandleLocalizationForRestored()
    {
        var expectedNomen = CreateNomen();

        LocalizedName = expectedNomen.Name;
        LocalizedDescription = expectedNomen.Info;

        RefreshLocalizationAppearance(expectedNomen);
        SaveCurrentLangIfNot();
    }

    private void ReadAttributes(Type type)
    {
        foreach (var it in type.GetCustomAttributes(false))
        {
            switch (it)
            {
                case MinimalVersionAttribute ver when _isSupported:
                    _isSupported = ver.SatisfyRequirement();
                    if (!_isSupported)
                    {
                        _versionRequirementString = ver.GetAnticipatedVersion();
                    }
                    break;
            }
        }
    }
}
