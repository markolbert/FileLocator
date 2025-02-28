using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.FileUtilities;

public class JsonNullFloatConverter( string nullText ) : JsonConverter<float?>
{
    private readonly string _nullText = nullText.ToLower();

    public override float? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var text = reader.GetString()!.ToLower();

        return text.Equals( _nullText, StringComparison.OrdinalIgnoreCase )
            ? null
            : float.TryParse( text, out var temp )
                ? temp
                : null;
    }

    public override void Write( Utf8JsonWriter writer, float? value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value?.ToString() ?? nullText );
}
