namespace J4JSoftware.FileUtilities;

internal interface IWorkbookCreatorInternal
{
    SheetCollection SheetCreators { get; }
    string[]? SheetSequence { get; set; }
}
