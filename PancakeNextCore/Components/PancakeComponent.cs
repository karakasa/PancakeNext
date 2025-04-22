using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using PancakeNextCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Canvas;
using System.Diagnostics.CodeAnalysis;
using PancakeNextCore.GH;
using System.Reflection;

namespace PancakeNextCore.Components;

public abstract partial class PancakeComponent : Component, IPancakeLocalizable
{
    protected PancakeComponent(Nomen nomen) : base(nomen)
    {
        ProcessRuntimeModifier();
        HandleLocalizationForNewlyCreated();

        ReadConfig();
    }
    protected PancakeComponent(IReader reader) : base(reader)
    {
        ReadVersion(reader);

        ProcessRuntimeModifier();
        HandleLocalizationForRestored();

        ReadConfig();
    }


    protected override void AddInputs(InputAdder inputs)
    {
        _currentInputManager = inputs;
        RegisterInputs();
        _currentInputManager = null;
    }

    protected override void AddOutputs(OutputAdder outputs)
    {
        _currentOutputManager = outputs;
        RegisterOutputs();
        _currentOutputManager = null;
    }
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

        while (type != typeof(PancakeComponent) && type is not null && depth < 16)
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
    protected abstract void HandleLocalizationForRestored();

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

    public string LocalizedName { get; protected set; }
    public string LocalizedDescription { get; protected set; }
    protected override void PreProcess(Solution solution)
    {
        if (!_isSupported)
        {
            throw new NotSupportedException($"This component requires {_versionRequirementString ?? ""}");
        }

        base.PreProcess(solution);
    }

    private InputAdder? _currentInputManager = null;
    private OutputAdder? _currentOutputManager = null;

    protected abstract void RegisterInputs();
    protected abstract void RegisterOutputs();
    private static T CreateLocalizedParameter<T>(
        string identifier,
        Access access,
        Requirement requirement
        )
        where T : AbstractParameter, new()
    {
        if (!ComponentLibrary.LookupLocalizedParamInfo(identifier,
            out var name, out var nickname, out var desc))
        {
            // The identifier doesn't exist. Shouldn't happen.
            name = nickname = desc = identifier;
            IssueTracker.ReportInPlace($"Unknown identifier during param creation: {identifier}");
        }

        return AccessWrapper.CreateParam<T>(name, nickname, desc, access, requirement);
    }
    protected TParam AddParam<TParam>(
        string identifier,
        Access access = Access.Item,
        Requirement requirement = Requirement.MustExist
        )
        where TParam : AbstractParameter, new()
    {
        EnsureDuringCreationPeriod();

        var paramToAdd = CreateLocalizedParameter<TParam>(identifier, access, requirement);

        if (_currentInputManager != null)
            Parameters.AddInput(paramToAdd);
        else
            Parameters.AddOutput(paramToAdd);

        return paramToAdd;
    }

    protected TParam AddParam<TParam, TValue>(
        string identifier,
        TValue defValue,
        Access access = Access.Item,
        Requirement requirement = Requirement.MustExist
        )
        where TParam : Parameter<TValue>, new()
    {
        EnsureDuringCreationPeriod();

        var paramToAdd = CreateLocalizedParameter<TParam>(identifier, access, requirement);

        paramToAdd.Set(defValue);

        if (_currentInputManager != null)
            Parameters.AddInput(paramToAdd);
        else
            Parameters.AddOutput(paramToAdd);

        return paramToAdd;
    }
    private void EnsureDuringCreationPeriod()
    {
        if (_currentInputManager == null && _currentOutputManager == null)
            IssueTracker.ReportAndThrow("PancakeComponent.AddParam is called outside param creation period");
    }

    protected IntegerParameter AddParam(string identifier, int defValue, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        => AddParam<IntegerParameter, int>(identifier, defValue, access, requirement);

    protected TextParameter AddParam(string identifier, string defValue, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        => AddParam<TextParameter, string>(identifier, defValue, access, requirement);

    protected BooleanParameter AddParam(string identifier, bool defValue, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        => AddParam<BooleanParameter, bool>(identifier, defValue, access, requirement);

    protected NumberParameter AddParam(string identifier, double defValue, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        => AddParam<NumberParameter, double>(identifier, defValue, access, requirement);

    protected GenericParameter AddParam(string identifier, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        => AddParam<GenericParameter>(identifier, access, requirement);

    private const string SettingLastSaveLocal = "LastSaveLocalization";

    public string? LastSaveLocalization => GetValue(SettingLastSaveLocal, default(string));

    protected void SaveCurrentLangIfNot()
    {
        if (string.IsNullOrEmpty(LastSaveLocalization))
        {
            SetValue(SettingLastSaveLocal, GlobalizationResolver.CurrentLanguageTraits);
        }
    }

    protected string? GetValue(string settingName, string? defaultValue)
        => CustomValues.Get(settingName, defaultValue);
    protected void SetValue(string settingName, string? strValue)
        => CustomValues.Set(settingName, strValue);
    protected bool GetValue(string settingName, bool defaultValue)
        => CustomValues.Get(settingName, defaultValue);
    protected void SetValue(string settingName, bool boolValue)
        => CustomValues.Set(settingName, boolValue);
    public Version? LastSaveVersion { get; private set; }
    private const string CfgSaveVersion = "LastSaveVersion";
    protected bool IsNewlyCreated { get; private set; } = true;
    public override void Store(IWriter writer)
    {
        base.Store(writer);

        writer.String(CfgSaveVersion, PancakeInfo.CoreVersion?.ToString() ?? "unknown");
    }

    private void ReadVersion(IReader reader)
    {
        IsNewlyCreated = false;

        var version = reader.String(CfgSaveVersion);

        if (string.IsNullOrEmpty(version))
        {
            LastSaveVersion = null;
        }
        else
        {
            if (Version.TryParse(version, out var ver))
                LastSaveVersion = ver;
        }
    }

    protected void FailInUntrusted(IDataAccess? access = null)
    {
        if (!Config.IsInUntrustedMode) return;

        Document?.Solution?.Stop();
        access?.AddError("Access violation", "This feature is disabled in the untrusted mode.");
        throw new InvalidOperationException();
    }

    protected void ExpireSolution()
    {
        Expire();
        base.Document?.Solution.Start();
    }

    protected virtual void ReadConfig()
    {

    }
}

public abstract class PancakeComponent<T> : PancakeComponent
    where T : IPancakeLocalizable<T> 
    // Use CRTP to circumvent Nomen creation issues from localized names
{

    protected PancakeComponent() : base(CreateNomen())
    {
    }
    protected PancakeComponent(IReader reader) : base(reader)
    {
    }

#if NET
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<T>();
    }
#else
    private static string? CachedLocalizedName;
    private static string? CachedLocalizedDescription;
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<T>(ref CachedLocalizedName, ref CachedLocalizedDescription);
    }
#endif

    protected override void HandleLocalizationForRestored()
    {
        var expectedNomen = CreateNomen();

        LocalizedName = expectedNomen.Name;
        LocalizedDescription = expectedNomen.Info;

        RefreshLocalizationAppearance(expectedNomen);
        SaveCurrentLangIfNot();
    }
}