namespace J4JSoftware.FileUtilities;

public interface IEntityFilter
{
    bool Include( object entity );
}

public interface IEntityFilter<in TEntity> : IEntityFilter
    where TEntity : class
{
    bool Include( TEntity entity );
}
