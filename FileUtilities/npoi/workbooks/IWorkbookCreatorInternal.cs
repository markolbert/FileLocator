namespace J4JSoftware.FileUtilities;

internal interface IWorkbookCreatorInternal
{
    string[]? SheetSequence { get; set; }
    void ChangeSheetName( string oldName, string newName );
}
