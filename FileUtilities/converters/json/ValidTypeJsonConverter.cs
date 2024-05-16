using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class ValidTypeJsonConverter( List<Type> validTypes, ILoggerFactory? loggerFactory ) : JsonConverter<Type>
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<ValidTypeJsonConverter>();

    public override Type? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var typeText = reader.GetString();

        var retVal = validTypes.FirstOrDefault(et=>et.Name.Equals(typeText, StringComparison.OrdinalIgnoreCase));
        if( retVal != null )
            return retVal;

        _logger?.ConvertTextToType(typeText ?? string.Empty, typeof(Type));
        return null;
    }

    public override void Write( Utf8JsonWriter writer, Type value, JsonSerializerOptions options ) => writer.WriteStringValue( value.Name );
}
