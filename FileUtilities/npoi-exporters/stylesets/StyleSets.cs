using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class StyleSets : IStyleSets
{
    private record ResolvedStyle( StyleSetBase StyleSet, ICellStyle CellStyle );

    private readonly Dictionary<string, StyleSetBase> _namedStyles = new( StringComparer.OrdinalIgnoreCase );
    private readonly Dictionary<Type, StyleSetBase> _typedStyles = [];
    private readonly List<ResolvedStyle> _cachedStyles = [];

    private readonly ILogger? _logger;

    public StyleSets(
        StyleConfiguration config,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger(GetType());

        // this ensures style names are unique
        config.ValidateStyles( loggerFactory );

        DefaultFontHeightInPoints = config.FontHeightInPoints;
        DefaultFontName = config.FontName;

        LoadConfiguredStyles( config.Defined );

        if( !_namedStyles.TryGetValue( "base", out var baseStyleSet ) )
            baseStyleSet = new FormatCodeStyleSet( new FormatCodeStyle
            {
                FontHeightInPoints = DefaultFontHeightInPoints,
                FontName = DefaultFontName,
                FormatText = "General"
            } );

        DefaultBase = baseStyleSet;

        DefaultTitle = GetDefaultByName("title") ?? (FormatCodeStyleSet) DefaultBase with { FontHeightInPoints = 14, FontStyles = FontStyles.Bold };
        DefaultHeader = GetDefaultByName( "header" )
         ?? (FormatCodeStyleSet) DefaultBase with
            {
                Color = IndexedColors.Aqua,
                FontStyles = FontStyles.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                WrapText = true
            };

        DefaultDouble = GetDefaultByType<DoubleStyleSet>( "double",
                                                          new DoubleStyle
                                                          {
                                                              DecimalPlaces = 2,
                                                              FontHeightInPoints = DefaultFontHeightInPoints,
                                                              FontName = DefaultFontName
                                                          } );

        DefaultInteger = GetDefaultByType<IntegerStyleSet>( "integer",
                                                            new IntegerStyle
                                                            {
                                                                FontHeightInPoints = DefaultFontHeightInPoints,
                                                                FontName = DefaultFontName
                                                            } );

        DefaultPercent = DefaultDouble with { IsPercent = true, DecimalPlaces = 1 };

        DefaultDate = GetDefaultByType<DateStyleSet>( "date",
                                                      new DateTimeStyle
                                                      {
                                                          FontHeightInPoints = DefaultFontHeightInPoints,
                                                          FontName = DefaultFontName
                                                      } );

        DefaultBoolean = baseStyleSet;
        DefaultUngroupedInteger = DefaultInteger with { GroupSeparator = string.Empty };

        DefaultUngroupedIntegerHeader = DefaultInteger with
        {
            GroupSeparator = string.Empty,
            HorizontalAlignment = HorizontalAlignment.Center,
            Color = IndexedColors.Aqua,
            FontStyles = FontStyles.Bold
        };
    }

    private void LoadConfiguredStyles( List<BaseStyle> defined )
    {
        foreach (var styleConfig in defined)
        {
            var styleSet = styleConfig switch
            {
                DateTimeStyle dtConfig => (StyleSetBase)new DateStyleSet(dtConfig),
                IntegerStyle intConfig => new IntegerStyleSet(intConfig),
                DoubleStyle doubleConfig => new DoubleStyleSet(doubleConfig),
                FormatCodeStyle codeConfig => new FormatCodeStyleSet(codeConfig),
                SimpleNumberStyle simpleConfig => new SimpleNumberStyleSet(simpleConfig),
                _ => null
            };

            if (styleSet == null)
            {
                _logger?.UnsupportedStyleConfig(styleConfig.GetType());
                continue;
            }

            _namedStyles.Add(styleSet.StyleName, styleSet);

            // the first style of each type is the default for that type
            if (!_typedStyles.TryGetValue(styleSet.GetType(), out _))
                _typedStyles.Add(styleSet.GetType(), styleSet);
        }

    }

    private StyleSetBase? GetDefaultByName( string styleName )
    {
        if( _namedStyles.TryGetValue(styleName, out var retVal))
            return retVal;

        _logger?.UndefinedDefaultStyle( styleName );
        return null;
    }

    private TStyleSet GetDefaultByType<TStyleSet>( string styleName, BaseStyle baseConfig )
        where TStyleSet : StyleSetBase
    {
        if( GetDefaultByName( styleName ) is TStyleSet retVal )
            return retVal;

        _typedStyles.TryGetValue(typeof(TStyleSet), out var temp);

        if (temp is TStyleSet temp2)
            retVal = temp2;
        else
            retVal = (TStyleSet) Activator.CreateInstance( typeof( TStyleSet ), [baseConfig] )!;

        return retVal;
    }

    public string DefaultFontName { get; }
    public int DefaultFontHeightInPoints { get; }

    public StyleSetBase DefaultBase { get; }
    public StyleSetBase DefaultTitle { get; }
    public StyleSetBase DefaultHeader { get; }
    public DoubleStyleSet DefaultDouble { get; }
    public IntegerStyleSet DefaultInteger { get; }
    public IntegerStyleSet DefaultUngroupedInteger { get; }
    public IntegerStyleSet DefaultUngroupedIntegerHeader { get; }
    public DoubleStyleSet DefaultPercent { get; }
    public DateStyleSet DefaultDate { get; }
    public StyleSetBase DefaultBoolean { get; }

    public ICellStyle? ResolveCellStyle( IWorkbook workbook, StyleSetBase? styleSet )
    {
        if( styleSet == null )
            return null;

        var matches = _cachedStyles.Where(x => x.StyleSet.ValuesEqual(styleSet))
                                   .ToList();

        if (matches.Count > 0)
            return matches.First().CellStyle;

        var retVal = styleSet.CreateCellStyle(workbook);
        _cachedStyles.Add(new ResolvedStyle(styleSet, retVal));

        return retVal;
    }

    public StyleSetBase this[ string name ] => _namedStyles.GetValueOrDefault( name, DefaultBase );

    public StyleSetBase this[ Type type ] =>
        _typedStyles.TryGetValue( type, out var retVal ) ? retVal : GetDefaultForType( type );

    private StyleSetBase GetDefaultForType( Type type ) =>
        Type.GetTypeCode( type ) switch
        {
            TypeCode.Double => DefaultDouble,
            TypeCode.Single => DefaultDouble,
            TypeCode.Int32 => DefaultInteger,
            TypeCode.Int16 => DefaultInteger,
            TypeCode.Int64 => DefaultInteger,
            TypeCode.DateTime => DefaultDate,
            _ => DefaultBase
        };
}
