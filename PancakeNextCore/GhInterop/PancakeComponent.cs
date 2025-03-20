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
using PancakeNext.Dataset;
using Grasshopper2.UI.Icon;

namespace PancakeNextCore.GhInterop;

public abstract class PancakeComponent : Component, IPancakeLocalizable
{
    private static readonly Nomen EmptyNomen = Nomen.Empty;
    protected PancakeComponent() : base(EmptyNomen)
    {
        PopulateInfoFromAttributes();
        var name = CreateNameFromMetadata();
        this.SetNomenByReflection(name);
    }

    protected PancakeComponent(IReader reader) : base(reader)
    {
        ReadVersion(reader);
    }

    private Nomen CreateName()
    {

    }

    private void PopulateInfoFromAttributes()
    {
        var type = GetType();

        while (type != typeof(PancakeComponent))
        {
            ReadAttributes(type);
            type = type.BaseType;
        }

        if (_subPanelIndex == -1)
        {
            // TODO
            // Debug.WriteLine("missing: " + GetType().Name + " no subpanel info.");
        }

        RefreshLocalizationAppearance();
        SaveCurrentLangIfNot();
    }
    private Nomen CreateNameFromMetadata()
    {
        var rank = ShouldBeVisible ? DisplayRank : Rank.Hidden;

        return new Nomen(
            LocalizedName,
            LocalizedDescription,
            CategoryName,
            _sectionName,
            _subPanelIndex,
            rank);
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

    public const string CategoryName = "Pancake";

    public void RefreshLocalizationAppearance()
    {
        //CopyFrom(new GH_InstanceDescription(DisplayName,
          //  LocalizedNickname, DisplayDescription, CategoryName, _sectionName));
    }

    private bool _isSupported = true;
    private string _sectionName = "";
    private int _subPanelIndex = -1;

    [Obsolete]
    public virtual string LocalizedNickname { get => GetType().Name; }

    private void ReadAttributes(Type type)
    {
        foreach (var it in type.GetCustomAttributes(false))
        {
            switch (it)
            {
                case MinimalVersionAttribute ver when _isSupported:
                    _isSupported = ver.SatisfyRequirement();
                    break;
                case ComponentCategoryAttribute comp when _subPanelIndex == -1:
                    _sectionName = ComponentLibrary.GetCategoryString(comp.SectionName);
                    _subPanelIndex = comp.SubPanelIndex;
                    break;
            }
        }
    }

    public abstract string LocalizedName { get; }
    public abstract string LocalizedDescription { get; }

    protected virtual Rank DisplayRank { get; } = Rank.Normal;
    private bool ShouldBeVisible => _isSupported && !Obsolete && (!DebugOnly || Config.DevMode);
    protected virtual bool DebugOnly => false;
    protected override void PreProcess(Solution solution)
    {
        if (!_isSupported)
        {
            throw new NotSupportedException("Version not supported.");
        }

        base.PreProcess(solution);
    }

    private InputAdder _currentInputManager = null;
    private OutputAdder _currentOutputManager = null;

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
            out string name, out string nickname, out string desc))
        {
            // The identifier doesn't exist. Shouldn't happen.
            name = nickname = desc = identifier;
            IssueTracker.ReportInPlace($"Unknown identifier during param creation: {identifier}");
        }

        var param = new T
        {
            Requirement = requirement,
            UserName = nickname
        };

        param.ModifyNameAndInfo(name, desc);

        // Access doesn't have a public setter (designed to be set by non-default ctor)
        // Find out a better way to change Access.
        param.SetAccessByReflection(access);

        return param;
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

    [Obsolete]
    protected IParameter LastAddedParameter
    {
        get
        {
            EnsureDuringCreationPeriod();

            if (_currentInputManager != null)
                return Parameters.Input(Parameters.InputCount - 1);
            else
                return Parameters.Output(Parameters.OutputCount - 1);
        }
    }

    [Obsolete]
    public virtual IEnumerable<string> LocalizedKeywords => null;

    private const string SettingLastSaveLocal = "LastSaveLocalization";

    public string LastSaveLocalization 
        => GetValue(SettingLastSaveLocal, default(string));

    private void SaveCurrentLangIfNot()
    {
        if (string.IsNullOrEmpty(LastSaveLocalization))
        {
            SetValue(SettingLastSaveLocal, GlobalizationResolver.CurrentLanguageTraits);
        }
    }

    protected string GetValue(string settingName, string defaultValue)
        => CustomValues.Get(settingName, defaultValue);
    protected void SetValue(string settingName, string strValue)
        => CustomValues.Set(settingName, strValue);

    public Version LastSaveVersion { get; private set; }
    private const string CfgSaveVersion = "LastSaveVersion";
    protected bool IsNewlyCreated { get; private set; } = true;
    public override void Store(IWriter writer)
    {
        base.Store(writer);

        writer.String(CfgSaveVersion, PluginInfo.CoreVersion?.ToString() ?? "unknown");
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

    protected void FailInUntrusted()
    {
        if (Config.IsInUntrustedMode)
        {
            // TODO

            // AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Access violation. Cannot use this feature in the untrusted mode.");
            // OnPingDocument().RequestAbortSolution();
            throw new InvalidOperationException();
        }
    }

    protected virtual IIcon? ActualIcon { get; }
    protected sealed override IIcon? IconInternal
    {
        get
        {
            PluginLifetime.HandleIconEvent();
            return ActualIcon;
        }
    }
}
