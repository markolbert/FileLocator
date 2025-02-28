using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReaderBase<TEntity>
    where TEntity : class
{
    protected enum ProcessRecordResult
    {
        Okay,
        Failed,
        FilteredOut
    }

    private StreamReader? _textReader;

    protected CsvTableReaderBase(
        ILoggerFactory? loggerFactory
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected CsvReader? CsvReader { get; private set; }
    protected int CurrentRecord { get; private set; }

    public IRecordFilter<TEntity>? Filter { get; set; }
    public IEntityAdjuster<TEntity>? EntityAdjuster { get; set; }

    protected virtual bool Initialize() => true;

    protected virtual bool BeginGetData( ImportContext context )
    {
        if( context.ImportStream == null )
        {
            Logger?.UndefinedStream();
            return false;
        }

        // initialize the filter, if one exists
        if( !Filter?.Initialize() ?? false )
            return false;

        if( !EntityAdjuster?.Initialize( context ) ?? false )
            return false;

        // finally, complete whatever custom reader initialization
        // may be defined
        if( !Initialize() )
            return false;

        CurrentRecord = 0;

        try
        {
            _textReader = new StreamReader( context.ImportStream );
            CsvReader = new CsvReader( _textReader, CultureInfo.InvariantCulture );
        }
        catch( Exception ex )
        {
            Logger?.StreamParsingError( ex.Message );
            Dispose();

            return false;
        }

        return true;
    }

    protected bool ProcessHeader( ImportContext context, ref bool headerRead, ref List<string> headers )
    {
        if( headerRead || !context.HasHeaders )
            return true;

        if( !CsvReader!.ReadHeader() )
        {
            Logger?.StreamHeaderUnreadable();
            return false;
        }

        headerRead = true;
        headers = CsvReader.HeaderRecord!.ToList();

        return true;
    }

    protected ProcessRecordResult ProcessRecord( ImportContext context, List<string> headers, out DataRecord curRecord )
    {
        CurrentRecord++;

        curRecord = CreateDataRecord( headers );

        if( !EntityAdjuster?.AdjustEntity( curRecord ) ?? false )
            return ProcessRecordResult.Failed;

        if( Filter != null && !Filter.Include( curRecord ) )
            return ProcessRecordResult.FilteredOut;

        return ProcessRecordResult.Okay;
    }

    // CsvReader will always be non-null when this is called
    protected virtual DataRecord CreateDataRecord( List<string> headers )
    {
        var retVal = new DataRecord( CurrentRecord, headers );

        for( var colIdx = 0; colIdx < CsvReader!.ColumnCount; colIdx++ )
        {
            if( retVal.AddValue( colIdx, CsvReader[ colIdx ]! ) )
                continue;

            Logger?.DuplicateColumnReadFromStream( colIdx, CurrentRecord );
        }

        return retVal;
    }

    protected virtual void OnReadingEnded()
    {
        // save whatever changes/updates were recorded
        EntityAdjuster?.SaveAdjustmentRecords();
    }

    public void Dispose()
    {
        _textReader?.Dispose();
        CsvReader?.Dispose();
    }
}
