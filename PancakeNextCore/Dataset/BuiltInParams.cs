namespace PancakeNextCore.Dataset;

internal static partial class ComponentLibrary
{
    private static void AddBuiltinParamList()
    {
        Param("0", "0", "0", Strings.Data0);
        Param("1", "1", "1", Strings.Data1);
        //Param("(pre-)option", "(Pre-)Option", "(Pre)Opt", Strings.ConnectToExportAsComponent);
        Param("+infinity", "+Infinity", "+��", Strings.PositiveInfinityAsIn10);
        Param("-infinity", "-Infinity", "-��", Strings.NeagtiveInfinityAsIn10);
        Param("adjustedquantity", "Adjusted Quantity", "Q", Strings.AdjustedQuantity);
        Param("amount", "Amount", "A", Strings.Amount);
        Param("amount2", "Amount", "A", Strings.AmountInside);
        Param("amountinfeet", "Amount in feet", "F", Strings.EntireLengthInDecimalFeet);
        Param("assoc", "Assoc", "A", Strings.AssociativeArray);
        Param("assoc2", "Assoc", "A", Strings.InputListOnlyNamedValuesWillBeExtracted);
        Param("assoc4", "Assoc", "A", Strings.IfEmptyANewAssocWillBeCreated);
        //Param("assoctoset", "Data to Assign", "A", Strings.AssocToSet);
        Param("atomlist", "Atom list", "AL", Strings.TheInputAtomListYouMayAlsoUseTheDataWrapperFromMetahopper);
        Param("atomlist2", "Atom list", "AL", Strings.TheOutputAtomList);
        Param("content", "Content", "Text", Strings.TheTextYouWantToExport);
        Param("count", "Count", "C", Strings.CountOfEachKeyInAccordanceToKL);
        Param("counteroffirstfile", "Counter of first file", "C", Strings.CounterOfFirstFile);
        Param("csv", "CSV", "C", Strings.CSVLinesReadByReadFileEtc);
        //Param("cullunnecessaryvertices", "Cull Unnecessary Vertices", "CV", Strings.DefaultTrue);
        Param("data", "Data", "D", Strings.Data);
        Param("data3", "Data", "D", Strings.PostponedData);
        //Param("defaultused", "Default used", "D?", Strings.WhetherTheDefaultValueIsUsed);
        //Param("defaultvalue", "Default Value", "D", Strings.DefaultValueIfTheClusterInputIsEmpty);
        Param("delimiter", "Delimiter", "D", Strings.DelimiterByDefault);
        Param("delimiter2", "Delimiter", "D", Strings.DelimiterByDefaultSlash);
        Param("desiredtype", "Desired Type", "T", Strings.TheComponentWillNotTryAnythingElseOtherThanDesiredTypes);
        Param("directory", "Directory", "D", Strings.DirectoryOfWhereTheCurrentDefinitionIsStored);
        Param("directoryseparator", "Directory Separator", "S", Strings.DirectorySeparatorOfCurrentOperatingSystem);
        //Param("droppreviewmesh", "Drop Preview Mesh", "PM", Strings.ControlWhetherToDropPreviewMeshWithoutPreviewMesh);
        Param("error", "Error", "E", Strings.DifferenceBetweenFractionRepresentationAndActualValueInFeet);
        Param("export", "Export", "Export", Strings.SetToTrueToConductTheExportUseTrueOnlyButton);
        //Param("exportcommand", "Export command", "Cmd", Strings.TheFinalCommandExecuted);
        //Param("exportparameterspacecurves", "Export Parameter Space Curves", "PSC", Strings.DefaultFalse);
        //Param("exportplanarregionsaspolygons", "Export Planar Regions As Polygons", "PR", Strings.DefaultTrueOtherwisePlanarRegionsWillBeConvertedIntoMesh);
        Param("feetinteger", "Feet Integer", "FI", Strings.IntegerPartOfFeet);
        Param("file", "File", "F", Strings.FileToBeImported);
        Param("filelocation", "File Location", "File", Strings.WhereToStoreTheFileFilenameAndExtensionShouldBeIncluded);
        Param("filepath", "Filepath", "FP", Strings.FilePath);
        Param("generatenext", "Generate next", "OK", Strings.SetToTrueToGenerateTheNextFilepath);
        //Param("geo", "Geo", "G", Strings.ImportedGeometry);
        //Param("geoguid", "Guid", "ID", Strings.GUIDsOfImportedGeometry);
        //Param("geometry", "Geometry", "Geo", Strings.TheGeometryYouWantToExport);
        Param("headers", "Headers", "H", Strings.HeadersOfTheTable);
        Param("inch", "Inch", "I", Strings.DecimalInch);
        Param("inchfractiondenominator", "Inch Fraction Denominator", "ID", Strings.DenominatorOfInchFraction);
        Param("inchfractionnumerator", "Inch Fraction Numerator", "IN", Strings.NumeratorOfInchFraction);
        Param("inchinteger", "Inch Integer", "II", Strings.IntegerPartOfInch);
        //Param("index", "Index", "I", Strings.IndexAsTheNewPrincipleValueYouMustProvideANameOrAnIndex);
        Param("items", "Items", "I", Strings.ItemsExtracted);
        Param("items2", "Items", "I", Strings.ItemsToBeComposedAsAList);
        Param("key", "Key", "K", Strings.Keys);
        Param("key2", "Key-paths", "KP", Strings.KeyPathsToBeImportedByDefaultAllPathsAreImported);
        Param("keylist", "Key list", "KL", Strings.ListOfCategorizedKeys);
        //Param("layer", "Layer", "Layer", Strings.LayerToPutTheGeometriesByDefaultThisInputWillOverrideObjAttr);
        //Param("layer3", "Layer", "L", Strings.LayerPaths);
        //Param("layerfilter", "Layer filter", "L", Strings.OnlyGeometriesOnTheDesignatedLayersWillBeImported);
        //Param("list", "List", "L", Strings.ListToBeShuffled);
        //Param("list2", "List", "L", Strings.ShuffledList);
        Param("listoffilepath", "List of Filepath", "FPL", Strings.FilePathList);
        //Param("log", "Log", "Log", Strings.LogIfAvailable);
        Param("lostaccuracy?", "Lost Accuracy?", "L?", Strings.ReturnIfInternalAccuracyHasLostDuringPrecisionResettingRN);
        //Param("mapytoz", "MapYtoZ", "T", Strings.MapYAxisToZAxisByDefaultFalse);
        //Param("mapztoy", "Map Z to Y", "YU", Strings.MapRhinoZAxisToFBXYAxisDefaultFalse);
        //Param("materialasphong?", "Material as Phong?", "P", Strings.WhetherToExportMaterialAsPhongMaterialDefaultTrueOtherwiseTheyWillBeExportedAsLambert);
        Param("mesh", "Mesh", "Mesh", Strings.MeshToBeExported);
        //Param("minimumfacecount", "Minimum Face Count", "MF", Strings.Default2Must2);
        Param("name", "Name", "N", Strings.FileNameOfCurrentDefinition);
        //Param("name2", "Name", "N", Strings.NamedValueAsTheNewPrincipleValueYouMustProvideANameOrAnIndex);
        Param("nan", "NaN", "NaN", Strings.NaNAsIn00);
        Param("null", "Null", "Null", Strings.NullTheDefaultEmptyRepresentationOfNETFramework);
        //Param("nurbsexport", "NURBS Export", "N?", Strings.WhetherToExportNURBSAsNURBSOtherwiseAsMeshDefaultFalse);
        //Param("object", "Object", "O", Strings.ObjectDeserialized);
        //Param("object2", "Object", "O", Strings.Object);
        //Param("object3", "Object", "O", Strings.ObjectToSerialize);
        //Param("objectattributes", "Object Attributes", "ObjAttr", Strings.ObjectAttributesDesc);
        //Param("objectattributes3", "Object attributes", "A", Strings.BasicAttributesOfTheImportedGeometry);
        Param("ok", "OK", "OK", Strings.ReturnsIfTheActionIsSuccessful);
        Param("ok5", "OK", "I", Strings.SetTrueToConductTheImport);
        //Param("omitcolorsofblackobjects", Strings.OmitColorsOfBlackObjects, "OC", "Default true");
        //Param("operanda", "Operand A", "A", Strings.FirstOperand);
        //Param("operandb", "Operand B", "B", Strings.SecondOperand);
        //Param("operator", "Operator", "O", Strings.OperatorWhichCanBeOrOrOrOrOr);
        //Param("option", "Option", "Opt", Strings.ConnectToExportAsComponent);
        //Param("option8", "Option", "O", Strings.OptionsOfImport);
        //Param("options", "Options", "Opt", Strings.OptionsOfExportYouCanFeedItWithOtherComponentsOrManualData);
        //Param("output", "Output", "O", Strings.OutputValue);
        Param("overwrite", "Overwrite", "Overwrite", Strings.DefaultFalseControlIfPancakeShouldOverwriteTheDestinationFile);
        Param("parsed", "Parsed", "P", Strings.ParsedResultIfSomethingCannotBeDeterminedTheResultWillBeNull);
        Param("path", "Path", "P", Strings.Path);
        Param("paths", "Paths", "P", Strings.Paths);
        Param("patternoffilepath", "Pattern of filepath", "P", Strings.ForExampleDSTLExport0StlWithoutQuoteWillResultInDSTLExport);
        Param("precision", "Precision", "P", Strings.OptionalByDefault4);
        Param("precision2", "Precision", "P", Strings.PrecisionRNForDecimalQuantitiesThePrecisionIsHowManyDigits);
        //Param("pre-options", "Pre-Options", "PreOpt", Strings.GeneralOptionsOfExportConnectRhinoFileOptionsToThisInputIfNeeded);
        Param("quantity", "Quantity", "Q", Strings.ConstructedQuantity);
        Param("quantity2", "Quantity", "Q", Strings.TheFeetInchQuantityToBeDecomposed);
        Param("quantity3", "Quantity", "Q", Strings.QuantityToBeDeconstructed);
        Param("quantity4", "Quantity", "Q", Strings.QuantityCategory);
        Param("quantity5", "Quantity", "Q", Strings.QuantityToBeConverted);
        Param("quantity6", "Quantity", "Q", Strings.OutputQuantity);
        Param("quantity7", "Quantity", "Q", Strings.QuantityToBeConvertedRNIfTheInputIsNotAnQuantityButANumberTheDocument);
        Param("quantityoffilenames", "Quantity of filenames", "C", Strings.Default1DoesnTOutputMultipleNames);
        Param("relocate", "Relocate", "Reloc", Strings.PancakeWillMoveTheBasePointOfMeshToOriginWhichHelpsPostprocessing);
        Param("reset", "Reset", "R", Strings.SetToTrueToResetInternalCounterResettingWonTBypass);
        //Param("result", "Result", "R", Strings.Result);
        Param("saveasbinaryfile", "Save as binary file", "Binary", Strings.TrueForBinaryOutputAndFalseForASCIIOutput);
        //Param("savegeometryonly", "Save Geometry Only", "GO", Strings.ControlIfOnlyGeometryIsSavedDefaultFalse);
        //Param("savenotes", "Save Notes", "N", Strings.ControlIfNoteIsSavedDefaultTrue);
        Param("savenow", "Save now", "Save", Strings.SetToTrueToSaveTheFile);
        Param("savepath", "Save path", "Path", Strings.ThePlaceWhereSTLFileWillBeSaved);
        //Param("saveplugindata", "Save Plugin Data", "PD", Strings.ControlIfPluginDataIsSavedDefaultTrue);
        //Param("savetexture", "Save Texture", "T", Strings.ControlIfTextureIsSavedDefaultTrue);
        //Param("schema", "Schema", "S", Strings.SchemaOfSTEPExportAvailableValuesAreAP203ConfigControlDesignAP214AutomotiveDesignAndAP214AutomotiveDesignCC2);
        //Param("scheme", "Scheme", "S", Strings.DefaultDefaultSeeTheDefinitionInRhinoExportDialog);
        //Param("scheme3", "Scheme", "S", Strings.TheTypeOfIGESExportDefinedInIGESExportDialogByDefaultItUsesTheOneFromLastTime);
        //Param("scheme4", "Scheme", "S", Strings.TheTypeOfSATExportByDefaultItUsesTheOneFromLastTime);
        //Param("seed", "Seed", "S", Strings.RandomSeed);
        //Param("serialized", "Serialized", "S", Strings.SerializedObjectInBase64Format);
        Param("signal", "Signal", "S", Strings.SignalData);
        //Param("splitclosedsurfaces", "Split Closed Surfaces", "SCS", Strings.DefaultTrue);
        Param("startingindex", "Starting index", "S", Strings.Default1);
        Param("string", "String", "S", Strings.OutputString);
        Param("string2", "String", "S", Strings.InputString);
        //Param("tolerance", "Tolerance", "T", Strings.ByDefaultDeterminedByRhinoceros);
        Param("type", "Type", "T", Strings.TypeOfParsedResultSeeExampleForMoreInformation);
        Param("unit", "Unit", "U", Strings.IfOptionalOrEmptyCurrentDocumentUnitWillBeUsed);
        Param("unit2", "Unit", "U", Strings.Unit);
        Param("unit3", "Unit", "U", Strings.TheDesiredUnitToConvertToAFeetInchLengthUseToFeetInchLengthComponentInstead);
        //Param("unitsystem", "Unitsystem", "U", Strings.DefaultDecidedByRhinocerosTheProcedureCannotBeAutomatic);
        Param("unittype", "Unit Type", "T", Strings.TypeOfUnit);
        Param("value", "Value", "V", Strings.ValueS);
        //Param("value3", "Value", "V", Strings.TheValueFromClusterInput);
        Param("values", "Values", "V", Strings.ValuesOfTheTable);
        Param("valuetree", "Value tree", "VT", Strings.CategorizedValuesInAccordanceToKL);
        //Param("version", "Version", "V", Strings.TheVersionOfRhinoFileYouWantToExport);
        //Param("version2", "Version", "V", Strings.LeaveEmptyForDefaultAvailableChoicesAreVersion7);
        //Param("version3", "Version", "V", Strings.VersionOfTheSketchupExportAvailableOnesAre);
        //Param("version4", "Version", "Ver", Strings.TheVersionOf3dmFile);
        //Param("layername", "Name", "L", Strings.NameOfTheCurrentLayer);
        //Param("layerfullpath", "Fullpath", "FP", Strings.FullpathOfTheCurrentLayer);
        Param("roottag", "RootTag", "R", Strings.NameOfXMLRootTag);
        Param("depth", "Depth", "D", Strings.HowDeepTheDataIsUnwrappedByDefault0ZeroMeansAssocIs);

        Param("text", "Text", "T", Strings.ImportedText);
        Param("encoding", "Encoding", "E", Strings.EncodingThatTheFileShouldBeTreatedAs);
        Param("encoding2", "Encoding", "Encoding", Strings.EncodingThatTheFileShouldBeTreatedAs);
        Param("interestednames", "Names", "N", Strings.OnlyTheseDesignatedNamesAreImportedByDefaultAllNamesAreImported);

        Param("k0in", "Key 0", "K0", Strings.KeysToBeUsedAsSortingCriteriaOneByOne);
        Param("d0in", "Data 0", "D0", Strings.DataToBeSortedAccordingToKeys);
        Param("k0out", "Key 0", "K0", Strings.SortedKeys);
        Param("d0out", "Data 0", "D0", Strings.DataInAccordanceToTheSortedKeys);

        //Param("unref_input", "Geometry", "G", Strings.GeometryToBeUnReferenced);
        //Param("unref_output", "Geometry", "G", Strings.GeometryWithoutReferences);

        Param("condition", "Condition", "C", Strings.SetToTrueToAbortTheCurrentDefinition);
        Param("passthrough", "Passthrough", "D", Strings.DataToPassThroughOptional);

        //Param("docname", "Name", "N", Strings.NameOfTheCurrentRhinoFileMayBeEmpty);
        //Param("docpath", "Path", "P", Strings.PathOfTheCurrentRhinoFileMayBeEmpty);

        //Param("refresh", "Refresh", "R", "Refresh to reflect the latest settings.");
        //Param("docenv", "Doc Env", "DE", "A string representing the environment of Rhino document.");
        //Param("objattr_new", "Attributes", "Attr", "Attributes being applied to the geometry.\r\nYou may also directly connect texts with this input to indicate layers.\r\nAttributes from Elefront or Human are also accepted.");

        Param("json", "Json", "JSON", "Json to be parsed");

        //Param("Attr", "Attribute", "A", "Object attributes");

        //Param("RhinoDoc_Reserved", "Reserved", "R", "Reserved input.");
        //Param("LwID", "Guid for referenced geometry", "ID", "ID of the Rhino object you would like to manipulate.");
        //Param("BakeAction", "Bake action", "BA", "What to do when the lightweight object is baked. Right-click for options.");
        //Param("AllowNewAttributes", "Allow new attribute", "AA", "Whether new attributes are allowed when the object is baked.\r\nIf the value is false, the baked object would maintain its original attribute no matter what is defined by baking plugin, e.g. Elefront, Human.\r\nOtherwise its attribute would be replaced by that from the baking plugin.");
        //Param("PM?", "Preview mesh?", "PM?", "Creates a preview of that object for better user feedback.");
        //Param("CPM", "Custom preview mesh", "CPM", "Custom preview mesh, useful when the original one is compliated and resource-consuming.");

        //Param("BaseGeo", "Geometry", "G", "The original geometry to be wrapped.");

        //Param("LW", "Lightweight object", "LW", "Lightweight object");
        //Param("LwG", "Calculated geometry", "G", "Calculated geometry, after transformations.");
        //Param("LwX", "Transform", "X", "Combined transformation inside the lightweight object.");

        //Param("SxT", "Translation", "T", "Translation vector");
        //Param("SxR", "Rotation angle", "R", "Rotation angle, around XY plane");
        //Param("SxX", "X Scale", "X", "Scale on the X Axis");
        //Param("SxY", "Y Scale", "Y", "Scale on the Y Axis");
        //Param("SxZ", "Z Scale", "Z", "Scale on the Z Axis");
        //Param("SxX", "Transform", "X", "Combined transformation");

        //Param("outerCrv", "Outer Curve", "OC", "Outer curve of the extrusion.");
        //Param("innerCrv", "Inner Curve", "IC", "Inner curve(s) of the extrusion. Optional.");
        //Param("Height", "Height", "H", "Height of the extrusion.");
        //Param("Cap", "Capped?", "C?", "Whether the extrusion is capped.");

        //Param("Direction", "Direction", "D", "Direction of the extrusion.");

    }
}
