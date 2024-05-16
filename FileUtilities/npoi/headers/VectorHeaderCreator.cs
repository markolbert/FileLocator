using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public abstract class VectorHeaderCreator( ITableCreator creator, StyleSetBase styleSet )
    : HeaderBase( creator, styleSet, HeaderSource.Vector )
{
    public override void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn )
    {
        if( Creator.Sheet == null )
            return;

        for( var colIdx = 0; colIdx < NumColumns; colIdx++ )
        {
            var cell = Creator.Sheet.GetOrCreateCell( startingRow, startingColumn + colIdx );

            CreateVectorHeader( workbook, cell, colIdx );
            cell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, StyleSet );
        }
    }

    protected abstract void CreateVectorHeader( IWorkbook workbook, ICell cell, int colNum );
}
