namespace J4JSoftware.FileUtilities;

public interface IFileParser
{
    bool LoadFile( string path );
    object? GetContents();
}

public interface IFileParser<out TResult> : IFileParser
    where TResult : class
{
    TResult? Contents { get; }
}