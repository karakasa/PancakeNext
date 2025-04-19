using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("data", 0)]
public class pcCategorizeByInd : PancakeComponent
{
    public override string LocalizedName => Strings.CategorizeByIndex;
    public override string LocalizedDescription => Strings.CategorizeValuesByIntegerKeysComparedToTheGenericVersionThisComponentHasBetterPerformance;
    protected override void RegisterInputs()
    {
        AddParam<Param_Integer>("key", GH_ParamAccess.list);
        AddParam("value", GH_ParamAccess.list);
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_Integer>("keylist", GH_ParamAccess.list);
        AddParam("valuetree", GH_ParamAccess.tree);
    }

    private List<int> _keyList = new List<int>();
    private List<object> _valList = new List<object>();

    private void ClearList()
    {
        _keyList.Clear();
        _valList.Clear();
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        ClearList();

        DA.GetDataList(0, _keyList);
        DA.GetDataList(1, _valList);

        if (_keyList.Count != _valList.Count)
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.KeyAndValueListMustHaveTheSameLength);
            return;
        }

        if (_keyList.Count == 0)
        {
            ClearList();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.TheListCannotBeEmpty);
            return;
        }

        SortKeyValues(out var keysOut, out var valsOut);

        DA.SetDataList(0, keysOut);
        DA.SetDataTree(1, valsOut);
    }

    private int _minKey = 0;
    private int _maxKey = 0;
    private const int CacheLimit = 32;
    private const int SmallBucketThreshold = 1024 * 32;

    private bool _disableOptimization = false;

    private void ClearCache()
    {
        foreach (var list in _sortedData.Values) list.Clear();
        _sortedData.Clear();

        _hashedData.ForEach(it => it.Clear());
        _hashedData.Clear();
    }

    private SortedDictionary<int, List<object>> _sortedData = new SortedDictionary<int, List<object>>();
    private List<List<object>> _hashedData = new List<List<object>>();

    private void InitializeListCache(int capacity, int innerCapacity = 0)
    {
        _hashedData.Capacity = capacity;

        for (var i = 0; i < capacity; i++)
        {
            var inner = new List<object>();
            if (innerCapacity != 0)
                inner.Capacity = innerCapacity;
            _hashedData.Add(inner);
        }
    }

    private static int CalculateCacheCapacity(int dataCount, int bucketCount)
    {
        if (dataCount <= SmallBucketThreshold / bucketCount)
        {
            return dataCount;
        }
        return Convert.ToInt32(Math.Ceiling(1.0 * dataCount / bucketCount));
    }

    private void SortKeyValues(out List<int> keysOut, out IGH_DataTree valsOut)
    {
        ClearCache();

        _minKey = _keyList.Min();
        _maxKey = _keyList.Max();

        var cacheSize = _maxKey - _minKey + 1;

        if (!_disableOptimization && cacheSize <= CacheLimit)
        {
            InitializeListCache(cacheSize, CalculateCacheCapacity(_keyList.Count(), cacheSize));

            for (var i = 0; i < _keyList.Count; i++)
            {
                _hashedData[_keyList[i] - _minKey].Add(_valList[i]);
            }

            keysOut = new List<int>();
            var tree = new DataTree<object>();
            var branchIndex = 0;

            for (var i = 0; i < cacheSize; i++)
            {
                if (_hashedData[i].Count != 0)
                {
                    keysOut.Add(i + _minKey);
                    tree.AddRange(_hashedData[i], new GH_Path(branchIndex));
                    branchIndex++;
                }
            }

            valsOut = tree;
        }
        else
        {
            for (var i = 0; i < _keyList.Count; i++)
            {
                List<object> list;

                if (!_sortedData.TryGetValue(_keyList[i], out list))
                    _sortedData[_keyList[i]] = list = new List<object>();

                list.Add(_valList[i]);
            }

            keysOut = _sortedData.Keys.ToList();

            var tree = new DataTree<object>();
            var branchIndex = 0;

            foreach (var it in _sortedData.Values)
            {
                tree.AddRange(it, new GH_Path(branchIndex));
                branchIndex++;
            }

            valsOut = tree;
        }

        ClearCache();
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
            return ComponentIcon.CategorizeByIndex;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("b521157b-0ed0-4229-940a-7c9c2d9357ec"); }
    }

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        var mnu = Menu_AppendItem(menu, Strings.DisableOptimization, MnuDisableOpt_Click, true, _disableOptimization);
        mnu.ToolTipText = Strings.DisablingOptimizationMayReduceMemoryUsage;


        base.AppendAdditionalMenuItems(menu);
    }

    private void MnuDisableOpt_Click(object sender, EventArgs e)
    {
        _disableOptimization = !_disableOptimization;
    }

    private const string DisableOptOptionName = "DisableOptimization";

    public override bool Write(GH_IWriter writer)
    {
        writer.SetBoolean(DisableOptOptionName, _disableOptimization);
        return base.Write(writer);
    }

    public override bool Read(GH_IReader reader)
    {
        reader.TryGetBoolean(DisableOptOptionName, ref _disableOptimization);
        return base.Read(reader);
    }

}