using System.Text.Json;

namespace J4JSoftware.FileUtilities;

public interface IJsonFileParser : IFileParser
{
    JsonSerializerOptions SerializerOptions { get; }
}

public interface IJsonFileParser<out TResult> : IFileParser<TResult>, IJsonFileParser
    where TResult : class
{
}
