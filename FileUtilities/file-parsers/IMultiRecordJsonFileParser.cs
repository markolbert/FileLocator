using System.Text.Json;

namespace J4JSoftware.FileUtilities;

public interface IMultiRecordJsonFileParser<out TRecord> : IFileParser<IEnumerable<TRecord>>
    where TRecord : class
{
    JsonSerializerOptions SerializerOptions { get; }
}
