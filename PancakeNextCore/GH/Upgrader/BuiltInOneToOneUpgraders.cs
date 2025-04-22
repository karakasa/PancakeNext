using PancakeNextCore.Components.Algorithm;
using PancakeNextCore.Components.Association;
using PancakeNextCore.Components.IO;
using PancakeNextCore.Components.Miscellaneous;
using PancakeNextCore.Components.Quantity;
using PancakeNextCore.GH.Params;

namespace PancakeNextCore.GH.Upgrader;

public sealed class Upgrader_pcConQty : OneToOneComponentUpgrader<pcConQty> { }
public sealed class Upgrader_pcDeconFeetInch : OneToOneComponentUpgrader<pcDeconFeetInch> { }
public sealed class Upgrader_pcDeconQty : OneToOneComponentUpgrader<pcDeconQty> { }
public sealed class Upgrader_pcSetPrecision : OneToOneComponentUpgrader<pcSetPrecision> { }
public sealed class Upgrader_pcToDecimalLen : OneToOneComponentUpgrader<pcToDecimalLen> { }
public sealed class Upgrader_pcToFtInLen : OneToOneComponentUpgrader<pcToFtInLen> { }
public sealed class Upgrader_pcExportSTL : OneToOneComponentUpgrader<pcExportSTL> { }
public sealed class Upgrader_pcExportTXT : OneToOneComponentUpgrader<pcExportTXT> { }
public sealed class Upgrader_pcImportTXT : OneToOneComponentUpgrader<pcImportTXT> { }
public sealed class Upgrader_pcImportTXTOld : OneToOneComponentUpgrader<pcImportTXT> {
    public override Guid Grasshopper1Id => new("{49602452-908A-4FDA-992E-CB8C4E2D62F2}");
}
public sealed class Upgrader_pcParseString : OneToOneComponentUpgrader<pcParseString> { }
public sealed class Upgrader_pcAssocToCsv : OneToOneComponentUpgrader<pcAssocToCsv> { }
public sealed class Upgrader_pcAssocToDatatable : OneToOneComponentUpgrader<pcAssocToDatatable> { }
public sealed class Upgrader_pcAssocToKv : OneToOneComponentUpgrader<pcAssocToKv> { }
public sealed class Upgrader_pcAssocToString : OneToOneComponentUpgrader<pcAssocToString> { }
public sealed class Upgrader_pcAssocToXml : OneToOneComponentUpgrader<pcAssocToXml> { }
public sealed class Upgrader_pcConAssoc : OneToOneComponentUpgrader<pcConAssoc> { }
public sealed class Upgrader_pcCsvToAssoc : OneToOneComponentUpgrader<pcCsvToAssoc> { }
public sealed class Upgrader_pcDeconAssoc : OneToOneComponentUpgrader<pcDeconAssoc> { }
public sealed class Upgrader_pcExtractFromAssoc : OneToOneComponentUpgrader<pcExtractFromAssoc> { }
public sealed class Upgrader_pcJsonToAssoc : OneToOneComponentUpgrader<pcJsonToAssoc> { }
public sealed class Upgrader_pcKvToAssoc : OneToOneComponentUpgrader<pcKvToAssoc> { }
public sealed class Upgrader_pcMergeAssoc : OneToOneComponentUpgrader<pcMergeAssoc> { }
public sealed class Upgrader_pcUnwrapList : OneToOneComponentUpgrader<pcUnwrapList> { }
public sealed class Upgrader_pcWrapList : OneToOneComponentUpgrader<pcWrapList> { }
public sealed class Upgrader_pcXmlToAssoc : OneToOneComponentUpgrader<pcXmlToAssoc> { }
public sealed class Upgrader_pcCategorize : OneToOneComponentUpgrader<pcCategorize> { }
public sealed class Upgrader_pcCountUnique : OneToOneComponentUpgrader<pcCountUnique> { }
public sealed class Upgrader_pcMultiSort : OneToOneComponentUpgrader<pcMultiSort> { }
public sealed class Upgrader_AssociationParameter : OneToOneParameterUpgrader<AssociationParameter> { }
public sealed class Upgrader_QuantityParameter : OneToOneParameterUpgrader<QuantityParameter> { }
public sealed class Upgrader_pcAbort : OneToOneComponentUpgrader<pcAbort> { }
public sealed class Upgrader_pcGhFilePath : OneToOneComponentUpgrader<pcGhFilePath> { }
public sealed class Upgrader_pcNull : OneToOneComponentUpgrader<pcNull> { }
public sealed class Upgrader_pcWaitUntil : OneToOneComponentUpgrader<pcWaitUntil> { }
public sealed class Upgrader_pcFileSeries : OneToOneComponentUpgrader<pcFileSeries> { }