namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class CsvExcludedAttribute( params string[] excludedProperties ) : Attribute
{
    public string[] ExcludedProperties { get; } = excludedProperties;
}
