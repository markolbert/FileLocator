using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace J4JSoftware.FileUtilities;

public class CsvYesNoConverter : DefaultTypeConverter
{
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

        return null;
    }
}
