using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class EntityCleaner<TEntity> : IEntityCleaner<TEntity>
    where TEntity : class
{
    private readonly List<IFieldCleaner> _fieldProcessors = [];

    protected EntityCleaner(
        IUpdateRecorder updateRecorder,
        IUpdateRecorder2 updateRecorder2,
        IFieldReplacements<TEntity>? fieldReplacements,
        ILoggerFactory? loggerFactory
    )
    {
        FieldReplacements = fieldReplacements;
        UpdateRecorder = updateRecorder;
        UpdateRecorder2 = updateRecorder2;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public IFieldReplacements? FieldReplacements { get; }

    public IUpdateRecorder UpdateRecorder { get; }
    public IUpdateRecorder2 UpdateRecorder2 { get; }
    public Type EntityType => typeof( TEntity );

    public virtual bool Initialize() => true;

    protected void AddFieldCleaner<TTgtProp>(
        Expression<Func<TEntity, int>> keyExpr,
        Expression<Func<TEntity, TTgtProp>> propExpr,
        params Func<IFieldCleaner, TEntity, CleanerResult>[] cleaners
    )
    {
        var fieldProcessor = new FieldCleaner<TEntity, TTgtProp>( keyExpr, propExpr, UpdateRecorder, true, LoggerFactory );
        _fieldProcessors.Add( fieldProcessor );

        foreach( var modifier in cleaners )
        {
            fieldProcessor.AddCleaner( modifier );
        }
    }

    protected void AddFieldCleaner<TSrcProp, TTgtProp>(
        Expression<Func<TEntity, int>> keyExpr,
        Expression<Func<TEntity, TSrcProp>> srcPropExpr,
        Expression<Func<TEntity, TTgtProp>> tgtPropExpr,
        params Func<IFieldCleaner, TEntity, CleanerResult>[] cleaners
    )
    {
        var fieldProcessor =
            new FieldToFieldCleaner<TEntity, TSrcProp, TTgtProp>( keyExpr,
                                                                srcPropExpr,
                                                                tgtPropExpr,
                                                                UpdateRecorder,
                                                                true,
                                                                LoggerFactory );
        _fieldProcessors.Add( fieldProcessor );

        foreach( var modifier in cleaners )
        {
            fieldProcessor.AddCleaner( modifier );
        }
    }

    public virtual void CleanFields( TEntity entity )
    {
        FieldReplacements?.ApplyTweaks(entity);

        foreach ( var processor in _fieldProcessors )
        {
            processor.ProcessEntityFields( entity );
        }
    }

    void IEntityCleaner.CleanFields( object entity )
    {
        if( entity is TEntity castEntity )
            CleanFields(castEntity);
        else Logger?.UnexpectedType( typeof( TEntity), entity.GetType() );
    }
}