using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public record NumericStyleSet : StyleSetBase
{
    protected NumericStyleSet(
        NumericStyle config
    )
        : base( config.StyleName )
    {
        DecimalSeparator = config.DecimalSeparator;
        GroupSeparator = config.GroupSeparator;

        PositiveColor = config.PositiveColor;
        NegativeColor = config.NegativeColor;
        ZeroColor = config.ZeroColor;

        SuppressZero = config.SuppressZero;
        IsPercent = config.IsPercent;
        NegativeParentheses = config.NegativeParentheses;

        LeadingCurrency = config.LeadingCurrencySymbol;
        TrailingCurrency = config.TrailingCurrencySymbol;
    }

    public bool SuppressZero { get; init; }
    public bool IsPercent { get; init; }
    public string GroupSeparator { get; init; }
    public string DecimalSeparator { get; init; }
    public bool NegativeParentheses { get; init; }
    public string LeadingCurrency { get; init; }
    public string TrailingCurrency { get; init; }
    public IndexedColors PositiveColor { get; init; }
    public IndexedColors NegativeColor { get; init; }
    public IndexedColors ZeroColor { get; init; }

    public override bool ValuesEqual( StyleSetBase other )
    {
        if( !base.ValuesEqual( other ) )
            return false;

        if( other is not NumericStyleSet numericOther )
            return false;

        return SuppressZero == numericOther.SuppressZero
         && string.Equals( GroupSeparator, numericOther.GroupSeparator, StringComparison.OrdinalIgnoreCase )
         && string.Equals( DecimalSeparator, numericOther.DecimalSeparator, StringComparison.OrdinalIgnoreCase )
         && NegativeParentheses == numericOther.NegativeParentheses
         && string.Equals( LeadingCurrency, numericOther.LeadingCurrency, StringComparison.OrdinalIgnoreCase )
         && string.Equals( TrailingCurrency, numericOther.TrailingCurrency, StringComparison.OrdinalIgnoreCase )
         && PositiveColor.Equals( numericOther.PositiveColor )
         && NegativeColor.Equals( numericOther.NegativeColor )
         && ZeroColor.Equals( numericOther.ZeroColor );
    }

    protected override string CreateTextFormat()
    {
        var digitsSection = GetDigitsSection();

        string positive;
        string negative;
        string zero;

        // percents override other properties
        if( IsPercent )
        {
            positive = $"{digitsSection}%";
            negative = $"-{digitsSection}%";
            zero = SuppressZero ? "-" : $"{digitsSection}%";
        }
        else
        {
            // this element controls whether the entire cell is filled with the format...
            // which, if present, suppresses centering
            var fillCell = HorizontalAlignment != HorizontalAlignment.General ? string.Empty : "* ";

            if( NegativeParentheses )
            {
                positive = $"_({LeadingCurrency}{fillCell}{digitsSection}{TrailingCurrency}_)";
                negative = $"_({LeadingCurrency}{fillCell}{digitsSection}{TrailingCurrency})";
                zero = $"_({LeadingCurrency}{fillCell}-??{TrailingCurrency}_)";
            }
            else
            {
                positive = $"{LeadingCurrency}{fillCell}{digitsSection}{TrailingCurrency}";
                negative = $"-{LeadingCurrency}{fillCell}{digitsSection}{TrailingCurrency}";
                zero = $"{LeadingCurrency}{fillCell}-??{TrailingCurrency}";
            }
        }

        return
            $"{ColorToText( PositiveColor )}{positive};{ColorToText( NegativeColor )}{negative};{ColorToText( ZeroColor )}{zero}";
    }

    protected virtual string GetDigitsSection() =>
        string.IsNullOrEmpty( GroupSeparator ) ? "0" : $"#{GroupSeparator}##0";
}
