using System.Dynamic;
using System.Linq.Expressions;
using System.Text.Json;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public abstract class EntityAdjuster<TEntity> : IEntityAdjuster<TEntity>
    where TEntity : class
{
    private readonly IUpdateRecorder? _updateRecorder;
    private readonly Dictionary<int, HashSet<string>> _propsChanged = [];
    private readonly Func<TEntity, int> _keyGetter;
    private readonly string _keyName;
    private readonly Dictionary<string, Func<TEntity, object?>> _getters = [];
    private readonly Dictionary<string, Action<TEntity, object?>> _setters = [];

    private Dictionary<int, TEntity>? _replEntities;
    private bool _replacementsDefined;

    protected EntityAdjuster(
        IUpdateRecorder? updateRecorder,
        Expression<Func<TEntity, int>> keyExpr,
        string nullText,
        ILoggerFactory? loggerFactory
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );

        _keyGetter = keyExpr.Compile();
        _keyName = keyExpr.GetPropertyInfo().Name;

        _updateRecorder = updateRecorder;
        NullText = nullText;
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected string NullText { get; }
    protected JsonSerializerOptions? JsonReplacementSerializerOptions { get; set; }

    public Type EntityType => typeof( TEntity );
    public bool IsValid { get; protected set; }

    public virtual bool Initialize( ImportContext context )
    {
        if( string.IsNullOrEmpty( context.ReplacementsPath ) )
            IsValid = true;
        else
        {
            _replacementsDefined = InitializeReplacements( context.ReplacementsPath );

            IsValid = _replacementsDefined;
        }

        return IsValid;
    }

    public HashSet<int> GetReplacementIds() => _replEntities?.Select( re => re.Key ).ToHashSet() ?? [];

    public virtual bool AdjustEntity( TEntity entity )
    {
        if( !CorrectProperties( entity ) )
            return false;

        return !_replacementsDefined || ApplyReplacements( entity );
    }

    protected virtual void AdjustField<TProp>(
        TEntity entity,
        Expression<Func<TEntity, TProp>> propExpr,
        List<IPropertyAdjuster<TProp>> adjusters
    )
    {
        var getter = propExpr.Compile();

        var propInfo = propExpr.GetPropertyInfo();

        var adjustments = 0;
        var initialValue = getter(entity);

        foreach (var adjuster in adjusters)
        {
            if (!adjuster.AdjustField(getter(entity), out var adjValue))
                continue;

            Setter(entity, adjValue);

            adjustments++;
        }

        if( adjustments > 0 )
            RecordSuccessfulAdjustment( _keyGetter( entity ),
                                        propInfo.Name,
                                        initialValue?.ToString(),
                                        getter( entity )?.ToString() );

        return;

        void Setter(TEntity c, TProp value) => propInfo.SetValue(c, value);
    }

    protected virtual bool CorrectProperties( TEntity entity ) => true;

    #region replacements 

    // will not be called unless filePath is defined
    private bool InitializeReplacements(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger?.FileNotFound(filePath);
            return false;
        }

        try
        {
            if (!LoadPropsChanged(filePath))
                return false;

            if (!TryLoadEntities(filePath))
                return false;

            var masterPropsChanged = new HashSet<string>();
            masterPropsChanged.UnionWith(_propsChanged.SelectMany(kvp => kvp.Value));

            // we never want to change the key field
            masterPropsChanged.Remove(_keyName);

            foreach (var propInfo in typeof(TEntity).GetProperties()
                                                    .Where(x => masterPropsChanged.Contains(x.Name)))
            {
                _getters.Add(propInfo.Name, (e) => propInfo.GetValue(e));
                _setters.Add(propInfo.Name, (e, value) => propInfo.SetValue(e, value));
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(
                $"Could not parse '{filePath}', message was '{ex.Message}'");

            return false;
        }

        return true;
    }

    private bool LoadPropsChanged(string filePath)
    {
        var expandoReader = new MultiRecordJsonFileReader<ExpandoObject>(LoggerFactory);
        expandoReader.LoadFile(filePath);

        var rawExpando = expandoReader.Contents!.ToList();

        foreach (dynamic expando in rawExpando)
        {
            var dict = (IDictionary<string, object?>)expando;

            if (!dict.ContainsKey(_keyName))
            {
                Logger?.ExpandoKeyFieldNotFound(EntityType, _keyName);
                return false;
            }

            var key = GetExpandoKey(expando);

            if (_propsChanged.ContainsKey(key))
                continue;

            // don't add the key field to the props changed set, becuase
            // we never want to change it
            _propsChanged.Add(key,
                               ((IDictionary<string, object?>)expando).Keys
                                                                         .Where(k => k != _keyName)
                                                                         .ToHashSet());
        }

        return true;
    }

    private bool TryLoadEntities(string filePath)
    {
        var reader = new MultiRecordJsonFileReader<TEntity>(LoggerFactory);

        if (JsonReplacementSerializerOptions != null)
            reader.SerializerOptions = JsonReplacementSerializerOptions;

        if (!reader.LoadFile(filePath))
            return false;

        _replEntities = reader.Contents!.ToDictionary(_keyGetter, x => x);

        return true;
    }

    protected abstract int GetExpandoKey(dynamic expando);

    protected bool ApplyReplacements( TEntity dbEntity )
    {
        var id = _keyGetter( dbEntity );

        if( !_replEntities!.TryGetValue( id, out var replEntity ) )
            return true;

        if( !_propsChanged.TryGetValue( id, out var propsChanged ) )
            return true;

        var allOkay = true;

        foreach( var propName in propsChanged )
        {
            if( !_getters.TryGetValue( propName, out var getter ) )
            {
                Logger?.UndefinedGetter( EntityType, propName );
                allOkay = false;

                continue;
            }

            var existingValue = getter( dbEntity );
            var replValue = getter( replEntity );

            if( ( existingValue == null && replValue == null ) || ( existingValue?.Equals( replValue ) ?? false ) )
                continue;

            if( !_setters.TryGetValue( propName, out var setter ) )
            {
                Logger?.UndefinedSetter( EntityType, propName );
                allOkay = false;

                continue;
            }

            RecordSuccessfulAdjustment( id, propName, existingValue?.ToString(), replValue?.ToString() );

            setter( dbEntity, replValue );
        }

        return allOkay;
    }

    #endregion

    public void RecordSuccessfulAdjustment(
        int key,
        string field,
        string? originalValue,
        string? adjValue,
        string? reason = null
    ) =>
        _updateRecorder?.FieldRevised( EntityType, key, field, originalValue, adjValue, reason );

    public virtual void SaveAdjustmentRecords()
    {
        _updateRecorder?.SaveChanges();
    }

    bool IEntityAdjuster.AdjustEntity( object entity )
    {
        if( entity is TEntity castEntity )
            return AdjustEntity( castEntity );

        Logger?.InvalidTypeAssignment( entity.GetType(), EntityType );

        return false;
    }
}
