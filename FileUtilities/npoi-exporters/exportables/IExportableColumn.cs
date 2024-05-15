using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface IExportableColumn : IStylable
{
    Type EntityType { get; }
    Type PropertyType { get; }
    string BoundProperty { get; }
    int ColumnsNeeded { get; }
    List<IHeaderCreator> HeaderCreators { get; }
    
    bool PopulateSheet( IWorkbook workbook, int startingRow, int startingCol );

    ITableCreator GetTableCreator();
}

public interface IExportableColumn<TEntity, TProp> : IExportableColumn
    where TEntity : class
{
    ITableCreator<TEntity> TableCreator { get; }
    List<Aggregator<TEntity, TProp>> Aggregators { get; }
}
