namespace Pancake.Modules.PortabilityChecker
{

    public class Rhino5WinTargetConfiguration : StaticConfiguration
    {
        public const string Version = "Rhinoceros 5 (Win)";
        public override string Name => Version;
        public Rhino5WinTargetConfiguration()
        {
            Add<Rh5WinTargetChecker>();

            Add<QuantityOpCheck>();
            Add<CoincidenceChecker>();
            Add<ExternalFiles>();
            Add<ReferencedGeometryChecker>();
            Add<ThirdPartyLibrary>();
            Add<VersionWatermark>();
            Add<PotentialUnexpectedBehavior>();
            Add<HiddenObjectScanner>();
        }
    }

    public class Rhino5MacTargetConfiguration : StaticConfiguration
    {
        public const string Version = "Rhinoceros 5 (Mac)";
        public override string Name => Version;
        public Rhino5MacTargetConfiguration()
        {
            Add<Rh5MacTargetChecker>();

            Add<QuantityOpCheck>();
            Add<CoincidenceChecker>();
            Add<ExternalFiles>();
            Add<ReferencedGeometryChecker>();
            Add<ThirdPartyLibrary>();
            Add<VersionWatermark>();
            Add<PotentialUnexpectedBehavior>();
            Add<HiddenObjectScanner>();
        }
    }

    public class Rhino6TargetConfiguration : StaticConfiguration
    {
        public const string Version = "Rhinoceros 6";
        public override string Name => Version;
        public Rhino6TargetConfiguration()
        {
            Add<Rh6TargetChecker>();

            Add<CoincidenceChecker>();
            Add<ExternalFiles>();
            Add<ReferencedGeometryChecker>();
            Add<ThirdPartyLibrary>();
            Add<VersionWatermark>();
            Add<PotentialUnexpectedBehavior>();
            Add<HiddenObjectScanner>();
        }
    }

    public class Rhino7TargetConfiguration : StaticConfiguration
    {
        public const string Version = "Rhinoceros 7";
        public override string Name => Version;
        public Rhino7TargetConfiguration()
        {
            Add<CoincidenceChecker>();
            Add<ExternalFiles>();
            Add<ReferencedGeometryChecker>();
            Add<ThirdPartyLibrary>();
            Add<VersionWatermark>();
            Add<PotentialUnexpectedBehavior>();
            Add<HiddenObjectScanner>();
        }
    }
}
