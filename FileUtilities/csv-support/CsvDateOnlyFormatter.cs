using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class CsvDateOnlyFormatter( ILoggerFactory? loggerFactory ) : DefaultTypeConverter
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<CsvDateOnlyFormatter>();

    public override object ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData )
    {
        if( DateTime.TryParse( text, out var retVal ) )
            return retVal.Date;

        _logger?.ParseTextToType(text ?? string.Empty, typeof( DateTime ) );

        return DateTime.MinValue;
    }

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData )
    {
        if( value is DateTime dt )
            return dt.ToString( "yyyy-MM-dd" );

        _logger?.ConvertTextToType(value?.ToString() ?? string.Empty, typeof( DateTime ) );

        return null;
    }
}
