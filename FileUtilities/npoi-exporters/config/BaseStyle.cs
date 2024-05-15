using System.Text.Json.Serialization;

namespace J4JSoftware.FileUtilities;

[ JsonPolymorphic( IgnoreUnrecognizedTypeDiscriminators = true,
                   UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor ) ]
[JsonDerivedType(typeof(SimpleNumberStyle), "number")]
[JsonDerivedType(typeof(IntegerStyle), "integer")]
[JsonDerivedType(typeof(FormatCodeStyle), "format-code")]
[JsonDerivedType(typeof(DateTimeStyle), "datetime")]
[JsonDerivedType(typeof(DoubleStyle), "double")]
public class BaseStyle
{
    public const string DefaultFontName = "Segoe UI";
    public const int DefaultFontHeightInPoints = 12;

    private string _fontName = DefaultFontName;
    private int _fontHeightInPts = DefaultFontHeightInPoints;

    protected BaseStyle()
    {
    }

    public string StyleName { get; set; } = string.Empty;

    public string FontName
    {
        get => _fontName;

        set
        {
            value = value.Trim();

            _fontName = string.IsNullOrEmpty( value ) ? BaseStyle.DefaultFontName : value;
        }
    }

    public int FontHeightInPoints
    {
        get => _fontHeightInPts;
        set => _fontHeightInPts = value <= 0 ? BaseStyle.DefaultFontHeightInPoints : value;
    }
}
