using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.FileUtilities;

public class JsonNullTextConverter( string nullText ) : JsonConverter<string?>
{
    private readonly string _nullText = nullText.ToLower();

    public override string? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var text = reader.GetString()!.ToLower();

        return text.Equals( _nullText, StringComparison.OrdinalIgnoreCase ) ? null : reader.GetString();
    }

    public override void Write( Utf8JsonWriter writer, string? value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value ?? nullText );
}
