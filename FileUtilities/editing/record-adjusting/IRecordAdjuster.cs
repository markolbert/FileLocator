namespace J4JSoftware.FileUtilities;

public interface IRecordAdjuster
{
    Type EntityType { get; }

    bool AdjustRecord( object entity );
}

public interface IRecordAdjuster<in TEntity> : IRecordAdjuster
{
    bool UpdateRecord(TEntity entity);
}