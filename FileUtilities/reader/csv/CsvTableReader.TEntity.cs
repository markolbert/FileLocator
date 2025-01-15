using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader<TEntity> : ITableReader<TEntity, ImportContext>
    where TEntity : class, new()
{
    private FileStream? _fs;
    private StreamReader? _reader;
    private CsvReader? _csvReader;

    public CsvTableReader(
        ILoggerFactory? loggerFactory = null
        )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );

    public IRecordFilter<TEntity>? Filter { get; set; }
    public IEntityAdjuster<TEntity>? EntityAdjuster { get; set; }

    public HashSet<int> GetReplacementIds() => EntityAdjuster?.GetReplacementIds() ?? [];

    public IEnumerable<TEntity> GetData(ImportContext context)
    {
        if (!File.Exists(context.ImportPath))
        {
            Logger?.FileNotFound(context.ImportPath);
            yield break;
        }

        // initialize the filter, if one exists
        if (!Filter?.Initialize() ?? false)
            yield break;

        var classMap = GetClassMap();
        if( classMap == null )
            yield break;

        if( !EntityAdjuster?.Initialize( context ) ?? false )
            yield break;

        // finally, complete whatever custom reader initialization
        // may be defined
        if (!Initialize())
            yield break;

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
                                         .Where( x => ( Filter == null || Filter.Include( x ) ) ) )
        {
            if( !EntityAdjuster?.AdjustEntity( record ) ?? false )
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
        EntityAdjuster?.SaveAdjustmentRecords();
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

    bool ITableReader.SetAdjuster(IEntityAdjuster? adjuster)
    {
        if( adjuster == null )
        {
            EntityAdjuster = null;
            return true;
        }

        if (adjuster is not IEntityAdjuster<TEntity> castAdjuster)
        {
            Logger?.InvalidTypeAssignment(adjuster?.GetType() ?? typeof(object), typeof(IEntityAdjuster<TEntity>));
            return false;
        }
        
        EntityAdjuster = castAdjuster;
        return true;
    }

    bool ITableReader.SetFilter(IRecordFilter? filter)
    {
        if( filter == null )
        {
            Filter = null;
            return true;
        }

        if (filter is not IRecordFilter<TEntity> castFilter)
        {
            Logger?.InvalidTypeAssignment(filter?.GetType() ?? typeof(object), typeof(IRecordFilter<TEntity>));
            return false;
        }

        Filter = castFilter;
        return true;
    }
}
