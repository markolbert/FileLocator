using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class RecordFilter<TRaw> : IRecordFilter<TRaw>
    where TRaw : class
{
    private readonly ILogger? _logger;

    public RecordFilter(
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );
    }

    public virtual bool Include( TRaw record ) => true;

    bool IRecordFilter.Include( object? record )
    {
        if( record is TRaw castRecord )
            return Include( castRecord );

        _logger?.UnexpectedType( typeof( TRaw ), record?.GetType() ?? typeof(object) );

        return false;
    }
}
