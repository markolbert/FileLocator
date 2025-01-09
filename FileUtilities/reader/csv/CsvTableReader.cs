using CsvHelper;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader : ITableReader
{
    private readonly IRecordFilter<DataRecord>? _filter;
    private readonly IEntityCorrector<DataRecord>? _propAdjuster;

    private FileStream? _fs;
    private StreamReader? _reader;

    public CsvTableReader(
        IRecordFilter<DataRecord>? filter = null,
        IEntityCorrector<DataRecord>? propAdjuster = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        _filter = filter;
        _propAdjuster = propAdjuster;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger(GetType());
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected CsvReader? CsvReader { get; private set; }
    protected int CurrentRecord { get; private set; }

    public Type ImportedType => typeof( DataRecord );

    public IEnumerable<DataRecord> GetData( ImportContext context )
    {
        if (!File.Exists(context.ImportPath))
        {
            Logger?.FileNotFound(context.ImportPath);
            yield break;
        }

        // initialize the filter, if one exists
        if( !_filter?.Initialize() ?? false )
            yield break;

        // finally, complete whatever custom reader initialization
        // may be defined
        if( !Initialize() )
            yield break;

        CurrentRecord = 0;

        try
        {
            _fs = File.Open(context.ImportPath, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_fs);

            CsvReader = new CsvReader(_reader, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Logger?.FileParsingError(context.ImportPath, ex.Message);
            Dispose();

#pragma warning disable CA2200
            // ReSharper disable once PossibleIntendedRethrow
            throw ex;
#pragma warning restore CA2200
        }

        var headerRead = false;
        var headers = new List<string>();

        while (CsvReader.Read())
        {
            if (!headerRead && context.HasHeaders)
            {
                if (!CsvReader.ReadHeader())
                {
                    Logger?.HeaderUnreadable(context.ImportPath);
                    yield break;
                }

                headerRead = true;
                headers = CsvReader.HeaderRecord!.ToList();

                continue;
            }

            CurrentRecord++;

            var curRecord = CreateDataRecord(context.ImportPath, headers);

            if( !_propAdjuster?.AdjustEntity( curRecord ) ?? false )
                yield break;

            if (_filter != null && !_filter.Include(curRecord))
                continue;

            yield return curRecord;
        }

        OnReadingEnded();
    }

    protected virtual bool Initialize() => true;

    //private bool InitializeInternal( ImportContext context )
    //{
    //    if (_cleaner == null)
    //        return Initialize();

    //    if (_cleaner.FieldReplacements == null)
    //        return _cleaner.Initialize() && Initialize();

    //    if (string.IsNullOrWhiteSpace(context.TweaksPath))
    //    {
    //        Logger?.UndefinedPath(nameof(CsvTableReader));
    //        return false;
    //    }

    //    if (!File.Exists(context.TweaksPath))
    //    {
    //        Logger?.FileNotFound(context.TweaksPath);
    //        return false;
    //    }

    //    _cleaner.FieldReplacements.Load(context.TweaksPath!);

    //    return _cleaner.Initialize() && Initialize();
    //}

    // CsvReader will always be non-null when this is called
    protected virtual DataRecord CreateDataRecord(string importPath, List<string> headers )
    {
        var retVal = new DataRecord( CurrentRecord, headers );

        for( var colIdx = 0; colIdx < CsvReader!.ColumnCount; colIdx++ )
        {
            if( retVal.AddValue(colIdx, CsvReader[colIdx]!))
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
        _propAdjuster?.SaveAdjustmentInfo();
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
}
