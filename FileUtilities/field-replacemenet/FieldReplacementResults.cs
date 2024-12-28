namespace J4JSoftware.FileUtilities;

public class FieldReplacementResults
{
    public int Key { get; set; }
    public Dictionary<string, object> Changes { get; } = [];
    public bool IsComplete { get; set; } = true;
}