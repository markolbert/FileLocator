namespace J4JSoftware.FileUtilities;

public interface IStylable
{
    StyleSetBase StyleSet { get; set; }
    ISheetCreator Creator { get; }
}
