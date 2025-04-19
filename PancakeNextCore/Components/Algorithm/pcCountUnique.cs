using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Pancake.Attributes;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("data", 0)]
public class pcCountUnique : PancakeComponent
{
    public override string LocalizedName => Strings.CountUnique;
    public override string LocalizedDescription => Strings.ExtractAllUniqueValuesFromAListAndCountTheirOccurences;
    protected override void RegisterInputs()
    {
        AddParam("key", GH_ParamAccess.list);
    }
    protected override void RegisterOutputs()
    {
        AddParam("keylist", GH_ParamAccess.list);
        AddParam<Param_Integer>("count", GH_ParamAccess.list);
    }

    private List<object> _keyList = new List<object>();

    private void ClearList()
    {
        _keyList.Clear();
    }

    private bool HasReferenceType()
    {
        return _keyList.Any(e => !(e is ValueType) && !(e is string));
    }

    private bool HasNull()
    {
        return _keyList.Any(e => e == null);
    }

    private bool IsAllIComparable()
    {
        if (_keyList.Count == 0)
            return false;

        if (!(_keyList[0] is IComparable))
            return false;

        var type = _keyList[0].GetType();
        for (var i = 1; i < _keyList.Count; i++)
        {
            if (!_keyList[i].GetType().Equals(type) || !(_keyList[i] is IComparable))
                return false;
        }

        return true;
    }

    private object UnwrapGhGoo(object rawObj)
    {
        if (!(rawObj is IGH_Goo goo))
            return rawObj;

        var obj = goo.ScriptVariable();

        return obj ?? rawObj;
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        ClearList();

        var _tempKeyList = new List<object>();
        DA.GetDataList(0, _tempKeyList);

        if (_tempKeyList.Count == 0)
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.TheListCannotBeEmpty);
            return;
        }

        _keyList.AddRange(_tempKeyList.Select(UnwrapGhGoo));
        _tempKeyList.Clear();

        if (HasNull())
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.KeyCannotBeNull);
            return;
        }

        if (HasReferenceType())
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                Strings.SomeOfTheKeysAreNotValueTypeTheResultMayBeWrongCheckCarefully);
        }

        SortKeyValues(out var keysOut, out var valsOut, _sortIfPossible);

        DA.SetDataList(0, keysOut);
        DA.SetDataList(1, valsOut);
    }

    internal class ComparerOfIComparable : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            return ((IComparable)x).CompareTo(y);
        }
    }

    private void SortKeyValues(out List<object> keysOut, out List<int> valsOut, bool sortIfPossible = true)
    {
        IDictionary<object, int> currentDict = null;

        if (sortIfPossible && IsAllIComparable())
        {
            currentDict = new SortedDictionary<object, int>(new ComparerOfIComparable());
        }
        else
        {
            currentDict = new Dictionary<object, int>();
        }

        for (var i = 0; i < _keyList.Count; i++)
        {
            if (currentDict.ContainsKey(_keyList[i]))
            {
                currentDict[_keyList[i]]++;
            }
            else
            {
                currentDict[_keyList[i]] = 1;
            }
        }

        keysOut = currentDict.Keys.ToList();
        valsOut = currentDict.Values.ToList();

        currentDict.Clear();
        currentDict = null;
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon
    {
        get
        {
            //You can add image files to your project resources and access them like this:
            // return Resources.IconForThisComponent;
            return ComponentIcon.CountUnique;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("b521157b-0ed0-5229-940a-7c9c2d9357ee"); }
    }

    private const string SortIfPossibleConfigName = "SortIfPossible";
    private bool _sortIfPossible = false;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        var mnu = Menu_AppendItem(menu, Strings.SortKeysIfPossible, MnuSortIfPossible, true, _sortIfPossible);
        mnu.ToolTipText = Strings.KeysWillBeSortedIfAllAreComparableAndBelongToASameTypeWhichWillSlowTheComputation;
        base.AppendAdditionalMenuItems(menu);
    }

    private void MnuSortIfPossible(object sender, EventArgs e)
    {
        _sortIfPossible = !_sortIfPossible;
        ExpireSolution(true);
    }

    public override bool Read(GH_IReader reader)
    {
        reader.TryGetBoolean(SortIfPossibleConfigName, ref _sortIfPossible);
        return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer)
    {
        writer.SetBoolean(SortIfPossibleConfigName, _sortIfPossible);
        return base.Write(writer);
    }
}