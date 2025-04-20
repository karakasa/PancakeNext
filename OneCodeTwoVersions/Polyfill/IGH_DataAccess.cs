using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_DataAccess
{
    /// <summary>
	/// Gets the current iteration count. 
	/// The first time the SolveInstance() function is called on a component 
	/// during a solution the Iteration counter will be zero. It will be 
	/// incremented by one for every subsequent call.
	/// </summary>
	/// <returns>An integer representing the iteration count.</returns>
	int Iteration { get; }

    /// <summary>
    /// Call this method if you wish to stop solving this component.
    /// </summary>
    void AbortComponentSolution();

    /// <summary>
    /// Stores data in an output parameter during GH_Component.SolveInstance(). 
    /// Use this function only for setting individual data items. 
    /// If you want to set lists of data, you *must* call SetDataList() instead.
    /// </summary>
    /// <param name="paramIndex">Index of the output parameter at which to store the data.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    bool SetData(int paramIndex, object data);

    /// <summary>
    /// Expert user function. Stores data in an output parameter during GH_Component.SolveInstance(). 
    /// Use this function only for setting individual data items. 
    /// If you want to set lists of data, you *must* call SetDataList() instead.
    /// </summary>
    /// <param name="paramIndex">Index of the output parameter at which to store the data.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    /// <param name="itemIndexOverride">Typically the item index is a result of Data matching logic. 
    /// If you want to override the item index, then call this function.</param>
    bool SetData(int paramIndex, object data, int itemIndexOverride);

    /// <summary>
    /// Stores data in an output parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <param name="paramName">Name of the output parameter at which to store the data.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    /// <remarks>Parameter access by index is faster, consider using the other overload if possible.</remarks>
    bool SetData(string paramName, object data);

    /// <summary>
    /// Stores a list of data in an output parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <param name="paramIndex">Index of the output parameter at which to store the list.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    bool SetDataList(int paramIndex, IEnumerable data);

    /// <summary>
    /// Expert user function. Stores a list of data in an output parameter 
    /// during GH_Component.SolveInstance().
    /// </summary>
    /// <param name="paramIndex">Index of the output parameter at which to store the list.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    /// <param name="listIndexOverride">Typically the branch index is a result of Data matching logic. 
    /// If you want to override the branch index, then call this function.</param>
    bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride);

    /// <summary>
    /// Stores data in an output parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <param name="paramName">Name of the parameter at which to store the data.</param>
    /// <param name="data">Data to store. Data will be converted if necessary, 
    /// but it will not be duplicated automatically.</param>
    /// <remarks>Parameter access by index is faster, consider using the other overload if possible.</remarks>
    bool SetDataList(string paramName, IEnumerable data);

    /// <summary>
    /// Folds a utility tree representation into this tree.
    /// </summary>
    /// <param name="paramIndex">Index of output parameter to merge with.</param>
    /// <param name="tree">Tree to merge</param>
    // bool SetDataTree(int paramIndex, IGH_DataTree tree);

    /// <summary>
    /// Folds a utility tree representation into this tree.
    /// </summary>
    /// <param name="paramIndex">Index of output parameter to merge with.</param>
    /// <param name="tree">Tree to merge</param>
    // bool SetDataTree(int paramIndex, IGH_Structure tree);

    /// <summary>
    /// Retrieve data from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. 
    /// If the data type does not match the parameter type, 
    /// a conversion will be attempted.</typeparam>
    /// <param name="Index">Index of input parameter from which to retrieve data.</param>
    /// <param name="Destination">Destination instance of type T in which data will be stored if successful. 
    /// Destination can be a null reference.</param>
    /// <returns>True on success, false on failure.</returns>
    bool GetData<T>(int index, ref T destination);

    /// <summary>
    /// Retrieve data from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. 
    /// If the data type does not match the parameter type, 
    /// a conversion will be attempted.</typeparam>
    /// <param name="Name">Name of input parameter from which to retrieve data.</param>
    /// <param name="Destination">Destination instance of type T in which data will be stored if successful. 
    /// Destination can be a null reference.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>Parameter access by index is faster, consider using the other overload if possible.</remarks>
    bool GetData<T>(string name, ref T destination);

    /// <summary>
    /// Retrieve an entire list of data from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. 
    /// If the data type does not match the parameter type, 
    /// a conversion will be attempted.</typeparam>
    /// <param name="Index">Index of input parameter from which to retrieve data.</param>
    /// <param name="List">The data list will be appended to the supplied List. 
    /// List must not be a null reference.</param>
    /// <returns>True on success, false on failure.</returns>
    bool GetDataList<T>(int index, List<T> list);

    /// <summary>
    /// Retrieve an entire list of data from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. 
    /// If the data type does not match the parameter type, 
    /// a conversion will be attempted.</typeparam>
    /// <param name="Name">Name of input parameter from which to retrieve data.</param>
    /// <param name="List">The data list will be appended to the supplied List. 
    /// List must not be a null reference.</param>
    /// <returns>True on success, false on failure.</returns>\
    /// <remarks>Parameter access by index is faster, consider using the other overload if possible.</remarks>
    bool GetDataList<T>(string name, List<T> list);

    /// <summary>
    /// Retrieve an entire data tree from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. Must be identical to the type stored in the parameter.</typeparam>
    /// <param name="Index">Index of input parameter from which to retrieve data.</param>
    /// <param name="Tree">The data tree target. The current value will be overwritten, you should probably pass in a null.</param>
    /// <returns>True on success, false on failure.</returns>
    // bool GetDataTree<T>(int index, out GH_Structure<T> tree) where T : IGH_Goo;

    /// <summary>
    /// Retrieve an entire data tree from an input parameter during GH_Component.SolveInstance().
    /// </summary>
    /// <typeparam name="T">Type of the data to retrieve. Must be identical to the type stored in the parameter.</typeparam>
    /// <param name="Name">Name of input parameter from which to retrieve data.</param>
    /// <param name="Tree">The data tree target. The current value will be overwritten, you should probably pass in a null.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>Parameter access by index is faster, consider using the other overload if possible.</remarks>
    // bool GetDataTree<T>(string name, out GH_Structure<T> tree) where T : IGH_Goo;
}
