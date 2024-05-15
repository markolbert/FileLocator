using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public record StyleSetBase
{
    protected StyleSetBase(
        string styleName
        )
    {
        StyleName = styleName;

        Color = IndexedColors.Automatic;
        BackgroundColor = IndexedColors.Automatic;
        FontName = BaseStyle.DefaultFontName;
        FontHeightInPoints = BaseStyle.DefaultFontHeightInPoints;
        FontStyles = FontStyles.Normal;
        BorderInfo = new BorderInfo();
        WrapText = false;
        AutoSize = false;
        MaxWidth = 0;
        VerticalAlignment = VerticalAlignment.None;
        HorizontalAlignment = HorizontalAlignment.General;
    }

    public string StyleName { get; }
    public IndexedColors Color { get; init; }
    public IndexedColors BackgroundColor { get; init; }
    public string FontName { get; init; }
    public int FontHeightInPoints { get; init; }
    public FontStyles FontStyles { get; init; }
    public BorderInfo BorderInfo { get; init; }
    public bool WrapText { get; init; }
    public bool AutoSize { get; init; }

    // <= 0 means no max width
    public int MaxWidth { get; init; }

    public VerticalAlignment VerticalAlignment { get; init; }
    public HorizontalAlignment HorizontalAlignment { get; init; }

    public virtual bool ValuesEqual( StyleSetBase other )
    {
        return other.GetType().IsAssignableTo( GetType() )
         && string.Equals( StyleName, other.StyleName, StringComparison.OrdinalIgnoreCase )
         && string.Equals( FontName, other.FontName, StringComparison.OrdinalIgnoreCase )
         && FontHeightInPoints == other.FontHeightInPoints
         && FontStyles == other.FontStyles
         && MaxWidth == other.MaxWidth
         && Color.Equals( other.Color )
         && BackgroundColor.Equals( other.BackgroundColor )
         && WrapText == other.WrapText
         && AutoSize == other.AutoSize
         && VerticalAlignment == other.VerticalAlignment
         && HorizontalAlignment == other.HorizontalAlignment
         && BorderInfo.BorderInfoComparer.Equals( BorderInfo, other.BorderInfo );
    }

    public virtual ICellStyle CreateCellStyle( IWorkbook workbook )
    {
        var retVal = workbook.CreateCellStyle();

        var font = workbook.CreateFont();
        font.FontHeightInPoints = FontHeightInPoints;
        font.FontName = FontName;

        if (FontStyles.HasFlag(FontStyles.Bold))
            font.IsBold = true;

        if (FontStyles.HasFlag(FontStyles.Italic))
            font.IsItalic = true;

        font.Color = Color.Index;
        retVal.SetFont(font);

        retVal.FillBackgroundColor = BackgroundColor.Index;

        retVal.WrapText = WrapText;

        retVal.VerticalAlignment = VerticalAlignment;
        retVal.Alignment = HorizontalAlignment;
        
        retVal.BorderBottom = BorderInfo.Bottom;
        retVal.BorderRight = BorderInfo.Right;
        retVal.BorderTop = BorderInfo.Top;
        retVal.BorderLeft = BorderInfo.Left;

        // allow for creation of format string
        var formatText = CreateTextFormat();

        if( !string.IsNullOrEmpty(formatText))
            retVal.DataFormat = workbook.CreateDataFormat().GetFormat(formatText);

        return retVal;
    }

    protected virtual string CreateTextFormat() => string.Empty;

    protected static string ColorToText( IndexedColors color ) =>
        color == IndexedColors.Automatic ? string.Empty : $"[{color}]";
}
