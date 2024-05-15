namespace J4JSoftware.FileUtilities;

public class Configuration : IConfiguration
{
    //public static DateTime DefaultEarliestGiftDate { get; } = new(2007, 1, 1);
    //public const double DefaultTolerance = 0.01;

    //private double _tolerance = DefaultTolerance;
    //private string _dupeAddressConfigPath = null!;

    //// attempting to assign a value < 0 defaults to DefaultTolerance
    //public double Tolerance
    //{
    //    get => _tolerance;
    //    set => _tolerance = value < 0 ? DefaultTolerance : value;
    //}

    //// we allow this to not be defined in the configuration file...
    //// but if it isn't, we default to 1/1/2007, so that the property
    //// >>always<< has a value, even though it's nullable
    //public DateTime EarliestGiftDate { get; set; } = DefaultEarliestGiftDate;

    //public double TotalGifts { get; set; }

    //public string DuplicateAddressConfigurationPath
    //{
    //    get => _dupeAddressConfigPath;
    //    set => _dupeAddressConfigPath = FileExtensions.NormalizeFilePath(value);
    //}

    public List<ImportDirectory> ImportDirectories { get; set; } = [];

    public StyleConfiguration Styles { get; set; } = new();

}
