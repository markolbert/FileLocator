using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class DbContextJsonConverter( ILoggerFactory? loggerFactory, params Type[] dbContextTypes ) : JsonConverter<Type>
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<DbContextJsonConverter>();

    public override Type? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var typeText = reader.GetString();

        var retVal = dbContextTypes.FirstOrDefault(et => et.Name.Equals(typeText, StringComparison.OrdinalIgnoreCase));
        if (retVal != null)
            return retVal;

        _logger?.ConvertTextToType(typeText ?? string.Empty, typeof(Type));
        return null;
    }

    public override void Write( Utf8JsonWriter writer, Type value, JsonSerializerOptions options )
    {
        throw new NotImplementedException();
    }
}