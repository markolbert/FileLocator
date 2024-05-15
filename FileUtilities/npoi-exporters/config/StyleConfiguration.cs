using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class StyleConfiguration
{
    public const string DefaultFontName = "Segoe UI";
    public const int DefaultFontHeightInPoints = 12;

    private string _fontName = DefaultFontName;
    private int _fontHeightInPts = DefaultFontHeightInPoints;

    public string FontName
    {
        get => _fontName;

        set
        {
            value = value.Trim();

            _fontName = string.IsNullOrEmpty(value) ? StyleConfiguration.DefaultFontName : value;
        }
    }

    public int FontHeightInPoints
    {
        get => _fontHeightInPts;
        set => _fontHeightInPts = value <= 0 ? StyleConfiguration.DefaultFontHeightInPoints : value;
    }

    public List<BaseStyle> Defined { get; set; } = [];

    public void ValidateStyles( ILoggerFactory? loggerFactory )
    {
        var logger = loggerFactory?.CreateLogger<StyleConfiguration>();

        if( Defined.Count == 0 )
        {
            logger?.StylesNotDefined();
            return;
        }

        // correct any duplicate style names
        var grouped = Defined.GroupBy( s => s.StyleName,
                                       s => s,
                                       ( name, defs ) => new { Name = name, Styles = defs.ToList() },
                                       StringComparer.OrdinalIgnoreCase );

        foreach( var group in grouped.Where( g => g.Styles.Count > 1 ) )
        {
            for( var idx = 0; idx < group.Styles.Count; idx++ )
            {
                if( idx == 0 )
                    continue;

                var newName = $"{group.Styles[ idx ].StyleName}{idx + 1}";
                logger?.StyleNameCollision( group.Styles[ idx ].StyleName, newName );

                group.Styles[ idx ].StyleName = newName;
            }
        }

        foreach( var simpleNumber in Defined.Where( s => s is SimpleNumberStyle ).Cast<SimpleNumberStyle>() )
        {
            switch( simpleNumber?.Clauses.Count )
            {
                case 0:
                    logger?.NoSimpleNumberClauses();
                    break;

                case > 4:
                    logger?.TooManySimpleNumberClauses();
                    break;
            }
        }

        if( Defined.Count( d => d.StyleName.Equals( "Base", StringComparison.OrdinalIgnoreCase ) ) == 0 )
            Defined.Add( new FormatCodeStyle
            {
                FontHeightInPoints = FontHeightInPoints,
                FontName = FontName,
                StyleName = "Base",
                FormatText = "General"
            } );

    }
}
