using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace J4JSoftware.FileUtilities;

public class CsvNullableDoubleConverter : DefaultTypeConverter
{
    public override object? ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData )
    {
        if( !double.TryParse( text, out var retVal ) )
            return null;

        return retVal == 0.0 ? null : retVal;
    }

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData ) =>
        value == null ? string.Empty : value.ToString();
}
