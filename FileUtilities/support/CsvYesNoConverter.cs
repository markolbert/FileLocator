using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvYesNoConverter : DefaultTypeConverter
{
    private readonly ILogger? _logger;

    public override object ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData )
    {
        return text?.ToLower() switch
        {
            "yes" => true,
            _ => false
        };
    }

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData )
    {
        if( value is bool yesNo )
            return yesNo ? "Yes" : "No";

        _logger?.ConversionFailed(value?.ToString() ?? string.Empty, typeof(bool) );

        return null;
    }
}
