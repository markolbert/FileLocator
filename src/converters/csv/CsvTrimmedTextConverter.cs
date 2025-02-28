using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace J4JSoftware.FileUtilities;

public class CsvTrimmedTextConverter : DefaultTypeConverter
{
    public override object ConvertFromString( string? text, IReaderRow row, MemberMapData memberMapData ) =>
        string.IsNullOrWhiteSpace( text ) ? string.Empty : text.Trim();

    public override string? ConvertToString( object? value, IWriterRow row, MemberMapData memberMapData ) =>
        value switch
        {
            null => string.Empty,
            string textValue => textValue.Trim(),
            _ => value.ToString()
        };
}
