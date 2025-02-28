using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

/// <summary>
///     This is a test structured comment
/// </summary>
/// <param name="loggerFactory">This is a test structured comment param node</param>
public class CsvDateOnlyFormatter( ILoggerFactory? loggerFactory ) : DefaultTypeConverter
{
    // this is a plain old comment on a field
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<CsvDateOnlyFormatter>();

    // this is a plain old comment on a method
    public override object ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData )
    {
        // this is a plain old comment inside a method
        if( DateTime.TryParse( text, out var retVal ) )
            return retVal.Date;

        _logger?.ParseTextToType( text ?? string.Empty, typeof( DateTime ) );

        return DateTime.MinValue;
    }

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData )
    {
        if( value is DateTime dt )
            return dt.ToString( "yyyy-MM-dd" );

        _logger?.ConvertTextToType( value?.ToString() ?? string.Empty, typeof( DateTime ) );

        return null;
    }
}
