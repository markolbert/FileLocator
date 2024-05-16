using System.Collections;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class DefaultVectorHeaderCreator : VectorHeaderCreator
{
    public DefaultVectorHeaderCreator(
        ITableCreator creator,
        FormatCodeStyleSet styleSet
    )
        : base( creator, styleSet )
    {
        var rawData = creator.GetData();

        NumColumns = rawData.All( x => x is IList ) ? rawData.Cast<IList<object>>().Max( x => x.Count ) : 1;
    }

    public override int NumColumns { get; }
    public override int NumRows => 1;

    protected override void CreateVectorHeader( IWorkbook workbook, ICell cell, int colNum )
    {
        cell.SetCellValue( colNum + 1 );
        cell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, StyleSet );
    }
}
