using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class HeaderFromProperty(
    IExportableColumn column,
    StyleSetBase? styleSet = null,
    int numRows = 1,
    int numColumns = 1,
    string boundName = "Unknown"
)
    : HeaderBase( column.Creator, styleSet, HeaderSource.PropertyName )
{
    public override int NumRows { get; } = numRows <= 0 ? 1 : numRows;
    public override int NumColumns { get; } = numColumns <= 0 ? 1 : numColumns;

    public string PropertyName { get; } =
        string.IsNullOrEmpty( column.BoundProperty ) ? boundName : column.BoundProperty;

    public override void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn )
    {
        if( Creator.Sheet == null )
            return;

        var cell = Creator.Sheet.GetOrCreateCell( startingRow, startingColumn );

        cell.SetCellValue( PropertyName );
        cell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, StyleSet );
    }
}
