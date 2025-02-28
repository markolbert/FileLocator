using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal class StyleSetCollection<TStyle>
    where TStyle : StyleSetBase
{
    private readonly Dictionary<Type, TStyle> _typeStyles = [];
    private readonly Dictionary<string, TStyle> _namedStyles = new( StringComparer.OrdinalIgnoreCase );

    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;

    public StyleSetCollection(
        ILoggerFactory? loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger( GetType() );
    }

    private StyleSetCollection( StyleSetCollection<TStyle> toCopy )
    {
        _loggerFactory = toCopy._loggerFactory;
        _logger = _loggerFactory?.CreateLogger( GetType() );

        foreach( var kvp in toCopy._namedStyles )
        {
            _namedStyles.Add( kvp.Key, kvp.Value );
        }

        foreach( var kvp in toCopy._typeStyles )
        {
            _typeStyles.Add( kvp.Key, kvp.Value );
        }
    }

    public StyleSetCollection<TStyle> Copy() => new( this );

    public void AddStyle( string name, TStyle style, Type? defaultForType = null )
    {
        name = name.Trim();

        if( _namedStyles.TryGetValue( name, out _ ) )
        {
            _logger?.ReplacedDuplicate( $"named {typeof( TStyle ).Name}", name );
            _namedStyles[ name ] = style;
        }
        else _namedStyles.Add( name, style );

        if( defaultForType == null )
            return;

        AddStyle( defaultForType, style );
    }

    public void AddStyle( Type type, TStyle style, string? name = null )
    {
        if( _typeStyles.TryGetValue( type, out _ ) )
        {
            _logger?.ReplacedDuplicate( $"named {typeof( TStyle ).Name}", type.Name );
            _typeStyles[ type ] = style;
        }
        else _typeStyles.Add( type, style );

        if( string.IsNullOrEmpty( name ) )
            return;

        AddStyle( name, style );
    }

    public TStyle? this[ string name ]
    {
        get
        {
            name = name.Trim();

            if( _namedStyles.TryGetValue( name, out var retVal ) )
                return retVal;

            _logger?.KeyNotFound( nameof( name ), name );
            return null;
        }
    }

    public TStyle? this[ Type type ]
    {
        get
        {
            if( _typeStyles.TryGetValue( type, out var retVal ) )
                return retVal;

            _logger?.KeyNotFound( nameof( type ), type.Name );
            return null;
        }
    }
}
