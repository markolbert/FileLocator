namespace J4JSoftware.FileUtilities;

public class Stylable(
    ISheetCreator creator,
    StyleSetBase styleSet
) : IStylable
{
    public ISheetCreator Creator { get; } = creator;

    public StyleSetBase StyleSet { get; set; } = styleSet;
}
