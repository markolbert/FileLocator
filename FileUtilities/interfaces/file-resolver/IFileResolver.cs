namespace J4JSoftware.FileUtilities;

public interface IFileResolver
{
    bool TryResolveFile(
        IFileContext source,
        out List<string> filePaths,
        bool searchSubDir = false,
        FileResolution resolution = FileResolution.SingleFile
    );
}