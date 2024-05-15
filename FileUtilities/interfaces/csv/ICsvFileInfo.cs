namespace J4JSoftware.FileUtilities;

public interface ICsvFileInfo : IFileContext
{
    bool HasHeader { get; }
}
