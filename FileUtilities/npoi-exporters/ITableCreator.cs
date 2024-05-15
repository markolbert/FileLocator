using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public interface ITableCreator : ISheetCreator
{
    Type EntityType { get; }
    ILoggerFactory? LoggerFactory { get; }

    IWorkbookCreator WorkbookCreator { get; }
    
    int NumColumnsDefined { get; }
    int FreezeColumn { get; }
    
    ReadOnlyCollection<IExportableColumn> Columns { get; }

    List<object> GetData();
}

public interface ITableCreator<TEntity> : ITableCreator
{
    List<TEntity> Data { get; }
}
