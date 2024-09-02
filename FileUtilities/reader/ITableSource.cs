namespace J4JSoftware.FileUtilities;

public interface ITableSource : IFileContext
{
    Type EntityType { get; }
    string FileType { get; }

    string? TweakPath { get; set; }
}
