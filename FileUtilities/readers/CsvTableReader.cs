using CsvHelper;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Globalization;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader : ICsvTableReader
{
    private readonly IRecordFilter<DataRecord>? _filter;
    private readonly IKeyedEntityUpdater<DataRecord>? _entityUpdater;

    private ICsvContext? _source;
    private FileStream? _fs;
    private StreamReader? _reader;

    public CsvTableReader(
        IRecordFilter<DataRecord>? filter = null,
        IKeyedEntityUpdater<DataRecord>? entityUpdater = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        _filter = filter;
        _entityUpdater = entityUpdater;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger(GetType());
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected CsvReader? CsvReader { get; private set; }
    protected int CurrentRecord { get; private set; }

    public Type ImportedType => typeof( DataRecord );

    public ICsvContext? Source
    {
        get => _source;

        set
        {
            _source = value;

            if (_entityUpdater != null)
                _entityUpdater.Source = _source;
        }
    }

    public IEnumerator<DataRecord> GetEnumerator()
    {
        if (!InitializeInternal())
            yield break;

        try
        {
            _fs = File.Open(Source!.FilePath, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_fs);

            CsvReader = new CsvReader(_reader, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Logger?.FileParsingError(Source!.FilePath, ex.Message);
            Dispose();

            yield break;
        }

        var headerRead = false;

        while( CsvReader.Read() )
        {
            if( !headerRead && Source.HasHeader )
            {
                if( !CsvReader.ReadHeader() )
                {
                    Logger?.HeaderUnreadable( Source!.FilePath );
                    yield break;
                }

                headerRead = true;
                continue;
            }

            CurrentRecord++;

            var curRecord = CreateDataRecord();

            _entityUpdater?.ProcessEntityFields( curRecord );

            if( _filter != null && !_filter.Include( curRecord ) )
                continue;

            yield return curRecord;
        }

        OnReadingEnded();
    }

    protected virtual bool Initialize() => true;

    // CsvReaDER will always be non-null when this is called
    protected virtual DataRecord CreateDataRecord()
    {
        var retVal = new DataRecord( CurrentRecord );

        for( var colIdx = 0; colIdx < CsvReader!.ColumnCount; colIdx++ )
        {
            if( retVal.AddValue(colIdx, CsvReader[colIdx]))
                continue;

            Logger?.DuplicateColumnRead( Source!.FilePath, colIdx, CurrentRecord );
        }

        return retVal;
    }

    private bool InitializeInternal()
    {
        CurrentRecord = 0;

        if (Source == null)
        {
            Logger?.UndefinedImportSource();
            return false;
        }

        if (!File.Exists(Source.FilePath))
        {
            Logger?.FileNotFound(Source.FilePath);
            return false;
        }

        if (_entityUpdater == null)
            return Initialize();

        _entityUpdater.Source = Source;

        return _entityUpdater.Initialize() && Initialize();
    }

    protected virtual void OnReadingEnded()
    {
        // save whatever changes/updates were recorded
        _entityUpdater?.UpdateRecorder.SaveChanges();
    }

    public void Dispose()
    {
        _fs?.Dispose();
        _reader?.Dispose();
        CsvReader?.Dispose();
    }

    bool ITableReader.SetSource(object source)
    {
        if (source is ICsvContext castSource)
        {
            Source = castSource;
            return true;
        }

        Logger?.UnexpectedType(typeof(IFileContext), source.GetType());
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
