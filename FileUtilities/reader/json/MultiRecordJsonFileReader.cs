using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class MultiRecordJsonFileReader<TRecord>( ILoggerFactory? loggerFactory ) : IJsonFileParser<IEnumerable<TRecord>>
    where TRecord : class
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<MultiRecordJsonFileReader<TRecord>>();

    private FileStream? _fs;

    public JsonSerializerOptions SerializerOptions { get; set; } = new();

    public IEnumerable<TRecord>? Contents { get; private set; }

    public bool LoadFile( string path )
    {
        if (!File.Exists(path))
        {
            _logger?.FileNotFound(path);
            return false;
        }

        Contents = null;

        try
        {
            _fs = File.Open(path, FileMode.Open, FileAccess.Read);

            Contents = JsonSerializer.Deserialize<IEnumerable<TRecord>>( _fs, SerializerOptions )!;
            Dispose();

            return true;
        }
        catch (Exception ex)
        {
            _logger?.FileParsingError(path, ex.Message);

            Dispose();

            return false;
        }
    }

    public void Dispose()
    {
        _fs?.Dispose();
    }

    public IEnumerator<TRecord> GetEnumerator()
    {
        if( Contents == null )
            yield break;

        foreach( var record in Contents )
        {
            yield return record;
        }
    }

    object? IJsonFileParser.GetContents() => Contents;
}
