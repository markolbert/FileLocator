using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public abstract class ExportableColumnBase : Stylable, IExportableColumn
{
    protected ExportableColumnBase(
        Type entityType,
        Type propType,
        ITableCreator creator,
        StyleSetBase styleSet,
        ILoggerFactory? loggerFactory
    )
        : base( creator, styleSet )
    {
        EntityType = entityType;
        PropertyType = propType;

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type EntityType { get; }
    public Type PropertyType { get; }

    public string BoundProperty { get; init; } = null!;
    public abstract int ColumnsNeeded { get; }
    public List<IHeaderCreator> HeaderCreators { get; } = [];

    public abstract bool PopulateSheet( IWorkbook workbook, int startingRow, int startingCol );

    // rowNum and colNum are absolute references in ISheet, and so need
    // to be adjusted for, e.g., the existence of headers, before calling
    // the method
    protected virtual void CreateCell( IWorkbook workbook, int rowNum, int colNum, object? value )
    {
        if( value == null || Creator.Sheet == null )
            return;

        var cell = Creator.Sheet.GetOrCreateCell( rowNum, colNum );

        switch( value )
        {
            case bool boolValue:
                cell.SetCellValue( boolValue );
                break;

            case int intValue:
                cell.SetCellValue( intValue );
                break;

            case double doubleValue:
                cell.SetCellValue( doubleValue );
                break;

            case DateTime dtValue:
                cell.SetCellValue( dtValue );
                break;

            case string textValue:
                cell.SetCellValue( textValue );
                break;

            default:
                Logger?.UnsupportedCellType( value.GetType(), Creator.SheetName );
                break;
        }

        var tweakedStyleSet =
            !StyleSet.WrapText ? StyleSet : StyleSet with { VerticalAlignment = VerticalAlignment.Top };
        cell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, tweakedStyleSet );
    }

    ITableCreator IExportableColumn.GetTableCreator() => (ITableCreator) Creator;
}
