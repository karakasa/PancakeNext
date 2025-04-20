using Grasshopper2.UI;
using PancakeNextCore.Components.Algorithm;
using PancakeNextCore.Components.Association;
using PancakeNextCore.Components.IO;
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
        AddCategory("IO",
            [
            (typeof(pcParseString), Strings.ParseString, Strings.ParseFormattedStringToItsCorrectTypeCurrentlyThisComponentSupportsIntegerNumberBooleanGuidLengthQuantityDatetimePointDomain12DColourAndJsonSeeExamplesOrManualForMoreInformation, 0),
            (typeof(pcImportTXT), Strings.ImportTXT, Strings.ReadTextFileWithASpecificEncoding, 0),
            (typeof(pcExportSTL), Strings.ExportSTL, Strings.ExportSTLDesc, 1),
            (typeof(pcExportTXT), Strings.ExportTXT,  Strings.ThisComponentExportsTextToAFile, 1),
            ]);

        AddCategory("Association", 
            [
            (typeof(pcJsonToAssoc), "Json to Assoc", "Converts a json string to Assoc object.\r\nUse 'Assoc to String' to convert assoc to json.", 0),
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

        AddCategory("Misc", [
            (typeof(pcCategorize),     Strings.Categorize,Strings.CategorizeValuesByKeys,0),
            ]);
    }
}
