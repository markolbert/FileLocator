using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class HeaderFromText( string text, ISheetCreator creator, StyleSetBase? styleSet )
    : HeaderBase( creator, styleSet, HeaderSource.Text )
{
    public override int NumRows => 1;
    public override int NumColumns => 1;

    public override void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn )
    {
        if( Creator.Sheet == null )
            return;

        var cell = Creator.Sheet.GetOrCreateCell( startingRow, startingColumn );

        cell.SetCellValue( text );
        cell.CellStyle = Creator.StyleSets.ResolveCellStyle(workbook, StyleSet);
    }
}