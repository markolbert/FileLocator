namespace J4JSoftware.FileUtilities;

public interface IEntityAdjuster
{
    Type EntityType { get; }
    bool IsValid { get; }
    IUpdateRecorder? UpdateRecorder { get; set; }

    bool Initialize( ImportContext context );
    HashSet<int> GetReplacementIds();

    bool AdjustEntity( object entity );

    void SaveAdjustmentRecords();
}

public interface IEntityAdjuster<in TEntity> : IEntityAdjuster
{
    bool AdjustEntity(TEntity entity);
    void RecordAdjustment(
        TEntity entity,
        string field,
        ChangeSource source,
        string? priorValue,
        string? adjValue,
        string? reason = null
    );

}