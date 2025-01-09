using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

// this class manages field adjusters for an entity of type
// TEntity, discovering and calling whatever field adjusters
// are defined for its properties
public abstract partial class EntityCorrector<TEntity> : IEntityCorrector<TEntity>
    where TEntity : class
{
    private readonly IUpdateRecorder2 _updateRecorder;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly PropertyAdjusters _propAdjusters = [];
    private readonly List<IRecordAdjuster> _recAdjusters = [];

    protected EntityCorrector(
        IUpdateRecorder2 updateRecorder,
        ILoggerFactory? loggerFactory
    )
    {
        _updateRecorder = updateRecorder;
        _loggerFactory = loggerFactory;

        Logger = loggerFactory?.CreateLogger( GetType() );

        if( !CreatePropertyAdjusters() )
            throw new Exception( $"Check logs, could not create property adjusters for {EntityType} " );

        if( !CreateRecordAdjusters())
            throw new Exception($"Check logs, could not create record adjusters for {EntityType} ");
    }

    private bool CreatePropertyAdjusters()
    {
        foreach( var propInfo in typeof( TEntity ).GetProperties() )
        {
            if( !_propAdjusters.TryGetValue( propInfo.Name, out var curPropAdjusters ) )
            {
                curPropAdjusters = new AdjusterInfo( propInfo, _loggerFactory );
                _propAdjusters.Add( curPropAdjusters );
            }

            foreach ( var adjusterAttr in propInfo.GetCustomAttributes<PropertyAdjusterAttribute>() )
            {
                if( adjusterAttr.TryCreateAdjuster( _loggerFactory, out var propAdjuster ) )
                {
                    if( propAdjuster!.PropertyType != propInfo.PropertyType )
                    {
                        Logger?.InvalidTypeAssignment(propAdjuster.PropertyType, propInfo.PropertyType);
                        return false;
                    }

                    // if we have an associated entity filter, make sure it refers
                    // to TEntity
                    IEntityFilter? filter = null;

                    if ( adjusterAttr.FilterType != null )
                    {
                        if( !adjusterAttr.TryCreateFilter( _loggerFactory, out filter ) )
                        {
                            Logger?.CannotCreateEntityFilter(adjusterAttr.FilterType);
                            return false;
                        }
                        else
                        {
                            var filterType = typeof( IEntityFilter<> ).MakeGenericType( EntityType );

                            if( filter!.GetType() != filterType )
                            {
                                Logger?.IncorrectEntityFilterType( filter.GetType(), filterType );
                                return false;
                            }
                        }
                    }

                    curPropAdjusters.Adjusters.Add( new FilteredAdjuster( propAdjuster, filter ) );
                }
                else Logger?.PropertyAdjusterNotCreated( typeof( TEntity ), propInfo.Name, adjusterAttr.AdjusterType );
            }
        }

        return true;
    }

    private bool CreateRecordAdjusters()
    {
        foreach (var adjusterAttr in EntityType.GetCustomAttributes<RecordAdjusterAttribute>())
        {
            if (adjusterAttr.TryCreateAdjuster(_loggerFactory, out var recAdjuster))
            {
                if (recAdjuster!.EntityType != EntityType)
                {
                    Logger?.InvalidTypeAssignment(recAdjuster.EntityType, EntityType);
                    return false;
                }

                _recAdjusters.Add( recAdjuster );
            }
            else Logger?.RecordAdjusterNotCreated(typeof(TEntity), adjusterAttr.RecordAdjusterType);
        }

        return true;
    }

    protected ILogger? Logger { get; }

    public Type EntityType => typeof( TEntity );

    public bool AdjustEntity( TEntity entity )
    {
        foreach( var adjusterInfo in _propAdjusters )
        {
            if( !adjusterInfo.TryGetPropertyValue( entity, out var initialValue ) )
                return false;

            var adjusted = initialValue;

            foreach( var filteredAdjuster in adjusterInfo.Adjusters )
            {
                if( !filteredAdjuster.EntityFilter?.Include( entity ) ?? false )
                    continue;

                adjusted = filteredAdjuster.PropertyAdjuster.AdjustField( adjusted );
            }

            if( !adjusterInfo.TrySetPropertyValue( entity, adjusted ) )
                return false;

            _updateRecorder.FieldRevised( EntityType,
                                          GetKeyValue( entity ),
                                          adjusterInfo.PropertyName,
                                          initialValue?.ToString(),
                                          adjusted?.ToString(),
                                          null );
        }

        foreach( var recAdjuster in _recAdjusters )
        {
            if( !recAdjuster.AdjustRecord( entity ) )
                return false;
        }

        return true;
    }

    public abstract void SaveAdjustmentInfo();

    protected abstract int GetKeyValue( TEntity entity );

    bool IEntityCorrector.AdjustEntity( object entity )
    {
        if( entity is TEntity castEntity )
            return AdjustEntity( castEntity );

        Logger?.InvalidTypeAssignment( entity.GetType(), EntityType );

        return false;
    }
}
