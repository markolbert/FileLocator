using System.Collections.ObjectModel;

namespace J4JSoftware.FileUtilities;

internal class Correctors<TEntity> : KeyedCollection<string, ICorrector<TEntity>>
{
    protected override string GetKeyForItem( ICorrector<TEntity> item ) => item.PropertyName;
}
