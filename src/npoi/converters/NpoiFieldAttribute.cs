namespace J4JSoftware.FileUtilities;

[AttributeUsage(AttributeTargets.Property)]
public class NpoiFieldAttribute( string npoiFieldName, Type? converterType = null ) : Attribute
{
    public string NpoiFieldName { get; } = npoiFieldName;
    public Type? ConverterType { get; } = converterType;
}
