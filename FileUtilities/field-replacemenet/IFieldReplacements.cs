using System.Collections.ObjectModel;

namespace J4JSoftware.FileUtilities;

public interface IFieldReplacements
{
    string KeyFieldName { get; }
    ReadOnlyDictionary<int, FieldReplacementResults> Collection { get; }
    bool AllComplete { get; }

    bool Load(string filePath);

    void ApplyTweaks( object entity );
}

public interface IFieldReplacements<in TEntity> : IFieldReplacements
    where TEntity : class
{
    void ApplyTweaks( TEntity entity );
}
