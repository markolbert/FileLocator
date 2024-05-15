using System.Globalization;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NumericStyle : BaseStyle
{
    protected NumericStyle()
    {
    }

    public bool SuppressZero { get; set; }
    public bool IsPercent { get; set; }
    public bool NegativeParentheses { get; set; }

    public IndexedColors PositiveColor { get; set; } = IndexedColors.Automatic;
    public IndexedColors NegativeColor { get; set; } = IndexedColors.Automatic;
    public IndexedColors ZeroColor { get; set; } = IndexedColors.Automatic;

    public string LeadingCurrencySymbol { get; set; } = string.Empty;
    public string TrailingCurrencySymbol { get; set; } = string.Empty;

    public string GroupSeparator { get; set; } = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
    public string DecimalSeparator { get; set; } = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
}
