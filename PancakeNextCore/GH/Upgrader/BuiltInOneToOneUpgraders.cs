using PancakeNextCore.Components.IO;
using PancakeNextCore.Components.Quantity;

namespace PancakeNextCore.GH.Upgrader;

public sealed class Upgrader_ExportSTL : OneToOneComponentUpgrader<pcExportSTL> { }
public sealed class Upgrader_ConQty : OneToOneComponentUpgrader<pcConQty> { }
public sealed class Upgrader_DeconQty : OneToOneComponentUpgrader<pcDeconQty> { }
public sealed class Upgrader_DeconFeetInch : OneToOneComponentUpgrader<pcDeconFeetInch> { }
public sealed class Upgrader_SetPrecision : OneToOneComponentUpgrader<pcSetPrecision> { }
public sealed class Upgrader_ToDecimalLen : OneToOneComponentUpgrader<pcToDecimalLen> { }
public sealed class Upgrader_ToFtInLen : OneToOneComponentUpgrader<pcToFtInLen> { }