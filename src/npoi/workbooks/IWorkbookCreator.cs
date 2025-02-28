using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public interface IWorkbookCreator
{
    ILoggerFactory? LoggerFactory { get; }

    ITableCreator<TEntity> AddTableSheet<TEntity>( IEnumerable<TEntity> entities )
        where TEntity : class;

    ISheetCreator AddSheet( ISheetCreator export );

    bool Export( string filePath, bool forceRecreation = true );
}
