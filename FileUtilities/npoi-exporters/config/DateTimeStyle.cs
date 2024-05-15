namespace J4JSoftware.FileUtilities;

public class DateTimeStyle : BaseStyle
{
    public const string DefaultDateSeparator = "/";

    private int _secDecPlaces;

    public MonthDayTimeFormat DateFormat { get; set; } = MonthDayTimeFormat.Numbers;
    public DateSequence DateSequence { get; set; } = DateSequence.MonthDayYear;
    public bool DateLeadingZero { get; set; }
    public bool FourDigitYear { get; set; } = true;
    public string DateSeparator { get; set; } = DefaultDateSeparator;

    public bool IncludeTime { get; set; }
    public MonthDayTimeFormat TimeFormat { get; set; } = MonthDayTimeFormat.Numbers;
    public bool TimeLeadingZero { get; set; }
    public bool TwentyFourHourTime { get; set; }
    public bool IncludeSeconds { get; set; }

    public int SecondsDecimalPlaces
    {
        get => _secDecPlaces;
        set => _secDecPlaces = value < 0 ? 0 : value;
    }
}
