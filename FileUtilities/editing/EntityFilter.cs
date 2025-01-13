using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public abstract class EntityFilter<TEntity> : IEntityFilter<TEntity>
    where TEntity: class
{
    protected EntityFilter(
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILogger? Logger { get; }

    public abstract bool Include( TEntity entity );

    public bool Include(object entity)
    {
        if( entity is TEntity castEntity)
            return Include(castEntity);

        Logger?.InvalidTypeAssignment( entity.GetType(), typeof( TEntity ) );

        return false;
    }
}
