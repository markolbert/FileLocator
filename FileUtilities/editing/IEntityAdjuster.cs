namespace J4JSoftware.FileUtilities;

public interface IEntityAdjuster
{
    Type EntityType { get; }
    bool IsValid { get; }

    bool Initialize( ImportContext context );
    HashSet<int> GetReplacementIds();

    bool AdjustEntity( object entity );

    void RecordSuccessfulAdjustment(
        int key,
        string field,
        ChangeSource source,
        string? priorValue,
        string? adjValue,
        string? reason = null
    );

    void SaveAdjustmentRecords();
}

public interface IEntityAdjuster<in TEntity> : IEntityAdjuster
{
    bool AdjustEntity(TEntity entity);
}