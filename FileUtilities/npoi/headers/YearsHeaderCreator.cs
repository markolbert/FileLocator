using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class YearsHeaderCreator(
    int firstYear,
    int lastYear,
    ITableCreator creator,
    IntegerStyleSet? styleSet
)
    : VectorHeaderCreator( creator,
                           styleSet ?? creator.StyleSets.DefaultUngroupedIntegerHeader )
{
    public override int NumColumns => lastYear - firstYear + 1;
    public override int NumRows => 1;

    protected override void CreateVectorHeader( IWorkbook workbook, ICell cell, int colNum )
    {
        cell.SetCellValue( firstYear + colNum );
        cell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, StyleSet );
    }
}