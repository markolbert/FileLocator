using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class MonthDayTimeFormatJsonConverter( ILoggerFactory? loggerFactory ) : JsonConverter<MonthDayTimeFormat>
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<DbContextJsonConverter>();

    public override MonthDayTimeFormat Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var text = reader.GetString();

        if( Enum.TryParse<MonthDayTimeFormat>( text, true, out var retVal ) )
            return retVal;

        _logger?.UnsupportedEnumValue( typeof( MonthDayTimeFormat ),
                                       text ?? string.Empty,
                                       MonthDayTimeFormat.Numbers.ToString() );

        return MonthDayTimeFormat.Numbers;
    }

    public override void Write( Utf8JsonWriter writer, MonthDayTimeFormat value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value.ToString() );
}
