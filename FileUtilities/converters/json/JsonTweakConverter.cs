using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class JsonTweakConverter<TEntity>( Tweaks<TEntity> tweaks, ILoggerFactory? loggerFactory ) : JsonConverter<Tweak>
    where TEntity : class
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<JsonTweakConverter<TEntity>>();
    private readonly Dictionary<Type, ITweakParser> _tweakParsers = [];

    public override Tweak Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        // defaults to IsComplete == true
        var retVal = new Tweak();
        var priorPropName = string.Empty;

        while( reader.TokenType != JsonTokenType.EndObject )
        {
            while( reader.TokenType != JsonTokenType.PropertyName && reader.TokenType != JsonTokenType.EndObject )
            {
                if( reader.Read() )
                    continue;

                _logger?.EndOfJson();

                break;
            }

            if( reader.TokenType == JsonTokenType.EndObject )
                break;

            var propName = reader.GetString();
            reader.Read();

            // if propName hasn't changed it means something is wrong in the code, and
            // we didn't move past the prior property entry
            if( propName != priorPropName )
                priorPropName = propName;
            else
                throw new FileUtilityException( typeof( JsonTweakConverter<TEntity> ),
                                                nameof( Read ),
                                                $"Repeating processing property '{propName}'; contact support" );

            if( !tweaks.TryGetTweakInfo(propName ?? string.Empty, out var tweakInfo ) )
            {
                _logger?.PropertyNotFound( typeof( TEntity ), propName ?? "<undefined>" );
                retVal.IsComplete = false;

                continue;
            }

            object? propValue;

            if( tweakInfo!.ParserType == null )
                propValue = GetPropertyValue( ref reader, tweakInfo );
            else
            {
                if( !_tweakParsers.TryGetValue( tweakInfo.PropertyType, out var parser ) )
                {
                    parser = (ITweakParser) Activator.CreateInstance( tweakInfo.ParserType )!;
                    _tweakParsers.Add( tweakInfo.PropertyType, parser );
                }

                var rawBytes = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
                var textValue = Encoding.UTF8.GetString( rawBytes );

                propValue = parser.GetParsedValue( textValue );
            }

            if( propValue == null )
            {
                var rawBytes = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
                var textValue = Encoding.UTF8.GetString( rawBytes );

                // skip past this property
                reader.Read();

                retVal.IsComplete = false;

                _logger?.JsonParsingFailed( typeof( TEntity ),
                                            retVal.Key.ToString(),
                                            propName ?? "<undefined>",
                                            textValue,
                                            tweakInfo.PropertyType.Name );

                continue;
            }

            // don't store the key as a change
            if( tweaks.KeyFieldName.Equals( tweakInfo.Name ) )
            {
                if( propValue is not int keyValue )
                {
                    _logger?.InvalidTweakKey( typeof( TEntity ), typeof( int ), propValue.GetType() );
                    return retVal;
                }

                retVal.Key = keyValue;
            }
            else
            {
                if( retVal.Changes.TryGetValue( tweakInfo.Name, out _ ) )
                {
                    _logger?.ReplacedDuplicate( "property value for tweak", tweakInfo.Name );
                    retVal.Changes[ tweakInfo.Name ] = propValue;
                }
                else retVal.Changes.Add( tweakInfo.Name, propValue );
            }
        }

        return retVal;
    }

    private object? GetPropertyValue( ref Utf8JsonReader reader, TweakProperty<TEntity> propInfo ) =>
        Type.GetTypeCode( propInfo.PropertyType ) switch
        {
            TypeCode.Boolean when reader.TokenType is JsonTokenType.True or JsonTokenType.False
                => ( reader.TokenType == JsonTokenType.True ),
            TypeCode.Double when reader.TokenType == JsonTokenType.Number => TryReadDouble( ref reader,
                out var temp )
                ? temp
                : null,
            TypeCode.Int32 when reader.TokenType is JsonTokenType.Number => TryReadInt32( ref reader, out var temp )
                ? temp
                : null,
            TypeCode.String when reader.TokenType is JsonTokenType.String => reader.GetString() ?? Globals.NullText,
            TypeCode.DateTime when reader.TokenType is JsonTokenType.String => TryReadDateTime( ref reader,
                out var temp )
                ? temp
                : null,
            _ => null
        };

    private bool TryReadInt32( ref Utf8JsonReader reader, out int value )
    {
        if( reader.TryGetInt32( out value ) )
            return true;

        _logger?.InvalidJsonValue( "an integer", "something else" );

        return false;
    }

    private bool TryReadDouble( ref Utf8JsonReader reader, out double value )
    {
        if( reader.TryGetDouble( out value ) )
            return true;

        _logger?.InvalidJsonValue( "a double", "something else" );

        return false;
    }

    private bool TryReadDateTime( ref Utf8JsonReader reader, out DateTime value )
    {
        if( reader.TryGetDateTime( out value ) )
            return true;

        _logger?.InvalidJsonValue( "a DateTime", "something else" );

        return false;
    }

    public override void Write( Utf8JsonWriter writer, Tweak tweak, JsonSerializerOptions options )
    {
        writer.WriteNumber( tweaks.KeyFieldName, tweak.Key );

        foreach( var kvp in tweak.Changes )
        {
            switch( kvp.Value )
            {
                case int intValue:
                    writer.WriteNumber( kvp.Key, intValue );
                    break;

                case double doubleValue:
                    writer.WriteNumber( kvp.Key, doubleValue );
                    break;

                case bool boolValue:
                    writer.WriteBoolean( kvp.Key, boolValue );
                    break;

                case DateTime dtValue:
                    writer.WriteString( kvp.Key, dtValue.ToShortDateString() );
                    break;

                case string stringValue:
                    writer.WriteString( kvp.Key, stringValue );
                    break;

                default:
                    _logger?.UnsupportedType( kvp.Value.GetType(), ", ignoring" );
                    break;
            }
        }
    }
}
