namespace J4JSoftware.FileUtilities;

public interface IFileContext
{
    string FilePath { get; set; }
    string Scope { get; }
}