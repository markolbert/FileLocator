namespace J4JSoftware.FileUtilities;

public interface IEntityCleaner
{
    IUpdateRecorder UpdateRecorder { get; }
    Type EntityType { get; }
    IFieldReplacements? FieldReplacements { get; }

    bool Initialize();

    void CleanFields( object entity );
}

public interface IEntityCleaner<in TEntity> : IEntityCleaner
    where TEntity : class
{
    void CleanFields( TEntity entity );
}