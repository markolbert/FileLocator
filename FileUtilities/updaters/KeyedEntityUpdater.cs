using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class KeyedEntityUpdater<TEntity> : IKeyedEntityUpdater<TEntity>
    where TEntity : class
{
    private readonly List<IFieldUpdater> _fieldProcessors = [];

    private IFileContext? _source;

    protected KeyedEntityUpdater(
        IUpdateRecorder updateRecorder,
        ITweaks<TEntity>? tweaks,
        ILoggerFactory? loggerFactory
    )
    {
        Tweaks = tweaks;
        UpdateRecorder = updateRecorder;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected ITweaks<TEntity>? Tweaks { get; }

    public IUpdateRecorder UpdateRecorder { get; }
    public Type EntityType => typeof( TEntity );

    public IFileContext? Source
    {
        get => _source;

        set
        {
            _source = value;

            if( Tweaks != null )
                Tweaks.Source = _source;
        }
    }

    public virtual bool Initialize()
    {
        if( Tweaks == null )
            return true;

        Tweaks.Source = Source;
        return Tweaks.Load();
    }

    protected void AddFieldCleaner<TTgtProp>(
        Expression<Func<TEntity, int>> keyExpr,
        Expression<Func<TEntity, TTgtProp>> propExpr,
        params Action<IFieldUpdater, IUpdateRecorder, TEntity>[] modifiers
    )
    {
        var fieldProcessor = new FieldUpdater<TEntity, TTgtProp>( keyExpr, propExpr, UpdateRecorder, true, LoggerFactory );
        _fieldProcessors.Add( fieldProcessor );

        foreach( var modifier in modifiers )
        {
            fieldProcessor.AddModifier( modifier );
        }
    }

    protected void AddFieldCleaner<TSrcProp, TTgtProp>(
        Expression<Func<TEntity, int>> keyExpr,
        Expression<Func<TEntity, TSrcProp>> srcPropExpr,
        Expression<Func<TEntity, TTgtProp>> tgtPropExpr,
        params Action<IFieldUpdater, IUpdateRecorder, TEntity>[] modifiers
    )
    {
        var fieldProcessor =
            new MultiFieldUpdater<TEntity, TSrcProp, TTgtProp>( keyExpr,
                                                                srcPropExpr,
                                                                tgtPropExpr,
                                                                UpdateRecorder,
                                                                true,
                                                                LoggerFactory );
        _fieldProcessors.Add( fieldProcessor );

        foreach( var modifier in modifiers )
        {
            fieldProcessor.AddModifier( modifier );
        }
    }

    public virtual void ProcessEntityFields( TEntity entity )
    {
        Tweaks?.ApplyTweaks(entity);

        foreach ( var processor in _fieldProcessors )
        {
            processor.ProcessEntityFields( entity );
        }
    }

    void IKeyedEntityUpdater.ProcessEntityFields( object entity )
    {
        if( entity is TEntity castEntity )
            ProcessEntityFields(castEntity);
        else Logger?.UnexpectedType( typeof( TEntity), entity.GetType() );
    }
}
