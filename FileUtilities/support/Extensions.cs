using System.Reflection;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static class Extensions
{
    public static bool TypeAcceptsArguments( this Type type, params Type[] args )
    {
        foreach( var ctor in type.GetConstructors() )
        {
            var ctorParams = ctor.GetParameters();

            if( ctorParams.Length != args.Length )
                continue;

            var typesMatch = true;

            for( var idx = 0; idx < ctorParams.Length; idx++ )
            {
                typesMatch &= args[ idx ].IsAssignableTo( ctorParams[ idx ].ParameterType );
                if( !typesMatch )
                    break;
            }

            if( typesMatch )
                return true;
        }

        return false;
    }

    public static ClassMap CreateClassMap( Type entityType, ILoggerFactory? loggerFactory )
    {
        var defaultMapType = typeof( DefaultClassMap<> ).MakeGenericType( entityType );

        ClassMap classMap;

        try
        {
            classMap = ( Activator.CreateInstance( defaultMapType ) as ClassMap )!;
        }
        catch( Exception ex )
        {
            throw new FileUtilityException( typeof( Extensions ),
                                        nameof( CreateClassMap ),
                                        $"Could not create instance of {nameof( ClassMap )}",
                                        ex );
        }

        // grab any properties we're supposed to exclude from the map we're building
        var excluded = entityType.GetCustomAttribute<CsvExcludedAttribute>();

        foreach( var propInfo in entityType.GetProperties() )
        {
            if( excluded?.ExcludedProperties.Any( ep => ep.Equals( propInfo.Name, StringComparison.OrdinalIgnoreCase ) )
            ?? false )
                continue;

            var attr = propInfo.GetCustomAttribute<CsvFieldAttribute>();
            if( attr == null )
                continue;

            var propMap = classMap.Map( entityType, propInfo ).Name( attr.CsvHeader );

            if( attr.ConverterType != null && attr.TryCreateConverter( out var converter, loggerFactory ) )
                propMap.TypeConverter( converter! );
        }

        return classMap;
    }

    public static bool TryCreateConverter( this CsvFieldAttribute attr, out ITypeConverter? converter, ILoggerFactory? loggerFactory )
    {
        converter = null;

        var logger = loggerFactory?.CreateLogger( nameof( TryCreateConverter ) );

        if( attr.ConverterType == null )
            return false;

        if (!attr.ConverterType.IsAssignableTo(typeof(ITypeConverter)))
        {
            logger?.InvalidTypeAssignment(attr.ConverterType, typeof(ITypeConverter));
            return false;
        }

        var ctorParams = attr.ConverterType.TypeAcceptsArguments( typeof( ILoggerFactory ) )
            ? new object?[] { loggerFactory }
            : attr.ConverterType.TypeAcceptsArguments()
                ? []
                : null;

        if( ctorParams == null )
        {
            logger?.NoMatchingConstructor();
            return false;
        }

        try
        {
            converter = (ITypeConverter) Activator.CreateInstance( attr.ConverterType, ctorParams )!;
            return true;
        }
        catch (Exception ex)
        {
            logger?.InstanceCreationFailed( attr.ConverterType, ex.Message );
            
            throw new FileUtilityException(attr.ConverterType,
                                        nameof(CsvFieldAttribute),
                                        $"Could not create instance of {attr.ConverterType}",
                                        ex);
        }
    }
}
