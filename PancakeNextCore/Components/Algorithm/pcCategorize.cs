using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Pancake.Attributes;
using Pancake.Utility;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("data", 0)]
public class pcCategorize : PancakeComponent
{
    protected override void RegisterInputs()
    {
        AddParam("key", GH_ParamAccess.list);
        AddParam("value", GH_ParamAccess.list);
    }

    protected override void RegisterOutputs()
    {
        AddParam("keylist", GH_ParamAccess.list);
        AddParam("valuetree", GH_ParamAccess.tree);
    }

    private List<object> _keyList = new List<object>();
    private List<object> _valList = new List<object>();

    private void ClearList()
    {
        _keyList.Clear();
        _valList.Clear();
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

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        ClearList();

        var _tempKeyList = new List<object>();
        DA.GetDataList(0, _tempKeyList);
        DA.GetDataList(1, _valList);

        if (_tempKeyList.Count != _valList.Count)
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.KeyAndValueListMustHaveTheSameLength);
            return;
        }

        if (_tempKeyList.Count == 0)
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.TheListCannotBeEmpty);
            return;
        }

        _keyList.AddRange(_tempKeyList.Select(GooHelper.UnwrapIfPossible));
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
        DA.SetDataTree(1, valsOut);
    }

    internal class ComparerOfIComparable : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            return ((IComparable)x).CompareTo(y);
        }
    }

    private void SortKeyValues(out List<object> keysOut, out IGH_DataTree valsOut, bool sortIfPossible = true)
    {
        IDictionary<object, List<object>> currentDict = null;

        if (sortIfPossible && IsAllIComparable())
        {
            currentDict = new SortedDictionary<object, List<object>>(new ComparerOfIComparable());
        }
        else
        {
            currentDict = new Dictionary<object, List<object>>();
        }

        for (var i = 0; i < _keyList.Count; i++)
        {
            List<object> list;

            if (!currentDict.TryGetValue(_keyList[i], out list))
                currentDict[_keyList[i]] = list = new List<object>();

            list.Add(_valList[i]);
        }

        keysOut = currentDict.Keys.ToList();

        var tree = new DataTree<object>();
        var branchIndex = 0;

        foreach (var it in currentDict.Values)
        {
            tree.AddRange(it, new GH_Path(branchIndex));
            branchIndex++;
        }

        valsOut = tree;

        currentDict.Clear();
        currentDict = null;
    }

    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.Categorize;

    public override Guid ComponentGuid => new Guid("b521157b-0ed0-4229-940a-7c9c2d9357ee");

    public override string LocalizedName => Strings.Categorize;

    public override string LocalizedDescription => Strings.CategorizeValuesByKeys;

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