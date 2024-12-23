using System.Collections.ObjectModel;

namespace J4JSoftware.FileUtilities;

public interface ITweaks
{
    string KeyFieldName { get; }
    ReadOnlyDictionary<int, Tweak> Collection { get; }
    bool AllComplete { get; }

    bool Load(string filePath);

    void ApplyTweaks( object entity );
}

public interface ITweaks<in TEntity> : ITweaks
    where TEntity : class
{
    void ApplyTweaks( TEntity entity );
}
