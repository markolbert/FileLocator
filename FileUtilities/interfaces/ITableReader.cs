using System.Collections;

namespace J4JSoftware.FileUtilities;

public interface ITableReader : IDisposable, IEnumerable
{
    Type ImportedType { get; }

    bool SetSource( object source );
}
