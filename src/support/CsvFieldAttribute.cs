namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Property ) ]
public class CsvFieldAttribute(
    string csvFieldName,
    Type? converterType = null
) : Attribute
{
    public string CsvFieldName { get; } = csvFieldName;
    public Type? ConverterType { get; } = converterType;
}
