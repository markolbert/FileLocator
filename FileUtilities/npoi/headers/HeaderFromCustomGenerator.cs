namespace J4JSoftware.FileUtilities;

public abstract class HeaderFromCustomGenerator( ISheetCreator creator, FormatCodeStyleSet styleSet )
    : HeaderBase( creator, styleSet, HeaderSource.Custom );
