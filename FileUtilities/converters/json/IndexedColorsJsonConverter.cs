using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class IndexedColorsJsonConverter( ILoggerFactory? loggerFactory ) : JsonConverter<IndexedColors>
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<DbContextJsonConverter>();

    public override IndexedColors? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var colorText = reader.GetString()?.Trim();

        if( string.IsNullOrEmpty( colorText ) )
        {
            _logger?.UnknownColor( string.Empty, nameof( IndexedColors.Automatic ) );
            return IndexedColors.Automatic;
        }

        var colorProp = typeof(IndexedColors ).GetField( colorText );

        if( colorProp != null )
            return colorProp.GetValue( null ) as IndexedColors ?? IndexedColors.Automatic;

        _logger?.UnknownColor(colorText, nameof(IndexedColors.Automatic));

        return IndexedColors.Automatic;
    }

    public override void Write( Utf8JsonWriter writer, IndexedColors value, JsonSerializerOptions options ) =>
        writer.WriteStringValue( value.ToString() );
}