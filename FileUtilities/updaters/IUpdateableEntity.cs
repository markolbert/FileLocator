namespace J4JSoftware.FileUtilities;

public interface IUpdateableEntity
{
    void Update( object updater );
}

public interface IUpdateableEntity<out TEntity> : IUpdateableEntity
    where TEntity : class
{
    void Update( IKeyedEntityUpdater<TEntity> updater );
}
