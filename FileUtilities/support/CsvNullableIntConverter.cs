using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace J4JSoftware.FileUtilities;

public class CsvNullableIntConverter : DefaultTypeConverter
{
    public override object? ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData )
    {
        if( int.TryParse( text, out var retVal ) )
            return retVal == 0 ? null : retVal;

        return null;
    }

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData ) =>
        value == null ? string.Empty : value.ToString();
}
