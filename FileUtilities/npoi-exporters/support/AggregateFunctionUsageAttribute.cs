namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Field ) ]
public class AggregateFunctionUsageAttribute( string functionText, string label, params Type[] applicableTypes )
    : Attribute
{
    public string FunctionText { get; } = functionText;
    public string Label { get; } = label;
    public Type[] ApplicableTypes { get; } = applicableTypes;

    public bool IsSupported( Type type ) => ApplicableTypes.Length == 0 || ApplicableTypes.Any( t => t == type );

    public bool IsSupported<T>( T? value ) => IsSupported( typeof( T ) );
}
