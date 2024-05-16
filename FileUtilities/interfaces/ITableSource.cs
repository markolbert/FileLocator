namespace J4JSoftware.FileUtilities;

public interface ITableSource : IFileContext
{
    Type EntityType { get; }
    string FileType { get; }

    string? TweakPath { get; set; }
}

public interface ICsvTableSource : ITableSource, ICsvContext
{
}

public interface IWorksheetTableSource : ITableSource, IWorkbookContext
{
}