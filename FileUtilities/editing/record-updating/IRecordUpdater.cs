using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public interface IRecordUpdater
{
    Type EntityType { get; }

    bool UpdateRecord( object entity );
}

public interface IRecordUpdater<in TEntity> : IRecordUpdater
{
    bool UpdateRecord(TEntity entity);
}

public abstract class EntityUpdater<TEntity> : IRecordUpdater<TEntity>
    where TEntity : class
{
    protected EntityUpdater(
        IUpdateRecorder2 updateRecorder,
        Dictionary<int, TEntity> adjustments,
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );

        UpdateRecorder = updateRecorder;
        Adjustments = adjustments;
    }

    protected ILogger? Logger { get; }
    protected IUpdateRecorder2 UpdateRecorder { get; }
    protected Dictionary<int, TEntity> Adjustments { get; }

    public Type EntityType => typeof( TEntity );

    public abstract bool UpdateRecord( TEntity entity );

    protected abstract int GetKeyValue( TEntity entity );

    bool IRecordUpdater.UpdateRecord( object entity )
    {
        if( entity is TEntity castEntity )
            return UpdateRecord( castEntity );

        Logger?.InvalidTypeAssignment( entity.GetType(), EntityType );

        return false;
    }
}
