using System.Collections;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader<TEntity> : ICsvTableReader<TEntity>
    where TEntity : class, new()
{
    private readonly IRecordFilter<TEntity>? _filter;
    private readonly IKeyedEntityUpdater<TEntity>? _entityUpdater;

    private ICsvFileInfo? _source;
    private FileStream? _fs;
    private StreamReader? _reader;
    private CsvReader? _csvReader;

    public CsvTableReader(
        IRecordFilter<TEntity>? filter = null,
        IKeyedEntityUpdater<TEntity>? entityUpdater = null,
        ILoggerFactory? loggerFactory = null
        )
    {
        _filter = filter;
        _entityUpdater = entityUpdater;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );

    public ICsvFileInfo? Source
    {
        get => _source;

        set
        {
            _source = value;

            if( _entityUpdater != null )
                _entityUpdater.Source = _source;
        }
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        if (!InitializeInternal(out var classMap))
            yield break;

        try
        {
            _fs = File.Open(Source!.FilePath, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_fs);

            _csvReader = new CsvReader(_reader, CultureInfo.InvariantCulture);
            _csvReader.Context.RegisterClassMap(classMap);
        }
        catch (Exception ex)
        {
            Logger?.FileParsingError(Source!.FilePath, ex.Message);
            Dispose();

            yield break;
        }

        foreach( var record in _csvReader.GetRecords<TEntity>()
                                         .Where( x => x != null && ( _filter == null || _filter.Include( x ) ) ) )
        {
            _entityUpdater?.ProcessEntityFields( record );

            yield return record;
        }

        CompleteImport();
    }

    private bool InitializeInternal( out ClassMap<TEntity>? classMap )
    {
        classMap = null;

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

        var defaultMapType = typeof(DefaultClassMap<>).MakeGenericType(ImportedType);

        try
        {
            classMap = (Activator.CreateInstance(defaultMapType) as ClassMap<TEntity>)!;
        }
        catch (Exception ex)
        {
            Logger?.InstanceCreationFailed(typeof(ClassMap<TEntity> ), ex.Message);
            return false;
        }

        // grab any properties we're supposed to exclude from the map we're building
        var excluded = ImportedType.GetCustomAttribute<CsvExcludedAttribute>();

        foreach (var propInfo in ImportedType.GetProperties())
        {
            if (excluded?.ExcludedProperties.Any(ep => ep.Equals(propInfo.Name, StringComparison.OrdinalIgnoreCase))
             ?? false)
                continue;

            var attr = propInfo.GetCustomAttribute<CsvFieldAttribute>();
            if (attr == null)
                continue;

            var propMap = classMap.Map(ImportedType, propInfo).Name(attr.CsvHeader);

            if (attr.ConverterType != null && attr.TryCreateConverter(out var converter, LoggerFactory))
                propMap.TypeConverter(converter);
        }

        if( _entityUpdater == null )
            return Initialize();

        _entityUpdater.Source = Source;
            
        return _entityUpdater.Initialize() && Initialize();
    }

    protected virtual bool Initialize() => true;

    protected virtual void CompleteImport()
    {
        // save whatever changes/updates were recorded
        _entityUpdater?.UpdateRecorder.SaveChanges();
    }

    public void Dispose()
    {
        _fs?.Dispose();
        _reader?.Dispose();
        _csvReader?.Dispose();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ITableReader.SetSource( object source )
    {
        if( source is ICsvFileInfo castSource )
        {
            Source = castSource;
            return true;
        }

        Logger?.UnexpectedType(typeof(ICsvFileInfo), source.GetType());
        return false;
    }
}
