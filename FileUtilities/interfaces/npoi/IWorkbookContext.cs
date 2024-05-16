namespace J4JSoftware.FileUtilities;

public interface IWorkbookContext : IFileContext
{
    string SheetName { get; }
}
