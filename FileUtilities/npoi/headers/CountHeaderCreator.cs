using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class CountHeaderCreator(
    int firstCol,
    int lastCol,
    ITableCreator creator,
    IntegerStyleSet? styleSet
)
    : VectorHeaderCreator(creator,
                          styleSet ?? creator.StyleSets.DefaultUngroupedIntegerHeader)
{
    public override int NumColumns => lastCol - firstCol + 1;
    public override int NumRows => 1;

    protected override void CreateVectorHeader(IWorkbook workbook, ICell cell, int colNum)
    {
        cell.SetCellValue(firstCol + colNum);
    }
}
