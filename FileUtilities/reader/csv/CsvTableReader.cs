using CsvHelper;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader : ITableReader
{
    private readonly IRecordFilter<DataRecord>? _filter;
    private readonly IKeyedEntityUpdater<DataRecord>? _entityUpdater;

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

    public IEnumerable<DataRecord> GetData( ImportContext context )
    {
        if( !InitializeInternal(context) )
            yield break;

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

            _entityUpdater?.ProcessEntityFields(curRecord);

            if (_filter != null && !_filter.Include(curRecord))
                continue;

            yield return curRecord;
        }

        OnReadingEnded();
    }

    protected virtual bool Initialize() => true;

    private bool InitializeInternal( ImportContext context )
    {
        CurrentRecord = 0;

        if (!File.Exists(context.ImportPath))
        {
            Logger?.FileNotFound(context.ImportPath);
            return false;
        }

        if (_entityUpdater == null)
        {
            if ( !string.IsNullOrWhiteSpace(context.TweaksPath) && File.Exists(context.TweaksPath))
                Logger?.UnneededTweaksFile(GetType(), context.TweaksPath);

            return Initialize();
        }

        if (_entityUpdater.Tweaks == null)
            return _entityUpdater.Initialize() && Initialize();

        if (string.IsNullOrWhiteSpace(context.TweaksPath))
        {
            Logger?.UndefinedPath(nameof(CsvTableReader));
            return false;
        }

        if (!File.Exists(context.TweaksPath))
        {
            Logger?.FileNotFound(context.TweaksPath);
            return false;
        }

        _entityUpdater.Tweaks.Load(context.TweaksPath!);

        return _entityUpdater.Initialize() && Initialize();
    }

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
        _entityUpdater?.UpdateRecorder.SaveChanges();
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
