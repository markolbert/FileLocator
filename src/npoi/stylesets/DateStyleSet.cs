using System.Text;

namespace J4JSoftware.FileUtilities;

public record DateStyleSet : StyleSetBase
{
    public DateStyleSet(
        DateTimeStyle config
    )
        : base( config.StyleName )
    {
        MonthDayFormat = config.DateFormat;
        DateSequence = config.DateSequence;
        DateLeadingZero = config.DateLeadingZero;
        FourDigitYear = config.FourDigitYear;
        DateSeparator = config.DateSeparator;
        IncludeTime = config.IncludeTime;
        TimeFormat = config.TimeFormat;
        TimeLeadingZero = config.TimeLeadingZero;
        Hour24 = config.TwentyFourHourTime;
        IncludeSeconds = config.IncludeSeconds;
        SecondsDecimalPlaces = config.SecondsDecimalPlaces;
    }

    public MonthDayTimeFormat MonthDayFormat { get; init; }
    public DateSequence DateSequence { get; init; }
    public string DateSeparator { get; init; }
    public bool DateLeadingZero { get; init; }
    public bool FourDigitYear { get; init; }
    public bool IncludeTime { get; init; }
    public bool IncludeSeconds { get; init; }
    public MonthDayTimeFormat TimeFormat { get; init; }
    public bool TimeLeadingZero { get; init; }
    public bool Hour24 { get; init; }
    public int SecondsDecimalPlaces { get; init; }

    public override bool ValuesEqual( StyleSetBase other )
    {
        if( !base.ValuesEqual( other ) )
            return false;

        if( other is not DateStyleSet dateOther )
            return false;

        return MonthDayFormat == dateOther.MonthDayFormat
         && DateSequence == dateOther.DateSequence
         && DateLeadingZero == dateOther.DateLeadingZero
         && FourDigitYear == dateOther.FourDigitYear
         && string.Equals( DateSeparator, dateOther.DateSeparator, StringComparison.OrdinalIgnoreCase )
         && IncludeTime == dateOther.IncludeTime
         && TimeFormat == dateOther.TimeFormat
         && TimeLeadingZero == dateOther.TimeLeadingZero
         && Hour24 == dateOther.Hour24
         && IncludeSeconds == dateOther.IncludeSeconds
         && SecondsDecimalPlaces == dateOther.SecondsDecimalPlaces;
    }

    protected override string CreateTextFormat()
    {
        var multiple = MonthDayFormat switch
        {
            MonthDayTimeFormat.Numbers => DateLeadingZero ? 2 : 1,
            MonthDayTimeFormat.Abbreviations => 3,
            MonthDayTimeFormat.FullNames => 4,
            _ => 2
        };

        var monthText = new string( 'M', multiple );
        var dayText = new string( 'd', multiple );
        var yearText = FourDigitYear ? "yyyy" : "yy";

        var dateText = DateSequence switch
        {
            DateSequence.MonthDayYear => MonthDayFormat == MonthDayTimeFormat.Numbers
                ? $"{monthText}{DateSeparator}{dayText}{DateSeparator}{yearText}"
                : $"{monthText} {dayText}, {yearText}",
            DateSequence.DayMonthYear => MonthDayFormat == MonthDayTimeFormat.Numbers
                ? $"{dayText}{DateSeparator}{monthText}{DateSeparator}{yearText}"
                : $"{dayText} {monthText} {yearText}",
            DateSequence.YearMonthDay => MonthDayFormat == MonthDayTimeFormat.Numbers
                ? $"{yearText}{DateSeparator}{monthText}{DateSeparator}{dayText}"
                : $"{yearText}{monthText}{dayText}",
            _ => $"{monthText} {dayText}, {yearText}"
        };

        var sb = new StringBuilder();
        sb.Append( dateText );

        if( IncludeTime )
        {
            sb.Append( ' ' );
            sb.Append( GetTimeText() );
        }

        return sb.ToString();
    }

    private string GetTimeText()
    {
        var multiple = TimeFormat switch
        {
            MonthDayTimeFormat.Numbers => TimeLeadingZero ? 2 : 1,
            MonthDayTimeFormat.Abbreviations => 3,
            MonthDayTimeFormat.FullNames => 4,
            _ => 2
        };

        var sb = new StringBuilder();

        sb.Append( new string( 'h', multiple ) );
        sb.Append( ':' );
        sb.Append( new string( 'm', multiple ) );

        if( !IncludeSeconds )
        {
            sb.Append( Hour24 ? string.Empty : " AM" );
            return sb.ToString();
        }

        sb.Append( ':' );
        sb.Append( new string( 's', multiple ) );

        if( SecondsDecimalPlaces > 0 )
            sb.Append( $".{new string( '0', SecondsDecimalPlaces )}" );

        sb.Append( Hour24 ? string.Empty : " AM" );

        return sb.ToString();
    }
}
