using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public abstract class HeaderBase(
    ISheetCreator creator,
    StyleSetBase? styleSet,
    HeaderSource source
) : Stylable( creator, styleSet ?? creator.StyleSets.DefaultHeader ), IHeaderCreator
{
    public abstract int NumRows { get; }
    public abstract int NumColumns { get; }

    public HeaderSource HeaderSource { get; } = source;

    public abstract void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn );
}
