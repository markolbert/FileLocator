using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public abstract class NpoiConverter<T>( ILoggerFactory? loggerFactory ) : INpoiConverter<T>
{
    protected ILogger? Logger { get; } = loggerFactory?.CreateLogger<NpoiConverter<T>>();

    public Type TargetType => typeof( T );

    public abstract T Convert( ICell cell );

    object? INpoiConverter.ConvertValue( ICell cell ) => Convert( cell );
}