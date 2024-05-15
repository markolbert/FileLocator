namespace J4JSoftware.FileUtilities;

public class FileResolver( IDirectoryResolver dirResolver ) : IFileResolver
{
    public bool TryResolveFile(
        IFileContext source,
        out List<string> filePaths,
        bool searchSubDir = false,
        FileResolution resolution = FileResolution.SingleFile
    )
    {
        filePaths = [];

        if( Path.IsPathRooted( source.FilePath ) )
        {
            filePaths.Add(source.FilePath  );
            return true;
        }

        var fileName = Path.GetFileName(source.FilePath);
        var fileDir = Path.GetDirectoryName( source.FilePath ) ?? string.Empty;

        foreach( var dir in dirResolver.GetDirectories( source.Scope ) )
        {
            var curPath = Path.Combine( dir, fileDir );

            filePaths.AddRange( Directory.GetFiles( curPath,
                                                  fileName,
                                                  searchSubDir
                                                      ? SearchOption.AllDirectories
                                                      : SearchOption.TopDirectoryOnly ) );
        }

        return filePaths.Count switch
        {
            0 => false,
            1 => true,
            _ => resolution == FileResolution.MultipleFiles
        };
    }
}
