using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public class PathResolver : IPathResolver
{
    private readonly List<ImportDirectory> _importDirs;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, Func<string, string>> _resolvers = new(StringComparer.OrdinalIgnoreCase);

    public PathResolver(
        Configuration config,
        ILoggerFactory? loggerFactory
    )
    {
        _importDirs = config.ImportDirectories;
        _logger = loggerFactory?.CreateLogger(GetType());
    }

    public void ResolveFiles( ITableSource source )
    {
        var tweakSource = source.TweakPath?.Trim();

        if( !TryResolveSourceFilePath( source ) || string.IsNullOrEmpty( tweakSource ) )
            return;

        ResolveTweakFilePath( source );
    }

    private bool TryResolveSourceFilePath(ITableSource source)
    {
        var rawScope = source.Scope.Trim().ToLower();

        string scope;

        switch (rawScope)
        {
            case "auctria":
                if (source is not IAuctriaFileInfo auctriaInfo)
                {
                    _logger?.UnexpectedType(typeof(IAuctriaFileInfo), source.GetType());
                    return false;
                }

                scope = $"Auctria{auctriaInfo.Year}";

                break;

            case "lgl":
                scope = rawScope;
                break;

            default:
                _logger?.UnsupportedFileScope(rawScope);
                return false;
        }

        if( !TryGetResolver( scope, out var resolver ) )
            return false;

        source.FilePath = resolver!.Invoke(source.FilePath);

        return true;
    }

    private bool TryGetResolver( string scope, out Func<string, string>? resolver )
    {
        resolver = null;

        if (!_resolvers.TryGetValue(scope, out resolver))
        {
            var dirInfo =
                _importDirs.FirstOrDefault(id => id.Scope.Trim()
                                                   .Equals(scope.Trim(), StringComparison.OrdinalIgnoreCase));
            if (dirInfo == null)
            {
                _logger?.UnsupportedFileScope(scope);
                return false;
            }

            resolver = x => Path.Combine(dirInfo.Path, x);
            _resolvers.Add(scope, resolver);
        }

        return true;
    }

    private void ResolveTweakFilePath( ITableSource source )
    {
        // shouldn't happen, but...
        if( string.IsNullOrEmpty( source.TweakPath ) )
            return;

        if( !TryGetResolver( "tweaks", out var resolver ) )
            return;

        source.TweakPath = resolver!.Invoke( source.TweakPath );
    }
}
