using System.Reflection;
using System.Runtime.CompilerServices;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvTableReader<TEntity>( ILoggerFactory? loggerFactory = null )
    : CsvTableReaderBase<TEntity>( loggerFactory ), ITableReader<TEntity, ImportContext>
    where TEntity : class, new()
{
    private ClassMap<TEntity>? _classMap;

    public Type ImportedType => typeof( TEntity );

    public HashSet<int> GetReplacementIds() => EntityAdjuster?.GetReplacementIds() ?? [];

    public IEnumerable<TEntity> GetData( ImportContext context )
    {
        if( !BeginGetData( context ) )
            yield break;

        foreach( var record in CsvReader!.GetRecords<TEntity>()
                                         .Where( x => Filter == null || Filter.Include( x ) ) )
        {
            if( !EntityAdjuster?.AdjustEntity( record ) ?? false )
                yield break;

            yield return record;
        }

        OnReadingEnded();
    }

    protected override bool BeginGetData( ImportContext context )
    {
        if( !base.BeginGetData( context ) )
            return false;

        if( !InitializeClassMap( context ) )
            return false;

        try
        {
            CsvReader!.Context.RegisterClassMap( _classMap! );
        }
        catch( Exception ex )
        {
            Logger?.InvalidClassMap( typeof( TEntity ).Name, ex.Message );
            return false;
        }

        return true;
    }

    public async IAsyncEnumerable<TEntity> GetDataAsync(
        ImportContext context,
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        if( !BeginGetData( context ) )
            yield break;

        await foreach( var record in CsvReader!.GetRecordsAsync<TEntity>( ctx )
                                               .Where( x => Filter == null || Filter.Include( x ) )
                                               .WithCancellation( ctx ) )
        {
            if( !EntityAdjuster?.AdjustEntity( record ) ?? false )
                yield break;

            yield return record;
        }

        OnReadingEnded();
    }

    private bool InitializeClassMap( ImportContext context )
    {
        var defaultMapType = typeof( DefaultClassMap<> ).MakeGenericType( ImportedType );

        try
        {
            _classMap = ( Activator.CreateInstance( defaultMapType ) as ClassMap<TEntity> )!;
        }
        catch( Exception ex )
        {
            Logger?.InstanceCreationFailed( typeof( ClassMap<TEntity> ), ex.Message );
            return false;
        }

        foreach( var propInfo in ImportedType.GetProperties() )
        {
            // skip fields that we're supposed to ignore
            if( context.PropertiesToIgnore.Any( ep => ep.Equals( propInfo.Name, StringComparison.OrdinalIgnoreCase ) ) )
                continue;

            var attr = propInfo.GetCustomAttribute<CsvFieldAttribute>();
            if( attr == null )
                continue;

            var propMap = _classMap.Map( ImportedType, propInfo ).Name( attr.CsvFieldName );

            if( attr.ConverterType != null
            && attr.TryCreateConverter( out var converter, LoggerFactory )
            && converter != null )
                propMap.TypeConverter( converter );
        }

        return true;
    }

    bool ITableReader.TryGetData(
        ImportContext context,
        out IEnumerable<object>? data
    )
    {
        data = GetData( context );
        return true;
    }

    IAsyncEnumerable<object> ITableReader.GetObjectDataAsync(
        ImportContext context,
        CancellationToken ctx
    ) =>
        GetDataAsync( context, ctx );

    bool ITableReader.SetAdjuster( IEntityAdjuster? adjuster )
    {
        if( adjuster == null )
        {
            EntityAdjuster = null;
            return true;
        }

        if( adjuster is not IEntityAdjuster<TEntity> castAdjuster )
        {
            Logger?.InvalidTypeAssignment( adjuster.GetType(), typeof( IEntityAdjuster<TEntity> ) );
            return false;
        }

        EntityAdjuster = castAdjuster;
        return true;
    }

    bool ITableReader.SetFilter( IRecordFilter? filter )
    {
        if( filter == null )
        {
            Filter = null;
            return true;
        }

        if( filter is not IRecordFilter<TEntity> castFilter )
        {
            Logger?.InvalidTypeAssignment( filter.GetType(), typeof( IRecordFilter<TEntity> ) );
            return false;
        }

        Filter = castFilter;
        return true;
    }
}
