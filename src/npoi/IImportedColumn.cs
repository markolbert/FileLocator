using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface IImportedColumn
{
    int ColumnNumber { get; set; }
    string ColumnNameInSheet { get; }
    string PropertyName { get; }

    bool SetValue(ISheet sheet, object entity, ICell cell);
}

public interface IImportedColumn<in TEntity> : IImportedColumn
{
    bool SetValue(ISheet sheet, TEntity entity, ICell cell);
}
