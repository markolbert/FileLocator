using System.Linq.Expressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public record MultiFieldUpdater<TEntity, TSrcProp, TTgtProp> : IFieldUpdater
    where TEntity : class
{
    private readonly Func<TEntity, int> _keyGetter;
    private readonly Func<TEntity, TSrcProp?> _srcPropGetter;
    private readonly Func<TEntity, TTgtProp> _tgtPropGetter;
    private readonly Action<TEntity, TTgtProp> _tgtPropSetter;
    private readonly List<Action<IFieldUpdater, IUpdateRecorder, TEntity>> _modifiers = [];

    private readonly ILogger? _logger;
    private readonly bool _isCleaner;
    private readonly bool _tgtPropIsNullable;

    public MultiFieldUpdater(
        Expression<Func<TEntity, int>> keyPropExpr,
        Expression<Func<TEntity, TSrcProp>> srcPropExpr,
        Expression<Func<TEntity, TTgtProp>> tgtPropExpr,
        IUpdateRecorder updateRecorder,
        bool isCleaner,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );

        var uniqueKeyPropInfo = keyPropExpr.GetPropertyInfo();
#pragma warning disable CS8605 // Unboxing a possibly null value.
        _keyGetter = x => (int) uniqueKeyPropInfo.GetValue( x );
#pragma warning restore CS8605 // Unboxing a possibly null value.

        var srcPropInfo = srcPropExpr.GetPropertyInfo();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        _srcPropGetter = x => (TSrcProp) srcPropInfo.GetValue( x );
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        var tgtPropInfo = tgtPropExpr.GetPropertyInfo();
        FieldName = tgtPropInfo.Name;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        _tgtPropGetter = x => (TTgtProp) tgtPropInfo.GetValue( x );
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
        _tgtPropSetter = ( x, y ) => tgtPropInfo.SetValue( x, y );

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

    public void AddModifier( Action<IFieldUpdater, IUpdateRecorder, TEntity> modifier ) => _modifiers.Add( modifier );

    public void ProcessEntityFields( TEntity entity )
    {
        foreach( var modifier in _modifiers )
        {
            modifier( this, UpdateRecorder, entity );
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

    bool IFieldUpdater.TryGetKeyValue( object entity, out int key )
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

    void IFieldUpdater.SetValue( object entity, object? newValue )
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

    void IFieldUpdater.ProcessEntityFields( object entity )
    {
        if( entity is not TEntity castEntity )
        {
            _logger?.UnexpectedType(typeof(TEntity), entity.GetType());
            return;
        }

        ProcessEntityFields( castEntity );
    }
}
