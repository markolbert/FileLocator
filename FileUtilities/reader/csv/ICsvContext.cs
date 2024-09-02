namespace J4JSoftware.FileUtilities;

public interface ICsvContext : IFileContext
{
    bool HasHeader { get; }
}

public interface ICsvTableSource : ITableSource, ICsvContext
{
}
