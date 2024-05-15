namespace J4JSoftware.FileUtilities;

public class Tweak
{
    public int Key { get; set; }
    public Dictionary<string, object> Changes { get; } = [];
    public bool IsComplete { get; set; } = true;
}