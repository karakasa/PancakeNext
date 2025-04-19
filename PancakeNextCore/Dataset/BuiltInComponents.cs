using Grasshopper2.UI;
using PancakeNextCore.Components.Io;
using PancakeNextCore.Components.Quantity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Dataset;
internal static partial class ComponentLibrary
{
    private static void AddBuiltinComponentList()
    {
        AddCategory("Export", 
            [
            (typeof(pcExportSTL), Strings.ExportSTL, Strings.ExportSTLDesc, 0),
            ]);

        AddCategory("Quantity",
            [
            (typeof(pcConQty), Strings.ConstructQuantity, Strings.AddUnitToANumberToConvertItIntoAQuantityWhenTheUnitIsNotSuppliedDocumentUnitIsUsedRNUseParseStringComponentToCreateAFeetInchLengthQuantity, 0),
            (typeof(pcDeconFeetInch), Strings.DeconstructFeetInchLength, Strings.DeconstructAFeetInchLengthQuantityToItsComponents, 0),
            (typeof(pcDeconQty),  Strings.DeconstructQuantity, Strings.DeconstructQuantityToItsInsideAmountUnitAndUnitTypeFeetAndInchLengthWillBecomeADecimalAmountInFeetOtherwiseTheAmountIsNotConverted, 0),
            (typeof(pcSetPrecision), Strings.SetPrecision, Strings.SetThePrecisionOfAQuantityPrecisionMayHaveDifferentMeaningsOnDifferentQuantitiesSeeManualOrExampleForMoreInformation, 0),
            (typeof(pcToDecimalLen), Strings.ToDecimalLength, Strings.ConvertAQuantityToADecimalLengthWithDesignatedUnit, 0),
            (typeof(pcToFtInLen), Strings.ToFeetInchLength, Strings.ConvertAQuantityToAFeetInchLengthWithDesignatedUnit, 0),
            ]);
    }
}
