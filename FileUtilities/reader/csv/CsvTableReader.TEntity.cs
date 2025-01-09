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
    private readonly IEntityCorrector<TEntity>? _propAdjuster;

    private FileStream? _fs;
    private StreamReader? _reader;
    private CsvReader? _csvReader;

    public CsvTableReader(
        IRecordFilter<TEntity>? filter = null,
        IEntityCorrector<TEntity>? propAdjuster = null,
        ILoggerFactory? loggerFactory = null
        )
    {
        _filter = filter;
        _propAdjuster = propAdjuster;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );

    public IEnumerable<TEntity> GetData(ImportContext context)
    {
        if (!File.Exists(context.ImportPath))
        {
            Logger?.FileNotFound(context.ImportPath);
            yield break;
        }

        // initialize the filter, if one exists
        if (!_filter?.Initialize() ?? false)
            yield break;

        var classMap = GetClassMap();
        if( classMap == null )
            yield break;

        // finally, complete whatever custom reader initialization
        // may be defined
        if (!Initialize())
            yield break;

        //if (!InitializeInternal(context, out var classMap))
        //    yield break;

        foreach ( var fieldToIgnore in context.FieldsToIgnore )
        {
            var fieldMap = classMap.MemberMaps.FirstOrDefault(
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
            _csvReader.Context.RegisterClassMap(classMap);
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
            if( !_propAdjuster?.AdjustEntity( record ) ?? false )
                yield break;

            yield return record;
        }

        CompleteImport();
    }

    protected virtual bool Initialize() => true;

    private ClassMap<TEntity>? GetClassMap()
    {
        ClassMap<TEntity>? retVal;

        var defaultMapType = typeof(DefaultClassMap<>).MakeGenericType(ImportedType);

        try
        {
            retVal = (Activator.CreateInstance(defaultMapType) as ClassMap<TEntity>)!;
        }
        catch (Exception ex)
        {
            Logger?.InstanceCreationFailed(typeof(ClassMap<TEntity>), ex.Message);
            return null;
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

            var propMap = retVal.Map(ImportedType, propInfo).Name(attr.CsvHeader);

            if (attr.ConverterType != null && attr.TryCreateConverter(out var converter, LoggerFactory) && converter != null)
                propMap.TypeConverter(converter);
        }

        return retVal;
    }

    //private bool InitializeInternal( ImportContext context, out ClassMap<TEntity>? classMap )
    //{
    //    classMap = null;

    //    if (!File.Exists(context.ImportPath))
    //    {
    //        Logger?.FileNotFound(context.ImportPath);
    //        return false;
    //    }

    //    var defaultMapType = typeof(DefaultClassMap<>).MakeGenericType(ImportedType);

    //    try
    //    {
    //        classMap = (Activator.CreateInstance(defaultMapType) as ClassMap<TEntity>)!;
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger?.InstanceCreationFailed(typeof(ClassMap<TEntity> ), ex.Message);
    //        return false;
    //    }

    //    // grab any properties we're supposed to exclude from the map we're building
    //    var excluded = ImportedType.GetCustomAttribute<CsvExcludedAttribute>();

    //    foreach (var propInfo in ImportedType.GetProperties())
    //    {
    //        if (excluded?.ExcludedProperties.Any(ep => ep.Equals(propInfo.Name, StringComparison.OrdinalIgnoreCase))
    //         ?? false)
    //            continue;

    //        var attr = propInfo.GetCustomAttribute<CsvFieldAttribute>();
    //        if (attr == null)
    //            continue;

    //        var propMap = classMap.Map(ImportedType, propInfo).Name(attr.CsvHeader);

    //        if (attr.ConverterType != null && attr.TryCreateConverter(out var converter, LoggerFactory) && converter != null)
    //            propMap.TypeConverter(converter);
    //    }

    //    if( _cleaner == null )
    //        return Initialize();

    //    if (_cleaner.FieldReplacements == null)
    //        return _cleaner.Initialize() && Initialize();

    //    if( string.IsNullOrWhiteSpace( context.TweaksPath ) )
    //    {
    //        Logger?.UndefinedPath( nameof( CsvTableReader<TEntity> ) );
    //        return false;
    //    }

    //    if( !File.Exists( context.TweaksPath ) )
    //    {
    //        Logger?.FileNotFound( context.TweaksPath );
    //        return false;
    //    }

    //    _cleaner.FieldReplacements.Load(context.TweaksPath!);

    //    return _cleaner.Initialize() && Initialize();
    //}

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
        _propAdjuster?.SaveAdjustmentInfo();
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
