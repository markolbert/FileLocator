using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface IHeaderCreator : IStylable
{
    HeaderSource HeaderSource { get; }
    int NumRows { get; }
    int NumColumns { get; }

    void PopulateSheet( IWorkbook workbook, int startingRow, int startingColumn );
}
