using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface ISheetCreator
{
    ISheet? Sheet { get; }
    string SheetName { get; set; }

    IStyleSets StyleSets { get; }

    void Export( IWorkbook workbook, bool workbookIsNew );
}
