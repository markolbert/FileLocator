namespace J4JSoftware.FileUtilities;

public interface ICsvFileInfo : ITableSource
{
    bool HasHeader { get; }
}
