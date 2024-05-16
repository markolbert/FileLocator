namespace J4JSoftware.FileUtilities;

[AttributeUsage(AttributeTargets.Property)]
public class TweakParserAttribute( Type parserType ) : Attribute
{
    public Type ParserType { get; } = parserType;
}