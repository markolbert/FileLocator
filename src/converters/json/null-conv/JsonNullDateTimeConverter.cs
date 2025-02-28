using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.FileUtilities;

public class JsonNullDateTimeConverter( string nullText ) : JsonConverter<DateTime?>
{
    private readonly string _nullText = nullText.ToLower();

    public override DateTime? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var text = reader.GetString()!.ToLower();

        return text.Equals( _nullText, StringComparison.OrdinalIgnoreCase )
            ? null
            : DateTime.TryParse( text, out var temp )
                ? temp
                : null;
    }

    public override void Write( Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value?.ToString() ?? nullText );
}
