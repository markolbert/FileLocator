using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class TypeCodeJsonConverter( ILoggerFactory? loggerFactory ) : JsonConverter<TypeCode>
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<DbContextJsonConverter>();

    public override TypeCode Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var typeText = reader.GetString();

        if( Enum.TryParse<TypeCode>( typeText, out var typeCode ) )
            return typeCode;

        _logger?.ConvertTextToType( typeText ?? string.Empty, typeof( TypeCode ) );

        return TypeCode.Empty;
    }

    public override void Write( Utf8JsonWriter writer, TypeCode value, JsonSerializerOptions options )
    {
        throw new NotImplementedException();
    }
}
