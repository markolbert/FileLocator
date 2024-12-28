namespace J4JSoftware.FileUtilities;

internal record FieldReplacementInfo<TEntity>( 
    string Name, 
    Type PropertyType, 
    Func<TEntity, object?> GetValue, 
    Action<TEntity, object>? SetValue, 
    Type? ParserType )
    where TEntity: class;