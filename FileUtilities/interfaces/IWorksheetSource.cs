namespace J4JSoftware.FileUtilities;

public interface IWorksheetSource : ITableSource
{
    string SheetName { get; }
}
