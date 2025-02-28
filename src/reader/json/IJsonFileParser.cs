using System.Text.Json;

namespace J4JSoftware.FileUtilities;

public interface IJsonFileParser
{
    JsonSerializerOptions SerializerOptions { get; }

    bool LoadFile( string path );
    object? GetContents();
}

public interface IJsonFileParser<out TResult> : IJsonFileParser
    where TResult : class
{
    TResult? Contents { get; }
}
