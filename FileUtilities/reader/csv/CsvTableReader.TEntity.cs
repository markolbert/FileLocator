using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader<TEntity> : ITableReader<TEntity, ImportContext>
    where TEntity : class, new()
{
    private readonly IRecordFilter<TEntity>? _filter;
    private readonly IEntityCleaner<TEntity>? _cleaner;

    private FileStream? _fs;
    private StreamReader? _reader;
    private CsvReader? _csvReader;

    public CsvTableReader(
        IRecordFilter<TEntity>? filter = null,
        IEntityCleaner<TEntity>? cleaner = null,
        ILoggerFactory? loggerFactory = null
        )
    {
        _filter = filter;
        _cleaner = cleaner;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );

    public IEnumerable<TEntity> GetData(ImportContext context)
    {
        if (!InitializeInternal(context, out var classMap))
            yield break;

        foreach( var fieldToIgnore in context.FieldsToIgnore )
        {
            var fieldMap = classMap!.MemberMaps.FirstOrDefault(
                mm => mm.Data.Names.Any( n => n.Equals( fieldToIgnore, StringComparison.OrdinalIgnoreCase ) ) );

            if( fieldMap == null )
            {
                Logger?.IgnoreFieldNotFound( typeof( TEntity ), fieldToIgnore );
                yield break;
            }

            fieldMap.Ignore( true );
        }

        try
        {
            _fs = File.Open(context.ImportPath, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_fs);

            _csvReader = new CsvReader(_reader, CultureInfo.InvariantCulture);
            _csvReader.Context.RegisterClassMap(classMap!);
        }
        catch (Exception ex)
        {
            Logger?.FileParsingError(context.ImportPath, ex.Message);
            Dispose();

            yield break;
        }

        foreach( var record in _csvReader.GetRecords<TEntity>()
                                         .Where( x => ( _filter == null || _filter.Include( x ) ) ) )
        {
            _cleaner?.CleanFields( record );

            yield return record;
        }

        CompleteImport();
    }

    protected virtual bool Initialize() => true;

    private bool InitializeInternal( ImportContext context, out ClassMap<TEntity>? classMap )
    {
        classMap = null;

        if (!File.Exists(context.ImportPath))
        {
            Logger?.FileNotFound(context.ImportPath);
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

            if (attr.ConverterType != null && attr.TryCreateConverter(out var converter, LoggerFactory) && converter != null)
                propMap.TypeConverter(converter);
        }

        if( _cleaner == null )
        {
            if (!string.IsNullOrWhiteSpace(context.ImportPath) && File.Exists(context.TweaksPath))
                Logger?.UnneededTweaksFile(GetType(), context.TweaksPath);

            return Initialize();
        }

        if (_cleaner.FieldReplacements == null)
            return _cleaner.Initialize() && Initialize();

        if( string.IsNullOrWhiteSpace( context.TweaksPath ) )
        {
            Logger?.UndefinedPath( nameof( CsvTableReader<TEntity> ) );
            return false;
        }

        if( !File.Exists( context.TweaksPath ) )
        {
            Logger?.FileNotFound( context.TweaksPath );
            return false;
        }

        _cleaner.FieldReplacements.Load(context.TweaksPath!);

        return _cleaner.Initialize() && Initialize();
    }

    protected virtual void CompleteImport()
    {
        // release the file!
        if( _fs != null )
        {
            _fs.Close();
            _fs.Dispose();
            _fs = null;
        }

        // save whatever changes/updates were recorded
        _cleaner?.UpdateRecorder.SaveChanges();
    }

    public void Dispose()
    {
        _fs?.Dispose();
        _reader?.Dispose();
        _csvReader?.Dispose();
    }

    bool ITableReader.TryGetData(
        ImportContext context,
        out IEnumerable<object> data
    )
    {
        data = GetData(context);
        return true;
    }
}
