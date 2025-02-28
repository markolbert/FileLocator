using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.FileUtilities;

public class JsonNullDoubleConverter( string nullText ) : JsonConverter<double?>
{
    private readonly string _nullText = nullText.ToLower();

    public override double? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var text = reader.GetString()!.ToLower();

        return text.Equals( _nullText, StringComparison.OrdinalIgnoreCase )
            ? null
            : double.TryParse( text, out var temp )
                ? temp
                : null;
    }

    public override void Write( Utf8JsonWriter writer, double? value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value?.ToString() ?? nullText );
}
