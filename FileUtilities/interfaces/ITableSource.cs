namespace J4JSoftware.FileUtilities;

public interface ITableSource
{
    Type EntityType { get; set; }

    string FilePath { get; set; }
    string FileType { get; }
    string Scope { get; }

    string? TweakPath { get; set; }
}