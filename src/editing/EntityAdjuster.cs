using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public abstract class EntityAdjuster<TEntity> : IEntityAdjuster<TEntity>
    where TEntity : class
{
    private readonly Dictionary<int, HashSet<string>> _propsChanged = [];
    private readonly Func<TEntity, int> _keyGetter;
    private readonly Correctors<TEntity> _correctors = [];
    private readonly string _keyName;
    private readonly Dictionary<string, Func<TEntity, object?>> _getters = [];
    private readonly Dictionary<string, Action<TEntity, object?>> _setters = [];
    private readonly List<PropertyInfo> _entityProps;
    private readonly HashSet<JsonConverter> _jsonConverters = [];

    private Dictionary<int, TEntity>? _replEntities;
    private bool _replacementsDefined;

    protected EntityAdjuster(
        Expression<Func<TEntity, int>> keyExpr,
        string nullText,
        ILoggerFactory? loggerFactory
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );

        _keyGetter = keyExpr.Compile();
        _keyName = keyExpr.GetPropertyInfo().Name;
        _entityProps = EntityType.GetProperties().ToList();

        NullText = nullText;
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    protected string NullText { get; }

    public Type EntityType => typeof( TEntity );
    public bool IsValid { get; protected set; }
    public IUpdateRecorder? UpdateRecorder { get; set; }

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
        // do replacements first so we can skip correcting any
        // replaced properties
        HashSet<string> propsReplaced;

        if( _replacementsDefined )
        {
            if( !ApplyReplacements( entity, out propsReplaced ) )
                return false;
        }
        else propsReplaced = [];

        CorrectProperties( entity, propsReplaced );

        return true;
    }

    protected void AddJsonConverter( params JsonConverter[] converters )
    {
        foreach( var converter in converters )
        {
            _jsonConverters.Add( converter );
        }
    }

    protected void AddSinglePropertyCorrector<TProp>(
        Expression<Func<TEntity, TProp?>> propExpr,
        params IPropertyAdjuster<TProp?>[] adjusters
    )
    {
        if( adjusters.Length == 0 )
            return;

        var propInfo = propExpr.GetPropertyInfo();

        if( !_correctors.TryGetValue( propInfo.Name, out var corrector ) )
        {
            corrector = new Corrector<TEntity, TProp>( propExpr, UpdateRecorder );
            _correctors.Add( corrector );
        }

        ( (Corrector<TEntity, TProp>) corrector ).Adjusters.AddRange( adjusters );
    }

    protected virtual void CorrectProperties( TEntity entity, HashSet<string> propsReplaced )
    {
        foreach( var corrector in _correctors.Where( c => !propsReplaced.Contains( c.PropertyName ) ) )
        {
            corrector.CorrectEntity( entity );
        }
    }

    #region replacements

    // will not be called unless filePath is defined
    private bool InitializeReplacements( string filePath )
    {
        if( !File.Exists( filePath ) )
        {
            Logger?.FileNotFound( filePath );
            return false;
        }

        try
        {
            if( !LoadPropsChanged( filePath ) )
                return false;

            if( !TryLoadEntities( filePath ) )
                return false;

            var masterPropsChanged = new HashSet<string>();
            masterPropsChanged.UnionWith( _propsChanged.SelectMany( kvp => kvp.Value ) );

            // we never want to change the key field
            masterPropsChanged.Remove( _keyName );

            foreach( var propInfo in _entityProps.Where( x => masterPropsChanged.Contains( x.Name ) ) )
            {
                _getters.Add( propInfo.Name, e => propInfo.GetValue( e ) );
                _setters.Add( propInfo.Name, ( e, value ) => propInfo.SetValue( e, value ) );
            }
        }
        catch( Exception ex )
        {
            Logger?.Error( $"Could not parse '{filePath}', message was '{ex.Message}'" );

            return false;
        }

        return true;
    }

    private bool LoadPropsChanged( string filePath )
    {
        var expandoReader = new MultiRecordJsonFileReader<ExpandoObject>( LoggerFactory );
        expandoReader.LoadFile( filePath );

        var rawExpando = expandoReader.Contents!.ToList();

        foreach( dynamic expando in rawExpando )
        {
            var dict = (IDictionary<string, object?>) expando;

            if( !dict.ContainsKey( _keyName ) )
            {
                Logger?.ExpandoKeyFieldNotFound( EntityType, _keyName );
                return false;
            }

            var key = GetExpandoKey( expando );

            if( _propsChanged.ContainsKey( key ) )
                continue;

            // don't add the key field to the props changed set, because
            // we never want to change it
            _propsChanged.Add( key,
                               ( (IDictionary<string, object?>) expando ).Keys
                                                                         .Where( k => k != _keyName )
                                                                         .ToHashSet() );
        }

        return true;
    }

    private bool TryLoadEntities( string filePath )
    {
        var reader = new MultiRecordJsonFileReader<TEntity>( LoggerFactory );

        foreach( var converter in _jsonConverters )
        {
            reader.SerializerOptions.Converters.Add( converter );
        }

        if( !reader.LoadFile( filePath ) )
            return false;

        _replEntities = reader.Contents!.ToDictionary( _keyGetter, x => x );

        return true;
    }

    protected abstract int GetExpandoKey( dynamic expando );

    protected bool ApplyReplacements( TEntity dbEntity, out HashSet<string> propsReplaced )
    {
        propsReplaced = [];

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

            RecordAdjustment( dbEntity,
                              propName,
                              ChangeSource.Replacement,
                              existingValue?.ToString(),
                              replValue?.ToString() );

            setter( dbEntity, replValue );

            propsReplaced.Add( propName );
        }

        return allOkay;
    }

    #endregion

    public void RecordAdjustment(
        TEntity entity,
        string field,
        ChangeSource source,
        string? originalValue,
        string? adjValue,
        string? reason = null
    )
    {
        UpdateRecorder?.PropertyValueChanged( entity, field, source, originalValue, adjValue, reason );
    }

    public virtual void SaveAdjustmentRecords()
    {
        UpdateRecorder?.SaveChanges();
    }

    bool IEntityAdjuster.AdjustEntity( object entity )
    {
        if( entity is TEntity castEntity )
            return AdjustEntity( castEntity );

        Logger?.InvalidTypeAssignment( entity.GetType(), EntityType );

        return false;
    }
}
