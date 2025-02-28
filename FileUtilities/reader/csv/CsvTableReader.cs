using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader( ILoggerFactory? loggerFactory = null ) : CsvTableReaderBase<DataRecord>( loggerFactory ), ITableReader
{
    public Type ImportedType => typeof( DataRecord );

    public HashSet<int> GetReplacementIds() => EntityAdjuster?.GetReplacementIds() ?? [];

    public IEnumerable<DataRecord> GetData( ImportContext context )
    {
        if (!BeginGetData(context))
            yield break;

        var headerRead = false;
        var headers = new List<string>();

        while( CsvReader!.Read() )
        {
            if (!ProcessHeader(context, ref headerRead, ref headers))
                yield break;

            switch( ProcessRecord( context, headers, out var curRecord ) )
            {
                case ProcessRecordResult.Okay:
                    yield return curRecord;
                    break;

                case ProcessRecordResult.Failed:
                    yield break;

                case ProcessRecordResult.FilteredOut:
                    // no op; just continue to next record
                    break;
            }
        }

        OnReadingEnded();
    }

    // The CancellationToken arg is needed to conform to the interface, but CsvReader does not 
    // support using it in ReadAsync()...which is weird
#pragma warning disable CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
    public async IAsyncEnumerable<DataRecord> GetDataAsync(ImportContext context, CancellationToken ctx)
#pragma warning restore CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
    {
        if( !BeginGetData( context ) )
            yield break;

        var headerRead = false;
        var headers = new List<string>();

        while (await CsvReader!.ReadAsync())
        {
            if( !ProcessHeader( context, ref headerRead, ref headers ) )
                yield break;

            switch (ProcessRecord(context, headers, out var curRecord))
            {
                case ProcessRecordResult.Okay:
                    yield return curRecord;
                    break;

                case ProcessRecordResult.Failed:
                    yield break;

                case ProcessRecordResult.FilteredOut:
                    // no op; just continue to next record
                    break;
            }
        }

        OnReadingEnded();
    }

    bool ITableReader.TryGetData(
        ImportContext context,
        out IEnumerable<object> data
    )
    {
        data = GetData( context );
        return true;
    }

    IAsyncEnumerable<object> ITableReader.GetObjectDataAsync( ImportContext context, CancellationToken ctx ) =>
        GetDataAsync( context, ctx );

    bool ITableReader.SetAdjuster( IEntityAdjuster? adjuster )
    {
        if (adjuster == null)
        {
            EntityAdjuster = null;
            return true;
        }

        if ( adjuster is not IEntityAdjuster<DataRecord> castAdjuster )
        {
            Logger?.InvalidTypeAssignment( adjuster.GetType(),
                                           typeof( IEntityAdjuster<DataRecord> ) );
            return false;
        }

        EntityAdjuster = castAdjuster;
        return true;
    }

    bool ITableReader.SetFilter( IRecordFilter? filter )
    {
        if (filter == null)
        {
            Filter = null;
            return true;
        }

        if ( filter is not IRecordFilter<DataRecord> castFilter )
        {
            Logger?.InvalidTypeAssignment( filter.GetType(), typeof( IRecordFilter<DataRecord> ) );
            return false;
        }

        Filter = castFilter;
        return true;
    }
}
