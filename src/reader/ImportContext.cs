namespace J4JSoftware.FileUtilities;

public class ImportContext
{
    public Stream? ImportStream { get; set; }

    public bool HasHeaders { get; set; }
    public string? ReplacementsPath { get; set; }
    public virtual string[] PropertiesToIgnore { get; } = [];
}
