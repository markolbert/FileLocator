namespace J4JSoftware.FileUtilities;

public interface IConfiguration
{
    List<ImportDirectory> ImportDirectories { get; set; }
    StyleConfiguration Styles { get; set; }
}
