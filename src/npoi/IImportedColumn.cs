using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface IImportedColumn
{
    int ColumnNumber { get; }
    string ColumnNameInSheet { get; }
    string PropertyName { get; }

    bool SetValue( ISheet sheet, object entity, ICell cell );
}
