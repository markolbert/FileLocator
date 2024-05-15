namespace J4JSoftware.FileUtilities;

public interface ITableImporter
{
    Type ImporterType { get; set; }
    string FileName { get; set; }
}