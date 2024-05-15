using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class JsonFileParser<TFile>( ILoggerFactory? loggerFactory ) : IJsonFileParser<TFile>
    where TFile : class
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<JsonFileParser<TFile>>();

    public JsonSerializerOptions SerializerOptions { get; } = new();

    public TFile? Contents { get; private set; }

    public bool LoadFile( string path )
    {
        if (!File.Exists(path))
        {
            _logger?.FileNotFound( path );
            return false;
        }

        Contents = null;

        try
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
            Contents = JsonSerializer.Deserialize<TFile>( fs, SerializerOptions )!;

            return true;
        }
        catch (Exception ex)
        {
            _logger?.FileParsingError( path, ex.Message );
            return false;
        }
    }

    object? IFileParser.GetContents() => Contents;
}
