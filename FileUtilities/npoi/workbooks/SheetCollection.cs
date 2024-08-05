using System.Collections.ObjectModel;

namespace J4JSoftware.FileUtilities;

internal class SheetCollection() : KeyedCollection<string, ISheetCreator>( StringComparer.OrdinalIgnoreCase )
{
    protected override string GetKeyForItem( ISheetCreator item ) => item.SheetName;
}
