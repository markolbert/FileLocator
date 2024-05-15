namespace J4JSoftware.FileUtilities;

public class TitleRow(
    string text,
    ISheetCreator creator,
    StyleSetBase styleSet
) : Stylable( creator, styleSet )
{
    public string Text { get; } = text;
}

