namespace J4JSoftware.FileUtilities;

public interface IFieldCleaner
{
    Type EntityType { get; }
    string FieldName { get; }

    bool TryGetKeyValue( object entity, out int key );
    object? GetSourceValue( object entity );
    object? GetTargetValue( object entity );
    void SetValue( object entity, object? value );

    void ProcessEntityFields( object entity );
}
