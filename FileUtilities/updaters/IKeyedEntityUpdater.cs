namespace J4JSoftware.FileUtilities;

public interface IKeyedEntityUpdater
{
    IUpdateRecorder UpdateRecorder { get; }
    Type EntityType { get; }
    ITweaks? Tweaks { get; }

    bool Initialize();

    void ProcessEntityFields( object entity );
}

public interface IKeyedEntityUpdater<in TEntity> : IKeyedEntityUpdater
    where TEntity : class
{
    void ProcessEntityFields( TEntity entity );
}