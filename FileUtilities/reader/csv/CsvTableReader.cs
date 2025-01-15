using CsvHelper;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader : ITableReader
{
    private FileStream? _fs;
    private StreamReader? _reader;

    public CsvTableReader(
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected CsvReader? CsvReader { get; private set; }
    protected int CurrentRecord { get; private set; }

    public Type ImportedType => typeof( DataRecord );

    public IRecordFilter<DataRecord>? Filter { get; set; }
    public IEntityAdjuster<DataRecord>? EntityAdjuster { get; set; }

    public HashSet<int> GetReplacementIds() => EntityAdjuster?.GetReplacementIds() ?? [];

    public IEnumerable<DataRecord> GetData( ImportContext context )
    {
        if( !File.Exists( context.ImportPath ) )
        {
            Logger?.FileNotFound( context.ImportPath );
            yield break;
        }

        // initialize the filter, if one exists
        if( !Filter?.Initialize() ?? false )
            yield break;

        if( !EntityAdjuster?.Initialize( context ) ?? false )
            yield break;

        // finally, complete whatever custom reader initialization
        // may be defined
        if( !Initialize() )
            yield break;

        CurrentRecord = 0;

        try
        {
            _fs = File.Open( context.ImportPath, FileMode.Open, FileAccess.Read );
            _reader = new StreamReader( _fs );

            CsvReader = new CsvReader( _reader, CultureInfo.InvariantCulture );
        }
        catch( Exception ex )
        {
            Logger?.FileParsingError( context.ImportPath, ex.Message );
            Dispose();

#pragma warning disable CA2200

            // ReSharper disable once PossibleIntendedRethrow
            throw ex;
#pragma warning restore CA2200
        }

        var headerRead = false;
        var headers = new List<string>();

        while( CsvReader.Read() )
        {
            if( !headerRead && context.HasHeaders )
            {
                if( !CsvReader.ReadHeader() )
                {
                    Logger?.HeaderUnreadable( context.ImportPath );
                    yield break;
                }

                headerRead = true;
                headers = CsvReader.HeaderRecord!.ToList();

                continue;
            }

            CurrentRecord++;

            var curRecord = CreateDataRecord( context.ImportPath, headers );

            if( !EntityAdjuster?.AdjustEntity( curRecord ) ?? false )
                yield break;

            if( Filter != null && !Filter.Include( curRecord ) )
                continue;

            yield return curRecord;
        }

        OnReadingEnded();
    }

    protected virtual bool Initialize() => true;

    // CsvReader will always be non-null when this is called
    protected virtual DataRecord CreateDataRecord( string importPath, List<string> headers )
    {
        var retVal = new DataRecord( CurrentRecord, headers );

        for( var colIdx = 0; colIdx < CsvReader!.ColumnCount; colIdx++ )
        {
            if( retVal.AddValue( colIdx, CsvReader[ colIdx ]! ) )
                continue;

            Logger?.DuplicateColumnRead( importPath, colIdx, CurrentRecord );
        }

        return retVal;
    }

    protected virtual void OnReadingEnded()
    {
        // release the file!
        if( _fs != null )
        {
            _fs.Close();
            _fs.Dispose();
            _fs = null;
        }

        // save whatever changes/updates were recorded
        EntityAdjuster?.SaveAdjustmentRecords();
    }

    public void Dispose()
    {
        _fs?.Dispose();
        _reader?.Dispose();
        CsvReader?.Dispose();
    }

    bool ITableReader.TryGetData(
        ImportContext context,
        out IEnumerable<object> data
    )
    {
        data = GetData( context );
        return true;
    }

    bool ITableReader.SetAdjuster( IEntityAdjuster? adjuster )
    {
        if (adjuster == null)
        {
            EntityAdjuster = null;
            return true;
        }

        if ( adjuster is not IEntityAdjuster<DataRecord> castAdjuster )
        {
            Logger?.InvalidTypeAssignment( adjuster?.GetType() ?? typeof( object ),
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
            Logger?.InvalidTypeAssignment( filter?.GetType() ?? typeof( object ), typeof( IRecordFilter<DataRecord> ) );
            return false;
        }

        Filter = castFilter;
        return true;
    }
}
