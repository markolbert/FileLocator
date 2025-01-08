using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

// this class manages field adjusters for an entity of type
// TEntity, discovering and calling whatever field adjusters
// are defined for its properties
public abstract partial class EntityPropertyAdjuster<TEntity> : IEntityPropertyAdjuster<TEntity>
    where TEntity : class
{
    private readonly IUpdateRecorder2 _updateRecorder;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly PropertyAdjusters _adjusters = [];

    protected EntityPropertyAdjuster(
        IUpdateRecorder2 updateRecorder,
        ILoggerFactory? loggerFactory
    )
    {
        _updateRecorder = updateRecorder;
        _loggerFactory = loggerFactory;

        Logger = loggerFactory?.CreateLogger( GetType() );

        CreateAdjusters();
    }

    private void CreateAdjusters()
    {
        foreach( var propInfo in typeof( TEntity ).GetProperties() )
        {
            if( !_adjusters.TryGetValue( propInfo.Name, out var curPropAdjusters ) )
            {
                curPropAdjusters = new AdjusterInfo( propInfo, _loggerFactory );
                _adjusters.Add( curPropAdjusters );
            }

            foreach ( var adjusterAttr in propInfo.GetCustomAttributes<PropertyAdjusterAttribute>() )
            {
                if( adjusterAttr.TryCreateAdjuster( _loggerFactory, out var propAdjuster ) )
                {
                    if( propAdjuster!.PropertyType == propInfo.PropertyType )
                        curPropAdjusters.Adjusters.Add( propAdjuster );
                    else Logger?.InvalidTypeAssignment( propAdjuster.PropertyType, propInfo.PropertyType );
                }
                else Logger?.FieldAdjusterNotCreated( typeof( TEntity ), propInfo.Name, adjusterAttr.AdjusterType );
            }
        }
    }

    protected ILogger? Logger { get; }

    public Type EntityType => typeof( TEntity );

    public bool AdjustEntity( TEntity entity )
    {
        foreach( var adjusterInfo in _adjusters )
        {
            if( !adjusterInfo.TryGetPropertyValue( entity, out var initialValue ) )
                return false;

            var adjusted = initialValue;

            foreach( var propAdjuster in adjusterInfo.Adjusters )
            {
                adjusted = propAdjuster.AdjustField( adjusted );
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

        return true;
    }

    public abstract void SaveAdjustmentInfo();

    protected abstract int GetKeyValue( TEntity entity );

    bool IEntityPropertyAdjuster.AdjustEntity( object entity )
    {
        if( entity is TEntity castEntity )
            return AdjustEntity( castEntity );

        Logger?.InvalidTypeAssignment( entity.GetType(), EntityType );

        return false;
    }
}
