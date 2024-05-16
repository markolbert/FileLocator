using System.Collections.ObjectModel;
using System.Reflection;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class Tweaks<TEntity> : ITweaks<TEntity>
    where TEntity : class
{
    private readonly Func<TEntity, int> _keyGetter = null!;
    private readonly IUpdateRecorder? _updateRecorder;
    private readonly Dictionary<int, Tweak> _tweaks = [];
    private readonly Dictionary<string, TweakProperty<TEntity>> _tweakableProps = [];
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;

    public Tweaks(
        DbContext dbContext,
        IUpdateRecorder? updateRecorder,
        ILoggerFactory? loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory?.CreateLogger<Tweaks<TEntity>>();

        _updateRecorder = updateRecorder;

        if( !dbContext.TryGetEntitySingleFieldPrimaryKey( typeof( TEntity ), out var keyField ) )
            throw new FileUtilityException( typeof( Tweaks<TEntity> ),
                                            "ctor",
                                            $"{typeof( TEntity )} is not part of {dbContext.GetType()} with a single field primary key" );

        KeyFieldName = keyField!;

        foreach( var propInfo in typeof( TEntity ).GetProperties()
                                                  .Where(pi=>pi.CanWrite  ) )
        {
            var parserType = propInfo.GetCustomAttribute<TweakParserAttribute>()?.ParserType;

            void Setter( TEntity entity, object value ) => propInfo.SetValue( entity, ConvertTextNull( value ) );
            object? Getter(TEntity entity) => propInfo.GetValue(entity);

            if( propInfo.Name.Equals( keyField ) )
            {
                if( propInfo.PropertyType == typeof( int ) )
                    _keyGetter = x => (int) Getter( x )!;
                else
                    throw new FileUtilityException(typeof(Tweaks<TEntity>),
                                                   "ctor",
                                                   $"{typeof(TEntity).Name}::{propInfo.Name} is not an integer");

                _tweakableProps.Add(propInfo.Name,
                                    new TweakProperty<TEntity>(propInfo.Name,
                                                               propInfo.PropertyType,
                                                               Getter,
                                                               null,
                                                               parserType));
            }
            else
                _tweakableProps.Add( propInfo.Name,
                                     new TweakProperty<TEntity>( propInfo.Name,
                                                                 propInfo.PropertyType,
                                                                 Getter,
                                                                 Setter,
                                                                 parserType ) );
        }
    }

    private static object? ConvertTextNull( object value )
    {
        if( value is string textValue && textValue.Equals( Globals.NullText, StringComparison.OrdinalIgnoreCase ) )
            return null;
        else return value;
    }

    public IFileContext? Source { get; set; }

    public bool Load()
    {
        // we're interested in the tweaks file associated with Source, not the source file itself
        // check to ensure Source is actually ITableSource
        if( Source is not ITableSource tableSource )
        {
            _logger?.TweaksPathMissing( Source?.GetType() ?? typeof( object ) );
            return false;
        }

        if( tableSource.TweakPath == null )
        {
            _logger?.UndefinedImportSource();
            return false;
        }

        var parser = new MultiRecordJsonFileParser<Tweak>( _loggerFactory );
        parser.SerializerOptions.Converters.Add( new JsonTweakConverter<TEntity>( this, _loggerFactory ) );

        if( !parser.LoadFile( tableSource.TweakPath ) )
        {
            _logger?.Error( $"Parsing of {typeof( Tweaks<TEntity> )} failed" );
            return false;
        }

        var incompleteIds = parser.Contents!.Where( x => !x.IsComplete )
                               .Select( x => x.Key )
                               .ToList();

        if( incompleteIds.Count != 0 )
            _logger?.JsonIncompleteTweaks( incompleteIds.Count, string.Join( ", ", incompleteIds ) );

        foreach( var tweak in parser.Contents! )
        {
            if( _tweaks.TryGetValue( tweak.Key, out _ ) )
            {
                _logger?.ReplacedDuplicate( $"{nameof( TEntity )} tweak for id", tweak.Key.ToString() );
                _tweaks[ tweak.Key ] = tweak;
            }
            else _tweaks.Add( tweak.Key, tweak );
        }

        return true;
    }

    public string KeyFieldName { get; }
    public ReadOnlyDictionary<int, Tweak> Collection => new ( _tweaks );
    public bool AllComplete => _tweaks.All( kvp => kvp.Value.IsComplete );

    public void ApplyTweaks( TEntity entity )
    {
        var entityKey = _keyGetter( entity );

        if( !_tweaks.TryGetValue( entityKey, out var tweak ) )
            return;

        foreach( var kvp in tweak.Changes )
        {
            // should never happen, but we don't want to try and update the key field!
            if( kvp.Key.Equals( KeyFieldName ) )
                continue;

            if( !_tweakableProps.TryGetValue( kvp.Key, out var tweakInfo ) )
            {
                _logger?.PropertyNotTweakable( typeof( TEntity ).Name, kvp.Key );
                continue;
            }

            try
            {
                var priorValue = tweakInfo.GetValue( entity );

                tweakInfo.SetValue?.Invoke( entity, kvp.Value );

                _updateRecorder?.RecordChange( typeof( TEntity ),
                                               entityKey,
                                               kvp.Key,
                                               priorValue?.ToString() ?? Globals.NullText,
                                               kvp.Value.ToString() );
            }
            catch( Exception ex )
            {
                _logger?.Error( $"Could not tweak {typeof( TEntity ).Name}::{kvp.Key}, message was '{ex.Message}'" );
            }
        }
    }

    internal bool TryGetTweakInfo(string propName, out TweakProperty<TEntity>? tweakInfo) => _tweakableProps.TryGetValue(propName, out tweakInfo);

    void ITweaks.ApplyTweaks( object entity )
    {
        if( entity is TEntity castEntity )
            ApplyTweaks( castEntity );
        else _logger?.UnexpectedType( typeof( TEntity ), entity.GetType() );
    }
}
