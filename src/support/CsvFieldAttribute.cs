namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Property ) ]
public class CsvFieldAttribute(
    string csvHeader,
    Type? converterType = null
) : Attribute
{
    public string CsvHeader { get; } = csvHeader;
    public Type? ConverterType { get; } = converterType;
}
