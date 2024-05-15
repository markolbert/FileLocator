using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace J4JSoftware.FileUtilities;

public class SpanningHeader( string text, int numColumns, ISheetCreator creator, StyleSetBase? styleSet )
    : HeaderBase( creator, styleSet, HeaderSource.Spanning )
{
    public override int NumRows => 1;
    public override int NumColumns { get; } = numColumns <= 1 ? 1 : numColumns;

    public override void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn )
    {
        if( Creator.Sheet == null )
            return;

        var cell = Creator.Sheet.GetOrCreateCell( startingRow, startingColumn );

        cell.SetCellValue( text );

        var spanStyle = StyleSet with
        {
            BorderInfo = new BorderInfo( BorderStyle.None, BorderStyle.None, BorderStyle.Thin, BorderStyle.None )
        };

        cell.CellStyle = Creator.StyleSets.ResolveCellStyle(workbook, spanStyle);

        var mergeRegion = new CellRangeAddress( startingRow, startingRow, startingColumn, startingColumn + NumColumns - 1 );
        Creator.Sheet.AddMergedRegion( mergeRegion );

        // update styles in other cells in merged region
        for( var colIdx = 1; colIdx < NumColumns; colIdx++ )
        {
            var mergeCell = Creator.Sheet.GetOrCreateCell( startingRow, startingColumn + colIdx );
            mergeCell.CellStyle = cell.CellStyle;
        }
    }
}
