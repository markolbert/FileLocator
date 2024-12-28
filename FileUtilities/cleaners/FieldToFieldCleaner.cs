using System.Linq.Expressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public record FieldToFieldCleaner<TEntity, TSrcProp, TTgtProp> : IFieldCleaner
    where TEntity : class
{
    private readonly Func<TEntity, int> _keyGetter;
    private readonly Func<TEntity, TSrcProp?> _srcPropGetter;
    private readonly Func<TEntity, TTgtProp> _tgtPropGetter;
    private readonly Action<TEntity, TTgtProp> _tgtPropSetter;
    private readonly List<Action<IFieldCleaner, IUpdateRecorder, TEntity>> _cleaners = [];

    private readonly ILogger? _logger;
    private readonly bool _isCleaner;
    private readonly bool _tgtPropIsNullable;

    public FieldToFieldCleaner(
        Expression<Func<TEntity, int>> keyPropExpr,
        Expression<Func<TEntity, TSrcProp>> srcPropExpr,
        Expression<Func<TEntity, TTgtProp>> tgtPropExpr,
        IUpdateRecorder updateRecorder,
        bool isCleaner,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );

        _keyGetter = keyPropExpr.Compile();
        _srcPropGetter = srcPropExpr.Compile();

        var exprHelper = new ExpressionHelpers(loggerFactory);

        if( exprHelper.TryGetPropertyInfo( tgtPropExpr, out var propInfo ) )
            FieldName = propInfo!.Name;
        else
        {
            _logger?.UnboundProperty(GetType(), tgtPropExpr.ToString(), "Unknown");
            FieldName = "Unknown";
        }

        _tgtPropGetter = tgtPropExpr.Compile();

        _tgtPropSetter = exprHelper.CreatePropertySetter( tgtPropExpr )
         ?? throw new FileUtilityException( GetType(),
                                            "ctor",
                                            $"Could not create target property setter from {tgtPropExpr}" );

        UpdateRecorder = updateRecorder;
        _isCleaner = isCleaner;
        _tgtPropIsNullable = typeof( TTgtProp ).IsClass;
    }

    public Type EntityType => typeof( TEntity );
    public string FieldName { get; }

    public IUpdateRecorder UpdateRecorder { get; }

    private void SetValue( TEntity entity, TTgtProp newValue )
    {
        var existingValue = _tgtPropGetter( entity );

        if( _isCleaner )
        {
            UpdateRecorder.RecordChange( EntityType,
                                         _keyGetter( entity ),
                                         FieldName,
                                         existingValue?.ToString() ?? Globals.NullText,
                                         newValue?.ToString() ?? Globals.NullText );
        }
        else
        {
            UpdateRecorder.MarkAsInvalid( EntityType,
                                          _keyGetter( entity ),
                                          FieldName,
                                          existingValue?.ToString() ?? Globals.NullText );
        }

        _tgtPropSetter( entity, newValue );
    }

    public void AddCleaner( Action<IFieldCleaner, IUpdateRecorder, TEntity> modifier ) => _cleaners.Add( modifier );

    public void ProcessEntityFields( TEntity entity )
    {
        foreach( var cleaner in _cleaners )
        {
            cleaner( this, UpdateRecorder, entity );
        }
    }

    public int GetKeyValue( TEntity entity ) => _keyGetter( entity );

    public object? GetSourceValue( object entity )
    {
        if( entity is TEntity castEntity )
            return _srcPropGetter( castEntity );

        _logger?.UnexpectedType(typeof( TEntity ), entity.GetType() );

        return null;
    }

    public object? GetTargetValue( object entity )
    {
        if( entity is TEntity castEntity )
            return _tgtPropGetter( castEntity );

        _logger?.UnexpectedType(typeof(TEntity), entity.GetType());

        return null;
    }

    bool IFieldCleaner.TryGetKeyValue( object entity, out int key )
    {
        key = -1;

        if( entity is TEntity castEntity )
        {
            key = _keyGetter( castEntity );
            return true;
        }

        _logger?.UnexpectedType( typeof( TEntity ), entity.GetType() );
        return false;
    }

    void IFieldCleaner.SetValue( object entity, object? newValue )
    {
        if( entity is not TEntity castEntity )
        {
            _logger?.UnexpectedType(typeof(TEntity), entity.GetType());
            return;
        }

        if( newValue is TTgtProp castNewValue )
            SetValue( castEntity, castNewValue );
        else
        {
            if( _tgtPropIsNullable )
            {
                var nullableGeneric = typeof( Nullable<> ).MakeGenericType( typeof( TTgtProp ) );
#pragma warning disable CS8604 // Possible null reference argument.
                SetValue( castEntity, (TTgtProp?) Activator.CreateInstance( nullableGeneric ) );
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else _logger?.UnexpectedType( typeof( TTgtProp ), newValue?.GetType() ?? typeof( object ) );
        }
    }

    void IFieldCleaner.ProcessEntityFields( object entity )
    {
        if( entity is not TEntity castEntity )
        {
            _logger?.UnexpectedType(typeof(TEntity), entity.GetType());
            return;
        }

        ProcessEntityFields( castEntity );
    }
}
