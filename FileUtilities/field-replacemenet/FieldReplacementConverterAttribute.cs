namespace J4JSoftware.FileUtilities;

[AttributeUsage(AttributeTargets.Property)]
public class FieldReplacementConverterAttribute( Type parserType ) : Attribute
{
    public Type ParserType { get; } = parserType;
}