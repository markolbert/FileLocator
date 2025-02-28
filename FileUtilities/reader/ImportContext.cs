namespace J4JSoftware.FileUtilities;

public class ImportContext( params string[] fieldsToIgnore )
{
    public Stream? ImportStream { get; set; }
    //public string ImportPath { get; set; } = null!;
    public bool HasHeaders { get; set; }
    public string? ReplacementsPath { get; set; }
    public virtual string[] FieldsToIgnore { get; } = fieldsToIgnore;
}