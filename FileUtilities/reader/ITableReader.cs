namespace J4JSoftware.FileUtilities;

public interface ITableReader : IDisposable
{
    Type ImportedType { get; }

    HashSet<int> GetReplacementIds();

    bool TryGetData( ImportContext context, out IEnumerable<object>? data );
}

public interface ITableReader<out TEntity, in TContext> : ITableReader
    where TEntity : class
    where TContext : ImportContext
{
    IEnumerable<TEntity> GetData(TContext context);
}
