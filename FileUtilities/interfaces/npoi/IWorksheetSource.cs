namespace J4JSoftware.FileUtilities;

public interface IWorksheetSource : IFileContext
{
    string SheetName { get; }
}
