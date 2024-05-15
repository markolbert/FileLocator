namespace J4JSoftware.FileUtilities;

public class ImportDirectory
{
    private string _path = Environment.CurrentDirectory;

    public string Scope { get; set; } = null!;

    public string Path
    {
        get => _path;

        set
        {
            value = value.Trim();
            value = string.IsNullOrEmpty( value ) ? Environment.CurrentDirectory : value;

            _path = System.IO.Path.IsPathRooted( value )
                ? value
                : System.IO.Path.Combine( Environment.CurrentDirectory, value );
        }
    }
}
