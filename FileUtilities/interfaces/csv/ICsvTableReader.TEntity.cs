namespace J4JSoftware.FileUtilities;

public interface ICsvTableReader<out TEntity> : ITableReader, IEnumerable<TEntity>
    where TEntity : class, new()
{
    ICsvContext? Source { get; set; }
}