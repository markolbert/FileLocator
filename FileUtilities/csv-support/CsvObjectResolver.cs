using CsvHelper;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvObjectResolver : IObjectResolver
{
    [Flags]
    private enum ConstructorArguments
    {
        HasParameterlessConstructor = 1 << 0,
        OnlyILoggerFactory = 1 << 1,

        Unsupported = 0
    }

    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;
    private readonly Dictionary<Type, ConstructorArguments> _typeInfo = [];

    public CsvObjectResolver(
        ILoggerFactory? loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory?.CreateLogger( GetType() );
    }

    private bool IsTypeResolvable( Type type )
    {
        if( _typeInfo.TryGetValue( type, out var typeInfo ) )
            return typeInfo == ConstructorArguments.HasParameterlessConstructor
             || typeInfo.HasFlag( ConstructorArguments.OnlyILoggerFactory );

        if (type.TypeAcceptsArguments())
            typeInfo |= ConstructorArguments.HasParameterlessConstructor;

        if ( type.TypeAcceptsArguments( typeof( ILoggerFactory ) ) )
            typeInfo |= ConstructorArguments.OnlyILoggerFactory;

        _typeInfo.Add( type, typeInfo );

        return typeInfo != ConstructorArguments.Unsupported;
    }

    public object? Resolve( Type type, object[] ctorArgs )
    {
        if( !CanResolve( type ) )
            return null;

        try
        {
            switch( _typeInfo[ type ] )
            {
                case ConstructorArguments.OnlyILoggerFactory:
                    return Activator.CreateInstance( type, [_loggerFactory] )!;

                case ConstructorArguments.HasParameterlessConstructor:
                    return Activator.CreateInstance( type )!;

                default:
                    return null;
            }
        }
        catch( Exception )
        {
            return null;
        }
    }

    public T? Resolve<T>( params object[] constructorArgs ) => (T?) Resolve( typeof( T ), constructorArgs );

    public bool UseFallback { get; } = true;
    public Func<Type, bool> CanResolve => IsTypeResolvable;
    public Func<Type, object[], object?> ResolveFunction => Resolve;
}
