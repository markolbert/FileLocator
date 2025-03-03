namespace J4JSoftware.FileUtilities;

public class WorksheetImportContext : ImportContext
{
    public string SheetName { get; set; } = null!;
    public WorksheetType WorksheetType { get; set; } = WorksheetType.Xlsx;
}
